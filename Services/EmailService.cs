using RestSharp;
using RestSharp.Authenticators;

namespace ASP.NET_Project.Services
{
    public class EmailService
    {
        private readonly string _domain;
        private readonly string _apiKey;
        private readonly string _fromEmail;

        // Constructor to initialize EmailService with domain, API key, and from email
        public EmailService(string domain, string apiKey, string fromEmail)
        {
            _domain = domain;
            _apiKey = apiKey;
            _fromEmail = fromEmail;
        }

        public void SendOtpEmail(string email, string otp)
        {
            var options = new RestClientOptions($"https://api.mailgun.net/v3/{_domain}")
            {
                Authenticator = new HttpBasicAuthenticator("api", _apiKey)
            };
            var client = new RestClient(options);

            var request = new RestRequest("messages", Method.Post);
            request.AddParameter("from", _fromEmail);
            request.AddParameter("to", email);
            request.AddParameter("subject", "Your OTP Code");
            request.AddParameter("text", $"Your OTP code is: {otp}");

            client.ExecuteAsync(request);
        }
    }
}
