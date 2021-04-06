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

using P3TableExporter.TableSegments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P3TableExporter
{
    class Table
    {
        public List<TableSegment> Segments { get; set; }

        public Table()
        {
            Segments = new();
        }

        public Table(FileInfo info)
        {
            Segments = new();

            using FileStream stream = info.OpenRead();
            using BinaryReader reader = new(stream);

            // Determine what table segments we need based on the table filename
            string tableName = info.Name.ToUpperInvariant().Split(".")[0];
            if (tableName == "PERSONA" || tableName == "PERSONA_F")
            {
                Segments.Add(new PersonaDataArray(reader));
                Utils.ReadPadding(reader, 16);
                Segments.Add(new PersonaGrowthAndSkillsArray(reader));
                Utils.ReadPadding(reader, 16);
            }
            else if (tableName == "MSG")
            {
                Segments.Add(new ArcanaNames(reader));
                Utils.ReadPadding(reader, 16);
                Segments.Add(new SkillNames(reader));
                Utils.ReadPadding(reader, 16);
                Segments.Add(new EnemyNames(reader));
                Utils.ReadPadding(reader, 16);
                Segments.Add(new PersonaNames(reader));
                Utils.ReadPadding(reader, 16);
                Segments.Add(new TextBMDs(reader));
                Utils.ReadPadding(reader, 16);
            }
            else
            {
                throw new NotImplementedException($"Table type {tableName} is not currently supported.");
            }

            return;
        }
    }
}
