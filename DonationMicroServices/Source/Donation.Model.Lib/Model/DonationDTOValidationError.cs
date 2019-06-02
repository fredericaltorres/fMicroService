using System;

namespace Donation.Model
{
    public class DonationDTOValidationError
    {
        public Guid DonationGuid;
        public string Property;
        public string Reason;

        public DonationDTOValidationError(Guid donationGuid,  string reason)
        {
            this.DonationGuid = donationGuid;
            this.Reason = reason;
        }
        public DonationDTOValidationError(Guid donationGuid, string property, string reason)
        {
            this.DonationGuid = donationGuid;
            this.Property = property;
            this.Reason = reason;
        }

        public override string ToString()
        {
            if(string.IsNullOrEmpty(Property))
                return $"Guid {DonationGuid}, Reason:{Reason}";
            else
            return $"Guid {DonationGuid}, Property:{Property}, Reason:{Reason}";
        }
    }
}
