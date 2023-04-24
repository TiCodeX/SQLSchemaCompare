namespace TiCodeX.SQLSchemaCompare.Test
{
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    /// <summary>
    /// Implementation of the ILoggerFactory interface that uses the Xunit log system to write
    /// </summary>
    public sealed class XunitLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// The output
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitLoggerFactory"/> class.
        /// </summary>
        /// <param name="output">The Xunit logger</param>
        public XunitLoggerFactory(ITestOutputHelper output)
        {
            this.output = output;
        }

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
