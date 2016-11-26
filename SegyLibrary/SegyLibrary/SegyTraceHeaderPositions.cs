using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegyLibrary
{
    public struct SegyTraceHeaderPositions
    {
        public const int CoordinatesScalingConst = 70;
        public const int CdpNo = 20;
        public const int CdpX = 180;
        public const int CdpY = 184;
        public const int TraceHeaderEnd = 240;
        public const int TraceLength = 114;
    }

}
