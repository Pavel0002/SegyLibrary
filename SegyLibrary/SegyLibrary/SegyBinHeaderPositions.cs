using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegyLibrary
{
    //использованны структуры а не перечисления, т.к. перечисления нужно явно приводить к числам при арифметических операциях.
    public struct SegyBinHeaderPositions
    {
        public const int NumType = 3224;
        public const int BinHeaderEnd = 3600;
        public const int TextHeaderEnd = 3200;
        public const int SampleIntervalAddress = 16 + 3200;
        public const int SamplesPerTraceAddress = 20 + 3200;
        public const int SampleFormatAddress = 24 + 3200;
        public const int TraceLength = 3220;
        public const int NumOfExtTextHeaders = 3504;
        public const int ExtTextHeaderSizes = 3200;
    }
    
}
