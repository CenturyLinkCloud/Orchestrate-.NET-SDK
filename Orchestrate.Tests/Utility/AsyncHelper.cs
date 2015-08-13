using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    static class AsyncHelper
    {
        public static void RunSync(Func<Task> action)
        {
            if (SynchronizationContext.Current == null)
                action().GetAwaiter().GetResult();
            else
                Task.Run(async () => await action()).GetAwaiter().GetResult();
        }

        public static T RunSync<T>(Func<Task<T>> action)
        {
            return SynchronizationContext.Current == null
                ? action().GetAwaiter().GetResult()
                : Task.Run(async () => await action()).GetAwaiter().GetResult();
        }
    }
}