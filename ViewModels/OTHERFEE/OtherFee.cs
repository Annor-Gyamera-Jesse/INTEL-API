namespace INTEL_API.ViewModels.OTHERFEE
{
    public class OtherFee
    {

        public class StudentOtherFeeDto
        {
            public int StudentID { get; set; }
            public string StudentFirstName { get; set; }
            public string StudentLastName { get; set; }
            public string ClassID { get; set; }
        }

        public class OtherFeeDto
        {
            public int FeeTypeID { get; set; }
            public string FeeTypeName { get; set; }
            public decimal Amount { get; set; }
            public string ClassID { get; set; }
        }

        public class TermDto
        {
            public int TermID { get; set; }
            public string Term { get; set; }
        }

        public class OtherFeePaymentRequest
        {
            public int StudentID { get; set; }
            public int FeeTypeID { get; set; }
            public string StudentName { get; set; }
            public string FeeTypeName { get; set; }
            public string ClassID { get; set; }
            public decimal AmountPaid { get; set; }
            public DateTime PaymentDate { get; set; }
            public int UserID { get; set; }
            public int TermID { get; set; }
        }
    }
}
