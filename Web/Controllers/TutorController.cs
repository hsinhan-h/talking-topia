using Microsoft.AspNetCore.Mvc;
using Web.Entities;
using Web.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ApplicationCore.Entities;
using System.Security.Claims;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Authorization;



namespace Web.Controllers
{
    public class TutorController : Controller
    {
        private readonly ILogger<TutorController> _logger;
        private readonly ResumeDataService _resumeDataService;
        private readonly BookingService _bookingService;
        private readonly TutorDataservice _tutorDataService;
        private readonly AppointmentDetailViewModelService _appointmentDetailVMService;
        private readonly CourseCategoryService _courseCategoryService;
        private readonly IMemberService _memberService;
        public TutorController(ResumeDataService resumeDataService, BookingService bookingService, TutorDataservice tutorDataservice, AppointmentDetailViewModelService appointmentDetailService, CourseCategoryService courseCategoryService, IMemberService memberService, ILogger<TutorController> logger)
        {
            _resumeDataService = resumeDataService;
            _bookingService = bookingService;
            _tutorDataService = tutorDataservice;
            _appointmentDetailVMService = appointmentDetailService;
            _courseCategoryService = courseCategoryService;
            _memberService = memberService;
            _logger = logger;
        }



        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> IndexpostAsync()
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            int memberId = int.Parse(memberIdClaim.Value);
            var result = await _memberService.GetMemberId(memberId);

            if (!result)
            { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            return RedirectToAction(nameof(TutorResume));
        }


        //Tutor Data Read and update
        [HttpGet]
        public async Task<IActionResult> TutorData()
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            int memberId = int.Parse(memberIdClaim.Value);
            var result = await _memberService.GetMemberId(memberId);
            if (!result)
            {
                return RedirectToAction(nameof(AccountController.Account), "Account");
            }
            var tutorData = await _tutorDataService.GetAllInformationAsync(memberId);
            var isTeacher = await _tutorDataService.Isteacher(memberId);
            if (!isTeacher)
            {
                // 如果還不是老師，返回一個訊息頁面或顯示提示訊息
                ViewData["Message"] = "您還沒成為老師，請提交履歷，如已提交請耐心等待。";
                return View("_ShowMessage");
            }

            ViewData["MemberId"] = memberId;
            return View(tutorData);
        }


        [HttpPost]

        public async Task<IActionResult> TutorData(TutorDataViewModel qVM)
        {
            if (!ModelState.IsValid)
            {
                var fieldErrors = ModelState.Where(ms => ms.Value.Errors.Any())
                                    .ToDictionary(
                                        ms => ms.Key,
                                        ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList()
                                    );

                ViewData["Success"] = false;
                ViewData["ValidationErrors"] = fieldErrors;
                return View(qVM);
            }
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            int memberId = int.Parse(memberIdClaim.Value);
            var result = await _tutorDataService.CreateTutorData(qVM, memberId);

            if (result.Success)
            {
                TempData["Message"] = "教師資料更新成功";
                return RedirectToAction("TutorData");

            }
            else
            {
                TempData["Message"] = "教師資料更新失敗";
                return View("_ShowMessage");
            }
        }

        [HttpPost]
        public async Task<IActionResult> TutorTimeData(TutorDataViewModel qVM)
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            {
                return RedirectToAction(nameof(AccountController.Account), "Account");
            }

            int memberId = int.Parse(memberIdClaim.Value);

            var resultTime = await _tutorDataService.CreateTutorTimeData(qVM, memberId);
            if (resultTime.Success)
            {

                TempData["Message"] = qVM.Message;
                return RedirectToAction("TutorData");
            }
            else
            {
                TempData["Message"] = "教師資料更新失敗";
                return View("_ShowMessage");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TutorResumeAsync()
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            {
                return RedirectToAction(nameof(AccountController.Account), "Account");
            }

            int memberId = int.Parse(memberIdClaim.Value);
            var result = await _memberService.GetMemberId(memberId);
            if (!result)
            {
                return RedirectToAction(nameof(AccountController.Account), "Account");
            }

            var allTutorResumeData = await _resumeDataService.ReadAllTutorResumeAsync(memberId);
            if (allTutorResumeData == null)
            {
                allTutorResumeData = new TutorResumeViewModel();
            }
            ViewData["MemberId"] = memberId;

            return View(allTutorResumeData);
        }
        [HttpPost]
        public async Task<IActionResult> TutorResume(TutorResumeViewModel qVM)
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            {
                return RedirectToAction(nameof(AccountController.Account), "Account");
            }
            int memberId = int.Parse(memberIdClaim.Value);
            ViewData["MemberId"] = memberId;
            if (qVM.StudyStartYear.HasValue && qVM.StudyEndYear.HasValue)
            {
                if (qVM.StudyStartYear > qVM.StudyEndYear)
                {
                    ModelState.AddModelError("StudyStartYear", "就學始年必須小於結束年");
                }
            }
            if (!ModelState.IsValid)
            {
                return View(qVM);
            }

            
            var result = await _resumeDataService.AddResumeAsync(qVM, memberId);

            if (result.Success)
            {
                TempData["Message"] = "履歷資料更新成功";
                return RedirectToAction("TutorData");
            }
            else
            {
                ViewData["Message"] = "履歷資料更新失敗";
                return View("TutorResume", qVM);
            }
        }

        /// <summary>
        /// 課程列表資訊
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PublishCourse()
        {
            //int memberId = 3;

            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            int memberId = int.Parse(memberIdClaim.Value);
            var result = await _memberService.GetMemberId(memberId);

            var model = await _bookingService.GetPublishCourseList(memberId);
            // 歷史課程
            ViewData["HistoryList"] = await _bookingService.GetPublishCourseHistoryList(memberId);
            // 課程類別列表(下拉選單使用)
            ViewData["CourseCategoryList"] = await _courseCategoryService.GetCourseCategoryListAsync();
            ViewBag.IsTutor = _memberService.GetIsTutor(memberId);

            return View(model);
        }
        public IActionResult RecommendedTutorAI()
        {
            return View();
        }

        /// <summary>
        /// 測試使用
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Test()
        {
            int MemberId = 3;
            var model = await _bookingService.GetPublishCourseList(MemberId);
            ViewData["HistoryList"] = await _bookingService.GetPublishCourseHistoryList(MemberId);

            return View(model);
        }

        public async Task<IActionResult> AppointmentDetails()
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null) { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            int memberId = int.Parse(memberIdClaim.Value);
            var result = await _memberService.GetMemberId(memberId);
            if (!result) { return RedirectToAction(nameof(AccountController.Account), "Account"); }

            var isTeacher = await _tutorDataService.Isteacher(memberId);
            if (!isTeacher)
            {
                // 如果還不是老師，返回一個訊息頁面或顯示提示訊息
                ViewData["Message"] = "您還沒成為老師，請提交履歷，如已提交請耐心等待。";
                return View("_ShowMessage");
            }
            var appointmentDetails = await _appointmentDetailVMService.GetAppointmentData(memberId);
            if (appointmentDetails == null)
            {
                appointmentDetails = new AppointmentDetailsViewModel
                {
                    AppointmentDetailsList = new List<AppointmentDetailVM>()
                };
            }
            return View(appointmentDetails);
        }

        
    }
}
