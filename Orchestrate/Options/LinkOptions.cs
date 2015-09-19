using System;

namespace Orchestrate.Io
{
    public class LinkOptions
    {
        public int Offset { get; private set; }

        public int Limit { get; private set; }

        public LinkOptions(int offset = 0, int limit = 10)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 100");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "offset must be at least 0");

            Offset = offset;
            Limit = limit;
        }
    }
}
