using MassTransit;
using Shared.Messages;
using Microsoft.Extensions.Logging;

namespace Analytics.Worker
{
    public class FlightSearchedConsumer : IConsumer<FlightSearchedEvent>
    {
        private readonly ILogger<FlightSearchedConsumer> _logger;

        public FlightSearchedConsumer(ILogger<FlightSearchedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<FlightSearchedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Analytics recorded search: routing from {Origin} to {Destination} at {SearchTime}", 
                message.Origin, 
                message.Destination, 
                message.SearchTime);

            // In a real application, you would save this to a NoSQL database or Data Warehouse here
            return Task.CompletedTask;
        }
    }
}
