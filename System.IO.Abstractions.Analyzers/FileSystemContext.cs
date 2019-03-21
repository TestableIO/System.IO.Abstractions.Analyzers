using Microsoft.CodeAnalysis;

namespace System.IO.Abstractions.Analyzers
{
	public class FileSystemContext
	{
		private readonly INamedTypeSymbol fileSystemType;

		public FileSystemContext(Compilation compilation)
		{
			fileSystemType = compilation.GetTypeByMetadataName(Constants.FileSystem);
		}

		public bool HasReference => FileSystemType != null;

		public INamedTypeSymbol FileSystemType => fileSystemType;
	}
}