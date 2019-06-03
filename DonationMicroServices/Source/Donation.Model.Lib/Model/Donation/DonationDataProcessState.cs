using System;

namespace Donation.Model
{
    [Flags]
    public enum DonationDataProcessState 
    {
        New = 0x1,
        DataValidated = 0x2,
        ApprovedForSubmission = 0x4,
        NotApprovedForSubmission = 0x8,
    };
}
