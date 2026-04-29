namespace TiCodeX.SQLSchemaCompare.Test
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of the ILogger interface that uses the Xunit log system to write
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="XunitLogger"/> class.
    /// </remarks>
    /// <param name="name">The logger name</param>
    /// <param name="output">The Xunit logger</param>
    public sealed class XunitLogger(string name, ITestOutputHelper output) : ILogger, IDisposable
    {
        /// <summary>
        /// The name
        /// </summary>
        private readonly string name = name;

        /// <summary>
        /// The output
        /// </summary>
        private readonly ITestOutputHelper output = output;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            this.output?.WriteLine($"[{logLevel}] [{eventId}] [{this.name}]: " + formatter(state, exception));
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do nothing
        }
    }
}
