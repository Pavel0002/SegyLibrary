using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegyLibrary
{
    public struct TraceHeader
    {
        public bool IsInitialized;
        public int TraceNumLine;
        public int TraceNumReel;
        public int FFID;
        public int RecNum;
        public int SouNum;
        public int CDPNum;
        public int TraceNum;
        public short TraceId;
        public int Offcet;
        public int DepthAtSou;
        public int DepthAtRec;
        public short ElevationsScaler;
        public short CoordinatesScaler;
        public int SouX;
        public int SouY;
        public int RecX;
        public int RecY;
        public int CDPX;
        public int CDPY;
        public int InlineNum;
        public int CrosslineNum;
    }
}
