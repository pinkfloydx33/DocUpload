namespace DocumentUpload.Core.Services
{
	/// <summary>
	/// Gets Metadata about a file based on its name
	/// </summary>
	public interface IFileTypeInfoProvider
	{
		string GetMimeType(string fileName);
		DocumentType GetDocumentType(string fileName);
	}
}