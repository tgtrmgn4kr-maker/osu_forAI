// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.



using osu.Framework.Allocation;
using osu.Game.Scoring;
using osu.Game.Screens.Play.Leaderboards;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public partial class AIPlayer : Player
    {
        [Cached(typeof(IGameplayLeaderboardProvider))]
        private readonly SoloGameplayLeaderboardProvider leaderboardProvider = new SoloGameplayLeaderboardProvider();

        protected override void LoadComplete()
        {
            DrawableRuleset?.SetAIHandler();
            base.LoadComplete();
        }
        protected override ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score)
        {
            AllowRetry = true,
            IsLocalPlay = true,
        };
        [BackgroundDependencyLoader(true)]
        private void load()
        {
            AddInternal(leaderboardProvider);
        }
    }
}
