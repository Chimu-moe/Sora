using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sora.Database;
using Sora.Database.Models;
using Achievement = Sora.Objects.Achievement;
using Beatmap = Sora.Utilities.Beatmap;
using BeatmapSet = Sora.Utilities.BeatmapSet;

namespace Sora
{
    public static class AchievementProcessor
    {
        /// <summary>
        /// Create Default Achievements
        /// </summary>
        /// <param name="factory">Context Factory</param>
        public static async Task CreateDefaultAchievements(SoraDbContext ctx)
        {
            if (ctx.Achievements.FirstOrDefault(x => x.Name == "oog") == null)
                await DbAchievement.NewAchievement(
                    ctx,
                    "oog",
                    "Oooooooooooooooog!",
                    "You just oooged JSE",
                    "https://onii-chan-please.come-inside.me/achivement_oog.png"
                );
        }

        /// <summary>
        /// Check if User has Obtained an achievement on this Score Submission
        /// </summary>
        /// <param name="factory">Context Factory</param>
        /// <param name="user">Who tries to Obtain</param>
        /// <param name="score">Submitted Score</param>
        /// <param name="map">Beatmap</param>
        /// <param name="set">Beatmap Set</param>
        /// <param name="oldLb">Old LeaderBoard</param>
        /// <param name="newLb">New LeaderBoard</param>
        /// <returns>Obtained Achievements</returns>
        public static string ProcessAchievements(SoraDbContext ctx,
            DbUser user,
            DbScore score,
            Beatmap map,
            BeatmapSet set,
            DbLeaderboard oldLb,
            DbLeaderboard newLb
        )
        {
            var l = new List<Achievement>();

            /*
            if ((int) newLB.PerformancePointsOsu == 4914)
            {
                var ach = DBAchievement.GetAchievement(factory, "oog");
                if (!user.AlreadyOptainedAchievement(ach))
                    _l.Add(ach);
            }
            */

            // Insert custom achievements here. I'll implement a Plugin System later! but this will work for now.


            // END OF CUSTOM ACHIEVEMENTS

            var retVal = l.Aggregate("", (current, ach) => current + ach.ToOsuString() + "/");
            retVal.TrimEnd('/');

            return retVal;
        }
    }
}