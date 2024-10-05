using System.ComponentModel.DataAnnotations;
using SYS.CRU.CURRENCY.GRABBER.Dto;

namespace SYS.CRU.CURRENCY.GRABBER.Data;
/// <summary>
/// Основная модель для работы.
/// </summary>
public class Currency
{
    private readonly CurrencyDto _dto;

    public string? BCurrencyID
    {
        get => _dto.BCurrencyID;
        set => _dto.BCurrencyID = value;
    }

    public string? TCurrencyID
    {
        get => _dto.TCurrencyID;
        set => _dto.TCurrencyID = value;
    }

    public bool IsVisible
    {
        get => _dto.IsVisible;
        set => _dto.IsVisible = value;
    }

    public bool IsManual
    {
        get => _dto.IsManual;
        set => _dto.IsManual = value;
    }

    public decimal? Rate
    {
        get => _dto.Rate;
        set => _dto.Rate = value;
    }

    public decimal? Fee
    {
        get => _dto.Fee;
        set => _dto.Fee = value;
    }

    public DateTime Updated
    {
        get => _dto.Updated;
        set => _dto.Updated = value;
    }

    private Currency(CurrencyDto dto)
    {
        _dto = dto;
    }
    
    /// <summary>
    /// Маппер для БД
    /// </summary>
    public static class Mapper
    {
        public static Currency? Map(CurrencyDto dto) => new Currency(dto);
        public static CurrencyDto? Map(Currency? currency) => currency._dto;
    }
}