using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020136.DomainModels
{
    public class OrderDetailModel
    {
        public Order? Order { get; set; }
        public List<OrderDetail>? Details { get; set; }
        public decimal Total
        {
            get
            {
                decimal result = 0;
                foreach (var item in Details!)
                    result += item.TotalPrice;

                return result;
            }
        }
    }
}
