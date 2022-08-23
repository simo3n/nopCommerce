using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Nop.Core.Infrastructure;

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

        IEnumerable<RoxyImageInfo> GetFiles(string directoryPath = "", string type = "");

        /// <summary>
        /// Moves a file or a directory and its contents to a new location
        /// </summary>
        /// <param name="sourceDirName">The path of the file or directory to move</param>
        /// <param name="destDirName">
        /// The path to the new location for sourceDirName. If sourceDirName is a file, then destDirName
        /// must also be a file name
        /// </param>
        void DirectoryMove(string sourceDirName, string destDirName);
    }
}