using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;

namespace Nop.Services.Media.RoxyFileman
{
    public class RoxyFilemanService : IRoxyFilemanService
    {

        #region Fields

        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IRoxyFilemanFileProvider _fileProvider;
        protected readonly IWorkContext _workContext;
        protected readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

        public RoxyFilemanService(IHttpContextAccessor httpContextAccessor, IRoxyFilemanFileProvider fileProvider, IWorkContext workContext, MediaSettings mediaSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _fileProvider = fileProvider;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
        }

        #endregion

        #region Utils

        /// <summary>
        /// Check whether there are any restrictions on handling the file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>
        /// The result contains the rue if the file can be handled; otherwise false
        /// </returns>
        protected virtual bool CanHandleFile(string path)
        {
            var result = false;

            var fileExtension = Path.GetExtension(path).Replace(".", string.Empty).ToLowerInvariant();

            var roxyConfig = Singleton<RoxyFilemanConfig>.Instance;

            var forbiddenUploads = roxyConfig.FORBIDDEN_UPLOADS.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(forbiddenUploads))
            {
                var forbiddenFileExtensions = new ArrayList(Regex.Split(forbiddenUploads, "\\s+"));
                result = !forbiddenFileExtensions.Contains(fileExtension);
            }

            var allowedUploads = roxyConfig.ALLOWED_UPLOADS.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(allowedUploads))
                return result;

            var allowedFileExtensions = new ArrayList(Regex.Split(allowedUploads, "\\s+"));
            return allowedFileExtensions.Contains(fileExtension);
        }

        protected virtual HttpResponse GetJsonResponse()
        {
            var response = GetHttpContext().Response;

            response.Headers.TryAdd("Content-Type", "application/json");

            return response;
        }

        /// <summary>
        /// Get the http context
        /// </summary>
        /// <returns>Http context</returns>
        protected virtual HttpContext GetHttpContext()
        {
            return _httpContextAccessor.HttpContext;
        }

        #endregion

        public async Task ConfigureAsync()
        {
            var currentPathBase = _httpContextAccessor.HttpContext.Request.PathBase.ToString();
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();

            await _fileProvider.CreateConfigurationAsync(currentPathBase, currentLanguage.UniqueSeoCode);
        }

        public async Task CopyDirectoryAsync(string sourcePath, string destinationPath)
        {
            var sourceDirInfo = _fileProvider.GetFileInfo(sourcePath);

            if (!sourceDirInfo.Exists || !sourceDirInfo.IsDirectory)
                throw new Exception(await GetLanguageResourceAsync("E_CopyDirInvalidPath"));

            var destinationDirInfo = _fileProvider.GetFileInfo(destinationPath);

            if (!destinationDirInfo.IsDirectory)
                throw new Exception(await GetLanguageResourceAsync("E_CopyDirInvalidPath"));

            if (destinationDirInfo.Exists)
                throw new Exception(await GetLanguageResourceAsync("E_DirAlreadyExists"));

            _fileProvider.CopyDirectory(sourcePath, destinationPath);
        }

        public Task CopyFileAsync(string sourcePath, string destinationPath)
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

        public async Task GetDirectoriesAsync(string type)
        {
            // if (!_fileProvider.DirectoryExists(rootDirectoryPath))
            //     throw new Exception("Invalid files root directory. Check your configuration.");

            var contents = _fileProvider.GetDirectories(type);

            var result = new List<object>();

            foreach (var fName in contents)
            {
                var (path, countFiles, countDirectories) = fName;

                result.Add(new
                {
                    p = path,
                    f = countFiles,
                    d = countDirectories
                });
            }

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(result);
        }

        public string GetErrorResponse(string message = null)
        {
            throw new System.NotImplementedException();
        }

        public async Task GetFilesAsync(string directoryPath, string type)
        {
            var result = _fileProvider.GetFiles(directoryPath, type)
                .Select(f => new
                {
                    p = f.Name,
                    t = f.LastWriteTime.ToUnixTimeSeconds(),
                    s = f.FileLength,
                    w = f.Width,
                    h = f.Height
                });

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(result);
        }

        public Task<string> GetLanguageResourceAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        public bool IsAjaxRequest()
        {
            throw new System.NotImplementedException();
        }

        public async Task MoveDirectoryAsync(string sourcePath, string destinationPath)
        {
            if (destinationPath.IndexOf(sourcePath, StringComparison.InvariantCulture) == 0)
                throw new RoxyFilemanException("E_CannotMoveDirToChild");

            _fileProvider.DirectoryMove(sourcePath, destinationPath);

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(new { res = "ok" });
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