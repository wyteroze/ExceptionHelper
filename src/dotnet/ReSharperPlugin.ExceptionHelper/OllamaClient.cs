using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JetBrains.Diagnostics;
using JetBrains.Util.Logging;
using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.Lifetimes;
using JetBrains.Application.Parts;

namespace ReSharperPlugin.ExceptionHelper;

[SolutionComponent(Instantiation.DemandAnyThreadUnsafe)]
public class OllamaClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsStore _settingsStore;

    public OllamaClient(Lifetime lifetime, ISettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
        
        lifetime.OnTermination(() => _httpClient.Dispose());
    }
    
    public async Task<string> GenerateAsync(string prompt)
    {
        try
        {
            var settings = _settingsStore.BindToContextTransient(ContextRange.ApplicationWide)
                .GetKey<ExceptionHelperSettings>(SettingsOptimization.DoMeSlowly);

            var url = settings.OllamaUrl;
            if (string.IsNullOrWhiteSpace(url))
                url = "http://localhost:11434";
            
            var model = settings.OllamaModel;
            if (string.IsNullOrWhiteSpace(model))
                model = "qwen2.5-coder:7b";
            
            var temperature = settings.Temperature;
            if (temperature <= 0 || temperature > 100)
                temperature = 30;

            var baseUri = new Uri(url.TrimEnd('/') + "/");

            Logger.LogMessage(LoggingLevel.INFO, $"Ollama: Calling {baseUri}api/generate");
            
            var requestData = new
            {
                model,
                prompt,
                stream = false,
                options = new
                {
                    temperature = (double)temperature/100,
                    num_predict = 50,
                }
            };

            string json = JsonSerializer.Serialize(requestData);
            Logger.LogMessage(LoggingLevel.INFO, $"Ollama: Using model {model} with temperature {(double)temperature/100}");
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var requestUri = new Uri(baseUri, "api/generate");
            var response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);
            
            Logger.LogMessage(LoggingLevel.INFO, $"Ollama: Response status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();
            string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var result = JsonSerializer.Deserialize<OllamaResponse>(responseJson, options);
            
            if (result == null)
            {
                Logger.LogMessage(LoggingLevel.ERROR, "Deserialization returned null");
                return null;
            }
        
            if (string.IsNullOrWhiteSpace(result.Response))
            {
                Logger.LogMessage(LoggingLevel.WARN, "Response property is empty");
                return null;
            }
        
            Logger.LogMessage(LoggingLevel.INFO, $"Successfully got response: {result.Response}");
            return result.Response;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogMessage(LoggingLevel.ERROR, $"Ollama HTTP Error: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException)
        {
            Logger.LogMessage(LoggingLevel.ERROR, "Ollama request timed out");
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogMessage(LoggingLevel.ERROR, $"Ollama Error: {ex.Message}");
            Logger.LogException(ex);
            return null;
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var settings = _settingsStore.BindToContextTransient(ContextRange.ApplicationWide)
                .GetKey<ExceptionHelperSettings>(SettingsOptimization.DoMeSlowly);

            var url = settings.OllamaUrl;
            if (string.IsNullOrWhiteSpace(url))
                url = "http://localhost:11434";

            var baseUri = new Uri(url.TrimEnd('/') + "/");
            var requestUri = new Uri(baseUri, "api/tags");

            var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogMessage(LoggingLevel.WARN, $"Ollama not available: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

public class OllamaResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; }
    
    [JsonPropertyName("model")]
    public string Model { get; set; }
    
    [JsonPropertyName("done")]
    public bool Done { get; set; }
}