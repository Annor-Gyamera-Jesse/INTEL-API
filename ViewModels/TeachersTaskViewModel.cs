namespace INTEL_API.ViewModels
{
    public class TeachersTaskViewModel
    {
        public string TeachersName { get; set; }
        public string TeacherTask { get; set; }
        public bool SwitchBar { get; set; } // Assuming SwitchBar is a boolean
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
