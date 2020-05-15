using System;
using System.Threading.Tasks;
using DocumentUpload.Core;
using DocumentUpload.Core.Services;

namespace DocumentUpload.Services.Generators
{
	internal class UnknownDescriptionGenerator : IDescriptionGenerator
	{
		internal static IDescriptionGenerator Instance { get; } = new UnknownDescriptionGenerator();

		private UnknownDescriptionGenerator() { }
		public DocumentType Type => DocumentType.Unknown;
		public ValueTask<string> GetDescriptionAsync(ReadOnlyMemory<byte> fileContent) => default;

	}
}