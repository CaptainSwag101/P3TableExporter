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
    class EnemyEncounters : TableSegment
    {
        public EncounterData[] Data;
        public const int STRUCT_SIZE = 18 + (2 * 5);

        private readonly string[] MusicStrings =
        {
            "Mass Destruction",
            "UNUSED",
            "Master of Shadow",
            "Unavoidable Battle",
            "Burn My Dread -Last Battle-",
            "The Battle For Everyone's Souls",
            "Master of Tartarus"
        };

        public EnemyEncounters(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();
            int structCount = (segmentSize / STRUCT_SIZE);
            Data = new EncounterData[structCount];

            for (uint i = 0; i < structCount; ++i)
            {
                Data[i] = new();
                Data[i].Flags = reader.ReadUInt32();
                Data[i].Unknown1 = reader.ReadUInt16();
                Data[i].Unknown2 = reader.ReadUInt16();
                Data[i].Units = new ushort[5];
                for (int j = 0; j < 5; ++j)
                {
                    Data[i].Units[j] = reader.ReadUInt16();
                }
                Data[i].FieldID = reader.ReadUInt16();
                Data[i].RoomID = reader.ReadUInt16();
                Data[i].MusicID = reader.ReadUInt16();
                Data[i].Unknown3 = reader.ReadUInt16();
                Data[i].Unknown4 = reader.ReadUInt16();
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
                    0 => "Flags",
                    1 => "Unknown 1",
                    2 => "Unknown 2",
                    3 => "Unit 1",
                    4 => "Unit 2",
                    5 => "Unit 3",
                    6 => "Unit 4",
                    7 => "Unit 5",
                    8 => "Field ID",
                    9 => "Room ID",
                    10 => "Music",
                    11 => "Unknown 3",
                    12 => "Unknown 4",
                    _ => "ERROR"
                };
                rowStrings.Add(colName);
            }
            outputBuilder.AppendJoin(',', rowStrings);
            outputBuilder.Append('\n');

            // Extract Enemy names from MSG.TBL
            string[]? enemyNames = null;
            if (msgTbl != null)
            {
                foreach (TableSegment segment in msgTbl.Segments)
                {
                    if (segment is EnemyNames enemyNameSegment)
                    {
                        enemyNames = enemyNameSegment.Names;
                    }
                }
            }

            // Remaining rows are data
            for (int i = 0; i < Data.Length; ++i)
            {
                rowStrings.Clear();

                rowStrings.Add(Data[i].Flags.ToString());
                rowStrings.Add(Data[i].Unknown1.ToString());
                rowStrings.Add(Data[i].Unknown2.ToString());
                foreach (var unit in Data[i].Units)
                {
                    if (enemyNames != null)
                        rowStrings.Add(enemyNames[unit]);
                    else
                        rowStrings.Add(unit.ToString());
                }
                rowStrings.Add(Data[i].FieldID.ToString());
                rowStrings.Add(Data[i].RoomID.ToString());

                if (Data[i].MusicID >= 0 && Data[i].MusicID < MusicStrings.Length)
                    rowStrings.Add(MusicStrings[Data[i].MusicID]);
                else
                    rowStrings.Add(Data[i].MusicID.ToString());

                rowStrings.Add(Data[i].Unknown3.ToString());
                rowStrings.Add(Data[i].Unknown4.ToString());

                outputBuilder.AppendJoin(',', rowStrings);
                outputBuilder.Append('\n');
            }

            return outputBuilder.ToString();
        }
    }

    struct EncounterData
    {
        public uint Flags;
        public ushort Unknown1;
        public ushort Unknown2;
        public ushort[] Units;
        public ushort FieldID;
        public ushort RoomID;
        public ushort MusicID;
        public ushort Unknown3;
        public ushort Unknown4;
    }
}
