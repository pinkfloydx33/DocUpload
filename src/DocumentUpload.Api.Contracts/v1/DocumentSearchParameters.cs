using System;
using System.ComponentModel.DataAnnotations;

namespace DocumentUpload.Api.Contracts.v1
{
	public class DocumentSearchParameters
	{
		public const int MaxSize = 50;

		private int _size = 10;

		[Range(1, int.MaxValue)]
		public int PageNumber { get; set; } = 1;

		[Range(1, MaxSize)]
		public int PageSize
		{
			get => _size;
			set => _size = value > MaxSize ? MaxSize : value;
		}

		public DocumentType[] DocumentTypes { get; set; } = Array.Empty<DocumentType>();
	}
}