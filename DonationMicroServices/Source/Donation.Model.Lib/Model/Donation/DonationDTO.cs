using Newtonsoft.Json;
using System;
using System.JSON;

namespace Donation.Model
{
    public class DonationDTO : DonationPersonWithPaymentMethod
    {
        public string IpAddress { get; set; }
        public string Amount { get; set; } //$15.92
        public DateTime UtcCreationDate { get; set; }

        [JsonIgnore]
        public DonationDataProcessState ProcessState = DonationDataProcessState.New;

        public DonationDTO()
        {
            this.UtcCreationDate = DateTime.UtcNow;
        }

        public string ToJSON()
        {
            return JsonObject.Serialize(this);
        }

        public static DonationDTO FromJSON(string json)
        {
            return JsonObject.Deserialize<DonationDTO>(json);            
        }
    }
}
