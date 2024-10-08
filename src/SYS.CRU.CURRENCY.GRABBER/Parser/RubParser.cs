using System.Xml;
using SYS.CRU.CURRENCY.GRABBER.Data;
using SYS.CRU.CURRENCY.GRABBER.Loggers;
using SYS.CRU.CURRENCY.GRABBER.Services;

namespace SYS.CRU.CURRENCY.GRABBER.Parser;
/// <summary>
/// Парсер RUB.
/// </summary>
public class RubParser : BaseParser
{
    public RubParser(DownloadService downloadService, CancellationToken token) : base(downloadService, token)
    {
        CurrencyCode = "RUB";
        Url =
            $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Обновление курса RUB.
    /// </summary>
    /// <param name="currencies">Старые курсы валют.</param>
    /// <returns>Массив обновленных курсов.</returns>
    public override async Task<Currency[]> ParseAsync(Currency[] currencies)
    {
        XmlDocument currenciesXml = await DownloadService.DownloadAsync(Url!, Token).ConfigureAwait(false);
        var result = currencies;
        
        XmlNodeList currenciesNodes = currenciesXml.GetElementsByTagName("Valute");

        var values = from XmlNode valute in currenciesNodes
            let charCode = valute["CharCode"]?.InnerText.Trim()
            let value = decimal.TryParse(valute["VunitRate"]?.InnerText.Trim(), out var result) ? result : 0m
            select new
            {
                CharCode = charCode,
                Rate = value
            };

        foreach (var cur in result)
        {
            var update = values.FirstOrDefault(id => id.CharCode == cur.TCurrencyID);

            if (update != null && update.Rate != 0m)
            {
                cur!.Rate = update!.Rate;
                var log = new Log($"{CurrencyCode} => {update.CharCode} = {update!.Rate}", "OK");
                await Logger.AddLogAsync(log).ConfigureAwait(false);
            }
        }
        
        return result;
    }
}