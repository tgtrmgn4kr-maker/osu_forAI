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
using osu.Framework.Logging;


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
            public bool Failed;
            public RewardEvent()
            {
                EventID = -1;
                ObjectType = 0;
                ResultType = 0;
                TimeOffset = -1;
                Failed = false;
            }
        }
        private Dictionary<Type, int> objectType = new()
        {
            // HitCircle
            [typeof(DrawableHitCircle)] = 1,

            // Slider
            [typeof(DrawableSliderHead)] = 2,
            [typeof(DrawableSliderTail)] = 3,
            [typeof(DrawableSlider)] = 4,
            [typeof(DrawableSliderRepeat)] = 5,
            [typeof(DrawableSliderTick)] = 6,

            // Spinner
            [typeof(DrawableSpinner)] = 7,
            [typeof(DrawableSpinnerTick)] = 8,
            [typeof(DrawableSpinnerBonusTick)] = 9,
        };

        private Dictionary<HitResult, int> scoreConverter = new()
        {
            // HitCircle, Spinner, SliderHead
            {HitResult.Great, 1},
            {HitResult.Good, 2},
            {HitResult.Ok, 3},
            {HitResult.Meh, 4},
            {HitResult.Miss, 5},

            // SliderTick, SliderRepeat
            {HitResult.LargeTickHit, 6},
            {HitResult.LargeTickMiss, 7},

            // SpinnerTick
            {HitResult.SmallBonus, 8},
            // SpinnerBonusTick
            {HitResult.LargeBonus, 9},

            // Slider
            {HitResult.SliderTailHit, 10},
            {HitResult.IgnoreHit, 0},
            {HitResult.IgnoreMiss, 0},
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
            GetRewards = new RewardEvent[10];
        }

        internal void CollectObjects(DrawableHitObject obj)
        {
            if (!state.SubscribedObjects.Contains(obj.HitObject)
                && !state.SubscribedInt.Contains(obj.GetHashCode())
                && !state.SubscribedInt.Contains(obj.HitObject.GetHashCode()))
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
                Failed = result.FailedAtJudgement
            };

            eventID++;
            Logger.Log($"Failed {rewardEvent.Failed}");
            GetRewards[rewardCount] = rewardEvent;
            rewardCount++;

        }
        public void Update()
        {
            // Every frame has its own reward
            GetRewards = new RewardEvent[10];
            rewardCount = 0;
        }
    }
}
