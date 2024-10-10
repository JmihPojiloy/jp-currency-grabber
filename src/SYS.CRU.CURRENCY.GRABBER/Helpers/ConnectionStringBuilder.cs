using Microsoft.Extensions.Configuration;

namespace SYS.CRU.CURRENCY.GRABBER.Helpers;

public class ConnectionStringBuilder
{
    private readonly IConfiguration _configuration;

    public ConnectionStringBuilder(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString(string connectionName)
    {
        return connectionName switch
        {
            "ODBC" => GetOdbcConnectionString(),
            _ => null!
        };
    }

    private string GetOdbcConnectionString()
    {
        var connectionSettings = _configuration.GetSection("ConnectionStrings:ODBC");

        var driver = connectionSettings["Driver"];
        var server = connectionSettings["Server"];
        var port = connectionSettings["Port"];
        var database = connectionSettings["Database"];
        var uid = connectionSettings["Uid"];
        var pwd = connectionSettings["Pwd"];

        return $"Driver={driver};Server={server},{port};Database={database};Uid={uid};Pwd={pwd};Encrypt=no;";
    }
}