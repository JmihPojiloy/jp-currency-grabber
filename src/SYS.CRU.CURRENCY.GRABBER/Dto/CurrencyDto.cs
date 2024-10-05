namespace SYS.CRU.CURRENCY.GRABBER.Dto;
/// <summary>
/// Dto для обмена с БД.
/// </summary>
public sealed class CurrencyDto
{
    public string? BCurrencyID { get; set; }
    public string? TCurrencyID { get; set; }
    public bool IsVisible {get; set;}
    public bool IsManual {get; set;}
    public decimal? Rate {get; set;}
    public decimal? Fee {get; set;}
    public DateTime Updated {get; set;}
}