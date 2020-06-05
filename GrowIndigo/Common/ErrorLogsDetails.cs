using GrowIndigo.Data;
using Rest;
using RestSharp;
using System;
using System.Data.Entity;
using System.IO;
using System.Net;

namespace GrowIndigo.Common
{
    public class CommonClasses
    {
        GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities();

        /// <summary>
        /// For inserting exceptions in the ErrorLogs table
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="controllerName"></param>
        /// <param name="methodName"></param>
        public void InsertExceptionDetails(Exception ex, string controllerName, string methodName)
        {
            ErrorLogs objErrorLogs = new ErrorLogs();
            objErrorLogs.CreatedDate = DateTime.Now;
            objErrorLogs.ErrorFrom = "ControllerName: " + controllerName + ", MethodName: " + methodName + "";
            objErrorLogs.ErrorMessage = ex.Message;
            objErrorLogs.InnerException = (!string.IsNullOrEmpty(Convert.ToString(ex.InnerException)) ? ex.InnerException.ToString()  : "" );
            dbContext.ErrorLogs.Add(objErrorLogs);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// For generating random 4 digit number OTP for user
        /// </summary>
        /// <returns></returns>
        public string GenerateOTP()
        {
            string strrandom = string.Empty;
            try
            {
                string numbers = "0123456789";
                Random objrandom = new Random();
                for (int i = 0; i < 4; i++)
                {
                    int temp = objrandom.Next(0, numbers.Length);
                    strrandom += temp;
                }
            }
            catch (Exception ex)
            {
                InsertExceptionDetails(ex, "UserController", "GenerateOTP");
                throw;
            }
            return strrandom;
        }

        /// <summary>
        /// For sending 6 digit generated OTP via MSG91
        /// </summary>
        /// <param name="otp"></param>
        /// <param name="mobileNumber"></param>
        /// <returns></returns>
        public bool SendOTP(string otp, string mobileNumber)
        {
            try
            {
                string message = " Your OTP Code is : " + otp + " elV9POcPbDI&sender=GROWIN&mobile=" + mobileNumber + "&otp=" + otp;
                string strUrl = "http://control.msg91.com/api/sendotp.php?authkey=326613AM8WQB495e9d47b5P1&message=%3C%23%3E" + message;

                WebRequest request = HttpWebRequest.Create(strUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream s = (Stream)response.GetResponseStream();

                StreamReader readStream = new StreamReader(s);
                string dataString = readStream.ReadToEnd();
                response.Close();
                s.Close();
                readStream.Close();

                return true;
            }
            catch (Exception ex)
            {
                InsertExceptionDetails(ex, "UserController", "SendOTP");
                return false;
            }
        }


        /// <summary>
        /// For sending SMS via MSG91
        /// </summary>
        /// <param name="mobileNumber"></param>
        /// <returns></returns>
        public bool SendSMS(string mobileNumber, string message)
        {
            try
            {
                //string message = "Your Order Is received";
                var client = new RestSharp.RestClient("https://api.msg91.com/api/v2/sendsms");
                var request = new RestSharp.RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authkey", "236638A2qjFh8bbBMv5b9607d8");
                request.AddParameter("application/json", "{ \"sender\": \"SOCKET\", \"route\": \"4\", \"country\": \"91\", \"sms\": [ { \"message\": \"" + message +  "\", \"to\": [ \"" + mobileNumber + "\" ]}]}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

             

                return true;
            }
            catch (Exception ex)
            {
                InsertExceptionDetails(ex, "UserController", "SendSMS");
                return false;
            }
        }




    }
}