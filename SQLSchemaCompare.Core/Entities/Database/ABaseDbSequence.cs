namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database sequence classes
    /// </summary>
    public class ABaseDbSequence : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the data type of the sequence
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the start value
        /// </summary>
        public string StartValue { get; set; }

        /// <summary>
        /// Gets or sets the increment value
        /// </summary>
        public string Increment { get; set; }

        /// <summary>
        /// Gets or sets the minimum value
        /// </summary>
        public string MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value
        /// </summary>
        public string MaxValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is cycling
        /// </summary>
        public bool IsCycling { get; set; }
    }
}
