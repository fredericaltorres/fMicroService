using Donation.Model;
using DynamicSugar;
using fAzureHelper;
using System;

namespace Donation.Table.Lib
{
    public class DonationAzureTableRecord : TableRecordManager
    {
        public Guid Guid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }

        public string CC_Number { get; set; }
        public int CC_ExpMonth { get; set; }
        public int CC_ExpYear { get; set; }
        public int CC_SecCode { get; set; }

        public string IpAddress { get; set; }
        public string Amount { get; set; }
        public DateTime UtcCreationDate { get; set; }

        public DonationDataProcessState ProcessState { get; set; } = DonationDataProcessState.New;

        public string __EntranceMachineID { get; set; }
        public string __ProcessingMachineID { get; set; }
        public string __QueueMessageID { get; set; }

        public Errors Set(DonationDTO fromDonationDTO)
        {
            var r = new Errors();
            var dic = ReflectionHelper.GetDictionary(fromDonationDTO);
            foreach(var e in dic)
            {
                try
                {
                    ReflectionHelper.SetProperty(this, e.Key, e.Value);
                }
                catch(System.Exception ex)
                {
                    r.Add(new Error($"Cannot copy property {e.Key} from DonationDTO to DonationAzureTableRecord - ex:{ex.Message}", ex));
                }
            }
            this.SetIdentification();
            return r;
        }

        public void SetIdentification()
        {
            base.SetIdentification(Id: this.Guid.ToString(),  partition: this.Country);
        }
    }
}
