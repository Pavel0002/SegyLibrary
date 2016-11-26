using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Unplugged.IbmBits;

namespace SegyLibrary
{
    public class SegyBinHeader
    {
        public SampleFormat SampleFormat { get; private set; }
        public ByteOrder ByteOrder { get; private set; }
        public int TraceLength { get; private set; }
        public int NumOfExtTextHeaders { get; private set; }
        public short SampleInterval { get; private set; }
        public SegyBinHeader(string fileName, int numOfExtTextHeaders = 0)
        {
            DefineNumberFormats(fileName);
            if (!(SampleFormat == SampleFormat.IBM || SampleFormat == SampleFormat.IEEE || SampleFormat == SampleFormat.INT8 || SampleFormat == SampleFormat.INT16 || SampleFormat == SampleFormat.INT32))
            {
                throw new ArgumentException("File format is not supported!");
            }
            this.NumOfExtTextHeaders = numOfExtTextHeaders;
            //Все остальные поля заполняются через FillBinaryHeader (т.к. функции для чтения полей хранятся не в данном классе)!!!
            //Такого неудобства можно было бы избежать, воспользовавшись техникой вложенных классов. но не понятно, лучше ли это было...
        }
        public void FillBinaryHeader(Func<short> Fields16ReadFunc, Func<float> SamplesReadFunk, FileStream InSegyStream)
        {
            //тут лучше считать все поля подряд и последовательно, но пока так.
            InSegyStream.Seek(SegyBinHeaderPositions.SampleIntervalAddress, SeekOrigin.Begin);
            SampleInterval = Fields16ReadFunc();
            InSegyStream.Seek(SegyBinHeaderPositions.TraceLength, SeekOrigin.Begin);
            TraceLength = Fields16ReadFunc();
            InSegyStream.Seek(SegyBinHeaderPositions.NumOfExtTextHeaders, SeekOrigin.Begin);
            NumOfExtTextHeaders = Fields16ReadFunc();
            if (NumOfExtTextHeaders == -1 && this.NumOfExtTextHeaders == 0)
            {
                throw new ArgumentException("Number of Extended Textual Headers is expected in binary header or as constructor parameter");
            }
        }
        private void DefineNumberFormats(string fileName)
        {
            using (FileStream inSgyStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader inSgyBinReader = new BinaryReader(inSgyStream))
                {
                    inSgyStream.Seek(SegyBinHeaderPositions.NumType, SeekOrigin.Begin);
                    short numtypeLittleEndian = inSgyBinReader.ReadInt16();
                    if (numtypeLittleEndian <= 8 && numtypeLittleEndian > 0)
                    {
                        SampleFormat = (SampleFormat)numtypeLittleEndian;
                        ByteOrder = ByteOrder.LittleEndian;
                        return;
                    }
                    //byte[] bar = BitConverter.GetBytes(numtype);
                    //Array.Reverse(bar);
                    //numtype = BitConverter.ToInt16(bar, 0);
                    inSgyStream.Seek(SegyBinHeaderPositions.NumType, SeekOrigin.Begin);
                    short numtypeBigEndian = inSgyBinReader.ReadInt16BigEndian();
                    if (numtypeBigEndian <= 8 && numtypeBigEndian > 0)
                    {
                        SampleFormat = (SampleFormat)numtypeBigEndian;
                        ByteOrder = ByteOrder.BigEndian;
                        return;
                    }

                    throw new System.ArgumentException($"Can't define file format by numbers {numtypeBigEndian}, {numtypeLittleEndian}");
                }
            }
        }
    }
}
