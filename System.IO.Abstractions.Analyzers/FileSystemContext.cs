using Microsoft.CodeAnalysis;

namespace System.IO.Abstractions.Analyzers
{
	public class FileSystemContext
	{
		private readonly Lazy<INamedTypeSymbol> _lazyIFileSystemType;

		public INamedTypeSymbol FileSystemType
			=> _lazyIFileSystemType?.Value;

		public FileSystemContext(Compilation compilation)
		{
			_lazyIFileSystemType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(typeof(IFileSystem).FullName));
		}

		public bool HasReference => true;
	}
}