using System;
using DocumentUpload.Services.Files;
using Microsoft.Extensions.Options;
using Xunit;

namespace DocumentUpload.Services.Tests.Validation
{
	public class FileValidatorTests
	{

		private readonly FileValidator _validator;

		public FileValidatorTests()
        {
            var opts = new FileValidationOptions
            {
                Extensions = new[] { "txt", "jpg" },
                MaxSize = 100
            };

			_validator = new FileValidator(Options.Create(opts));
		}

		[Fact]
		public void Constructor_Throws_OnNullOptions()
		{
			Assert.Throws<ArgumentNullException>(() => new FileValidator(null));
		}


		[Fact]
		public void Constructor_DoesNotThrow_OnEmptyExtensions()
		{
			var opts = new FileValidationOptions();
			var val = new FileValidator(Options.Create(opts));
			
			Assert.NotNull(val.SupportedExtensions);
			Assert.Empty(val.SupportedExtensions);
		}

		[Theory]
		[InlineData(null, "test.txt", false, "no content")]
		[InlineData(0, "test.txt", false, "no content")]
		[InlineData(101, "test.txt", false, "is larger than")]
		[InlineData(1, "test.xml", false, "is not supported")]
		[InlineData(1, "test.XML", false, "is not supported")]
		[InlineData(1, "test", false, "is not supported")]
		[InlineData(15, "", false, "name is required")]
		[InlineData(15, "   ", false, "name is required")]
		[InlineData(15, null, false, "name is required")]
		[InlineData(100, "test.txt", true, null)]
		[InlineData(1, "test.jpg", true, null)]
		[InlineData(1, "test.JPG", true, null)]
		public void IsValid_ReturnsExpected(int? len, string name, bool expected, string expectedMessage)
		{
			byte[] buffer = null;
			if (len.HasValue)
				buffer = new byte[len.Value];

			var result = _validator.IsValid(name, buffer, out var message);

			Assert.Equal(expected, result);
			if (expectedMessage is null)
				Assert.Null(message);
			else
				Assert.Contains(expectedMessage, message);
		}
	}
}