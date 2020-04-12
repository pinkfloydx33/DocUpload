using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentUpload.Api.Utilities;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DocumentUpload.Api.Tests.Utilities
{
	public class ExtensionsTests
	{
        [Fact]
        public async Task GetFileBytesAsync_ThrowsOnNullFile()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((IFormFile) null).GetFileBytesAsync());
        }

        [Fact]
        public async Task GetFileBytesAsync_ReturnsByteArray()
        {
            var inBytes = Encoding.UTF8.GetBytes("Hello World");
            var backingStream = new MemoryStream(inBytes);

            var mockFile = new Mock<IFormFile>();
            mockFile.SetupGet(c => c.Length)
                    .Returns(backingStream.Length);

            mockFile.Setup(c => c.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Callback<Stream, CancellationToken>((target, cancellationToken) =>
                     {
                         backingStream.CopyTo(target);
                     })
                    .Returns<Stream, CancellationToken>((target, cancellationToken) => Task.CompletedTask);

            var outBytes = await mockFile.Object.GetFileBytesAsync();

            Assert.NotNull(outBytes);
            Assert.Equal(inBytes.Length, outBytes.Length);
            Assert.Equal(inBytes, outBytes);

        }

        [Fact]
        public void ToLinkHeaderValues_ThrowsOnNullBuilder()
        {
            Assert.Throws<ArgumentNullException>(() => ((PaginatedLinkBuilder) null).ToLinkHeaderValues());
        }
	}
}
