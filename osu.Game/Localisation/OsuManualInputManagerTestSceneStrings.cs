// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class OsuManualInputManagerTestSceneStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.OsuManualInputManagerTestScene";

        /// <summary>
        /// "click {0}"
        /// </summary>
        public static LocalisableString Click(string arg0) => new TranslatableString(getKey(@"click"), @"click {0}", arg0);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}