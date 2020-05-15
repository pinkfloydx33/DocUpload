using System;
using System.Threading.Tasks;
using DocumentUpload.Core;

namespace DocumentUpload.Services.Generators
{
	internal class PdfDescriptionGenerator : DescriptionGeneratorBase
	{
		private const int BytesPerKb = 1024;

		public override DocumentType Type => DocumentType.PDF;

		protected override ValueTask<string> GenerateDescription(ReadOnlyMemory<byte> fileContent)
		{
			var sizeInKb = fileContent.Length / BytesPerKb;
			var result = $"This is a PDF File ({sizeInKb:N}kb)";
			return new ValueTask<string>(result);
		}
	}
}