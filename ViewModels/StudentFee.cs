namespace INTEL_API.ViewModels
{
    public class StudentFee
    {
        public int FeeID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string FeeTypeName { get; set; }
        public string ClassID { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountLeft { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Note { get; set; }
        public int TermID { get; set; }
    }
}
