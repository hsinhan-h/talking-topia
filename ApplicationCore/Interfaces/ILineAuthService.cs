using ApplicationCore.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface ILineAuthService
    {
        Task<LineTokenResponseDto> GetAccessTokenAsync(string code);
        Task<LineUserProfileDto> GetUserProfileAsync(string accessToken);
        Task<string> GetEmailFromIdTokenAsync(string idToken); // 新增這個方法
    }
}
