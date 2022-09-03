using NHibernateMapper.Entities;
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
            var allLines = File.ReadAllLines(inputFilePath);

            var createLineIndex = Array.FindIndex(allLines, l => l.Contains("CREATE TABLE", StringComparison.InvariantCultureIgnoreCase));

            var tableName = GetTableName(allLines[createLineIndex]);
            var lines = GenerateLines(allLines, createLineIndex, out int processedLineIndex);

            UpdateConstraints(ref lines, allLines, processedLineIndex);

            IOHelper.WriteToFile(lines, tableName, outputFilePath);
            IOHelper.WriteToConsole(lines, tableName);
        }

        private static void UpdateConstraints(ref List<Line> lines, string[] allLines, int processedLineIndex)
        {
            while (processedLineIndex < allLines.Length)
            {
                if (allLines[processedLineIndex].Trim().StartsWith("CONSTRAINT", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (allLines[processedLineIndex].Contains("PRIMARY KEY", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //TODO: assign PK attribute to line
                    }
                }
            }
        }

        private static List<Line> GenerateLines(string[] allLines, int createLineIndex, out int processedLineIndex)
        {
            var result = new List<Line>();
            
            for (processedLineIndex = createLineIndex + 1; processedLineIndex < allLines.Length; processedLineIndex++)
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
                "GO", "COMMIT", "CONSTRAINT", ")"
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
