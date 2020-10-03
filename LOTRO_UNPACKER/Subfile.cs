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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DAT_UNPACKER
{
    /// <summary>
    /// Turbine subfile
    /// </summary>
    public class Subfile : IDisposable
    {
        private IntPtr _dataPtr = IntPtr.Zero;
        private readonly int _did = -1;
        private bool _disposed;
        private readonly int _handle = -1;
        private readonly bool _isCompressed;
        private bool _isDataLoaded;
        private readonly int _iteration = -1;
        private readonly int _size = -1;
        private readonly int _version = -1;

        internal Subfile(int handle, int did, int size, int iteration, int version)
        {
            _handle = handle;
            _did = did;
            _size = size;
            _iteration = iteration;
            _version = version;
            _isCompressed = 1 == DatExport.GetSubfileCompressionFlag(_handle, _did);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && !IntPtr.Zero.Equals(_dataPtr))
                {
                    Marshal.FreeHGlobal(_dataPtr);
                    _dataPtr = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Determine file type and guess it's extension
        /// </summary>
        /// <param name="buffer">Current header buffer</param>
        /// <param name="category">Category</param>
        /// <param name="headerLength">Length of header buffer</param>
        /// <param name="newHeader">Correct header buffer</param>
        /// <returns></returns>
        public string GetExtension(byte[] buffer, string category, out int headerLength, out byte[] newHeader)
        {
            headerLength = 0;
            newHeader = new byte[] { };
          
            if (category == "0A0")
            {
                headerLength = 8;

                byte h1 = buffer[headerLength];
                byte h2 = buffer[headerLength + 1];
                byte h3 = buffer[headerLength + 2];
                byte h4 = buffer[headerLength + 3];

                if (h1 == 0x52 && h2 == 0x49 && h3 == 0x46 && h4 == 0x46) // wav
                    return "wav";
                
                if (h1 == 0x4F && h2 == 0x67 && h3 == 0x67 && h4 == 0x53) // ogg
                    return "ogg";
            }
            else if (category == "040")
            {
                if (buffer.Length >= 12 && buffer[8] == 0x57 && buffer[9] == 0xE0 && buffer[10] == 0xE0 && buffer[11] == 0x57) // WaaW
                {
                    headerLength = 8;
                    return "hkx";
                }
            }
            else if (category == "050")
            {
                if (buffer.Length >= 21 && buffer[17] == 0x57 && buffer[18] == 0xE0 && buffer[19] == 0xE0 && buffer[20] == 0x57) // WaaW
                {
                    headerLength = 17;
                    return "hkx";
                }
            }
            else if (category == "410" || category == "411")
            {
                if (buffer.Length >= 28 && buffer[24] == 0xFF && buffer[25] == 0xD8 && buffer[26] == 0xFF) // jpeg
                {
                    headerLength = 24;
                    return "jpg";
                }

                if (buffer.Length >= 20 && buffer[16] == 0x44 && buffer[17] == 0x58 && buffer[18] == 0x54) // dxt
                {
                    if (buffer[19] == 0x31) // dxt1
                    {
                        newHeader = new byte[128];

                        newHeader[0] = 0x44; // 'D'
                        newHeader[1] = 0x44; // 'D'
                        newHeader[2] = 0x53; // 'S'
                        newHeader[3] = 0x20; // ' '

                        newHeader[4] = 0x7C;

                        newHeader[8] = 0x7;
                        newHeader[9] = 0x10;

                        newHeader[12] = buffer[12];
                        newHeader[13] = buffer[13];
                        newHeader[14] = buffer[14];
                        newHeader[15] = buffer[15];

                        newHeader[16] = buffer[8];
                        newHeader[17] = buffer[9];
                        newHeader[18] = buffer[10];
                        newHeader[19] = buffer[11];

                        newHeader[76] = 0x20;
                        newHeader[80] = 0x4;

                        newHeader[84] = 0x44; // 'D'
                        newHeader[85] = 0x58; // 'X'
                        newHeader[86] = 0x54; // 'T'
                        newHeader[87] = 0x31; // '1'

                        headerLength = 16;

                        return "dds";
                    }

                    if (buffer[19] == 0x33) // dxt3
                    {
                        newHeader = new byte[128];

                        newHeader[0] = 0x44; // 'D'
                        newHeader[1] = 0x44; // 'D'
                        newHeader[2] = 0x53; // 'S'
                        newHeader[3] = 0x20; // ' '

                        newHeader[4] = 0x7C;

                        newHeader[8] = 0x7;
                        newHeader[9] = 0x10;

                        newHeader[12] = buffer[12];
                        newHeader[13] = buffer[13];
                        newHeader[14] = buffer[14];
                        newHeader[15] = buffer[15];

                        newHeader[16] = buffer[8];
                        newHeader[17] = buffer[9];
                        newHeader[18] = buffer[10];
                        newHeader[19] = buffer[11];

                        newHeader[22] = 0x1;

                        newHeader[76] = 0x20;
                        newHeader[80] = 0x4;

                        newHeader[84] = 0x44; // 'D'
                        newHeader[85] = 0x58; // 'X'
                        newHeader[86] = 0x54; // 'T'
                        newHeader[87] = 0x33; // '3'

                        newHeader[108] = 0x8;
                        newHeader[109] = 0x10;
                        newHeader[110] = 0x40;

                        headerLength = 24;

                        return "dds";
                    }

                    if (buffer[19] == 0x35) // dxt5
                    {
                        newHeader = new byte[128];

                        newHeader[0] = 0x44; // 'D'
                        newHeader[1] = 0x44; // 'D'
                        newHeader[2] = 0x53; // 'S'
                        newHeader[3] = 0x20; // ' '

                        newHeader[4] = 0x7C;

                        newHeader[8] = 0x7;
                        newHeader[9] = 0x10;
                        newHeader[10] = 0x8;

                        newHeader[12] = buffer[12];
                        newHeader[13] = buffer[13];
                        newHeader[14] = buffer[14];
                        newHeader[15] = buffer[15];

                        newHeader[16] = buffer[8];
                        newHeader[17] = buffer[9];
                        newHeader[18] = buffer[10];
                        newHeader[19] = buffer[11];

                        newHeader[22] = 0x1;
                        newHeader[28] = 0x1;

                        newHeader[76] = 0x20;
                        newHeader[80] = 0x4;

                        newHeader[84] = 0x44; // 'D'
                        newHeader[85] = 0x58; // 'X'
                        newHeader[86] = 0x54; // 'T'
                        newHeader[87] = 0x35; // '5'

                        newHeader[88] = 0x20;
                        newHeader[94] = 0xFF;
                        newHeader[97] = 0xFF;
                        newHeader[100] = 0xFF;
                        newHeader[107] = 0xFF;
                        newHeader[109] = 0x10;

                        headerLength = 24;

                        return "dds";
                    }
                }
                else // uncompressed texture
                {
                    headerLength = 24;

                    newHeader = new byte[128];

                    newHeader[0] = 0x44; // 'D'
                    newHeader[1] = 0x44; // 'D'
                    newHeader[2] = 0x53; // 'S'
                    newHeader[3] = 0x20; // ' '

                    newHeader[4] = 0x7C;

                    newHeader[8] = 0x7;
                    newHeader[9] = 0x10;

                    newHeader[12] = buffer[8];
                    newHeader[13] = buffer[9];
                    newHeader[14] = buffer[10];
                    newHeader[15] = buffer[11];
                    newHeader[16] = buffer[12];
                    newHeader[17] = buffer[13];
                    newHeader[18] = buffer[14];
                    newHeader[19] = buffer[15];

                    newHeader[76] = 0x20;
                    newHeader[80] = 0x40;
                    newHeader[88] = 0x18;

                    newHeader[94] = 0xFF;
                    newHeader[97] = 0xFF;
                    newHeader[100] = 0xFF;

                    return "dds";
                }
            }

            return "bin";
        }

        /// <summary>
        /// Extract subfile
        /// </summary>
        /// <param name="name">Path</param>
        /// <param name="options">Options</param>
        /// <param name="writer">Stream to write</param>
        /// <param name="dic">File dictionary</param>
        /// <returns>Result</returns>
        public unsafe bool Extract(string name, Options options, StreamWriter writer, ref Dictionary<int,int> dic)
        {
            var buffer = new byte[Size];
            int id = DataID;
            int size = Size;
            int version = Version;

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ((byte*)Data)[i];
            }

            Marshal.FreeHGlobal(Data);

            if (IsCompressed)
            {
                int decompressedLength = buffer[3] << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0];
                int compressedLength = size - 4;

                var dst = new byte[decompressedLength];
                var src = new byte[compressedLength];

                for (int i = 4; i < size; i++)
                {
                    src[i - 4] = buffer[i];
                }

                DatExport.uncompress(dst, ref decompressedLength, src, compressedLength);
                buffer = dst;
            }

            string filename = id.ToString("X8");

            string extension, path;
            int headerLength;
            byte[] newHeader;
            GetCommonData(filename, name, options, buffer, out extension, out path, out headerLength, out newHeader);

            if (!CheckExtension(options, extension))
                return false;

            int ver;
            if (dic.TryGetValue(id, out ver))
            {
                if (ver == version && File.Exists(path))
                    return false;
            }

            using (var bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                if (newHeader.Length > 0)
                    bw.Write(newHeader);

                for (int i = 0; i < buffer.Length; i++)
                {
                    if (i >= headerLength)
                        bw.Write(buffer[i]);
                }
            }

 
            if (File.Exists(path))
            {
                int vers;
                if (dic.TryGetValue(id, out vers))
                {
                    Program.WriteInfo(string.Format("{0}.{1} was extracted. File was modified.", filename, extension), writer, false);
                    dic[id] = version;
                }
                else
                {
                    Program.WriteInfo(string.Format("{0}.{1} was extracted. File was added.", filename, extension), writer, false);
                    dic.Add(id, version);
                }
            }

            return true;
        }

        /// <summary>
        /// Get common data for file
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="name">Name</param>
        /// <param name="options">Options</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="extension">Extension</param>
        /// <param name="path">Path</param>
        /// <param name="headerLength">Length of new header</param>
        /// <param name="newHeader">New header</param>
        public void GetCommonData(string filename, string name, Options options, byte[] buffer, 
            out string extension, out string path, out int headerLength, out byte[] newHeader)
        {
            string c1 = filename.Substring(0, 1);
            string c2 = filename.Substring(1, 1);
            string c3 = filename.Substring(2, 1);
            string cat = c1 + c2 + c3;

            if (!Directory.Exists(string.Format("data\\{0}\\{1}XXXXXXX", name, c1)))
                Directory.CreateDirectory(string.Format("data\\{0}\\{1}XXXXXXX", name, c1));

            if (!Directory.Exists(string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX", name, c1, c2)))
                Directory.CreateDirectory(string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX", name, c1, c2));

            if (!Directory.Exists(string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX\\{1}{2}{3}XXXXX", name, c1, c2, c3)))
                Directory.CreateDirectory(string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX\\{1}{2}{3}XXXXX", name, c1, c2, c3));

            newHeader = new byte[] { };
            headerLength = 0;

            extension = (options & Options.ExtractRawFile) == 0 ? GetExtension(buffer, cat, out headerLength, out newHeader) : "bin";
            path = string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX\\{1}{2}{3}XXXXX\\{4}.{5}", name, c1, c2, c3, filename, extension);
        }

        /// <summary>
        /// Check extension for options rules
        /// </summary>
        /// <param name="options">Options</param>
        /// <param name="extension">Extension</param>
        /// <returns>Result</returns>
        public bool CheckExtension(Options options, string extension)
        {
            bool writeFile = true;

            switch (extension)
            {
                case "wav":
                    {
                        if ((options & Options.LoadWav) == 0)
                            writeFile = false;

                        break;
                    }
                case "ogg":
                    {
                        if ((options & Options.LoadOgg) == 0)
                            writeFile = false;

                        break;
                    }
                case "jpg":
                    {
                        if ((options & Options.LoadJpg) == 0)
                            writeFile = false;

                        break;
                    }
                case "dds":
                    {
                        if ((options & Options.LoadDds) == 0)
                            writeFile = false;

                        break;
                    }
                case "hks":
                    {
                        if ((options & Options.LoadHks) == 0)
                            writeFile = false;

                        break;
                    }
                case "bin":
                    {
                        if ((options & Options.LoadBin) == 0)
                            writeFile = false;

                        break;
                    }
            }

            return writeFile;
        }
        
        ~Subfile()
        {
            Dispose(false);
        }

        public IntPtr Data
        {
            get
            {
                if (!_isDataLoaded)
                {
                    _dataPtr = Marshal.AllocHGlobal(_size);

                    int version;
                    DatExport.GetSubfileData(_handle, _did, _dataPtr, 0, out version);

                    _isDataLoaded = true;
                }

                return _dataPtr;
            }
        }

        public int DataID
        {
            get
            {
                return _did;
            }
        }

        internal int Handle
        {
            get
            {
                return _handle;
            }
        }

        public bool IsCompressed
        {
            get
            {
                return _isCompressed;
            }
        }

        public int Iteration
        {
            get
            {
                return _iteration;
            }
        }

        public int Size
        {
            get
            {
                return _size;
            }
        }

        public int Version
        {
            get
            {
                return _version;
            }
        }
    }

    /// <summary>
    /// Subfile collection
    /// </summary>
    public class SubfileCollection : IEnumerable<Subfile>
    {
        private readonly int _handle = -1;

        internal SubfileCollection(DatFile df)
        {
            _handle = df.Handle;
        }

        public IEnumerator<Subfile> GetEnumerator()
        {
            return new SubfileEnumerator(_handle);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SubfileEnumerator(_handle);
        }
    }

    /// <summary>
    /// Subfile collection enumerator
    /// </summary>
    public class SubfileEnumerator : IEnumerator<Subfile>
    {
        private int _currentIndex = -1;
        private readonly int _handle = -1;
        private readonly int _numSubFiles;

        internal SubfileEnumerator(int handle)
        {
            _handle = handle;
            _numSubFiles = DatExport.GetNumSubfiles(_handle);
        }

        public void Dispose()
        {
        }

        private Subfile GetCurrentSubfile()
        {
            int did;
            int size;
            int iteration;
            DatExport.GetSubfileSizes(_handle, out did, out size, out iteration, _currentIndex, 1);

            return new Subfile(_handle, did, size, iteration, DatExport.GetSubfileVersion(_handle, did));
        }

        public bool MoveNext()
        {
            if (_currentIndex < (_numSubFiles - 1))
            {
                _currentIndex++;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public Subfile Current
        {
            get
            {
                return GetCurrentSubfile();
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return GetCurrentSubfile();
            }
        }
    }
}