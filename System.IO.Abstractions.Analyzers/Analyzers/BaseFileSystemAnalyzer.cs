using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers;

/// <inheritdoc />
public abstract class BaseFileSystemAnalyzer : DiagnosticAnalyzer
{
	/// <summary>
	/// Diagnostic Analyzer Category
	/// </summary>
	protected const string Category = Constants.FileSystemNameSpace;

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(compilationStartContext =>
		{
			var fileSystemContext = new FileSystemContext(compilationStartContext.Compilation);

			if (ShouldAnalyze(fileSystemContext))
			{
				AnalyzeCompilation(compilationStartContext, fileSystemContext);
			}
		});
	}

	/// <summary>
	/// Analysis
	/// </summary>
	/// <param name="compilationStartContext"> Compilation Start Analysis Context </param>
	/// <param name="fileSystemContext"> FileSystem Context </param>
	protected abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext,
												FileSystemContext fileSystemContext);

	private static bool ShouldAnalyze(FileSystemContext fileSystemContext)
	{
		return fileSystemContext.HasReference;
	}
}