using Microsoft.CodeAnalysis;

namespace Roslyn.Testing.Model
{
	/// <summary>
	/// Struct that stores information about a Diagnostic appearing in a source
	/// </summary>
	public struct DiagnosticResult
	{
		private DiagnosticResultLocation[] _locations;

		public DiagnosticResultLocation[] Locations
		{
			get => _locations ?? (_locations = new DiagnosticResultLocation[] {});
			set => _locations = value;
		}

		public DiagnosticSeverity Severity { get; set; }

		public string Id { get; set; }

		public string Message { get; set; }

		public string Path => Locations.Length > 0 ? Locations[0].Path : "";

		public int Line => Locations.Length > 0 ? Locations[0].Line : -1;

		public int Column => Locations.Length > 0 ? Locations[0].Column : -1;
	}
}