using System;
using System.IO;
using DocumentUpload.Core;
using DocumentUpload.Core.Services;

namespace DocumentUpload.Services.Files
{
	public class FileTypeInfoProvider : IFileTypeInfoProvider
	{
		public delegate bool TryGetMimeDelegate(string fileName, out string mimeType);

		private const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

		private const string PdfExtension = "pdf",
			TextExtension = "txt",
			JpegExtesion = "jpg",
			PngExtension = "png";


		private readonly TryGetMimeDelegate _mimeFactory;

		public FileTypeInfoProvider(TryGetMimeDelegate mimeFactory)
		{
			_mimeFactory = mimeFactory ?? throw new ArgumentNullException(nameof(mimeFactory));
		}

		public string GetMimeType(string fileName)
		{
			if (fileName is null)
				throw new ArgumentNullException(nameof(fileName));

			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentException("Filename is required", nameof(fileName));

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (_mimeFactory(fileName, out var mimeType))
				return mimeType;

			return System.Net.Mime.MediaTypeNames.Application.Octet;
		}

		public DocumentType GetDocumentType(string fileName)
		{
			if (fileName is null)
				throw new ArgumentNullException(nameof(fileName));

			var asSpan = fileName.AsSpan();
			if (asSpan.IsWhiteSpace())
				throw new ArgumentException("Filename is required", nameof(fileName));

			// Dictionary lookup probably better... but for four types not worried about it
			// Span<> can't be put in Dictionary anyways
			var ext = Path.GetExtension(asSpan).TrimStart('.');
			if (ext.Equals(PdfExtension, Comparison))
				return DocumentType.PDF;

			if (ext.Equals(TextExtension, Comparison))
				return DocumentType.Text;

			if (ext.Equals(PngExtension, Comparison) || ext.Equals(JpegExtesion, Comparison))
				return DocumentType.Image;

			return DocumentType.Unknown;

		}
	}
}