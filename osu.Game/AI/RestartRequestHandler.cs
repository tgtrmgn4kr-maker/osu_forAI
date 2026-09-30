// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace osu.Game.AI
{
    public unsafe class RestartRequestHandler : IDisposable
    {
        private EnvironmentController environmentController;
        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;
        private RestartRequestBuffer restartRequestBuffer;
        private byte* shmPtr;
        private byte* ptr;
        public RestartRequestHandler(EnvironmentController environmentController, string name = "Osu_restart_request")
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("This library only supports Windows.");

            int size = Marshal.SizeOf<RestartRequestBuffer>();
            mmf = MemoryMappedFile.CreateOrOpen(name, size);
            accessor = mmf.CreateViewAccessor();
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref shmPtr);

            this.environmentController = environmentController;

            restartRequestBuffer = new();
        }

        private void setPtr()
        {
            ptr = shmPtr; // ptr points to the start of the shared memory buffer
        }

        private void write()
        {
            *ptr = (byte)environmentController.State;
            ptr += sizeof(byte); // ptr points to the next byte in the shared memory buffer
            *ptr = (byte)environmentController.EndReason;
        }

        private void read()
        {
            RestartRequestBuffer restartRequestBuffer = *(RestartRequestBuffer*)ptr;
            if (restartRequestBuffer.ResetRequest == 1)
                environmentController?.ResetRequest();
        }
        public void Update()
        {
            restartRequestBuffer = new(); // Initialise the buffer to 0
            write();
            setPtr();
            if (environmentController.State == EnvironmentState.Failed)
                read();
        }

        public void Dispose()
        {
            accessor?.Dispose();
            mmf?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RestartRequestBuffer
    {
        public byte EnvironmentState;
        public byte EpisodeEndReason;
        public byte ResetRequest; // Receive this byte to reset the environment from Python. 1 = Reset, 0 = No reset.
        public RestartRequestBuffer()
        {
            EnvironmentState = 0;
            EpisodeEndReason = 0;
            ResetRequest = 0;
        }
    }
}
