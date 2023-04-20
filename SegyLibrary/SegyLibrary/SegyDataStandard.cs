using System;
using System.IO;

namespace SegyLibrary
{
    public class SegyDataStandard:SegyData
    {
        public SegyDataStandard(string filePath, short sampleIntervalInMicroseconds, short samplesPerTrace, int traceCount,
            bool isLittleEndian, SampleFormat sampleFormat)
            : base(filePath, sampleIntervalInMicroseconds, samplesPerTrace, traceCount, isLittleEndian, sampleFormat)
        {
        }

        public SegyDataStandard(string filePath, TextHeaderEncoding textHeaderEncoding = TextHeaderEncoding.EBCDIC,
            int numOfExtTextHeaders = 0) : base(filePath, textHeaderEncoding, numOfExtTextHeaders)
        {
        }

        public override long GetTraceHeaderAddress(int traceNum)
        {
            if (traceNum < 0 || traceNum >= NumOfTraces)
            {
                throw new ArgumentOutOfRangeException("There is no trace with such number in the file!");
            }
            long adress = SegyBinHeaderPositions.BinHeaderEnd + BinHeader.NumOfExtTextHeaders * SegyBinHeaderPositions.ExtTextHeaderSizes + (traceNum) * ((long)TraceSize + SegyTraceHeaderPositions.TraceHeaderEnd);
            return adress;
        }

        //нужна ли эта функция?
        private long GetTraceDataAddress(int traceNum)
        {
            if (traceNum < 0 || traceNum >= NumOfTraces)
            {
                throw new ArgumentOutOfRangeException(
                    $"The current trace number {traceNum} is out of calculated bounds!");
            }
            long adress = SegyBinHeaderPositions.BinHeaderEnd + this.BinHeader.NumOfExtTextHeaders * SegyBinHeaderPositions.ExtTextHeaderSizes + (traceNum) * ((int)TraceSize + SegyTraceHeaderPositions.TraceHeaderEnd);
            adress += SegyTraceHeaderPositions.TraceHeaderEnd;
            return adress;
        }
        
        public override TraceHeader[] ReadTracesHeaders(int startTrace, int numOfTraces)
        {
            var headers = new TraceHeader[numOfTraces];
            InSgyStream.Seek(GetTraceHeaderAddress(startTrace),SeekOrigin.Begin);
            for (int i = 0; i < headers.Length && InSgyStream.Position < Filesize; i++)
            {
                ReadTraceHeader(headers, i);
                InSgyStream.Seek(SegyTraceHeaderPositions.TraceHeaderEnd - 196 + BinHeader.TraceLength * SampleSize, SeekOrigin.Current);
            }
            return headers;
        }

        public override void WriteTracesHeaders(int startTrace, TraceHeader[] headers)
        {
            InSgyStream.Seek(GetTraceHeaderAddress(startTrace), SeekOrigin.Begin);
            for (int i = 0; i < headers.Length && InSgyStream.Position < Filesize && headers[i].IsInitialized; i++)
            {
                //temporary contains magic numbers))
                WriteTraceHeader(headers, i);
                InSgyStream.Seek(SegyTraceHeaderPositions.TraceHeaderEnd - 196 + BinHeader.TraceLength * SampleSize, SeekOrigin.Current);
            }
        }

        //TODO: maybe change arguments positions and do them unnecessary, with startTrace = 0, and NumOfTraces = (tracesCount in file)
        public override float[][] ReadTracesSamples(int startTrace, int numOfTraces)
        {
            float[][] traces = new float[numOfTraces][];
            int traceLength = BinHeader.TraceLength;
            InSgyStream.Seek(GetTraceHeaderAddress(startTrace), SeekOrigin.Begin);
            for (int i = 0; i < numOfTraces; i++)
            {
                if (startTrace + i >= NumOfTraces)
                {
                    for (; i < traces.Length; i++)
                    {
                        traces[i] = new float[0];
                    }
                    return traces;
                    //throw new ArgumentOutOfRangeException("Out of file bounds");
                }
                traces[i] = new float[traceLength];
                InSgyStream.Seek(SegyTraceHeaderPositions.TraceHeaderEnd, SeekOrigin.Current);
                for (int t = 0; t < traceLength; t++)
                {
                    traces[i][t] = SamplesReadFunc();
                }
            }
            return traces;
        }

        public override void WriteTracesSamples(int startTrace, float[][] traces)
        {
            InSgyStream.Seek(GetTraceHeaderAddress(startTrace), SeekOrigin.Begin);
            for (int i = 0; i < traces.Length; i++)
            {
                if (startTrace + i >= NumOfTraces)
                {
                    return;
                    //throw new ArgumentOutOfRangeException("Out of file bounds");
                }
                InSgyStream.Seek(SegyTraceHeaderPositions.TraceHeaderEnd, SeekOrigin.Current);
                for (int t = 0; t < traces[i].Length; t++)
                {
                    SamplesWriteFunc(traces[i][t]);
                }
            }
        }
    }
}
