using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Entities;

namespace Web.Controllers
{
    public class BookingTableController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly CourseService _courseService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IMemberService _memberService;
        private readonly MemberAppointmentService _memberAppointmentService;
        private readonly IEmailService _emailService;

        public BookingTableController(BookingService bookingService, CourseService courseService, IShoppingCartService shoppingCartService, IMemberService memberService, MemberAppointmentService memberAppointmentService, IEmailService emailService)
        {
            _bookingService = bookingService;
            _courseService = courseService;
            _shoppingCartService = shoppingCartService;
            _memberService = memberService;
            _memberAppointmentService = memberAppointmentService;
            _emailService = emailService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int courseId, DateTime bookingDate, short bookingTime)
        {
           
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            int memberId = int.Parse(memberIdClaim.Value);
            var result = await _memberService.GetMemberId(memberId);

            if (!result) { return BadRequest("找不到會員"); }

            // todo: 剩餘堂數 > 0, 允許預約
            // 寫入booking資料表, 導向預約成功頁面，發email通知預約成功
            int remainSessions = await _bookingService.GetRemainCourseQtyAsync(memberId, courseId);
            if (remainSessions > 0)
            {
                _bookingService.CreateBookingData(courseId, bookingDate, bookingTime, memberId);

                //發送預約成功郵件
                var memberEmail = await _memberAppointmentService.GetMemberEmailAsync(memberId);
                var memberName = await _memberAppointmentService.GetMemberFirstNameAsync(memberId);
                var courseName = await _courseService.GetCourseNameAsync(courseId);
                string subject = $"Talking Topia - 課程預約成功通知: {courseName}";
                string formattedTime = new TimeSpan(bookingTime - 1, 0, 0).ToString(@"hh\:mm");
                string body = $"{memberName} 您好，<br>您已成功在Talking Topia預約一筆課程<br><br>課程名稱: {courseName}<br>預約時間:  {bookingDate.ToString("yyyy-MM-dd")} {formattedTime}";

                //用Task.Run()將發信任務放到背景執行
                Task.Run(() => _emailService.SendEmailAsync(memberEmail, subject, body));

                return RedirectToAction("Success", "BookingTable");
            }

            //todo: 剩餘堂數 <= 0, 將課程寫入購物車資料表, 跳轉購物車 
            else
            {
                await _shoppingCartService.CreateShoppingCartAsync(memberId, courseId, 25, 1, bookingDate, bookingTime);
                return RedirectToAction("Index", "ShoppingCart");
            }
        }


        public IActionResult Success()
        {
            return View();
        }
    }


}
