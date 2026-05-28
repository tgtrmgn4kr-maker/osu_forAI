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
        internal HashSet<DrawableHitObject> SubscribedObjects;

        public struct ScoreContainer
        {
            public DrawableHitObject? HitObject;
            public JudgementResult? Result;
        }
        public struct RewardEvent
        {
            public int ObjectType;
            public int ResultType;
            public float TimeOffset;
        }
        public RewardTracker(OsuPlayfield.AIPlayfield playfield, SharedTrackerState state)
        {
            SubscribedObjects = new();
            playfield.OnAIPlayFieldNewDrawableHitObject += CollectObjects;
            this.state = state;
        }
        private readonly SharedTrackerState state;


        internal void CollectObjects(DrawableHitObject obj)
        {
            if (state.SubscribedObjects.Contains(obj.HitObject)) return;
            obj.OnNewResult += ScoreGetter;
            state.SubscribedObjects.Add(obj.HitObject);
        }
        public void ScoreGetter(DrawableHitObject obj, JudgementResult result)
        {
            ScoreContainer scoreContainer = new()
            {
                HitObject = obj,
                Result = result
            };/*
            Logger.Log($"Type: {scoreContainer.HitObject.GetType()}");
            Logger.Log($"SmallType: {scoreContainer.HitObject.HitObject.GetType()}");
            Logger.Log($"TimeOffset: {scoreContainer.Result.TimeOffset}");
            Logger.Log($"Judgement: {scoreContainer.Result.Type}");*/
            state.SubscribedObjects.Remove(obj.HitObject);
        }
    }

}
