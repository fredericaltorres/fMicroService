using Donation.Model;

namespace Donation.Service
{

    public class DonationsAggregationService
    {
        DonationDTOs _donations = new DonationDTOs();
        public DonationsAggregate CountryAggregateData = new DonationsAggregate();
        public int AggregatedRecordCount;

        public void Clear()
        {
            this.CountryAggregateData.Clear();
            Reset();
        }

        private void Reset()
        {
            this._donations = new DonationDTOs();
        }

        public DonationsAggregationService()
        {
            this.Reset();
        }

        public DonationsAggregationService(DonationDTOs donations)
        {
            this._donations.AddRange(donations);
        }

        public void Add(DonationDTOs donations)
        {
            this._donations.AddRange(donations);
        }
       
        private void Aggregate(DonationDTO donation)
        {
            CountryAggregateData.Aggregate(donation.Country, donation.GetAmount());
            this.AggregatedRecordCount += 1;
        }
        
        public Errors AggregateData()
        {
            var totalErrors = new Errors();
            // this.CountryAggregateData.Clear();
            foreach (var donation in _donations)
            {
                this.Aggregate(donation);
            }
            this._donations.Clear(); // Once we aggregate the data we clear it, so we do not recount the data

            return totalErrors;
        }
    }
}
