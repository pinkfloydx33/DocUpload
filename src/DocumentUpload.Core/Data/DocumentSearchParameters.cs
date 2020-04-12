using System.Collections.Generic;

namespace DocumentUpload.Core.Data
{
	public class DocumentSearchParameters
	{
		public int? PageSize { get; set; }
		public int? Page { get; set; } 
		public List<DocumentType> DocumentTypes { get; set; }

	}
}