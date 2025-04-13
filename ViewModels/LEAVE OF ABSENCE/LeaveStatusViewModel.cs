namespace INTEL_API.ViewModels.LEAVE_OF_ABSENCE
{
    public class LeaveStatusViewModel
    {
        public int LeaveID { get; set; }
        public string UserFullName { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime DateRequested { get; set; }
        public DateTime? DateApproved { get; set; }
    }
}
