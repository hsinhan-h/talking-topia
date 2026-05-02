using ApplicationCore.Interfaces;
using ApplicationCore.Entities;
using Api.Dtos;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Microsoft.AspNetCore.Http.HttpResults;


namespace Api.Services
{
    public class MemberManagermentApiService
    {
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<Nation> _nationRepository;
        private readonly IRepository<ApplyList> _applyListRepository;

        public MemberManagermentApiService(IRepository<Member> memberRepository, IRepository<Nation> nationRepository, IRepository<ApplyList> applyListRepository)
        {
            _memberRepository = memberRepository;
            _nationRepository = nationRepository;
            _applyListRepository= applyListRepository;
        }




        //AI相關
        public async Task<TutorHeadImgDto> GetDbImgUrlinforamtionfun(int memberId)
        {
            // 從資料庫取得對應的 Member
            var member = await _memberRepository.GetByIdAsync(memberId);


            if (member == null)
            {
                throw new Exception($"找不到 ID 為 {memberId} 的會員資料");
            }
            var headImgUrl = member.HeadShotImage;
            var fileType = Path.GetExtension(new Uri(headImgUrl).AbsolutePath).ToLower();

            // 定義支援的圖片類型
            var supportedTypes = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!supportedTypes.Contains(fileType))
            {
                throw new Exception($"不支援的圖片類型: {fileType}");
            }

            // 解析出檔案名稱
            var fileName = Path.GetFileName(new Uri(headImgUrl).AbsolutePath);

            // 回傳包含圖片 URL、檔案名稱和會員 ID 的 DTO
            return new TutorHeadImgDto
            {
                HeadImgUrl = headImgUrl,
                MemberId = memberId,
                FileName = fileName  // 回傳檔案名稱
            };
        }


        public async Task<string> UpdateMemberImageUrlsAsync(int memberId, List<string> imageUrls)
        {
            var memberApplyData =
                from member in await _memberRepository.ListAsync(m => m.MemberId == memberId)
                join applyList in await _applyListRepository.ListAsync(a => a.MemberId == memberId)
                on member.MemberId equals applyList.MemberId
                select new { Member = member, ApplyList = applyList };

            var memberToUpdate = memberApplyData.FirstOrDefault();
            if (memberToUpdate == null)
            {
                return ("未找到該會員。");
            }

            memberToUpdate.ApplyList.AiimageUrl1 = imageUrls.Count > 0 ? imageUrls[0] : null;
            memberToUpdate.ApplyList.AiimageUrl2 = imageUrls.Count > 1 ? imageUrls[1] : null;
            memberToUpdate.ApplyList.AiimageUrl3 = imageUrls.Count > 2 ? imageUrls[2] : null;

            await _applyListRepository.UpdateAsync(memberToUpdate.ApplyList);

            return "會員資料已成功更新。";
        }




        public async Task<List<MemberDataDto>> GetMemberDataList()
        {
            var memberdatainfo = await _memberRepository.ListAsync();
            var nationdatainfo = await _nationRepository.ListAsync();
            var allmemberdata =
            from member in memberdatainfo
            join nation in nationdatainfo on member.NationId equals nation.NationId into nationinfo
            from nation in nationinfo.DefaultIfEmpty() 
            select new MemberDataDto
            {
                MemberId = member.MemberId,
                FirstName = member.FirstName.Trim(),
                LastName = member.LastName.Trim(),
                MemeberName = (member.LastName + " " + member.FirstName).Trim(),
                NickName =member.Nickname.Trim(),
                Gender = member.Gender == (int)GenderEnum.Male ? "男" : "女",
                Birthday = member.Birthday?.ToString("yyyy-MM-dd") ?? "Unknown",
                Phone = member.Phone.Trim(),
                Email = member.Email.Trim(),
                Cdate = member.Cdate.ToString("yyyy-MM-dd"),
                NationId = member.NationId,
                NationName = nation != null ? nation.NationName : "Unknown",
                Totalresult = member.IsEmailConfirmed == false && member.LineUserId == null ? "已停權" : "授權中"
            };

            return allmemberdata.ToList();
        }
        public async Task<MemberDataCountDto> GetMemberInformation()
        {
            var memberdatainfo = await _memberRepository.ListAsync();


            var memberCount = memberdatainfo.Count;
            var blockAccess = memberdatainfo.Count(x => x.IsEmailConfirmed == false);
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var currentMonth = DateTime.Now.ToString("yyyy/MM");
            var rateblockandnormal = Math.Round(((double)blockAccess / memberCount) * 100, 1);
            var monthlyNewMemberCount = memberdatainfo.Count(x => x.Cdate >= firstDayOfMonth);
            


            var membercountDto = new MemberDataCountDto
            {
                MemberCount = memberCount,
                MonthlyNewMemberCount = monthlyNewMemberCount,
                CurrentMonth = currentMonth,
                BlockAccessCount = blockAccess,
                Rateblockandnormal = rateblockandnormal,
            };

            return membercountDto;
        }
        public async Task<bool> UpdateMemberData(MemberDataDto memberDto)
        {
            var member = await _memberRepository.GetByIdAsync(memberDto.MemberId);
            if (member == null)
            {
                return false;
            }
            member.Nickname = memberDto.NickName;
            member.FirstName = memberDto.FirstName;
            member.LastName = memberDto.LastName;
            member.NationId = memberDto.NationId;
            if (memberDto.Gender == "男")
            {
                member.Gender = (short)GenderEnum.Male; 
            }
            else if(memberDto.Gender == "女")
            {
                member.Gender = (short)GenderEnum.Female; 
            }
            member.Birthday = DateTime.Parse(memberDto.Birthday);
            member.Phone = memberDto.Phone;
            member.Email = memberDto.Email;
            member.Cdate = DateTime.Parse(memberDto.Cdate); ;

            // 保存變更
            await _memberRepository.UpdateAsync(member);

            return true; 
        }

        public async Task<bool> LockMemberAccess(int memberId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
            {
                return false;
            }
            member.IsEmailConfirmed = false;

            await _memberRepository.UpdateAsync(member);
            return true;
        }
        public async Task<List<TutorDataDto>> GetTutorDataList()
        {
            var memberdatainfo = await _memberRepository.ListAsync();
            var applyListinfo = await _applyListRepository.ListAsync();

            var allTutordata =
            from member in memberdatainfo
            join applyList in applyListinfo on member.MemberId equals applyList.MemberId into tutorinfo
            from applyList in tutorinfo.DefaultIfEmpty()
            select new TutorDataDto
            {
                MemberId = member.MemberId,
                FirstName = member.FirstName,
                LastName = member.LastName,
                MemberName = member.LastName + " " + member.FirstName,
                ApplyDateTime = applyList != null ? applyList.ApplyDateTime.ToString("yyyy-MM-dd") : "N/A",
                RejectReason = applyList?.RejectReason ?? "無",
                ApplyStatus = applyList?.ApplyStatus ?? false,
                ApprovedDateTime = applyList?.ApprovedDateTime?.ToString("yyyy-MM-dd") ?? "N/A",
                Istutor = member.IsTutor ? "已成為教師" : "尚未成為教師",
                RequestAI = applyList != null && applyList.AiimgageStatus == true ? "需要" : "無需求",
                ResumeStatus = applyList != null && applyList.ApplyStatus
                                ? resumeStatus.通過申請.ToString()  
                                : applyList?.RejectReason != null
                                    ? resumeStatus.駁回申請.ToString()
                                    : resumeStatus.未審核.ToString()
            };

            return allTutordata.ToList();
        }

        
        public async Task<(bool IsSuccess, string Message)> ProcessTutorApplicationAsync(UpdateTutorDataDto tutorDto)
        {
            var memberApplyData =
                from member in await _memberRepository.ListAsync(m => m.MemberId == tutorDto.MemberId)
                join applyList in await _applyListRepository.ListAsync(a => a.MemberId == tutorDto.MemberId)
                on member.MemberId equals applyList.MemberId
                select new { Member = member, ApplyList = applyList };

            var memberToUpdate = memberApplyData.FirstOrDefault();
            if (memberToUpdate == null)
            {
                return (false, "未找到該會員或申請記錄。");
            }

            // 更新 IsTutor 和 ApplyStatus
            memberToUpdate.Member.IsTutor = tutorDto.Istutor;
            memberToUpdate.ApplyList.ApplyStatus = tutorDto.ApplyStatus;
            memberToUpdate.ApplyList.RejectReason = tutorDto.RejectReason;

            // 更新 ApprovedDateTime，只有當 ApplyStatus 為 true 時才更新
            if (tutorDto.ApplyStatus)
                if (DateTime.TryParse(tutorDto.ApprovedDateTime, out var parsedDateTime))
                {
                    memberToUpdate.ApplyList.ApprovedDateTime = parsedDateTime;
                }
                else
                {
                memberToUpdate.ApplyList.ApprovedDateTime = null; 
                }

            // 保存變更
            await _memberRepository.UpdateAsync(memberToUpdate.Member);
            await _applyListRepository.UpdateAsync(memberToUpdate.ApplyList);

            return (true, "會員資料已成功更新。");
        }

        public async Task<(bool IsSuccess, string Message)> ProcessTutorRejectAsync(UpdateTutorDataDto tutorDto)
        {
            var memberApplyData =
                from member in await _memberRepository.ListAsync(m => m.MemberId == tutorDto.MemberId)
                join applyList in await _applyListRepository.ListAsync(a => a.MemberId == tutorDto.MemberId)
                on member.MemberId equals applyList.MemberId
                select new { Member = member, ApplyList = applyList };

            var memberToUpdate = memberApplyData.FirstOrDefault();
            if (memberToUpdate == null)
            {
                return (false, "未找到該會員或申請記錄。");
            }

            // 更新 IsTutor 和 ApplyStatus
            memberToUpdate.Member.IsTutor = tutorDto.Istutor;
            memberToUpdate.ApplyList.ApplyStatus = tutorDto.ApplyStatus;
            memberToUpdate.ApplyList.RejectReason = tutorDto.RejectReason;

            // 更新 ApprovedDateTime，只有當 ApplyStatus 為 true 時才更新
            if (!tutorDto.ApplyStatus && DateTime.TryParse(tutorDto.ApprovedDateTime, out var parsedDateTime))
            {
             memberToUpdate.ApplyList.ApprovedDateTime = null;
            }
            
                

            // 保存變更
            await _memberRepository.UpdateAsync(memberToUpdate.Member);
            await _applyListRepository.UpdateAsync(memberToUpdate.ApplyList);

            return (true, "會員資料已成功更新。");
        }
        public async Task<TutorDataStatisticsDto> GetTutorDataInformation()
        {
            var tutorDataInfo = await _applyListRepository.ListAsync();
            var memberDataInfo = await _memberRepository.ListAsync();

            var memberCount = memberDataInfo.Count;
            var isTutorCount = memberDataInfo.Count(x => x.IsTutor);
            var applyCount = tutorDataInfo.Count(x => x.ApplyDateTime != null);
            var pendingReviewCount = tutorDataInfo.Count(x => x.RejectReason != null);

            //從這個月開始算起
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var currentMonth = DateTime.Now.ToString("yyyy/MM");

            // 計算當月的數據
            var monthlyApplyCount = tutorDataInfo.Count(x => x.ApplyDateTime != null && x.ApplyDateTime >= firstDayOfMonth);
            var monthlyPendingReviewCount = tutorDataInfo.Count(x => x.RejectReason != null && x.ApplyDateTime >= firstDayOfMonth);
            var monthlyIsTutorCount = tutorDataInfo.Count(x => x.ApprovedDateTime != null && x.ApprovedDateTime >= firstDayOfMonth); 
            var monthlyNewMemberCount = memberDataInfo.Count(x => x.Cdate >= firstDayOfMonth); 


            var statisticsDto = new TutorDataStatisticsDto
            {
                MemberCount = memberCount,
                IsTutorCount = isTutorCount,
                ApplyCount = applyCount,
                PendingReviewCount = pendingReviewCount,
                MonthlyApplyCount = monthlyApplyCount, 
                MonthlyPendingReviewCount = monthlyPendingReviewCount, 
                MonthlyIsTutorCount = monthlyIsTutorCount, 
                MonthlyNewMemberCount = monthlyNewMemberCount,
                CurrentMonth = currentMonth
            };

            return statisticsDto;
        }
    }
    public enum GenderEnum
    {
        Male = 1,  // 男
        Female = 2 // 女
    }
    public enum resumeStatus
    {
        未審核 = 0,
        通過申請 = 1,
        駁回申請 = 2,
    }
}
