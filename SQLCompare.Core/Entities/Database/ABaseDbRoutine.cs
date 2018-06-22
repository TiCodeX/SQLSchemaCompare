namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database routine classes
    /// </summary>
    public abstract class ABaseDbRoutine : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the database routine definition script
        /// </summary>
        public string RoutineDefinition { get; set; }
    }
}