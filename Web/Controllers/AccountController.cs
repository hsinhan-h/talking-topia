using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using Web.Exceptions;
using Web.Services;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Data;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using System.Security.Cryptography;
using System.Text;
using Web.Data;
using TalkingTopiaContext = Infrastructure.Data.TalkingTopiaDbContext;
using ApplicationCore.Interfaces;


namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly TalkingTopiaContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly AccountService _accountService;
        private readonly IRepository _repository;
        private readonly ILineAuthService _lineAuthService;
        private readonly IEmailService _emailService;



        public AccountController(TalkingTopiaContext context, ILogger<AccountController> logger, AccountService accountService, IRepository repository, ILineAuthService lineAuthService, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _accountService = accountService;
            _repository = repository;
            _lineAuthService = lineAuthService;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            var model = new AccountViewModel
            {
                LoginViewModel = new LoginViewModel(),
                RegisterViewModel = new RegisterViewModel(),
                IsAuthenticated = User.Identity.IsAuthenticated // 判斷是否登入
            };
            return View(model);
        }

        // 帳戶頁面
        public IActionResult Account()
        {
            bool isLoggedIn = User.Identity.IsAuthenticated;
            ViewData["IsLoggedIn"] = isLoggedIn;
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        // 註冊
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Register(AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 執行註冊邏輯
                    await _accountService.RegisterUserAsync(model);

                    // 取得註冊用戶的驗證 Token
                    var member = await _accountService.GetMemberByEmailAsync(model.RegisterViewModel.Email);
                    if (member == null)
                    {
                        ModelState.AddModelError("", "註冊過程中發生錯誤，請稍後再試。");
                        return View("Register", model);
                    }

                    // 生成驗證連結
                    var verificationUrl = Url.Action("VerifyEmail", "Account", new { token = member.EmailVerificationToken }, Request.Scheme);

                    // 發送驗證電子郵件
                    var subject = "驗證會員信箱";
                    var body = $"<p>請點擊以下連結以驗證您的信箱：</p><a href='{verificationUrl}'>驗證信箱</a>";
                    await _emailService.SendEmailAsync(member.Email, subject, body);

                    // 註冊成功後，使用 TempData 傳遞成功訊息
                    TempData["SuccessMessage"] = "註冊成功，請查收您的電子郵件以完成驗證";
                    return RedirectToAction("RegisterSuccess");
                }
                catch (UserAlreadyExistsException)
                {
                    // 添加錯誤訊息到 ModelState
                    ModelState.AddModelError(nameof(model.RegisterViewModel.Email), "該電子郵件已被註冊");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "註冊過程中發生錯誤");
                    ModelState.AddModelError("", "註冊過程中發生錯誤，請稍後再試。");
                }
            }

            // 返回註冊頁面，而不是 Account 頁面
            return View("Register", model);
        }
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(loginViewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login([FromForm] AccountViewModel model)
        {
            var request = model.LoginViewModel;

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "登入失敗，請檢查您的輸入資料");
                return View("AccessDenied", model);
            }

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                ModelState.AddModelError(string.Empty, "帳號或密碼不可為空");
                return View("AccessDenied", model);
            }

            // 使用 AccountService 取得會員資料
            var member = await _accountService.GetMemberByEmailAsync(request.Email);

            // 驗證使用者是否存在
            if (member == null)
            {
                ModelState.AddModelError(string.Empty, "無效的帳號或密碼");
                return View("AccessDenied", model);
            }

            // 檢查是否已經完成 Email 驗證
            if (!member.IsEmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "您的信箱尚未驗證，請先完成驗證。");
                return View("AccessDenied", model);
            }

            // 使用 PasswordHasher 驗證密碼
            var passwordHasher = new PasswordHasher<Web.Entities.Member>();
            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(member, member.Password, request.Password);

            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                _logger.LogError("密碼驗證失敗。輸入的密碼: {Password}", request.Password);
                ModelState.AddModelError(string.Empty, "無效的帳號或密碼");
                return View("AccessDenied", model);
            }


            // 使用 SignInMemberAsync 進行登入
            await _accountService.SignInMemberAsync(member);

            if (string.IsNullOrEmpty(request.ReturnUrl))
            {
                request.ReturnUrl = "/";
            }

            return Redirect(request.ReturnUrl);
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult RegisterSuccess()
        {
            if (TempData["SuccessMessage"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpGet]
        public IActionResult LineLogin()
        {
            var state = Guid.NewGuid().ToString("N");
            HttpContext.Session.SetString("LineState", state);

            var redirectUrl = $"https://access.line.me/oauth2/v2.1/authorize?response_type=code&client_id=2006372467&redirect_uri=https://talkingtopia-edfmd7fmeudpckg6.japaneast-01.azurewebsites.net/Account/SSOcallback&state={state}&scope=profile%20openid%20email";
            return Redirect(redirectUrl);
        }

        public async Task<IActionResult> SSOcallback(string code, string state)
        {
            // 從 GetAccessTokenAsync 獲取 LineTokenResponseDto
            var tokenResponse = await _lineAuthService.GetAccessTokenAsync(code);
            var accessToken = tokenResponse.AccessToken; // 提取 access_token
            var idToken = tokenResponse.IdToken; // 提取 id_token

            // 使用 accessToken 獲取 userProfile
            var userProfile = await _lineAuthService.GetUserProfileAsync(accessToken);

            // 檢查 LineUserId 是否有效
            if (string.IsNullOrEmpty(userProfile.LineUserId))
            {
                throw new Exception("無法取得 LineUserId，登入失敗");
            }

            // 使用 id_token 來解碼並獲取 email
            var email = await _lineAuthService.GetEmailFromIdTokenAsync(idToken);

            // 使用 LineUserId 查找 Members 表中的用戶
            var existingMember = await _repository.GetMemberByLineIdAsync(userProfile.LineUserId);

            if (existingMember == null)
            {
                // 處理 email 為空的情況
                email = string.IsNullOrEmpty(email) ? $"{userProfile.LineUserId}@line.com" : email;

                // 新增 User 到 Users 表
                var newUser = new User
                {
                    Name = userProfile.DisplayName,
                    Email = email,
                    LineId = userProfile.LineUserId,
                    PictureUrl = userProfile.PictureUrl
                };

                _repository.Create(newUser);
                await _repository.SaveChangesAsync();

                // 新增 Member 到 Members 表
                var newMember = new Member
                {
                    UserId = newUser.Id, // 將 User Id 設定到 Member 中
                    LineUserId = userProfile.LineUserId,
                    FirstName = userProfile.DisplayName,
                    Password = GenerateRandomPassword(),
                    LastName = "", // 預設為空白
                    Birthday = null,
                    Email = email, // 使用從 id_token 取得的 email 或生成的 email
                    Nickname = "",
                    Phone = "",
                    HeadShotImage = userProfile.PictureUrl,
                    Cdate = DateTime.Now,
                    Udate = DateTime.Now,
                    IsTutor = false,
                    IsVerifiedTutor = false,
                    EmailVerificationToken = "" // 初始化為空字符串，避免資料庫錯誤
                };

                _repository.Create(newMember);
                await _repository.SaveChangesAsync();

                existingMember = newMember;
            }

            // 設置登入 Claims，確保使用 Members 表中的 MemberId
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, existingMember.MemberId.ToString()),
        new Claim(ClaimTypes.Name, existingMember.FirstName),
        new Claim("picture", existingMember.HeadShotImage ?? string.Empty)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            return RedirectToAction("Index", "Home");
        }

        private string GenerateRandomPassword(int length = 10)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // 顯示忘記密碼的畫面 (GET 方法)
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // 處理忘記密碼的請求 (POST 方法)
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 使用 AccountService 查詢該 email 是否存在於系統中
                var member = await _accountService.GetMemberByEmailAsync(model.Email);

                if (member != null)
                {
                    // 建立重設密碼的 token 並儲存在資料庫
                    var token = Guid.NewGuid().ToString();
                    member.ResetPasswordToken = token;
                    await _accountService.UpdateMemberAsync(member);

                    // 建立重設密碼的連結
                    var resetUrl = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);

                    var subject = "忘記密碼";
                    var body = $"<p>請點擊以下連結以重設您的密碼：</p><a href='{resetUrl}'>重設密碼</a>";

                    // 發送電子郵件
                    await _emailService.SendEmailAsync(model.Email, subject, body);

                    TempData["SuccessMessage"] = "重設密碼的連結已寄到您的電子郵件，請查收";
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError("", "該電子郵件未註冊");
                }
            }

            return View(model);
        }

        // 忘記密碼確認畫面
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var member = await _accountService.GetMemberByResetTokenAsync(model.Token);
                if (member != null)
                {
                    var passwordHasher = new PasswordHasher<Web.Entities.Member>();
                    member.Password = passwordHasher.HashPassword(member, model.NewPassword); // 使用 PasswordHasher 加密新密碼
                    member.ResetPasswordToken = null;
                    await _accountService.UpdateMemberAsync(member);

                    TempData["SuccessMessage"] = "密碼已成功重設";
                    return RedirectToAction("ResetPasswordSuccess");
                }
                else
                {
                    // 調試訊息
                    _logger.LogError($"Token not found: {model.Token}");
                    ModelState.AddModelError("", "無效的重設密碼請求");
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult ResetPasswordSuccess()
        {
            return View();
        }

        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("無效的驗證請求");
            }

            var member = await _accountService.GetMemberByVerificationTokenAsync(token);

            if (member == null)
            {
                return NotFound("驗證失敗，無法找到相關帳戶");
            }

            // 驗證成功，更新 IsEmailConfirmed 和清除 EmailVerificationToken
            if (member.EmailVerificationToken == token && !member.IsEmailConfirmed)
            {
                member.IsEmailConfirmed = true;
                member.EmailVerificationToken = ""; // 清除驗證 Token
                await _accountService.UpdateMemberAsync(member);

                TempData["SuccessMessage"] = "電子郵件已驗證成功！";
                return RedirectToAction("Account", "Account");
            }
            else
            {
                return BadRequest("驗證失敗，無效的驗證請求");
            }
        }



    }

    public static class StringExtensions
    {
        public static string ToSHA256(this string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }


}
