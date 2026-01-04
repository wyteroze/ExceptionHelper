using System.Linq;
using System.Text;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ExceptionHelper;

public static class ExceptionContextExtractor
{
    public static string ExtractContext(IObjectCreationExpression creationExpression)
    {
        var context = new StringBuilder();
        
        // Get throw statement
        var throwStatement = creationExpression.GetContainingNode<IThrowStatement>();
        
        // Get containing method
        var method = throwStatement?.GetContainingNode<IMethodDeclaration>();
        if (method != null)
        {
            context.AppendLine("=== Method Information ===");
            
            // Method name and visibility
            var modifiers = method.ModifiersList?.Modifiers
                .Select(m => m.GetText())
                .Where(m => !string.IsNullOrWhiteSpace(m));
            if (modifiers != null && modifiers.Any())
            {
                context.AppendLine($"Modifiers: {string.Join(" ", modifiers)}");
            }
            
            context.AppendLine($"Method: {method.DeclaredName}");
            
            // Parameters with types
            if (method.ParameterDeclarations.Count > 0)
            {
                context.AppendLine("Parameters:");
                foreach (var param in method.ParameterDeclarations)
                {
                    var paramType = param.Type.GetPresentableName(CSharpLanguage.Instance);
                    context.AppendLine($"  - {paramType} {param.DeclaredName}");
                }
            }
            else
            {
                context.AppendLine("Parameters: none");
            }

            // Return type
            var returnType = method.Type.GetPresentableName(CSharpLanguage.Instance);
            context.AppendLine($"Returns: {returnType}");
            
            context.AppendLine();
        }
        
        // Get the containing class for additional context
        var classDeclaration = throwStatement?.GetContainingNode<IClassDeclaration>();
        if (classDeclaration != null)
        {
            context.AppendLine($"Class: {classDeclaration.DeclaredName}");
            context.AppendLine();
        }
        
        // Extract error message with better parsing
        context.AppendLine("=== Exception Details ===");
        var arguments = creationExpression.Arguments;
        if (arguments.Count > 0)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i].Value;
                if (arg != null)
                {
                    var argText = arg.GetText().Trim('"');
                    context.AppendLine(i == 0 
                        ? $"Message: \"{argText}\"" 
                        : $"Argument {i + 1}: {argText}");
                }
            }
        }
        else
        {
            context.AppendLine("Message: (none provided)");
        }
        context.AppendLine();
        
        // Get surrounding code with better formatting
        context.AppendLine("=== Code Context ===");
        
        // Get the immediate containing statement (if/else, try/catch, etc.)
        var parentStatement = throwStatement?.Parent;
        if (parentStatement != null)
        {
            var statementType = GetStatementType(parentStatement);
            if (!string.IsNullOrEmpty(statementType))
            {
                context.AppendLine($"Inside: {statementType}");
            }
        }
        
        // Get relevant code block (limit to 15 lines for context)
        var containingBlock = throwStatement?.GetContainingNode<IBlock>();
        if (containingBlock != null)
        {
            var blockText = containingBlock.GetText();
            var lines = blockText.Split('\n');
            
            // If block is too long, get surrounding context
            if (lines.Length > 15)
            {
                var throwLineIndex = FindThrowStatementLine(lines, throwStatement);
                var startLine = System.Math.Max(0, throwLineIndex - 7);
                var endLine = System.Math.Min(lines.Length, throwLineIndex + 8);
                
                if (startLine > 0)
                    context.AppendLine("... (code above omitted)");
                
                for (int i = startLine; i < endLine; i++)
                {
                    var marker = i == throwLineIndex ? " <-- HERE" : "";
                    context.AppendLine(lines[i] + marker);
                }
                
                if (endLine < lines.Length)
                    context.AppendLine("... (code below omitted)");
            }
            else
            {
                context.AppendLine(blockText);
            }
        }
        
        // Analyze nearby null checks or validation
        context.AppendLine();
        context.AppendLine("=== Analysis Hints ===");
        
        var hints = AnalyzeContext(throwStatement, method);
        if (hints.Count > 0)
        {
            foreach (var hint in hints)
            {
                context.AppendLine($"- {hint}");
            }
        }
        else
        {
            context.AppendLine("- No specific patterns detected");
        }

        return context.ToString();
    }
    
    private static string GetStatementType(ITreeNode node)
    {
        return node switch
        {
            IIfStatement => "if statement",
            ITryStatement => "try block",
            ICatchClause => "catch block",
            IForStatement => "for loop",
            IForeachStatement => "foreach loop",
            IWhileStatement => "while loop",
            ISwitchStatement => "switch statement",
            _ => null
        };
    }
    
    private static int FindThrowStatementLine(string[] lines, IThrowStatement throwStatement)
    {
        var throwText = throwStatement.GetText();
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("throw") && lines[i].Contains("Exception"))
                return i;
        }
        return lines.Length / 2; // Fallback to middle
    }
    
    private static System.Collections.Generic.List<string> AnalyzeContext(
        IThrowStatement throwStatement, 
        IMethodDeclaration method)
    {
        var hints = new System.Collections.Generic.List<string>();
        
        if (method == null) return hints;
        
        // Check for null checks
        var methodBody = method.Body;
        if (methodBody != null)
        {
            var hasNullCheck = methodBody.GetText().Contains("== null") 
                               || methodBody.GetText().Contains("is null");
            if (hasNullCheck)
            {
                hints.Add("Code contains null checks - consider ArgumentNullException or NullReferenceException");
            }
            
            // Check for parameter validation
            var throwsInParameterCheck = IsInParameterValidation(throwStatement, method);
            if (throwsInParameterCheck)
            {
                hints.Add("Thrown during parameter validation - likely ArgumentException or ArgumentNullException");
            }
            
            // Check for state validation
            var hasStateCheck = methodBody.GetText().Contains("State") 
                                || methodBody.GetText().Contains("IsInitialized")
                                || methodBody.GetText().Contains("IsValid");
            if (hasStateCheck)
            {
                hints.Add("Code contains state checks - consider InvalidOperationException");
            }
            
            // Check for not supported scenarios
            var hasNotSupported = methodBody.GetText().Contains("not supported")
                                  || methodBody.GetText().Contains("not implemented");
            if (hasNotSupported)
            {
                hints.Add("Contains 'not supported' message - consider NotSupportedException or NotImplementedException");
            }
        }
        
        return hints;
    }
    
    private static bool IsInParameterValidation(IThrowStatement throwStatement, IMethodDeclaration method)
    {
        // Check if the throw is within the first few statements (typically parameter validation)
        var methodStatements = method.Body?.Statements;
        if (methodStatements == null)
            return false;
        
        var statements = methodStatements.Value;
        if (statements.Count == 0)
            return false;
        
        // Get the index of the statement containing the throw
        for (int i = 0; i < System.Math.Min(3, statements.Count); i++)
        {
            if (statements[i].Contains(throwStatement))
                return true;
        }
        
        return false;
    }
}