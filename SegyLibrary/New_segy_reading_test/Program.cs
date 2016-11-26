using System;
using SegyLibrary;
using System.IO;
using System.Diagnostics;
using Unplugged.IbmBits;

namespace SegyLibraryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //добавить SampleInterval (ну и неплохо бы и остальные поля) в BinaryHeader
            //изменить short на ushort, где можно. для этого понадобится добавить функцию чтения полей, возвращающую ushort
            //сделать, чтобы читались все поля заголовков.
            //сравнить скорость чтения с Unplugged.Segy
            //удалить магические числа из методов ReadTracesHeaders
            //может можно улучшить инкапсуляцию класса "SegyDataVarTr" (попробовать скрыть переменную "AreTracesAdressesFilled)"
            //сделать корректное чтение всех форматов - сделано, протестировать, посмотреть, в порядке ли имена функций
            string SgyPath1 = "F:\\filteringR4LE.sgy";
            string SgyPath2 = "F:\\filteringR4BE.sgy";
            //с этим файлом не работает (файл битый)
            string SgyPath3 = "F:\\BIGout.sgy";
            string SgyPath4 = "F:\\130802ish.sgy"; //‪"F:\\130201vish.sgy"   
            //чтение линии не работает с этим файлом - пока только форматы ibm и ieee
            string SgyPath5 = "F:\\130201vish.sgy";
            string SgyPath6 = "F:\\In.sgy";
            string SgyPath7 = "F:\\testgeom.sgy";//"‪F:\\testgeom.sgy"
            string SgyPath8 = "F:\\filtering.sgy";
            //в файле начиная с трассы 5042 есть битые заголовки (например длыны трасс).
            string SgyPath9 = @"F:\FromPromax.sgy";
            string SgyPath10 = @"F:\bigEndianIEEEFloat.sgy";
            string NumTestFile = @"F:\Test.bin";

            
            using (var fs = new FileStream("Empty.sgy", FileMode.Create))
            {
                fs.SetLength(100000);
                fs.Seek(100, SeekOrigin.Begin);
                var br = new BinaryReader(fs);
                int ss = br.ReadInt32();
                br.Close();
            }


            using (SegyDataStandart segyData3 = new SegyDataStandart("mysegy.sgy", 300, 1000, 100, false,
                    SampleFormat.IEEE))
            {
                float[][] data = new float[1][];
                data[0] = new float[1000];
                data[0][10] = -10;

                segyData3.WriteTracesSamples(2, data);
            }

            //тест функций расширения чтения и записи различных чисел
                /*
                using (FileStream fs = new FileStream(NumTestFile, FileMode.Create))
                using (BinaryReader br = new BinaryReader(fs))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    fs.SetLength(4);
                    fs.Seek(0, SeekOrigin.Begin);
                    float fl = 127;
                    fs.Seek(0, SeekOrigin.Begin);
                    bw.WriteBigEndian16FromSingle(fl);
                    fs.Seek(0, SeekOrigin.Begin);
                    float intshort = br.ReadBigEndian16ToSingle();
                    fl = 3000;
                    fs.Seek(0, SeekOrigin.Begin);
                    bw.WriteBigEndian32FromSingle(fl);
                    fs.Seek(0, SeekOrigin.Begin);
                    float intint = br.ReadBigEndian32ToSingle();
                    fs.Seek(0, SeekOrigin.Begin);
                    fl = 1234.567f;
                    bw.WriteSingleBigEndian(fl);
                    fs.Seek(0, SeekOrigin.Begin);
                    float floatfloat = br.ReadSingleBigEndian();
                    fs.Seek(0, SeekOrigin.Begin);
                    fl = 127;
                    bw.WriteSignedByteFromSingle(fl);
                    fs.Seek(0, SeekOrigin.Begin);
                    float intbyte = br.ReadSignedByteToSingle();
                    }
                    */
                /*
                var watch = Stopwatch.StartNew();
                float[][] traces = new float[48][];
                using (FileStream fs = new FileStream(SgyPath2, FileMode.Open))
                using(BinaryReader br = new BinaryReader(fs))
                using(BinaryWriter bw = new BinaryWriter(fs))
                {
                    fs.Seek(3840,SeekOrigin.Begin);
                    for (int j = 0; j < 48; j++)
                    {
                        traces[j] = new float[2048];
                        for (int i = 0; i < 2048; i++)
                        {
                            traces[j][i] = br.ReadSingleIbm();
                            //traces[j][i] = br.ReadSingle();
                        }
                        fs.Seek(240, SeekOrigin.Current);
                    }
                }
                var elapsedMs = watch.ElapsedMilliseconds;
                watch.Stop();



                */
            
            using (var sgyData1 = new SegyDataVarTr(SgyPath1))
            {
                //sgyData1.FillCdpLineFromFile();
                /*
                if (sgyData1.CdpLine.Equals(sgyData1.CdpLine))
                {
                    Console.WriteLine("Линии CDP в файлах совпадают");
                }
                else Console.WriteLine("Линии CDP в файлах не совпадают");
                */

                for (int i = 0; i < sgyData1.NumOfTraces; i+=8)
                {
                    TraceHeader[] headers = sgyData1.ReadTracesHeaders(i, 2);
                    float[][] samples = sgyData1.ReadTracesSamples(i, 2);
                }
                var s = sgyData1.BinHeader.SampleInterval;
            }

            Console.Read();
            
            
        }
    }
}
