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
using System.Text;
using System.Threading.Tasks;

namespace P3TableExporter.TableSegments
{
    class PersonaDataArray : TableSegment
    {
        public PersonaData[] Data;
        public const int STRUCT_SIZE = 14;

        private readonly string[] InheritanceStrings =
        {
            "None",
            "Fire",
            "Ice",
            "Elec",
            "Wind",
            "Light",
            "Dark",
            "Party Members 1",
            "Party Members 2",
            "Healing",
            "UNUSED 1",
            "Strike",
            "Slash",
            "Pierce",
            "UNUSED 2",
            "Ailment",
            "Light & Dark",
            "All"
        };

        public PersonaDataArray(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();
            int structCount = (segmentSize / STRUCT_SIZE);
            Data = new PersonaData[structCount];

            for (uint i = 0; i < structCount; ++i)
            {
                Data[i] = new();
                Data[i].Type = reader.ReadUInt16();
                Data[i].Arcana = reader.ReadByte();
                Data[i].BaseLevel = reader.ReadByte();
                Data[i].Stats.Strength = reader.ReadByte();
                Data[i].Stats.Magic = reader.ReadByte();
                Data[i].Stats.Endurance = reader.ReadByte();
                Data[i].Stats.Agility = reader.ReadByte();
                Data[i].Stats.Luck = reader.ReadByte();
                Data[i].Unknown1 = reader.ReadByte();
                Data[i].Inheritance = reader.ReadByte();
                Data[i].Flags = reader.ReadUInt16();
                Data[i].Unknown2 = reader.ReadByte();
            }
        }

        public override string GenerateRowColumnText(Table? msgTbl = null)
        {
            StringBuilder outputBuilder = new();

            // First row is column names
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
                    11 => "Special Flags",
                    12 => "Unknown2",
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

            // Remaining rows are data
            for (int i = 0; i < Data.Length; ++i)
            {
                rowStrings.Clear();

                if (personaNames != null)
                    rowStrings.Add(personaNames[i]);
                else
                    rowStrings.Add(i.ToString());

                rowStrings.Add(Data[i].Type.ToString());

                if (arcanaNames != null)
                    rowStrings.Add(arcanaNames[Data[i].Arcana]);
                else
                    rowStrings.Add(Data[i].Arcana.ToString());

                rowStrings.Add(Data[i].BaseLevel.ToString());
                rowStrings.Add(Data[i].Stats.Strength.ToString());
                rowStrings.Add(Data[i].Stats.Magic.ToString());
                rowStrings.Add(Data[i].Stats.Endurance.ToString());
                rowStrings.Add(Data[i].Stats.Agility.ToString());
                rowStrings.Add(Data[i].Stats.Luck.ToString());
                rowStrings.Add(Data[i].Unknown1.ToString());

                if (Data[i].Inheritance >= 0 && Data[i].Inheritance < InheritanceStrings.Length)
                    rowStrings.Add(InheritanceStrings[Data[i].Inheritance]);
                else
                    rowStrings.Add(Data[i].Inheritance.ToString());

                if (Data[i].Flags == 9984)
                    rowStrings.Add("Can Give Heart Item");
                else if (Data[i].Flags == 12800)
                    rowStrings.Add("Party Members Only");
                else if (Data[i].Flags == 0)
                    rowStrings.Add("None");
                else
                    rowStrings.Add(Data[i].Flags.ToString());

                rowStrings.Add(Data[i].Unknown2.ToString());

                outputBuilder.AppendJoin(',', rowStrings);
                outputBuilder.Append('\n');
            }

            return outputBuilder.ToString();
        }
    }

    struct PersonaData
    {
        public ushort Type;
        public byte Arcana;
        public byte BaseLevel;
        public StatValues Stats;
        public byte Unknown1;
        public byte Inheritance;
        public ushort Flags;
        public byte Unknown2;
    }
}
