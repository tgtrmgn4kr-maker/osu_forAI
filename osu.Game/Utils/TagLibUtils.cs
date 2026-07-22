// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using TagLib;
using File = TagLib.File;

namespace osu.Game.Utils
{
    public class TagLibUtils
    {

#pragma warning disable IDE0001 // Simplify Names
#pragma warning disable IDE0001 // Simplify Names
        /// <summary>
        /// Creates a <see cref="TagLib.File"/> with culture-invariant MIME type detection, based on stream data.
        /// </summary>
        /// <returns>The <see cref="TagLib.File"/> created.</returns>
        public static File GetTagLibFile(string filename, Stream stream)
#pragma warning restore IDE0001 // Simplify Names
#pragma warning restore IDE0001 // Simplify Names
        {
            var fileAbstraction = new StreamFileAbstraction(filename, stream);

            return File.Create(fileAbstraction, getMimeType(fileAbstraction.Name), ReadStyle.Average | ReadStyle.PictureLazy);
        }


#pragma warning disable IDE0001 // Simplify Names
#pragma warning disable IDE0001 // Simplify Names
        /// <summary>
        /// Creates a <see cref="TagLib.File"/> with culture-invariant MIME type detection based on a file on disk.
        /// </summary>
        /// <param name="filePath">The full path of the file to be created.</param>
        /// <returns>The <see cref="TagLib.File"/> created.</returns>
        public static File GetTagLibFile(string filePath) =>
            File.Create(filePath, getMimeType(filePath), ReadStyle.Average | ReadStyle.PictureLazy);
#pragma warning restore IDE0001 // Simplify Names
#pragma warning restore IDE0001 // Simplify Names

        // Manual MIME type resolution to avoid culture variance (ie. https://github.com/ppy/osu/issues/32962)
        private static string getMimeType(string fileName) => @"taglib/" + Path.GetExtension(fileName.ToLowerInvariant()).TrimStart('.');

        private class StreamFileAbstraction : File.IFileAbstraction
        {
            public StreamFileAbstraction(string filename, Stream fileStream)
            {
                ReadStream = fileStream;
                Name = filename;
            }

            public string Name { get; }

            public Stream ReadStream { get; }
            public Stream WriteStream => ReadStream;

            public void CloseStream(Stream stream)
            {
                ArgumentNullException.ThrowIfNull(stream);

                stream.Close();
            }
        }
    }
}
