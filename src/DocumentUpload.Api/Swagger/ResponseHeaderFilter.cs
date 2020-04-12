using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentUpload.Api.Swagger
{
	internal sealed class ResponseHeaderFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var attributes = context.ApiDescription.CustomAttributes()
				.OfType<ProducesResponseHeaderAttribute>()
				.ToArray();

			if (attributes.Length == 0 || operation.Responses.Keys.Count == 0)
				return;

			foreach (var status in operation.Responses.Keys)
			{

				var responseHeaders = attributes.Where(r => r.StatusCode.ToString() == status).ToArray();
				if (responseHeaders.Length == 0)
					continue;

				var response = operation.Responses[status];

				response.Headers ??= new Dictionary<string, OpenApiHeader>();

				foreach (var responseHeader in responseHeaders)
				{
					response.Headers[responseHeader.HeaderName] = CreateHeader(responseHeader);
				}
			}
		}

		private static OpenApiHeader CreateHeader(ProducesResponseHeaderAttribute responseHeader)
		{
			var schema = new OpenApiSchema();

			if (responseHeader.Type == ResponseHeaderType.Numeric)
			{
				schema.Format = SwaggerConstants.FormatInt32;
				schema.Type = SwaggerConstants.TypeNumber;
			}
			else
			{
				schema.Format = SwaggerConstants.FormatString;
				schema.Type = SwaggerConstants.TypeString;
			}

			return new OpenApiHeader
			{
				Description = responseHeader.Description,
				Schema = schema
			};
		}

	}
}