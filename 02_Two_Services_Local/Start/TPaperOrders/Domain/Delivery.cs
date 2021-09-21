using System;
using System.Collections.Generic;

namespace TPaperOrders
{
    public class Delivery
    {
        public Delivery()
        {
            this.Orders = new HashSet<EdiOrder>();
        }

        public int Id { get; set; }

        public decimal Number { get; set; }

        public int ClientId { get; set; }

        public int EdiOrderId { get; set; }

        public int ProductCode { get; set; }

        public int ProductId { get; set; }

        public string Notes { get; set; }

        public EdiOrder EdiOrder { get; set; }

        public ICollection<EdiOrder> Orders { get; }
    }
}
