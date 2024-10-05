using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using SYS.CRU.CURRENCY.GRABBER.Loggers;
using SYS.CRU.CURRENCY.GRABBER.Services;

namespace SYS.CRU.CURRENCY.GRABBER;

public static class Program
{
    private static IConfiguration? _configuration;
    private static string? _connectionString;
    private static string? _proxy;
    private static Dictionary<string, string>? _exchangeRatesDictionary;
    
    public static async Task Main()
    {
        await Console.Out.WriteLineAsync("Starting...");
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        _configuration = builder.Build();
        
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
        _proxy = _configuration["Proxy"];
        
        var updatedExchangeRates = _configuration.GetSection("UpdatedExchangeRates").GetChildren();

        _exchangeRatesDictionary = updatedExchangeRates.ToDictionary(
            x => x.GetValue<string>("currency"),
            x =>
            {
                var url = x.GetValue<string>("url");
                var formattedDate = DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                return string.Format(url, formattedDate);
            }
        );
        
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromMinutes(1));
        var token = cancellationTokenSource.Token;
        
        await Console.Out.WriteLineAsync("Configuration complete.");
        
        var stopWatch = Stopwatch.StartNew();
        
        var currencyServices = _exchangeRatesDictionary
            .Select(kvp => new CurrencyService(kvp.Key, kvp.Value, _connectionString, _proxy, token))
            .ToArray();
        
        var executions = currencyServices.Select(service => service.Execute());
        var result = await Task.WhenAll(executions);

        var count = result.Count(r => r == true);
        
        stopWatch.Stop();

        await Logger.SaveLogsAsync();
        
        await Console.Out.WriteLineAsync(
            $@"[DESCRIPTION] updated {{{count}/{updatedExchangeRates.Count()}}} time {stopWatch.Elapsed:hh\:mm\:ss}.");
    }
}