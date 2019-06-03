namespace Donation.Model
{
    public class DonationPersonWithPaymentMethod : DonationPerson
    {
        public string CC_Number { get; set; }
        public int CC_ExpMonth { get; set; }
        public int CC_ExpYear { get; set; }
        public int CC_SecCode { get; set; }
    }
}
