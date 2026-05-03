using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.CodeDom.Compiler;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Forum.Infrastructure.Services
{
    public class TokenService
    {
        private readonly string secret = "something";
        public string Email { get; set; }

        public TokenService(string email)
        {
            Email = email;
        }

        public string GenerateJwt()
        {
            string header = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT"});

            string payload = JsonSerializer.Serialize(new JwtPayload
            {
                sub = Email,
                iss = "forum.com",
                aud = "api.myapp.com",
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            });

            string signature = CreateSignature(Base64UrlEncode(header) + "." + Base64UrlEncode(payload));

            return $"{Base64UrlEncode(header)}.{Base64UrlEncode(payload)}.{Base64UrlEncode(signature)}";
        }

        public bool Validate (string input)
        {
            //Structure: Ensuring the token has the standard three parts(header, payload, signature) separated by dots.
            string[] parts = input.Split('.');

            if (parts.Length != 3) 
                return false;

            //Format: Verifying that each part is correctly encoded (Base64URL) and that the payload contains expected claims.
            foreach (string part in parts) 
            {
                var base64String = Convert.ToBase64String(Base64UrlDecode(part));
                if(!IsBase64UrlString(base64String))
                    return false;
            }

            JwtPayload jwtPayload = JsonSerializer.Deserialize<JwtPayload>(Base64UrlDecode(parts[1]));

            if (jwtPayload == null) return false;

            // works for any class, no matter how many properties you add or remove
            var properties = typeof(JwtPayload).GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(jwtPayload);
                if (value == null) return false;

                // also check for default values like 0 for long
                if (value is long l && l == 0) return false;
                if (value is string s && string.IsNullOrEmpty(s)) return false;
            }

            //Content: Checking if the claims within the payload are correct, such as expiration time (exp),
            //issued at (iat), not before (nbf), among others, to ensure the token isn't expired, isn't used before its time, etc.

            if(jwtPayload.exp <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return false;

            if(jwtPayload.iat > DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return false;

            return true;
        }

        public bool Verify(string input)
        {
            string[] parts = input.Split('.');
            //Signature Verification: This is the primary aspect of verification where the signature part of the
            //JWT is checked against the header and payload. This is done using the algorithm specified
            //in the header(like HMAC, RSA, or ECDSA) with a secret key or public key.If the signature doesn't match what's expected,
            //the token might have been tampered with or is not from a trusted source.
            string jwtReSignature = CreateSignature($"{parts[0]}.{parts[1]}");

            if (Base64UrlEncode(jwtReSignature) != parts[2]) return false;

            //Issuer Verification: Checking if the iss claim matches an expected issuer.
            JwtPayload jwtPayload = JsonSerializer.Deserialize<JwtPayload>(Base64UrlDecode(parts[1]));

            if (jwtPayload.iss != "forum.com") return false;

            //Audience Check: Ensuring the aud claim matches the expected audience.
            if (jwtPayload.aud != "api.myapp.com") return false;

            return true;
        }
        private string CreateSignature (string input)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(input);

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                return Encoding.UTF8.GetString(hashBytes);
            };
        }
        
        private string Base64UrlEncode(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            return Convert.ToBase64String(bytes)
            .Replace('+', '-') // 62nd char of encoding
            .Replace('/', '_') // 63rd char of encoding
            .Replace("=", ""); // Remove any trailing '='s;
        }

        private byte [] Base64UrlDecode(string input)
        {
            input = input
                .Replace('-', '+') 
                .Replace('_', '/');

            input = (input.Length % 4) switch
            {
                0 => input,
                2 => input + "==",
                3 => input + "=",
                _ => throw new Exception("Illegal base64url string!")
            };

            return (Convert.FromBase64String(input));
        }

        public Boolean IsBase64UrlString(string value)
        {
            if (value == null || value.Length == 0 || value.Length % 4 != 0
                || value.Contains(' ') || value.Contains('\t') || value.Contains('\r') || value.Contains('\n'))
                return false;
            var index = value.Length - 1;
            if (value[index] == '=')
                index--;
            if (value[index] == '=')
                index--;
            for (var i = 0; i <= index; i++)
                if (IsInvalid(value[i]))
                    return false;
            return true;
        }
        private Boolean IsInvalid(char value)
        {
            var intValue = (Int32)value;

            // 1 - 9
            if (intValue >= 48 && intValue <= 57)
                return false;

            // A - Z
            if (intValue >= 65 && intValue <= 90)
                return false;

            // a - z
            if (intValue >= 97 && intValue <= 122)
                return false;

            // + or /
            return intValue != 43 && intValue != 47;
        }
    }
}
