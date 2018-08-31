using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql function definition
    /// </summary>
    public class PostgreSqlFunction : ABaseDbFunction
    {
        /// <summary>
        /// Gets or sets the external language
        /// </summary>
        public string ExternalLanguage { get; set; }

        /// <summary>
        /// Gets or sets the security type
        /// </summary>
        public string SecurityType { get; set; }

        /// <summary>
        /// Gets or sets the function cost
        /// </summary>
        public float Cost { get; set; }

        /// <summary>
        /// Gets or sets the function rows
        /// </summary>
        public float Rows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the function is strict
        /// </summary>
        public bool IsStrict { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the function returns a set
        /// </summary>
        public bool ReturnSet { get; set; }

        /// <summary>
        /// Gets or sets the function volatile value
        /// </summary>
        public char Volatile { get; set; }

        /// <summary>
        /// Gets or sets the function arguments count
        /// </summary>
        public short ArgsCount { get; set; }

        /// <summary>
        /// Gets or sets the function return type
        /// </summary>
        public uint ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the function argument types
        /// </summary>
        public IEnumerable<uint> ArgTypes { get; set; }

        /// <summary>
        /// Gets or sets the function all argument types
        /// </summary>
        public IEnumerable<uint> AllArgTypes { get; set; }

        /// <summary>
        /// Gets or sets the function argument modes
        /// </summary>
        public IEnumerable<char> ArgModes { get; set; }

        /// <summary>
        /// Gets or sets the function argument names
        /// </summary>
        public IEnumerable<string> ArgNames { get; set; }
    }
}
