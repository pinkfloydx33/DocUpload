using DocumentUpload.Api.Contracts.v1;
using DocumentTypeV1 = DocumentUpload.Api.Contracts.v1.DocumentType;
using DocumentType = DocumentUpload.Core.DocumentType;

namespace DocumentUpload.Api.Utilities
{
	public static class Mapper
	{
		public static DocumentType Map(DocumentTypeV1 flagType)
		{
			return flagType switch
			{
				DocumentTypeV1.Unknown => DocumentType.Unknown,
				DocumentTypeV1.Image => DocumentType.Image,
				DocumentTypeV1.PDF => DocumentType.PDF,
				DocumentTypeV1.Text => DocumentType.Text,
				_ => DocumentType.Unknown
			};
		}

		public static DocumentTypeV1 Map(DocumentType type)
		{
			return type switch
			{
				DocumentType.Unknown => DocumentTypeV1.Unknown,
				DocumentType.Image => DocumentTypeV1.Image,
				DocumentType.PDF => DocumentTypeV1.PDF,
				DocumentType.Text => DocumentTypeV1.Text,
				_ => DocumentTypeV1.Unknown
			};
		}


		public static DocumentDetails Map(Core.Data.DocumentDetailsDto details)
		{
			var output = new DocumentDetails
			{
				FileName = details.FileName,
				Description = details.Description,
				Title = details.Title,
				DocumentId = details.DocumentId,
				FileSize = details.FileSize,
				Owner = details.Owner,
				CreateDate = details.CreateDate,
				DocumentType = Map(details.DocumentType)
			};

			return output;
		}

	}
}