using SYS.CRU.CURRENCY.GRABBER.Data;
using SYS.CRU.CURRENCY.GRABBER.Parser;

namespace SYS.CRU.CURRENCY.GRABBER.Services;
/// <summary>
/// Фабрика парсеров.
/// </summary>
public class ParserFactory
{
    private readonly DownloadService _downloadService;
    private readonly CancellationToken _token;

    public ParserFactory(DownloadService downloadService, CancellationToken token)
    {
        _downloadService = downloadService;
        _token = token;
    }

    public BaseParser GetParserAsync(string countryCode)
    {
        return countryCode switch
        {
            "KZT" => new KztParser(_downloadService, _token),
            "RUB" => new RubParser(_downloadService, _token),
            _ => null!
        };
    }
}