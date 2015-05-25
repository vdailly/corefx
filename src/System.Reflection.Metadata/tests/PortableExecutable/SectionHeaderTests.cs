﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

using TestUtilities;

using Xunit;

namespace System.Reflection.Metadata.Tests.PortableExecutable
{
    public class SectionHeaderTests
    {
        [Theory]
        [InlineData(".debug", 0, 0, 0x5C, 0x152, 0, 0, 0, 0, 0x42100048)] // no pad, initialized, discardable, 1-byte align, read-only
        [InlineData(".drectve", 0, 0, 26, 0x12C, 0, 0, 0, 0, 0x00100A00)] // info, linker-remove, 1-byte align
        public void Ctor(
            string name,
            int virtualSize,
            int virtualAddress,
            int sizeOfRawData,
            int ptrToRawData,
            int ptrToRelocations,
            int ptrToLineNumbers,
            ushort numRelocations,
            ushort numLineNumbers,
            SectionCharacteristics characteristics)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write(PadSectionName(name));
            writer.Write(virtualSize);
            writer.Write(virtualAddress);
            writer.Write(sizeOfRawData);
            writer.Write(ptrToRawData);
            writer.Write(ptrToRelocations);
            writer.Write(ptrToLineNumbers);
            writer.Write(numRelocations);
            writer.Write(numLineNumbers);
            writer.Write((uint) characteristics);
            writer.Dispose();

            stream.Position = 0;
            var reader = new PEBinaryReader(stream, (int) stream.Length);

            var header = new SectionHeader(ref reader);

            AssertEx.AreEqual(name, header.Name);
            AssertEx.AreEqual(virtualSize, header.VirtualSize);
            AssertEx.AreEqual(virtualAddress, header.VirtualAddress);
            AssertEx.AreEqual(sizeOfRawData, header.SizeOfRawData);
            AssertEx.AreEqual(ptrToRawData, header.PointerToRawData);
            AssertEx.AreEqual(ptrToLineNumbers, header.PointerToLineNumbers);
            AssertEx.AreEqual(numRelocations, header.NumberOfRelocations);
            AssertEx.AreEqual(numLineNumbers, header.NumberOfLineNumbers);
            AssertEx.AreEqual(characteristics, header.SectionCharacteristics);
        }

        private static byte[] PadSectionName(string name)
        {
            var nameBytes = Encoding.UTF8.GetBytes(name);
            Assert.True(name.Length <= PEFileConstants.SizeofSectionName);

            var bytes = new byte[PEFileConstants.SizeofSectionName];
            Buffer.BlockCopy(nameBytes, 0, bytes, 0, nameBytes.Length);
            return bytes;
        }
    }
}
