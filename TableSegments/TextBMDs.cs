using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P3TableExporter.TableSegments
{
    class TextBMDs : TableSegment
    {
        public string[] Strings;
        public const int STRUCT_SIZE = 1;

        public TextBMDs(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();
            int structCount = segmentSize / STRUCT_SIZE;
            Strings = new string[structCount];

            for (int i = 0; i < structCount; ++i)
            {
                Strings[i] = Encoding.ASCII.GetString(reader.ReadBytes(STRUCT_SIZE)).TrimEnd('\0');
            }
        }

        public override string GenerateRowColumnText(Table? msgTbl = null)
        {
            throw new NotImplementedException();
        }
    }
}
