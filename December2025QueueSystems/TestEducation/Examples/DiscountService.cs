using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestEducation.Examples.Observer;

namespace TestEducation.Examples
{
    internal class DiscountService : IDiscountService, IUserObserver
    {
        private readonly IDiscountService _discountService;

        public DiscountService()
        {
        }

        public void Apply(int userId, decimal amount, string discountCode)
        {
            _discountService.Apply(1, amount, "Wellcome package discount");

        }

        public void ProcessOtherOperations()
        {
            Console.WriteLine("Discount is applied");
        }
    }
}

