// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Input.Handlers;
using System.Collections.Generic;
using osu.Game.AI;
using osu.Framework.Input.StateChanges;
using osuTK;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuAIInputHandler : ReplayInputHandler
    {
        private SharedActionReader? memory;
        private List<OsuAction>? actions;
        public override bool Initialize(GameHost host)
        {
            return base.Initialize(host);
        }

        public OsuAIInputHandler()
        {
        }

        public override bool IsActive => true;

        public override double? SetFrameFromTime(double time) => time;

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            var action = memory!.Read();

            inputs.Add(
                new MousePositionAbsoluteInput
                {
                    Position = GamefieldToScreenSpace(
                        new Vector2(action.CursorX, action.CursorY)
                    )
                }
            );
            inputs.Add(
                new ReplayState<OsuAction>
                {
                    PressedActions = actions
                }
            );
        }
    }

}
