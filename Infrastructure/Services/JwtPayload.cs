using Microsoft.AspNetCore.SignalR;

namespace Forum.Infrastructure.Services
{
    public class JwtPayload
    {
        public string sub { get; set; }
        public long exp { get; set; }
        public long iat { get; set; }

        public string iss { get; set; }
        public string aud { get; set; }
    }
}
