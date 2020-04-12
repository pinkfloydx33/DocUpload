using System.Collections.Generic;
using DocumentUpload.Core;
using DocumentUpload.Core.Services;

namespace DocumentUpload.Services.Generators
{
	public class DescriptionGeneratorFactory : IDescriptionGeneratorFactory
	{
		private static readonly Dictionary<DocumentType, IDescriptionGenerator> Lookup =
			new Dictionary<DocumentType, IDescriptionGenerator>
			{
				[DocumentType.Image] = new ImageDescriptionGenerator(),
				[DocumentType.Text] = new TextDescriptionGenerator(),
				[DocumentType.PDF] = new PdfDescriptionGenerator()
			};

		public IDescriptionGenerator GetGenerator(DocumentType type)
		{
			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (Lookup.TryGetValue(type, out var generator))
				return generator;

			return UnknownDescriptionGenerator.Instance;
		}
	}
}