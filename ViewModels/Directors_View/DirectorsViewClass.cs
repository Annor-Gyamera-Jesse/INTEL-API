namespace INTEL_API.ViewModels.Directors_View
{
    public class DirectorsViewClass
    {
        public class FeeSummary
        {
            public string FeeTypeName { get; set; }
            public string ClassID { get; set; }
            public int TotalStudents { get; set; }
            public decimal ExpectedAmount { get; set; }
            public decimal TotalPaid { get; set; }
            public decimal TotalOwing { get; set; }
            public decimal CollectionPercentage { get; set; }
        }

        public class OwingSummary
        {
            public string ClassID { get; set; }
            public string FeeTypeName { get; set; }
            public int TotalStudents { get; set; }
            public decimal ExpectedAmount { get; set; }
            public decimal TotalPaid { get; set; }
            public decimal TotalOwing { get; set; }
            public int OwingStudentsCount { get; set; }
        }
    }
}
