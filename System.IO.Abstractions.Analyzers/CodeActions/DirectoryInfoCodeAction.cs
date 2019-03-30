using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace System.IO.Abstractions.Analyzers.CodeActions
{
	public class DirectoryInfoCodeAction : CodeAction
	{
		private readonly Document _document;

		private readonly ObjectCreationExpressionSyntax _creationExpressionSyntax;

		private readonly FieldDeclarationSyntax _field;

		public override string Title { get; }

		public override string EquivalenceKey => Title;

		public DirectoryInfoCodeAction(string title, Document document, ObjectCreationExpressionSyntax creationExpressionSyntax,
									FieldDeclarationSyntax field)
		{
			Title = title;
			_document = document;
			_creationExpressionSyntax = creationExpressionSyntax;
			_field = field;
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(_document, cancellationToken).ConfigureAwait(false);
			var arguments = _creationExpressionSyntax.ArgumentList.Arguments.Select(x => x.ToFullString());

			editor.ReplaceNode(_creationExpressionSyntax,
				SF.ParseExpression($"{_field.Declaration.Variables.ToFullString()}.DirectoryInfo.FromDirectoryName({string.Join(",", arguments)})"));

			return editor.GetChangedDocument();
		}
	}
}