using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;
using JetBrains.ReSharper.Resources.Settings;

namespace ReSharperPlugin.ExceptionHelper;

[SettingsKey(typeof(CodeEditingSettings), "Exception Helper settings")]
public class ExceptionHelperSettings
{
    [SettingsEntry("qwen2.5-coder:7b", "Ollama model to use for exception suggestions")]
    public string OllamaModel { get; set; }
    
    [SettingsEntry("http://localhost:11434", "Ollama server URL")]
    public string OllamaUrl { get; set; }
    
    [SettingsEntry(30, "Temperature for AI generation (0-100), lower = more deterministic, higher = may hallucinate more)")]
    public int Temperature { get; set; }
    
    [SettingsEntry(true, "Enable AI-powered exception suggestions")]
    public bool EnableAiSuggestions { get; set; }
}