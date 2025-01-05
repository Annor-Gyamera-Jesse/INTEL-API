namespace INTEL_API.ViewModels
{
    public class SchoolExam
    {
        public int UserId { get; set; }
        public int ExamID { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string AcademicYear { get; set; }
        public DateTime? VacationDate { get; set; } // Nullable DateTime
        public string PromotedTo { get; set; }
        public int NumberOnRoll { get; set; }
        public string Term { get; set; }
        public string Position { get; set; }
        public DateTime? NextTermsBegins { get; set; } // Nullable DateTime
        public int AttendanceOut { get; set; }
        public int AttendanceIn { get; set; }
        public string SchoolCourse { get; set; }
        public int ClassScore { get; set; }
        public int ExamsScore { get; set; }
        public int TotalScore { get; set; }
        public string SubjectsPositions { get; set; }
        public string Grade { get; set; }
        public string TeachersRemarks { get; set; }
        public string Conduct { get; set; }
        public string HeadmasterRemark { get; set; }
        public string SchoolInformation { get; set; }
        public string TeachersSignature { get; set; }
        public string HeadMasterSignature { get; set; }
    }
}
