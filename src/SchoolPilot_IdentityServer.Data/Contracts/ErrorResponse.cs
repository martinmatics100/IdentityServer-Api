

namespace SchoolPilot_IdentityServer.Data.Contracts
{
    public class ErrorResponse
    {
        public string Title { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
