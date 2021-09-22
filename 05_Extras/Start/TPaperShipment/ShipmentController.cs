using CloudNative.CloudEvents;
using Dapr.AzureFunctions.Extension;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace TPaperShipment
{
    public static class ShipmentController
    {
        [FunctionName("ProcessShipment")]
        public static void ProcessShipment(
            [DaprTopicTrigger("%PubSubName%", Topic = "shipment")] CloudEvent subEvent, ILogger log)
        {
            log.LogInformation("Shipment start from the Dapr Runtime.");
            log.LogInformation($"Topic Shipment received a message: {subEvent.Data}.");
        }
    }
}
