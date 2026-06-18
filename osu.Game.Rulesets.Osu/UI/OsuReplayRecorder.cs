// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuReplayRecorder : ReplayRecorder<OsuAction>
    {
        public byte GetHitButton { get; private set; }

        public OsuReplayRecorder(Score score)
            : base(score)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<OsuAction> actions, ReplayFrame previousFrame)
        {
            // Combined two button in one byte
            byte Button = 0;
            if (actions.Contains(OsuAction.LeftButton))
                Button |= 1;
            if (actions.Contains(OsuAction.RightButton))
                Button |= 1 << 1;

            // Make sure both the buttons are updated
            GetHitButton = Button;
            return new OsuReplayFrame(Time.Current, mousePosition, actions.ToArray());
        }
    }
}
