using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers
{
	/// <inheritdoc />
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	[UsedImplicitly]
	public class FileServiceInterfaceInjectionAnalyzer : BaseFileSystemAnalyzer
	{
		/// <summary>
		/// Diagnostic Identifier
		/// </summary>
		[UsedImplicitly]
		public const string DiagnosticId = "IO0001";

		/// <summary>
		/// Diagnostic Title
		/// </summary>
		private const string Title = "For improved application testability, use the IFileSystem.";

		/// <summary>
		/// Diagnostic Message Format
		/// </summary>
		public const string MessageFormat = "For improved application testability, use the IFileSystem.";

		/// <summary>
		/// Diagnostic Description
		/// </summary>
		private const string Description = "For improved application testability, use the IFileSystem.";

		/// <summary>
		/// Правило
		/// </summary>
		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
			Title,
			MessageFormat,
			Category,
			DiagnosticSeverity.Warning,
			true,
			Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext,
													FileSystemContext fileSystemContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				{
					var constructor = (ConstructorDeclarationSyntax) syntaxContext.Node;

					var compilationUnitSyntax = GetCompilationUnit(constructor);

					if (compilationUnitSyntax.Usings.All(x => x.Name.NormalizeWhitespace().ToFullString() != typeof(Path).Namespace))
					{
						return;
					}

					var fileSystem = constructor.ParameterList.Parameters.FirstOrDefault(x =>
						x.Type.NormalizeWhitespace().ToFullString() == fileSystemContext.FileSystemType.Name);

					if (fileSystem == null)
					{
						syntaxContext.ReportDiagnostic(Diagnostic.Create(Rule,
							constructor.GetLocation()));
					}
				},
				SyntaxKind.ConstructorDeclaration);
		}

		private CompilationUnitSyntax GetCompilationUnit(SyntaxNode node)
		{
			switch (node)
			{
				case null:
					return null;
				case CompilationUnitSyntax compilationUnitSyntax:
					return compilationUnitSyntax;
				default:
					return GetCompilationUnit(node.Parent);
			}
		}
	}
}