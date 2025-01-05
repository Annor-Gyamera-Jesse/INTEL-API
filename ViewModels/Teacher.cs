namespace INTEL_API.ViewModels
{
    public class Teacher
    {
        public int TeacherID { get; set; }
        public string TeacherFirstName { get; set; }
        public string TeacherLastName { get; set; }
        public DateTime TeacherDateOfBirth { get; set; }
        public string TeacherGender { get; set; } // Assuming gender is stored as a string
        public string TeacherAddress { get; set; }
        public string TeacherPhoneNumber { get; set; }
        public string TeacherEmail { get; set; }
        public string GuardianFullName { get; set; }
        public string GuardianGender { get; set; } // Assuming gender is stored as a string
        public string GuardianHouseAddress { get; set; }
        public string GuardianWorkAddress { get; set; }
        public string GuardianEmail { get; set; }
        public string GuardianFirstContact { get; set; }
        public string GuardianSecondContact { get; set; }
        public bool EnableSwitch { get; set; }
        public DateTime ClockIN { get; set; }
        public DateTime ClockOUT { get; set; }
    }
}
