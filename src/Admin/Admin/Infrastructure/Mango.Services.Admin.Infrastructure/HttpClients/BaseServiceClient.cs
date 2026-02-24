using System.Text.Json;
using Serilog;

namespace Mango.Services.Admin.Infrastructure.HttpClients;

/// <summary>
/// Base class for service-to-service HTTP communication.
/// Provides common functionality for all microservice clients.
/// </summary>
public abstract class BaseServiceClient
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    protected BaseServiceClient(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Sends a GET request and deserializes the response.
    /// </summary>
    /// <typeparam name="T">Type to deserialize response to.</typeparam>
    /// <param name="url">URL path (relative to base address).</param>
    /// <returns>Deserialized response object.</returns>
    protected async Task<T?> GetAsync<T>(string url) where T : class
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            return await HandleResponse<T>(response, url);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning(ex, "HTTP request failed for GET {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during GET {Url}", url);
            return null;
        }
    }

    /// <summary>
    /// Sends a POST request with JSON body and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">Type of request body.</typeparam>
    /// <typeparam name="TResponse">Type to deserialize response to.</typeparam>
    /// <param name="url">URL path (relative to base address).</param>
    /// <param name="request">Request body object.</param>
    /// <returns>Deserialized response object.</returns>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            return await HandleResponse<TResponse>(response, url);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning(ex, "HTTP request failed for POST {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during POST {Url}", url);
            return null;
        }
    }

    /// <summary>
    /// Sends a PUT request with JSON body and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">Type of request body.</typeparam>
    /// <typeparam name="TResponse">Type to deserialize response to.</typeparam>
    /// <param name="url">URL path (relative to base address).</param>
    /// <param name="request">Request body object.</param>
    /// <returns>Deserialized response object.</returns>
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);
            return await HandleResponse<TResponse>(response, url);
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning(ex, "HTTP request failed for PUT {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during PUT {Url}", url);
            return null;
        }
    }

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    /// <param name="url">URL path (relative to base address).</param>
    /// <returns>True if successful (2xx status code), false otherwise.</returns>
    protected async Task<bool> DeleteAsync(string url)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.Warning(ex, "HTTP request failed for DELETE {Url}", url);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during DELETE {Url}", url);
            return false;
        }
    }

    /// <summary>
    /// Handles HTTP response and deserializes to specified type.
    /// </summary>
    private async Task<T?> HandleResponse<T>(HttpResponseMessage response, string url) where T : class
    {
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.Warning(ex, "Failed to deserialize response from {Url}", url);
                return null;
            }
        }
        else
        {
            _logger.Warning(
                "HTTP request to {Url} returned {StatusCode}",
                url,
                response.StatusCode);
            return null;
        }
    }
}
