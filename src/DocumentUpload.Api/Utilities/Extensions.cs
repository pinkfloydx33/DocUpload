using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DocumentUpload.Api.Utilities
{
	internal static class Extensions
	{
		public static StringValues ToLinkHeaderValues(this PaginatedLinkBuilder builder)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			var o = new List<string>(4);

			static void Add(ICollection<string> list, Uri uri, string type)
			{
				if (uri != null)
					list.Add($"<{uri}>; rel=\"{type}\"");
			}

			Add(o, builder.FirstPage, "first");
			Add(o, builder.PreviousPage, "prev");
			Add(o, builder.NextPage, "next");
			Add(o, builder.LastPage, "last");

			return o.ToArray();

		}

        public static Task<byte[]> GetFileBytesAsync(this IFormFile file, CancellationToken token = default)
        {
			if (file is null)
				throw new ArgumentNullException(nameof(file));

            return Impl(file, token);

            static async Task<byte[]> Impl(IFormFile formFile, CancellationToken ct)
            {
                await using var ms = new MemoryStream((int) formFile.Length);
                await formFile.CopyToAsync(ms, ct);
                return ms.ToArray();
            }
        }
	}
}