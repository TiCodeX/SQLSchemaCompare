namespace TiCodeX.SQLSchemaCompare.Test
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of the ILogger interface that uses the Xunit log system to write
    /// </summary>
    public sealed class XunitLogger : ILogger, IDisposable
    {
        /// <summary>
        /// The name
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The output
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitLogger"/> class.
        /// </summary>
        /// <param name="name">The logger name</param>
        /// <param name="output">The Xunit logger</param>
        public XunitLogger(string name, ITestOutputHelper output)
        {
            this.name = name;
            this.output = output;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            this.output?.WriteLine($"[{logLevel.ToString()}] [{eventId.ToString()}] [{this.name}]: " + formatter(state, exception));
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
