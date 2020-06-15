using log4net;
using System.Configuration;

namespace BetfairApi
{
    public class SessionTokenProvider : ISessionTokenProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonRpcClient));

        private readonly AuthClient _authClient;

        private readonly string _certPath;

        public SessionTokenProvider(string appKey, string certPath)
        {
            this._authClient = new AuthClient(appKey);
            this._certPath = string.IsNullOrEmpty(certPath) ? "client-2048.p12" : certPath;
        }

        public string GetToken()
        {
            var resp = this._authClient.DoLogin("99williamsdav", "12178559d", _certPath);
            var msg = $"Login status: {resp.LoginStatus}";
            if (resp.LoginStatus == "SUCCESS")
                Log.Debug(msg);
            else
                Log.Error(msg);
            return resp.SessionToken;
        }
    }
}
