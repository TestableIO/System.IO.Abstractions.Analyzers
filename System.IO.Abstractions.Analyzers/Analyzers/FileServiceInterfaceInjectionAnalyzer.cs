using System.Collections.Immutable;
using System.IO.Abstractions.Analyzers.RoslynToken;
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
		public const string DiagnosticId = Constants.Io0001;

		/// <summary>
		/// Diagnostic Title
		/// </summary>
		private const string Title = "Use System.IO.Abstractions for improved application testability";

		/// <summary>
		/// Diagnostic Message Format
		/// </summary>
		public const string MessageFormat = "Use System.IO.Abstractions for improved application testability";

		/// <summary>
		/// Diagnostic Description
		/// </summary>
		private const string Description = "Use System.IO.Abstractions for improved application testability";

		private const string Link = "http://bit.ly/2OyHRbq";

		/// <summary>
		/// Правило
		/// </summary>
		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
			Title,
			MessageFormat,
			Category,
			DiagnosticSeverity.Warning,
			true,
			Description,
			Link);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext,
													FileSystemContext fileSystemContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				{
					var classDeclarationSyntax = (ClassDeclarationSyntax) syntaxContext.Node;

					var compilationUnitSyntax = GetCompilationUnit(classDeclarationSyntax);

					if (compilationUnitSyntax.Usings.All(x => x.Name.NormalizeWhitespace().ToFullString() != typeof(Path).Namespace))
					{
						return;
					}

					if (RoslynClassFileSystem.HasFileSystemField(classDeclarationSyntax)
						&& RoslynClassFileSystem.HasConstructor(classDeclarationSyntax))
					{
						var ctor = RoslynClassFileSystem.GetConstructor(classDeclarationSyntax);
						var field = RoslynClassFileSystem.GetFileSystemFieldFromClass(classDeclarationSyntax);

						if (!RoslynClassFileSystem.ConstructorHasAssignmentExpression(ctor, field.Declaration.Variables.ToFullString()))
						{
							syntaxContext.ReportDiagnostic(Diagnostic.Create(Rule,
								classDeclarationSyntax.Identifier.GetLocation()));
						}
					} else
					{
						syntaxContext.ReportDiagnostic(Diagnostic.Create(Rule,
							classDeclarationSyntax.Identifier.GetLocation()));
					}
				},
				SyntaxKind.ClassDeclaration);
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