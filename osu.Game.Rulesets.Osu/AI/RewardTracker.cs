// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.UI;
using System.Collections.Generic;
using osu.Framework.Logging;



namespace osu.Game.Rulesets.Osu.AI
{
    public class RewardTracker
    {
        public HashSet<DrawableHitObject> SubscribedObjects;

        public struct ScoreContainer
        {
            public ScoreContainer()
            {
            }
            public DrawableHitObject? HitObject = null;
            public JudgementResult? Result = null;
        }
        public RewardTracker(OsuPlayfield.AIPlayfield playfield)
        {
            this.playfield = playfield;
            playfield.NewResult += onNewResult;
            SubscribedObjects = new();
        }
        private OsuPlayfield.AIPlayfield? playfield;


        // Not executed
        private void onNewResult(DrawableHitObject obj, JudgementResult result)
        {
            CollectObjects();
        }
        public void CollectObjects()
        {
            var nextObjects = playfield?.HitObjectContainer.Get8AliveObjects();

            if (nextObjects is null) return;

            foreach (var obj in nextObjects)
            {
                if (SubscribedObjects.Contains(obj)) continue;

                obj.OnNewResult += ScoreGetter;
                SubscribedObjects.Add(obj);
            }

        }
        public void ScoreGetter(DrawableHitObject obj, JudgementResult result)
        {
            ScoreContainer scoreContainer = new()
            {
                HitObject = obj,
                Result = result
            };
            Logger.Log($"TimeOffset:{scoreContainer.Result.TimeOffset}");
        }
    }

}
