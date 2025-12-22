using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples
{
    internal interface IDiscountService
    {
        void Apply(int userId,decimal amount, string discountCode);
    }
}

