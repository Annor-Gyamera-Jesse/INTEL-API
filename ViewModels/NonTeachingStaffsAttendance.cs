namespace INTEL_API.ViewModels
{
    public class NonTeachingStaffsAttendance
    {
        public int NonTeachingStaffsAttendanceID { get; set; }
        public string NonTeachingStaffsFirstName { get; set; }
        public string NonTeachingStaffsLastName { get; set; }
        public bool EnableSwitch { get; set; }
        public DateTime ClockIN { get; set; }
        public DateTime ClockOUT { get; set; }
    }
}
