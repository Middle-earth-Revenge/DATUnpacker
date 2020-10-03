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
using System.IO;
using System.Reflection;

namespace DAT_UNPACKER
{
    class Program
    {
        /// <summary>
        /// Print header
        /// </summary>
        /// <param name="writer">Stream to write</param>
        public static void WriteHeader(StreamWriter writer)
        {
            for (int i = 0; i < 300; i++)
            {
                writer.Write("#");
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Print start log
        /// </summary>
        /// <param name="writer">Stream to write</param>
        /// <param name="date">Date</param>
        public static void StartLog(StreamWriter writer, DateTime date)
        {
            WriteHeader(writer);
            writer.WriteLine("DAT_UNPACKER log started {0}", date);
        }

        /// <summary>
        /// Print end log
        /// </summary>
        /// <param name="writer">Stream to write</param>
        public static void EndLog(StreamWriter writer)
        {
            writer.WriteLine("DAT_UNPACKER log ended {0}", DateTime.Now);
            WriteHeader(writer);
        }

        /// <summary>
        /// Print information
        /// </summary>
        /// <param name="info">Information</param>
        /// <param name="writer">Stream to write</param>
        /// <param name="console">Write to console</param>
        public static void WriteInfo(string info, StreamWriter writer, bool console)
        {
            WriteInfo(info, writer, console, true);
        }

        /// <summary>
        /// Print information
        /// </summary>
        /// <param name="info">Information</param>
        /// <param name="writer">Stream to write</param>
        /// <param name="console">Write to console</param>
        /// <param name="newLine">Add new line</param>
        public static void WriteInfo(string info, StreamWriter writer, bool console, bool newLine)
        {
            if (newLine)
                writer.WriteLine(info);
            else
                writer.Write(info);

            if (console)
            {
                if (newLine)
                    Console.WriteLine(info);
                else
                    Console.Write(info);
            }
        }

        /// <summary>
        /// Usage helper
        /// </summary>
        static readonly string[] FormatDescription = new[]
            {
                "Bad format. Usage: DAT_UNPACKER.exe <file_name> <flags> <file_id>",
                "Flags and file_id are optional parameters. File_id is presented in hex format (example: A0000394). Available flags:",
                "1 - extract wav files, 2 - extract ogg files, 4 - extract jpg files, 8 - extract dds files, 16 - extract hks files, 256 - extract raw files, 512 - extract selected file (must be used with file_id), 1024 - no extracting, just generating file list.",
                "You have to summ flags for extracting several types of files.",
                "Example: you want to extract only wav, ogg and jpg files. You should use flags = 7. By default, flags = 271."
            };

        /// <summary>
        /// Print helper
        /// </summary>
        /// <param name="writer">Stream to write</param>
        /// <param name="onlyConsole">Only console output</param>
        static void PrintFormat(StreamWriter writer, bool onlyConsole)
        {
            foreach (string t in FormatDescription)
            {
                if (onlyConsole)
                    Console.WriteLine(t);
                else if (writer != null)
                    WriteInfo(t, writer, true);
            }
        }

        /// <summary>
        /// Main entry
        /// </summary>
        /// <param name="args">Arguments</param>
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintFormat(null, true);
                return;
            }

            DateTime now = DateTime.Now;
            string name = args[0];
            string path = name.Substring(0, name.IndexOf('.'));

            if (!Directory.Exists(string.Format("data\\{0}",path)))
                Directory.CreateDirectory(string.Format("data\\{0}", path));

            using (var writer = new StreamWriter(string.Format("data\\{0}\\report_{1}_{2}_{3}_{4}_{5}_{6}.txt", path, now.Day, now.Month, now.Year, now.Hour, now.Minute, now.Second), false))
            {
                StartLog(writer, now);

                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine("DAT_UNPACKER {0}.{1}.{2} written by Dancing_on_a_rock_hacker (dancingonarockhacker@gmail.com)", version.Major, version.Minor, version.Build);

                if (args.Length < 1)
                {
                    PrintFormat(writer, false);
                    EndLog(writer);
                    return;
                }

                if (!File.Exists("zlib1T.dll"))
                {
                    WriteInfo("Please move DAT_UNPACKER.exe and datexport.dll into your game folder.", writer, true);
                    EndLog(writer);
                    return;
                }

                if (!File.Exists(name))
                {
                    WriteInfo(string.Format("File {0} was not found.", name), writer, true);
                    EndLog(writer);
                    return;
                }

                Options options = Options.LoadAllFiles;

                if (args.Length > 1)
                {
                    string option = args[1];

                    int flags;
                    if (int.TryParse(option, out flags))
                        options = (Options) flags;
                }

                Console.Write("Opening file {0}...", name);
                DatFile file = DatFile.OpenExisting(new DatFileInitParams(name, true));
                Console.WriteLine("done");

                Console.Write("Loading dictionary...");
                Dictionary<int, int> dic = FileDictionary.LoadDictionary(path);
                Console.WriteLine("done");

                if ((options & Options.ExtractSelectedFile) != 0 && args.Length > 2)
                {
                    string fileId = args[2];
                    int fileID = Convert.ToInt32(fileId, 16);

                    WriteInfo(string.Format("Extracting file {0}...", fileID.ToString("X2")), writer, true);

                    Options opt = Options.LoadAllFiles;

                    if ((options & Options.ExtractRawFile) != 0)
                        opt |= Options.ExtractRawFile;

                    try
                    {
                        file.Files[fileID].Extract(path, opt, writer, ref dic);
                        WriteInfo("File was extracted.", writer, true);
                    }
                    catch
                    {
                        WriteInfo("File was not found.", writer, true);
                    }

                    EndLog(writer);
                    return;
                }
                
                if ((options & Options.GenerateFileList) != 0)
                {
                    Console.Write("Generating filelist...");

                    if (!Directory.Exists("filelist"))
                        Directory.CreateDirectory("filelist");

                    using (var sw = new StreamWriter(string.Format("filelist\\{0}_filelist.txt", path), false))
                    {
                        foreach (Subfile s in file.Subfiles)
                        {
                            sw.WriteLine("File {0} with Size = {1} bytes, Compressed: {2}, Version = {3}", s.DataID.ToString("X2"), s.Size, s.IsCompressed, s.Version);
                        }
                    }

                    Console.WriteLine("done");
                    
                    EndLog(writer);
                    return;
                }

                DateTime before = DateTime.Now;
                Console.Write("Extracting...");

                int count = 0;

                foreach (var s in file.Subfiles)
                {
                    if (s.Extract(path, options, writer, ref dic))
                        count++;
                }

                DeleteEmptySubFolders(Directory.GetCurrentDirectory());

                DateTime after = DateTime.Now;
                Console.WriteLine("done");

                WriteInfo(string.Format("Total {0} files were extracted from {1} in ~{2} seconds.", count, name, (int)(after - before).TotalSeconds), writer, true);

                Console.Write("Saving dictionary...");
                FileDictionary.SaveDictionary(dic, path);
                Console.WriteLine("done");

                EndLog(writer);
            }
        }

        /// <summary>
        /// Delete empty subfolders after unpacking
        /// </summary>
        /// <param name="rootDirectory">Root directory</param>
        /// <param name="deleteEmptyFiles">Delete empty files</param>
        private static void DeleteEmptySubFolders(string rootDirectory, bool deleteEmptyFiles = false)
        {
            DirectoryInfo di = new DirectoryInfo(rootDirectory);

            foreach (DirectoryInfo diSon in di.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                FileInfo[] fis = diSon.GetFiles("*.*", SearchOption.AllDirectories);

                if (fis.Length < 1)
                {
                    diSon.Delete(true);
                }
                else
                {
                    if (deleteEmptyFiles)
                    {
                        long total = 0;
                        foreach (FileInfo fi in fis)
                        {
                            total = total + fi.Length;
                            if (total > 0)
                            {
                                break;
                            }
                        }

                        if (total == 0)
                        {
                            diSon.Delete(true);
                            continue;
                        }
                    }

                    DeleteEmptySubFolders(diSon.FullName, deleteEmptyFiles);
                }
            }
        }
    }
}