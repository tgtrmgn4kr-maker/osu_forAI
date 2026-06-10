// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Input.Handlers;
using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osuTK;
using System;
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Osu.AI.Play
{
    public class OsuAIInputHandler : AIInputHandler
    {
        protected readonly List<OsuAction> PressedAction = new();
        public override double? SetFrameFromTime(double time) => time;

        private readonly SharedActionReader memory;

        private readonly ObjectTracker objectTracker;

        private int count;

        // Test
        private double nextHitObjectTime;
        private Vector2 lastPos;
        //

        private Random random;

        public OsuAIInputHandler(SharedActionReader memory, ObjectTracker objectTracker, SharedTrackerState state)
        {
            this.memory = memory;
            this.objectTracker = objectTracker;
            random = new();
        }

        public override bool IsActive => true;

        public sealed override void CollectPendingInputs(List<IInput> inputs)
        {
            base.CollectPendingInputs(inputs);
            CollectAIInputs(inputs);
        }

        protected void CollectAIInputs(List<IInput> inputs)
        {
            var actions = memory.Read();

            var obs = objectTracker.GetFrameObservation;

            if (obs.Data?[0] is null) return;

            Logger.Log($"Time to hit: {obs.Data[0].TimeToHit * 1000}");

            var firstObject = obs.Data[0];
            var CursorPosition = new Vector2(obs.CursorRuntimeData.X, obs.CursorRuntimeData.Y);

            if (obs.SliderRuntimeData.Progress != -1)
            {
                hold(
                    new Vector2(
                        (obs.SliderRuntimeData.DistanceToCursorX + CursorPosition.X) * 256 + 256,
                        (obs.SliderRuntimeData.DistanceToCursorY + CursorPosition.Y) * 192 + 192
                    )
                );
                return;
            }

            foreach (var obj in obs.Data)
            {
                if (obj.TimeToHit * 1000 < 20 && obj.TimeToHit * 1000 > -20 && obj.ObjectType == 0)
                {
                    hit(new Vector2(
                            (obj.DistanceToCursorX + CursorPosition.X) * 256 + 256,
                            (obj.DistanceToCursorY + CursorPosition.Y) * 192 + 192
                    ));
                }

                else if (obj.TimeToHit * 1000 < 20 && obj.TimeToHit * 1000 > -20 && obj.ObjectType == 3)
                {
                    hit(new Vector2(
                        (obj.DistanceToCursorX + CursorPosition.X) * 256 + 256,
                        (obj.DistanceToCursorY + CursorPosition.Y) * 192 + 192
                    ));
                }
            }

            void hit(Vector2 pos)
            {
                inputs.Add(new MousePositionAbsoluteInput
                {
                    Position = GamefieldToScreenSpace(pos)
                });

                inputs.Add(new ReplayState<OsuAction>
                {
                    PressedActions = new List<OsuAction> { count == 0 ? OsuAction.LeftButton : OsuAction.RightButton },
                });

                count = 1 - count;
            }
            void hold(Vector2 pos)
            {
                inputs.Add(new MousePositionAbsoluteInput
                {
                    Position = GamefieldToScreenSpace(pos)
                });
                inputs.Add(new ReplayState<OsuAction>
                {
                    PressedActions = new List<OsuAction> { count == 0 ? OsuAction.LeftButton : OsuAction.RightButton }
                });
            }
        }
    }
}
