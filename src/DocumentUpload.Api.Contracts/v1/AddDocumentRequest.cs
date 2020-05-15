using System.ComponentModel.DataAnnotations;

namespace DocumentUpload.Api.Contracts.v1
{
	public class AddDocumentRequest
	{
		
		[Required(AllowEmptyStrings = false)] 
		[StringLength(512, MinimumLength = 1)]
		[RegularExpression(".*\\S.*", ErrorMessage = "Must contain at least one non-whitespace character")]
		public string Title { get; set; }

		[Required(AllowEmptyStrings = false)]
		[EmailAddress]
		[MaxLength(512)]
		public string Owner { get; set; }

		[MaxLength(1024)]
		public string Description { get; set; }
	}
}