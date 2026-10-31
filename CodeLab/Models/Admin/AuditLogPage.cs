using System.Collections.Generic;

namespace CodeLab.Models
{
    /// <summary>
    /// One page of audit log rows, and where it sits among all the rows that match.
    /// </summary>
    public class AuditLogPage
    {
        public List<AuditLog> Rows { get; set; }

        // How many rows match in total, across every page
        public int TotalCount { get; set; }

        // The page shown, starting at 1, and how many pages there are
        public int Page { get; set; }
        public int PageCount { get; set; }
    }
}
