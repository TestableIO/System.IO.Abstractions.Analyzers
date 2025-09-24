using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace System.IO.Abstractions.Analyzers.Analyzers;

/// <inheritdoc />
public abstract class BaseFileSystemNodeAnalyzer : BaseFileSystemAnalyzer
{
	/// <inheritdoc />
	protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext)
	{
		compilationStartContext.RegisterOperationAction(operationContext =>
			{
				var invocationOperation = (IInvocationOperation) operationContext.Operation;

				if (IsFileSystemType(invocationOperation.TargetMethod.ContainingType, GetFileSystemTypeSymbol(compilationStartContext.Compilation)))
				{
					Analyze(operationContext, invocationOperation);
				}
			},
			OperationKind.Invocation);

		compilationStartContext.RegisterOperationAction(operationContext =>
			{
				var objectCreationOperation = (IObjectCreationOperation) operationContext.Operation;

				if (IsFileSystemType(objectCreationOperation.Type, GetFileSystemTypeSymbol(compilationStartContext.Compilation)))
				{
					Analyze(operationContext, objectCreationOperation);
				}
			},
			OperationKind.ObjectCreation);
	}

	protected abstract void Analyze(OperationAnalysisContext context, IOperation operation);

	protected abstract Type GetFileSystemType();

	private INamedTypeSymbol GetFileSystemTypeSymbol(Compilation compilation) => compilation.GetTypeByMetadataName(GetFileSystemType().FullName);

	private static bool IsFileSystemType(ITypeSymbol typeSymbol, INamedTypeSymbol fileSystemType)
	{
		if (typeSymbol is null)
		{
			return false;
		}

		return typeSymbol.Equals(fileSystemType, SymbolEqualityComparer.Default);
	}

	protected static bool IsFirstConstructorParameterOfType<T>(OperationAnalysisContext context, IOperation operation)
	{
		var parameterTypeSymbol = context.Compilation.GetTypeByMetadataName(typeof(T).FullName);
		return (operation as IObjectCreationOperation)?.Arguments.FirstOrDefault()?.Parameter?.Type?.Equals(parameterTypeSymbol, SymbolEqualityComparer.Default) ?? false;
	}
}