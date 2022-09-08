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
    class PersonaGrowthAndSkillsArray : TableSegment
    {
        public PersonaGrowthAndSkills[] Data;
        public const int STRUCT_SIZE = 6 + (4 * 16);
        public int MostLearnableSkills = 0;

        public PersonaGrowthAndSkillsArray(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();
            int structCount = (segmentSize / STRUCT_SIZE);
            Data = new PersonaGrowthAndSkills[structCount];

            for (uint i = 0; i < structCount; ++i)
            {
                // Read weighted stat growth
                Data[i].StatGrowthWeights.Strength = reader.ReadByte();
                Data[i].StatGrowthWeights.Magic = reader.ReadByte();
                Data[i].StatGrowthWeights.Endurance = reader.ReadByte();
                Data[i].StatGrowthWeights.Agility = reader.ReadByte();
                Data[i].StatGrowthWeights.Luck = reader.ReadByte();

                // Read unknown byte (should be padding)
                byte unknown = reader.ReadByte();
                if (unknown != 0)
                    throw new InvalidDataException("The padding byte was non-zero, this could indicate an alignment problem or a fundamental misunderstanding of the table segment data!");

                // Read learnable skills. Also figure out the maximum number of
                // learnable skills by any Persona, so we don't need to
                // go all the way to 21 columns if we don't need to.
                Data[i].Skills = new PersonaSkill[16];
                for (int j = 0; j < 16; ++j)
                {
                    Data[i].Skills[j] = new();
                    Data[i].Skills[j].PendingLevels = reader.ReadByte();
                    Data[i].Skills[j].Learnable = (reader.ReadByte() > 0);
                    Data[i].Skills[j].SkillID = reader.ReadUInt16();

                    if (Data[i].Skills[j].Learnable && Data[i].Skills[j].SkillID != 0)
                        if (j > MostLearnableSkills)
                            MostLearnableSkills = j;
                }

            }
        }

        public override string GenerateRowColumnText(Table? msgTbl = null)
        {
            StringBuilder outputBuilder = new();

            List<string> rowStrings = new();
            for (int col = 0; col < 5 + MostLearnableSkills; ++col)
            {
                string colName = col switch
                {
                    0 => "Strength Growth Weight",
                    1 => "Magic Growth Weight",
                    2 => "Endurance Growth Weight",
                    3 => "Agility Growth Weight",
                    4 => "Luck Growth Weight",
                    5 => "Skill 0",
                    6 => "Skill 1",
                    7 => "Skill 2",
                    8 => "Skill 3",
                    9 => "Skill 4",
                    10 => "Skill 5",
                    11 => "Skill 6",
                    12 => "Skill 7",
                    13 => "Skill 8",
                    14 => "Skill 9",
                    15 => "Skill 10",
                    16 => "Skill 11",
                    17 => "Skill 12",
                    18 => "Skill 13",
                    19 => "Skill 14",
                    20 => "Skill 15",
                    _ => "ERROR"
                };
                rowStrings.Add(colName);
            }
            rowStrings.Add("Last skill @ base lvl +");
            outputBuilder.AppendJoin(',', rowStrings);
            outputBuilder.Append('\n');

            // Extract Skill names from MSG.TBL
            string[]? skillNames = null;
            if (msgTbl != null)
            {
                foreach (TableSegment segment in msgTbl.Segments)
                {
                    if (segment is SkillNames skillNameSegment)
                    {
                        skillNames = skillNameSegment.Names;
                    }
                }
            }

            foreach (var growthAndSkills in Data)
            {
                rowStrings.Clear();

                // Write weighted stat growth values
                rowStrings.Add((growthAndSkills.StatGrowthWeights.Strength + 10).ToString() + '%');
                rowStrings.Add((growthAndSkills.StatGrowthWeights.Magic + 10).ToString() + '%');
                rowStrings.Add((growthAndSkills.StatGrowthWeights.Endurance + 10).ToString() + '%');
                rowStrings.Add((growthAndSkills.StatGrowthWeights.Agility + 10).ToString() + '%');
                rowStrings.Add((growthAndSkills.StatGrowthWeights.Luck + 10).ToString() + '%');

                // Write skills
                int highestLevelSkill = 0;
                for (int skillNum = 0; skillNum < MostLearnableSkills; ++skillNum)
                {
                    var skill = growthAndSkills.Skills[skillNum];

                    string skillText = "";
                    if (skill.Learnable && skill.SkillID != 0)
                    {
                        if (skillNames != null)
                            skillText += skillNames[skill.SkillID];
                        else
                            skillText += skill.SkillID.ToString();

                        skillText += $" (base level + {skill.PendingLevels})";

                        if (skill.PendingLevels > highestLevelSkill)
                            highestLevelSkill = skill.PendingLevels;
                    }
                    rowStrings.Add(skillText);
                }

                // Write highest level skill
                rowStrings.Add($"{highestLevelSkill}");

                outputBuilder.AppendJoin(',', rowStrings);
                outputBuilder.Append('\n');
            }

            return outputBuilder.ToString();
        }
    }

    struct PersonaGrowthAndSkills
    {
        public StatValues StatGrowthWeights;
        public PersonaSkill[] Skills;
    }

    struct PersonaSkill
    {
        public byte PendingLevels;
        public bool Learnable;
        public ushort SkillID;
    }
}
