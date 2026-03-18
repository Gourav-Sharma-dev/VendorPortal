namespace Server.Model.Enums
{
    /// <summary>
    /// Represents the various states of a vendor in the approval workflow
    /// </summary>
    public enum VendorStatus
    {
        /// <summary>
        /// Vendor application has been submitted and awaiting review
        /// </summary>
        Submitted = 1,

        /// <summary>
        /// Procurement team is reviewing the vendor application
        /// </summary>
        UnderReview = 2,

        /// <summary>
        /// Finance team is verifying tax and bank details
        /// </summary>
        FinanceVerification = 3,

        /// <summary>
        /// Vendor application has been approved by all levels
        /// </summary>
        Approved = 4,

        /// <summary>
        /// Vendor application has been rejected
        /// </summary>
        Rejected = 5,

        /// <summary>
        /// Vendor application sent back for corrections
        /// </summary>
        CorrectionRequested = 6,

        /// <summary>
        /// Vendor is active and can be used in the system
        /// </summary>
        Active = 7
    }
}
