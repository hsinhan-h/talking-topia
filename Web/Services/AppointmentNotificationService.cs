using ApplicationCore.Interfaces;

namespace Web.Services
{
    public class AppointmentNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly BookingService _bookingService;
        private readonly MemberAppointmentService _memberAppointmentService;
        private readonly CourseService _courseService;
        private readonly ILogger<AppointmentNotificationService> _logger;

        public AppointmentNotificationService(IEmailService emailService, BookingService bookingService, MemberAppointmentService memberAppointmentService, CourseService courseService, ILogger<AppointmentNotificationService> logger)
        {
            _emailService = emailService;
            _bookingService = bookingService;
            _memberAppointmentService = memberAppointmentService;
            _courseService = courseService;
            _logger = logger;
        }

        public async Task SendNotificationsAsync()
        {
            var today = DateTime.Today;
            var bookingsInTwoDays = await _bookingService.GetBookingsByDateAsync(today.AddDays(2));
            var bookingsTomorrow = await _bookingService.GetBookingsByDateAsync(today.AddDays(1));

            await SendReminderEmailsAsync(bookingsInTwoDays, 2); //二天前通知
            await SendReminderEmailsAsync(bookingsTomorrow, 1); //一天前通知
        }

        private async Task SendReminderEmailsAsync(IEnumerable<Booking> bookings, int daysLeft)
        {
            foreach (var booking in bookings)
            {
                try
                {
                    if ((daysLeft == 2 && booking.NotifyCount == 0) || (daysLeft == 1 && booking.NotifyCount <= 1))
                    {
                        var memberEmail = await _memberAppointmentService.GetMemberEmailAsync(booking.StudentId);
                        var memberName = await _memberAppointmentService.GetMemberFirstNameAsync(booking.StudentId);
                        var courseName = await _courseService.GetCourseNameAsync(booking.CourseId);
                        string formattedTime = new TimeSpan(booking.BookingTime - 1, 0, 0).ToString(@"hh\:mm");

                        string subject = $"提醒: {memberName}，您預約的Talking Topia課程將在{daysLeft}天後開始 {courseName}";
                        string body = $"{memberName}您好，<br>提醒您，您已預約的課程 <strong style='color: blue;'>{courseName}</strong> <br>即將在 <strong style='color: red;'>{daysLeft} 天</strong>後開始，請準時與您的教師赴約。<br><br><strong style='color: red;'>預約時間: {booking.BookingDate.ToString("yyyy-MM-dd")} {formattedTime}</strong>";

                        await _emailService.SendEmailAsync(memberEmail, subject, body);

                        //更新notifyCount
                        if (daysLeft == 2)
                        {
                            booking.NotifyCount = 1;
                        }
                        else if (daysLeft == 1) 
                        { 
                            booking.NotifyCount = 2;
                        }

                        await _bookingService.UpdateBookingAsync(booking);
                    }                   
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "發送提醒郵件時發生錯誤");
                }
            }
        }
    }

}
