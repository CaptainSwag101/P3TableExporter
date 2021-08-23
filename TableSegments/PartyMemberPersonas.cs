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
    class PartyMemberPersonas : TableSegment
    {
        public PartyMemberPersonaData[] Data;
        public const int STRUCT_SIZE = 4 + (4 * 32) + (5 * 98);

        public PartyMemberPersonas(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();
            int structCount = (segmentSize / STRUCT_SIZE);
            Data = new PartyMemberPersonaData[structCount];

            for (int i = 0; i < structCount; ++i)
            {
                Data[i].CharacterID = reader.ReadUInt16();
                Data[i].LevelsAvailable = reader.ReadUInt16();
                Data[i].Skills = new PartyMemberSkillData[32];
                for (int j = 0; j < 32; ++j)
                {
                    Data[i].Skills[j].LevelLearned = reader.ReadByte();
                    Data[i].Skills[j].Learnable = (reader.ReadByte() > 0);
                    Data[i].Skills[j].SkillID = reader.ReadUInt16();
                }
                Data[i].StatGainsByLevel = new StatValues[98];
                for (int j = 0; j < 98; ++j)
                {
                    Data[i].StatGainsByLevel[j].Strength = reader.ReadByte();
                    Data[i].StatGainsByLevel[j].Magic = reader.ReadByte();
                    Data[i].StatGainsByLevel[j].Endurance = reader.ReadByte();
                    Data[i].StatGainsByLevel[j].Agility = reader.ReadByte();
                    Data[i].StatGainsByLevel[j].Luck = reader.ReadByte();
                }
            }
        }

        public override string GenerateRowColumnText(Table? msgTbl = null)
        {
            throw new NotImplementedException();
        }
    }

    struct PartyMemberPersonaData
    {
        public ushort CharacterID;
        public ushort LevelsAvailable;
        public PartyMemberSkillData[] Skills;
        public StatValues[] StatGainsByLevel;
    }

    struct PartyMemberSkillData
    {
        public byte LevelLearned;
        public bool Learnable;
        public ushort SkillID;
    }
}
