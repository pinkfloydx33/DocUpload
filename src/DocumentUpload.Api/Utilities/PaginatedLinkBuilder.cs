using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DocumentUpload.Api.Utilities
{
	internal sealed class PaginatedLinkBuilder
	{
		public PaginatedLinkBuilder(IUrlHelper urlHelper, string actionName, object routeValues, long pageNo, long pageSize, long recordCount)
        {
            if (urlHelper is null)
				throw new ArgumentNullException(nameof(urlHelper));

            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException(nameof(actionName));
            
            if (pageNo <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNo));

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize));
			
			if (recordCount < 0)
				throw new ArgumentOutOfRangeException(nameof(recordCount));

            const string keyPageNumber = "pageNumber";
			const string keyPageSize = "pageSize";

            var pageCount = recordCount == 0 ? 1 : (long) Math.Ceiling(recordCount / (double) pageSize);

            var dict = new RouteValueDictionary(routeValues)
			{
				[keyPageSize] = pageSize
			};

			Uri Create(long thisPage)
			{
				dict[keyPageNumber] = thisPage;
                var link = urlHelper.ActionLink(actionName, values: dict);
				return new Uri(link);
			}

			FirstPage = Create(1);
			LastPage = Create(pageCount);

			if (pageNo > 1)
				PreviousPage = Create(pageNo - 1);

			if (pageNo < pageCount)
				NextPage = Create(pageNo + 1);

		}

		public Uri FirstPage { get; }
		public Uri LastPage { get; }
		public Uri NextPage { get; }
		public Uri PreviousPage { get; }

	}
}