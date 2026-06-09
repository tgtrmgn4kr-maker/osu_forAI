// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Rulesets.Osu.AI.Play
{
    public partial class OsuAIPlayer : SoloPlayer
    {
        protected override ResultsScreen CreateResults(ScoreInfo score)
        => new SoloResultsScreen(score)
        {
            AllowRetry = true,
            IsLocalPlay = true,
        };
    }

}
