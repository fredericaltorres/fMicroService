using System;
using System.Linq;
using System.Collections.Generic;
using System.JSON;

namespace Donation.Service
{
    public class DonationsAggregate : Dictionary<string, decimal>
    {
        public void Aggregate(string country, Decimal amount)
        {
            if(this.ContainsKey(country.ToLowerInvariant()))
                this[country.ToLowerInvariant()] += amount;
            else
                this[country.ToLowerInvariant()] = amount;
        }

        public decimal GetTotal()
        {
            return this.Values.Sum(v => v);
        }

        public string ToJSON()
        {
            return JsonObject.Serialize(this);
        }

        public static DonationsAggregate FromJSON(string json)
        {
            return JsonObject.Deserialize<DonationsAggregate>(json);
        }
    }
}
