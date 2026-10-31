using System;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class AuditManager
    {
        private const int MaxDetailsLength = 500;
        private const int PageSize = 25;

        private readonly AuditRepository _auditRepository = new AuditRepository();

        /// <summary>
        /// Records a business activity (e.g. "CREATE_QUIZ") against a user.
        /// Call this from a BLL method after the operation it describes has succeeded.
        /// A failure to write the audit record is logged but never thrown to the caller.
        /// </summary>
        public void RecordActivity(int userId, string action, string details)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(action))
            {
                return;
            }

            if (details != null && details.Length > MaxDetailsLength)
            {
                details = details.Substring(0, MaxDetailsLength);
            }

            AuditLog log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _auditRepository.InsertAuditLog(log);
        }

        /// <summary>
        /// Returns one page of the audit log, newest first. category is one of AuditCategories, or null
        /// for all of them. A page past the end shows the last page.
        /// </summary>
        public OperationResult<AuditLogPage> Search(string search, string category, int page)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<AuditLogPage>.Fail("Unauthorized: Only administrators can view audit logs.");
            }

            if (category != null && AuditCategories.Find(category) == null)
            {
                return OperationResult<AuditLogPage>.Fail("Unknown audit category.");
            }

            try
            {
                string searchText = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

                int totalCount = _auditRepository.CountLogs(searchText, category);
                int pageCount = Math.Max(1, (totalCount + PageSize - 1) / PageSize);
                int currentPage = Math.Min(Math.Max(page, 1), pageCount);

                return OperationResult<AuditLogPage>.Ok(new AuditLogPage
                {
                    Rows = _auditRepository.GetLogs(searchText, category, (currentPage - 1) * PageSize, PageSize),
                    TotalCount = totalCount,
                    Page = currentPage,
                    PageCount = pageCount
                });
            }
            catch (Exception exception)
            {
                LogHelper.LogError("AuditManager.Search failure", exception);
                return OperationResult<AuditLogPage>.Fail("The audit logs could not be loaded. Please try again.");
            }
        }
    }
}
