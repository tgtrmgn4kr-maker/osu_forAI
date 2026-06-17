// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.AI.Learn
{
    public class HumanPlayRecorder
    {
        public FileStream F;
        private ObjectTracker objectTracker;
        private RewardTracker rewardTracker;
        private OsuReplayRecorder osuReplayRecorder;

        public HumanPlayRecorder(ObjectTracker objectTracker, RewardTracker rewardTracker, OsuReplayRecorder osuReplayRecorder)
        {
            F = new(@"D:\Programming\Data\TrainingData.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            this.objectTracker = objectTracker;
            this.rewardTracker = rewardTracker;
            this.osuReplayRecorder = osuReplayRecorder;
        }

        public void WriteData()
        {
            F.Seek(0, SeekOrigin.End);
            byte[] data = System.Text.Encoding.ASCII.GetBytes($"");
            F.Write(data, 0, data.Length);
        }

        public void Close()
        {
            F.Close();
        }

    }
}
