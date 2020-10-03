/*  LOTRO/DDO DAT FILES UNPACKER
    Copyright(C) 2011-2012 Dancing_on_a_rock_hacker (dancingonarockhacker@gmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.If not, see<http://www.gnu.org/licenses/>.
*/
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DAT_UNPACKER
{
    /// <summary>
    /// Turbine's DatExport.dll P/Invoke wrapper
    /// </summary>
    internal class DatExport
    {
        public const uint DcofCreate = 8;
        public const uint DcofCreateIfNeeded = 0x10;
        public const uint DcofExpandable = 2;
        public const uint DcofFreeThreaded = 0x40;
        public const uint DcofJournalled = 0x100;
        public const uint DcofLoadIterations = 0x80;
        public const uint DcofOptionalFile = 0x20;
        public const uint DcofReadOnly = 4;
        public const uint DcofSkipIndexCheck = 0x200;
        public const uint DcofUseLRU = 1;

        [DllImport("DatExport.dll")]
        public static extern void CloseDatFile(int handle);

        [DllImport("DatExport.dll")]
        public static extern int GetNumSubfiles(int handle);

        [DllImport("DatExport.dll")]
        public static extern byte GetSubfileCompressionFlag(int handle, int id);

        [DllImport("DatExport.dll")]
        public static extern int GetSubfileData(int handle, int did, IntPtr buffer, int writeOffset, out int version);

        [DllImport("DatExport.dll")]
        public static extern void GetSubfileSizes(int handle, out int did, out int size, out int iteration, int offset, int count);

        [DllImport("DatExport.dll")]
        public static extern int GetSubfileVersion(int handle, int did);

        [DllImport("DatExport.dll", EntryPoint="OpenDatFileEx2")]
        public static extern int OpenDatFile(int handle, string fileName, uint flags, out int didMasterMap, out int blockSize,
            out int vnumDatFile, out int vnumGameData, out ulong datFileID, [MarshalAs(UnmanagedType.LPStr)] StringBuilder datIdStamp, 
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder firstIterGuid);

        [DllImport("zlib1T")]
        public static extern int uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);
    }
}