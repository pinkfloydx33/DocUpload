using System.Collections.Generic;

namespace DocumentUpload.Core.Data
{
	public struct DocumentDetailList
	{
		public long Total { get; set; }
		public List<DocumentDetailsDto> Documents { get; set; }
        public long Returned => Documents?.Count ?? 0;
    }
}