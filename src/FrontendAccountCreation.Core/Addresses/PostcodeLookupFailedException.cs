using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Addresses;


[ExcludeFromCodeCoverage]
public class PostcodeLookupFailedException : Exception
{
	public PostcodeLookupFailedException() { }
	public PostcodeLookupFailedException(string message) : base(message) { }
	public PostcodeLookupFailedException(string message, Exception inner) : base(message, inner) { }
}