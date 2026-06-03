// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.UI;
using System.Collections.Generic;
using osu.Framework.Logging;
using System;
using osu.Game.Rulesets.Scoring;




namespace osu.Game.Rulesets.Osu.AI
{
    public class RewardTracker
    {
        private Dictionary<Type, int> objectType = new()
        {
            // HitCircle
            [typeof(DrawableHitCircle)] = 0,

            // Slider
            [typeof(DrawableSliderHead)] = 1,
            [typeof(DrawableSliderTail)] = 2,
            [typeof(DrawableSlider)] = 3,
            [typeof(DrawableSliderRepeat)] = 4,
            [typeof(DrawableSliderTick)] = 5,

            // Spinner
            [typeof(DrawableSpinner)] = 6,
            [typeof(DrawableSpinnerTick)] = 7,
            [typeof(DrawableSpinnerBonusTick)] = 8,
        };

        private Dictionary<HitResult, int> scoreConverter = new()
        {
            // HitCircle, Spinner, SliderHead
            {HitResult.Great, 0},
            {HitResult.Good, 1},
            {HitResult.Ok, 2},
            {HitResult.Meh, 3},
            {HitResult.Miss, 4},

            // SliderTick, SliderRepeat
            {HitResult.LargeTickHit, 5},
            {HitResult.LargeTickMiss, 6},

            // SpinnerTick
            {HitResult.SmallBonus, 7},
            // SpinnerBonusTick
            {HitResult.LargeBonus, 8},

            // Slider
            {HitResult.SliderTailHit, 9},
            {HitResult.IgnoreHit, -1},
            {HitResult.IgnoreMiss, -1},
        };

        public struct RewardEvent
        {
            public int ObjectType;
            public int ResultType;
            public double TimeOffset;
        }

        public RewardTracker(OsuPlayfield.AIPlayfield playfield, SharedTrackerState state)
        {
            playfield.OnAIPlayFieldNewDrawableHitObject += CollectObjects;
            this.state = state;
        }
        private readonly SharedTrackerState state;


        internal void CollectObjects(DrawableHitObject obj)
        {
            if (!state.SubscribedObjects.Contains(obj.HitObject))
            {
                obj.OnNewResult += ScoreGetter;
                state.SubscribedObjects.Add(obj.HitObject);
            }
        }
        public void ScoreGetter(DrawableHitObject obj, JudgementResult result)
        {

            RewardEvent rewardEvent = new()
            {
                ObjectType = objectType[obj.GetType()],
                ResultType = scoreConverter[result.Type],
                TimeOffset = result.TimeOffset,
            };
            Logger.Log($"Event: {rewardEvent.TimeOffset}");
            state.SubscribedObjects.Remove(obj.HitObject);
        }
    }

}
