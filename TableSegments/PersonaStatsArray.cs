/*
    Persona 3 Table Exporter, a tool for exporting data tables from Persona 3 for analysis.
    Copyright (C) 2021  James Pelster "CaptainSwag101"

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P3TableExporter.TableSegments
{
    class PersonaStatsArray : TableSegment
    {
        public PersonaStats[] StatsArray;
        public const int STRUCT_SIZE = 14;

        public PersonaStatsArray(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();
            int structCount = (segmentSize / STRUCT_SIZE);
            StatsArray = new PersonaStats[structCount];

            for (uint i = 0; i < structCount; ++i)
            {
                StatsArray[i] = new();
                StatsArray[i].Type = reader.ReadUInt16();
                StatsArray[i].Arcana = reader.ReadByte();
                StatsArray[i].BaseLevel = reader.ReadByte();
                StatsArray[i].Strength = reader.ReadByte();
                StatsArray[i].Magic = reader.ReadByte();
                StatsArray[i].Endurance = reader.ReadByte();
                StatsArray[i].Agility = reader.ReadByte();
                StatsArray[i].Luck = reader.ReadByte();
                StatsArray[i].Unknown1 = reader.ReadByte();
                StatsArray[i].Inheritance = reader.ReadByte();
                StatsArray[i].Unknown2 = reader.ReadUInt16();
                StatsArray[i].Unknown3 = reader.ReadByte();
            }
        }

        public override string GenerateRowColumnText(Table? msgTbl = null)
        {
            StringBuilder outputBuilder = new();

            List<string> rowStrings = new();
            for (int col = 0; col < 13; ++col)
            {
                string colName = col switch
                {
                    0 => "Persona Name",
                    1 => "Type",
                    2 => "Arcana",
                    3 => "Base Level",
                    4 => "Strength",
                    5 => "Magic",
                    6 => "Endurance",
                    7 => "Agility",
                    8 => "Luck",
                    9 => "Unknown1",
                    10 => "Inheritance",
                    11 => "Unknown2",
                    12 => "Unknown3",
                    _ => "ERROR"
                };
                rowStrings.Add(colName);
            }
            outputBuilder.AppendJoin(',', rowStrings);
            outputBuilder.Append('\n');

            // Extract Persona names from MSG.TBL
            string[]? personaNames = null;
            if (msgTbl != null)
            {
                foreach (TableSegment segment in msgTbl.Segments)
                {
                    if (segment is PersonaNames personaNameSegment)
                    {
                        personaNames = personaNameSegment.Names;
                    }
                }
            }

            // Extract arcana names from MSG.TBL
            string[]? arcanaNames = null;
            if (msgTbl != null)
            {
                foreach (TableSegment segment in msgTbl.Segments)
                {
                    if (segment is ArcanaNames arcanaNameSegment)
                    {
                        arcanaNames = arcanaNameSegment.Names;
                    }
                }
            }

            for (int i = 0; i < StatsArray.Length; ++i)
            {
                rowStrings.Clear();

                if (personaNames != null)
                    rowStrings.Add(personaNames[i]);
                else
                    rowStrings.Add(i.ToString());

                rowStrings.Add(StatsArray[i].Type.ToString());

                if (arcanaNames != null)
                    rowStrings.Add(arcanaNames[StatsArray[i].Arcana]);
                else
                    rowStrings.Add(StatsArray[i].Arcana.ToString());

                rowStrings.Add(StatsArray[i].BaseLevel.ToString());
                rowStrings.Add(StatsArray[i].Strength.ToString());
                rowStrings.Add(StatsArray[i].Magic.ToString());
                rowStrings.Add(StatsArray[i].Endurance.ToString());
                rowStrings.Add(StatsArray[i].Agility.ToString());
                rowStrings.Add(StatsArray[i].Luck.ToString());
                rowStrings.Add(StatsArray[i].Unknown1.ToString());
                rowStrings.Add(StatsArray[i].Inheritance.ToString());
                rowStrings.Add(StatsArray[i].Unknown2.ToString());
                rowStrings.Add(StatsArray[i].Unknown3.ToString());

                outputBuilder.AppendJoin(',', rowStrings);
                outputBuilder.Append('\n');
            }

            return outputBuilder.ToString();
        }
    }

    struct PersonaStats
    {
        public ushort Type;
        public byte Arcana;
        public byte BaseLevel;
        public byte Strength;
        public byte Magic;
        public byte Endurance;
        public byte Agility;
        public byte Luck;
        public byte Unknown1;
        public byte Inheritance;
        public ushort Unknown2;
        public byte Unknown3;
    }
}
