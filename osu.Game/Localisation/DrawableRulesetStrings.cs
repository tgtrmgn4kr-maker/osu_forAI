// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DrawableRulesetStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DrawableRuleset";

        /// <summary>
        /// "A {0} which supports replay loading is not available"
        /// </summary>
        public static LocalisableString AWhichSupportsReplayLoading(string arg0) => new TranslatableString(getKey(@"awhich_supports_replay_loading"), @"A {0} which supports replay loading is not available", arg0);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}