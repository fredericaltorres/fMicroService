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
            var donations = DonationDTOs.LoadFromJsonFile(DonationJsonFile);
            Assert.AreEqual(donations.Count, 100);
        }
    }
}
