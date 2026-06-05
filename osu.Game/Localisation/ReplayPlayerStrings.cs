// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ReplayPlayerStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ReplayPlayer";

        /// <summary>
        /// " on "
        /// </summary>
        public static LocalisableString On => new TranslatableString(getKey(@"on"), @" on ");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}