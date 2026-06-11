// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.UI;
using System.Collections.Generic;
using System;
using osu.Game.Rulesets.Scoring;
using System.Runtime.InteropServices;


namespace osu.Game.Rulesets.Osu.AI
{
    public class RewardTracker
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RewardEvent
        {
            public long EventID;
            public int ObjectType;
            public int ResultType;
            public double TimeOffset;
        }
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

        private long eventID;
        private readonly SharedTrackerState state;

        private RewardEvent rewardEvent;

        public RewardEvent[] GetRewards { get; private set; }
        private int rewardCount;

        public RewardTracker(OsuPlayfield.AIPlayfield playfield, SharedTrackerState state)
        {
            playfield.OnAIPlayFieldNewDrawableHitObject += CollectObjects;
            this.state = state;

            eventID = 0;
            rewardCount = 0;
            GetRewards = new RewardEvent[5];
        }

        internal void CollectObjects(DrawableHitObject obj)
        {
            if (!state.SubscribedObjects.Contains(obj.HitObject) && !state.SubscribedInt.Contains(obj.GetHashCode()) && !state.SubscribedInt.Contains(obj.HitObject.GetHashCode()))
            {
                obj.OnNewResult += ScoreGetter;
                state.SubscribedObjects.Add(obj.HitObject);
                state.SubscribedInt.Add(obj.HitObject.GetHashCode());
            }
        }
        public void ScoreGetter(DrawableHitObject obj, JudgementResult result)
        {
            obj.OnNewResult -= ScoreGetter;

            rewardEvent = new()
            {
                ObjectType = objectType[obj.GetType()],
                ResultType = scoreConverter[result.Type],
                TimeOffset = result.TimeOffset,
                EventID = eventID,
            };

            eventID++;

            GetRewards[rewardCount] = rewardEvent;
            rewardCount++;

        }
        public void Update()
        {
            GetRewards = new RewardEvent[5];
            rewardCount = 0;
        }
    }
}
