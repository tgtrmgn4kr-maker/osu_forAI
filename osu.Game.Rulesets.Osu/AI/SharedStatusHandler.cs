// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Osu.AI
{
    public unsafe class SharedStatusHandler : IDisposable
    {
        private RestartHandler restartHandler;
        private byte* ptr;
        private byte* shmPtr;
        private byte* nullSpacePtr;
        private int size = Marshal.SizeOf<RestartHandler.CurrentState>();
        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;
        private RestartHandler.CurrentState currentState => collectDataFromMemory();
        public SharedStatusHandler(RestartHandler restartHandler, string name = "osu_status")
        {
            this.restartHandler = restartHandler;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();

            // Declare a memory space to write
            mmf = MemoryMappedFile.CreateOrOpen(name, size);
            accessor = mmf.CreateViewAccessor();

            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref shmPtr);
            nullSpacePtr = shmPtr + size - sizeof(byte);
            restartHandler.State = currentState;
        }
        /// <summary>
        /// When a restart is requested, everything is initialised except the memory,
        /// so we need to collect data from the memory and increase the episode ID
        /// </summary>
        /// <returns>Returns a <see cref="RestartHandler.CurrentState"/> for
        /// the current <see cref="RestartHandler"/> to deal with the
        /// playing state and restart request</returns>
        private RestartHandler.CurrentState collectDataFromMemory()
        {
            setPointer();
            int episodeID;

            if (*nullSpacePtr == 0)
            {
                (*nullSpacePtr)++;
                episodeID = 0;
            }
            else
            {
                episodeID = ++*(int*)ptr;
            }
            Logger.Log($"Episode ID: {episodeID}");

            ptr += sizeof(int);
            ptr += sizeof(byte) * 2; // EnvironmentState and EpisodeEndReason
            int restartRequestID = *(int*)ptr;
            ptr += sizeof(int);
            int restartAckID = *(int*)ptr;
            ptr += sizeof(int);

            if (restartRequestID != restartAckID)
                throw new DataNotMatchedException("Restart request ID and restart ack ID do not match");

            return new RestartHandler.CurrentState
            {
                EpisodeID = episodeID,
                RestartRequestID = restartRequestID,
                RestartAckID = restartAckID,
            };
        }
        public void Update(byte state)
        {
            // Get the current state from the PlayingStateContainer
            restartHandler.Update();

            setPointer();
            ptr += sizeof(int);
            // The first 4 bytes are reserved for the episode ID
            *ptr = restartHandler.State.EnvironmentState;
            ptr += sizeof(byte);
            *ptr = restartHandler.State.EpisodeEndReason;
            ptr += sizeof(byte);
            int restartRequestID = *(int*)ptr;

            if (state == 2 ||
                restartHandler.State.EnvironmentState == 1 ||
                restartHandler.State.EpisodeEndReason == 0)
                return; // If is playing, there is no need to check the restart request

            // Read the reset request
            if (restartHandler.CheckRestartRequest(restartRequestID))
            {
                setPointer();
                ptr += sizeof(int);
                // The first 4 bytes are reserved for the episode ID
                *ptr = restartHandler.State.EnvironmentState;
                ptr += sizeof(byte);
                *ptr = restartHandler.State.EpisodeEndReason;
                ptr += sizeof(byte);
                *(int*)ptr = restartHandler.State.RestartRequestID;
                ptr += sizeof(int);
                *(int*)ptr = restartHandler.State.RestartAckID;

                Logger.Log("Restarting");
                Thread.Sleep(1000);
            }
        }
        private void setPointer()
        {
            ptr = shmPtr;
        }
        public void Dispose()
        {
            mmf?.Dispose();
            accessor?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    public class DataNotMatchedException : Exception
    {
        public DataNotMatchedException(string message) : base(message)
        {
        }
    }
}
