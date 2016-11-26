using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Unplugged.IbmBits;

namespace SegyLibrary
{
    public class TextHeader
    {
        public string Text;
        TextHeaderEncoding TextHeaderEncoding;
        public TextHeader()
        {
            Text = "";
            TextHeaderEncoding = TextHeaderEncoding.EBCDIC;
        }
        public TextHeader(TextHeaderEncoding TextHeaderEncoding, BinaryReader InSgyBinReader)
        {
            FillTextHeader(TextHeaderEncoding, InSgyBinReader);
            this.TextHeaderEncoding = TextHeaderEncoding;
        }
        public void FillTextHeader(TextHeaderEncoding TextHeaderEncoding, BinaryReader InSgyBinReader)
        {
            InSgyBinReader.BaseStream.Seek(0, SeekOrigin.Begin);
            var bytes = InSgyBinReader.ReadBytes(3200);
            Text = (TextHeaderEncoding == TextHeaderEncoding.ASCII) ?
                ASCIIEncoding.Default.GetString(bytes) :
                IbmConverter.ToString(bytes);
        }
    }
}
