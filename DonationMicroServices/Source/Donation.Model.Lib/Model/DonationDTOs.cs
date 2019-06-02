using System.Collections.Generic;
using System.JSON;

namespace Donation.Model
{
    public class DonationDTOs : List<DonationDTO>
    {
        public static DonationDTOs LoadFromJsonFile(string jsonFile)
        {
            var json = System.IO.File.ReadAllText(jsonFile);
            var donations = JsonObject.Deserialize<DonationDTOs>(json);
            return donations;
        }
    }
}
