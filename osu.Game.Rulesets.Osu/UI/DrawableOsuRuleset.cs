// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osuTK;
using osu.Game.Rulesets.Osu.AI;
using osu.Game.Rulesets.Osu.AI.Play;
using osu.Framework.Logging;
using osu.Game.AI;


namespace osu.Game.Rulesets.Osu.UI
{
    public partial class DrawableOsuRuleset : DrawableRuleset<OsuHitObject>
    {
        private Bindable<bool>? cursorHideEnabled;

        public new OsuInputManager KeyBindingInputManager => (OsuInputManager)base.KeyBindingInputManager;

        public new OsuPlayfield Playfield => (OsuPlayfield)base.Playfield;

        private OsuPlayfield.AIPlayfield aIPlayfield = null!;

        private ObjectTracker objectTracker = null!;
        private RewardTracker rewardTracker = null!;
        private SharedTrackerState? sharedState = null;
        private SharedActionReader? actionReader = null;
        private PlayingStateContainer? playingStateContainer = null;
        private ObservationWriter? observationWriter = null;



        protected new OsuRulesetConfigManager Config => (OsuRulesetConfigManager)base.Config;

        public DrawableOsuRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
            : base(ruleset, beatmap, mods)
        {
            Logger.Log("DrawableOsuRuleset Ready");
        }

        /*
        Loading order
        0. DrawableRuleset
        1. Input Manager
        2. Load()
        3. Replay Recorder
        */

        [BackgroundDependencyLoader]
        private void load(ReplayPlayer? replayPlayer)
        {
            Logger.Log("Load Ready");
            sharedState = new();
            actionReader = new();
            playingStateContainer = new();
            objectTracker = new ObjectTracker(aIPlayfield, playingStateContainer);
            rewardTracker = new RewardTracker(aIPlayfield, sharedState);


            if (replayPlayer != null)
            {
                ReplayAnalysisOverlay analysisOverlay;
                PlayfieldAdjustmentContainer.Add(analysisOverlay = new ReplayAnalysisOverlay(replayPlayer.Score.Replay));
                Overlays.Add(analysisOverlay.CreateProxy().With(p => p.Depth = float.NegativeInfinity));
                replayPlayer.AddSettings(new ReplayAnalysisSettings(Config));

                cursorHideEnabled = Config.GetBindable<bool>(OsuRulesetSetting.ReplayCursorHideEnabled);

                // I have little faith in this working (other things touch cursor visibility) but haven't broken it yet.
                // Let's wait for someone to report an issue before spending too much time on it.
                cursorHideEnabled.BindValueChanged(enabled => Playfield.Cursor.FadeTo(enabled.NewValue ? 0 : 1), true);
            }
        }


        public override DrawableHitObject<OsuHitObject>? CreateDrawableRepresentation(OsuHitObject h) => null;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true; // always show the gameplay cursor

        protected override Playfield CreatePlayfield()
        {
            aIPlayfield = new OsuPlayfield.AIPlayfield();
            return aIPlayfield;
        }

        protected override PassThroughInputManager CreateInputManager()
        {
            Logger.Log("InputManager Ready");
            return new OsuInputManager(Ruleset.RulesetInfo);
        }


        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { AlignWithStoryboard = true };

        protected override ResumeOverlay CreateResumeOverlay()
        {
            if (Mods.Any(m => m is OsuModAutopilot or OsuModTouchDevice))
                return new DelayedResumeOverlay { Scale = new Vector2(0.65f) };

            return new OsuResumeOverlay();
        }

        protected override void Update()
        {
            base.Update();

            long frameID = aIPlayfield.FrameID;
            objectTracker!.Update(frameID);

            observationWriter!.Write();

            rewardTracker!.Clear();
        }
        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay)
        {
            Logger.Log("ReplayInputHandler ready");
            return new OsuFramedReplayInputHandler(replay);
        }


        // Activating only when AIPlayer is activated
        protected override AIInputHandler CreateAIInputHandler(PlayingStateContainer playingStateContainer)
        {
            Logger.Log($"AIInputHandler Created");
            Logger.Log($"HashCode: {playingStateContainer.GetHashCode()}");

            this.playingStateContainer = playingStateContainer;
            objectTracker = new ObjectTracker(aIPlayfield, playingStateContainer);
            observationWriter = new ObservationWriter(objectTracker, rewardTracker!);
            return new OsuAIInputHandler(actionReader!);
        }

        protected override ReplayRecorder CreateReplayRecorder(Score score)
        {
            Logger.Log("ReplayRecorder Ready");

            return new OsuReplayRecorder(score);
        }

        public override double GameplayStartTime
        {
            get
            {
                if (Objects.FirstOrDefault() is OsuHitObject first)
                    return first.StartTime - Math.Max(2000, first.TimePreempt);

                return 0;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            observationWriter?.Dispose();

            base.Dispose(isDisposing);
        }

    }
}
