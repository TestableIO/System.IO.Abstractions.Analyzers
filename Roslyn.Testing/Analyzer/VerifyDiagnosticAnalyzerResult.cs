namespace Roslyn.Testing.Analyzer;

internal struct VerifyDiagnosticAnalyzerResult
{
	public bool Success { get; private set; }

	public string ErrorMessage { get; private set; }

	public static VerifyDiagnosticAnalyzerResult Ok() => new()
	{
		Success = true
	};

	public static VerifyDiagnosticAnalyzerResult Fail(string message) => new()
	{
		Success = false,
		ErrorMessage = message
	};
}