// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using System.Collections.Generic;

namespace osu.Game.Input.Handlers
{
    public class AIInputHandler : ReplayInputHandler
    {
        public override bool Initialize(GameHost host)
        {
            return base.Initialize(host);
        }

        public AIInputHandler()
        {
        }

        public override bool IsActive => true;

        public override double? SetFrameFromTime(double time) => time;

        public override void CollectPendingInputs(List<IInput> inputs)
        {

        }
    }

}
