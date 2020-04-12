using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentUpload.Api.Contracts.v1;
using DocumentUpload.Api.Tests.Fixtures;
using DocumentUpload.Core.Data;
using DocumentUpload.Core.Services;
using Moq;
using Newtonsoft.Json;
using Xunit;
using DocumentType = DocumentUpload.Api.Contracts.v1.DocumentType;

namespace DocumentUpload.Api.Tests.Controllers
{
	public class DocumentsControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly Mock<IDocumentRepository> _docRepoMock;
        private readonly Mock<IFileValidator> _fileValidatorMock;
        private readonly Mock<IFileTypeInfoProvider> _typeInfoMock;

        public DocumentsControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();

            _docRepoMock = factory.DocumentRepoMock;
            _fileValidatorMock = factory.FileValidatorMock;
            _typeInfoMock = factory.TypeInfoMock;
        }


        [Fact]
        public async Task Get_Returns200_WhenFound()
        {
            const int docNumber = 1;
            const string fileName = "myfile.txt";
            const string title = "My Title";

            _docRepoMock.Setup(c => c.GetByIdAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new DocumentDetailsDto
                         {
                             DocumentId = docNumber, FileName = fileName, 
                             Title = title, DocumentType = Core.DocumentType.Text
                         });

            var response = await _client.GetAsync($"documents/{docNumber}");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var document = JsonConvert.DeserializeObject<DocumentDetails>(content);

            Assert.NotNull(document);
            Assert.Equal(docNumber, document.DocumentId);
            Assert.Equal(fileName, document.FileName);
            Assert.Equal(title, document.Title);
            Assert.Equal(DocumentType.Text, document.DocumentType);

        }


        [Fact]
        public async Task Get_Returns404_WhenNotFound()
        {
            const int docNumber = 5;

            _docRepoMock.Setup(c => c.GetByIdAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => null);

            var response = await _client.GetAsync($"documents/{docNumber}");

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task Get_Returns500_OnError()
        {

            _docRepoMock.Setup(c => c.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new InvalidOperationException("Error!"));

            var response = await _client.GetAsync("documents/1");

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }


        [Fact]
        public async Task Download_Returns404_WhenNotFound()
        {
            const int docNumber = 5;


            _docRepoMock.Setup(c => c.GetByIdAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => null);

            var response = await _client.GetAsync($"documents/{docNumber}/download");

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Download_Returns404_IfContentMissing()
        {
            const int docNumber = 5;
            const string fileName = "MyFile.txt";


            _docRepoMock.Setup(c => c.GetByIdAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new DocumentDetailsDto
                         {
                             DocumentId = docNumber, FileName = fileName, 
                             DocumentType = Core.DocumentType.Text
                         });

            // Content Missing
            _docRepoMock.Setup(c => c.GetContentAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => null);

            var response = await _client.GetAsync($"documents/{docNumber}/download");

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task Download_Returns200AndFile_WhenContentFound()
        {
            const int docNumber = 5;
            const string fileName = "MyFile.txt";
            const string fileText = "This is some text";

            _docRepoMock.Setup(c => c.GetByIdAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new DocumentDetailsDto
                         {
                             DocumentId = docNumber, FileName = fileName, 
                             DocumentType = Core.DocumentType.Text
                         });

            _docRepoMock.Setup(c => c.GetContentAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => Encoding.UTF8.GetBytes(fileText));

            _typeInfoMock.Setup(c => c.GetMimeType(fileName))
                         .Returns(System.Net.Mime.MediaTypeNames.Text.Plain);

            var response = await _client.GetAsync($"documents/{docNumber}/download");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // should be plain text
            Assert.Equal(System.Net.Mime.MediaTypeNames.Text.Plain, response.Content.Headers.ContentType.MediaType);

            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            var responseText = Encoding.UTF8.GetString(responseBytes);

            Assert.Equal(fileText, responseText);
        }

        [Fact]
        public async Task Delete_Returns204_WhenDeleteSuccessful()
        {
            const int docNumber = 5;

            _docRepoMock.Setup(c => c.DeleteAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

            var response = await _client.DeleteAsync($"documents/{docNumber}");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Returns404_WhenNotFound()
        {
            const int docNumber = 5;

            _docRepoMock.Setup(c => c.DeleteAsync(docNumber, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

            var response = await _client.DeleteAsync($"documents/{docNumber}");

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_Returns200_WhenNoResultsReturned()
        {
            _docRepoMock.Setup(c => c.ListDocumentsAsync(It.IsAny<Core.Data.DocumentSearchParameters>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => default);

            var response = await _client.GetAsync("documents");
            
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<DocumentDetails[]>(responseContent);
            
            Assert.NotNull(result);
            Assert.Empty(result);

            AssertPaginationHeaders(response.Headers, 0);

        }

        [Fact]
        public async Task GetAll_Returns200_WithResults()
        {
            _docRepoMock.Setup(c => c.ListDocumentsAsync(It.IsAny<Core.Data.DocumentSearchParameters>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => new DocumentDetailList
                         {
                             Total = 1,
                             Documents = new List<DocumentDetailsDto>
                             {
                                 new DocumentDetailsDto { DocumentId = 1 }
                             }
                         });

            var response = await _client.GetAsync("documents");
            
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<DocumentDetails[]>(responseContent);
            
            Assert.NotNull(result);

            var item = Assert.Single(result);

            Assert.NotNull(item);
            Assert.Equal(1, item.DocumentId);

            AssertPaginationHeaders(response.Headers, 1);

        }

        [Fact]
        public async Task GetAll_Returns200_WithPagination()
        {
            _docRepoMock.Setup(c => c.ListDocumentsAsync(It.IsAny<Core.Data.DocumentSearchParameters>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => new DocumentDetailList
                         {
                             Total = 10,
                             Documents = new List<DocumentDetailsDto>
                             {
                                 new DocumentDetailsDto { DocumentId = 3 },
                                 new DocumentDetailsDto { DocumentId = 4 },
                             }
                         });

            var response = await _client.GetAsync("documents?pageNumber=2&pageSize=2");
            
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<DocumentDetails[]>(responseContent);
            
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Collection(
                result,
                s => Assert.Equal(3, s.DocumentId),
                s => Assert.Equal(4, s.DocumentId)
            );

            AssertPaginationHeaders(response.Headers, 10, true, true);
        }

        private static void AssertPaginationHeaders(HttpHeaders headers, int expectedTotal, bool hasNext = false, bool hasPrev = false)
        {
            var paginationHeaders = headers.GetValues("X-Pagination-Count");
            var pHeader = Assert.Single(paginationHeaders);

            Assert.Equal(expectedTotal.ToString(), pHeader);
            
            var linkHeaders = headers.GetValues("Link")?.ToArray();
            Assert.NotNull(linkHeaders);

            var totalCount = 2 + (hasNext ? 1 : 0) + (hasPrev ? 1 : 0);

            Assert.Equal(totalCount, linkHeaders.Length);

            Assert.Contains(linkHeaders, s => s.Contains("rel=\"first\""));
            Assert.Contains(linkHeaders, s => s.Contains("rel=\"last\""));

            if (hasNext)
                Assert.Contains(linkHeaders, s => s.Contains("rel=\"next\""));

            if (hasPrev)
                Assert.Contains(linkHeaders, s => s.Contains("rel=\"prev\""));

        }

        private delegate void DelegateWrapper(string fileName, byte[] content, out string message);

        [Fact]
        public async Task Post_Returns400_ForInvalidFiles()
        {
            _fileValidatorMock.Setup(c => c.IsValid(It.IsAny<string>(), It.IsAny<byte[]>(), out It.Ref<string>.IsAny))
                              .Callback(new DelegateWrapper((string fileName, byte[] content, out string message) =>
                               {
                                   message = "Bad File";
                               }))
                              .Returns(false);


            var formContent = CreateFormWithFile("file.txt", 1024);

            var result = await _client.PostAsync("documents", formContent);

            Assert.False(result.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

            Assert.Equal("Bad File", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_Returns409_WhenTitleInUse()
        {
            _fileValidatorMock.Setup(c => c.IsValid(It.IsAny<string>(), It.IsAny<byte[]>(), out It.Ref<string>.IsAny))
                              .Returns(true);

            _typeInfoMock.Setup(c => c.GetDocumentType("filename.txt"))
                         .Returns(Core.DocumentType.Text);

            // return zero -> TitleInUse (did not insert)
            _docRepoMock.Setup(c => c.InsertAsync(It.IsAny<DocumentDetailsDto>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => 0);

            var formContent = CreateFormWithFile("filename.txt", 1024);
            var result = await _client.PostAsync("documents", formContent);

            Assert.False(result.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Assert.Contains("already exists", await result.Content.ReadAsStringAsync());

        }


        [Fact]
        public async Task Post_Returns201_WhenCreated()
        {
            const int newId = 1234;


            _fileValidatorMock.Setup(c => c.IsValid(It.IsAny<string>(), It.IsAny<byte[]>(), out It.Ref<string>.IsAny))
                              .Returns(true);

            _typeInfoMock.Setup(c => c.GetDocumentType("filename.txt"))
                         .Returns(Core.DocumentType.Text);

            // return new Id
            _docRepoMock.Setup(c => c.InsertAsync(It.IsAny<DocumentDetailsDto>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => newId);

            var formContent = CreateFormWithFile("filename.txt", 1024);
            var result = await _client.PostAsync("documents", formContent);

            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal(newId.ToString(), await result.Content.ReadAsStringAsync());

            // Location Header pointing to new resource
            Assert.Equal(newId.ToString(), result.Headers.Location.Segments.Last());


        }

        private static readonly Random Random = new Random();

        private static MultipartFormDataContent CreateFormWithFile(string fileName, int length)
        {
            var bytes = new byte[length];
            
            Random.NextBytes(bytes);

            var formContent = new MultipartFormDataContent
            {
                { new StringContent("test@test.com"), "Owner" },
                { new StringContent("MyTitle"), "Title"  },
                { new ByteArrayContent(bytes), "file", fileName }
            };

            return formContent;
        }

	}
}
