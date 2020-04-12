using System;
using DocumentUpload.Api.Utilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DocumentUpload.Api.Tests.Utilities
{
    public class PaginatedLinkBuilderTests
    {
        [Fact]
        public void Constructor_Throws_OnInvalidPageSize()
        {
            var uriHelper = new Mock<IUrlHelper>();
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new PaginatedLinkBuilder(uriHelper.Object, "act", null, 1, 0, 1));

            Assert.Equal("pageSize", ex.ParamName);
        }

        [Fact]
        public void Constructor_Throws_OnInvalidPageNumber()
        {
            var uriHelper = new Mock<IUrlHelper>();
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new PaginatedLinkBuilder(uriHelper.Object, "act", null, 0, 1, 1));

            Assert.Equal("pageNo", ex.ParamName);
        }

        [Fact]
        public void Constructor_Throws_OnInvalidRecordCount()
        {
            var uriHelper = new Mock<IUrlHelper>();
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new PaginatedLinkBuilder(uriHelper.Object, "act", null, 1, 1, -1));

            Assert.Equal("recordCount", ex.ParamName);
        }


        [Fact]
        public void Constructor_Throws_OnNullUriHelper()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new PaginatedLinkBuilder(null, "act", null, 1, 1, 1));

            Assert.Equal("urlHelper", ex.ParamName);
        }


        [Fact]
        public void Constructor_Throws_OnEmptyAction()
        {
            var uriHelper = new Mock<IUrlHelper>();
            var ex = Assert.Throws<ArgumentNullException>(() => new PaginatedLinkBuilder(uriHelper.Object, "", null, 1, 1, -1));

            Assert.Equal("actionName", ex.ParamName);
        }
    }
}
