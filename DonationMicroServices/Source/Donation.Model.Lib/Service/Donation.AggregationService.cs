using Donation.Model;
using System;
using System.Collections.Generic;
using System.JSON;

namespace Donation.Service
{
    public class DonationsAggregate
    {

    }

    public class DonationsAggregationService
    {
        DonationDTOs _donations;

        public DonationsAggregationService(DonationDTO donation)
        {
            _donations = new DonationDTOs() { donation };
        }

        public DonationsAggregationService(DonationDTOs donations)
        {
            _donations = donations;
        }
        
        public Errors AggregateData()
        {
            var totalErrors = new Errors();
            foreach (var donation in _donations)
            {
            }

            return totalErrors;
        }
    }
}
