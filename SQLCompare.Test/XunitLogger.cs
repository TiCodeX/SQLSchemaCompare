using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace SQLCompare.Test
{
    /// <summary>
    /// Implementation of the ILogger interface that uses the Xunit log system to write
    /// </summary>
    /// <typeparam name="T">The type who's name is used for the logger category name</typeparam>
    public sealed class XunitLogger<T> : ILogger<T>, IDisposable
    {
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitLogger{T}"/> class.
        /// </summary>
        /// <param name="output">The Xunit logger</param>
        public XunitLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.output.WriteLine(state.ToString());
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
        }
    }
}
