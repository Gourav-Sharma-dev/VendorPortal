namespace Server.Model.Enums
{
    /// <summary>
    /// Represents the type of document uploaded by vendor
    /// </summary>
    public enum DocumentType
    {
        /// <summary>
        /// PAN Card document (Mandatory)
        /// </summary>
        PANCard = 1,

        /// <summary>
        /// GST Certificate document (Mandatory)
        /// </summary>
        GSTCertificate = 2,

        /// <summary>
        /// Bank statement or cancelled cheque
        /// </summary>
        BankStatement = 3,

        /// <summary>
        /// Other supporting documents
        /// </summary>
        Other = 4
    }
}
