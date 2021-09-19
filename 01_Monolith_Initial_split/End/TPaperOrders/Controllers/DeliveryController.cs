using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TPaperOrders
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController
    {
        private readonly DeliveryDbContext _context;

        public DeliveryController(DeliveryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("create/{clientId}/{ediOrderId}/{productCode}/{number}")]
        public async Task<IActionResult> ProcessEdiOrder(
            int clientId,
            int ediOrderId,
            int productCode,
            int number,
            CancellationToken cts)
        {
            Product product = await _context.Product.FirstOrDefaultAsync(x => x.ExternalCode == productCode, cts);

            var newDelivery = new Delivery
            {
                Id = 0,
                ClientId = clientId,
                EdiOrderId = ediOrderId,
                Number = number,
                ProductId = product.Id,
                ProductCode = product.ExternalCode,
                Notes = "Prepared for shipment"
            };

            Delivery savedDelivery = (await _context.Delivery.AddAsync(newDelivery, cts)).Entity;
            await _context.SaveChangesAsync(cts);


            return new OkObjectResult(savedDelivery);
        }
    }
}
