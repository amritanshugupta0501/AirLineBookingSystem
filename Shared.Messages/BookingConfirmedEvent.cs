namespace Shared.Messages
{
    public record BookingConfirmedEvent
    {
        public string PNR { get; init; } = string.Empty;
        public string PassengerEmail { get; init; } = string.Empty;
        public decimal AmountPaid { get; init; }
    }
}
