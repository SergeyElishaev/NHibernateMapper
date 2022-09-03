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
            var items = clearLine.Split(spaceSeparator);

            SetColumnName(items[0]);
            SetPropertyName(items[0]);
            SetSqlType(items[1]);
            SetSqlSize(items[1]);
            IsNull = items[2].ToLower().StartsWith("null");
            SetCsType();
            SetAttribute();
            SetProperty();
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
            PropertyName = propertyName.ToTitleCase();
        }

        internal bool IsValid()
        {
            return !string.IsNullOrEmpty(Attribute) && !string.IsNullOrWhiteSpace(ColumnName) && !string.IsNullOrWhiteSpace(SqlType);
        }

        private void SetCsType()
        {
            CsType = sqlToCsTypes.GetValueOrDefault(SqlType.Unwrap());

            if (IsNull && CsType != "string")
            {
                CsType += "?";
            }
        }

        //TODO: Identify Identity on ID 
        private void SetAttribute()
        {
            if (sqlToCsTypes.TryGetValue(SqlType.Unwrap(), out string attributeType))
            {
                if (IsPrimaryKey)
                {
                    Attribute = $"[Id(Name = \"{PropertyName}\", Column = \"{ColumnName}\", Type = \"{attributeType}\")]";
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
            Property = $"public {CsType} {PropertyName.Unwrap()} {{ get; set; }}";
        }

        public string LineText { get; set; }
        public string ColumnName { get; set; }
        public string PropertyName { get; set; }
        public string SqlType { get; set; }
        public int SqlSize { get; set; }
        public bool IsNull { get; set; }
        public string CsType { get; set; }
        public string Attribute { get; set; }
        public string Property { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}
