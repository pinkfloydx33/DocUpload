using System;
using System.Collections.Generic;
using System.IO;
using DocumentUpload.Core.Services;
using Microsoft.Extensions.Options;

namespace DocumentUpload.Services.Files
{
	public class FileValidator : IFileValidator
	{
		public FileValidator(IOptions<FileValidationOptions> options)
		{

			if (options is null)
				throw new ArgumentNullException(nameof(options));

			MaxSize = options.Value.MaxSize;
			SupportedExtensions = new HashSet<string>(options.Value.Extensions ?? Array.Empty<string>(),
				StringComparer.OrdinalIgnoreCase);
		}

		public int MaxSize { get; }

		public ICollection<string> SupportedExtensions { get; }

		public bool IsValid(string fileName, byte[] content, out string errorMessage)
		{

			if (content is null || content.Length == 0)
			{
                errorMessage = "File has no content";
				return false;
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
                errorMessage = "File name is required";
				return false;
			}

			if (content.Length > MaxSize)
			{
                errorMessage = $"File is larger than {MaxSize} bytes";
				return false;
			}

			var extension = Path.GetExtension(fileName.AsSpan()).TrimStart('.').ToString();
			if (!SupportedExtensions.Contains(extension))
			{
                errorMessage = $"File extension '{extension}' is not supported";
				return false;
			}

            errorMessage = default;
			return true;

		}
	}
}
