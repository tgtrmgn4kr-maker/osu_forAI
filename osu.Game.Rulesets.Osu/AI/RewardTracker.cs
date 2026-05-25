// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;



namespace osu.Game.Rulesets.Osu.AI
{
    public class RewardTracker
    {

        public void AIResultRegister(DrawableHitObject obj)
        {
            obj.OnNewResult += ScoreGetter;
        }

        public void ScoreGetter(DrawableHitObject obj, JudgementResult result)
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var type = result.Type;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            int comboAfterJudgement = result.ComboAfterJudgement;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }
    }

}
