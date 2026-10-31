using System;
using System.Diagnostics;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class AdminStatsManager
    {
        private readonly AdminStatsRepository _repository = new AdminStatsRepository();

        /// <summary>
        /// Returns the numbers for the admin stat cards, tab badges and banner.
        /// </summary>
        public OperationResult<AdminStats> GetStats()
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<AdminStats>.Fail("Unauthorized: Only administrators can view platform statistics.");
            }

            try
            {
                AdminStats stats = _repository.GetStats();

                Stopwatch stopwatch = Stopwatch.StartNew();
                _repository.Ping();
                stopwatch.Stop();
                stats.QueryMilliseconds = stopwatch.ElapsedMilliseconds;

                return OperationResult<AdminStats>.Ok(stats);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("AdminStatsManager.GetStats failure", exception);
                return OperationResult<AdminStats>.Fail("The platform statistics could not be loaded.");
            }
        }
    }
}
