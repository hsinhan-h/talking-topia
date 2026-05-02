namespace Web.ViewModels
{
    public class AppointmentDetailsViewModel
    {
        public List<AppointmentDetailVM> AppointmentDetailsList { get; set; }
       
    }
    public class AppointmentDetailVM
    {
        /// <summary>
        /// 課程ID 
        /// </summary>
        public int CourseId { get; set; }
        /// <summary>
        /// 預約上課日期
        /// </summary>
        public string BookingDate { get; set; }
        /// <summary>
        /// 預約上課時間
        /// </summary>
        public string BookingTime { get; set; }
        /// <summary>
        /// 學生ID
        /// </summary>
        public int StudentId { get; set; }
        /// <summary>
        /// 學生名稱
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// 課程科目
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 課程名稱
        /// </summary>
        public string CourseTitle { get; set; }
    }
}
