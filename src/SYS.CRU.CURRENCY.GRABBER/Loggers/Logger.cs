using System.Collections.Concurrent;

namespace SYS.CRU.CURRENCY.GRABBER.Loggers;
/// <summary>
/// Запись информации о работе приложения с выводом в консоль.
/// </summary>
public static class Logger
{
    private static readonly ConcurrentBag<Log> _logs = new();
    private static readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

    static Logger()
    {
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }    
    }
    
    public static async Task AddLogAsync(Log log)
    {
        _logs.Add(log);
        await Console.Out.WriteLineAsync(log.ToString()).ConfigureAwait(false);
    }

    /// <summary>
    /// Логи сохраняются, перед окончанием работы приложения.
    /// </summary>
    public static async Task SaveLogsAsync()
    {
        var logName = $"{DateTime.Now:yyyy-MM-dd}.log";
        if (!File.Exists(Path.Combine(_path, logName)))
        {
            File.Create(Path.Combine(_path, logName)).Close();
        }
        
        await using var stream = new FileStream(Path.Combine(_path, logName), FileMode.Append, FileAccess.Write);
        stream.Seek(0, SeekOrigin.End);
        
        await using var writer = new StreamWriter(stream);
        foreach (var log in _logs)
        {
            await writer.WriteLineAsync(log.ToString());
        }
    }
}