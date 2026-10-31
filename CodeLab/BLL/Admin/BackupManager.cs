using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// Admin database backups. Each backup is a .bak file in App_Data/Backups with a .sha256 file next
    /// to it. IIS never serves App_Data, so the only way to get a backup is through this class.
    /// There is no restore: it would replace the database everyone is using.
    /// </summary>
    public class BackupManager
    {
        private const string BackupFolder = "~/App_Data/Backups";
        private const string ChecksumExtension = ".sha256";
        private const string UnauthorizedMessage = "Unauthorized: Only administrators can manage database backups.";

        // The only file names this class creates. Nothing else can be listed or downloaded.
        private static readonly Regex BackupName = new Regex(@"^codelab-\d{8}-\d{6}\.bak\z");

        private readonly BackupRepository _repository = new BackupRepository();

        /// <summary>
        /// Returns the stored backups, newest first.
        /// </summary>
        public OperationResult<List<BackupFile>> GetBackups()
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<List<BackupFile>>.Fail(UnauthorizedMessage);
            }

            try
            {
                string folder = HostingEnvironment.MapPath(BackupFolder);
                if (!Directory.Exists(folder))
                {
                    return OperationResult<List<BackupFile>>.Ok(new List<BackupFile>());
                }

                List<BackupFile> backups = new DirectoryInfo(folder).GetFiles("*.bak")
                    .Where(file => BackupName.IsMatch(file.Name))
                    .OrderByDescending(file => file.LastWriteTimeUtc)
                    .Select(file => new BackupFile
                    {
                        Name = file.Name,
                        SizeBytes = file.Length,
                        CreatedAt = file.LastWriteTimeUtc,
                        Sha256 = ReadChecksum(file.FullName)
                    })
                    .ToList();

                return OperationResult<List<BackupFile>>.Ok(backups);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("BackupManager.GetBackups failure", exception);
                return OperationResult<List<BackupFile>>.Fail("The backups could not be listed. Please try again.");
            }
        }

        /// <summary>
        /// Backs up the database and records the file's SHA-256 next to it.
        /// </summary>
        public OperationResult Create()
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult.Fail(UnauthorizedMessage);
            }

            string name = "codelab-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".bak";

            try
            {
                string folder = HostingEnvironment.MapPath(BackupFolder);
                Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, name);
                if (!_repository.CreateBackup(path))
                {
                    return OperationResult.Fail("The backup could not be created. Please try again.");
                }

                File.WriteAllText(path + ChecksumExtension, ComputeChecksum(path));

                new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "CREATE_BACKUP",
                    "Created database backup " + name + ".");

                return OperationResult.Ok("Backup " + name + " was created.");
            }
            catch (Exception exception)
            {
                LogHelper.LogError("BackupManager.Create failure", exception);
                return OperationResult.Fail("The backup could not be finished. Please try again.");
            }
        }

        /// <summary>
        /// Checks a download request. On success, Data holds the file's full path on the server.
        /// </summary>
        public OperationResult<string> GetFileForDownload(string name)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<string>.Fail(UnauthorizedMessage);
            }

            // A strict match, so a changed name such as ..\Database.mdf can never reach another file
            if (name == null || !BackupName.IsMatch(name))
            {
                return OperationResult<string>.Fail("This backup could not be found.");
            }

            string path = Path.Combine(HostingEnvironment.MapPath(BackupFolder), name);
            if (!File.Exists(path))
            {
                return OperationResult<string>.Fail("This backup could not be found. It may have been removed.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "DOWNLOAD_BACKUP",
                "Downloaded database backup " + name + ".");

            return OperationResult<string>.Ok(path);
        }

        private static string ReadChecksum(string backupPath)
        {
            string checksumPath = backupPath + ChecksumExtension;
            return File.Exists(checksumPath) ? File.ReadAllText(checksumPath).Trim() : null;
        }

        // Upper-case hex, the same format as PowerShell's Get-FileHash, so they are easy to compare
        private static string ComputeChecksum(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }
    }
}
