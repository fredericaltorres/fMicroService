using System;

namespace Donation.Model
{
    [Flags]
    public enum DonationDataProcessState 
    {
        New = 1 << 0,
        DataValidated = 1 << 1,
        ApprovedForSubmission = 1 << 2,
        NotApprovedForSubmission = 1 << 3,
        Submitted = 1 << 4,
    };
}
