using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace System.IO.Abstractions.Analyzers.CodeActions;

public class FileStreamCodeAction : CodeAction
{
	private readonly ObjectCreationExpressionSyntax _creationExpressionSyntax;

	private readonly Document _document;

	private readonly FieldDeclarationSyntax _field;

	public FileStreamCodeAction(string title, Document document, ObjectCreationExpressionSyntax creationExpressionSyntax,
								FieldDeclarationSyntax field)
	{
		Title = title;
		_document = document;
		_creationExpressionSyntax = creationExpressionSyntax;
		_field = field;
	}

	public override string Title { get; }

	public override string EquivalenceKey => Title;

	protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(_document, cancellationToken).ConfigureAwait(false);
		var arguments = _creationExpressionSyntax.ArgumentList.Arguments.Select(x => x.ToFullString());

		editor.ReplaceNode(_creationExpressionSyntax,
			SF.ParseExpression($"{_field.Declaration.Variables.ToFullString()}.FileStream.Create({string.Join(",", arguments)})"));

		return await Formatter.FormatAsync(editor.GetChangedDocument(), cancellationToken: cancellationToken).ConfigureAwait(false);
	}
}