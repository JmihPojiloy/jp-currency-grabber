namespace SYS.CRU.CURRENCY.GRABBER.Loggers;
/// <summary>
/// Текущее действие.
/// </summary>
public struct Log
{
    private DateTime DateTime { get; } = DateTime.Now;
    private string Message { get; }
    private string Status { get; } 

    public Log(string message, string status)
    {
        Message = message;
        Status = status;
    }

    public override string ToString()
    {
        return $"[{DateTime:yy-MM-dd}] : {Message} - [{Status}]";
    }
}