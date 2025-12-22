using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal class DiscountService(IDiscountService discountService)
    {
        public void ApplyDiscount(decimal amount, decimal discountPercentage)
        {
            discountService.Apply(1,amount, "Wellcome package discount");
        }
    }
}

