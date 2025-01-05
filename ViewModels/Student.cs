namespace INTEL_API.ViewModels
{
    public class Student
    {
        public int StudentID { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public DateTime StudentDateOfBirth { get; set; }
        public char StudentGender { get; set; } // Assuming gender is stored as a single character
        public string StudentAddress { get; set; }
        public string StudentPhoneNumber { get; set; }
        public string StudentEmail { get; set; }
        public byte[] ImageData { get; set; } // Assuming image data is stored as a byte array
        public string ClassID { get; set; }
        public string GuardianFullName { get; set; }
        public char GuardianGender { get; set; } // Assuming gender is stored as a single character
        public string GuardianHouseAddress { get; set; }
        public string GuardianWorkAddress { get; set; }
        public string GuardianEmail { get; set; }
        public string GuardianFirstContact { get; set; }
        public string GuardianSecondContact { get; set; }
        public bool EnableSwitch { get; set; }
    }
}
