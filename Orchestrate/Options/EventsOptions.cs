namespace Orchestrate.Io
{
    public class EventsOptions
    {
        public int Limit { get; }

        public EventsOptions(int limit = 100)
        {
            this.Limit = limit;
        }
    }
}