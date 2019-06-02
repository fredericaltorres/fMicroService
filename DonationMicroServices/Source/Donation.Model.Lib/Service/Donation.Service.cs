using Donation.Model;
using System;
using System.Collections.Generic;
using System.JSON;

namespace Donation.Service
{
    public class DonationsService
    {
        DonationDTOs _donations;

        public DonationsService(DonationDTO donation)
        {
            _donations = new DonationDTOs() { donation };
        }

        public DonationsService(DonationDTOs donations)
        {
            _donations = donations;
        }

        const string VALIDATION_ERROR_PROPERTY_IS_REQUIRED = "Property is required";
        const string VALIDATION_ERROR_PROPERTY_CANNOT_BE_NULL = "Property cannot be null";

        private bool IsOfTypeString(object v)
        {
            if (v == null)
                return false;
            return v.GetType().Name == "String";
        }

        private bool CanBeNull(string property)
        {
            return property == "ZipCode";
        }

        public DonationDTOValidationErrors ValidateData()
        {
            var totalErrors = new DonationDTOValidationErrors();
            foreach (var donation in _donations)
            {
                var donnationErrors = new DonationDTOValidationErrors();
                var donnationDic = DynamicSugar.ReflectionHelper.GetDictionary(donation);
                foreach(var e in donnationDic)
                {
                    if (e.Value == null && !CanBeNull(e.Key))
                    {
                        donnationErrors.Add(new DonationDTOValidationError(donation.Guid, e.Key, VALIDATION_ERROR_PROPERTY_CANNOT_BE_NULL));
                    }
                    else if (IsOfTypeString(e.Value) && string.IsNullOrEmpty(e.Value.ToString()))
                    {
                        donnationErrors.Add(new DonationDTOValidationError(donation.Guid, e.Key, VALIDATION_ERROR_PROPERTY_IS_REQUIRED));
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
