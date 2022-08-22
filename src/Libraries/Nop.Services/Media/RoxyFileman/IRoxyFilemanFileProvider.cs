using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Nop.Services.Media.RoxyFileman
{
    public interface IRoxyFilemanFileProvider : IFileProvider
    {
        /// <summary>
        /// Create configuration file for RoxyFileman
        /// </summary>
        /// <param name="pathBase"></param>
        /// <param name="lang"></param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<RoxyFilemanConfig> CreateConfigurationAsync(string pathBase, string lang);

        void CopyDirectory(string sourcePath, string destinationPath);

        IEnumerable<(string relativePath, int countFiles, int countDirectories)> GetDirectories(string type, bool isRecursive = true, string rootDirectoryPath = "");
    }
}