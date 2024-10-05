using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace SYS.CRU.CURRENCY.GRABBER.Services;
/// <summary>
/// Сервис для создания клиента.
/// </summary>
public static class HttpService
{
    public static HttpClient GetHttpClient(string proxy)
    {
        var services = new ServiceCollection();
        var handler = new HttpClientHandler();

        if (!string.IsNullOrEmpty(proxy))
        {
            handler.Proxy = new WebProxy(proxy, true);
            handler.UseProxy = true;
        }
        
        services.AddHttpClient("Client")
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        
        var httpClient = httpClientFactory?.CreateClient("Client");
        
        return httpClient!;
    }
}