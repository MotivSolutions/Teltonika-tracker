using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OplexClient 
{
    readonly HttpClient _client = new();
    readonly string deviceKey;  

    public OplexClient(string server) 
    {
        var env = Environment.GetEnvironmentVariable("DEVICE_KEY");
        if(env == null)
        {
            throw new ArgumentNullException("Environment variable 'DEVICE_KEY' must be set");
        }

        deviceKey = env;
        _client.BaseAddress = new Uri(server);
    }

    public async Task Publish(string networkId, string payload)
    {
        var data = JsonSerializer.Serialize(new
        {
            MessageType = "DataMessage",
            data = JsonSerializer.Deserialize<Dictionary<string, object>>(payload)
        });
        using StringContent jsonContent = new(data);
        using HttpRequestMessage request = new(new HttpMethod("POST"), "callback/JsonIntegration/SingleObj");
        // Set request authorization header to Base64 encoded string networkId + ":" + deviceKey
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(networkId + ":" + deviceKey)));
        request.Content = jsonContent;

        using HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> authorize(string username, string password)
    {
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                username,
                password
            }),
            Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response = await _client.PostAsync("api/account/createtoken", jsonContent);
        response.EnsureSuccessStatusCode();
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse)["token"];
    }

}
