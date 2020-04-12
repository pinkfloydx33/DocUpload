using System.Collections.Generic;
using JetBrains.Annotations;

namespace DocumentUpload.Core.Services
{
    /// <summary>
    /// Verifies whether a file is supported by the system
    /// </summary>
    [PublicAPI]
    public interface IFileValidator
    {
        /// <summary>
        /// Maximum allowed file size in bytes
        /// </summary>
        int MaxSize { get; }

        /// <summary>
        /// Supported list of File Extensions
        /// </summary>
        ICollection<string> SupportedExtensions { get; }

        /// <summary>
        /// Determines whether a file and its content are valid
        /// </summary>
        /// <param name="fileName">name of the file</param>
        /// <param name="content">file content in bytes</param>
        /// <param name="errorMessage">validation failure message</param>
        /// <returns><c>true</c> if file is valid; <c>false</c> otherwise</returns>
        bool IsValid(string fileName, byte[] content, out string errorMessage);
    }
}