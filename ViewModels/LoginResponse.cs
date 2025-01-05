namespace INTEL_API.ViewModels
{
    public class LoginResponse
    {
        public bool IsAuthorized { get; set; }
        public int? UserId { get; set; } // Nullable in case of unauthorized access
        public string UserName { get; set; }
        public string Role { get; set; }
        public string ClassID { get; set; }
        public string MenuItem { get; set; }
        public string CategoryName { get; set; }
        public string Message { get; set; }
    }
}
