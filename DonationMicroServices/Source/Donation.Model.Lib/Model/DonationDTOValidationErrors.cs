using System.Collections.Generic;

namespace Donation.Model
{
    public class DonationDTOValidationErrors : List<DonationDTOValidationError>
    {
        public override string ToString()
        {
            var b = new System.Text.StringBuilder();
            foreach (var e in this)
                b.Append(e.ToString()).AppendLine();
            return b.ToString();
        }
    }
}
