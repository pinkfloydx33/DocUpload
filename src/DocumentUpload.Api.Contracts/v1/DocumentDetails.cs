using System;

namespace DocumentUpload.Api.Contracts.v1
{
	public class DocumentDetails
	{
		public long DocumentId { get; set; }
		public string FileName { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public long FileSize { get; set; }
		public DateTimeOffset CreateDate { get; set; }
		public string Owner { get; set; }
		public DocumentType DocumentType { get; set; }

	}
}