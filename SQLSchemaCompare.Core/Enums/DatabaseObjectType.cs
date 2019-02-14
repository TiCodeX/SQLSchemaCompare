namespace TiCodeX.SQLSchemaCompare.Core.Enums
{
    /// <summary>
    /// List of database object types
    /// </summary>
    public enum DatabaseObjectType
    {
        /// <summary>
        /// The schema
        /// </summary>
        Schema = 0,

        /// <summary>
        /// The table
        /// </summary>
        Table = 1,

        /// <summary>
        /// The column
        /// </summary>
        Column = 2,

        /// <summary>
        /// The primary key
        /// </summary>
        PrimaryKey = 3,

        /// <summary>
        /// The foreign key
        /// </summary>
        ForeignKey = 4,

        /// <summary>
        /// The index
        /// </summary>
        Index = 5,

        /// <summary>
        /// The constraint
        /// </summary>
        Constraint = 6,

        /// <summary>
        /// The trigger
        /// </summary>
        Trigger = 7,

        /// <summary>
        /// The view
        /// </summary>
        View = 8,

        /// <summary>
        /// The function
        /// </summary>
        Function = 9,

        /// <summary>
        /// The stored procedure
        /// </summary>
        StoredProcedure = 10,

        /// <summary>
        /// The data type
        /// </summary>
        DataType = 11,

        /// <summary>
        /// The sequence
        /// </summary>
        Sequence = 12,
    }
}
