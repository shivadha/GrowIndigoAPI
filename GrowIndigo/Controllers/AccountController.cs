using GrowIndigo.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace GrowIndigo.Controllers
{ 
    public class AccountController : ApiController
    {
        public static string CONTENT_TYPE = @"application/x-www-form-urlencoded";
        public static string POST_METHOD = "POST";
        public static string GET_METHOD = "GET";
        public static string PUT_METHOD = "PUT";
        public static AccessTokenModel authToken;
        public static string physmodoAccessToken;

        /// <summary>
        /// Build request for generate token
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static AccessTokenModel GetToken(string userName, string password)
        {
            try
            {
                var tokenUrl = ConfigurationManager.AppSettings["WebAPIURL"] + "/token";
                var request = string.Format("grant_type=password&username={0}&password={1}", userName, password);
                Log.Info(request);
                authToken = HttpPost(tokenUrl, request);
                Log.Info(authToken.ToString());
                return authToken;
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                throw ex;
            }
           
        }

        /// <summary>
        /// Send request for generate token using Owin
        /// </summary>
        /// <param name="tokenUrl"></param>
        /// <param name="requestDetails"></param>
        /// <returns></returns>
        public static AccessTokenModel HttpPost(string tokenUrl, string requestDetails)
        {
            AccessTokenModel token = null;
            try
            {
                Log.Info(tokenUrl);
                Log.Info(requestDetails);
                WebRequest webRequest = WebRequest.Create(tokenUrl);
                webRequest.ContentType = CONTENT_TYPE;
                webRequest.Method = POST_METHOD;
                byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
                webRequest.ContentLength = bytes.Length;
                using (Stream outputStream = webRequest.GetRequestStream())
                {
                    outputStream.Write(bytes, 0, bytes.Length);
                }
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    StreamReader newstreamreader = new StreamReader(webResponse.GetResponseStream());
                    string newresponsefromserver = newstreamreader.ReadToEnd();
                    newresponsefromserver = newresponsefromserver.Replace(".expires", "expires").Replace(".issued", "issued");
                    token = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenModel>(newresponsefromserver);// new JavaScriptSerializer().Deserialize<AccessToken>(newresponsefromserver);
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                Console.WriteLine(ex.Message);
                token = null;
            }

            return token;
        }
    }

    public class AccessTokenModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string userName { get; set; }
        public string issued { get; set; }
        public string expires { get; set; }

    }

}