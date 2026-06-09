// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Input.Handlers;
using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osuTK;
using System;

namespace osu.Game.Rulesets.Osu.AI.Play
{
    public class OsuAIInputHandler : AIInputHandler
    {
        protected readonly List<OsuAction> PressedAction = new();
        public override double? SetFrameFromTime(double time) => time;

        private readonly SharedActionReader memory;

        private Random random;

        public OsuAIInputHandler(SharedActionReader memory)
        {
            this.memory = memory;
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

            inputs.Add(
                new MousePositionAbsoluteInput
                {
                    Position = GamefieldToScreenSpace
                        (new Vector2
                            (random.Next(0, 512),
                            random.Next(0, 384)))
                });

            // ReplayState may could be replaced by AI state
            inputs.Add(new ReplayState<OsuAction> { PressedActions = new List<OsuAction> { OsuAction.LeftButton } });
        }
    }
}
