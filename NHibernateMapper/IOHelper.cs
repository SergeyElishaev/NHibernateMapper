using NHibernateMapper.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NHibernateMapper
{
    static class IOHelper
    {
        private static readonly string titleLine = new('-', 50);
        private static readonly string nhibernateMappingAttribute = "[HibernateMapping(Assembly = \"DAL\", Namespace = \"DAL.Models\")]";
        private static readonly string classAttribute = "[Class(1, Schema = \"{0}\", Table = \"{1}\", Lazy = false)]";

        public static void WriteToFile(List<Line> lines, string fullTableName, string outputFilePath)
        {
            using StreamWriter file = new(outputFilePath);

            var tableNameParts = fullTableName.Split('.');
            var schemaName = tableNameParts[0].Trim('[', ']');
            var tableName = tableNameParts[1].Trim('[', ']');

            file.WriteLine(nhibernateMappingAttribute);
            file.WriteLine(classAttribute, schemaName, tableName);
            file.WriteLine($"public class {tableName}");
            file.WriteLine("{");
            foreach (var line in lines.Where(l => l.IsValid()))
            {
                file.WriteLine(line.Attribute);
                file.WriteLine(line.Property);
                file.WriteLine();
            }
            file.WriteLine("}");
        }

        public static void WriteToConsole(List<Line> lines, string tableName)
        {
            Console.WriteLine(tableName);
            Console.WriteLine(titleLine);

            foreach (var line in lines.Where(l => l.IsValid()))
            {
                Console.WriteLine(line.Attribute);
                Console.WriteLine(line.Property);
                Console.WriteLine();
           }
        }
    }
}
