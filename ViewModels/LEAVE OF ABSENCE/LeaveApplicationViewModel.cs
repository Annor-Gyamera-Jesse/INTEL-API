namespace INTEL_API.ViewModels.LEAVE_OF_ABSENCE
{
    namespace INTEL_API.ViewModels
    {
        public class LeaveApplicationViewModel
        {
            public int UserID { get; set; }
            public string LeaveType { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Reason { get; set; }
        }
    }

}
