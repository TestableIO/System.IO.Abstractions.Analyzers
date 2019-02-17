namespace Roslyn.Testing.CodeFix
{
	internal struct VerifyCodeFixProviderResult
	{
		public bool Success { get; private set; }

		public string ErrorMessage { get; private set; }

		public string ActualSource { get; private set; }

		public string NewSource { get; private set; }

		public static VerifyCodeFixProviderResult Ok()
		{
			return new VerifyCodeFixProviderResult
			{
				Success = true
			};
		}

		public static VerifyCodeFixProviderResult Fail(string message)
		{
			return new VerifyCodeFixProviderResult
			{
				Success = false,
				ErrorMessage = message
			};
		}

		internal static VerifyCodeFixProviderResult Fail(string newSource, string actualSource)
		{
			return new VerifyCodeFixProviderResult
			{
				Success = false,
				NewSource = newSource,
				ActualSource = actualSource
			};
		}
	}
}