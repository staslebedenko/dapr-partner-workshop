using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TPaperOrders
{
    [ApiController]
    [Route("api/[controller]")] 
    public class OrderController
    {
        private readonly PaperDbContext _context;

        private readonly ILogger<OrderController> _logger;

        private readonly DaprClient _daprClient;

        public OrderController(
            PaperDbContext context,
            ILogger<OrderController> logger,
            DaprClient daprClient)
        {
            _context = context;
            _logger = logger;
            _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        }

        [HttpGet]
        [Route("create/{quantity}")]
        public async Task<IActionResult> ProcessEdiOrder(decimal quantity, CancellationToken cts)
        {
            _logger.LogInformation("Processed a request.");

            var order = new EdiOrder
            {
                ClientId = 1,
                DeliveryId = 1,
                Notes = "Test order",
                ProductCode = 1,
                Quantity = quantity
            };

            EdiOrder savedOrder = (await _context.EdiOrder.AddAsync(order, cts)).Entity;
            await _context.SaveChangesAsync(cts);



            Delivery savedDelivery = await CreateDeliveryForOrder(savedOrder, cts);

            Dictionary<string, string> secrets = await _daprClient.GetSecretAsync("azurekeyvault", "SqlPaperPassword");
            string superSecret = secrets["SqlPaperPassword"];

            string responseMessage = $"Accepted EDI message {order.Id} and created delivery {savedDelivery?.Id} with super secret{superSecret}";

            return new OkObjectResult(responseMessage);
        }

        private async Task<Delivery> CreateDeliveryForOrder(EdiOrder savedOrder, CancellationToken cts)
        {
            var newDelivery = new Delivery
            {
                Id = 0,
                ClientId = savedOrder.ClientId,
                EdiOrderId = savedOrder.Id,
                Number = savedOrder.Quantity,
                ProductId = 0,
                ProductCode = savedOrder.ProductCode,
                Notes = "Prepared for shipment"
            };

            await _daprClient.PublishEventAsync<Delivery>("pubsub", "createdelivery", newDelivery, cts);

            return newDelivery;
        }
    }
}
