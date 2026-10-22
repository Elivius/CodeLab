using System;
using System.IO;

namespace CodeLab.Helpers
{
    /// <summary>
    /// Checks uploaded attachments (lesson materials). Only document, archive and image types are
    /// allowed; video and anything executable is refused. Save and delete the checked file with
    /// ImageUploadHelper.SaveImage / DeleteImage, which work for any bytes and folder.
    /// </summary>
    public static class FileUploadHelper
    {
        public const string AttachmentFolder = "~/Uploads/Attachments/";

        public static readonly string[] AllowedExtensions = { ".pdf", ".zip", ".txt", ".docx", ".pptx", ".xlsx", ".png", ".jpg", ".jpeg", ".webp" };

        public const string AllowedTypesMessage = "Attachments can be PDF, ZIP, TXT, DOCX, PPTX, XLSX or image files (no video).";

        /// <summary>
        /// Checks an uploaded attachment (size, name, then the real file contents) and reads it into memory.
        /// Returns null when it is valid (data, extension and displayName are filled in), otherwise a message
        /// to show the user. displayName is the original file name with anything unsafe removed.
        /// </summary>
        public static string ReadValidatedAttachment(string fileName, int contentLength, Stream content, int maxBytes,
            out byte[] data, out string extension, out string displayName)
        {
            data = null;
            extension = null;
            displayName = null;

            if (content == null || contentLength <= 0)
            {
                return "Choose a file to attach first.";
            }

            if (contentLength > maxBytes)
            {
                return "The attachment must be " + (maxBytes / (1024 * 1024)) + " MB or smaller.";
            }

            string cleanName = CleanFileName(fileName);
            string declared = Path.GetExtension(cleanName).ToLowerInvariant();
            if (Array.IndexOf(AllowedExtensions, declared) < 0)
            {
                return AllowedTypesMessage;
            }

            byte[] buffer = new byte[contentLength];
            int bytesRead = 0;
            while (bytesRead < contentLength)
            {
                int read = content.Read(buffer, bytesRead, contentLength - bytesRead);
                if (read == 0)
                {
                    break;
                }

                bytesRead += read;
            }

            if (bytesRead != contentLength || !MatchesExtension(buffer, declared))
            {
                return "That file does not look like a valid " + declared.TrimStart('.').ToUpperInvariant() + " file.";
            }

            data = buffer;
            extension = declared == ".jpeg" ? ".jpg" : declared;
            displayName = cleanName;
            return null;
        }

        // Does the start of the file match what its extension claims?
        private static bool MatchesExtension(byte[] data, string extension)
        {
            switch (extension)
            {
                case ".pdf":
                    return data.Length >= 5 && data[0] == '%' && data[1] == 'P' && data[2] == 'D' && data[3] == 'F' && data[4] == '-';

                // zip, docx, pptx and xlsx are all zip archives
                case ".zip":
                case ".docx":
                case ".pptx":
                case ".xlsx":
                    return data.Length >= 4 && data[0] == 'P' && data[1] == 'K' && data[2] == 3 && data[3] == 4;

                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".webp":
                    return ImageUploadHelper.DetectImageExtension(data) != null;

                case ".txt":
                    // Plain text only: no NUL or other control bytes
                    foreach (byte b in data)
                    {
                        if (b == 0 || (b < 0x20 && b != 0x09 && b != 0x0A && b != 0x0D))
                        {
                            return false;
                        }
                    }

                    return true;

                default:
                    return false;
            }
        }

        // The file name without any folder part or control characters, shortened to a sensible length
        private static string CleanFileName(string fileName)
        {
            string name = Path.GetFileName((fileName ?? string.Empty).Replace('\\', '/'));
            char[] characters = name.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                if (char.IsControl(characters[i]))
                {
                    characters[i] = '_';
                }
            }

            name = new string(characters).Trim();
            const int maxLength = 120;
            if (name.Length > maxLength)
            {
                string extension = Path.GetExtension(name);
                name = name.Substring(0, maxLength - extension.Length) + extension;
            }

            return name;
        }
    }
}
