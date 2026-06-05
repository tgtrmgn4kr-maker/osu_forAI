// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using System.Linq;
using osu.Game.Rulesets.Mods;


namespace osu.Game.Screens.Play
{
    public partial class AIPlayer : Player
    {
        private bool isAutoplayPlayback => GameplayState.Mods.OfType<ModAutoplay>().Any();
        protected override ResultsScreen CreateResults(ScoreInfo score)
        => new SoloResultsScreen(score)
        {
            // Only show the relevant button otherwise things look silly.
            AllowWatchingReplay = !isAutoplayPlayback,
            AllowRetry = isAutoplayPlayback,
        };

        protected override void PrepareReplay()
        {
        }
    }
}
