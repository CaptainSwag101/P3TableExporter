using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P3TableExporter.TableSegments
{
    class ArcanaNames : TableSegment
    {
        public string[] Names;
        public const int STRUCT_SIZE = 21;

        public ArcanaNames(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();
            int structCount = segmentSize / STRUCT_SIZE;
            Names = new string[structCount];

            for (int i = 0; i < structCount; ++i)
            {
                Names[i] = Encoding.ASCII.GetString(reader.ReadBytes(STRUCT_SIZE)).TrimEnd('\0');
            }
        }

        public override string GenerateRowColumnText(Table? msgTbl = null)
        {
            throw new NotImplementedException();
        }
    }
}
