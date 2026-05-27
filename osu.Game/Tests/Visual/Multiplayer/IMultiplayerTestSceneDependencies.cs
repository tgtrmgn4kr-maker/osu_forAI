// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// Interface that defines the dependencies required for multiplayer test scenes.
    /// </summary>
    public interface IMultiplayerTestSceneDependencies : IOnlinePlayTestSceneDependencies
    {
        /// <summary>
        /// The cached <see cref="Online.Multiplayer.MultiplayerClient"/>.
        /// </summary>
        TestMultiplayerClient MultiplayerClient { get; }


#pragma warning disable IDE0001 // Simplify Names
        /// <summary>
        /// The cached <see cref="osu.Game.Online.Spectator.SpectatorClient"/>.
        /// </summary>
        TestSpectatorClient SpectatorClient { get; }
#pragma warning restore IDE0001 // Simplify Names
    }
}
