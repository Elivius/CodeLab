using System;
using System.IO;
using System.Web.Hosting;

namespace CodeLab.Helpers
{
    /// <summary>
    /// Checks and stores uploaded images (profile photos now, course covers later).
    /// Files are saved under ~/Uploads/ with a new random name; only the path goes into the database.
    /// </summary>
    public static class ImageUploadHelper
    {
        public const string AvatarFolder = "~/Uploads/Avatars/";

        /// <summary>
        /// True if the file name ends in an extension we accept (.jpg, .jpeg, .png, .webp).
        /// This is only a first check; DetectImageExtension looks at the real file contents.
        /// </summary>
        public static bool HasAllowedExtension(string fileName)
        {
            string extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp";
        }

        /// <summary>
        /// Looks at the first bytes of the file and returns ".jpg", ".png" or ".webp",
        /// or null if the data is not one of those image types (e.g. a renamed .exe).
        /// </summary>
        public static string DetectImageExtension(byte[] data)
        {
            if (data == null || data.Length < 12)
            {
                return null;
            }

            // JPEG: FF D8 FF
            if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return ".jpg";
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47
                && data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
            {
                return ".png";
            }

            // WebP: "RIFF" .... "WEBP"
            if (data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F'
                && data[8] == 'W' && data[9] == 'E' && data[10] == 'B' && data[11] == 'P')
            {
                return ".webp";
            }

            return null;
        }

        /// <summary>
        /// Saves the image into the folder (e.g. AvatarFolder) under a new random name and
        /// returns its app-relative path, e.g. "~/Uploads/Avatars/u2-3f9c....png".
        /// </summary>
        public static string SaveImage(byte[] data, string extension, string folder, string namePrefix)
        {
            string physicalFolder = HostingEnvironment.MapPath(folder);
            Directory.CreateDirectory(physicalFolder);

            string fileName = namePrefix + "-" + Guid.NewGuid().ToString("N") + extension;
            File.WriteAllBytes(Path.Combine(physicalFolder, fileName), data);

            return folder + fileName;
        }

        /// <summary>
        /// Deletes a file saved by SaveImage. Does nothing for paths outside the folder,
        /// so a stored value can never point this at another file on the server.
        /// </summary>
        public static void DeleteImage(string appRelativePath, string folder)
        {
            if (string.IsNullOrEmpty(appRelativePath)
                || !appRelativePath.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                string fileName = Path.GetFileName(appRelativePath);
                string physicalPath = Path.Combine(HostingEnvironment.MapPath(folder), fileName);
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch (Exception exception)
            {
                // A leftover file is harmless; never fail the user's action over it
                LogHelper.LogError("ImageUploadHelper.DeleteImage failure", exception);
            }
        }
    }
}
