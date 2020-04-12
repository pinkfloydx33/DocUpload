namespace DocumentUpload.Core.Services
{
	/// <summary>
	/// Gets an <see cref="IDescriptionGeneratorFactory"/> for the specified <see cref="DocumentType"/>
	/// </summary>
	public interface IDescriptionGeneratorFactory
	{
		IDescriptionGenerator GetGenerator(DocumentType type);
	}
}