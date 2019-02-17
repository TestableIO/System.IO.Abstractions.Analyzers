using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers
{
	/// <inheritdoc />
	public abstract class BaseFileSystemAnalyzer : DiagnosticAnalyzer

	{
		protected static readonly string Category = typeof(IFileSystem).Namespace;

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

		protected abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext,
													FileSystemContext fileSystemContext);

		private static bool ShouldAnalyze(FileSystemContext fileSystemContext) => fileSystemContext.HasReference;
	}
}