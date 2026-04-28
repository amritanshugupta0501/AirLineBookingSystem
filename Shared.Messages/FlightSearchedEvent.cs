namespace Shared.Messages
{
    public interface FlightSearchedEvent
    {
        string Origin { get; }
        string Destination { get; }
        DateTime SearchTime { get; }
    }
}
