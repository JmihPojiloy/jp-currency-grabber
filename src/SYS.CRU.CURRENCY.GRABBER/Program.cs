using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using SYS.CRU.CURRENCY.GRABBER.Helpers;
using SYS.CRU.CURRENCY.GRABBER.Loggers;
using SYS.CRU.CURRENCY.GRABBER.Services;

namespace SYS.CRU.CURRENCY.GRABBER;

public static class Program
{
    private static IConfiguration? _configuration;
    private static string? _connectionString;
    private static string? _proxy;
    private static List<string>? _exchangeRates;
    
    public static async Task Main()
    {
        await Console.Out.WriteLineAsync("Starting...");
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        _configuration = builder.Build();

        var connectionStringBuilder = new ConnectionStringBuilder(_configuration);
        var connectionName = _configuration.
            GetSection("ConnectionStrings")
            .GetChildren()
            .FirstOrDefault()?.Key;

        if (connectionName is null)
        {
            await Console.Out.WriteLineAsync("No connection string found.");
            return;
        }
        
        _connectionString = connectionStringBuilder.GetConnectionString(connectionName!);
        _proxy = _configuration["Proxy"];

        _exchangeRates = _configuration.GetSection("UpdatedExchangeRates").Get<List<string>>();
        
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromMinutes(1));
        var token = cancellationTokenSource.Token;
        
        await Console.Out.WriteLineAsync("Configuration complete.");
        
        var stopWatch = Stopwatch.StartNew();
        
        var currencyServices = _exchangeRates
            .Select(currencyCode => new CurrencyService(currencyCode, _connectionString, _proxy, token))
            .ToArray();
        
        var executions = currencyServices.Select(service => service.Execute());
        var result = await Task.WhenAll(executions);

        var counts = currencyServices.Select(s => new
        {
            inCount = s.InCount,
            outCount = s.OutCount,
        }).ToArray();
        
        var totalInCount = counts.Sum(c => c.inCount);
        var totalOutCount = counts.Sum(c => c.outCount);
        
        stopWatch.Stop();

        await Logger.SaveLogsAsync();
        
        await Console.Out.WriteLineAsync(
            $@"[DESCRIPTION] updated {totalInCount}/{totalOutCount} time {stopWatch.Elapsed:hh\:mm\:ss}.");
    }
}