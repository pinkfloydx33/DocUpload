using System;
using System.Threading.Tasks;

namespace DocumentUpload.Core.Services
{
	/// <summary>
	/// Generates Descriptions for a file of a specific <see cref="DocumentType"/>
	/// </summary>
	public interface IDescriptionGenerator
	{
        ValueTask<string> GetDescriptionAsync(ReadOnlyMemory<byte> fileContent);
		DocumentType Type { get; }
	}
}