// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;


namespace osu.Game.Rulesets.Osu.AI
{
    public class SharedTrackerState
    {
        /// <summary>
        /// A HashSet to store the Alive Objects until it has been judged.
        /// </summary>
        public HashSet<HitObject> SubscribedObjects { get; } = new();
    }

}
