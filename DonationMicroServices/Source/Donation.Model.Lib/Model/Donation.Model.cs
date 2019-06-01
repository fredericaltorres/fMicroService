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

    public class DonationPerson
    {
        public Guid Guid;
        public string FirstName;
        public string LastName;
        public string Email;
        public Gender Gender;
        public string Phone;
        public string Country;
        public string ZipCode;
    }

    public class DonationPersonWithPaymentMethod : DonationPerson
    {
        public string CC_Number;
        public int CC_ExpMonth;
        public int CC_ExpYear;
        public int CC_SecCode;
    }

    [Flags]
    public enum DonationDataProcessState 
    {
        New = 0x1,
        DataValidated = 0x2,
        ApprovedForSubmission = 0x4,
        NotApprovedForSubmission = 0x8,
    };

    public class DonationDTO : DonationPersonWithPaymentMethod
    {
        public string IpAddress;
        public string Amount; //$15.92
        public DateTime UTCCreationDate;
        public DonationDataProcessState ProcessState = DonationDataProcessState.New;
    }

    public class Donations : List<DonationDTO>
    {
      
        public static Donations LoadFromJsonFile(string jsonFile)
        {
            var json = System.IO.File.ReadAllText(jsonFile);
            var donations = JsonObject.Deserialize<Donations>(json);
            return donations;
        }
    }

    public class DonationDTOValidationError
    {
        public Guid DonationGuid;
        public string Property;
        public string Reason;

        public DonationDTOValidationError(Guid donationGuid, string property, string reason)
        {
            this.DonationGuid = donationGuid;
            this.Property = property;
            this.Reason = reason;
        }
    }

    public class DonationDTOValidationErrors : List<DonationDTOValidationError>
    {

    }
}
