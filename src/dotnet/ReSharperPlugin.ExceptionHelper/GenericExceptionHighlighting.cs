using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ExceptionHelper;

[RegisterConfigurableSeverity(
    SeverityId,
    null,
    HighlightingGroupIds.BestPractice,
    "Generic Exception Usage",
    "Consider using a more specific exception type instead of generic Exception",
    Severity.SUGGESTION)]
[ConfigurableSeverityHighlighting(
    SeverityId,
    CSharpLanguage.Name,
    OverlapResolve = OverlapResolveKind.NONE,
    ToolTipFormatString = Message)]
public class GenericExceptionHighlighting(IObjectCreationExpression creationExpression) : IHighlighting
{
    private const string SeverityId = "GenericExceptionUsage";
    private const string Message = "Consider using a more specific exception type";

    public IObjectCreationExpression CreationExpression { get; } = creationExpression;

    public bool IsValid() => CreationExpression.IsValid();
    public DocumentRange CalculateRange() => CreationExpression.GetDocumentRange();

    public string ToolTip => Message;
    public string ErrorStripeToolTip => Message;
}