namespace INTEL_API.ViewModels.SchoolTerm
{
    public class SchoolTerm
    {
        public int TermID { get; set; }
        public string Term { get; set; }
    }

    public class SchoolCurrentTerm
    {
        public int TermID { get; set; }
        public string Term { get; set; }  
        public bool IsCurrentTerm { get; set; }
        public DateTime? TermEndDate { get; set; }
        public int? UserID { get; set; }
        public DateTime RecDateCreated { get; set; }
    }

}
