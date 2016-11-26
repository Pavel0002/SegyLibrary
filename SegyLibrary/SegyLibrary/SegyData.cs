using System;
using System.Text;
using System.IO;
using Unplugged.IbmBits;

namespace SegyLibrary
{
    //Возможно стоит улучшить безопасность при записи файлов, создав функции
    //для записи лишь определенного типа (чтение безопасно в этом плане).
    public abstract class SegyData : IDisposable
    {
        public string FilePath {get; private set; }
        public SegyBinHeader BinHeader;
        public CdpLine CdpLine;
        protected FileStream InSgyStream;
        BinaryReader SegyDataBinReader;
        BinaryWriter SegyDataBinWriter;
        public TextHeader TextHeader;
        public int SampleSize;
        protected Func<float> SamplesReadFunc;
        protected Func<short> Fields16ReadFunc;
        protected Func<int> Fields32ReadFunc;
        protected Action<float> SamplesWriteFunc;
        protected Action<short> Fields16WriteFunc;
        protected Action<int> Fields32WriteFunc;
        public int NumOfTraces { get; protected set; }
        public double Filesize{get; private set; }
        public double TraceSize { get; private set; } //Now is not valid for variable tracesize filetypes

        protected SegyData(string filePath, TextHeaderEncoding textHeaderEncoding = TextHeaderEncoding.EBCDIC,  int numOfExtTextHeaders = 0)
        {
            InitializeFields(filePath, numOfExtTextHeaders);
        }

        private void InitializeFields(string filePath, int numOfExtTextHeaders = 0)
        {
            TextHeader = new TextHeader();
            this.FilePath = filePath;
            BinHeader = new SegyBinHeader(filePath, numOfExtTextHeaders);
            InSgyStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            SegyDataBinReader = new BinaryReader(InSgyStream, Encoding.Default, false);
            SegyDataBinWriter = new BinaryWriter(InSgyStream, Encoding.Default, false);
            Filesize = InSgyStream.Length;
            SetSamplesFormat(BinHeader.SampleFormat, BinHeader.ByteOrder, out SamplesReadFunc, out SamplesWriteFunc);
            SetByteOrder(BinHeader.ByteOrder, out Fields16ReadFunc, out Fields32ReadFunc, out Fields16WriteFunc,
                out Fields32WriteFunc);
            BinHeader.FillBinaryHeader(Fields16ReadFunc, SamplesReadFunc, InSgyStream);
            //Size is without header
            TraceSize = (BinHeader.TraceLength*SampleSize);
            NumOfTraces =
                (int)
                    Math.Floor((Filesize - SegyBinHeaderPositions.BinHeaderEnd)/
                               (TraceSize + SegyTraceHeaderPositions.TraceHeaderEnd));
            CdpLine = new CdpLine();
        }

        protected SegyData(string filePath, short sampleIntervalInMicroseconds, short samplesPerTrace, int traceCount,
            bool isLittleEndian, SampleFormat sampleFormat)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                int headerSize = SegyBinHeaderPositions.BinHeaderEnd;
                int traceHeaderSize = SegyTraceHeaderPositions.TraceHeaderEnd;
                int sampleSize;
                switch (sampleFormat)
                {
                    case SampleFormat.INT8:
                        sampleSize = 1;
                        break;
                    case SampleFormat.INT16:
                        sampleSize = 2;
                        break;
                    default:
                        sampleSize = 4;
                        break;
                }
                int fileSize = headerSize + sampleSize*samplesPerTrace*traceCount + traceCount*traceHeaderSize;
                fs.SetLength(fileSize);
                using (var br = new BinaryWriter(fs))
                {
                    fs.Seek(SegyBinHeaderPositions.SampleIntervalAddress, SeekOrigin.Begin);
                    if (isLittleEndian)
                        br.Write(sampleIntervalInMicroseconds);
                    else
                        br.WriteBigEndian(sampleIntervalInMicroseconds);

                    fs.Seek(SegyBinHeaderPositions.SamplesPerTraceAddress, SeekOrigin.Begin);
                    if (isLittleEndian)
                        br.Write((short)samplesPerTrace);
                    else
                        br.WriteBigEndian(samplesPerTrace);

                    fs.Seek(SegyBinHeaderPositions.SampleFormatAddress, SeekOrigin.Begin);
                    if (isLittleEndian)
                        br.Write((short)sampleFormat);
                    else
                        br.WriteBigEndian((short)sampleFormat);
                }
            }
            InitializeFields(filePath);
        }

        private void SetByteOrder(ByteOrder byteOrder, out Func<short> Fields16ReadFunc, out Func<int> Fields32ReadFunc, out Action<short> Fields16WriteFunc, out Action<int> Fields32WriteFunc)
        {
            switch (byteOrder)
            {
                case ByteOrder.LittleEndian:
                    Fields16ReadFunc = SegyDataBinReader.ReadInt16;
                    Fields32ReadFunc = SegyDataBinReader.ReadInt32;
                    Fields16WriteFunc = SegyDataBinWriter.Write;
                    Fields32WriteFunc = SegyDataBinWriter.Write;
                    break;
                case ByteOrder.BigEndian:
                    Fields16ReadFunc = SegyDataBinReader.ReadInt16BigEndian;
                    Fields32ReadFunc = SegyDataBinReader.ReadInt32BigEndian;
                    Fields16WriteFunc = SegyDataBinWriter.WriteBigEndian;
                    Fields32WriteFunc = SegyDataBinWriter.WriteBigEndian;
                    break;
                default:
                    throw new NotSupportedException("Can't define byte order, maybe the file is damaged");
            }
        }

        private void SetSamplesFormat(SampleFormat sampleFormat, ByteOrder byteOrder, out Func<float> SamplesReadFunc, out Action<float> SamplesWriteFunc)
        {
            switch (sampleFormat)
            {
                case SampleFormat.IBM:
                    SamplesReadFunc = SegyDataBinReader.ReadSingleIbm;
                    SamplesWriteFunc = SegyDataBinWriter.WriteIbmSingle;
                    SampleSize = 4;
                    break;
                case SampleFormat.IEEE:
                    if (byteOrder == ByteOrder.LittleEndian)
                    {
                        SamplesReadFunc = SegyDataBinReader.ReadSingle;
                        SamplesWriteFunc = SegyDataBinWriter.Write; 
                    }
                    else
                    {
                        SamplesReadFunc = SegyDataBinReader.ReadSingleBigEndian;
                        SamplesWriteFunc = SegyDataBinWriter.WriteSingleBigEndian;
                    }
                    SampleSize = 4;
                    break;
                case SampleFormat.INT8:
                    SamplesReadFunc = SegyDataBinReader.ReadSignedByteToSingle;
                    SamplesWriteFunc = SegyDataBinWriter.WriteSignedByteFromSingle;
                    SampleSize = 1;
                    break;
                case SampleFormat.INT16:
                    if (byteOrder == ByteOrder.LittleEndian)
                    {
                        SamplesReadFunc = SegyDataBinReader.ReadInt16ToSingle;
                        SamplesWriteFunc = SegyDataBinWriter.WriteInt16FromSingle;
                    }
                    else
                    {
                        SamplesReadFunc = SegyDataBinReader.ReadBigEndian16ToSingle;
                        SamplesWriteFunc = SegyDataBinWriter.WriteBigEndian16FromSingle;
                    }
                    SampleSize = 2;
                    break;
                case SampleFormat.INT32:
                    if (byteOrder == ByteOrder.LittleEndian)
                    {
                        SamplesReadFunc = SegyDataBinReader.ReadInt32ToSingle;
                        SamplesWriteFunc = SegyDataBinWriter.WriteInt32FromSingle;
                    }
                    else
                    {
                        SamplesReadFunc = SegyDataBinReader.ReadBigEndian32ToSingle;
                        SamplesWriteFunc = SegyDataBinWriter.WriteBigEndian32FromSingle;
                    }
                    SampleSize = 4;
                    break;
                default:
                    throw new NotSupportedException("Unsupported sample format");
            }
        }

        public void FillTextHeader(TextHeaderEncoding textHeaderEncoding)
        {
            TextHeader.FillTextHeader(textHeaderEncoding, SegyDataBinReader);
        }

        public void FillCdpLineFromFile()
        {
            CdpLine.FillFromFile(Fields32ReadFunc, Fields16ReadFunc, FilePath, InSgyStream, SegyDataBinReader, SampleSize, BinHeader.NumOfExtTextHeaders);
        }
        public void Dispose()
        {
            SegyDataBinReader.Close();
            InSgyStream.Close();
        }

        protected void WriteTraceHeader(TraceHeader[] headers, int headerIndex)
        {
            Fields32WriteFunc(headers[headerIndex].TraceNumLine);
            Fields32WriteFunc(headers[headerIndex].TraceNumReel);
            Fields32WriteFunc(headers[headerIndex].FFID);
            Fields32WriteFunc(headers[headerIndex].RecNum);
            Fields32WriteFunc(headers[headerIndex].SouNum);
            Fields32WriteFunc(headers[headerIndex].CDPNum);
            Fields32WriteFunc(headers[headerIndex].TraceNum);
            Fields16WriteFunc(headers[headerIndex].TraceId);
            InSgyStream.Seek(6, SeekOrigin.Current);
            Fields32WriteFunc(headers[headerIndex].Offcet);
            InSgyStream.Seek(20, SeekOrigin.Current);
            Fields32WriteFunc(headers[headerIndex].DepthAtSou);
            Fields32WriteFunc(headers[headerIndex].DepthAtRec);
            Fields16WriteFunc(headers[headerIndex].ElevationsScaler);
            Fields16WriteFunc(headers[headerIndex].CoordinatesScaler);
            Fields32WriteFunc(headers[headerIndex].SouX);
            Fields32WriteFunc(headers[headerIndex].SouY);
            Fields32WriteFunc(headers[headerIndex].RecX);
            Fields32WriteFunc(headers[headerIndex].RecY);
            InSgyStream.Seek(92, SeekOrigin.Current);
            Fields32WriteFunc(headers[headerIndex].CDPX);
            Fields32WriteFunc(headers[headerIndex].CDPY);
            Fields32WriteFunc(headers[headerIndex].InlineNum);
            Fields32WriteFunc(headers[headerIndex].CrosslineNum);
        }
        protected void ReadTraceHeader(TraceHeader[] headers, int i)
        {
            headers[i].IsInitialized = true;
            headers[i].TraceNumLine = Fields32ReadFunc();
            headers[i].TraceNumReel = Fields32ReadFunc();
            headers[i].FFID = Fields32ReadFunc();
            headers[i].RecNum = Fields32ReadFunc();
            headers[i].SouNum = Fields32ReadFunc();
            headers[i].CDPNum = Fields32ReadFunc();
            headers[i].TraceNum = Fields32ReadFunc();
            headers[i].TraceId = Fields16ReadFunc();
            InSgyStream.Seek(6, SeekOrigin.Current);
            headers[i].Offcet = Fields32ReadFunc();
            InSgyStream.Seek(20, SeekOrigin.Current);
            headers[i].DepthAtSou = Fields32ReadFunc();
            headers[i].DepthAtRec = Fields32ReadFunc();
            headers[i].ElevationsScaler = Fields16ReadFunc();
            headers[i].CoordinatesScaler = Fields16ReadFunc();
            headers[i].SouX = Fields32ReadFunc();
            headers[i].SouY = Fields32ReadFunc();
            headers[i].RecX = Fields32ReadFunc();
            headers[i].RecY = Fields32ReadFunc();
            InSgyStream.Seek(92, SeekOrigin.Current);
            headers[i].CDPX = Fields32ReadFunc();
            headers[i].CDPY = Fields32ReadFunc();
            headers[i].InlineNum = Fields32ReadFunc();
            headers[i].CrosslineNum = Fields32ReadFunc();
        }

        public abstract long GetTraceHeaderAddress(int traceNum);
        public abstract TraceHeader[] ReadTracesHeaders(int startTrace, int numOfTraces);
        public abstract void WriteTracesHeaders(int startTrace, TraceHeader[] headers);
        public abstract float[][] ReadTracesSamples(int startTrace, int numOfTraces);

        public abstract void WriteTracesSamples(int startTrace, float[][] traces);


    }
}
