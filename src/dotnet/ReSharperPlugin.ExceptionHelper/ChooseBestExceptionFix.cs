using System;
using System.Threading.Tasks;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.Application.Threading.Tasks;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.ProjectsHost.SolutionHost.Progress;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.TextControl;
using JetBrains.Util;
using JetBrains.Util.Logging;
using JetBrains.Util.Threading.Tasks;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Rider.Model;

namespace ReSharperPlugin.ExceptionHelper;

[QuickFix]
public class ChooseBestExceptionFix(GenericExceptionHighlighting highlighting) : QuickFixBase
{
    private readonly GenericExceptionHighlighting _highlighting = highlighting;
    public override string Text => "Choose best exception type (AI)";

    public override bool IsAvailable(IUserDataHolder cache)
    {
        return _highlighting.IsValid();
    }

    protected override Action<ITextControl> ExecutePsiTransaction(
        ISolution solution, 
        IProgressIndicator progress)
    {
        var expression = _highlighting.CreationExpression;
        if (expression == null || !expression.IsValid()) 
            return null;
        
        var settingsStore = expression.GetSettingsStoreWithEditorConfig();
        var settings = settingsStore.GetKey<ExceptionHelperSettings>(SettingsOptimization.DoMeSlowly);
        
        if (!settings.EnableAiSuggestions)
        {
            MessageBox.ShowInfo("AI suggestions are disabled. Enable them in Settings → Tools → Exception Helper");
            return null;
        }
        
        var ollamaClient = solution.GetComponent<OllamaClient>();
        string context = ExceptionContextExtractor.ExtractContext(expression);
        var threading = solution.GetComponent<IThreading>();
        
        solution.Locks.Tasks.StartNew(
            Lifetime.Eternal,
            Scheduling.FreeThreaded,
            TaskPriority.Low,
            (Func<Task>)(async () => {
                try
                {
                    string prompt = ExceptionPromptBuilder.BuildPrompt(context);
                    
                    // Note: Ideally we would use a visible progress indicator here, 
                    // but we'll stick to background processing for now to ensure stability.
                    string suggestion = await ollamaClient.GenerateAsync(prompt).ConfigureAwait(false);
                    
                    if (string.IsNullOrWhiteSpace(suggestion))
                    {
                        threading.ExecuteOrQueue(Lifetime.Eternal, "Show no response error", () => 
                            MessageBox.ShowInfo("AI provided no response."));
                        return;
                    }

                    suggestion = suggestion.Trim();
                    if (!suggestion.EndsWith("Exception", StringComparison.OrdinalIgnoreCase)) 
                        suggestion += "Exception";

                    threading.ExecuteOrQueue(Lifetime.Eternal, "Apply AI Suggestion", () =>
                    {
                        using (solution.Locks.UsingWriteLock())
                        {
                            if (!expression.IsValid()) return;
                            
                            solution.GetPsiServices().Transactions.Execute("Apply AI Suggestion", () =>
                            {
                                var factory = CSharpElementFactory.GetInstance(expression);
                                var argumentsText = expression.ArgumentList?.GetText() ?? "()";
                                var newExpressionText = $"new {suggestion}({argumentsText})";
                
                                var newExpression = factory.CreateExpression(newExpressionText);
                                
                                ModificationUtil.ReplaceChild(expression, newExpression);
                
                                Logger.LogMessage(LoggingLevel.INFO, $"Replacement successful with {suggestion}");
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    threading.ExecuteOrQueue(Lifetime.Eternal, "Show AI error", () => 
                        MessageBox.ShowError($"Error getting AI suggestion: {ex.Message}"));
                }
            }));

        return null;
    }
}