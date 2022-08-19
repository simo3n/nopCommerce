using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
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

        public RoxyFilemanFileProvider(INopFileProvider nopFileProvider) : base(nopFileProvider.Combine(nopFileProvider.WebRootPath, NopRoxyFilemanDefaults.DefaultRootDirectory))
        {
            _nopFileProvider = nopFileProvider;
        }

        public RoxyFilemanFileProvider(INopFileProvider defaultFileProvider, IPictureService pictureService, MediaSettings mediaSettings) : this(defaultFileProvider)
        {
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        #region Utils

        /// <summary>
        /// Resize image by targetSize
        /// </summary>
        /// <param name="image">Source image</param>
        /// <param name="format">Destination format</param>
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

            var maxWidth = image.Width > 1000 ? 1000 : 0; //TODO: do not forget to make it configurable
            var maxHeight = image.Height > 1000 ? 1000 : 0;

            var (width, height) = ImageResize(image, maxWidth > maxHeight ? maxWidth : maxHeight);

            var toBitmap = new SKBitmap(width, height, image.ColorType, image.AlphaType);

            if (image.ScalePixels(toBitmap, SKFilterQuality.Medium))
            {
                var newImage = SKImage.FromBitmap(toBitmap);
                var imageData = newImage.Encode();

                await _nopFileProvider.WriteAllBytesAsync(pictureInfo.PhysicalPath, imageData.ToArray());
            }
        }

        #endregion

        public new IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath))
                return new NotFoundFileInfo(subpath);

            subpath = subpath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Absolute paths not permitted.
            if (Path.IsPathRooted(subpath))
                return new NotFoundFileInfo(subpath);

            var pictureFileInfo = base.GetFileInfo(subpath);

            if (pictureFileInfo.Exists && !_pictureService.IsStoreInDbAsync().Result)
                return pictureFileInfo;

            var virtualPath = string.Join("/", NopRoxyFilemanDefaults.DefaultRootDirectory, subpath);
            var picture = _pictureService.GetPictureByPathAsync(virtualPath).Result;

            SaveImageOnDiskAsync(picture, pictureFileInfo).Wait();

            return base.GetFileInfo(subpath);
        }

        /// <summary>
        /// Returns the file name of the specified path string without the extension
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <returns>The file name, minus the last period (.) and all characters following it</returns>
        public string GetFileNameWithoutExtension(string filePath)
        {
            return _nopFileProvider.GetFileNameWithoutExtension(filePath);
        }
    }
}