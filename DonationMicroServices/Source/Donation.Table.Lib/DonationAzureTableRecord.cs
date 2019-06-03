using Donation.Model;
using DynamicSugar;
using fAzureHelper;
using System;

namespace Donation.Table.Lib
{
    public class DonationAzureTableRecord : TableRecordManager
    {
        public Guid Guid;
        public string FirstName;
        public string LastName;
        public string Email;
        public Gender Gender;
        public string Phone;
        public string Country;
        public string ZipCode;

        public string CC_Number;
        public int CC_ExpMonth;
        public int CC_ExpYear;
        public int CC_SecCode;

        public string IpAddress;
        public string Amount; //$15.92
        public DateTime UtcCreationDate;

        public DonationDataProcessState ProcessState = DonationDataProcessState.New;

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
                    r.Add(new Error($"Cannot copy property {e.Key} from DonationDTO to DonationAzureTableRecord - ex:{ex}"));
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
