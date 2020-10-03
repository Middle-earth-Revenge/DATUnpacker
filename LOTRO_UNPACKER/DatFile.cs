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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DAT_UNPACKER
{
    /// <summary>
    /// Extract options enumerable
    /// </summary>
    [Flags]
    public enum Options
    {
        None = 0x0,
        LoadWav = 0x1,
        LoadOgg = 0x2,
        LoadJpg = 0x4,
        LoadDds = 0x8,
        LoadHks = 0x10,
        // custom formats reserved
        LoadBin = 0x100,
        ExtractSelectedFile = 0x200,
        GenerateFileList = 0x400,
        ExtractRawFile = 0x800,
        LoadAllFiles = LoadWav | LoadOgg | LoadJpg | LoadDds | LoadBin | LoadHks
    }

    /// <summary>
    /// Turbine .dat file
    /// </summary>
    public class DatFile : IDisposable
    {
        private long _cachedSize;
        private bool _disposed;
        private int _handle;
        private static readonly bool[] HandleAllocArray = new bool[0x40];
        private static readonly object HandleAllocArrayLock = new object();
        private readonly bool _isReadOnly;
        private SubfileCollection _subfileCollection;

        public Dictionary<int, Subfile> Files;

        static DatFile()
        {
            for (int i = 0; i < HandleAllocArray.Length; i++)
            {
                HandleAllocArray[i] = false;
            }
        }

        private DatFile()
        {
            _subfileCollection = null;
            _isReadOnly = true;
            _handle = -1;
            _cachedSize = -1;
            _disposed = false;
            Files = new Dictionary<int, Subfile>();
        }

        /// <summary>
        /// Load files to collection
        /// </summary>
        public void Load()
        {
            foreach (Subfile s in _subfileCollection)
            {
                Files.Add(s.DataID, s);
            }
        }

        public Subfile this[int index]
        {
            get
            {
                return Files[index];
            }
        }

        /// <summary>
        /// Allocate handle
        /// </summary>
        /// <returns>Handle</returns>
        private static int AllocHandle()
        {
            lock (HandleAllocArrayLock)
            {
                for (int i = 0; i < HandleAllocArray.Length; i++)
                {
                    if (!HandleAllocArray[i])
                    {
                        HandleAllocArray[i] = true;
                        return i;
                    }
                }

                throw new DatFileException("Too many dat files are already open");
            }
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
                if (disposing)
                {
                    DatExport.CloseDatFile(_handle);
                    FreeHandle(_handle);
                }

                _disposed = true;
            }
        }

        ~DatFile()
        {
            Dispose(false);
        }

        /// <summary>
        /// Release handle
        /// </summary>
        /// <param name="handle">Handle</param>
        private static void FreeHandle(int handle)
        {
            HandleAllocArray[handle] = false;
        }

        /// <summary>
        /// Open .dat file
        /// </summary>
        /// <param name="initParams">Initial parameters</param>
        /// <returns>Datfile</returns>
        public static DatFile OpenExisting(DatFileInitParams initParams)
        {
            DatFile resultFile;
            int handle = AllocHandle();

            try
            {
                var df = new DatFile 
                {
                    _handle = handle,
                };

                var datIdStamp = new StringBuilder(0x40);
                var firstIterGuid = new StringBuilder(0x40);
                uint flags = 130;

                if (initParams.IsReadOnly)
                    flags |= 4;

                int masterMap, blockSize, versionNumDatFile, versionNumGameData;
                ulong datFileID;
                if (DatExport.OpenDatFile(handle, initParams.FileName, flags, out masterMap, out blockSize, out versionNumDatFile, out versionNumGameData, out datFileID, datIdStamp, firstIterGuid) == -1)
                    throw new DatFileException("Unable to open file [ " + initParams.FileName + " ]");

                df._subfileCollection = new SubfileCollection(df);
                
                resultFile = df;
                df.Load();
            }
            catch
            {
                FreeHandle(handle);
                throw;
            }

            return resultFile;
        }

        /// <summary>
        /// Cached size of file
        /// </summary>
        public long CachedSize
        {
            get
            {
                if (_cachedSize == -1)
                {
                    _cachedSize = 0;

                    if (_isReadOnly)
                    {
                        foreach (Subfile subfile in _subfileCollection)
                        {
                            _cachedSize += subfile.Size;
                        }
                    }
                }

                return _cachedSize;
            }
        }

        /// <summary>
        /// Handle
        /// </summary>
        internal int Handle
        {
            get
            {
                return _handle;
            }
        }

        /// <summary>
        /// Internal collection of files
        /// </summary>
        public SubfileCollection Subfiles
        {
            get
            {
                if (!_isReadOnly)
                    throw new DatFileException("Unable to iterate the subfiles of a writable dat file");

                return _subfileCollection;
            }
        }
    }

    /// <summary>
    /// Custom exception
    /// </summary>
    internal class DatFileException : ApplicationException
    {
        public DatFileException()
        {
        }

        public DatFileException(string message) : base(message)
        {
        }

        public DatFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DatFileException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Input .date file parameters
    /// </summary>
    public class DatFileInitParams : ICloneable
    {
        private string _fullFileName = string.Empty;
        private bool _readOnly = true;

        public DatFileInitParams(string filename, bool readOnly)
        {
            _fullFileName = filename;
            _readOnly = readOnly;
        }

        public DatFileInitParams()
        {
        }

        public object Clone()
        {
            return new DatFileInitParams 
            {
                _readOnly = _readOnly, _fullFileName = _fullFileName.Clone() as string 
            };
        }

        public string FileName
        {
            get
            {
                return _fullFileName;
            }
            set
            {
                _fullFileName = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                _readOnly = value;
            }
        }
    }
}