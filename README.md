# ExceptionHelper 
<p>ExceptionHelper is a JetBrains Rider and ReSharper plugin that uses AI to help you choose the best C# exception types for your situation.</p>

<p>It analyzes your code's context and the error message provided in a generic <code>throw new Exception("...")</code> statement to suggest a more precise exception from the standard .NET library (ex., <code>ArgumentNullException</code>, <code>InvalidOperationException</code>, <code>FileNotFoundException</code>, etc.).</p>

<img src="https://raw.githubusercontent.com/wyteroze/ExceptionHelper/refs/heads/main/docs/demo.gif" alt="ExceptionHelper in Action" />

<h3>Features:</h3>
<ul>
<li><b>AI powered suggestions</b>: Uses a local LLM to analyze your code and suggest the best exception type.</li>
<li><b>Context aware</b>: Considers method parameters, variable checks, and error messages.</li>
<li><b>Local and private</b>: Powered by <a href="https://ollama.com/">Ollama</a>, ensuring your code stays on your machine.</li>
<li><b>Seamless integration</b>: Provides a standard QuickFix (Alt+Enter) in the editor.</li>
</ul>

<h3>Getting started</h3>
<ol>
<li>Ensure <a href="https://ollama.com/">Ollama</a> is installed and running.</li>
<li>Access the plugin's settings in <code>Settings → Tools → Exception Helper</code> to choose the model you want to use</li>
<li><b>Note</b>: You must pull the model first from your terminal (ex. <code>ollama pull qwen2.5-coder:7b</code>) and make sure it's served (<code>ollama serve</code>)</li>
<li>Use <b>Alt+Enter</b> or click the lightbulb on the left of a generic <code>Exception</code> keyword and select <b>Choose best exception type (AI)</b>.</li>
</ol>

<h3>Other stuff</h3>
<p>This project uses the MIT license</p>