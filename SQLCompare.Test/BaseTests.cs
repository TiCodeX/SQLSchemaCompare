using System;
using Xunit.Abstractions;

namespace SQLCompare.Test
{
    public abstract class BaseTests<T> : IDisposable
    {
        protected BaseTests(ITestOutputHelper output)
        {
            Logger = new XunitLogger<T>(output);
        }

        protected XunitLogger<T> Logger { get; }

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
