using System;
using System.IO;
using System.Linq;

namespace Unplugged.IbmBits
{
    public static class BinaryWriterExtensionMethods
    {
        /// <summary>
        /// Writes the given Unicode string as an 8-bit EBCDIC encoded character string
        /// </summary>
        public static void WriteEbcdic(this BinaryWriter writer, string value)
        {
            var bytes = IbmConverter.GetBytes(value);
            writer.Write(bytes);
        }

        /// <summary>
        /// Writes a big endian encoded Int16 to the stream
        /// </summary>
        public static void WriteBigEndian(this BinaryWriter writer, short value)
        {
            var bytes = IbmConverter.GetBytes(value);
            writer.Write(bytes);
        }

        /// <summary>
        /// Writes a big endian encoded Int32 to the stream
        /// </summary>
        public static void WriteBigEndian(this BinaryWriter writer, int value)
        {
            var bytes = IbmConverter.GetBytes(value);
            writer.Write(bytes);
        }

        /// <summary>
        /// Writes an IBM System/360 Floating Point encoded Single to the stream
        /// </summary>
        public static void WriteIbmSingle(this BinaryWriter writer, float value)
        {
            var bytes = IbmConverter.GetBytes(value);
            writer.Write(bytes);
        }

        /// <summary>
        /// Writes a packed decimal to the stream
        /// </summary>
        public static void WriteIbmPackedDecimal(this BinaryWriter writer, decimal value)
        {
            var bytes = IbmConverter.GetBytes(value);
            writer.Write(bytes);
        }

        //для двух последующих методов нужно сделать методы записи и протестировать все.
        //проверить правильность последних двух, особенно второй
        /// <summary>
        /// Writes a single precision 32-bit floating point number from the stream 
        /// that has been encoded in IEEE BigEndian Floating Point format
        /// </summary>
        /// <returns>IEEE formatted single precision floating point</returns>
        public static void WriteSingleBigEndian(this BinaryWriter writer, float value)
        {
            var bytes = BitConverter.GetBytes(value).Reverse().ToArray();
            writer.Write(bytes);
        }

        //попробовать заменить данный метод методом из базовой библиотеки
        public static void WriteSignedByteFromSingle(this BinaryWriter writer, float value)
        {
            /*
            byte toWrite = value > 0 ? (byte)value : (byte)(value + 256); 
            writer.Write(toWrite);
            */
            writer.Write((sbyte)value);
        }
        
        /// <summary>
        /// Writes a big endian encoded Int16 to the stream
        /// </summary>
        public static void WriteBigEndian16FromSingle(this BinaryWriter writer, float value)
        {
            var bytes = IbmConverter.GetBytes((short)value);
            writer.Write(bytes);
        }

        /// <summary>
        /// Writes a big endian encoded Int32 to the stream
        /// </summary>
        public static void WriteBigEndian32FromSingle(this BinaryWriter writer, float value)
        {
            var bytes = IbmConverter.GetBytes((int)value);
            writer.Write(bytes);
        }

        public static void WriteInt32FromSingle(this BinaryWriter writer, float value)
        {
            writer.Write((int)value);
        }

        public static void WriteInt16FromSingle(this BinaryWriter writer, float value)
        {
            writer.Write((short)value);
        }
    }
}
