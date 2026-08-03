// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Input.Handlers;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;


namespace osu.Game.Rulesets.Osu.AI.Play
{
    public class OsuAIInputHandler : AIInputHandler
    {
        private readonly SharedActionReader memory;

        public OsuAIInputHandler(SharedActionReader memory)
        {
            Logger.Log("Input Handler Created");
            this.memory = memory;
        }

        public override bool IsActive => true;
        public override double? SetFrameFromTime(double time) => time;
        public sealed override void CollectPendingInputs(List<IInput> inputs)
        {
            base.CollectPendingInputs(inputs);
            CollectAIInputs(inputs);
        }

        protected void CollectAIInputs(List<IInput> inputs)
        {
            var actions = memory.Read();

            inputs.Add(new MousePositionAbsoluteInput
            {
                Position = GamefieldToScreenSpace(actions.CursorPosition)
            });

            inputs.Add(new ReplayState<OsuAction>
            {
                PressedActions = actions.OsuActions,
            });
        }
    }
}
