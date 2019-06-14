using Donation.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Donation.Model.Lib.UnitTests
{
    [TestClass]
    public class Donations_AggregationService_UnitTests
    {

        [TestMethod]
        public void GetTotal_ShoulReturnZeroWhenAggregateDataMethodIsNotCalled()
        {
            var donations = DonationDTOs.FromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsAggregationService = new DonationsAggregationService(donations);
            Assert.AreEqual(0m, donationsAggregationService.CountryAggregateData.GetTotal());
        }
 

        [TestMethod]
        public void Aggregate()
        {
            const string jsonExpectedDataFileName = @".\TestFiles\Donation.Aggregate.0.json";
            var expectedAggregatedData = DonationsAggregate.FromJSON(File.ReadAllText(jsonExpectedDataFileName));
            var donations = DonationDTOs.FromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsAggregationService = new DonationsAggregationService(donations);

            donationsAggregationService.AggregateData();
            AssertAggregateDataForJson0DataFile(expectedAggregatedData, donationsAggregationService);
        }


        [TestMethod]
        public void Aggregate_MultipleTime()
        {
            const string jsonExpectedDataFileName = @".\TestFiles\Donation.Aggregate.0.json";
            var expectedAggregatedData = DonationsAggregate.FromJSON(File.ReadAllText(jsonExpectedDataFileName));
            var donations = DonationDTOs.FromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsAggregationService = new DonationsAggregationService(donations);

            donationsAggregationService.AggregateData();
            AssertAggregateDataForJson0DataFile(expectedAggregatedData, donationsAggregationService);

            // Add a second set of data
            donationsAggregationService.Add(donations);
            donationsAggregationService.AggregateData();
            Assert.AreEqual(5191.75m, donationsAggregationService.CountryAggregateData.GetTotal());
        }

        private static void AssertAggregateDataForJson0DataFile(DonationsAggregate expectedAggregatedData, DonationsAggregationService donationsAggregationService)
        {
            Assert.AreEqual(5191.75m, donationsAggregationService.CountryAggregateData.GetTotal());
            Assert.AreEqual(expectedAggregatedData.ToJSON(), donationsAggregationService.CountryAggregateData.ToJSON());
        }

        [TestMethod]
        public void Aggregate_MultipleTimeWithoutAddingData_ShouldProduceSameResult()
        {
            const string jsonExpectedDataFileName = @".\TestFiles\Donation.Aggregate.0.json";
            var expectedAggregatedData = DonationsAggregate.FromJSON(File.ReadAllText(jsonExpectedDataFileName));
            var donations = DonationDTOs.FromJsonFile(Donations_Model_UnitTests.DonationJsonFile);
            var donationsAggregationService = new DonationsAggregationService(donations);

            donationsAggregationService.AggregateData();

            AssertAggregateDataForJson0DataFile(expectedAggregatedData, donationsAggregationService);

            donationsAggregationService.AggregateData();

            AssertAggregateDataForJson0DataFile(expectedAggregatedData, donationsAggregationService);
        }

    }
}
