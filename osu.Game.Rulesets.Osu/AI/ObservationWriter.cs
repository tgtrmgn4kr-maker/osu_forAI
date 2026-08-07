// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace osu.Game.Rulesets.Osu.AI
{
    public unsafe class ObservationWriter : IDisposable
    {
        private ObjectTracker objectTracker;
        private RewardTracker rewardTracker;
        private byte* ptr;
        private byte* shmPtr;
        private int sizeOfObservation = Marshal.SizeOf<ObjectTracker.FrameObservation>();
        private int sizeofOsuObjectData = Marshal.SizeOf<ObjectTracker.OsuObjectsData>();
        private int sizeOfReward = Marshal.SizeOf<RewardTracker.RewardEvent>();
        private int totalSize;
        private int bufferSizeOfSharedObservation;
        private int bufferSizeOfObjectData;
        private int bufferSizeOfReward;

        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;

        public ObservationWriter(ObjectTracker objectTracker, RewardTracker rewardTracker, string name = "osu_obs")
        {
            this.objectTracker = objectTracker;
            this.rewardTracker = rewardTracker;

            bufferSizeOfSharedObservation = sizeOfObservation;
            bufferSizeOfObjectData = sizeofOsuObjectData * 10;
            bufferSizeOfReward = sizeOfReward * 10;

            totalSize =
                bufferSizeOfObjectData
                + bufferSizeOfReward
                + bufferSizeOfSharedObservation;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();

            // Declare a memory space to write
            mmf = MemoryMappedFile.CreateOrOpen(name, totalSize * 2);
            accessor = mmf.CreateViewAccessor();

            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref shmPtr);
        }
        private void setPointer()
        {
            ptr = shmPtr;
        }

        public void Write()
        {
            setPointer();

            *(ObjectTracker.FrameObservation*)ptr = objectTracker.GetFrameObservation;
            ptr += Marshal.SizeOf<ObjectTracker.FrameObservation>();

            for (int i = 0; i < 10; i++)
            {
                *(ObjectTracker.OsuObjectsData*)ptr = objectTracker.GetData[i];
                ptr += Marshal.SizeOf<ObjectTracker.OsuObjectsData>();
            }

            for (int i = 0; i < 10; i++)
            {
                *(RewardTracker.RewardEvent*)ptr = rewardTracker.GetRewards[i];
                ptr += Marshal.SizeOf<RewardTracker.RewardEvent>();
            }

        }


        public void Dispose()
        {
            // Send a end frame0
            setPointer();
            ObjectTracker.FrameObservation doneFrame = new();
            *(ObjectTracker.FrameObservation*)ptr = doneFrame;

            accessor?.Dispose();
            mmf?.Dispose();
        }
    }
}
