// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class GameplaySampleTriggerSourceStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.GameplaySampleTriggerSource";

        /// <summary>
        /// "concurrent sample pool"
        /// </summary>
        public static LocalisableString ConcurrentSamplePool => new TranslatableString(getKey(@"concurrent_sample_pool"), @"concurrent sample pool");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}