using System;
using System.Collections.Generic;

namespace TPaperDelivery
{
    public class Delivery
    {
        public int Id { get; set; }

        public decimal Number { get; set; }

        public int ClientId { get; set; }

        public int EdiOrderId { get; set; }

        public int ProductCode { get; set; }

        public int ProductId { get; set; }

        public string Notes { get; set; }
    }
}
