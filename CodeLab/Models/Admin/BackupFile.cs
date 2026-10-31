using System;

namespace CodeLab.Models
{
    /// <summary>
    /// One database backup in App_Data/Backups. There is no table for these: the folder is the list.
    /// </summary>
    public class BackupFile
    {
        public string Name { get; set; }
        public long SizeBytes { get; set; }

        // UTC
        public DateTime CreatedAt { get; set; }

        // SHA-256 of the file, from its .sha256 file next to it; null if that file is missing
        public string Sha256 { get; set; }

        public string SizeText
        {
            get { return (SizeBytes / 1048576.0).ToString("0.0") + " MB"; }
        }
    }
}
