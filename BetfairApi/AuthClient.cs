using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;

namespace BetfairApi
{
    /// <summary>
    /// https://github.com/betfair/API-NG-sample-code/tree/master/loginCode/Non-interactive-cSharp
    /// </summary>
    public class AuthClient
    {
        private string appKey;

        public string AppKey
        {
            get { return appKey; }
        }

        private HttpClientHandler getWebRequestHandlerWithCert(string certFilename)
        {
            var cert = new X509Certificate2(certFilename, "");
            var clientHandler = new HttpClientHandler();
            clientHandler.ClientCertificates.Add(cert);
            return clientHandler;
        }

        private const string DEFAULT_COM_BASEURL = "https://identitysso-api.betfair.com";

        private HttpClient InitHttpClientInstance(HttpClientHandler clientHandler, string appKey, string baseUrl = DEFAULT_COM_BASEURL)
        {
            var client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("X-Application", appKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private FormUrlEncodedContent GetLoginBodyAsContent(string username, string password)
        {
            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("username", username));
            postData.Add(new KeyValuePair<string, string>("password", password));
            return new FormUrlEncodedContent(postData);
        }

        public LoginResponse DoLogin(string username, string password, string certFilename)
        {
            var handler = getWebRequestHandlerWithCert(certFilename);
            var client = InitHttpClientInstance(handler, appKey);
            var content = GetLoginBodyAsContent(username, password);
            var result = client.PostAsync("/api/certlogin", content).Result;
            result.EnsureSuccessStatusCode();
            var jsonSerialiser = new DataContractJsonSerializer(typeof(LoginResponse));
            var stream = new MemoryStream(result.Content.ReadAsByteArrayAsync().Result);
            return (LoginResponse)jsonSerialiser.ReadObject(stream);

        }

        public AuthClient(string appKey)
        {
            this.appKey = appKey;
        }
    }

    [DataContract]
    public class LoginResponse
    {
        [DataMember(Name = "sessionToken")]
        public string SessionToken { get; set; }
        [DataMember(Name = "loginStatus")]
        public string LoginStatus { get; set; }
    }
}
