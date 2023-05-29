using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel
{
    public class OrderItemDM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid OrderId { get; set; }
        public string OrderName { get; set; }
        public Guid ItemMasterId { get; set; }
        public string ItemMasterName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }

    }
}
