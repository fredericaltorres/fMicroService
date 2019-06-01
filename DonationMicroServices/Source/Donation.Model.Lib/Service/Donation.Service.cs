using Donation.Model;
using System;
using System.Collections.Generic;
using System.JSON;

namespace Donation.Service
{
    public class DonationsService
    {
        Donations _donations;
        public DonationsService(Donations donations)
        {
            _donations = donations;
        }
        public decimal Sum()
        {
            Decimal total = 0m;
            foreach(var donnation in _donations)
            {
                if(donnation.Amount.StartsWith("$"))
                {
                    Decimal amount;
                    if(Decimal.TryParse(donnation.Amount.Replace("$", ""), out amount))
                    {
                        total += amount;
                    }
                    else
                        throw new InvalidOperationException($"Invalid amount {donnation.Amount}, guid:{donnation.Guid}");
                }
                else
                    throw new InvalidOperationException($"Invalid amount {donnation.Amount}, guid:{donnation.Guid}");
            }
            return total;
        }
    }
}
