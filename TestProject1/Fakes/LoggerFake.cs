using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Fakes
{
    internal class LoggerFake<T> : ILogger, ILogger<T>
    {
        public StringBuilder Contents { get; } = new StringBuilder();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId _, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Contents.AppendLine(formatter(state, exception));
        }
    }
}
