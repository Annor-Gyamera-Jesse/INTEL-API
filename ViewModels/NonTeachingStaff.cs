namespace INTEL_API.ViewModels
{
    public class NonTeachingStaff
    {
        public int NTSID { get; set; }
        public string NonTeachingStaffsFirstName { get; set; }
        public string NonTeachingStaffsLastName { get; set; }
        public DateTime NonTeachingStaffsDateOfBirth { get; set; }
        public char NonTeachingStaffsGender { get; set; } // Assuming gender is stored as a single character
        public string NonTeachingStaffsAddress { get; set; }
        public string NonTeachingStaffsPhoneNumber { get; set; }
        public string NonTeachingStaffsEmail { get; set; }
        public byte[] ImageData { get; set; } // Assuming image data is stored as a byte array
        public string GuardianFullName { get; set; }
        public char GuardianGender { get; set; } // Assuming gender is stored as a single character
        public string GuardianHouseAddress { get; set; }
        public string GuardianWorkAddress { get; set; }
        public string GuardianEmail { get; set; }
        public string GuardianFirstContact { get; set; }
        public string GuardianSecondContact { get; set; }
        // Add additional properties as needed
    }
}
