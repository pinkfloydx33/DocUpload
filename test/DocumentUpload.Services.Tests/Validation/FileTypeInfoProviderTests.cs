using System;
using DocumentUpload.Core;
using DocumentUpload.Services.Files;
using Moq;
using Xunit;

namespace DocumentUpload.Services.Tests.Validation
{
	public class FileTypeInfoProviderTests
	{
		[Fact]
		public void Constructor_Throws_OnNullDelegate()
		{
			Assert.Throws<ArgumentNullException>(() => new FileTypeInfoProvider(null));
		}

		private delegate void OutWrapperDelegate(string name, out string output);

		[Fact]
		public void GetMimeType_InvokesDelegate_ReturnsResult_WhenTrue()
		{
			const string expected = "result";

			var mockDelegate = new Mock<FileTypeInfoProvider.TryGetMimeDelegate>();
            mockDelegate.Setup(c => c.Invoke("dummy.txt", out It.Ref<string>.IsAny))
                        .Callback(new OutWrapperDelegate((string name, out string type) =>
                         {
                             type = expected;
                         }))
                        .Returns(true)
                        .Verifiable();

			var provider = new FileTypeInfoProvider(mockDelegate.Object);

			var result = provider.GetMimeType("dummy.txt");

			Assert.Equal(expected, result);

			mockDelegate.Verify(c => c("dummy.txt", out It.Ref<string>.IsAny), Times.Once);
				
		}

		[Fact]
		public void GetMimeType_InvokesDelegate_ReturnsApplicationOctet_WhenFalse()
		{

			var mockDelegate = new Mock<FileTypeInfoProvider.TryGetMimeDelegate>();
			mockDelegate.Setup(c => c.Invoke("dummy.txt", out It.Ref<string>.IsAny))
				.Returns(false)
				.Verifiable();

			var provider = new FileTypeInfoProvider(mockDelegate.Object);

			var resultMime = provider.GetMimeType("dummy.txt");
			const string expected = System.Net.Mime.MediaTypeNames.Application.Octet;

			Assert.Equal(expected, resultMime);

			mockDelegate.Verify(c => c("dummy.txt", out It.Ref<string>.IsAny), Times.Once);
				
		}

		[Fact]
		public void GetMimeType_Throws_OnNullOrEmpty()
		{
			var mockDelegate = new Mock<FileTypeInfoProvider.TryGetMimeDelegate>();
			var provider = new FileTypeInfoProvider(mockDelegate.Object);


			Assert.Throws<ArgumentNullException>(() => provider.GetMimeType(null));
			Assert.Throws<ArgumentException>(() => provider.GetMimeType(""));
			Assert.Throws<ArgumentException>(() => provider.GetMimeType(" "));
		}


		[Theory]
		[InlineData("file.txt", DocumentType.Text)]
		[InlineData("file.TXT", DocumentType.Text)]
		[InlineData("file.jpg", DocumentType.Image)]
		[InlineData("file.PnG", DocumentType.Image)]
		[InlineData(".PnG", DocumentType.Image)]
		[InlineData("file.pdf", DocumentType.PDF)]
		[InlineData("file.pDf", DocumentType.PDF)]
		[InlineData(".", DocumentType.Unknown)]
		[InlineData("file.", DocumentType.Unknown)]
		[InlineData("file", DocumentType.Unknown)]
		public void GetDocumentType_ReturnsExpectedResults(string name, DocumentType expected)
		{
			var mockDelegate = new Mock<FileTypeInfoProvider.TryGetMimeDelegate>();
			var provider = new FileTypeInfoProvider(mockDelegate.Object);

			var result = provider.GetDocumentType(name);
			
			Assert.Equal(expected, result);
		}

		[Fact]
		public void GetDocumentType_Throws_OnNullOrEmpty()
		{
			var mockDelegate = new Mock<FileTypeInfoProvider.TryGetMimeDelegate>();
			var provider = new FileTypeInfoProvider(mockDelegate.Object);


			Assert.Throws<ArgumentNullException>(() => provider.GetDocumentType(null));
			Assert.Throws<ArgumentException>(() => provider.GetDocumentType(""));
			Assert.Throws<ArgumentException>(() => provider.GetDocumentType(" "));
		}
	}
}
