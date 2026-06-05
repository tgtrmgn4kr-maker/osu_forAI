// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


namespace osu.Game.Screens.Play
{
    public partial class AIPlayerLoader : PlayerLoader
    {
        public AIPlayerLoader()
            : base(() => new AIPlayer())
        {

        }
    }

}
