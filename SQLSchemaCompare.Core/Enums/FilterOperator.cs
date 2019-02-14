namespace TiCodeX.SQLSchemaCompare.Core.Enums
{
    /// <summary>
    /// List of possible operators for the filtering clauses
    /// </summary>
    public enum FilterOperator
    {
        /// <summary>
        /// Filter objects that begins with a specific value
        /// </summary>
        BeginsWith = 0,

        /// <summary>
        /// Filter objects that ends with a specific value
        /// </summary>
        EndsWith = 1,

        /// <summary>
        /// Filter objects that contains a specific value
        /// </summary>
        Contains = 2,

        /// <summary>
        /// Filter objects that are equal to a specific value
        /// </summary>
        Equals = 3,

        /// <summary>
        /// Filter objects that not begins with a specific value
        /// </summary>
        NotBeginsWith = 4,

        /// <summary>
        /// Filter objects that not ends with a specific value
        /// </summary>
        NotEndsWith = 5,

        /// <summary>
        /// Filter objects that not contains a specific value
        /// </summary>
        NotContains = 6,

        /// <summary>
        /// Filter objects that are not equal to a specific value
        /// </summary>
        NotEquals = 7,
    }
}
