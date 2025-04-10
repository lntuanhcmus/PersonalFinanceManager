using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Services
{
    public class LocalServerCodeReceiver : ICodeReceiver
    {
        private readonly int _port;
        private readonly string _redirectUri;
        private readonly Google.Apis.Auth.OAuth2.LocalServerCodeReceiver _receiver;

        public LocalServerCodeReceiver(int port)
        {
            _port = port;
            _redirectUri = $"http://localhost:{port}/oauth/callback";
            _receiver = new Google.Apis.Auth.OAuth2.LocalServerCodeReceiver(); // Khởi tạo instance
        }

        public string RedirectUri => _redirectUri;

        public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl request, CancellationToken taskCancellationToken)
        {
            var response = _receiver.ReceiveCodeAsync(request, taskCancellationToken); // Gọi trên instance
            return response;
        }
    }
}