using fDotNetCoreContainerHelper;
using Newtonsoft.Json;
using System;
using System.JSON;
using System.Text;

namespace Donation.Model
{
    public class DonationDTO : DonationPersonWithPaymentMethod
    {
        public string IpAddress { get; set; }
        public string Amount { get; set; }
        public DateTime UtcCreationDate { get; set; }

        /// <summary>
        /// The machine id running the donation.restapi.entrace web api that
        /// received the donation first
        /// AKA in Kubernetes the pod host name that ran the donation.restapi.entrace web api
        /// </summary>
        public string __ProcessingMachineID { get; set; }

        /// <summary>
        /// The message id when the donation is read from a queue
        /// </summary>
        public string __QueueMessageID { get; set; }

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

        public decimal GetAmount()
        {
            if (this.Amount.StartsWith("$"))
            {
                Decimal amount;
                if (Decimal.TryParse(this.Amount.Replace("$", ""), out amount))
                {
                    return amount;
                }
            }
            return -1;
        }
    }
}
