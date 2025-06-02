namespace INTEL_API.ViewModels
{
    public class StudentsAttendance
    {
        public int UserId { get; set; }
        public int AttendanceID { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string ClassID { get; set; }
        public bool EnableSwitch { get; set; }
        public int TermID { get; set; }
    }
}
