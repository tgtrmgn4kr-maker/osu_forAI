// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SharedMemoryReaderStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SharedMemoryReader";

        /// <summary>
        /// "This library only supports Windows"
        /// </summary>
        public static LocalisableString ThisLibraryOnlySupportsWindows => new TranslatableString(getKey(@"this_library_only_supports_windows"), @"This library only supports Windows");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}