namespace INTEL_API.ViewModels
{
    public class PaymentViewModel
    {
        public int StudentID { get; set; }
        public int FeeTypeID { get; set; }
        public string StudentName { get; set; }
        public string FeeTypeName { get; set; }
        public string ClassID { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountLeft { get; set; }
        public DateTime PaymentDate { get; set; }
        public int UserID { get; set; }
    }

    public class PaymentVerificationRequest
    {
        /// <summary>
        /// The transaction reference returned by Paystack during the payment process.
        /// </summary>
        public string reference { get; set; }
    }

    public class PaymentRequest
    {
        /// <summary>
        /// The email address of the customer making the payment.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The payment amount in the smallest currency unit (e.g., Kobo for NGN).
        /// </summary>
        public decimal Amount { get; set; }
    }

}
