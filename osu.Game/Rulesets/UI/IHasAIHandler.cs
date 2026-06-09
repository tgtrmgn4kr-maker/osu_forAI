// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osu.Game.Input.Handlers;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Expose the <see cref="AIInputHandler"/> in a capable <see cref="InputManager"/>.
    /// </summary>
    public interface IHasAIHandler
    {
        AIInputHandler? AIInputHandler { get; set; }
    }
}
