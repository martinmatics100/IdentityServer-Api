using SchoolPilot_IdentityServer.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolPilot_IdentityServer.Data.Contracts
{
    public class UserRegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

    }


    public class UserResponse
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public UserRole Role { get; set; } 


    }

    public class UserLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class CurrentUserResponse
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string AccessToken { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public UserRole Role { get; set; } // Add this
    }

    public class AccessTokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public UserRole Role { get; set; } // Add this
        public DateTime ExpiresIn { get; set; }
    }


    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public Gender Gender { get; set; }
    }


    public class RevokeRefreshTokenResponse
    {
        public string Message { get; set; }
    }


    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }

}
