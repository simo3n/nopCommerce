using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            _fileProvider.CopyDirectory(sourcePath, destinationPath);

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(new { res = "ok" });
        }

        public async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            _fileProvider.CopyFile(sourcePath, destinationPath);
            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(new { res = "ok" });
        }

        public async Task CreateDirectoryAsync(string parentDirectoryPath, string name)
        {
            _fileProvider.CreateDirectory(parentDirectoryPath, name);

            var response = GetHttpContext().Response;
            await response.WriteAsJsonAsync(new { res = "ok" });
        }

        public Task CreateImageThumbnailAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public async Task DeleteDirectoryAsync(string path)
        {
            _fileProvider.DeleteDirectory(path);

            var response = GetHttpContext().Response;
            await response.WriteAsJsonAsync(new { res = "ok" });
        }

        public async Task DeleteFileAsync(string path)
        {
            _fileProvider.DeleteFile(path);

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(new { res = "ok" });
        }

        public Task DownloadDirectoryAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public async Task DownloadFileAsync(string path)
        {
            var file = _fileProvider.GetFileInfo(path);

            if (file.Exists)
            {
                var response = GetHttpContext().Response;
                response.Clear();
                response.Headers.ContentDisposition = $"attachment; filename=\"{WebUtility.UrlEncode(file.Name)}\"";
                response.ContentType = MimeTypes.ApplicationForceDownload;
                await response.SendFileAsync(file);
            }
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

            foreach (var (path, countFiles, countDirectories) in contents)
            {
                result.Add(new
                {
                    p = path.Replace("\\", "/"),
                    f = countFiles,
                    d = countDirectories
                });
            }

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(result);
        }

        public async Task GetFilesAsync(string directoryPath, string type)
        {
            var result = _fileProvider.GetFiles(directoryPath, type)
                .Select(f => new
                {
                    p = f.RelativePath.Replace("\\", "/"),
                    t = f.LastWriteTime.ToUnixTimeSeconds(),
                    s = f.FileLength,
                    w = f.Width,
                    h = f.Height
                });

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(result);
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

        public async Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            if (!CanHandleFile(sourcePath) && !CanHandleFile(destinationPath))
                throw new RoxyFilemanException("E_FileExtensionForbidden");

            _fileProvider.FileMove(sourcePath, destinationPath);

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(new { res = "ok" });
        }

        public async Task RenameDirectoryAsync(string sourcePath, string newName)
        {
            _fileProvider.RenameDirectory(sourcePath, newName);
            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(new { res = "ok" });
        }

        public async Task RenameFileAsync(string sourcePath, string newName)
        {
            if (!CanHandleFile(sourcePath) && !CanHandleFile(newName))
                throw new RoxyFilemanException("E_FileExtensionForbidden");

            _fileProvider.RenameFile(sourcePath, newName);

            var response = GetHttpContext().Response;

            await response.WriteAsJsonAsync(new { res = "ok" });
        }

        public Task UploadFilesAsync(string directoryPath)
        {
            throw new System.NotImplementedException();
        }
    }
}