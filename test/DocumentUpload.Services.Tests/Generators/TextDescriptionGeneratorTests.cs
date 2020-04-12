using System;
using System.Linq;
using System.Threading.Tasks;
using DocumentUpload.Services.Generators;
using Xunit;

namespace DocumentUpload.Services.Tests.Generators
{
	public class TextDescriptionGeneratorTests
	{
		[Fact]
		public async Task GetDescription_FileWithNonWhitespace_ReturnsFirstLineOfText()
		{
			var generator = new TextDescriptionGenerator();

            var bytes = System.Text.Encoding.UTF8.GetBytes("Hello\nWorld");
            var result = await generator.GetDescriptionAsync(bytes);

			Assert.Equal("Text file beginning with 'Hello'", result);
		}

		[Fact]
		public async Task GetDescription_FileWithNonWhitespace_ReturnsFirstLineOfText_UpToTwentyLetters()
		{
			var generator = new TextDescriptionGenerator();

            var bytes = System.Text.Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ\nWorld");

			var result = await generator.GetDescriptionAsync(bytes);

			Assert.Equal("Text file beginning with 'ABCDEFGHIJKLMNOPQRST'", result);
		}

		[Fact]
		public async Task GetDescription_FileWithNonWhitespace_ReturnsSecondLineOfText_WhenFirstIsBlank()
		{
			var generator = new TextDescriptionGenerator();

            var bytes = System.Text.Encoding.UTF8.GetBytes("\n\r\nWorld");

			var result = await generator.GetDescriptionAsync(bytes);

			Assert.Equal("Text file beginning with 'World'", result);
		}

		[Fact]
		public async Task GetDescription_FileWithNonWhitespace_ReturnsSecondLineOfText_WhenFirstIsWhitespaceOnly()
		{
			var generator = new TextDescriptionGenerator();
            var bytes = System.Text.Encoding.UTF8.GetBytes("  \r\nWorld");

			var result = await generator.GetDescriptionAsync(bytes);

			Assert.Equal("Text file beginning with 'World'", result);
		}


        [Fact]
        public async Task GetDescription_HugeFileMadeOfWhitespace_ButWithTextAtEnd_ReturnsThatText()
        {
            var generator = new TextDescriptionGenerator();

            var text = string.Concat(Enumerable.Repeat("\r\n", 4096)) + "        Hello     \r\nWorld";
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);

            var result = await generator.GetDescriptionAsync(bytes);

            Assert.Equal("Text file beginning with 'Hello'", result);
        }

		[Fact]
		public async Task GetDescription_FileWithWhitespaceOnly_ReturnsGenericDescription()
		{
			var generator = new TextDescriptionGenerator();
            
            var bytes = System.Text.Encoding.UTF8.GetBytes("  \r\n   ");

			var result = await generator.GetDescriptionAsync(bytes);

			Assert.Equal("Empty Text File", result);
		}


		[Fact]
		public async Task GetDescription_Throws_WhenContentIsNullOrEmpty()
		{
			var generator = new TextDescriptionGenerator();

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => generator.GetDescriptionAsync(null).AsTask()
            );

			Assert.Contains("must contain data", ex.Message);

            ex = await Assert.ThrowsAsync<ArgumentException>(
                () => generator.GetDescriptionAsync(Array.Empty<byte>()).AsTask()
            );

			Assert.Contains("must contain data", ex.Message);
		}
	}
}