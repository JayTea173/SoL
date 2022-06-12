using System.IO;
using System.Threading;

namespace SoL.Serialization
{
    public static class SerializationExtensions
    {
        public static BinaryWriter OpenWriteToFile(string filePath) =>
            new BinaryWriter(File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite));
        public static BinaryReader OpenReadFromFile(string filePath) =>
            new BinaryReader(File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite));
    }
}