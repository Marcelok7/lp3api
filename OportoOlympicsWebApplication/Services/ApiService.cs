// Services/ApiService.cs
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OportoOlympics.Controllers;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(ILogger<ApiService> logger)
    {
        _logger = logger;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://services.inapa.com/opo/api/")
        };

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("FG3:PtM#fh?R8o"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        _logger.LogInformation("HttpClient initialized with BaseAddress: " + _httpClient.BaseAddress);
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseData);
    }

    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        _logger.LogInformation($"Reach to APIService PostAsync | Endpoint -> {endpoint} | Data -> {data}");

        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var requestUri = new Uri(_httpClient.BaseAddress, endpoint);
        var response = await _httpClient.PostAsync(requestUri, content);

        _logger.LogInformation($"Request sent to: {response.RequestMessage?.RequestUri}");

        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseData);
    }

    public async Task<T> PutAsync<T>(string endpoint, object data)
    {
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseData);
    }

    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }
}