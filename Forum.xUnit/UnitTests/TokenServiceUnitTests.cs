using Forum.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forum.xUnit.UnitTests
{
    public class TokenServiceUnitTests
    {
        [Fact]
        public void ValidateShouldReturnTrue() { 
            TokenService jwtTokenService = new TokenService("some@email.com");

            string jwtToken = jwtTokenService.GenerateJwt();

            Assert.True(jwtTokenService.Validate(jwtToken));
        }


        [Fact]
        public void ValidateShouldReturnFalse()
        {
            TokenService jwtTokenService = new TokenService("some@email.com");

            string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";

            Assert.False(jwtTokenService.Validate(jwtToken));
        }

        [Fact]
        public void VerifyShouldReturnTrue()
        {
            TokenService jwtTokenService = new TokenService("some@email.com");

            string jwtToken = jwtTokenService.GenerateJwt();

            Assert.True(jwtTokenService.Verify(jwtToken));
        }

        [Fact]
        public void VerifyShouldReturnFalse()
        {
            TokenService jwtTokenService = new TokenService("some@email.com");

            string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";

            Assert.False(jwtTokenService.Validate(jwtToken));
        }
    }
}
