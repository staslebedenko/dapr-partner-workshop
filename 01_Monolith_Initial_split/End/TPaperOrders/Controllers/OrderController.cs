using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TPaperOrders
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController
    {
        private readonly PaperDbContext _context;

        private readonly ILogger<OrderController> _logger;

        public OrderController(PaperDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("create/{quantity}")]
        public async Task<IActionResult> ProcessEdiOrder(decimal quantity, CancellationToken cts)
        {
            //log.LogInformation("C# HTTP trigger function processed a request.");

            var order = new EdiOrder
            {
                Id = 0,
                ClientId = 1,
                Delivery = null,
                DeliveryId = null,
                Notes = "Test order",
                ProductCode = 1,
                Quantity = quantity
            };

            EdiOrder savedOrder = (await this._context.EdiOrder.AddAsync(order, cts)).Entity;
            await this._context.SaveChangesAsync(cts);

            // get product
            Product product = await this._context.Product.FirstOrDefaultAsync(x => x.ExternalCode == savedOrder.ProductCode, cts);

            // check if inventory have needed amount.
            //var availableNumber = (await this.context.Inventory.FirstOrDefaultAsync(x => x.ProductId == product.Id, cts)).Number;

            // create devliery from order. 
            var newDelivery = new Delivery
            {
                Id = 0,
                ClientId = savedOrder.ClientId,
                EdiOrder = savedOrder,
                EdiOrderId = savedOrder.Id,
                Number = savedOrder.Quantity,
                ProductId = product.Id,
                ProductCode = product.ExternalCode, 
                Notes = "Prepared for shipment"
            };

            Delivery savedDelivery = (await this._context.Delivery.AddAsync(newDelivery, cts)).Entity;
            await this._context.SaveChangesAsync(cts);

            string responseMessage = $"Accepted EDI message {order.Id} and created delivery {savedDelivery.Id}";

            return new OkObjectResult(responseMessage);
        }
    }
}
