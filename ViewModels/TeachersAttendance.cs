namespace INTEL_API.ViewModels
{
    public class TeachersAttendance
    {
        public int TeachersAttendanceID { get; set; }
        public string TeacherFirstName { get; set; }
        public string TeacherLastName { get; set; }
        public bool EnableSwitch { get; set; }
        public DateTime ClockIN { get; set; }
    }
}
