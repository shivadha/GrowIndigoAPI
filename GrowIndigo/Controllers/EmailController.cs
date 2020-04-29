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
                    subject = "Grow Mandi App : Client Transaction Details ";

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
                // mail.To.Add(emailTo);
                mail.To.Add("shital.khairnar@growindigo.co.in");
                mail.To.Add("arjun.jagtap@growindigo.co.in");
                //mail.To.Add("madhur.jain@growindigo.co.in");
                mail.To.Add("Shivam.Dhagat1@gmail.com");
                mail.Subject = subject;
                mail.Body = body;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("developer@growindigo.co.in", "lraoezrpruvcsrvy");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                if (type == "Enquiry")
                {

                    return null;
                }
                else
                {
                    objResponse.Message = "Thank you for showing interest in this "+ objEmailModel.CropName + " Product. Our support Team will be in touch with you as soon as poosible ";
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
    }
}
