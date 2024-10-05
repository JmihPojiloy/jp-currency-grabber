using System.Text;
using System.Xml;
using SYS.CRU.CURRENCY.GRABBER.Loggers;

namespace SYS.CRU.CURRENCY.GRABBER.Services;
/// <summary>
/// Сервис получения курсов валют.
/// </summary>
public class DownloadService
{
    private readonly HttpClient _httpClient;

    public DownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Метод скачивания актуальных курсов валют.
    /// </summary>
    /// <param name="url">Ссылка на api банка.</param>
    /// <param name="token">Заданное время.</param>
    /// <returns>XML документ со всеми предоставляемыми курсами валют, либо null.</returns>
    /// <exception cref="HttpRequestException">При этой ошибке повторный запуск метода с интервалом 5 секунд.</exception>
    /// <exception cref="OperationCanceledException">Заданное время истекло.</exception>
    public async Task<XmlDocument> DownloadAsync(string url, CancellationToken token)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, token);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpRequestException($"{url} status code {response.StatusCode}");
            }

            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var content = Encoding.UTF8.GetString(contentBytes);

            var document = new XmlDocument();
            document.LoadXml(content);

            var log = new Log($"GET Currency rates => [{url}] ", " --- DOWNLOAD SUCCESS.");
            await Logger.AddLogAsync(log).ConfigureAwait(false);

            return document;
        }
        catch (HttpRequestException ex)
        {
            var log = new Log($"GET Currency rates => [{url} {ex.Message}]", " --- DOWNLOAD ERROR.");
            await Logger.AddLogAsync(log).ConfigureAwait(false);

            await Task.Delay(5000, token).ConfigureAwait(false);
            await DownloadAsync(url, token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw new OperationCanceledException();
        }
        catch (OperationCanceledException)
        {
            var log = new Log($"GET Currency rates => [{url} canceled, time is up!]", " --- DOWNLOAD ERROR.");
            await Logger.AddLogAsync(log).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var log = new Log($"GET Currency rates => [{url} {ex.Message}]", " --- DOWNLOAD ERROR.");
            await Logger.AddLogAsync(log).ConfigureAwait(false);
        }
        
        return null!;
    }
}