using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Donation.Model.Lib.UnitTests
{
    [TestClass]
    public class Donations_Model_UnitTest
    {
        private string DonationJsonFile
        {
            get
            {
                return @"C:\DVT\microservices\fMicroService\DonationMicroServices\GeneratedData\donation.SmallSample.json";
            }
        }
        [TestMethod]
        public void LoadDonationJsonFile()
        {
            var donations = Donations.LoadFromJsonFile(DonationJsonFile);
            Assert.AreEqual(donations.Count, 100);
        }
    }
}
