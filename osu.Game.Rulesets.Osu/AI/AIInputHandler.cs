// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.StateChanges;
using osu.Game.Input.Handlers;
using osuTK;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.AI
{
    public class AIInputHandler : ReplayInputHandler
    {
        private readonly SharedActionReader reader;

        public AIInputHandler()
        {
            reader = new();
        }

        public override bool IsActive => throw new System.NotImplementedException();

        public override double? SetFrameFromTime(double time) => time;

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            base.CollectPendingInputs(inputs);
            var action = reader.Read();
            inputs.Add(
                new MousePositionAbsoluteInput
                {
                    Position =
                        GamefieldToScreenSpace(
                            new Vector2(action.CursorX, action.CursorY)
                        )
                }
            );
            List<OsuAction> actions = new();
            if (action.K1 != 0)
                actions.Add(OsuAction.LeftButton);
            if (action.K2 != 0)
                actions.Add(OsuAction.RightButton);

            inputs.Add(
                new ReplayState<OsuAction>
                {
                    PressedActions = new List<OsuAction>() { OsuAction.LeftButton }
                }
            );
        }
    }

}
