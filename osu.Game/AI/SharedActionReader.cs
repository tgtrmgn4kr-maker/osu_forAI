// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace osu.Game.AI
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ActionData
    {
        public float CursorX;
        public float CursorY;

        public byte K1;
        public byte K2;
    }

    public unsafe class SharedActionReader : IDisposable
    {
        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;

        private byte* ptr;

        public SharedActionReader(string name = "Osu_Action")
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("This library only supports Windows.");

            int size = sizeof(ActionData);
            mmf = MemoryMappedFile.CreateOrOpen(name, size);
            accessor = mmf.CreateViewAccessor();
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

        }
        public ActionData Read()
        {
            // 將 byte* 轉成 Action* 後取值
            return *(ActionData*)ptr;
        }

        public void Dispose()
        {
            accessor?.Dispose();
            mmf?.Dispose();
        }
    }

}
