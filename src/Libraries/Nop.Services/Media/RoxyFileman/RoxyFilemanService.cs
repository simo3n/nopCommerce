using System.Linq;
using System.Threading.Tasks;

namespace Nop.Services.Media.RoxyFileman
{
    public class RoxyFilemanService : IRoxyFilemanService
    {

        #region Fields

        private readonly IRoxyFilemanFileProvider _fileProvider;

        #endregion

        #region Ctor

        public RoxyFilemanService(IRoxyFilemanFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utils

        /// <summary>
        /// Check whether there are any restrictions on handling the file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue if the file can be handled; otherwise false
        /// </returns>
        protected virtual async Task<bool> CanHandleFileAsync(string path)
        {
            var fileExtension = _fileProvider.GetFileNameWithoutExtension(path).ToLowerInvariant();

            return !NopRoxyFilemanDefaults.ForbiddenUploadExtensions.Contains(fileExtension);
        }

        #endregion

        public Task ConfigureAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task CopyDirectoryAsync(string sourcePath, string destinationPath)
        {
            throw new System.NotImplementedException();
        }

        public Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateConfigurationAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task CreateDirectoryAsync(string parentDirectoryPath, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateImageThumbnailAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteDirectoryAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteFileAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task DownloadDirectoryAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task DownloadFileAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task FlushAllImagesOnDiskAsync(bool removeOriginal = true)
        {
            throw new System.NotImplementedException();
        }

        public Task FlushImagesOnDiskAsync(string directoryPath)
        {
            throw new System.NotImplementedException();
        }

        public string GetConfigurationFilePath()
        {
            throw new System.NotImplementedException();
        }

        public Task GetDirectoriesAsync(string type)
        {
            throw new System.NotImplementedException();
        }

        public string GetErrorResponse(string message = null)
        {
            throw new System.NotImplementedException();
        }

        public Task GetFilesAsync(string directoryPath, string type)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetLanguageResourceAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        public bool IsAjaxRequest()
        {
            throw new System.NotImplementedException();
        }

        public Task MoveDirectoryAsync(string sourcePath, string destinationPath)
        {
            throw new System.NotImplementedException();
        }

        public Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            throw new System.NotImplementedException();
        }

        public Task RenameDirectoryAsync(string sourcePath, string newName)
        {
            throw new System.NotImplementedException();
        }

        public Task RenameFileAsync(string sourcePath, string newName)
        {
            throw new System.NotImplementedException();
        }

        public Task UploadFilesAsync(string directoryPath)
        {
            throw new System.NotImplementedException();
        }
    }
}