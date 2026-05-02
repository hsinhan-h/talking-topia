using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Web.Entities;
using Web.Repository;
using Web.ViewModels;
using Booking = Web.Entities.Booking;
using Course = Web.Entities.Course;
using Member = Web.Entities.Member;
using OrderDetail = Web.Entities.OrderDetail;

namespace Web.Services
{
    public class AppointmentDetailViewModelService
    {
        private readonly ILogger<AppointmentDetailViewModelService> _logger;
        private readonly IRepository<ApplicationCore.Entities.Course> _courseRepository;
        private readonly IRepository<ApplicationCore.Entities.Member> _memberRepository;
        private readonly IRepository<ApplicationCore.Entities.Booking> _bookingRepository;
        private readonly IRepository<ApplicationCore.Entities.CourseSubject> _courseSubjectRepository;

        public AppointmentDetailViewModelService(ILogger<AppointmentDetailViewModelService> logger,
                                                 IRepository<ApplicationCore.Entities.Course> courseRepository,
                                                 IRepository<ApplicationCore.Entities.Member> memberRepository,
                                                 IRepository<ApplicationCore.Entities.Booking> bookingRepository,
                                                 IRepository<ApplicationCore.Entities.CourseSubject> courseSubjectRepository)
        {
            _logger = logger;
            _courseRepository = courseRepository;
            _memberRepository = memberRepository;
            _bookingRepository = bookingRepository;
            _courseSubjectRepository = courseSubjectRepository;
        }

        public async Task<AppointmentDetailsViewModel> GetAppointmentData(int tutorId)
        {
            // 先找出教師開立的所有課程
            var courses = await _courseRepository.ListAsync(c => c.TutorId == tutorId);

            if(courses.Count < 1) return null;

            var bookingResult = new List<AppointmentDetailVM>();

            foreach (var course in courses)
            {
                // 找出課程的預約紀錄
                var bookings = await _bookingRepository.ListAsync(b => b.CourseId == course.CourseId);

                if (bookings.Count < 1) continue;

                var subject = await _courseSubjectRepository.GetByIdAsync(course.SubjectId);

                foreach (var booking in bookings)
                {
                    if (booking.BookingDate < DateTime.Now.AddDays(1)) continue;
                    var student = await _memberRepository.GetByIdAsync(booking.StudentId);
                    var bookingDetail = new AppointmentDetailVM
                    {
                        CourseId = course.CourseId,
                        BookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                        BookingTime = booking.BookingTime + ":00",
                        StudentId = student.MemberId,
                        FullName = student.FirstName + " " + student.LastName,
                        Subject = subject.SubjectName,
                        CourseTitle = course.Title,
                    };
                    bookingResult.Add(bookingDetail);
                }
            }
            var result = new AppointmentDetailsViewModel()
            {
                AppointmentDetailsList = bookingResult,
            };

            return result;
        }
    }
}
