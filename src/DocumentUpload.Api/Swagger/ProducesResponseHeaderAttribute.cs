using System;

namespace DocumentUpload.Api.Swagger
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	internal sealed class ProducesResponseHeaderAttribute : Attribute
	{
		public ProducesResponseHeaderAttribute(string headerName, int statusCode)
		{
			HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
			StatusCode = statusCode;
		}

		public ResponseHeaderType Type { get; set; } = ResponseHeaderType.String;
		public string HeaderName { get; set; }
		public int StatusCode { get; set; }
		public string Description { get; set; }

	}
}