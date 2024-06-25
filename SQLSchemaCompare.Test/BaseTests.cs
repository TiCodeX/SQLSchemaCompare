namespace TiCodeX.SQLSchemaCompare.Test
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
            this.Logger = new XunitLogger(typeof(T).Name, output);
            this.LoggerFactory = new XunitLoggerFactory(output);
        }

        /// <summary>
        /// Gets the Logger
        /// </summary>
        protected XunitLogger Logger { get; }

        /// <summary>
        /// Gets the LoggerFactory
        /// </summary>
        protected XunitLoggerFactory LoggerFactory { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        /// <param name="disposing">Indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false)</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Logger?.Dispose();
            }
        }
    }
}
