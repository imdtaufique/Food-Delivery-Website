using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel
{
    public class ItemMasterDM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TypeValue { get; set; }
        public string TypeText { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        public decimal Price { get; set; }

    }
}
