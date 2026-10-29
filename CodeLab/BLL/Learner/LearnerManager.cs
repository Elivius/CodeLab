using System;
using CodeLab.DAL;
using CodeLab.Models;
namespace CodeLab.BLL
{
    public class LearnerManager
    {
        public sealed class LevelStage
        {
            public int Level { get; private set; }
            public string Title { get; private set; }
            public int RequiredXp { get; private set; }
            public LevelStage(int level, string title, int xp) { Level=level; Title=title; RequiredXp=xp; }
        }
        public static readonly System.Collections.ObjectModel.ReadOnlyCollection<LevelStage> Levels =
            System.Array.AsReadOnly(new[] {
                new LevelStage(1,"Novice Coder",0), new LevelStage(2,"Code Explorer",500),
                new LevelStage(3,"Rising Developer",1000), new LevelStage(4,"Code Voyager",1200),
                new LevelStage(5,"Skilled Developer",2000), new LevelStage(6,"Code Specialist",3500),
                new LevelStage(7,"Expert Developer",5000), new LevelStage(8,"Code Architect",7500),
                new LevelStage(9,"Programming Master",10000), new LevelStage(10,"Code Legend",15000)
            });
        public static LevelStage GetLevel(int points)
        {
            for(int i=Levels.Count-1;i>=0;i--) if(points>=Levels[i].RequiredXp) return Levels[i];
            return Levels[0];
        }
        private readonly LearnerRepository repository = new LearnerRepository();
        public LearnerDashboardData GetDashboard(int learnerId)
        {
            if (learnerId <= 0) throw new ArgumentException("Invalid learner ID", "learnerId");
            var data = repository.GetDashboard(learnerId);
            var stage = GetLevel(data.Points);
            data.Level = stage.Level; data.LevelTitle = stage.Title;
            data.Milestone = stage.Level < Levels.Count ? Levels[stage.Level].RequiredXp : stage.RequiredXp;
            return data;
        }
    }
}
