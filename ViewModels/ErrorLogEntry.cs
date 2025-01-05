namespace INTEL_API.ViewModels
{
    public class ErrorLogEntry
    {
        public DateTime LogDate { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public int? UserID { get; set; } // Nullable to allow for cases where UserID is not provided
    }
}
