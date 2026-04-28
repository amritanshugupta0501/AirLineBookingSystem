namespace Shared.Messages
{
    public interface PaymentConfirmedEvent
    {
        string Pnr { get; }
        string PassengerEmail { get; }
        decimal AmountPaid { get; }
        DateTime ConfirmationTime { get; }
    }
}
