// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using osu.Framework.Logging;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.AI.Learn
{
    public unsafe class HumanPlayRecorder : IDisposable
    {
        private FileStream fs;
        private BinaryWriter writer;
        private OsuReplayRecorder osuReplayRecorder;
        private ObjectTracker objectTracker;
        private RewardTracker rewardTracker;
        private string filePath = @"D:\Programming\Data\OsuTrainingData.bin";

        public HumanPlayRecorder(ObjectTracker objectTracker, RewardTracker rewardTracker, OsuReplayRecorder osuReplayRecorder)
        {
            this.objectTracker = objectTracker;
            this.rewardTracker = rewardTracker;
            this.osuReplayRecorder = osuReplayRecorder;
            fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096 * 1024);
            writer = new BinaryWriter(fs);
        }

        public void WriteStructIntoByte<T>(ref T structure) where T : unmanaged
        {
            int size = Marshal.SizeOf<T>();

            fixed (T* structPtr = &structure)
            {
                var span = new ReadOnlySpan<byte>(structPtr, size);
                fs.Write(span);
            }
        }

        public void WriteData(long frameID)
        {
            var frameObservation =
                objectTracker.GetFrameObservation;

            var objectData =
                objectTracker.GetData;

            var rewardEvents =
                rewardTracker.GetRewards;

            WriteStructIntoByte(ref frameObservation);

            for (int i = 0; i < 10; i++)
            {
                WriteStructIntoByte(ref objectData[i]);
            }
            Logger.Log($"Write TimeToHit: {objectData[0].TimeToHit}");

            for (int i = 0; i < 10; i++)
            {
                WriteStructIntoByte(ref rewardEvents[i]);
            }


            writer.Write(osuReplayRecorder.GetHitButton);
            Logger.Log($"Hit: {osuReplayRecorder.GetHitButton}");
            writer.Write(Encoding.ASCII.GetBytes("End of a frame."));

        }
        public void Dispose()
        {
            writer?.Dispose();
            fs?.Dispose();
        }
    }
}
