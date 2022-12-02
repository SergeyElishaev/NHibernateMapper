using NHibernateAttributesMapper.Utility;
using NHibernateMapper.Entities;
using NHibernateMapper.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NHibernateMapper
{
    static class MappingHelper
    {
        /// <summary>
        /// Takes SQL CREATE Query and convert it to C# Model with Nhibernate Mapping Attributes
        /// </summary>
        /// <param name="sqlCreateQuery"></param>
        /// <returns>C# Mapping File</returns>
        public static void Map(string inputFilePath, string outputFilePath)
        {
            var rawLines = File.ReadAllLines(inputFilePath);

            var createLineIndex = Array.FindIndex(rawLines, l => l.Contains(Constants.CreateTable, StringComparison.InvariantCultureIgnoreCase));

            var tableName = GetTableName(rawLines[createLineIndex]);
            var lines = GenerateLines(rawLines, createLineIndex);

            UpdateConstraints(ref lines, rawLines, createLineIndex + lines.Count);

            IOHelper.WriteToFile(lines, tableName, outputFilePath);
            IOHelper.WriteToConsole(lines, tableName);
        }

        private static void UpdateConstraints(ref List<Line> lines, string[] rawLines, int processedLineIndex)
        {
            while (processedLineIndex < rawLines.Length)
            {
                if (rawLines[processedLineIndex].Trim().StartsWith(Constants.Constraint, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (rawLines[processedLineIndex].Contains(Constants.PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //Find the column name in the first braces after "PRIMARY KEY".
                        while (!rawLines[processedLineIndex].Contains('(') && processedLineIndex < rawLines.Length)
                        {
                            processedLineIndex++;
                        }
                        if (processedLineIndex >= rawLines.Length)
                        {
                            break;
                        }
                        var columnName = "";
                        var indexOfOpeningBracket = rawLines[processedLineIndex].IndexOf('(');
                        var hasLettersAfterOpeningBracket = rawLines[processedLineIndex][indexOfOpeningBracket..].Any(c => char.IsLetter(c));

                        if (hasLettersAfterOpeningBracket)
                        {
                            //Take column name from the current line
                            columnName = rawLines[processedLineIndex][(indexOfOpeningBracket + 1)..].Unwrap();
                        }
                        processedLineIndex++;
                        while (processedLineIndex < rawLines.Length && !rawLines[processedLineIndex].Any(c=>char.IsLetter(c)))
                        {
                            processedLineIndex++;
                        }

                        //If this line is reached - means there are letters in it. Take the first word as the column name for Primary Key
                        //TODO: Improve functionality to support Primary Keys with multiple columns.
                        columnName = rawLines[processedLineIndex].Trim().Split(' ')[0].Unwrap();
                        var keyLine = lines.Where(l => l.ColumnName.Unwrap() == columnName).FirstOrDefault();
                        if (keyLine != null)
                        {
                            keyLine.SetAsPrimaryKey();
                        }
                    }
                    //TODO: Handle other constraints (Add a comment with details)
                }
                processedLineIndex++;
            }
        }

        private static List<Line> GenerateLines(string[] allLines, int createLineIndex)
        {
            var result = new List<Line>();
            
            for (var processedLineIndex = createLineIndex + 1; processedLineIndex < allLines.Length; processedLineIndex++)
            {
                var currentLine = TrimLine(allLines[processedLineIndex]);

                if (HasTerminateKeyword(currentLine))
                {
                    break;
                }
                if (IsValidLine(currentLine))
                {
                    result.Add(GenerateSingleLine(currentLine));
                }
            }

            return result;
        }

        private static bool HasTerminateKeyword(string line)
        {
            var terminateKeywords = new List<string>
            {
                Constants.Go, Constants.Commit, Constants.Constraint, ")"
            };

            return line == null || terminateKeywords.Any(k => line.TrimStart().StartsWith(k, StringComparison.InvariantCultureIgnoreCase));
        }

        private static bool IsValidLine(string line)
        {
            return line.Trim().Length > 5 && line.Any(c => char.IsLetter(c));
        }

        private static Line GenerateSingleLine(string line)
        {
            return new Line(line);
        }

        public static string GetTableName(string createTableLine)
        {
            var lastStringInLine = createTableLine.Split(' ').Last(item => !string.IsNullOrEmpty(item));

            return lastStringInLine.TrimEnd('(');
        }

        private static string TrimLine(string line)
        {
            var charsToTrim = new char[] { '\n', '\t', '\r' };

            return line.Trim(charsToTrim);
        }
    }
}
