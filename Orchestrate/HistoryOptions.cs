using System;

namespace Orchestrate.Io
{
    public class HistoryOptions
    {
        public int Offset { get; private set; }

        public int Limit { get; private set; }

        public bool Values { get; private set; }

        public HistoryOptions(int offset = 0, int limit = 10, bool values = false)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 100");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "offset must be at least 0");

            Offset = offset;
            Limit = limit;
            Values = values;
        }
    }
}
