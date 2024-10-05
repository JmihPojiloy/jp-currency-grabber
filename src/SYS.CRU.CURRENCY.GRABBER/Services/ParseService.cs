using System.Xml;
using SYS.CRU.CURRENCY.GRABBER.Data;
using SYS.CRU.CURRENCY.GRABBER.Loggers;

namespace SYS.CRU.CURRENCY.GRABBER.Services;
/// <summary>
/// Парсинг текущего курса валют.
/// </summary>
public class ParseService
{
    public async Task<Currency[]> GetCurrenciesAsync(string countryCode, XmlDocument currenciesXml, Currency[] currencies)
    {
        return countryCode switch
        {
            "KZT" => await UpdateKZTRates(currenciesXml, currencies),
            "RUB" => await UpdateRUBRates(currenciesXml, currencies),
            _ => null!
        };
    }
    
    /// <summary>
    /// Обновление курса KZN.
    /// </summary>
    /// <param name="currenciesXml">Свежие курсы валют.</param>
    /// <param name="currencies">Старые курсы валют.</param>
    /// <returns>ОМассив обновленных курсов.</returns>
    private async Task<Currency[]> UpdateKZTRates(XmlDocument currenciesXml, Currency[] currencies)
    {
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
            }
        }

        var log = new Log("KZT parsing rates", " --- COMPLETED.");
        await Logger.AddLogAsync(log).ConfigureAwait(false);
        
        return result;
    }

    /// <summary>
    /// Обновление курса RUB.
    /// </summary>
    /// <param name="currenciesXml">Свежие курсы валют.</param>
    /// <param name="currencies">Старые курсы валют.</param>
    /// <returns>Массив обновленных курсов.</returns>
    private async Task<Currency[]> UpdateRUBRates(XmlDocument currenciesXml, Currency[] currencies)
    {
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
            }
        }
        
        var log = new Log("RUB parsing rates", " --- COMPLETED.");
        await Logger.AddLogAsync(log).ConfigureAwait(false);
        
        return result;
    }
}