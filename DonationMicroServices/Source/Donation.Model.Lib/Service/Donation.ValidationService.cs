using Donation.Model;
using fAzureHelper;
using System;
using System.Collections.Generic;
using System.JSON;

namespace Donation.Service
{
    public class DonationsValidationService
    {
        DonationDTOs _donations;

        public DonationsValidationService(DonationDTO donation)
        {
            _donations = new DonationDTOs() { donation };
        }

        public DonationsValidationService(DonationDTOs donations)
        {
            _donations = donations;
        }

        const string VALIDATION_ERROR_PROPERTY_IS_REQUIRED = "Property is required ";
        const string VALIDATION_ERROR_PROPERTY_CANNOT_BE_NULL = "Property cannot be null ";

        private bool IsOfTypeString(object v)
        {
            if (v == null)
                return false;
            return v.GetType().Name == "String";
        }

        private bool CanBeNull(string property)
        {
            return  property == "ZipCode" || 
                    property == "__EntranceMachineID" ||
                    property == "__ProcessingMachineID" ||
                    property == "__QueueMessageID";
        }

        public Errors ValidateData()
        {
            var totalErrors = new Errors();
            foreach (var donation in _donations)
            {
                var donnationErrors = new Errors();
                var donnationDic = DynamicSugar.ReflectionHelper.GetDictionary(donation);
                foreach(var e in donnationDic)
                {
                    if (e.Value == null && !CanBeNull(e.Key))
                    {
                        donnationErrors.Add(new Error(VALIDATION_ERROR_PROPERTY_CANNOT_BE_NULL + e.Key, null));
                    }
                    else if (IsOfTypeString(e.Value) && string.IsNullOrEmpty(e.Value.ToString()))
                    {
                        donnationErrors.Add(new Error(VALIDATION_ERROR_PROPERTY_IS_REQUIRED+ e.Key, null));
                    }
                }
                donation.ProcessState = donnationErrors.Count == 0 ? DonationDataProcessState.ApprovedForSubmission : DonationDataProcessState.NotApprovedForSubmission;
                totalErrors.AddRange(donnationErrors);
            }

            return totalErrors;
        }

        public decimal Sum()
        {
            Decimal total = 0m;
            foreach(var donnation in _donations)
            {
                var errorMessage = $"Invalid amount {donnation.Amount}, guid:{donnation.Guid}";
                if (donnation.Amount.StartsWith("$"))
                {
                    Decimal amount;
                    if(Decimal.TryParse(donnation.Amount.Replace("$", ""), out amount))
                    {
                        total += amount;
                    }
                    else throw new InvalidOperationException(errorMessage);
                }
                else throw new InvalidOperationException(errorMessage);
            }
            return total;
        }
    }
}
