// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Logging;
using osu.Game.Screens.Play;

namespace osu.Game.AI
{
    public sealed class EnvironmentController
    {
        public EnvironmentState State { get; private set; } = EnvironmentState.Idle;
        public EpisodeEndReason EndReason { get; private set; } = EpisodeEndReason.None;
        private PlayerLoader? playerLoader;
        public EnvironmentController()
        {
        }
        /// <summary>
        /// Called whenever <see cref="PlayerLoader"/> is created
        /// </summary>
        /// <param name="playerLoader"></param>
        public void AttachPlayerLoader(PlayerLoader playerLoader)
        {
            this.playerLoader = playerLoader;
            playerLoader.PlayerCreated += player =>
            {
                player.HealthProcessorLoaded += () =>
                {
                    player.HealthProcessor.Failed += PlayerFailed;
                };
                player.Exited += PlayerExited;
                player.Passed += PlayerPassed;
                player.OnGameplayStarted += StartEpisode;
                State = EnvironmentState.Idle;
                EndReason = EpisodeEndReason.None;
            };
            State = EnvironmentState.Idle;
        }
        /// <summary>
        /// Called when the current <see cref="Player"/> is no longer valid
        /// </summary>
        public void DetachPlayer(Player player)
        {
            if (playerLoader?.CurrentPlayer == null)
                throw new InvalidOperationException("PlayerLoader is not attached to this controller.");

            playerLoader.CurrentPlayer.Passed -= PlayerPassed;
            playerLoader.CurrentPlayer.Exited -= PlayerExited;
            playerLoader.CurrentPlayer.HealthProcessor.Failed -= PlayerFailed;
        }
        public void PlayerPassed()
        {
            EndEpisode(EpisodeEndReason.Passed);
        }
        public bool PlayerFailed()
        {
            EndEpisode(EpisodeEndReason.Failed);
            return true;
        }
        public void PlayerExited(Player player)
        {
            DetachPlayer(player);
            State = EnvironmentState.Idle;
        }
        public void StartEpisode()
        {
            State = EnvironmentState.Playing;
            EndReason = EpisodeEndReason.None;
        }
        public void EndEpisode(EpisodeEndReason reason)
        {
            EndReason = reason;
            switch (reason)
            {
                case EpisodeEndReason.Failed:
                    State = EnvironmentState.Failed;
                    break;
                case EpisodeEndReason.Passed:
                    State = EnvironmentState.Passed;
                    break;
            }

        }
        /// <summary>
        /// Requires a native osu! restart.
        /// The new <see cref="Player"/> is created asynchronously by <see cref="PlayerLoader"/>.
        /// </summary>
        public bool ResetRequest()
        {
            if (playerLoader?.CurrentPlayer is null)
                throw new InvalidOperationException("PlayerLoader is not attached to this controller.");

            Logger.Log($"Reset requested. Current state: {State}"); // Debugging purpose only. Should not be visible in production.

            if (!((State == EnvironmentState.Failed) || (State == EnvironmentState.Passed)))
                throw new InvalidOperationException("PlayerLoader is not attached to this controller.");

            State = EnvironmentState.Resetting;
            EndReason = EpisodeEndReason.None;

            return playerLoader?.CurrentPlayer?.Restart(true) ?? throw new InvalidOperationException("PlayerLoader is not attached to this controller.");
        }
    }
    public enum EnvironmentState : byte
    {
        Idle = 0,
        Playing = 1,
        Failed = 1 << 1,
        Passed = 1 << 2,
        Resetting = 1 << 3,
    }
    public enum EpisodeEndReason : byte
    {
        None = 0,
        Failed = 1,
        Passed = 2,
    }

}
