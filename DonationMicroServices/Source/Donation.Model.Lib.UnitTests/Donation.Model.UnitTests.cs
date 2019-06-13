using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Donation.Model.Lib.UnitTests
{
    [TestClass]
    public class Donations_Model_UnitTests
    {
        internal static string DonationJsonFile
        {
            get
            {
                return @"C:\DVT\microservices\fMicroService\DonationMicroServices\Source\Donation.PersonSimulator.Console\GeneratedData\donation.SmallSample.json";
            }
        }
        [TestMethod]
        public void LoadDonationJsonFile()
        {
            var donations = DonationDTOs.FromJsonFile(DonationJsonFile);
            Assert.AreEqual(donations.Count, 100);
        }
        [TestMethod]
        public void GetAmout_ShouldReturnExpectedValue()
        {
            Assert.AreEqual(1.2m, new DonationDTO() { Amount = "$1.2" }.GetAmount());
        }
        [TestMethod]
        public void GetAmout_ShouldReturnInvalidValueErrorCode()
        {
            Assert.AreEqual(-1, new DonationDTO() { Amount = "1.2" }.GetAmount());
            Assert.AreEqual(-1, new DonationDTO() { Amount = "$bad" }.GetAmount());
        }
    }
}
