using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.UnitTestFramework.Resources;

namespace ReSharperPlugin.ExceptionHelper;

[OptionsPage(
    PageId,
    "Exception Helper",
    typeof(UnitTestingThemedIcons.Session),
    ParentId = ToolsPage.PID)]
public class ExceptionHelperOptionsPage : BeSimpleOptionsPage
{
    private const string PageId = "ExceptionHelperOptionsPage";
    
    public ExceptionHelperOptionsPage(
        Lifetime lifetime,
        OptionsPageContext optionsPageContext,
        OptionsSettingsSmartContext optionsSettingsSmartContext) 
        : base(lifetime, optionsPageContext, optionsSettingsSmartContext)
    {
        AddHeader("AI model configuration");
        
        AddBoolOption((ExceptionHelperSettings s) => s.EnableAiSuggestions,
            "Enable AI powered exception suggestions");
        
        AddStringOption((ExceptionHelperSettings s) => s.OllamaModel,
            "Ollama model:");
        
        AddCommentText("Common models: qwen2.5-coder:1.5b, qwen2.5-coder:3b, qwen2.5-coder:7b");
        AddCommentText("Smaller models are faster but less accurate. 7b recommended if you have 8GB+ RAM.");
        
        AddSpacer();
        
        AddHeader("Connection settings");
        
        AddStringOption((ExceptionHelperSettings s) => s.OllamaUrl,
            "Ollama server URL:");
        
        AddSpacer();
        
        AddHeader("Advanced settings");
        
        AddIntOption((ExceptionHelperSettings s) => s.Temperature,
            "Temperature (0-100):", minValue: 1, maxValue: 100);
        
        AddCommentText("Lower values (10-30) = more consistent, Higher values (50-80) = more creative");
    }
}