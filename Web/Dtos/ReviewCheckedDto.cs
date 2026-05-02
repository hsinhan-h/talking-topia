namespace Web.Dtos
{
    public class ReviewCheckedDto
    {
        public class ModerationResult
        {
            public string Id { get; set; }
            public string Model { get; set; }
            public List<Result> Results { get; set; }
        }

        public class Result
        {
            public bool Flagged { get; set; }
            public Dictionary<string, bool> Categories { get; set; }
            public Dictionary<string, float> CategoryScores { get; set; }
        }
    }
}
