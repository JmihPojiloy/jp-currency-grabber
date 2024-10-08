using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Data;
using System.Data.Odbc;
using SYS.CRU.CURRENCY.GRABBER.Dto;
using SYS.CRU.CURRENCY.GRABBER.Loggers;

namespace SYS.CRU.CURRENCY.GRABBER.Data;
/// <summary>
/// Подключение к базе данных
/// </summary>
public class CurrencyRepository
{
    private readonly string _connectionString;

    public CurrencyRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    /// <summary>
    /// Метод для получения коллекции обновляемых валют, через серверную функцию.
    /// </summary>
    /// <param name="currency">Идентификатор валют.</param>
    /// <returns>Коллекцию валютных пар.</returns>
    public async Task<Currency[]> GetAllCurrenciesByIdAsync(string currency, CancellationToken token)
    {
        // вызывает хранимую на сервере функцию, которая получает коллекцию валют по id
        var command = @"SELECT * FROM GetAllCurrenciesById(?)";

        var currencyDtos = new ConcurrentBag<CurrencyDto>();

        Currency[] result = null!; 

        await using OdbcConnection db = new(_connectionString);
        try
        {
            await db.OpenAsync(token);

            await using (OdbcCommand cmd = new(command, db))
            {
                cmd.Parameters.AddWithValue("?", currency.Trim());

                await using (OdbcDataReader reader = (OdbcDataReader)await cmd.ExecuteReaderAsync(token))
                {
                    while (await reader.ReadAsync(token))
                    {
                        var currencyDto = new CurrencyDtoBuilder()
                            .WithBCurrencyID(reader.GetString(0))
                            .WithTCurrencyID(reader.GetString(1))
                            .WithIsVisible(reader.GetBoolean(2))
                            .WithIsManual(reader.GetBoolean(3))
                            .WithRate(reader.GetDecimal(4))
                            .WithFee(reader.GetDecimal(5))
                            .WithUpdated(reader.GetDateTime(6))
                            .Build();

                        currencyDtos.Add(currencyDto);
                    }
                }
            }

            if (!currencyDtos.Any())
            {
                throw new NullReferenceException("No currencies found.");
            }
            
            var tCurrenciesId = string.Join(", ", currencyDtos.Select(x => x.TCurrencyID));
            var log = new Log($"Get {currency} from DB => [{tCurrenciesId}]", "OK");
            await Logger.AddLogAsync(log);
            
            result = currencyDtos.Select(Currency.Mapper.Map).ToArray()!;
        }
        catch (TaskCanceledException)
        {
            throw new OperationCanceledException();
        }
        catch (OperationCanceledException)
        {
            var log = new Log($"Get {currency} from DB => [time is up]", "ERROR");
            await Logger.AddLogAsync(log).ConfigureAwait(false);
        }
        catch (NullReferenceException ex)
        {
            var log = new Log($"Get {currency} from DB => [{ex.Message}]", "NEXT TRY");
            await Logger.AddLogAsync(log).ConfigureAwait(false);
            await Task.Delay(10000, token).ConfigureAwait(false);
            await GetAllCurrenciesByIdAsync(currency, token).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            var log = new Log($"Get {currency} from DB => [{ex.Message}]", "ERROR");
            await Logger.AddLogAsync(log).ConfigureAwait(false);
            return null!;
        }

        return result;
    }

    /// <summary>
    /// Метод для обновления валют в БД, работает через серверную процедуру.
    /// Время обновляется через триггер на сервере.
    /// </summary>
    /// <param name="currencies">Коллекция обновленных валют.</param>
    public async Task UpdateCurrenciesAsync(Currency[] currencies, CancellationToken token)
    {
        var currency = currencies[0].BCurrencyID;
        var currenciesDtos = currencies.Select(Currency.Mapper.Map).ToImmutableArray();

        await using OdbcConnection db = new(_connectionString);

        try
        {
            await db.OpenAsync(token);

            // Создание временной таблицы
            string createTempTableQuery = @"
            CREATE TABLE #TempCurrencyUpdates (
                BCurrencyID CHAR(3),
                TCurrencyID CHAR(3),
                Rate MONEY,
                Updated DATETIME
            )";

            await using var createTableCmd = new OdbcCommand(createTempTableQuery, db);
            await createTableCmd.ExecuteNonQueryAsync(token);

            // Формирование SQL команд для вставки данных
            foreach (var currencyDto in currenciesDtos)
            {
                string insertQuery = @"
                INSERT INTO #TempCurrencyUpdates (BCurrencyID, TCurrencyID, Rate, Updated) 
                VALUES (?, ?, ?, GETDATE())";
            
                await using var insertCmd = new OdbcCommand(insertQuery, db);
                insertCmd.Parameters.AddWithValue("@BCurrencyID", currencyDto?.BCurrencyID);
                insertCmd.Parameters.AddWithValue("@TCurrencyID", currencyDto?.TCurrencyID);
                insertCmd.Parameters.AddWithValue("@Rate", currencyDto?.Rate);

                await insertCmd.ExecuteNonQueryAsync(token);
            }

            // Вызов хранимой процедуры, которая использует временную таблицу
            await using var cmd = new OdbcCommand("UpdatingCurrencyRates", db);
            cmd.CommandType = CommandType.StoredProcedure;

            await cmd.ExecuteNonQueryAsync(token);
        }
        catch (TaskCanceledException)
        {
            throw new OperationCanceledException();
        }
        catch (OperationCanceledException)
        {
            var log = new Log($"Update {currency} from DB => [time is up]", "ERROR");
            await Logger.AddLogAsync(log).ConfigureAwait(false);

            throw;
        }
        catch (OdbcException ex)
        {
            foreach (var error in ex.Errors)
            {
                var log = new Log($"Update {currency} from DB => [{ex.Message}, {error}] ", "NEXT TRY");
                await Logger.AddLogAsync(log).ConfigureAwait(false);
            }
            await Task.Delay(10000, token).ConfigureAwait(false);
            await UpdateCurrenciesAsync(currencies, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var log = new Log($"Update {currency} from DB => [{ex.Message}] ", "ERROR");
            await Logger.AddLogAsync(log).ConfigureAwait(false);

            throw;
        }
    }
}