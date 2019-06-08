using Newtonsoft.Json;
using System;
using System.JSON;
using System.Text;

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

        public string GetSummary()
        {
            var s = new StringBuilder();
            s.Append($"guid:{this.Guid}, UtcCreationDate:{this.UtcCreationDate}, Amount:{this.Amount}");
            return s.ToString();
        }

        public static DonationDTO FromJSON(string json)
        {
            return JsonObject.Deserialize<DonationDTO>(json);            
        }
    }
}
