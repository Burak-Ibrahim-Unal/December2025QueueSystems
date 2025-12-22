using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal class DiscountService : IDiscountService
    {
        private readonly IDiscountService _discountService;

        public DiscountService(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        public void Apply(int userId, decimal amount, string discountCode)
        {
            _discountService.Apply(1, amount, "Wellcome package discount");

        }
    }
}

