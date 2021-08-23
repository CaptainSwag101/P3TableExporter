using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P3TableExporter.TableSegments
{
    class UnknownSegment : TableSegment
    {
        public List<byte[]> Data;
        public int STRUCT_SIZE = 0;

        public UnknownSegment(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();

            // Since we don't know ahead of time what the struct size will be,
            // try to find the greatest common factor between 0x10 and the total segment size.
            STRUCT_SIZE = (int)LCM(4, (uint)segmentSize);
            int structCount = (segmentSize / STRUCT_SIZE);

            if (STRUCT_SIZE * structCount != segmentSize)
                throw new ArgumentException("The specified struct size is not a divisible factor of the segment size.", nameof(STRUCT_SIZE));

            Data = new();
            for (int i = 0; i < structCount; ++i)
            {
                Data.Add(reader.ReadBytes(STRUCT_SIZE));
            }
        }

        public UnknownSegment(BinaryReader reader, int _structSize)
        {
            int segmentSize = reader.ReadInt32();

            STRUCT_SIZE = _structSize;
            int structCount = (segmentSize / STRUCT_SIZE);

            if (STRUCT_SIZE * structCount != segmentSize)
                throw new ArgumentException("The specified struct size is not a divisible factor of the segment size.", nameof(_structSize));

            Data = new();
            for (int i = 0; i < structCount; ++i)
            {
                Data.Add(reader.ReadBytes(STRUCT_SIZE));
            }
        }

        public override string GenerateRowColumnText(Table? msgTbl = null)
        {
            StringBuilder outputBuilder = new();

            foreach (var byteArray in Data)
            {
                List<string> rowStrings = new();

                foreach (byte b in byteArray)
                {
                    rowStrings.Add(b.ToString());
                }

                outputBuilder.AppendJoin(',', rowStrings);
                outputBuilder.Append('\n');
            }

            return outputBuilder.ToString();
        }

        private ulong LCM(ulong a, ulong b)
        {
            return ((a * b) / GCD(a, b));
        }

        private ulong GCD(ulong a, ulong b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }
    }
}
