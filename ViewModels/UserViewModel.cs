namespace INTEL_API.ViewModels
{
    public class UserViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; } // Considering omitting this in the response for security reasons
        public List<UserRoleViewModel> Roles { get; set; } = new List<UserRoleViewModel>();
    }
}
