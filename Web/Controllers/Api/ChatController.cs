using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Security.Claims;
using Web.Models.MongoDB;

namespace Web.Controllers.Api
{
    public class ChatController : Controller
    {
        private readonly MongoRepository _repository;
        private readonly IMemberService _memberService;
        //private readonly ChatIndexViewModelService _chatIndexViewModelService;

        public ChatController(MongoRepository repository, IMemberService memberService)
        {
            _repository = repository;
            _memberService = memberService;
            //_chatIndexViewModelService = chatIndexViewModelService;
        }

        public async Task<IActionResult> Index(int id, string type)
        {
            // 抓SenderID
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (memberIdClaim == null)
            { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            int memberId = int.Parse(memberIdClaim.Value);
            var result = await _memberService.GetMemberId(memberId);

            if (!result)
            { return RedirectToAction(nameof(AccountController.Account), "Account"); }
            string member = await _memberService.GetMemberName(memberId);

            ViewBag.SenderId = memberId;
            ViewBag.SenderName = member;

            if (type == "tutor")
            {
                // 抓ReceiverId
                var tutor = await _memberService.GetTutor(id);
                ViewBag.CourseId = id;
                ViewBag.ReceiverId = tutor.MemberId;
                ViewBag.ReceiverName = tutor.MemberName;
                ViewBag.HeadShotImage = tutor.HeadShotImage;
                ViewBag.CourseTitle = tutor.CourseTitle;
                ViewBag.CourseSubTitle = tutor.CourseSubTitle;
            }
            else if (type == "student")
            {
                // 抓ReceiverId
                var receiver = await _memberService.GetStudent(id);
                ViewBag.CourseId = 0;
                ViewBag.ReceiverId = receiver.MemberId;
                ViewBag.ReceiverName = receiver.MemberName;
                ViewBag.HeadShotImage = receiver.HeadShotImage;
                ViewBag.CourseTitle = "";
                ViewBag.CourseSubTitle = "";
            }
            return View();
        }

        [HttpGet("api/chat/history")]
        public async Task<IActionResult> GetChatHistory(string user1, string user2)
        {
            var conversationId = CreateConversationId(user1, user2);
            var messages = await _repository.GetMessagesByConversationIdAsync(conversationId);
            return Ok(messages);
        }

        private ObjectId CreateConversationId(string user1, string user2)
        {
            var idString = string.Compare(user1, user2) < 0 ? $"{user1}_{user2}" : $"{user2}_{user1}";
            return new ObjectId(HashStringToObjectId(idString));
        }

        private string HashStringToObjectId(string input)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                var hash = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 24);
            }
        }
    }
}
