namespace TiCodeX.SQLSchemaCompare.Test.Integration;

/// <summary>
/// The interface for integration tests.
/// </summary>
public interface IIntegrationTests
{
    /// <summary>
    /// Test the retrieval of database list
    /// </summary>
    /// <param name="port">The port of the server</param>
    void GetDatabaseList(ushort port);

    /// <summary>
    /// Test the retrieval of the database
    /// </summary>
    /// <param name="port">The port of the server</param>
    void GetDatabase(ushort port);

    /// <summary>
    /// Test cloning the database
    /// </summary>
    /// <param name="port">The port of the server</param>
    void CloneDatabase(ushort port);
}
