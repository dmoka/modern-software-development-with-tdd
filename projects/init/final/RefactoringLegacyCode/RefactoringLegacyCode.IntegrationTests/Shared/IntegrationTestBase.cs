using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RefactoringLegacyCode.IntegrationTests.Shared;

public abstract class IntegrationTestBase
    : IClassFixture<IntegrationTestWebFactory>, IDisposable
{
    public IntegrationTestBase(IntegrationTestWebFactory factory)
    {
        _serviceScope = factory.Services.CreateScope();
        _httpClient = factory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7294");
    }

    protected IServiceScope _serviceScope;
    protected HttpClient _httpClient;
    private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<TData?> GetAsync<TData>(string url)
    {
        var result = await _httpClient.GetAsync(url);
        var content = await result.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<TData>(content, _serializerOptions);

        return data;
    }

    public async Task<TResult?> PostAsync<TData, TResult>(string url, TData data)
    {
        var stringContent = new StringContent(JsonSerializer.Serialize(data),
            Encoding.UTF8, "application/json");

        var result = await _httpClient.PostAsync(url, stringContent);
        result.EnsureSuccessStatusCode();

        var content = await result.Content.ReadAsStringAsync();
        var returnData = content != "" ? JsonSerializer.Deserialize<TResult>(content, _serializerOptions) : (dynamic)null!;

        return returnData;
    }

    public async Task PostAsync<TData>(string url, TData data)
    {
        var stringContent = new StringContent(JsonSerializer.Serialize(data),
            Encoding.UTF8, "application/json");

        var result = await _httpClient.PostAsync(url, stringContent);

        result.EnsureSuccessStatusCode();

        await result.Content.ReadAsStringAsync();
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        _httpClient.Dispose();
    }
}