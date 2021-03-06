using Donation.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Donation.Model.Lib.UnitTests
{
    [TestClass]
    public class Donations_ValidationService_UnitTests
    {
        [TestMethod]
        public void SumDonations()
        {
            var expectedSumAmount = 5191.75M;
            var donations = DonationDTOs.FromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsService = new DonationsValidationService(donations);
            Assert.AreEqual(expectedSumAmount, donationsService.Sum());
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SumDonations_WithInvalidAmount_NoCurrencySymbol_ShouldThrow()
        {
            var expectedSumAmount = 4853.01M;
            var donations = new DonationDTOs() {
                new DonationDTO() { Amount = "99.11" }
            };
            var donationsService = new DonationsValidationService(donations);
            Assert.AreEqual(expectedSumAmount, donationsService.Sum());
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SumDonations_WithInvalidAmount_InvalidNumericAmount_ShouldThrow()
        {
            var expectedSumAmount = 4853.01M;
            var donations = new DonationDTOs() {
                new DonationDTO() { Amount = "$BAD" }
            };
            var donationsService = new DonationsValidationService(donations);
            Assert.AreEqual(expectedSumAmount, donationsService.Sum());
        }

        [TestMethod]
        public void ValidateDonationSmallSampleData()
        {
            var donations = DonationDTOs.FromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsService = new DonationsValidationService(donations);
            var errors = donationsService.ValidateData();
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void ValidateDonationAllDataFiles()
        {
            var path = Path.GetDirectoryName(Donations_Model_UnitTests.DonationJsonFile);
            var jsonFiles = Directory.GetFiles(path, "donation*.json");
            foreach(var jsonFile in jsonFiles)
            {
                var donations = DonationDTOs.FromJsonFile(jsonFile);
                var donationsService = new DonationsValidationService(donations);
                var errors = donationsService.ValidateData();
                Assert.AreEqual(0, errors.Count);
            }
        }

        [TestMethod]
        public void ValidateDonnationData_WithMissingRequiredProperty_ShouldReturnsErrores()
        {
            const int expectedValidationErrorCount = 7;
            var donations = new DonationDTOs() {
                new DonationDTO() {
                    Guid = Guid.NewGuid(),
                    Amount = "$123"
                }
            };
            var donationsService = new DonationsValidationService(donations);
            var errors = donationsService.ValidateData();
            Assert.AreEqual(expectedValidationErrorCount, errors.Count);
            Assert.IsTrue(donations[0].ProcessState == DonationDataProcessState.NotApprovedForSubmission);
        }
    }
}
