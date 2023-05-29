using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataModel;

namespace InnoAssignment1.ViewModel
{
    public class ViewOrder_VM
    {
        public OrderDM Order { get; set; }
        public List<OrderItemDM> OrderItems { get; set; }
        public List<ItemMasterDM> ItemMasters { get; set; }
    }
}