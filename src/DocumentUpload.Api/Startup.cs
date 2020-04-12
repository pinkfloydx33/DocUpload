using DocumentUpload.Api.Swagger;
using DocumentUpload.Core.Data;
using DocumentUpload.Core.Services;
using DocumentUpload.Services.Data;
using DocumentUpload.Services.Files;
using DocumentUpload.Services.Generators;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using static DocumentUpload.Api.Constants.Configuration;

namespace DocumentUpload.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                     // required for Swagger
                    .AddNewtonsoftJson(opts =>
                     {
                         opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
                     });


			// add swagger capabilities
            services.AddApiVersioning(opts =>
                     {
                         opts.DefaultApiVersion = ApiVersion.Default;
                         opts.AssumeDefaultVersionWhenUnspecified = true;
                         opts.ReportApiVersions = true;
                     })
                    .AddVersionedApiExplorer(opts =>
                     {
                         opts.GroupNameFormat = "'v'VVV";
                         opts.SubstituteApiVersionInUrl = true;
                     })
                    .AddSwaggerGen()
                    .ConfigureOptions<ConfigureSwaggerOptions>();

			// configure our document services
			services.Configure<DataContextOptions>(s =>
				s.ConnectionString = Configuration.GetConnectionString(StorageConnectionString)
			);
			services.Configure<FileValidationOptions>(Configuration.GetSection(FileConstraintsSection));

			// from the Static Files package
			services.AddSingleton<IFileTypeInfoProvider>(_ =>
				new FileTypeInfoProvider(new FileExtensionContentTypeProvider().TryGetContentType)
			);

			services.AddScoped<IDocumentRepository, DocumentRepository>();
			services.AddSingleton<IDescriptionGeneratorFactory, DescriptionGeneratorFactory>();
			services.AddSingleton<IFileValidator, FileValidator>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		[UsedImplicitly]
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseSwagger()
				.UseSwaggerUI();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}

}
