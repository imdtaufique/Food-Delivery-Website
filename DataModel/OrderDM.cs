using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel
{
    public class OrderDM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OrderNo { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ContactNo{ get; set; }
        public Guid DispatchedById { get; set; }
        public string DispatchedByName { get; set; }
        public Guid DeliveredById { get; set; }
        public string DeliveredByName { get; set; }
        public DateTime DeliveredOn { get; set; }
        public DateTime DispatchedOn { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Landmark { get; set; }
        

        public int StatusReasonValue { get; set; }
        public string StatusReasonText { get; set; }

        public string ReasonForCancellation { get; set; }
        public decimal Amount { get; set; }
        public decimal Gst { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PaymentModeValue { get; set; }
        public string PaymentModeText { get; set; }


    }
}
