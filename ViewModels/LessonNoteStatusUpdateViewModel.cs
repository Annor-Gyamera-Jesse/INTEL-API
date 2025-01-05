namespace INTEL_API.ViewModels
{
    public class LessonNoteStatusUpdateViewModel
    {
        public int LessonNoteId { get; set; }
        public string Status { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public enum LessonNoteStatus
    {
        New = 1,
        Approved = 2,
        Cancelled = 3
    }
}
