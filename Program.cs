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
using PowerArgs;
using System.Linq;
using System.Diagnostics;

namespace P3TableExporter
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    class LaunchArgs
    {
        [ArgRequired, ArgPosition(0), ArgRegex(@"^(.+)\.[Tt][Bb][Ll]$")]
        public string InputTablePath { get; set; }

        [ArgPosition(1), ArgRegex(@"^(.+)\.[Tt][Bb][Ll]$")]
        public string MsgTblPath { get; set; }

        [ArgPosition(2), ArgRegex(@"^(.+)\.[Cc][Ss][Vv]$")]
        public string OutputTextPath { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    class Program
    {
        static void PrintUsage(string? additionalInfo = null)
        {
            const string usage =
                "Persona 3 Table Exporter, Copyright (C) 2021-2022  James Pelster \"CaptainSwag101\"\n" +
                "Usage: P3TableExporter <input TBL file> [path to MSG.TBL] [output CSV file]";

            Console.WriteLine(usage);
            if (additionalInfo != null) Console.WriteLine(additionalInfo);
        }

        static void Main(string[] args)
        {
            FileInfo InputTableInfo;
            FileInfo? MsgTableInfo = null;
            FileInfo? OutputTextInfo = null;

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

            // Validate input args: process via PowerArgs
            try
            {
                LaunchArgs parsed = Args.Parse<LaunchArgs>(args);

                InputTableInfo = new(parsed.InputTablePath);
                if (parsed.MsgTblPath != null) MsgTableInfo = new(parsed.MsgTblPath);
                if (parsed.OutputTextPath != null) OutputTextInfo = new(parsed.OutputTextPath);
            }
            catch (ArgException ex)
            {
                throw;
            }

            // Validate Input Table
            if (!InputTableInfo.Exists)
            {
                PrintUsage("Error: Input table file does not exist!");
                return;
            }
            else if (InputTableInfo.Length > int.MaxValue)
            {
                PrintUsage("Error: Input table has an absurdly large size (>2 GB)! There's no way this can be correct, exiting...");
                return;
            }

            // Validate Message Table
            if (MsgTableInfo != null)
            {
                if (!MsgTableInfo.Exists)
                {
                    PrintUsage("Error: MSG table file does not exist!");
                    return;
                }
                else if (MsgTableInfo.Length > int.MaxValue)
                {
                    PrintUsage("Error: MSG table has an absurdly large size (>2 GB)! There's no way this can be correct, exiting...");
                }
            }

            // Automatically create an Output CSV, if one wasn't specified explicitly
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
            if (MsgTableInfo != null)
                MessageTable = new(MsgTableInfo);

            _ = 0;  // Breakpoint spot

            // If the data we're writing to the file is shorter than the existing data,
            // it could result in undesirable leftover garbage, so we outright delete the
            // file first to ensure that we start with a clean slate.
            // But first, warn the user that we're about to do so, in case they want to back out.
            if (OutputTextInfo.Exists)
            {
                char check = ' ';
                while (check != 'y' && check != 'n')
                {
                    Console.Write($"The output file \"{OutputTextInfo.FullName}\" already exists, and will be overwritten!\nAre you sure you want to continue? (Y/N): ");
                    string? check2 = Console.ReadLine()?.ToLowerInvariant();
                    if (!string.IsNullOrEmpty(check2))
                    {
                        check = check2[0];
                    }

                    if (check == 'n') return;
                }
                OutputTextInfo.Delete();
            }
            
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
                    _ = 0;  // Breakpoint spot
                }

                // Append a final newline, then proceed to the next segment.
                outputBuilder.Append('\n');
            }

            _ = 0;  // Breakpoint spot

            // Append credits/tagline
            outputBuilder.Append("Data exported with P3TableExporter by CaptainSwag101\n");
            outputBuilder.Append($"Last modified: {DateTime.Today.ToShortDateString()}");

            // When our output is totally built, write it to the output CSV file.
            outputWriter.Write(outputBuilder.ToString());
            outputWriter.Flush();
            outputWriter.Close();
        }
    }
}
