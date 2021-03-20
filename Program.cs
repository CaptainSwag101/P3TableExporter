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
using System.Text;

namespace P3TableExporter
{
    class Program
    {
        static void PrintUsage(string? additionalInfo = null)
        {
            const string usage =
                "Persona 3 Table Exporter, Copyright (C) 2021  James Pelster \"CaptainSwag101\"\n" +
                "Usage: P3TableExporter <input TBL file> [path to MSG.TBL] [output CSV file]";

            Console.WriteLine(usage);
            if (additionalInfo != null) Console.WriteLine(additionalInfo);
        }

        static void Main(string[] args)
        {
            FileInfo? InputTableInfo = null;
            FileInfo? OutputTextInfo = null;
            FileInfo? MsgTblInfo = null;

            // Validate input args: Preliminary checks
            if (args.Length == 0)
            {
                PrintUsage("Error: No input arguments provided!");
                return;
            }
            else if (args.Length > 3)
            {
                PrintUsage("Error: Too many input arguments provided! (max. 3)");
                return;
            }

            // Validate input args: First arg (always input TBL)
            InputTableInfo = new(args[0]);
            if (!InputTableInfo.Exists)
            {
                PrintUsage("Error: Input table file does not exist!");
                return;
            }
            else if (InputTableInfo.Extension.ToUpperInvariant() != ".TBL")
            {
                PrintUsage("Error: Input table file does not have the .TBL extension! Are you sure it's valid?");
                return;
            }
            else if (InputTableInfo.Length > int.MaxValue)
            {
                PrintUsage("Error: Input file has an absurdly large size (>2 GB)! This is absolutely invalid.");
                return;
            }

            // Validate input args: Remaining arguments
            for (int a = 1; a < args.Length; ++a)
            {
                string arg = args[a];
                FileInfo argInfo = new(arg);

                if (!argInfo.Exists)
                {
                    PrintUsage($"Error: File {arg} does not exist!");
                    return;
                }

                // We don't know what kind of file the current argument is because they're both optional,
                // so determine which file this is based on the extension.
                if (argInfo.Extension.ToUpperInvariant() == ".CSV")
                {
                    if (OutputTextInfo == null)
                    {
                        OutputTextInfo = argInfo;
                    }
                    else
                    {
                        PrintUsage("Error: An output CSV path was already provided!");
                        return;
                    }
                }
                else if (argInfo.Name.ToUpperInvariant() == "MSG.TBL")
                {
                    if (MsgTblInfo == null)
                    {
                        MsgTblInfo = argInfo;
                    }
                    else
                    {
                        PrintUsage("Error: Path to MSG.TBL was already provided!");
                        return;
                    }
                }
            }

            // Validate input args: Output CSV path, if one wasn't specified
            if (OutputTextInfo == null)
            {
                // Copy the input path but change the extension to "csv"
                string outPath = InputTableInfo.FullName.Split(".")[0] + ".csv";
                OutputTextInfo = new(outPath);
            }

            // Okay, all of our input arguments should be validated now, and we should have some sort of valid state.
            _ = 0;  // Breakpoint spot

            // Build the input table from the TBL file
            Table InputTable = new(InputTableInfo);

            // If MSG.TBL is defined, load it too!
            Table? MessageTable = null;
            if (MsgTblInfo != null)
                MessageTable = new(MsgTblInfo);

            _ = 0;  // Breakpoint spot

            // Output each segment of the input table
            using FileStream outputStream = OutputTextInfo.OpenWrite();
            using StreamWriter outputWriter = new(outputStream);
            StringBuilder outputBuilder = new();

            foreach (TableSegment segment in InputTable.Segments)
            {
                // The first row of each table will be populated with the table name.
                outputBuilder.Append(segment.GetType().ToString() + '\n');

                // The second row of each table will be populated with the name of its values,
                // and following rows will contain actual table data.
                try
                {
                    outputBuilder.Append(segment.GenerateRowColumnText(MessageTable));
                }
                catch (NotImplementedException ex)
                {
                    // Most tables' output functions are not implemented yet, so this
                    // catch block lets us at least test the ones I have implemented.
                }

                // Append a final newline, then proceed to the next segment.
                outputBuilder.Append('\n');
            }

            _ = 0;  // Breakpoint spot

            // When our output is totally built, write it to the output CSV file.
            outputWriter.Write(outputBuilder.ToString());
            outputWriter.Flush();
        }
    }
}
