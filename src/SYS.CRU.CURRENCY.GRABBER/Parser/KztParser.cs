using System.Xml;
using SYS.CRU.CURRENCY.GRABBER.Data;
using SYS.CRU.CURRENCY.GRABBER.Loggers;
using SYS.CRU.CURRENCY.GRABBER.Services;

namespace SYS.CRU.CURRENCY.GRABBER.Parser;
/// <summary>
/// Парсер KZT.
/// </summary>
public class KztParser : BaseParser
{
    public KztParser(DownloadService downloadService, CancellationToken token) : base(downloadService, token)
    {
        CurrencyCode = "KZT";
        Url =
            $"https://nationalbank.kz/rss/get_rates.cfm?fdate={DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)}";
    }
    
    /// <summary>
    /// Обновление курса KZN.
    /// </summary>
    /// <param name="currencies">Старые курсы валют.</param>
    /// <returns>Массив обновленных курсов.</returns>
    public override async Task<Currency[]> ParseAsync(Currency[] currencies)
    {
        XmlDocument currenciesXml = await DownloadService.DownloadAsync(Url!, Token);
        
        var result = currencies;
        
        XmlNodeList currenciesNodes = currenciesXml.GetElementsByTagName("item");

        var values = from XmlNode valute in currenciesNodes
            let charCode = valute["title"]?.InnerText.Trim()
            let descriptionValue = valute["description"]?.InnerText.Trim()
            // Заменяем точку на запятую для корректного парсинга
            let adjustedDescriptionValue = descriptionValue.Replace('.', ',') 
            let value = decimal.TryParse(adjustedDescriptionValue, out var rate) ? rate : 0m
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