namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    using System.Collections.Generic;

    /// <summary>
    /// Specific PostgreSql function definition
    /// </summary>
    public class PostgreSqlFunction : ABaseDbFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlFunction"/> class
        /// </summary>
        public PostgreSqlFunction()
        {
            this.AlterScriptSupported = false;
        }

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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is aggregate
        /// </summary>
        public bool IsAggregate { get; set; }

        /// <summary>
        /// Gets or sets the aggregate transition function
        /// </summary>
        public string AggregateTransitionFunction { get; set; }

        /// <summary>
        /// Gets or sets the aggregate transition type
        /// </summary>
        public uint AggregateTransitionType { get; set; }

        /// <summary>
        /// Gets or sets the aggregate final function
        /// </summary>
        public string AggregateFinalFunction { get; set; }

        /// <summary>
        /// Gets or sets the aggregate initial value
        /// </summary>
        public string AggregateInitialValue { get; set; }
    }
}
