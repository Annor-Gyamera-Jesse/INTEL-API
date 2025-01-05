namespace INTEL_API.ViewModels
{
    public class TeacherSubjectAssignmentViewModel
    {
        public int AssignmentID { get; set; }
        public string TeacherName { get; set; }
        public string SCID { get; set; }
        public string ClassID { get; set; }
        public int DayID { get; set; }
        public DateTime SubjectStartTime { get; set; }
        public DateTime SubjectEndTime { get; set; }
        public DateTime RecDateCreated { get; set; }
    }
}
