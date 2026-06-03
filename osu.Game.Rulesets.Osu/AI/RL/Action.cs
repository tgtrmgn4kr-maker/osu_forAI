// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Input.Handlers;

namespace osu.Game.Rulesets.Osu.AI.RL
{
    public class Action
    {
        public struct ActionData
        {
            public float MoveX;
            public float MoveY;
            public bool Key1;
            public bool Key2;

        }
    }

}
