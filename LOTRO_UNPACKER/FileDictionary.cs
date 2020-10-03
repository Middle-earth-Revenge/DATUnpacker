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
using System.Collections.Generic;
using System.IO;

namespace DAT_UNPACKER
{
    /// <summary>
    /// File dictionary
    /// </summary>
    static class FileDictionary
    {
        /// <summary>
        /// Load dictionary from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<int,int> LoadDictionary(string name)
        {
            if (!Directory.Exists(string.Format("data\\{0}",name)))
                Directory.CreateDirectory(string.Format("data\\{0}", name));

            var dic = new Dictionary<int,int>();

            if (File.Exists(string.Format("data\\{0}\\dictionary.bin", name)))
            {
                using (var br = new BinaryReader(new FileStream(string.Format("data\\{0}\\dictionary.bin", name), FileMode.Open)))
                {
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        int id = br.ReadInt32();
                        int version = br.ReadInt32();

                        if (!dic.ContainsKey(id))
                            dic.Add(id, version);
                    }
                }
            }

            return dic;
        }

        /// <summary>
        /// Save dictionary to file
        /// </summary>
        /// <param name="dic">Dictionary</param>
        /// <param name="name">Name</param>
        public static void SaveDictionary(Dictionary<int, int> dic, string name)
        {
            if (!Directory.Exists(string.Format("data\\{0}", name)))
                Directory.CreateDirectory(string.Format("data\\{0}", name));

            using (var bw = new BinaryWriter(new FileStream(string.Format("data\\{0}\\dictionary.bin", name), FileMode.Create)))
            {
                foreach (KeyValuePair<int, int> kvp in dic)
                {
                    bw.Write(kvp.Key);
                    bw.Write(kvp.Value);
                }
            }
        }
    }
}
