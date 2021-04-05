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
        public PersonaGrowthAndSkills[] DataArray;
        public const int STRUCT_SIZE = 6 + (4 * 16);

        public PersonaGrowthAndSkillsArray(BinaryReader reader)
        {
            int segmentSize = reader.ReadInt32();
            int structCount = (segmentSize / STRUCT_SIZE);
            DataArray = new PersonaGrowthAndSkills[structCount];

            for (uint i = 0; i < structCount; ++i)
            {
                // Read weighted stat growth
                DataArray[i].StrengthGrowthRate = reader.ReadByte();
                DataArray[i].MagicGrowthRate = reader.ReadByte();
                DataArray[i].EnduranceGrowthRate = reader.ReadByte();
                DataArray[i].AgilityGrowthRate = reader.ReadByte();
                DataArray[i].LuckGrowthRate = reader.ReadByte();

                // Read unknown byte (should be padding)
                byte unknown = reader.ReadByte();
                if (unknown != 0)
                    throw new InvalidDataException("The padding byte was non-zero, this could indicate an alignment problem or a fundamental misunderstanding of the table segment data!");

                // Read learnable skills
                DataArray[i].Skills = new PersonaSkill[16];
                for (int j = 0; j < 16; ++j)
                {
                    DataArray[i].Skills[j] = new();
                    DataArray[i].Skills[j].PendingLevels = reader.ReadByte();
                    DataArray[i].Skills[j].Learnable = (reader.ReadByte() > 0);
                    DataArray[i].Skills[j].SkillID = reader.ReadUInt16();
                }
            }
        }

        public override string GenerateRowColumnText(Table? msgTbl = null)
        {
            StringBuilder outputBuilder = new();

            List<string> rowStrings = new();
            for (int col = 0; col < 21; ++col)
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

            foreach (var growthAndSkills in DataArray)
            {
                rowStrings.Clear();

                // Write weighted stat growth values
                rowStrings.Add((growthAndSkills.StrengthGrowthRate + 10).ToString() + '%');
                rowStrings.Add((growthAndSkills.MagicGrowthRate + 10).ToString() + '%');
                rowStrings.Add((growthAndSkills.EnduranceGrowthRate + 10).ToString() + '%');
                rowStrings.Add((growthAndSkills.AgilityGrowthRate + 10).ToString() + '%');
                rowStrings.Add((growthAndSkills.LuckGrowthRate + 10).ToString() + '%');

                // Write skills
                foreach (var skill in growthAndSkills.Skills)
                {
                    string skillText = "";

                    if (!skill.Learnable || skill.SkillID == 0)
                    {
                        break;
                        //skillText += "(Unlearnable) ";
                    }

                    if (skillNames != null)
                        skillText += skillNames[skill.SkillID];
                    else
                        skillText += skill.SkillID.ToString();

                    skillText += $" (base level + {skill.PendingLevels})";

                    rowStrings.Add(skillText);
                }

                outputBuilder.AppendJoin(',', rowStrings);
                outputBuilder.Append('\n');
            }

            return outputBuilder.ToString();
        }
    }

    struct PersonaGrowthAndSkills
    {
        public byte StrengthGrowthRate;
        public byte MagicGrowthRate;
        public byte EnduranceGrowthRate;
        public byte AgilityGrowthRate;
        public byte LuckGrowthRate;
        public PersonaSkill[] Skills;
    }

    struct PersonaSkill
    {
        public byte PendingLevels;
        public bool Learnable;
        public ushort SkillID;
    }
}
