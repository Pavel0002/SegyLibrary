using System;
using System.IO;
using System.Runtime.CompilerServices;
using SegyLibrary;

namespace SegySamplesExtractorInserter
{
    class Program
    {
        enum Direction
        {
            FromSegyToBinary,
            FromBinaryToSegy
        }

        static void WriteFloatArrayToBinaryFile(float[][] data, string fileName)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                int numRows = data.Length;
                int numCols = data[0].Length;

                writer.Write(numRows);
                writer.Write(numCols);

                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numCols; j++)
                    {
                        writer.Write(data[i][j]);
                    }
                }
            }
        }

        static float[][] ReadFloatArrayFromBinaryFile(string fileName)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                int numRows = reader.ReadInt32();
                int numCols = reader.ReadInt32();

                float[][] data = new float[numRows][];
                for (int i = 0; i < numRows; i++)
                {
                    data[i] = new float[numCols];
                    for (int j = 0; j < numCols; j++)
                    {
                        data[i][j] = reader.ReadSingle();
                    }
                }

                return data;
            }
        }

        static int Main(string[] args)
        {
            Console.WriteLine("Started");

            if (args.Length != 3)
            {
                Console.WriteLine("Should be 3 arguments, but got" + args.Length);
                for (var i = 0; i < args.Length; i++)
                {
                    Console.WriteLine(args[i]);
                }
                return 1;
            }

            for (var i = 0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
            }

            string segyFilePath = args[0];
            string binaryFilePath = args[1];
            Direction direction =
                (args[2] == "FromSegyToBinary") ? Direction.FromSegyToBinary : Direction.FromBinaryToSegy;


            SegyDataStandard segyFile = new SegyDataStandard(segyFilePath);

            if (direction == Direction.FromSegyToBinary)
            {
                float[][] data = segyFile.ReadTracesSamples(0, segyFile.NumOfTraces);
                WriteFloatArrayToBinaryFile(data, binaryFilePath);
            }
            else // Direction.FromBinaryToSegy
            {
                float[][] data = ReadFloatArrayFromBinaryFile(binaryFilePath);
                segyFile.WriteTracesSamples(0, data);
            }

            //Console.WriteLine("Finished");
            Console.ReadKey();
            return 0;
        }
    }
}
