using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class AuditManager
    {
        private const int MaxDetailsLength = 500;

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

        public OperationResult<List<AuditLog>> GetRecentAuditLogs()
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<List<AuditLog>>.Fail("Unauthorized: Only administrators can view audit logs.");
            }

            try
            {
                return OperationResult<List<AuditLog>>.Ok(_auditRepository.GetRecentLogsWithUsernames());
            }
            catch (Exception exception)
            {
                LogHelper.LogError("AuditManager.GetRecentAuditLogs failure", exception);
                return OperationResult<List<AuditLog>>.Fail("The audit logs could not be loaded. Please try again.");
            }
        }
    }
}
