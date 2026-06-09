// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Play
{
    public partial class AIPlayLoader : PlayerLoader
    {
        public AIPlayLoader(Func<Player> createPlayer)
            : base(() => new AIPlayer())
        {
        }
    }

}

