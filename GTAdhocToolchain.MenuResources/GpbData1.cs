// Copyright (c) 2026 Nenkai
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using Syroot.BinaryData;
using Syroot.BinaryData.Memory;

using GTAdhocToolchain.Core;

namespace GTAdhocToolchain.Menu.Resources;

/// <summary>
/// Gpb1, used in GT4.
/// </summary>
public class GpbData1 : GpbBase
{
    public const int HeaderSize = 0x10;
    public const int EntrySize = 0x08;

    public override void Read(string fileName)
    {
        using var fs = new FileStream(fileName, FileMode.Open);
        using var bs = new BinaryStream(fs, ByteConverter.Big);

        string magic = bs.ReadString(4);
        if (magic == "1bpg")
            bs.ByteConverter = ByteConverter.Big;
        else if (magic == "gpb1")
            bs.ByteConverter = ByteConverter.Little;
        else
            throw new Exception($"Unsupported gpb with magic {magic}.");

        bs.ReadInt32(); // Relocation ptr
        bs.ReadInt32(); // Empty
        int entryCount = bs.ReadInt32();

        // Unlike GPB2 and later, entries only have a path/name and a dataOffset.
        // Each buffer’s size is the difference between consecutive offsets, or if the last buffer -- the end of the file.
        var fileNames = new string[entryCount];
        var dataOffsets = new int[entryCount];

        for (int i = 0; i < entryCount; i++)
        {
            bs.Position = HeaderSize + (i * EntrySize);

            int fileNameOffset = bs.ReadInt32();
            dataOffsets[i] = bs.ReadInt32();

            bs.Position = fileNameOffset;
            fileNames[i] = bs.ReadString(StringCoding.ZeroTerminated);
        }

        // Write the buffers
        for (int i = 0; i < entryCount; i++)
        {
            int start = dataOffsets[i];
            int end = i + 1 < entryCount ? dataOffsets[i + 1] : (int)fs.Length;

            bs.Position = start;
            Files.Add(new GpbPair()
            {
                FileName = fileNames[i],
                FileData = bs.ReadBytes(end - start),
            });
        }
    }
}
