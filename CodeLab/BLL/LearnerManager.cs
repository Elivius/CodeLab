using System;
using CodeLab.DAL;
using CodeLab.Models;
namespace CodeLab.BLL
{
    public class LearnerManager
    {
        private readonly LearnerRepository repository = new LearnerRepository();
        public LearnerDashboardData GetDashboard(int learnerId)
        {
            if (learnerId <= 0) throw new ArgumentException("Invalid learner ID", "learnerId");
            return repository.GetDashboard(learnerId);
        }
    }
}
