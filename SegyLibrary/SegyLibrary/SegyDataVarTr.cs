using System;
using System.Collections.Generic;
using System.IO;

namespace SegyLibrary
{
    public class SegyDataVarTr:SegyData
    {
        //пока не тестировался на файлах с переменным количеством трасс!!!!
        Dictionary<int, long> TracesAdresses;
        Dictionary<int, short> TracesLengths;
        private bool AreTracesAdressesFilled { get; set; }
        private int _maxTraceRead;
        public SegyDataVarTr(string filePath, TextHeaderEncoding textHeaderEncoding = TextHeaderEncoding.EBCDIC, int numOfExtTextHeaders = 0, bool suppressFillingTracesAdresses = false) : base(filePath, textHeaderEncoding, numOfExtTextHeaders)
        {
            TracesAdresses = new Dictionary<int, long>(NumOfTraces);
            TracesLengths = new Dictionary<int, short>(NumOfTraces);
            TracesAdresses[0] = SegyBinHeaderPositions.BinHeaderEnd + SegyBinHeaderPositions.ExtTextHeaderSizes * numOfExtTextHeaders;
            InSgyStream.Seek(TracesAdresses[0] + SegyTraceHeaderPositions.TraceLength, SeekOrigin.Begin);
            TracesLengths[0] = Fields16ReadFunc();
            if (!suppressFillingTracesAdresses)
                FillTracesAdresses();
        }

        public override long GetTraceHeaderAddress(int traceNum)
        {
            if (TracesAdresses.ContainsKey(traceNum))
            {
                return TracesAdresses[traceNum];
            }
            if (AreTracesAdressesFilled)
            {
                throw new ArgumentOutOfRangeException($"There is no trace with number {traceNum} in the file!");
            }
            //это плохо работает, если вызываем функцию последовательно для трасс из файла, т.к. если уже много трасс в словаре,
            //то поиск максимального номера идет очень долго.
            //int currentTraceNum = TracesAdresses.Keys.Max();
            int currentTraceNum = _maxTraceRead;
            while (currentTraceNum < traceNum)
            {
                long nextTraceAdress = TracesAdresses[currentTraceNum] + SegyTraceHeaderPositions.TraceHeaderEnd + TracesLengths[currentTraceNum] * SampleSize;
                if (nextTraceAdress < Filesize)
                    TracesAdresses.Add(currentTraceNum + 1, nextTraceAdress);
                else
                    break;
                InSgyStream.Seek(nextTraceAdress + SegyTraceHeaderPositions.TraceLength, SeekOrigin.Begin);
                short currentTraceLength = Fields16ReadFunc();
                TracesLengths.Add(currentTraceNum + 1, currentTraceLength);
                currentTraceNum++;
                _maxTraceRead++;
            }
            if (InSgyStream.Position >= InSgyStream.Length)
            {
                AreTracesAdressesFilled = true;
            }
            if (NumOfTraces < currentTraceNum)
            {
                NumOfTraces = currentTraceNum;
            }
            if (TracesAdresses.ContainsKey(traceNum))
            {
                return TracesAdresses[traceNum];
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    $"The current trace number {traceNum} is out of calculated bounds!");
            } 

        }

        public override TraceHeader[] ReadTracesHeaders(int startTrace, int numOfTraces)
        {
            var headers = new TraceHeader[numOfTraces];
            InSgyStream.Seek(GetTraceHeaderAddress(startTrace), SeekOrigin.Begin);
            for (int i = 0; i < headers.Length && InSgyStream.Position < Filesize; i++)
            {
                InSgyStream.Seek(GetTraceHeaderAddress(startTrace + i), SeekOrigin.Begin);
                ReadTraceHeader(headers, i);
                InSgyStream.Seek(SegyTraceHeaderPositions.TraceHeaderEnd - 196 + TracesLengths[startTrace + i] * SampleSize, SeekOrigin.Current);
            }
            return headers;
        }

        

        public override void WriteTracesHeaders(int startTrace, TraceHeader[] headers)
        {
            InSgyStream.Seek(GetTraceHeaderAddress(startTrace), SeekOrigin.Begin);
            for (int i = 0; i < headers.Length && InSgyStream.Position < Filesize && headers[i].IsInitialized; i++)
            {
                //temporary contains magic numbers))
                InSgyStream.Seek(GetTraceHeaderAddress(startTrace + i), SeekOrigin.Begin);
                WriteTraceHeader(headers, i);
                InSgyStream.Seek(SegyTraceHeaderPositions.TraceHeaderEnd - 196 + TracesLengths[startTrace + i] * SampleSize, SeekOrigin.Current);
            }
        }

        public override float[][] ReadTracesSamples(int startTrace, int numOfTraces)
        {
            float[][] traces = new float[numOfTraces][];
            for (int i = 0; i < numOfTraces; i++)
            {
                InSgyStream.Seek(GetTraceHeaderAddress(startTrace + i) + SegyTraceHeaderPositions.TraceHeaderEnd, SeekOrigin.Begin);
                if ((startTrace + i) >= NumOfTraces)
                {
                    for (; i < traces.Length; i++)
                    {
                        traces[i] = new float[0];
                    }
                    return traces;
                }
                traces[i] = new float[TracesLengths[startTrace + i]];
                for (int t = 0; t < traces[i].Length; t++)
                {
                    traces[i][t] = SamplesReadFunc();
                }
            }
            return traces;
        }

        public override void WriteTracesSamples(int startTrace, float[][] traces)
        {
            //!!! В данной версии, если в изначальном файле будет записано неверное количество отсчетов (например для последней тарссы) то в конец файла запишется мусор!
            //также в массиве, который записывается в файл длинны трасс должны соответствовать длинам трасс из файла.
            for (int i = 0; i < traces.Length; i++)
            {
                InSgyStream.Seek(GetTraceHeaderAddress(startTrace + i) + SegyTraceHeaderPositions.TraceHeaderEnd, SeekOrigin.Begin);
                if ((startTrace + i) >= NumOfTraces)
                {
                    return;
                    //throw new ArgumentOutOfRangeException("there is no trace with such number in file!");
                }
                for (int t = 0; t < (TracesLengths[startTrace + i]) && t < traces[i].Length; t++)
                {
                    SamplesWriteFunc(traces[i][t]);
                }
            }
        }

        private void FillTracesAdresses()
        {
            //int currentTraceNum = TracesAdresses.Keys.Max();
            int currentTraceNum = _maxTraceRead;
            while (InSgyStream.Position < Filesize)
            {
                long nextTraceAdress = TracesAdresses[currentTraceNum] + SegyTraceHeaderPositions.TraceHeaderEnd + TracesLengths[currentTraceNum] * SampleSize;
                if (nextTraceAdress < Filesize)
                    TracesAdresses.Add(currentTraceNum + 1, nextTraceAdress);
                else
                    break;
                InSgyStream.Seek(nextTraceAdress + SegyTraceHeaderPositions.TraceLength, SeekOrigin.Begin);
                var currentTraceLength = Fields16ReadFunc();
                TracesLengths.Add(currentTraceNum + 1, currentTraceLength);
                currentTraceNum++;
                _maxTraceRead++;
            }
            if (currentTraceNum + 1 > NumOfTraces)
                NumOfTraces = currentTraceNum + 1;
        }
    }
}
