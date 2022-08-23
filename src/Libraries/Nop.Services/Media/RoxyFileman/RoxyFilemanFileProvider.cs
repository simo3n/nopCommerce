using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using SkiaSharp;

namespace Nop.Services.Media.RoxyFileman
{
    public class RoxyFilemanFileProvider : PhysicalFileProvider, IRoxyFilemanFileProvider
    {
        #region Fields

        protected INopFileProvider _nopFileProvider;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

        public RoxyFilemanFileProvider(INopFileProvider nopFileProvider) : base(nopFileProvider.Combine(nopFileProvider.WebRootPath, NopRoxyFilemanDefaults.DefaultRootDirectory))
        {
            _nopFileProvider = nopFileProvider;
        }

        public RoxyFilemanFileProvider(INopFileProvider defaultFileProvider, IPictureService pictureService, MediaSettings mediaSettings) : this(defaultFileProvider)
        {
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        #endregion

        #region Utils

        /// <summary>
        /// Resize image by targetSize
        /// </summary>
        /// <param name="image">Source image</param>
        /// <param name="targetSize">Target size</param>
        /// <returns>Image as array of byte[]</returns>
        protected virtual (int, int) ImageResize(SKBitmap image, int targetSize)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            float width, height;
            if (image.Height > image.Width)
            {
                // portrait
                width = image.Width * (targetSize / (float)image.Height);
                height = targetSize;
            }
            else
            {
                // landscape or square
                width = targetSize;
                height = image.Height * (targetSize / (float)image.Width);
            }

            if ((int)width == 0 || (int)height == 0)
            {
                width = image.Width;
                height = image.Height;
            }

            return ((int)width, (int)height);
        }

                /// <summary>
        /// Get a file type by file extension
        /// </summary>
        /// <param name="fileExtension">File extension</param>
        /// <returns>File type</returns>
        protected virtual string GetFileType(string subpath)
        {
            var fileExtension = _nopFileProvider.GetFileExtension(subpath)?.ToLowerInvariant();

            return fileExtension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" => "image",
                ".swf" or ".flv" => "flash",
                ".mp4" or ".webm" or ".ogg" or ".mov" or ".m4a" or ".mp3" or ".wav" => "media",
                _ => "file"
            };

            /* These media extensions are not supported by HTML5 or tinyMCE out of the box
             * but may possibly be supported if You find players for them.
             * if (fileExtension == ".3gp" || fileExtension == ".flv" 
             *     || fileExtension == ".rmvb" || fileExtension == ".wmv" || fileExtension == ".divx"
             *     || fileExtension == ".divx" || fileExtension == ".mpg" || fileExtension == ".rmvb"
             *     || fileExtension == ".vob" // video
             *     || fileExtension == ".aif" || fileExtension == ".aiff" || fileExtension == ".amr"
             *     || fileExtension == ".asf" || fileExtension == ".asx" || fileExtension == ".wma"
             *     || fileExtension == ".mid" || fileExtension == ".mp2") // audio
             *     fileType = "media"; */
        }

        protected virtual string GetFullPath(string path)
        {
            var fullPath = Path.GetFullPath(Path.Combine(Root, path));

            if (!IsUnderneathRoot(fullPath))
                throw new Exception(path);

            return fullPath;
        }

        /// <summary>
        /// Get image format by mime type
        /// </summary>
        /// <param name="mimeType">Mime type</param>
        /// <returns>SKEncodedImageFormat</returns>
        protected virtual SKEncodedImageFormat GetImageFormatByMimeType(string mimeType)
        {
            var format = SKEncodedImageFormat.Jpeg;
            if (string.IsNullOrEmpty(mimeType))
                return format;

            var parts = mimeType.ToLowerInvariant().Split('/');
            var lastPart = parts[^1];

            switch (lastPart)
            {
                case "webp":
                    format = SKEncodedImageFormat.Webp;
                    break;
                case "png":
                case "gif":
                case "bmp":
                case "x-icon":
                    format = SKEncodedImageFormat.Png;
                    break;
                default:
                    break;
            }

            return format;
        }

        protected virtual bool IsUnderneathRoot(string fullPath)
        {
            return fullPath
                .StartsWith(Root, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Flush image on disk
        /// </summary>
        /// <param name="picture">Image to store on disk</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SaveImageOnDiskAsync(Picture picture, IFileInfo pictureInfo)
        {
            var pictureBinary = await _pictureService.GetPictureBinaryByPictureIdAsync(picture.Id);

            if (pictureBinary?.BinaryData == null || pictureBinary?.BinaryData.Length == 0)
                return;

            using var sourceStream = new SKMemoryStream(pictureBinary.BinaryData);
            using var image = SKBitmap.Decode(sourceStream);

            var roxyConfig = Singleton<RoxyFilemanConfig>.Instance;

            var maxWidth = image.Width > roxyConfig.MAX_IMAGE_WIDTH ? roxyConfig.MAX_IMAGE_WIDTH : 0;
            var maxHeight = image.Height > roxyConfig.MAX_IMAGE_HEIGHT ? roxyConfig.MAX_IMAGE_HEIGHT : 0;

            var (width, height) = ImageResize(image, maxWidth > maxHeight ? maxWidth : maxHeight);

            var toBitmap = new SKBitmap(width, height, image.ColorType, image.AlphaType);

            if (image.ScalePixels(toBitmap, SKFilterQuality.None))
            {
                using var mutex = new Mutex(false, pictureInfo.PhysicalPath);
                mutex.WaitOne();

                try
                {
                    var newImage = SKImage.FromBitmap(toBitmap);
                    var imageData = newImage.Encode(GetImageFormatByMimeType(picture.MimeType), _mediaSettings.DefaultImageQuality);

                    await _nopFileProvider.WriteAllBytesAsync(pictureInfo.PhysicalPath, imageData.ToArray());
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Moves a file or a directory and its contents to a new location
        /// </summary>
        /// <param name="sourceDirName">The path of the file or directory to move</param>
        /// <param name="destDirName">
        /// The path to the new location for sourceDirName. If sourceDirName is a file, then destDirName
        /// must also be a file name
        /// </param>
        public virtual void DirectoryMove(string sourceDirName, string destDirName)
        {
            var sourceDirInfo = new DirectoryInfo(GetFullPath(sourceDirName));
            if (!sourceDirInfo.Exists)
                throw new RoxyFilemanException("E_MoveDirInvalisPath");

            var destinationDirInfo = new DirectoryInfo(GetFullPath(destDirName));
            if (destinationDirInfo.Exists)
                throw new RoxyFilemanException("E_MoveDirInvalisPath");

            _nopFileProvider.DirectoryMove(sourceDirInfo.FullName, destinationDirInfo.FullName);
        }

        public new IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath))
                return new NotFoundFileInfo(subpath);

            subpath = subpath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Absolute paths not permitted.
            if (Path.IsPathRooted(subpath))
                return new NotFoundFileInfo(subpath);

            var fileInfo = base.GetFileInfo(subpath);

            if (fileInfo.IsDirectory)
                return fileInfo;

            if (fileInfo.Exists && !_pictureService.IsStoreInDbAsync().Result)
                return fileInfo;

            var virtualPath = string.Join("/", NopRoxyFilemanDefaults.DefaultRootDirectory, subpath);
            var picture = _pictureService.GetPictureByPathAsync(virtualPath).Result;

            if (picture is null)
                return new NotFoundFileInfo(subpath);

            SaveImageOnDiskAsync(picture, fileInfo).Wait();

            return base.GetFileInfo(subpath);
        }

        /// <summary>
        /// Create configuration file for RoxyFileman
        /// </summary>
        /// <param name="pathBase"></param>
        /// <param name="lang"></param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<RoxyFilemanConfig> CreateConfigurationAsync(string pathBase, string lang)
        {
            //check whether the path base has changed, otherwise there is no need to overwrite the configuration file
            if (Singleton<RoxyFilemanConfig>.Instance?.RETURN_URL_PREFIX?.Equals(pathBase) ?? false)
            {
                return Singleton<RoxyFilemanConfig>.Instance;
            }

            var filePath = _nopFileProvider.GetAbsolutePath(NopRoxyFilemanDefaults.ConfigurationFile);

            //create file if not exists
            _nopFileProvider.CreateFile(filePath);

            //try to read existing configuration
            var existingText = await _nopFileProvider.ReadAllTextAsync(filePath, Encoding.UTF8);
            var existingConfiguration = JsonConvert.DeserializeObject<RoxyFilemanConfig>(existingText);

            //create configuration
            var configuration = new RoxyFilemanConfig
            {
                FILES_ROOT = existingConfiguration?.FILES_ROOT ?? NopRoxyFilemanDefaults.DefaultRootDirectory,
                SESSION_PATH_KEY = existingConfiguration?.SESSION_PATH_KEY ?? string.Empty,
                THUMBS_VIEW_WIDTH = existingConfiguration?.THUMBS_VIEW_WIDTH ?? 140,
                THUMBS_VIEW_HEIGHT = existingConfiguration?.THUMBS_VIEW_HEIGHT ?? 120,
                PREVIEW_THUMB_WIDTH = existingConfiguration?.PREVIEW_THUMB_WIDTH ?? 300,
                PREVIEW_THUMB_HEIGHT = existingConfiguration?.PREVIEW_THUMB_HEIGHT ?? 200,
                MAX_IMAGE_WIDTH = existingConfiguration?.MAX_IMAGE_WIDTH ?? _mediaSettings.MaximumImageSize,
                MAX_IMAGE_HEIGHT = existingConfiguration?.MAX_IMAGE_HEIGHT ?? _mediaSettings.MaximumImageSize,
                DEFAULTVIEW = existingConfiguration?.DEFAULTVIEW ?? "list",
                FORBIDDEN_UPLOADS = existingConfiguration?.FORBIDDEN_UPLOADS ?? string.Join(" ", NopRoxyFilemanDefaults.ForbiddenUploadExtensions),
                ALLOWED_UPLOADS = existingConfiguration?.ALLOWED_UPLOADS ?? string.Empty,
                FILEPERMISSIONS = existingConfiguration?.FILEPERMISSIONS ?? "0644",
                DIRPERMISSIONS = existingConfiguration?.DIRPERMISSIONS ?? "0755",
                LANG = existingConfiguration?.LANG ?? lang,
                DATEFORMAT = existingConfiguration?.DATEFORMAT ?? "dd/MM/yyyy HH:mm",
                OPEN_LAST_DIR = existingConfiguration?.OPEN_LAST_DIR ?? "yes",

                //no need user to configure
                INTEGRATION = "custom",
                RETURN_URL_PREFIX = pathBase,
                DIRLIST = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=DIRLIST",
                CREATEDIR = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=CREATEDIR",
                DELETEDIR = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=DELETEDIR",
                MOVEDIR = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=MOVEDIR",
                COPYDIR = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=COPYDIR",
                RENAMEDIR = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=RENAMEDIR",
                FILESLIST = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=FILESLIST",
                UPLOAD = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=UPLOAD",
                DOWNLOAD = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=DOWNLOAD",
                DOWNLOADDIR = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=DOWNLOADDIR",
                DELETEFILE = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=DELETEFILE",
                MOVEFILE = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=MOVEFILE",
                COPYFILE = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=COPYFILE",
                RENAMEFILE = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=RENAMEFILE",
                GENERATETHUMB = $"{pathBase}/Admin/RoxyFileman/ProcessRequest?a=GENERATETHUMB"
            };

            //save the file
            var text = JsonConvert.SerializeObject(configuration, Formatting.Indented);
            await _nopFileProvider.WriteAllTextAsync(filePath, text, Encoding.UTF8);

            Singleton<RoxyFilemanConfig>.Instance = configuration;

            return configuration;
        }

        public virtual void CopyDirectory(string sourcePath, string destinationPath)
        {
            var dir = new DirectoryInfo(sourcePath);

            _nopFileProvider.CreateDirectory(destinationPath);

            foreach (var file in dir.GetFiles())
            {
                var filePath = _nopFileProvider.Combine(destinationPath, file.Name);
                if (!_nopFileProvider.FileExists(filePath))
                    file.CopyTo(filePath);
            }

            foreach (var directory in dir.GetDirectories())
            {
                var directoryPath = _nopFileProvider.Combine(destinationPath, directory.Name);
                CopyDirectory(directory.FullName, directoryPath);
            }
        }

        /// <summary>
        /// Get all available directories as a directory tree
        /// </summary>
        /// <param name="type">Type of the file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual IEnumerable<(string relativePath, int countFiles, int countDirectories)> GetDirectories(string type, bool isRecursive = true, string rootDirectoryPath = "")
        {
            foreach (var item in GetDirectoryContents(rootDirectoryPath))
            {
                if (item.IsDirectory)
                {
                    var dirInfo = new DirectoryInfo(item.PhysicalPath);

                    yield return (getRelativePath(item.Name), dirInfo.GetFiles().Count(x => isMatchType(x.Name)), dirInfo.GetDirectories().Length);
                }

                if (!isRecursive)
                    break;

                foreach (var subDir in GetDirectories(type, isRecursive, getRelativePath(item.Name)))
                    yield return subDir;
            }

            string getRelativePath(string name) => Path.Combine(rootDirectoryPath, name);
            bool isMatchType(string name) => string.IsNullOrEmpty(type) || GetFileType(name) == type;
        }

        public virtual IEnumerable<RoxyImageInfo> GetFiles(string directoryPath = "", string type = "")
        {
            var files = GetDirectoryContents(directoryPath);

            return files
                .Where(x => isMatchType(x.Name))
                .Select(f => {
                    var (width, height) = getImageMeasures(f.CreateReadStream());
                    return new RoxyImageInfo(f.Name, f.LastModified, f.Length, width, height);
                });

            bool isMatchType(string name) => string.IsNullOrEmpty(type) || GetFileType(name) == type;

            (int width, int height) getImageMeasures(Stream imageStream)
            {
                    using var skData = SKData.Create(imageStream);
                    var image = SKBitmap.DecodeBounds(skData);
                    return (image.Width, image.Height);
            }
        }

        #endregion
    }
}