namespace Forum.Application.UseCases.Login
{
    public class LoginInput
    {
        public string Username { get; init; }
        public string Password { get; init; }

        public LoginInput(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
