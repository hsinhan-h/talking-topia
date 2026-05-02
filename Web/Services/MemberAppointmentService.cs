using Humanizer;
using Web.ViewModels;

namespace Web.Services
{
    public class MemberAppointmentService
    {
        private readonly IRepository _repository;

        public MemberAppointmentService(IRepository repository)
        {
            _repository = repository;
        }

        //Logic 

        public async Task<MemberAppointmentViewModel> GetAppointmentData(int memberId)
        {

            var memberAppointments = from booking in _repository.GetAll<Booking>()
                         where booking.StudentId == memberId
                         join course in _repository.GetAll<Course>() on booking.CourseId equals course.CourseId
                         join tutor in _repository.GetAll<Member>() on course.TutorId equals tutor.MemberId
                         join order in _repository.GetAll<Order>() on memberId equals order.MemberId
                         join orderDetail in _repository.GetAll<OrderDetail>() on order.OrderId equals orderDetail.OrderId
                         group new { booking, course, tutor, order, orderDetail } by booking.BookingId into gp

                         select new MemberAppointmentVM
                         {
                             BookingId = gp.Key,
                             MemberId = memberId,          // 會員ID
                             CourseId = gp.FirstOrDefault().booking.CourseId,   // 課程ID
                             //TrackingNumber = "",              // 訂單編號
                             FullName = gp.FirstOrDefault().tutor.FirstName + " " + gp.FirstOrDefault().tutor.LastName, // 教師名稱
                             CourseTitle = gp.FirstOrDefault().course.Title,        // 課程名稱
                             Subtitle = gp.FirstOrDefault().course.SubTitle,//課程副標題
                             CourseLength = gp.FirstOrDefault().orderDetail.CourseType == 1 ? "25分鐘" : "50分鐘",// 購買時長
                             Quantity = gp.FirstOrDefault().orderDetail.Quantity,  // 購買堂數
                             TotalPrice = gp.FirstOrDefault().orderDetail.TotalPrice, // 總價
                             TaxIdNumber = gp.FirstOrDefault().order.TaxIdNumber,  // 統一編號
                             OrderDatetime = gp.FirstOrDefault().order.TransactionDate.ToString("yyyy-MM-dd"), // 格式化交易時間
                             BookingDate = gp.FirstOrDefault().booking.BookingDate,//預約上課日期
                             BookingTime = gp.FirstOrDefault().booking.BookingTime
                         };

            var orderedMemberAppointments = memberAppointments.OrderByDescending(o => o.BookingDate);

            // 返回包含訂單資訊的 ViewModel
            return new MemberAppointmentViewModel()
            {
                MemberAppointmentList = await orderedMemberAppointments.ToListAsync(), 
            };
        }

        public async Task<string> GetMemberEmailAsync(int memberId)
        {
            var member = await _repository.GetMemberByIdAsync(memberId);
            return member?.Email;
        }

        public async Task<string> GetMemberFirstNameAsync(int memberId)
        {
            var member = await _repository.GetMemberByIdAsync(memberId);
            return member?.FirstName;
        }

    }
}
