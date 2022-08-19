using Microsoft.Extensions.FileProviders;

namespace Nop.Services.Media.RoxyFileman
{
    public interface IRoxyFilemanFileProvider : IFileProvider
    {
        /// <summary>
        /// Returns the extension of the specified path string
        /// </summary>
        /// <param name="filePath">The path string from which to get the extension</param>
        /// <returns>The extension of the specified path</returns>
        string GetFileNameWithoutExtension(string filePath);
    }
}