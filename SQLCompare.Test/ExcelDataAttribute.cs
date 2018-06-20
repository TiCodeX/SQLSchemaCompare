using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace SQLCompare.Test
{
    /// <summary>
    /// Provides an excel data source for a data theory
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ExcelDataAttribute : DataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelDataAttribute"/> class.
        /// </summary>
        /// <param name="filePath">The path of the excel file</param>
        public ExcelDataAttribute(string filePath)
        {
            this.FilePath = filePath;
        }

        /// <summary>
        /// Gets the path of the excel file
        /// </summary>
        public string FilePath { get; }

        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException(nameof(testMethod));
            }

            // Get the absolute path to the file
            var path = Path.IsPathRooted(this.FilePath)
                ? this.FilePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), this.FilePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }

            using (var p = new ExcelPackage(new FileInfo(path)))
            {
                // Retrieve first Worksheet
                var ws = p.Workbook.Worksheets.First();

                // Read the first Row for the column names and place into a list so that
                // it can be used as reference to properties
                var columnNames = new Dictionary<string, int>();
                var colPosition = 0;
                foreach (var cell in ws.Cells[1, 1, 1, ws.Dimension.Columns])
                {
                    columnNames.Add(cell.Value.ToString().ToUpperInvariant(), colPosition++);
                }

                // Loop through the rows of the excel sheet
                for (var rowNum = 2; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Cells.Count()];

                    var objArr = new List<object>();
                    foreach (var parameterInfo in testMethod.GetParameters())
                    {
                        if (IsSimpleType(parameterInfo.ParameterType))
                        {
                            if (!columnNames.TryGetValue(
                                $"{parameterInfo.Name}".ToUpperInvariant(),
                                out var position))
                            {
                                throw new KeyNotFoundException($"Could not find column with header: {parameterInfo.Name}");
                            }

                            objArr.Add(ConvertParameter(parameterInfo.ParameterType, wsRow[rowNum, position + 1].Value));
                        }
                        else
                        {
                            var obj = Activator.CreateInstance(parameterInfo.ParameterType);
                            objArr.Add(obj);

                            this.RecursiveSetProperty(columnNames, wsRow, rowNum, parameterInfo.Name, obj);
                        }
                    }

                    yield return objArr.ToArray();
                }
            }
        }

        private static object ConvertParameter(Type parameterType, object value)
        {
            var type = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
            return value == null ? null : Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                new[]
                {
                    typeof(Enum),
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
        }

        private void RecursiveSetProperty(IReadOnlyDictionary<string, int> columnNames, ExcelRange wsRow, int rowNum, string parameterPrefix, object obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (!IsSimpleType(propertyInfo.PropertyType))
                {
                    this.RecursiveSetProperty(columnNames, wsRow, rowNum, $"{parameterPrefix}.{propertyInfo.Name}", propertyInfo.GetValue(obj));
                }

                if (!columnNames.TryGetValue(
                    $"{parameterPrefix}.{propertyInfo.Name}".ToUpperInvariant(),
                    out var position))
                {
                    continue;
                }

                propertyInfo.SetValue(obj, ConvertParameter(propertyInfo.PropertyType, wsRow[rowNum, position + 1].Value));
            }
        }
    }
}