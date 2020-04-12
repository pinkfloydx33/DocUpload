using System;
using System.Threading.Tasks;
using DocumentUpload.Core;
using DocumentUpload.Services.Generators;
using Moq;
using Xunit;

namespace DocumentUpload.Services.Tests.Generators
{
	public class DescriptionGeneratorBaseTests
	{
		private readonly Mock<DescriptionGeneratorBase> _generator;

		public DescriptionGeneratorBaseTests()
		{
			_generator = new Mock<DescriptionGeneratorBase> { CallBase = true };
		}

		[Fact]
		public async Task GetDescription_Throws_OnNullFile()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _generator.Object.GetDescriptionAsync(null).AsTask()
            );
        }

		[Fact]
		public async Task GetDescription_Throws_WhenContentIsNullOrEmpty()
		{
			_generator.SetupGet(c => c.Type).Returns(DocumentType.Image);

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _generator.Object.GetDescriptionAsync(null).AsTask()
            );

			Assert.Contains("must contain data", ex.Message);

            ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _generator.Object.GetDescriptionAsync(Array.Empty<byte>()).AsTask()
            );

			Assert.Contains("must contain data", ex.Message);
		}
	}
}
