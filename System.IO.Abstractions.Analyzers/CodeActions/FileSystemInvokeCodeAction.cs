using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace System.IO.Abstractions.Analyzers.CodeActions
{
	public class FileSystemInvokeCodeAction : CodeAction
	{
		private readonly Document _document;

		private readonly InvocationExpressionSyntax _invocation;

		private readonly FieldDeclarationSyntax _field;

		public override string Title { get; }

		public override string EquivalenceKey => Title;

		public FileSystemInvokeCodeAction(string title, Document document, InvocationExpressionSyntax invocation,
										FieldDeclarationSyntax field)
		{
			Title = title;
			_document = document;
			_invocation = invocation;
			_field = field;
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(_document, cancellationToken).ConfigureAwait(false);

			editor.ReplaceNode(_invocation,
				SF.ParseExpression(
					$"{_field.Declaration.Variables.FirstOrDefault().Identifier.Text}.{_invocation.NormalizeWhitespace().ToFullString()}"));

			return editor.GetChangedDocument();
		}
	}
}