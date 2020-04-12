using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace DocumentUpload.Api.Swagger
{
	public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>, IConfigureOptions<SwaggerUIOptions>
	{
		private const string SwaggerDocName = "swagger.json";
		private const string RoutePrefix = "swagger";

		private readonly IApiVersionDescriptionProvider _provider;
		private readonly string _appName;

		public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IHostEnvironment environment)
		{
			if (environment is null)
				throw new ArgumentNullException(nameof(environment));

			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
			_appName = environment.ApplicationName;

		}

		public void Configure(SwaggerGenOptions options)
		{
			options.EnableAnnotations();
			options.SchemaFilter<EnumTypeSchemaFilter>();
			options.OperationFilter<ResponseHeaderFilter>();

			foreach (var description in _provider.ApiVersionDescriptions)
			{
				options.SwaggerDoc(description.GroupName, CreateApiInfo(description));
			}
		}

		public void Configure(SwaggerUIOptions opts)
		{
			opts.DocumentTitle = $"{_appName} API Documentation";
			opts.RoutePrefix = RoutePrefix;

			foreach (var description in _provider.ApiVersionDescriptions)
			{
				opts.SwaggerEndpoint($"../{opts.RoutePrefix}/{description.GroupName}/{SwaggerDocName}",
					description.GroupName.ToUpperInvariant());
			}
		}

		private OpenApiInfo CreateApiInfo(ApiVersionDescription description)
		{
			var info = new OpenApiInfo
			{
				Title = _appName,
				Version = description.ApiVersion.ToString(),
				Description = _appName
			};

			if (description.IsDeprecated)
			{
				info.Description += " This API version is deprecated.";
			}


			return info;
		}
	}
}