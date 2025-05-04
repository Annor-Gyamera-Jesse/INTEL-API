namespace INTEL_API.ViewModels.PhotoRecord
{
    public class PhotoRecord
    {
        public string Class { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string PhotoTitle { get; set; }
        public string PhotoDescription { get; set; }
        public string PhotoData { get; set; } // base64 string
        public int TermID { get; set; }
        public int UserID { get; set; }
    }
}
