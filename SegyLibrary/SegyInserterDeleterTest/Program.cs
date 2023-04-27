using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using SegyLibrary;

namespace SegySamplesExtractorInserterTest
{
    class Program
    {
        static int LaunchExecutable(string executablePath, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = executablePath;
            startInfo.Arguments = arguments;
            //startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            //startInfo.CreateNoWindow = true;

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                //string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                int exitCode = process.ExitCode;
                return exitCode;
            }
        }
        static int Main(string[] args)
        {
            string segyFilePath = @"C:\MyRepo\SegyLibrary\SegyInserterDeleterTest\example.sgy";//this file must exist
            string newSegyFilePath = @"C:\MyRepo\SegyLibrary\SegyInserterDeleterTest\afterReadingWritingExample.sgy";
            string binaryFilePath = @"C:\MyRepo\SegyLibrary\SegyInserterDeleterTest\example.bin";
            string directionFromSegyToBinary = @"FromSegyToBinary";
            string directionFromBinaryToSegy = @"FromBinaryToSegy";

            string segySamplesExtractorInserterPath =
                @"C:\MyRepo\SegyLibrary\SegySamplesExtractorInserter2\bin\Debug\SegySamplesExtractorInserter2.exe";

            int completionCode = 0;
            string fromSegyArguments = segyFilePath + " " + binaryFilePath + " " + directionFromSegyToBinary;
            completionCode += LaunchExecutable(segySamplesExtractorInserterPath, fromSegyArguments);

            bool doOverwrite = true;
            File.Copy(segyFilePath, newSegyFilePath, doOverwrite);

            string toSegyArguments = newSegyFilePath + " " + binaryFilePath + " " + directionFromBinaryToSegy;
            completionCode += LaunchExecutable(segySamplesExtractorInserterPath, toSegyArguments);

            return completionCode;
        }



        //enum Direction
        //{
        //    FromSegyToBinary,
        //    FromBinaryToSegy
        //}
        //static void WriteFloatArrayToBinaryFile(float[][] data, string fileName)
        //{
        //    using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
        //    {
        //        int numRows = data.Length;
        //        int numCols = data[0].Length;

        //        writer.Write(numRows);
        //        writer.Write(numCols);

        //        for (int i = 0; i < numRows; i++)
        //        {
        //            for (int j = 0; j < numCols; j++)
        //            {
        //                writer.Write(data[i][j]);
        //            }
        //        }
        //    }
        //}

        //static float[][] ReadFloatArrayFromBinaryFile(string fileName)
        //{
        //    using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
        //    {
        //        int numRows = reader.ReadInt32();
        //        int numCols = reader.ReadInt32();

        //        float[][] data = new float[numRows][];
        //        for (int i = 0; i < numRows; i++)
        //        {
        //            data[i] = new float[numCols];
        //            for (int j = 0; j < numCols; j++)
        //            {
        //                data[i][j] = reader.ReadSingle();
        //            }
        //        }

        //        return data;
        //    }
        //}
        //static int MainForTest(string[] args)
        //{


        //    string segyFilePath = args[0];
        //    string binaryFilePath = args[1];
        //    Direction direction =
        //        (args[2] == "FromSegyToBinary") ? Direction.FromSegyToBinary : Direction.FromBinaryToSegy;

        //    SegyDataStandard segyFile = new SegyDataStandard(segyFilePath);

        //    if (direction == Direction.FromSegyToBinary)
        //    {
        //        float[][] data = segyFile.ReadTracesSamples(0, segyFile.NumOfTraces);
        //        WriteFloatArrayToBinaryFile(data, binaryFilePath);
        //    }
        //    else // Direction.FromBinaryToSegy
        //    {
        //        float[][] data = ReadFloatArrayFromBinaryFile(binaryFilePath);
        //        segyFile.WriteTracesSamples(0, data);
        //    }

        //    return 0;
        //}

        //static int Main(string[] args)
        //{
        //    string segyFilePath = @"C:\MyRepo\SegyLibrary\SegyInserterDeleterTest\example.sgy";//this file must exist
        //    //string newSegyFilePath = @"C:\MyRepo\SegyLibrary\SegyInserterDeleterTest\afterReadingWritingExample.sgy";
        //    string binaryFilePath = @"C:\MyRepo\SegyLibrary\SegyInserterDeleterTest\example.bin";
        //    string directionFromSegyToBinary = @"FromSegyToBinary";
        //   // string directionFromBinaryToSegy = @"FromBinaryToSegy";


        //    string[] myArgs = new string[3]
        //    {
        //        segyFilePath,
        //        binaryFilePath,
        //        directionFromSegyToBinary
        //    };

        //    MainForTest(myArgs);

        //    return 0;
        //}
    }
}
