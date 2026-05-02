namespace Web.Dtos
{
    public class ResumeDto
    {
        public int MemberId { get; set; }
        public List<int> ProfessionalLicenseId { get; set; }
        public List<string> ProfessionalLicenseName { get; set; }
        public List<string> ProfessionalLicenseUrl { get; set; }
        public int ProlLicenseId { get; set; }
        public string ProlLicenseUrl { get; set; }

        public string WorkName { get; set; }
        public DateOnly WorkStartDate { get; set; }
        public DateOnly WorkEndDate { get; set; }
        public List<int> WorkExperienceIds { get; set; }
        public List<string> WorkExperienceFiles { get; set; }
        public List<ResumeWorkExpDto> WorkBackground { get; set; }

    }
    public class ResumeWorkExpDto
    {
        public DateOnly? WorkStartDate { get; set; }
        public DateOnly? WorkEndDate { get; set; }
        public string WorkName { get; set; }
        public int? WorkExperienceId { get; set; }
        public string WorkExperienceFile
        {
            get; set;
        }
    }
    public class MemberIdDto
    {
        public int MemberId { get; set; }
    }
    public class AIImagesResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<AIImage> Data { get; set; }
    }
    public class AIImage
    {
        public string Url1 { get; set; }
        public string Url2 { get; set; }
        public string Url3 { get; set; }
    }
    public class UpdateAIImgDto
    {
        public int MemberId { get; set; }
        public string Url { get; set; }
    }
}
