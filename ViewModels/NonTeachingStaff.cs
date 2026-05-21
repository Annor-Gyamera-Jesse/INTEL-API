namespace INTEL_API.ViewModels
{
    public class NonTeachingStaff
    {
        public int NTSID { get; set; }
        public string NonTeachingStaffsFirstName { get; set; }
        public string NonTeachingStaffsLastName { get; set; }
        public DateTime NonTeachingStaffsDateOfBirth { get; set; }
        public char NonTeachingStaffsGender { get; set; }
        public string NonTeachingStaffsAddress { get; set; }
        public string NonTeachingStaffsPhoneNumber { get; set; }
        public string NonTeachingStaffsEmail { get; set; }
        public byte[] ImageData { get; set; } 
        public string GuardianFullName { get; set; }
        public char GuardianGender { get; set; }
        public string GuardianHouseAddress { get; set; }
        public string GuardianWorkAddress { get; set; }
        public string GuardianEmail { get; set; }
        public string GuardianFirstContact { get; set; }
        public string GuardianSecondContact { get; set; }
    }
}
