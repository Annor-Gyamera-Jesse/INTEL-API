namespace INTEL_API.ViewModels
{
    public class TeachersLessonNoteViewModel
    {
        public int UserId { get; set; }
        public string SchoolCourse { get; set; }
        public string Strand { get; set; }
        public string SubStrand { get; set; }
        public string ContentStandard { get; set; }
        public string Indicator { get; set; }
        public string TeachingLearningResources { get; set; }
        public string TeachingLearningResourcePreparationNotes { get; set; }
        public string SourcesLearningResources { get; set; }
        public string LearningGroup { get; set; }
        public string LearnerExpectation { get; set; }
        public string ImportantGradeExpectation { get; set; }
        public string LearningOutcomes { get; set; }
        public string FormofAssessment { get; set; }
        public string LearnerEntryBehavior { get; set; }
        public string SequenceofLesson { get; set; }
        public string ClassID { get; set; }
        public DateTime WeekEnding { get; set; } // Assuming Week_Ending is a DateTime
    }
}
