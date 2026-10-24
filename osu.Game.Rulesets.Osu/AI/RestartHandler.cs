// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using osu.Framework.Logging;
using osu.Game.AI;

namespace osu.Game.Rulesets.Osu.AI
{
    public class RestartHandler
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CurrentState
        {
            public int EpisodeID; // C# Write

            public byte EnvironmentState; // C# Write
            public byte EpisodeEndReason; // C# Write

            public int RestartRequestID; // Python Write
            public int RestartAckID; // C# Write
            public byte NullSpace;
        }
        private PlayingStateContainer playingStateContainer;
        public CurrentState State;
        public RestartHandler(PlayingStateContainer playingStateContainer)
        {
            this.playingStateContainer = playingStateContainer;
        }
        public void Update()
        {
            State.EnvironmentState = (byte)playingStateContainer.EnvironmentController.State;
            State.EpisodeEndReason = (byte)playingStateContainer.EnvironmentController.EndReason;
        }
        public bool CheckRestartRequest(int restartRequestID)
        {
            Logger.Log($"Restart request check: {State.RestartRequestID} == {State.RestartAckID}");
            if (restartRequestID == State.RestartAckID) return false;

            if (restartRequestID != State.RestartAckID + 1)
                throw new InvalidOperationException("Restart request ID mismatch: {restartRequestID} != {State.RestartAckID + 1}");

            State.RestartRequestID = restartRequestID;
            State.RestartAckID = restartRequestID;
            State.EnvironmentState = (byte)EnvironmentState.Resetting;
            State.EpisodeEndReason = (byte)EpisodeEndReason.None;
            return playingStateContainer.EnvironmentController.ResetRequest();
        }
    }
}
