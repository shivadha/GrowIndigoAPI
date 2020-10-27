using GrowIndigo.Common;
using GrowIndigo.Data;
using GrowIndigo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace GrowIndigo.Controllers
{
    public class EmailController : ApiController
    {
        GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities();
        SuccessResponse objResponse = new SuccessResponse();
        CommonClasses objCommonClasses = new CommonClasses();
        MandiUserController objMandiUserController = new MandiUserController();





        //public void sendEmailViaWebApi(string recipient, string subject, string message)
        [Route("api/Email/sendEmailViaWebApi")]
        public HttpResponseMessage sendEmailViaWebApi(EmailModel objEmailModel, string type = "")
        {
            try
            {
                string FromMail = "";
                string subject = "";
                string body = "";
                if (type == "OrderDetail")
                {
                    body = " Hello Admin,\r\n";
                    body += " Details of deal are as follows \r\n";
                    body += "1.Order Id: " + objEmailModel.orderId + "\r\n";
                    body += "2.Product Id: " + objEmailModel.ProductId + "\r\n";
                    body += "3.CropName : " + objEmailModel.CropName + "\r\n";
                    body += "4.Quantity : " + objEmailModel.Qty + "\r\n";
                    body += "5.Buyer Name: " + objEmailModel.BuyerName + "\r\n";
                    body += "6.Buyer Contact: " + objEmailModel.BuyerContact + "\r\n";
                    body += "7.Price: " + objEmailModel.Price + "\r\n";
                    body += "8.Service Tax: " + objEmailModel.ServiceTax + "\r\n";
                    body += "9.TotalAmount: " + objEmailModel.TotalAmount + "\r\n";
                    body += "10.Payment Status" + objEmailModel.PaymentStatus + "\r\n";
                    body += "\r\n";
                    body += "warm regards";


                    FromMail = "developer@growindigo.co.in";
                    subject = "zGrow Mandi App : Client Transaction Details ";

                }
                else if (type == "AddProduct")
                {
                    body = " Hello Admin,\r\n";
                    body += " User has added new product. Details of particular Product are as follows \r\n";
                    body += "1.Product Id: " + objEmailModel.ProductId + "\r\n";
                    body += "2.Crop Id: " + objEmailModel.CropId + "\r\n";
                    body += "3.Category Name: " + objEmailModel.CategoryName + "\r\n";
                    body += "4.Variety Id : " + objEmailModel.VarietyId + "\r\n";
                    body += "5.Product Address : " + objEmailModel.ProductAddress + "\r\n";
                    body += "6.Geo Address: " + objEmailModel.GeoAddress + "\r\n";
                    body += "7.Mobile Number: " + objEmailModel.MobileNumber + "\r\n";
                    body += "8.Quantity: " + objEmailModel.Quantity + "\r\n";
                    body += "9.Quantity Unit: " + objEmailModel.QuantityUnit + "\r\n";
                    body += "10.Price: " + objEmailModel.Price + "\r\n";
                    body += "11.State" + objEmailModel.State + "\r\n";
                    body += "12.District" + objEmailModel.District + "\r\n";
                    body += "13.Taluka" + objEmailModel.Taluka + "\r\n";
                    body += "\r\n";
                    body += "warm regards";


                    FromMail = "developer@growindigo.co.in";
                    subject = "Grow Mandi App: New Product Detail ";
                }
                else if (type == "UserRequirement")
                {


                    body = " Hello Admin,\r\n";
                    body += " There is new user requirement . Details of particular Product are as follows \r\n";
                    body += "1.BuyerId: " + objEmailModel.BuyerId + "\r\n";
                    body += "2.BuyerAddress: " + objEmailModel.BuyerAddress + "\r\n";
                    body += "3.CropName : " + objEmailModel.CropName + "\r\n";
                    body += "4.VarietyName: " + objEmailModel.VarietyName + "\r\n";
                    body += "5.Quantity: " + objEmailModel.Quantity + "\r\n";
                    body += "6.QualitySpecification: " + objEmailModel.QualitySpecification + "\r\n";
                    body += "7.DeliveryLocation: " + objEmailModel.DeliveryLocation + "\r\n";
                    body += "8.ExpectedPrice: " + objEmailModel.ExpectedPrice + "\r\n";
                    body += "9.ExpectedDate: " + objEmailModel.ExpectedDate + "\r\n";
                    body += "10.IsPriceNegotiable" + objEmailModel.IsPriceNegotiable + "\r\n";
                    body += "11.Remarks" + objEmailModel.Remarks + "\r\n";

                    body += "\r\n";
                    body += "warm regards";


                    FromMail = "developer@growindigo.co.in";
                    subject = "Grow Mandi App: New UserRequirement Detail ";

                    InterestedProductsViewModel objInterestedProductsViewModel = new InterestedProductsViewModel();
                    objInterestedProductsViewModel.Fk_MobileNumber = objEmailModel.BuyerId;
                    objInterestedProductsViewModel.BuyerId = objEmailModel.BuyerId;
                    objInterestedProductsViewModel.Tr_Id = objEmailModel.Tr_Id;
                    objInterestedProductsViewModel.ProductId = objEmailModel.ProductId;
                    objInterestedProductsViewModel.CreatedDate = DateTime.Now;
                    objInterestedProductsViewModel.BuyerAddress = objEmailModel.BuyerAddress;
                    objInterestedProductsViewModel.CropName = objEmailModel.CropName;
                    objInterestedProductsViewModel.VarietyName = objEmailModel.VarietyName;
                    objInterestedProductsViewModel.Quantity = objEmailModel.Quantity;
                    objInterestedProductsViewModel.QualitySpecification = objEmailModel.QualitySpecification;
                    objInterestedProductsViewModel.DeliveryLocation = objEmailModel.DeliveryLocation;
                    objInterestedProductsViewModel.ExpectedPrice = objEmailModel.ExpectedPrice;
                    objInterestedProductsViewModel.IsPriceNegotiable = objEmailModel.IsPriceNegotiable.ToString();
                    objInterestedProductsViewModel.Remarks = objEmailModel.Remarks;

                    MailMessage maile = new MailMessage();
                    SmtpClient SmtpServere = new SmtpClient("smtp.gmail.com");
                    maile.From = new MailAddress(FromMail);

                    //for test
                    // mail.To.Add("arjun.jagtap@growindigo.co.in");
                    //mail.To.Add("rahul.dhande@growindigo.co.in");
                    // mail.To.Add("mandi@growindigo.co.in");

                    //for live

                    //mail.To.Add("arjun.jagtap@growindigo.co.in");
                    //mail.To.Add("shital.khairnar@growindigo.co.in");
                    //mail.To.Add("madhur.jain@growindigo.co.in");

                    //for Developer
                    maile.To.Add("Shivam.Dhagat@systematixindia.com");
                    maile.To.Add("Ashish.Agrawal@systematixindia.com");
                    maile.To.Add("Shivamdhagat1@gmail.com");


                    maile.Subject = subject;
                    maile.Body = body;
                    SmtpServere.Port = 587;
                    SmtpServere.Credentials = new System.Net.NetworkCredential("developer@growindigo.co.in", "phoansnuhfutodwq");
                    SmtpServere.EnableSsl = true;
                    SmtpServere.Send(maile);
                    var addIntersProdct = objMandiUserController.AddInterestedProductForUser(objInterestedProductsViewModel);
                    if (addIntersProdct !=null)
                    {
                        //for sending notification to seller
                        //get seller detail by productId

                        objResponse.Message = "Thank you for showing interest in this " + objEmailModel.CropName + " Product. Our support Team will be in touch with you as soon as poosible ";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                    }
                    else
                    {
                        objResponse.Message = "Thank you for showing interest in this " + objEmailModel.CropName + " Product. Our support Team will be in touch with you as soon as poosible ";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    #region Get  Seller Details based on productId


                    int product_Id = Convert.ToInt32(objEmailModel.ProductId);
                    var getSellerMobileNumber = (from product in dbContext.Mandi_ProductMaster where product.Tr_Id == product_Id select product.MobileNumber).FirstOrDefault();
                    if (getSellerMobileNumber != null)
                    {
                        var getSellerdetails = (from mobile in dbContext.Mandi_UserInfo where mobile.MobileNumber == getSellerMobileNumber select mobile).FirstOrDefault();

                        body = " Dear Admin,\r\n";

                        body += "A buyer has shown an interest for purchasing the product on Grow Mandi.\r\n";
                        body += "Following are the details:\r\n";


                        body += "1.Buyer ID: " + objEmailModel.BuyerContact + "\r\n";
                        body += "2.Buyer Name: " + objEmailModel.BuyerName + "\r\n";
                        body += "3.Buyer Contact Number : " + objEmailModel.BuyerContact + "\r\n";
                        body += "4.Buyer Address: " + objEmailModel.BuyerAddress + "\r\n";

                        body += "5.Product ID: " + objEmailModel.ProductId + "\r\n";
                        body += "6.Product Name: " + objEmailModel.CropName + "\r\n";
                        body += "7.Quantity: " + objEmailModel.Qty + "\r\n";
                        body += "8.Rate: " + objEmailModel.Rate + "\r\n";


                        //body += "9.Seller Id: " + objEmailModel.SellerId + "\r\n";
                        body += "9.Seller Name:" + getSellerdetails.FullName + "\r\n";
                        body += "10.Seller Contact Number:" + getSellerdetails.MobileNumber + "\r\n";
                        body += "11.Seller Address" + getSellerdetails.State + " ," + getSellerdetails.Taluka + "," + getSellerdetails.Pincode + "\r\n";
                        body += "\r\n";
                        body += "warm regards";



                        FromMail = "developer@growindigo.co.in";
                        if (type == "Enquiry")
                        {
                            subject = "Buying Enquiry_Product ID: " + objEmailModel.ProductId + "Buyer ID: " + objEmailModel.BuyerId + "";
                        }
                        else
                        {
                            subject = "Buying Interest_Product ID: " + objEmailModel.ProductId + "Buyer ID: " + objEmailModel.BuyerId + "";
                        }

                    }
                    #endregion
                }
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(FromMail);

                //for test
                // mail.To.Add("arjun.jagtap@growindigo.co.in");
                //mail.To.Add("rahul.dhande@growindigo.co.in");
                // mail.To.Add("mandi@growindigo.co.in");

                //for live

                //mail.To.Add("arjun.jagtap@growindigo.co.in");
                //mail.To.Add("shital.khairnar@growindigo.co.in");
                //mail.To.Add("madhur.jain@growindigo.co.in");

                //for Developer
                mail.To.Add("Shivam.Dhagat@systematixindia.com");
                mail.To.Add("Ashish.Agrawal@systematixindia.com");
                mail.To.Add("Shivamdhagat1@gmail.com");


                mail.Subject = subject;
                mail.Body = body;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("developer@growindigo.co.in", "phoansnuhfutodwq");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                if (type == "Enquiry")
                {

                    return null;
                }
                else if (type == "AddProduct")
                {

                    return null;
                }
                else if (type == "UserRequirement")
                {

                    return null;
                }
                else
                {
                    objResponse.Message = "Thank you for showing interest in this " + objEmailModel.CropName + " Product. Our support Team will be in touch with you as soon as poosible ";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "Email", "SendEmail");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
                throw ex;
            }

        }





        #region Notification 

        public string SendFCMNotificationToUsers(string DeviceToken, string Message, string Title)
        {
            string notificationjson = string.Empty;
            if (!string.IsNullOrEmpty(DeviceToken))
            {
                notificationjson = "{ \"to\": \"" + DeviceToken + "\" ,\"notification\":{\"type\":\"" + Title + "\",\"body\":\"" + Message + "\"}}";
            }

            string postData = notificationjson;
            try
            {
                //var applicationID = Config.FCMApplicationIDPremium;
                //var senderId = Config.FCMsenderIdPremium;

                //var applicationID = "AAAAVZqtFXE:APA91bETQj3U5yQVGBGdcu15njSk5y3cGDPww1Xg1GY-d_AjnH20haGP3QdUzm1-GZEbPemiXoTjogwDRWB5LE6Hh-f7N9Ks8JoAdBeZQqwZXcLFhsmC9uQhBJNjUklHFfmpA3Jc-r2v";
                //var senderId = "367667254641";

                //New com.mahyco.retail.growmandi 
                var applicationID = "AAAAHR1Rh10:APA91bFA0t70thUmOM3HwrX5oWd-dUI55yk_psJjbRCR0pAvSmjZKAPef1kIcKxaV6RKaL4NCd81sIS2OLcPPGfA7K6D53wz_cg7jnEGbsxKfRpWL8P2XxcQCY9Mzd6FC2pav4o2ZsSa";
                var senderId = "125045933917";



                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                tRequest.UseDefaultCredentials = true;
                tRequest.PreAuthenticate = true;
                tRequest.Credentials = CredentialCache.DefaultCredentials;
                //  tRequest.ContentType = "application/x-www-urlencoded";

                // var serializer = new JavaScriptSerializer();
                //var json = serializer.Serialize(data);
                // Byte[] byteArray = Encoding.UTF8.GetBytes(json);

                Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));
                tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
                tRequest.ContentLength = byteArray.Length;

                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);

                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                return sResponseFromServer;

                            }
                        }
                    }
                }
            }

            catch (WebException e)
            {
                //Log.Error(e.Message);
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                //Log.Error(ex.Message);
                Console.Write(ex.Message);
            }
            return "";
        }

        #endregion

    }
}
