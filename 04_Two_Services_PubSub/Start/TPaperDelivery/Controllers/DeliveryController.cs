using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Dapr;
using Microsoft.Extensions.Logging;

namespace TPaperDelivery
{
    [ApiController]
    public class DeliveryController
    {
        private readonly DeliveryDbContext _context;

        private readonly ILogger<DeliveryController> _logger;


        public DeliveryController(DeliveryDbContext context, ILogger<DeliveryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Topic("pubsub", "createdelivery")]
        [HttpPost]
        [Route("createdelivery")]
        public async Task<IActionResult> ProcessEdiOrder(Delivery delivery)
        {
            _logger.LogWarning("Triggered method"); 

            Product product = await _context.Product.FirstOrDefaultAsync();

            var newDelivery = new Delivery
            {
                Id = 0,
                ClientId = delivery.ClientId,
                EdiOrderId = delivery.EdiOrderId,
                Number = delivery.Number,
                ProductId = product.Id,
                ProductCode = product.ExternalCode,
                Notes = "Prepared for shipment"
            };

            Delivery savedDelivery = (await _context.Delivery.AddAsync(newDelivery)).Entity;
            await _context.SaveChangesAsync();

            _logger.LogWarning("Saved delivery");

            return new OkObjectResult("");
        }

        [HttpGet]
        [Route("api/deliveries/get")]
        public async Task<IActionResult> Get(CancellationToken cts)
        {
            Delivery[] registeredDeliveries = await _context.Delivery.ToArrayAsync(cts);

            return new OkObjectResult(registeredDeliveries);
        }
    }
}
