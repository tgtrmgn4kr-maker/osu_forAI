// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Screens.Play;

namespace osu.Game.AI
{
    public sealed class EnvironmentController
    {
        public EnvironmentState State { get; private set; } = EnvironmentState.Idle;
        public EpisodeEndReason EndReason { get; private set; } = EpisodeEndReason.None;
        public long EpisodeID { get; private set; }
        private PlayerLoader? playerLoader;
        public bool IsPlaying => State == EnvironmentState.Playing;
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
            playerLoader.EpisodeStarted += StartEpisode;
            playerLoader.PlayerCreated += player =>
            {
                player.HealthProcessorLoaded += () =>
                {
                    player.HealthProcessor.Failed += PlayerFailed;
                };
                player.Exited += PlayerExited;
                player.Passed += PlayerPassed;
                State = EnvironmentState.WaitingForGamePlay;
            };
            State = EnvironmentState.WaitingForGamePlay;
            EpisodeID = 0;
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
        }
        public void StartEpisode()
        {
            EpisodeID++;
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
                return false;

            if (State != EnvironmentState.Playing)
                return false;

            State = EnvironmentState.Resetting;

            return playerLoader.Restart(true);
        }
    }

    public enum EnvironmentState : byte
    {
        Idle,
        Playing,
        Failed,
        Passed,
        Resetting,
        WaitingForGamePlay,
    }
    public enum EpisodeEndReason : byte
    {
        None,
        Failed,
        Passed,
    }

}
