using NHibernateAttributesMapper.Entities;
using NHibernateAttributesMapper.Utility;
using NHibernateMapper.Utility;
using System;
using System.Collections.Generic;

namespace NHibernateMapper.Entities
{
    public class Line
    {
        private readonly char spaceSeparator = ' ';
        private readonly Dictionary<string, string> sqlToCsTypes = new()
        {
            { "bigint", "long" },
            { "binary", "byte[]" },
            { "bit", "bool" },
            { "char", "string" },
            { "date", "DateTime" },
            { "datetime", "DateTime" },
            { "datetime2", "DateTime" },
            { "datetimeoffset", "DateTimeOffset" },
            { "decimal", "decimal" },
            { "filestream", "byte[]" },
            { "float", "double" },
            { "image", "byte[]" },
            { "int", "int" },
            { "money", "decimal" },
            { "nchar", "string" },
            { "ntext", "string" },
            { "numeric", "decimal" },
            { "nvarchar", "string" },
            { "rowversion", "byte[]" },
            { "smalldatetime", "DateTime" },
            { "smallint", "short" },
            { "smallmoney", "decimal" },
            { "text", "string" },
            { "time", "TimeAsTimeSpan" },
            { "timestamp", "byte[]" },
            { "tinyint", "byte" },
            { "uniqueidentifier", "Guid" },
            { "varbinary", "byte[]" },
            { "varchar", "string" },
            { "xml", "string" }
        };

        public Line(string clearLine)
        {
            LineText = clearLine;
            var lineWords = clearLine.Split(spaceSeparator);

            SetColumnName(lineWords[0]);
            SetPropertyName(lineWords[0]);

            SetSqlType(lineWords[1]);
            SetSqlSize(lineWords[1]);

            SetIsNull(lineWords);
            SetIdentity(lineWords);
            SetCsType();
            SetAttribute();
            SetProperty();

            //TODO: Check if line has DEFAULT and add a comment. (Consider adding a property Comment to the Line class)
        }

        private void SetIdentity(string[] lineWords)
        {
            foreach (var word in lineWords)
            {
                if (word.StartsWith(Constants.Identity, StringComparison.InvariantCultureIgnoreCase))
                {
                    var braces = word.Substring(8); //Take the braces that start right after the word "identity" - index 8
                    var numbers = braces.Unwrap('(', ')').Split(',');
                    Identity = new Identity
                    {
                        IsIdentity = true,
                        Seed = int.Parse(numbers[0]),
                        Increment = int.Parse(numbers[1])
                    };
                }
            }
        }

        private void SetIsNull(string[] lineWords)
        {
            for (int i = 1; i < lineWords.Length; i++)
            {
                //Use StartWith because it may be the last word and contain the comma at the end.
                if (lineWords[i].StartsWith(Constants.Null, StringComparison.InvariantCultureIgnoreCase))
                {
                    IsNull = !lineWords[i - 1].Equals(Constants.Not, StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }

        private void SetSqlSize(string v)
        {
            int size = 1;
            if (v.Contains('(') && v.Contains(')'))
            {
                int.TryParse(v.Substring(v.IndexOf('(') + 1, v.IndexOf(')') - v.IndexOf('(') - 1), out size);
            }

            SqlSize = size;
        }

        public void SetAsPrimaryKey()
        {
            IsPrimaryKey = true;
            SetAttribute();
        }

        private void SetSqlType(string v)
        {
            SqlType = v.Contains('(') ? v.Substring(0, v.IndexOf('(')) : v;
        }

        private void SetColumnName(string columnName)
        {
            ColumnName = columnName;
        }

        private void SetPropertyName(string propertyName)
        {
            PropertyName = propertyName.Unwrap().ToTitleCase();
        }

        internal bool IsValid()
        {
            return !string.IsNullOrEmpty(Attribute) && !string.IsNullOrWhiteSpace(ColumnName) && !string.IsNullOrWhiteSpace(SqlType);
        }

        private void SetCsType()
        {
            CsharpType = sqlToCsTypes.GetValueOrDefault(SqlType.Unwrap());

            if (IsNull && CsharpType != "string")
            {
                CsharpType += "?";
            }
        }

        private void SetAttribute()
        {
            if (sqlToCsTypes.TryGetValue(SqlType.Unwrap(), out string attributeType))
            {
                if (IsPrimaryKey)
                {
                    Attribute = $"[Id(Name = \"{PropertyName}\", Column = \"{ColumnName}\", Type = \"{attributeType}\")]";
                    if (Identity.IsIdentity)
                    {
                        Attribute += $"\n[Generator(1, Class = \"native\")]";
                    }
                }
                else
                {
                    Attribute = $"[Property(Column = \"{ColumnName}\", Type = \"{attributeType}\", NotNull = {(!IsNull).ToString().ToLower()})]";
                }
            }
            else
            {
                Attribute = "/* Note: Unable to create attribute for the following type. Please update manually */";
            }
        }

        private void SetProperty()
        {
            Property = $"public {CsharpType} {PropertyName} {{ get; set; }}";
        }

        /// <summary>
        /// Raw text of the current line
        /// </summary>
        public string LineText { get; set; }
        /// <summary>
        /// The name as it appears in the SQL Script (Database table)
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// The name of the property as it will appear in the code
        /// </summary>
        public string PropertyName { get; set; }
        public string SqlType { get; set; }
        public int SqlSize { get; set; }
        /// <summary>
        /// By default, a column holds NULL values
        /// </summary>
        public bool IsNull { get; set; } = true;
        public string CsharpType { get; set; }
        /// <summary>
        /// NHibernate Attribute to add on top of the property
        /// </summary>
        public string Attribute { get; set; }
        /// <summary>
        /// The full property as it appears in the class
        /// Including access modifier, type, name, getter and setter.
        /// </summary>
        public string Property { get; set; }
        public bool IsPrimaryKey { get; set; }
        public Identity Identity { get; set; }
    }
}
