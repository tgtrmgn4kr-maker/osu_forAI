// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuReplayRecorder : ReplayRecorder<OsuAction>
    {
        public byte GetHitButton { get; private set; }

        public OsuReplayRecorder(Score score)
            : base(score)
        {
        }

        public class RingBuffer<T> : IEnumerable<T>
        {
            private readonly T[] buffer;
            private int head;
            private int tail;

            public int Capacity => buffer.Length;
            public int Count { get; private set; }
            public bool IsFull => Count == Capacity;
            public bool IsEmpty => Count == 0;
            public RingBuffer(int Capacity)
            {
                if (Capacity <= 0)
                    throw new ArgumentOutOfRangeException(nameof(Capacity), "Capacity must be greater than zero.");

                buffer = new T[Capacity];
                Count = 0;
            }
            public void Write(T item)
            {
                buffer[head] = item;
                head = (head + 1) % Capacity;

                if (IsFull)
                    tail = (tail + 1) % Capacity;
                else
                    Count++;
            }
            public T Read()
            {
                if (IsEmpty)
                    throw new InvalidOperationException("Buffer is empty.");

                T item = buffer[tail];
                buffer[tail] = default!;
                tail = (tail + 1) % Capacity;
                Count--;

                return item;
            }
            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                    yield return buffer[(tail + i) % Capacity];
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<OsuAction> actions, ReplayFrame previousFrame)
        {
            // Combined two button in one byte
            byte Button = 0;
            if (actions.Contains(OsuAction.LeftButton))
                Button |= 1;
            if (actions.Contains(OsuAction.RightButton))
                Button |= 1 << 1;

            // Make sure both the buttons are updated
            GetHitButton = Button;
            return new OsuReplayFrame(Time.Current, mousePosition, actions.ToArray());
        }
    }
}
