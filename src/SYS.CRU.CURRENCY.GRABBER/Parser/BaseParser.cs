using System.Text;
using System.Xml;
using SYS.CRU.CURRENCY.GRABBER.Data;
using SYS.CRU.CURRENCY.GRABBER.Loggers;
using SYS.CRU.CURRENCY.GRABBER.Services;

namespace SYS.CRU.CURRENCY.GRABBER.Parser;
/// <summary>
/// Абстрактный класс - интерфейс парсеров.
/// </summary>
public abstract class BaseParser
{
    private protected string? Url = string.Empty;
    private protected string? CurrencyCode = string.Empty;

    private protected readonly DownloadService DownloadService;
    private protected readonly CancellationToken Token;

    protected BaseParser(DownloadService downloadService, CancellationToken token)
    {
        DownloadService = downloadService;
        Token = token;
    }
    
    /// <summary>
    /// Метод парсинга, реализуемый дочерними классами.
    /// </summary>
    /// <param name="currencies"></param>
    /// <returns></returns>
    public abstract Task<Currency[]> ParseAsync(Currency[] currencies);
}