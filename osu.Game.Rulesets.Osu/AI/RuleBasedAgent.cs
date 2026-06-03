// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Game.Input.Handlers;
using osuTK;

namespace osu.Game.Rulesets.Osu.AI
{
    public class RuleBasedAgent : ReplayInputHandler
    {
        public override bool IsActive => throw new System.NotImplementedException();

        public override double? SetFrameFromTime(double time) => time;

        private readonly ObjectTracker objectTracker;

        public RuleBasedAgent(ObjectTracker objectTracker)
        {
            this.objectTracker = objectTracker;
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            base.CollectPendingInputs(inputs);

            var action = objectTracker.GetFrameObservation;

            var obs = objectTracker.GetFrameObservation;

            float cursorX = obs.CursorRuntimeData.X;
            float cursorY = obs.CursorRuntimeData.Y;

            if (obs.Data[0].ObjectType == 3 && obs.Data[0].TimeToHit < 30)
            {
                inputs.Add(
                    new MousePositionAbsoluteInput
                    {
                        Position = GamefieldToScreenSpace(
                            new Vector2(
                                obs.SliderRuntimeData.DistanceToCursorX + cursorX,
                                obs.SliderRuntimeData.DistanceToCursorY + cursorY)
                        )
                    }
                );
                inputs.Add(
                    new ReplayState<OsuAction>
                    {
                        PressedActions = new List<OsuAction>() { OsuAction.LeftButton }
                    }
                );

            }

            else if (obs.Data[0].ObjectType == 0 && obs.Data[0].TimeToHit < 30)
            {
                inputs.Add(
                    new MousePositionAbsoluteInput
                    {
                        Position = GamefieldToScreenSpace(
                            new Vector2(
                                obs.Data[0].DistanceToCursorX + cursorX,
                                obs.Data[0].DistanceToCursorY + cursorY
                            )
                        )
                    }
                );
                inputs.Add(
                    new ReplayState<OsuAction>
                    {
                        PressedActions = new List<OsuAction>() { OsuAction.LeftButton }
                    }
                );
            }
        }
    }
}
