using System;
using DocumentUpload.Core.Data;
using DocumentUpload.Core.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace DocumentUpload.Api.Tests.Fixtures
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private readonly MockWrapper<IDocumentRepository> _docRepoMock = new MockWrapper<IDocumentRepository>();
        private readonly MockWrapper<IFileValidator> _fileValidatorMock = new MockWrapper<IFileValidator>();
        private readonly MockWrapper<IDescriptionGeneratorFactory> _generatorMock = new MockWrapper<IDescriptionGeneratorFactory>();
        private readonly MockWrapper<IFileTypeInfoProvider> _typeInfoMock = new MockWrapper<IFileTypeInfoProvider>();

        public CustomWebApplicationFactory()
        {
            ClientOptions.BaseAddress = new Uri("http://localhost/api/v1/");
        }

        // will automatically reset when accessed via Property
        public Mock<IFileValidator> FileValidatorMock => _fileValidatorMock.GetAndReset();
        public Mock<IDocumentRepository> DocumentRepoMock => _docRepoMock.GetAndReset();
        public Mock<IFileTypeInfoProvider> TypeInfoMock => _typeInfoMock.GetAndReset();
        

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder
                .ConfigureServices(
                    services =>
                    {
                        services.AddSingleton(_fileValidatorMock.Mock.Object);
                        services.AddSingleton(_docRepoMock.Mock.Object);
                        services.AddSingleton(_generatorMock.Mock.Object);
                        services.AddSingleton(_typeInfoMock.Mock.Object);
                    });

            var testHost = base.CreateHost(builder);

            return testHost;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // calls VerifyAll()
                _fileValidatorMock.Dispose();
                _docRepoMock.Dispose();
                _generatorMock.Dispose();
                _typeInfoMock.Dispose();
                
            }

            base.Dispose(disposing);
        }
    }
}
