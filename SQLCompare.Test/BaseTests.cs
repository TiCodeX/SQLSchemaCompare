using System;
using Xunit.Abstractions;

namespace SQLCompare.Test
{
    /// <summary>
    /// Base class for every test
    /// </summary>
    /// <typeparam name="T">Type for the initialization of the Logger</typeparam>
    public abstract class BaseTests<T> : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTests{T}"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        protected BaseTests(ITestOutputHelper output)
        {
            Logger = new XunitLogger<T>(output);
        }

        /// <summary>
        /// Gets the Logger
        /// </summary>
        protected XunitLogger<T> Logger { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger?.Dispose();
            }
        }
    }
}
