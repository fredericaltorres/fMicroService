using System;
using System.Collections.Generic;
using System.JSON;

namespace Donation.Model
{
    public enum Gender
    {
        Male = 1,
        Female = 2
    }
    public class Donation
    {
        public Guid Guid;
        public string FirstName;
        public string LastName;
        public string Email;
        public Gender Gender;
        public string Phone;
        public string Country;
        public string IpAddress;
        public string CreditCard;
        public string Amount; //$15.92
    }
    public class Donations : List<Donation>
    {
        public static Donations LoadFromJsonFile(string jsonFile)
        {
            var json = System.IO.File.ReadAllText(jsonFile);
            var donations = JsonObject.Deserialize<Donations>(json);
            return donations;
        }
    }
}
