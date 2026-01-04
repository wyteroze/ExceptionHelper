namespace ReSharperPlugin.ExceptionHelper;

public static class ExceptionPromptBuilder
{
    public static string BuildPrompt(string codeContext)
    {
        return $"""
                 You are an expert C# developer. Analyze the code and suggest the MOST SPECIFIC exception type.
                 
                 {codeContext}
                 
                 # Common Exception Patterns:
                 
                 ## ArgumentNullException
                 Use when: A parameter is null
                 Example: if (name == null) throw -> ArgumentNullException
                 
                 ## ArgumentOutOfRangeException  
                 Use when: A parameter value is outside valid range
                 Example: if (age < 0 || age > 150) throw -> ArgumentOutOfRangeException
                 
                 ## InvalidOperationException
                 Use when: Operation invalid due to current object state
                 Example: if (!isInitialized) throw -> InvalidOperationException
                 
                 ## NotImplementedException
                 Use when: Feature not yet implemented
                 Example: ""not yet implemented"", ""TODO"", ""coming soon"" -> NotImplementedException
                 
                 ## NotSupportedException
                 Use when: Operation not supported (different from not implemented)
                 Example: ""not supported"", ""unsupported"" -> NotSupportedException
                 
                 ## ObjectDisposedException
                 Use when: Operating on disposed object
                 Example: if (stream == null || disposed) throw -> ObjectDisposedException
                 
                 ## FormatException
                 Use when: String format is invalid
                 Example: ""invalid format"", ""cannot parse"" -> FormatException
                 
                 ## IndexOutOfRangeException
                 Use when: Array/collection index out of bounds
                 Example: if (index < 0 || index >= length) throw -> IndexOutOfRangeException
                 
                 ## FileNotFoundException
                 Use when: File does not exist
                 Example: if (!File.Exists(path)) throw -> FileNotFoundException
                 
                 ## UnauthorizedAccessException
                 Use when: Permission denied
                 Example: ""permission"", ""not authorized"" -> UnauthorizedAccessException
                 
                 ## DivideByZeroException
                 Use when: Division by zero
                 Example: if (denominator == 0) throw -> DivideByZeroException
                 
                 # Instructions:
                 1. READ the analysis hints - they're usually correct
                 2. Look at the error MESSAGE - it often tells you exactly what's wrong
                 3. Consider the CODE CONTEXT - what's being checked?
                 4. Choose the MOST SPECIFIC exception that matches
                 5. If it's clearly parameter validation at method start -> Argument* exception
                 6. If it's about object state -> InvalidOperationException
                 7. If it mentions ""null"" and checks a parameter -> ArgumentNullException
                 8. If it checks value range -> ArgumentOutOfRangeException
                 
                 Respond with ONLY the exception name.
                 Examples: ""ArgumentNullException"", ""InvalidOperationException"", ""NotImplementedException""
                 
                 Exception name:
                 """;
    }
}