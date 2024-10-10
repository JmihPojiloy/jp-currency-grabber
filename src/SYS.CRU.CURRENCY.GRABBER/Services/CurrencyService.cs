using SYS.CRU.CURRENCY.GRABBER.Data;
using SYS.CRU.CURRENCY.GRABBER.Loggers;

namespace SYS.CRU.CURRENCY.GRABBER.Services;
/// <summary>
/// Сервис для выполнения цикла обновления курса одной заданной валюты.
/// </summary>
public class CurrencyService
{
    private readonly string _currencyCode;
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;
    private readonly DownloadService _downloadService;
    private readonly ParserFactory _parserFactory;
    private readonly CurrencyRepository _currencyRepository;
    private readonly CancellationToken _token;

    public int InCount { get; private set; } = 0;
    public int OutCount { get; private set; } = 0;

    public CurrencyService(string currencyCode,string connectionString, string proxy, CancellationToken token)
    {
        _currencyCode = currencyCode;
        _connectionString = connectionString;

        _httpClient = HttpService.GetHttpClient(proxy);
        _downloadService = new DownloadService(_httpClient);
        _parserFactory = new ParserFactory(_downloadService, token);
        _currencyRepository = new CurrencyRepository(_connectionString);
        
        _token = token;
    }

    /// <summary>
    /// Основной метод для выполнения всего цикла обновления.
    /// </summary>
    /// <returns>true/false</returns>
    /// <exception cref="ArgumentNullException">Проверка выполняемых сервисов.</exception>
    public async Task<bool> Execute()
    {
        try
        {
            var currenciesTask = _currencyRepository.GetAllCurrenciesByIdAsync(_currencyCode, _token);

            await Task.WhenAll(currenciesTask);

            var parser = _parserFactory.GetParserAsync(_currencyCode);
            var currencies = await currenciesTask;

            InCount += currencies.Length;
            
            var updateCurrencies = await parser.ParseAsync(currencies);
            if (updateCurrencies is null)
            {
                throw new ArgumentNullException($"Currency not supported.");
            }
            
            OutCount += updateCurrencies.Length;
            
            await _currencyRepository.UpdateCurrenciesAsync(updateCurrencies, _token);

            return true;
        }
        catch (ArgumentNullException ex)
        {
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}