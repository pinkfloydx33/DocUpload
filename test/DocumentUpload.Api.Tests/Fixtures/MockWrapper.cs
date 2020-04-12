using System;
using Moq;
// ReSharper disable UnusedMember.Global

namespace DocumentUpload.Api.Tests.Fixtures 
{
    public class MockWrapper<T> : IDisposable where T : class
    {
        public MockWrapper() => Mock = new Mock<T>(MockBehavior.Loose);
        public MockWrapper(Mock<T> mock) => Mock = mock;
        
        public Mock<T> Mock { get; }

        public Mock<T> GetAndReset()
        {
            Mock.Reset();
            return Mock;
        }

        public void Dispose() => Mock.VerifyAll();
   
    }
}
