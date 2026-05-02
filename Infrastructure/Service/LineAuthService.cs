using ApplicationCore.Dtos;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class LineAuthService : ILineAuthService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public LineAuthService(IConfiguration configuration)
        {
            _clientId = configuration["LINE-Login-Setting:Channel_ID"];
            _clientSecret = configuration["LINE-Login-Setting:Channel_Secret"];
            _redirectUri = configuration["LINE-Login-Setting:CallbackURL"];
        }

       

        public async Task<LineTokenResponseDto> GetAccessTokenAsync(string code)
        {
            using (var httpClient = new HttpClient())
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _redirectUri),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret)
        });

                var response = await httpClient.PostAsync("https://api.line.me/oauth2/v2.1/token", requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<LineTokenResponseDto>(responseContent);
                    return tokenResponse; // 這裡直接回傳整個 tokenResponse 物件，因為你需要 id_token 來獲取 email
                }
                else
                {
                    throw new Exception($"Failed to retrieve access token. Response status code: {response.StatusCode}, Response content: {responseContent}");
                }
            }
        }

        public async Task<LineUserProfileDto> GetUserProfileAsync(string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.GetAsync("https://api.line.me/v2/profile");

                if (response.IsSuccessStatusCode)
                {
                    var profileContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LineUserProfileDto>(profileContent);
                }
                else
                {
                    throw new Exception("Failed to retrieve user profile");
                }
            }
        }

        public async Task<string> GetEmailFromIdTokenAsync(string idToken)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(idToken);
            var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            if (!string.IsNullOrEmpty(emailClaim))
            {
                return emailClaim;
            }
            else
            {
                throw new Exception("Email not found in id_token.");
            }
        }

    }

}
