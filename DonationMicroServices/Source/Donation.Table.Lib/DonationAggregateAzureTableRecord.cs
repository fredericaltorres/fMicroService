using Donation.Model;
using Donation.Service;
using DynamicSugar;
using fAzureHelper;
using fDotNetCoreContainerHelper;
using System;

namespace Donation.Table.Lib
{
    public class DonationAggregateAzureTableRecord : TableRecordManager
    {
        public Guid Guid { get; set; }
        public int DonationsProcessedCount { get; set; }
        public string DonationsAggregateJSON { get; set; }
        public string __EntranceMachineID { get; set; }
        public DateTime __UtcCreationDate { get; set; }

        public DonationAggregateAzureTableRecord(DonationsAggregate donationsAggregate, int donationsProcessedCount)
        {
            this.Set(donationsAggregate, donationsProcessedCount);
        }

        public Errors Set(DonationsAggregate donationsAggregate, int donationsProcessedCount)
        {
            var r = new Errors();
            this.Guid = Guid.NewGuid();
            this.DonationsProcessedCount = donationsProcessedCount;
            this.__UtcCreationDate = DateTime.UtcNow;
            this.__EntranceMachineID = RuntimeHelper.GetMachineName();
            this.DonationsAggregateJSON = donationsAggregate.ToJSON();
            this.SetIdentification();
            return r;
        }

        public void SetIdentification()
        {
            base.SetIdentification(Id: this.Guid.ToString(),  partition: "Aggregate");
        }
    }
}
