namespace TiCodeX.SQLSchemaCompare.Test
{
    /// <summary>
    /// Implementation of the ILoggerFactory interface that uses the Xunit log system to write
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="XunitLoggerFactory"/> class.
    /// </remarks>
    /// <param name="output">The Xunit logger</param>
    public sealed class XunitLoggerFactory(ITestOutputHelper output) : ILoggerFactory
    {
        /// <summary>
        /// The output
        /// </summary>
        private readonly ITestOutputHelper output = output;

        /// <inheritdoc/>
        public void AddProvider(ILoggerProvider provider)
        {
            // Do nothing
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(categoryName, this.output);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do nothing
        }
    }
}
