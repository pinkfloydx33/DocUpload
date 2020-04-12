using System;
using System.Threading.Tasks;
using DocumentUpload.Core;
using DocumentUpload.Core.Services;

namespace DocumentUpload.Services.Generators
{
	public abstract class DescriptionGeneratorBase : IDescriptionGenerator
	{
		public abstract DocumentType Type { get; }

		public ValueTask<string> GetDescriptionAsync(ReadOnlyMemory<byte> fileContent)
		{

			if (fileContent.IsEmpty)
                throw new ArgumentException("File must contain data", nameof(fileContent));

            return Impl();

            async ValueTask<string> Impl()
            {
                try
                {
                    return await GenerateDescription(fileContent);
                }
                catch
                {
                    return $"{Type:G} file with size {fileContent.Length} bytes";
                }
            }

        }

		protected abstract ValueTask<string> GenerateDescription(ReadOnlyMemory<byte> fileContent);

	}
}