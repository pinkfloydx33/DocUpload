using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DocumentUpload.Core;

namespace DocumentUpload.Services.Generators
{
	internal class ImageDescriptionGenerator : DescriptionGeneratorBase
	{
		public override DocumentType Type => DocumentType.Image;

		protected override unsafe ValueTask<string> GenerateDescription(ReadOnlyMemory<byte> fileContent)
		{

			fixed (byte* pBuffer = &fileContent.Span[0])
			{
				using var ms = new UnmanagedMemoryStream(pBuffer, fileContent.Length);
				using var img = Image.FromStream(ms);

				var result = $"A {img.Height}x{img.Width} {img.RawFormat} image";

				return new ValueTask<string>(result);
			}

		}
	}
}