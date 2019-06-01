using Donation.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Donation.Model.Lib.UnitTests
{
    [TestClass]
    public class Donations_Service_UnitTests
    {
        [TestMethod]
        public void SumDonations()
        {
            var expectedSumAmount = 4853.01M;
            var donations = Donations.LoadFromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsService = new DonationsService(donations);
            Assert.AreEqual(expectedSumAmount, donationsService.Sum());
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SumDonations_WithInvalidAmount_NoCurrencySymbol_ShouldThrow()
        {
            var expectedSumAmount = 4853.01M;
            var donations = new Donations() {
                new DonationDTO() { Amount = "99.11" }
            };
            var donationsService = new DonationsService(donations);
            Assert.AreEqual(expectedSumAmount, donationsService.Sum());
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SumDonations_WithInvalidAmount_InvalidNumericAmount_ShouldThrow()
        {
            var expectedSumAmount = 4853.01M;
            var donations = new Donations() {
                new DonationDTO() { Amount = "$BAD" }
            };
            var donationsService = new DonationsService(donations);
            Assert.AreEqual(expectedSumAmount, donationsService.Sum());
        }

        [TestMethod]
        public void ValidateDonnationData()
        {
            var donations = Donations.LoadFromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsService = new DonationsService(donations);
            var errors = donationsService.ValidateData();
            Assert.AreEqual(0, errors.Count);
        }


        [TestMethod]
        public void ValidateDonnationData_WithMissingRequiredProperty_ShouldReturnsErrores()
        {
            var donations = new Donations() {
                new DonationDTO() {
                    Guid = Guid.NewGuid(),
                    Amount = "$123"
                }
            };
            var donationsService = new DonationsService(donations);
            var errors = donationsService.ValidateData();
            Assert.AreEqual(8, errors.Count);
            Assert.IsTrue(donations[0].ProcessState == DonationDataProcessState.NotApprovedForSubmission);
        }
    }
}
