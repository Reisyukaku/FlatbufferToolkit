using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FlatbufferToolkit
{
    public class FileAccessor
    {
        public string Name;
        public byte[] Bytes;

        public FileAccessor(string name, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
        }

        public FileAccessor(string filepath)
        {
            Name = Path.GetFileNameWithoutExtension(filepath);
            Bytes = File.ReadAllBytes(filepath);
        }
    }
}
