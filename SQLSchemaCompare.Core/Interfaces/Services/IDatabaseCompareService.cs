namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to compare two database instances
    /// </summary>
    public interface IDatabaseCompareService
    {
        /// <summary>
        /// Compares two databases
        /// </summary>
        void StartCompare();
    }
}