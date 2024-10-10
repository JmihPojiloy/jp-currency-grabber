namespace SYS.CRU.CURRENCY.GRABBER.Dto;

public class CurrencyDtoBuilder
{
    private readonly CurrencyDto _currencyDto;

    public CurrencyDtoBuilder()
    {
        _currencyDto = new CurrencyDto();
    }

    public CurrencyDtoBuilder WithBCurrencyID(string? bCurrencyID)
    {
        _currencyDto.BCurrencyID = bCurrencyID;
        return this;
    }

    public CurrencyDtoBuilder WithTCurrencyID(string? tCurrencyID)
    {
        _currencyDto.TCurrencyID = tCurrencyID;
        return this;
    }

    public CurrencyDtoBuilder WithIsVisible(bool isVisible)
    {
        _currencyDto.IsVisible = isVisible;
        return this;
    }

    public CurrencyDtoBuilder WithIsManual(bool isManual)
    {
        _currencyDto.IsManual = isManual;
        return this;
    }

    public CurrencyDtoBuilder WithRate(decimal? rate)
    {
        _currencyDto.Rate = rate;
        return this;
    }

    public CurrencyDtoBuilder WithFee(decimal? fee)
    {
        _currencyDto.Fee = fee;
        return this;
    }

    public CurrencyDtoBuilder WithUpdated(DateTime updated)
    {
        _currencyDto.Updated = updated;
        return this;
    }

    public CurrencyDto Build()
    {
        return _currencyDto;
    }
}