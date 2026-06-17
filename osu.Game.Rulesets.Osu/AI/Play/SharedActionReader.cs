// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using osuTK;

namespace osu.Game.Rulesets.Osu.AI.Play
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ActionData
    {
        public float CursorX;
        public float CursorY;

        public byte K1;
        public byte K2;
    }
    public struct OsuActionData
    {
        public Vector2 CursorPosition;
        public List<OsuAction> OsuActions;
    }

    public unsafe class SharedActionReader : IDisposable
    {
        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;

        private byte* ptr;

        public SharedActionReader(string name = "osu_action")
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("This library only supports Windows.");

            int size = Marshal.SizeOf<ActionData>();
            mmf = MemoryMappedFile.CreateOrOpen(name, size);
            accessor = mmf.CreateViewAccessor();
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

        }
        public OsuActionData Read()
        {
            // 將 byte* 轉成 Action* 後取值
            var actionData = *(ActionData*)ptr;

            var action = new List<OsuAction>();

            if (actionData.K1 != 0)
                action.Add(OsuAction.LeftButton);
            if (actionData.K2 != 0)
                action.Add(OsuAction.RightButton);

            return new OsuActionData
            {
                CursorPosition = new Vector2(
                    actionData.CursorX * 256 + 256,
                    actionData.CursorY * 192 + 192
                ),
                OsuActions = action
            };
        }

        public void Dispose()
        {
            accessor?.Dispose();
            mmf?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
