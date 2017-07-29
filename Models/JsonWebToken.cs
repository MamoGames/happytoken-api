namespace HappyTokenApi.Models
{
    public class JsonWebToken
    {
        public string AccessToken { get; set; }

        public int ExpiresInSecs { get; set; }
    }
}
