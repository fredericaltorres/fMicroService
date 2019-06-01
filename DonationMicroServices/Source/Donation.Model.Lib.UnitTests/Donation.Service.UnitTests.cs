using Donation.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Donation.Model.Lib.UnitTests
{
    [TestClass]
    public class Donations_Service_UnitTests
    {
        
        [TestMethod]
        public void SumDonations()
        {
            var expectedSumAmount = 5059.95M;
            var donations = Donations.LoadFromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsService = new DonationsService(donations);
            Assert.AreEqual(expectedSumAmount, donationsService.Sum());

        }
    }
}
