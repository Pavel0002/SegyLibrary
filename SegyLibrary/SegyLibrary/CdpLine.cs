using System;
using System.Collections.Generic;
using System.IO;

namespace SegyLibrary
{
    public class CdpLine
    {
        string Name;
        //public bool HasErrors { get; private set; }
        //сейчас создается пустой, но лучше найти способ задать емкость словаря сразу
        public Dictionary<int, Point<double>> CdpPoints;

        public override int GetHashCode()
        {
            return CdpPoints[0].GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var p = obj as CdpLine;
            if (this.CdpPoints.Count != p?.CdpPoints.Count)
            {
                return false;
            }
            foreach (var key in this.CdpPoints.Keys)
            {
                if (p.CdpPoints.ContainsKey(key))
                {
                    if (this.CdpPoints[key] != p.CdpPoints[key])
                    {
                        return false;
                    }
                }
                else return false;
            }
            return true;
        }
        public CdpLine(string lineName, int size)
        {
            Name = lineName;
            CdpPoints = new Dictionary<int, Point<double>>(size);
        }
        public CdpLine()
        {
            Name = "";
            CdpPoints = new Dictionary<int, Point<double>>();
        }
        // Лучше было бы создать в классе SegyData массив заголовков, и заполнять линию оттуда, но пока, для ускорения написания делаем такой метод.
        public void FillFromFile(Func<int> Fields32ReadFunc, Func<short> Fields16ReadFunc, string path, FileStream InSgyFileStream, BinaryReader SegyDataBinReader, int SampleSize, int NumOfExtTextHeaders)
        {
            {
                //проходимся по заголовкам всех трасс, и добавляем номера cdp и соответствующие им значения в линию
                //пропускаем заголовки
                //SegyStream.Seek(SegyBinHeaderPositions.BinHeaderEnd, SeekOrigin.Begin);

                InSgyFileStream.Seek(SegyBinHeaderPositions.BinHeaderEnd + NumOfExtTextHeaders * SegyBinHeaderPositions.ExtTextHeaderSizes, SeekOrigin.Begin);
                int TraceLength;
                int ScaleConst;
                int CdpNo;
                Point<double> LinePoint = new Point<double>();
                while (SegyDataBinReader.BaseStream.Position < SegyDataBinReader.BaseStream.Length)
                //while(InSgyFileStream.Position < InSgyFileStream.Length)
                {
                    InSgyFileStream.Seek(SegyTraceHeaderPositions.CdpNo, SeekOrigin.Current);
                    CdpNo = Fields32ReadFunc();
                    InSgyFileStream.Seek(46, SeekOrigin.Current);
                    ScaleConst = Fields16ReadFunc();
                    InSgyFileStream.Seek(42, SeekOrigin.Current);
                    TraceLength = Fields16ReadFunc();
                    InSgyFileStream.Seek(64, SeekOrigin.Current);
                    LinePoint.X = Fields32ReadFunc() * (Math.Pow(Math.Abs(ScaleConst), Math.Sign(ScaleConst)));
                    LinePoint.Y = Fields32ReadFunc() * (Math.Pow(Math.Abs(ScaleConst), Math.Sign(ScaleConst)));
                    if (!CdpPoints.ContainsKey(CdpNo))
                    {
                        CdpPoints.Add(CdpNo, LinePoint);
                    }
                    InSgyFileStream.Seek(52 + TraceLength * SampleSize, SeekOrigin.Current);
                }
            }
        }
    }
}
