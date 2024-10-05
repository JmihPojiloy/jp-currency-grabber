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
    private readonly string _url;
    private readonly HttpClient _httpClient;
    private readonly DownloadService _downloadService;
    private readonly ParseService _parseService;
    private readonly CurrencyRepository _currencyRepository;
    private readonly CancellationToken _token;

    public CurrencyService(string currencyCode, string url, string connectionString, string proxy, CancellationToken token)
    {
        _currencyCode = currencyCode;
        _connectionString = connectionString;
        _url = url;

        _httpClient = HttpService.GetHttpClient(proxy);
        _downloadService = new DownloadService(_httpClient);
        _parseService = new ParseService();
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
            var downloadTask = _downloadService.DownloadAsync(_url, _token);
            var currenciesTask = _currencyRepository.GetAllCurrenciesByIdAsync(_currencyCode, _token);

            await Task.WhenAll(downloadTask, currenciesTask);

            var update = await downloadTask;
            var currencies = await currenciesTask;

            var updateCurrencies = await _parseService.GetCurrenciesAsync(_currencyCode, update, currencies);
            if (updateCurrencies is null)
            {
                throw new ArgumentNullException($"Currency not supported.");
            }

            await _currencyRepository.UpdateCurrenciesAsync(updateCurrencies, _token);

            var log = new Log($"Update => {_currencyCode} currency ", " --- EXCELLENT.");
            await Logger.AddLogAsync(log).ConfigureAwait(false);

            return true;
        }
        catch (ArgumentNullException ex)
        {
            var log = new Log($"Update => {_currencyCode} currency {ex.Message}", " --- ERROR.");
            await Logger.AddLogAsync(log).ConfigureAwait(false);
            
            return false;
        }
        catch (Exception ex)
        {
            var log = new Log($"Update => {_currencyCode} currency {ex.Message}", " --- ERROR.");
            await Logger.AddLogAsync(log).ConfigureAwait(false);
            
            return false;
        }
    }
}