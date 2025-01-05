namespace INTEL_API.ViewModels
{
    public class NoticeBoardViewModel
    {
        public int NoticeID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; } // Indicates if the notice has been read by the user
    }
}
