using GrowIndigo.Common;
using GrowIndigo.Data;
using GrowIndigo.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using static GrowIndigo.Models.MasterData;
using static GrowIndigo.Models.UsersAddressViewModel;

namespace GrowIndigo.Controllers
{
    //  [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        #region Dependencies

        GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities();
        Authentication auth = new Authentication();
        CommonClasses objCommonClasses = new CommonClasses();

        SuccessResponse objResponse = new SuccessResponse();

        #endregion


        #region Account GrowOnline

        /// <summary>
        /// For verifying mobile number of a exsisting user
        /// </summary>
        /// <param name="objUserMobileNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/User/VerifyMobileNo")]
        public HttpResponseMessage VerifyMobileNo(UserMobileNumber objUserMobileNumber)
        {
            try
            {
                string mobileNumber = objUserMobileNumber.MobileNumber;
                //For genrating OTP and saving it into UserOTPInfo table
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    var otpNumber = objCommonClasses.GenerateOTP();
                    if (objCommonClasses.SendOTP(otpNumber, mobileNumber))
                    {
                        //For inserting a new mobile number into UserOTPInfo table
                        UserOTPInfo objUserOTPInfo = new UserOTPInfo();
                        objUserOTPInfo.MobileNumber = mobileNumber;
                        objUserOTPInfo.OTP = otpNumber;
                        objUserOTPInfo.GenratedDate = DateTime.Now;

                        //var update = (from u in dbContext.UserOTPInfo where u.MobileNumber == mobileNumber select u).SingleOrDefault();
                        dbContext.UserOTPInfo.Add(objUserOTPInfo);
                        dbContext.SaveChanges();
                        objResponse.Message = "Success";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "please Resend OTP";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "Mobile Number Not Exist";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "VerifyMobileNo");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        /// <summary>
        /// For verifying the OTP sent from the user
        /// </summary>
        /// <param name="objOtpVerification"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/User/VerifyOTP")]
        public HttpResponseMessage VerifyOTP(OtpVerification objOtpVerification)
        {
            try
            {
                string mobileNumber = objOtpVerification.MobileNumber;
                string otp = objOtpVerification.Otp;

                var res = dbContext.UserOTPInfo.ToList();
                var otpDetails = (from user in dbContext.UserOTPInfo orderby user.GenratedDate descending where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (otpDetails != null && otpDetails.OTP == otp)
                {
                    Log.Info(otp);

                    var Otp = dbContext.UserOTPInfo.Where(x => x.MobileNumber == mobileNumber).OrderByDescending(x => x.GenratedDate).Select(x => x.OTP).FirstOrDefault();
                    Log.Info(Otp);
                    

                    var token = AccountController.GetToken(mobileNumber, Otp);

                    var Access_token = token.access_token;
                    var Token_type = token.token_type;
                    var Expires_in = token.expires_in;
                    objResponse.Message = "Success";
                    var response = Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    // Set headers for paging
                    response.Headers.Add("Token", token.access_token);
                    return response;
                }
                objResponse.Message = "Invalid OTP";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "VerifyOTP");
                objResponse.Message = "Unable to connect with server";
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, objResponse);
                //return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        /// <summary>
        /// For getting user details, after verfication of the OTP
        /// </summary>
        /// <param name="objUserVerification"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/User/GetUserInfo")]
        public HttpResponseMessage GetUserInfo(UserVerification objUserVerification)
        {
            try
            {
                string mobileNumber = objUserVerification.MobileNumber;

                var userDetails = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (userDetails != null)
                {
                    UserInfoViewModel objUserInfoViewModel = new UserInfoViewModel()
                    {
                        Type = userDetails.Type,
                        MobileNumber = userDetails.MobileNumber,
                        Name = userDetails.RetailerName,
                        EmailId = userDetails.EmailId,
                        State = userDetails.State,
                        District = userDetails.District,
                        Taluka = userDetails.Taluka,
                        Village = userDetails.Village,
                        Firm_Name = userDetails.Firm_Name,
                        IsProfileUpdated = userDetails.IsProfileUpdated,
                        RetailerId = userDetails.RetailerId
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, objUserInfoViewModel);
                }
                objResponse.Message = "User Not Exist";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "GetUserInfo");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        /// <summary>
        /// For registration of a new user.
        /// </summary>
        /// <param name="objUserViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/User/UserRegistration")]
        public HttpResponseMessage UserRegistration(UserInfoViewModel objUserViewModel)
        {
            try
            {
                UserInfo objUserInfo = new UserInfo();
                string mobileNumber = objUserViewModel.MobileNumber;
                //For genrating OTP and saving it into UserOTPInfo table
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    objResponse.Message = "Mobile number already exists.";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
                else
                {
                    objUserInfo.MobileNumber = objUserViewModel.MobileNumber;
                    objUserInfo.State = objUserViewModel.State;
                    objUserInfo.Firm_Name = objUserViewModel.Firm_Name;
                    objUserInfo.IsProfileUpdated = false;
                    objUserInfo.RetailerId = "12345";
                    dbContext.UserInfo.Add(objUserInfo);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "Registered Successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Registration Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "UserRegistration");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        /// <summary>
        /// For getting user details on the basis of mobile nummber.
        /// </summary>
        /// <param name="objUserVerification"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/User/GetProfileDetails")]
        public HttpResponseMessage GetProfileDetails(UserVerification objUserVerification)
        {
            try
            {
                string mobileNumber = objUserVerification.MobileNumber;

                UserMaster objUserMaster = new UserMaster();
                var profileDetails = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (profileDetails != null)
                {
                    UserProfile objUserProfile = new UserProfile()
                    {
                        FirstName = (!string.IsNullOrEmpty(profileDetails.RetailerName)) ? profileDetails.RetailerName : "",
                        LastName = (!string.IsNullOrEmpty(profileDetails.RetailerLastName)) ? profileDetails.RetailerLastName : "",
                        EmailId = (!string.IsNullOrEmpty(profileDetails.EmailId)) ? profileDetails.EmailId : "",
                        BirthDate = (!string.IsNullOrEmpty(Convert.ToString(profileDetails.BirthDate))) ? (profileDetails.BirthDate.Value.ToString("dd.MM.yyyy")) : "",
                        MobileNumber = (!string.IsNullOrEmpty(profileDetails.MobileNumber)) ? profileDetails.MobileNumber : "",
                        Address = (!string.IsNullOrEmpty(profileDetails.Address)) ? profileDetails.Address : "",
                        StreetLine1 = (!string.IsNullOrEmpty(profileDetails.StreetLine1)) ? profileDetails.StreetLine1 : "",
                        StreetLine2 = (!string.IsNullOrEmpty(profileDetails.StreetLine2)) ? profileDetails.StreetLine2 : "",
                        Pincode = (!string.IsNullOrEmpty(profileDetails.Pincode)) ? profileDetails.Pincode : "",
                        State = (!string.IsNullOrEmpty(profileDetails.State)) ? profileDetails.State : "",
                        District = (!string.IsNullOrEmpty(profileDetails.District)) ? profileDetails.District : "",
                        Taluka = (!string.IsNullOrEmpty(profileDetails.Taluka)) ? profileDetails.Taluka : "",
                        Town = (!string.IsNullOrEmpty(profileDetails.Town)) ? profileDetails.Town : "",
                        FirmName = (!string.IsNullOrEmpty(profileDetails.Firm_Name)) ? profileDetails.Firm_Name : "",
                        GstNumber = (!string.IsNullOrEmpty(profileDetails.GSTNumber)) ? profileDetails.GSTNumber : "",
                        PanNumber = (!string.IsNullOrEmpty(profileDetails.PANNumber)) ? profileDetails.PANNumber : "",
                        LicenseNumber = (!string.IsNullOrEmpty(profileDetails.LicenseNumber)) ? profileDetails.LicenseNumber : "",
                        LicenseValidity = (!string.IsNullOrEmpty(Convert.ToString(profileDetails.SeedLicenseValidity))) ? (profileDetails.SeedLicenseValidity.Value.ToString("dd.MM.yyyy")) : "",
                    };

                    if (!string.IsNullOrEmpty(objUserProfile.FirstName) && !string.IsNullOrEmpty(objUserProfile.LastName)
                        && !string.IsNullOrEmpty(objUserProfile.EmailId) && !string.IsNullOrEmpty(Convert.ToString(objUserProfile.BirthDate))
                        && !string.IsNullOrEmpty(objUserProfile.MobileNumber) && !string.IsNullOrEmpty(objUserProfile.Address)
                        && !string.IsNullOrEmpty(objUserProfile.StreetLine1) && !string.IsNullOrEmpty(objUserProfile.StreetLine2)
                        && !string.IsNullOrEmpty(objUserProfile.Pincode) && !string.IsNullOrEmpty(objUserProfile.State)
                        && !string.IsNullOrEmpty(objUserProfile.District) && !string.IsNullOrEmpty(objUserProfile.Taluka)
                        && !string.IsNullOrEmpty(objUserProfile.Town) && !string.IsNullOrEmpty(objUserProfile.FirmName)
                        && !string.IsNullOrEmpty(objUserProfile.GstNumber) && !string.IsNullOrEmpty(objUserProfile.PanNumber)
                        && !string.IsNullOrEmpty(objUserProfile.LicenseNumber) && !string.IsNullOrEmpty(objUserProfile.LicenseValidity))
                    {
                        objUserProfile.IsProfileUpdate = true;
                    }
                    else
                    {
                        objUserProfile.IsProfileUpdate = false;
                    }
                    objUserMaster.User = objUserProfile;
                    return Request.CreateResponse(HttpStatusCode.OK, objUserMaster);
                }
                objResponse.Message = "User Not found";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "GetProfileDetails");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        /// <summary>
        /// For uddating an exsisting user details on the basis of mobile nummber.
        /// </summary>
        /// <param name="objUserProfile"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/User/UpdateUserDetails")]
        public HttpResponseMessage UpdateUserDetails(UserProfile objUserProfile)
        {
            try
            {
                UserInfo objUserInfo = new UserInfo();
                string mobileNumber = objUserProfile.MobileNumber;
                //to get userinfo 
                var userDetails = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (userDetails != null)
                {
                    {
                        userDetails.RetailerName = !string.IsNullOrEmpty(objUserProfile.FirstName) ? objUserProfile.FirstName : userDetails.RetailerName;
                        userDetails.RetailerLastName = !string.IsNullOrEmpty(objUserProfile.LastName) ? objUserProfile.LastName : userDetails.RetailerLastName;
                        userDetails.EmailId = !string.IsNullOrEmpty(objUserProfile.EmailId) ? objUserProfile.EmailId : userDetails.EmailId;
                        userDetails.BirthDate = !string.IsNullOrEmpty(objUserProfile.BirthDate) ? Convert.ToDateTime(objUserProfile.BirthDate) : userDetails.BirthDate;
                        userDetails.Address = !string.IsNullOrEmpty(objUserProfile.Address) ? objUserProfile.Address : userDetails.Address;
                        userDetails.StreetLine1 = !string.IsNullOrEmpty(objUserProfile.StreetLine1) ? objUserProfile.StreetLine1 : userDetails.StreetLine1;
                        userDetails.StreetLine2 = !string.IsNullOrEmpty(objUserProfile.StreetLine2) ? objUserProfile.StreetLine2 : userDetails.StreetLine2;
                        userDetails.Pincode = !string.IsNullOrEmpty(objUserProfile.Pincode) ? objUserProfile.Pincode : userDetails.Pincode;
                        userDetails.State = !string.IsNullOrEmpty(objUserProfile.State) ? objUserProfile.State : userDetails.State;
                        userDetails.District = !string.IsNullOrEmpty(objUserProfile.District) ? objUserProfile.District : userDetails.District;
                        userDetails.Taluka = !string.IsNullOrEmpty(objUserProfile.Taluka) ? objUserProfile.Taluka : userDetails.Taluka;
                        userDetails.Town = !string.IsNullOrEmpty(objUserProfile.Town) ? objUserProfile.Town : userDetails.Town;
                        userDetails.Firm_Name = !string.IsNullOrEmpty(objUserProfile.FirmName) ? objUserProfile.FirmName : userDetails.Firm_Name;
                        userDetails.GSTNumber = !string.IsNullOrEmpty(objUserProfile.GstNumber) ? objUserProfile.GstNumber : userDetails.GSTNumber;
                        userDetails.PANNumber = !string.IsNullOrEmpty(objUserProfile.PanNumber) ? objUserProfile.PanNumber : userDetails.PANNumber;
                        userDetails.LicenseNumber = !string.IsNullOrEmpty(objUserProfile.LicenseNumber) ? objUserProfile.LicenseNumber : userDetails.LicenseNumber;
                        userDetails.SeedLicenseValidity = !string.IsNullOrEmpty(objUserProfile.LicenseValidity) ? Convert.ToDateTime(objUserProfile.LicenseValidity) : userDetails.SeedLicenseValidity;
                        userDetails.IsProfileUpdated = objUserProfile.IsProfileUpdate;
                    }

                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "Updated successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Something went wrong, please try again later.";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "User not found.";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "UpdateUserDetails");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }


        #endregion


        #region User Address

        //[HttpPost]
        //[AllowAnonymous]
        //[Route("api/User/GetMenuItems")]
        //public HttpResponseMessage GetMenuItems(Roles objUserRole)
        //{
        //    try
        //    {
        //        Roles objUserRoles = new Roles();
        //        SubCategoryViewModel objListSubCategory = new SubCategoryViewModel();
        //        CategoryViewModel objListCategory = new CategoryViewModel();
        //        int roleId = objUserRole.RoleId;
        //        //For genrating OTP and saving it into UserOTPInfo table
        //        var userDetails = dbContext.Roles.Where(x => x.RoleId == roleId).Include(x => x.Category).ToList();
        //        if (userDetails != null)
        //        {
        //            foreach(var i in userDetails)
        //            {
        //                SubCategoryViewModel objSubCategory = new SubCategoryViewModel()
        //                {

        //                };
        //                CategoryViewModel objCategory = new CategoryViewModel()
        //                {

        //                };

        //            }



        //            var j = dbContext.SaveChanges();
        //            if (j != 0)
        //            {
        //                objResponse.Message = "Updated successfully";
        //                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
        //            }
        //            else
        //            {
        //                objResponse.Message = "Something went wrong, please try again later.";
        //                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
        //            }
        //        }
        //        else
        //        {
        //            objResponse.Message = "User not found.";
        //            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objCommonClasses.InsertExceptionDetails(ex, "UserController", "UpdateUserDetails");
        //        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
        //    }

        //}


        [HttpPost]
        [Authorize]
        [Route("api/User/GetAddress")]
        public HttpResponseMessage GetAddress(UserMobileNumber objUserMobileNumber)
        {
            try
            {
                AddressMaster objListAddressMaster = new AddressMaster();
                List<Address> objListAddress = new List<Address>();
                string mobileNumber = objUserMobileNumber.MobileNumber;

                //get mobileNumber from user table
                var getUser = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();

                //For getting list of address from the table.
                var getaddress = (from address in dbContext.UsersAddress where address.MobileNumberForOnline == getUser.MobileNumber select address).ToList();
                if (getaddress != null)
                {
                    foreach (var i in getaddress)
                    {
                        Address objAddress = new Address()
                        {
                            tr_id = i.tr_id,
                            retailer_id = i.retailer_id,
                            retailer_mobile = i.MobileNumberForOnline,
                            reciver_name = i.reciver_name,
                            ship_address = i.ship_address,
                            city = i.city,
                            pincode = i.pincode,
                            ship_mobile = i.ship_mobile,
                            email_id = i.email_id,
                            PanNumber = i.Pan_number,
                            tr_date = Convert.ToString(i.tr_date),

                        };
                        objListAddress.Add(objAddress);
                    }
                    objListAddressMaster.Addresses = objListAddress;
                    return Request.CreateResponse(HttpStatusCode.OK, objListAddressMaster);
                }
                objResponse.Message = "Address not found";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);


            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MasterController", "GetAddress");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }


        /// <summary>
        /// To insert Users Address.
        /// </summary>
        /// <param name="objAddress"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/User/AddAddress")]
        public HttpResponseMessage AddAddress(Address objAddress)
        {
            try
            {
                UsersAddress objUsersAddress = new UsersAddress();
                string mobileNumber = objAddress.retailer_mobile;
                //get mobileNumber from user table
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    objUsersAddress.retailer_id = objAddress.retailer_id;
                    objUsersAddress.MobileNumberForOnline = objAddress.retailer_mobile;
                    objUsersAddress.reciver_name = objAddress.reciver_name;
                    objUsersAddress.ship_address = objAddress.ship_address;
                    objUsersAddress.city = objAddress.city;
                    objUsersAddress.pincode = objAddress.pincode;
                    objUsersAddress.ship_mobile = objAddress.ship_mobile;
                    objUsersAddress.email_id = objAddress.email_id;
                    objUsersAddress.Pan_number = objAddress.PanNumber;
                    objUsersAddress.tr_date = !string.IsNullOrEmpty(objAddress.tr_date) ? Convert.ToDateTime(objAddress.tr_date) : (DateTime?)null;


                    dbContext.UsersAddress.Add(objUsersAddress);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "Added successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "AddAddress");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        #endregion

        #region Feedback

        /// <summary>
        /// To insert Users Feedback.
        /// </summary>
        /// <param name="objFeedback"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/User/SubmitFeedback")]
        public HttpResponseMessage SubmitFeedback(UserFeedbackViewModel objFeedback)
        {
            try
            {
                UserFeedback objUserFeedback = new UserFeedback();
                string mobileNumber = objFeedback.Usercode;
                //get mobileNumber from user table
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    //objUserFeedback.Tr_Id = objFeedback.Tr_Id;
                    objUserFeedback.Usercode = objFeedback.Usercode;
                    objUserFeedback.Product = objFeedback.Product;
                    objUserFeedback.Feedback = objFeedback.Feedback;
                    //objUserFeedback.Tr_Date = !string.IsNullOrEmpty(objFeedback.Tr_Date) ? Convert.ToDateTime(objFeedback.Tr_Date) : (DateTime?)null;
                    objUserFeedback.Tr_Date = DateTime.Now;


                    dbContext.UserFeedback.Add(objUserFeedback);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "Submitted successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "SubmitFeedback");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        #endregion

        #region User Order Details

        /// <summary>
        /// To insertOrder Details.
        /// </summary>
        /// <param name="objOrderDetailsViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Route("api/User/AddOrderDetails")]
        public HttpResponseMessage AddOrderDetails(OrderDetailsViewModel objOrderDetailsViewModel)
        {

            try
            {
                OrderDetails objOrderDetails = new OrderDetails();
                string mobileNumber = objOrderDetailsViewModel.Retailer_Mobile;
                //get mobileNumber from user table
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    //objOrderDetails.Order_Id = objOrderDetailsViewModel.Order_Id;
                    objOrderDetails.Retailer_Id = objOrderDetailsViewModel.Retailer_Id;
                    objOrderDetails.Retailer_Mobile = objOrderDetailsViewModel.Retailer_Mobile;
                    objOrderDetails.Totalprice = objOrderDetailsViewModel.Totalprice;
                    //objOrderDetails.AmountToPayOnline = objOrderDetailsViewModel.AmountToPayOnline;
                    // objOrderDetails.WalletCurrentBalance = objOrderDetailsViewModel.WalletCurrentBalance;

                    objOrderDetails.Payment_Mode = objOrderDetailsViewModel.Payment_Mode;
                    objOrderDetails.OrderDate = DateTime.Now;
                    //objOrderDetails.OrderDate = !string.IsNullOrEmpty(objOrderDetailsViewModel.OrderDate) ? Convert.ToDateTime(objOrderDetailsViewModel.OrderDate) : (DateTime?)null;
                    objOrderDetails.Shipping_Address_Id = objOrderDetailsViewModel.Shipping_Address_Id;
                    objOrderDetails.Order_Status = objOrderDetailsViewModel.Order_Status;
                    objOrderDetails.SAP_Order_ID = "12345";
                    objOrderDetails.GeoCoordinates = objOrderDetailsViewModel.GeoCoordinates;
                    objOrderDetails.Rzp_Order_Id = objOrderDetailsViewModel.Rzp_Order_Id;
                    objOrderDetails.Rzp_Payment_Id = objOrderDetailsViewModel.Rzp_Payment_Id;
                    objOrderDetails.Rzp_Signature = objOrderDetailsViewModel.Rzp_Signature;
                    objOrderDetails.Rzp_Payment_Status = "Paid";


                    dbContext.OrderDetails.Add(objOrderDetails);
                    var i = dbContext.SaveChanges();
                    int OrderId = objOrderDetails.Order_Id; // Get OrderId After Save Changes
                    if (i != 0)
                    {
                        objOrderDetailsViewModel.Order_Id = Convert.ToString(OrderId);
                        AddOrderProductDetail(objOrderDetailsViewModel);
                        string message = "Your Order Is received";
                        objCommonClasses.SendSMS(mobileNumber, message);
                        if (objOrderDetailsViewModel.Payment_Mode == "Pay Online")
                        {

                            Dictionary<string, object> input = new Dictionary<string, object>();
                            input.Add("amount", Convert.ToDouble(objOrderDetailsViewModel.Totalprice) * 100); // this amount should be same as transaction amount
                            input.Add("currency", "INR");
                            input.Add("receipt", objOrderDetailsViewModel.Order_Id);
                            input.Add("payment_capture", 1);

                            string key = "rzp_test_rGmx3fJtAsO7Nq";
                            string secret = "tuivsSqAlQZcI9MACQWQ6F46";

                            RazorpayClient client = new RazorpayClient(key, secret);
                            Razorpay.Api.Order order = client.Order.Create(input);
                            //var razorPayOrderId= JsonConvert.SerializeObject(order.Attributes.id);
                            //var razorPayReceipt = JsonConvert.SerializeObject(order.Attributes.receipt);

                            //object a = razorPayOrderId + razorPayReceipt;
                            return Request.CreateResponse(HttpStatusCode.OK, order);

                        }

                        else
                        {
                            objResponse.Message = objOrderDetailsViewModel.Totalprice;
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }



                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }

                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);

                OrderErrorLogDetails objOrderErrorLogDetails = new OrderErrorLogDetails();
                objOrderErrorLogDetails.Retailer_Id = objOrderDetailsViewModel.Retailer_Id;
                objOrderErrorLogDetails.Retailer_Mobile = objOrderDetailsViewModel.Retailer_Mobile;
                objOrderErrorLogDetails.PaymentMode = objOrderDetailsViewModel.Payment_Mode;
                objOrderErrorLogDetails.Total_Price = objOrderDetailsViewModel.Totalprice;
                objOrderErrorLogDetails.Qty = objOrderDetailsViewModel.Qty;
                //objOrderErrorLogDetails.Csv_File =;
                objOrderErrorLogDetails.Error_Msg = ex.Message;
                objOrderErrorLogDetails.Tr_Date = DateTime.Now;

                dbContext.OrderErrorLogDetails.Add(objOrderErrorLogDetails);
                var i = dbContext.SaveChanges();

                if (i != 0)
                {
                    objResponse.Message = "Exception Logged Successfully";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
                else
                {
                    objResponse.Message = "Failed";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }

                //objCommonClasses.InsertExceptionDetails(ex, "UserController", "SubmitFeedback");
                //return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);

            }


        }


        public string AddOrderProductDetail(OrderDetailsViewModel objOrderDetailsViewModel)
        {
            var i = 0;

            try
            {
                //get mobileNumber from user table
                string mobileNumber = objOrderDetailsViewModel.Retailer_Mobile;
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    OrderProductDetails objOrderProductDetails = new OrderProductDetails();
                    var getcsvfile = objOrderDetailsViewModel.csvfile.Table1;

                    foreach (var j in getcsvfile)
                    {
                        objOrderProductDetails.order_id = Convert.ToInt32(objOrderDetailsViewModel.Order_Id);
                        objOrderProductDetails.product_id = j.product_Id;
                        objOrderProductDetails.qty = j.OrderQuantity;
                        objOrderProductDetails.price = j.price;
                        objOrderProductDetails.tr_date = DateTime.Now;
                        objOrderProductDetails.SAP_order_ID = objOrderDetailsViewModel.SAP_Order_ID;
                        dbContext.OrderProductDetails.Add(objOrderProductDetails);
                        i = dbContext.SaveChanges();


                    }

                    //objOrderProductDetails.product_id = "115001003CBE17";
                    //objOrderProductDetails.qty = "60";
                    //objOrderProductDetails.price = "410";




                    if (i != 0)
                    {
                        objResponse.Message = objOrderDetailsViewModel.Totalprice;
                        return objResponse.Message;
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        return objResponse.Message;
                    }
                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return objResponse.Message;

                }

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);

                OrderErrorLogDetails objOrderErrorLogDetails = new OrderErrorLogDetails();
                objOrderErrorLogDetails.Retailer_Id = objOrderDetailsViewModel.Retailer_Id;
                objOrderErrorLogDetails.Retailer_Mobile = objOrderDetailsViewModel.Retailer_Mobile;
                objOrderErrorLogDetails.PaymentMode = objOrderDetailsViewModel.Payment_Mode;
                objOrderErrorLogDetails.Total_Price = objOrderDetailsViewModel.Totalprice;
                objOrderErrorLogDetails.Qty = objOrderDetailsViewModel.Qty;
                objOrderErrorLogDetails.Csv_File = JsonConvert.SerializeObject(objOrderDetailsViewModel.csvfile.Table1);
                objOrderErrorLogDetails.Error_Msg = ex.Message;
                objOrderErrorLogDetails.Tr_Date = DateTime.Now;

                dbContext.OrderErrorLogDetails.Add(objOrderErrorLogDetails);
                i = dbContext.SaveChanges();

                if (i != 0)
                {
                    objResponse.Message = "Exception Logged Successfully";
                    return objResponse.Message;
                }
                else
                {
                    objResponse.Message = "Failed to Log Exception";
                    return objResponse.Message;
                }

                //    objCommonClasses.InsertExceptionDetails(ex, "UserController", "AddOrderProductDetail");
                //    return ex.Message;
            }

        }


        /// <summary>
        /// To Update Order Details.
        /// </summary>
        /// <param name="objOrderDetailsViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Route("api/User/UpdateOrderDetails")]
        public HttpResponseMessage UpdateOrderDetails(OrderDetailsViewModel objOrderDetailsViewModel)
        {

            try
            {
                OrderDetails objOrderDetails = new OrderDetails();
                int orderId = Convert.ToInt32(objOrderDetailsViewModel.Order_Id);
                //get Order from OrderDetails table
                var orderDetails = (from order in dbContext.OrderDetails where order.Order_Id == orderId select order).FirstOrDefault();
                if (orderDetails != null && orderDetails.Payment_Mode == "Pay Online" && objOrderDetailsViewModel.Rzp_Payment_Status == "Paid")
                {

                    orderDetails.Rzp_Order_Id = objOrderDetailsViewModel.Rzp_Order_Id;
                    orderDetails.Rzp_Payment_Id = objOrderDetailsViewModel.Rzp_Payment_Id;
                    orderDetails.Rzp_Signature = objOrderDetailsViewModel.Rzp_Signature;
                    orderDetails.Rzp_Payment_Status = objOrderDetailsViewModel.Rzp_Payment_Status;


                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        TransactionResponse objTransactionResponse = new TransactionResponse();
                        objTransactionResponse.Order_Id = objOrderDetailsViewModel.Order_Id;
                        objTransactionResponse.Rzp_Order_Id = objOrderDetailsViewModel.Rzp_Order_Id;
                        objTransactionResponse.Rzp_Payment_Id = objOrderDetailsViewModel.Rzp_Payment_Id;
                        objTransactionResponse.Rzp_Signature = objOrderDetailsViewModel.Rzp_Signature;
                        objTransactionResponse.Rzp_Payment_Status = objOrderDetailsViewModel.Rzp_Payment_Status;
                        objTransactionResponse.Rzp_payment_message = "Order placed successfully";
                        objTransactionResponse.Totalprice = objOrderDetailsViewModel.Totalprice;

                        return Request.CreateResponse(HttpStatusCode.OK, objTransactionResponse);
                    }
                    else
                    {
                        objResponse.Message = "Transaction Failed.";

                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }

                }

                else if (objOrderDetailsViewModel.Rzp_Payment_Status == "Fail")
                {
                    orderDetails.Rzp_Payment_Status = objOrderDetailsViewModel.Rzp_Payment_Status;
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        TransactionResponse objTransactionResponse = new TransactionResponse();
                        objTransactionResponse.Order_Id = objOrderDetailsViewModel.Order_Id;
                        objTransactionResponse.Rzp_Order_Id = objOrderDetailsViewModel.Rzp_Order_Id;
                        objTransactionResponse.Rzp_Payment_Id = objOrderDetailsViewModel.Rzp_Payment_Id;
                        objTransactionResponse.Rzp_Signature = objOrderDetailsViewModel.Rzp_Signature;
                        objTransactionResponse.Rzp_Payment_Status = objOrderDetailsViewModel.Rzp_Payment_Status;
                        objTransactionResponse.Rzp_payment_message = objOrderDetailsViewModel.Rzp_payment_message;
                        objTransactionResponse.Totalprice = objOrderDetailsViewModel.Totalprice;

                        return Request.CreateResponse(HttpStatusCode.OK, objTransactionResponse);
                    }
                    else
                    {
                        objResponse.Message = "Error occured While Saving Transaction Status.";

                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }

                }
                else
                {
                    objResponse.Message = "No Order Details Found.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);

                OrderErrorLogDetails objOrderErrorLogDetails = new OrderErrorLogDetails();
                objOrderErrorLogDetails.Retailer_Id = objOrderDetailsViewModel.Retailer_Id;
                objOrderErrorLogDetails.Retailer_Mobile = objOrderDetailsViewModel.Retailer_Mobile;
                objOrderErrorLogDetails.PaymentMode = objOrderDetailsViewModel.Payment_Mode;
                objOrderErrorLogDetails.Total_Price = objOrderDetailsViewModel.Totalprice;
                objOrderErrorLogDetails.Qty = objOrderDetailsViewModel.Qty;
                //objOrderErrorLogDetails.Csv_File =;
                objOrderErrorLogDetails.Error_Msg = ex.Message;
                objOrderErrorLogDetails.Tr_Date = DateTime.Now;

                dbContext.OrderErrorLogDetails.Add(objOrderErrorLogDetails);
                var i = dbContext.SaveChanges();

                if (i != 0)
                {
                    objResponse.Message = "Exception Logged Successfully";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
                else
                {
                    objResponse.Message = "Failed";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }


            }


        }



        /// <summary>
        /// To Get Order Details History.
        /// </summary>
        /// <param name="objOrderHistoryViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/User/GetOrderHistory")]
        public HttpResponseMessage GetOrderHistory(OrderHistoryViewModel objOrderHistoryViewModel)
        {


            try
            {
                OrderList objOrderList = new OrderList();
                List<OrderHistoryViewModel> objListOrderHistory = new List<OrderHistoryViewModel>();
                string mobileNumber = objOrderHistoryViewModel.Retailer_Mobile;

                //get mobileNumber from user table
                var getUser = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();


                var getOrderList = (from orderList in dbContext.OrderDetails
                                    join product in dbContext.OrderProductDetails on orderList.Order_Id equals product.order_id
                                    join address in dbContext.UsersAddress on orderList.Shipping_Address_Id equals address.tr_id
                                    join productmaster in dbContext.Product_Master on product.product_id equals productmaster.ItemCode

                                    orderby orderList.Order_Id descending
                                    select new
                                    {
                                        orderList.Order_Id,
                                        orderList.Retailer_Id,
                                        orderList.Retailer_Mobile,
                                        orderList.Totalprice,
                                        orderList.Payment_Mode,
                                        orderList.Shipping_Address_Id,
                                        orderList.Order_Status,
                                        orderList.OrderDate,
                                        address.reciver_name,
                                        address.ship_address,
                                        address.city,
                                        address.pincode,
                                        address.ship_mobile,
                                        product.qty,
                                        productmaster.SkuName

                                    }).ToList();



                if (getOrderList != null)
                {

                    foreach (var i in getOrderList)
                    {
                        OrderHistoryViewModel objOrderHistoryList = new OrderHistoryViewModel()
                        {
                            Order_Id = Convert.ToString(i.Order_Id),
                            Retailer_Id = i.Retailer_Id,
                            Retailer_Mobile = i.Retailer_Mobile,
                            Totalprice = i.Totalprice,
                            Payment_Mode = i.Payment_Mode,
                            Shipping_Address_Id = Convert.ToString(i.Shipping_Address_Id),
                            Order_Status = i.Order_Status,
                            OrderDate = Convert.ToString(i.OrderDate),
                            reciver_name = i.reciver_name,
                            ship_address = i.ship_address,
                            city = i.city,
                            pincode = i.pincode,
                            ship_mobile = i.ship_mobile,
                            Qty = i.qty,
                            ProductName = i.SkuName



                        };
                        objListOrderHistory.Add(objOrderHistoryList);
                    }


                    objOrderList.Orders = objListOrderHistory;
                    return Request.CreateResponse(HttpStatusCode.OK, objOrderList);
                }
                objResponse.Message = "Order History not found";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);


            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "GetOrderHistory");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }


        #endregion


        #region User Wallet functionality

        [HttpPost]
        //[Authorize]
        [Route("api/User/UpdateWalletBalance")]
        public HttpResponseMessage UpdateWalletBalance(UserWalletViewModel objAddress)
        {
            try
            {
                UserWallet objUserWallet = new UserWallet();
                string mobileNumber = objAddress.MobileNumber;
                //get mobileNumber from user table
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {

                    var CheckBalanceforUser = (from balance in dbContext.UserWallet where balance.MobileNumber == mobileNumber select balance).FirstOrDefault();
                    if (CheckBalanceforUser != null)
                    {
                        int Availablebalance = Convert.ToInt32(CheckBalanceforUser.WalletBalance);
                        int TobeDeductedAmount = Convert.ToInt32(objAddress.WalletBalance);
                        int totalAmount = Availablebalance - TobeDeductedAmount;

                        objUserWallet.Id = CheckBalanceforUser.Id;
                        var getWalletInfo = (from balance in dbContext.UserWallet where balance.Id == CheckBalanceforUser.Id select balance).FirstOrDefault();

                        getWalletInfo.WalletBalance = totalAmount.ToString();

                        getWalletInfo.ModifiedDate = DateTime.Now;



                        dbContext.Entry(getWalletInfo).State = EntityState.Modified;
                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            objResponse.Message = "Wallet balance updated successfully";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            objResponse.Message = "Failed to add wallet balance.";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else
                    {
                        objResponse.Message = "Mobile number not exists.";

                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                    }

                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// for adding wallet balance 
        /// </summary>
        /// <param name="objAddress"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Route("api/User/AddWalletBalance")]
        public HttpResponseMessage AddWalletBalance(UserWalletViewModel objAddress)
        {
            try
            {
                UserWallet objUserWallet = new UserWallet();
                string mobileNumber = objAddress.MobileNumber;
                //get mobileNumber from user table
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {

                    var CheckBalanceforUser = (from balance in dbContext.UserWallet where balance.MobileNumber == mobileNumber select balance).FirstOrDefault();
                    if (CheckBalanceforUser == null)
                    {
                        objUserWallet.Id = objAddress.Id;
                        objUserWallet.WalletBalance = objAddress.WalletBalance;
                        objUserWallet.MobileNumber = objAddress.MobileNumber;
                        objUserWallet.Status = true;
                        objUserWallet.CreatedDate = DateTime.Now;



                        dbContext.UserWallet.Add(objUserWallet);
                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            objResponse.Message = "Wallet balace added successfully";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            objResponse.Message = "Failed to add wallet balance.";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else
                    {
                        int Availablebalance = Convert.ToInt32(CheckBalanceforUser.WalletBalance);
                        int TobeAddedAmount = Convert.ToInt32(objAddress.WalletBalance);
                        int totalAmount = Availablebalance + TobeAddedAmount;

                        objUserWallet.Id = CheckBalanceforUser.Id;
                        var getWalletInfo = (from balance in dbContext.UserWallet where balance.Id == CheckBalanceforUser.Id select balance).FirstOrDefault();

                        getWalletInfo.WalletBalance = totalAmount.ToString();

                        getWalletInfo.ModifiedDate = DateTime.Now;



                        dbContext.Entry(getWalletInfo).State = EntityState.Modified;
                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            objResponse.Message = "Wallet balace added successfully";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            objResponse.Message = "Failed to add wallet balance.";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "AddWalletBalance");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }



        [HttpPost]
        //[Authorize]
        [Route("api/User/GetWalletInfo")]
        public HttpResponseMessage GetWalletInfo(UserMobileNumber objUserMobileNumber)
        {
            try
            {
                string mobileNumber = objUserMobileNumber.MobileNumber;

                var userwalletDetails = (from user in dbContext.UserWallet where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (userwalletDetails != null)
                {
                    UserWalletViewModel objUserWalletViewModel = new UserWalletViewModel()
                    {
                        Id = userwalletDetails.Id,
                        MobileNumber = userwalletDetails.MobileNumber,
                        Status = userwalletDetails.Status,
                        WalletBalance = userwalletDetails.WalletBalance,

                    };
                    return Request.CreateResponse(HttpStatusCode.OK, objUserWalletViewModel);
                }
                objResponse.Message = "User wallet does Not exist";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "GetUserWalletInfo");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }


        #endregion

        #region Requirement

        [HttpPost]
        //[Authorize]
        [Route("api/User/AddUserRequirement")]
        public HttpResponseMessage AddUserRequirement(MandiUserRequirementViewModel objUserRequirementViewModel)
        {
            try
            {
                Mandi_Requirement objRequirement = new Mandi_Requirement();
                string mobileNumber = objUserRequirementViewModel.MobileNumber;
                //get mobileNumber from user table
                var number = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {

                    objRequirement.Id = objUserRequirementViewModel.Id;
                    objRequirement.MobileNumber = objUserRequirementViewModel.MobileNumber;
                    objRequirement.BuyerId = objUserRequirementViewModel.MobileNumber;
                    objRequirement.BuyerContact = objUserRequirementViewModel.BuyerContact;
                    objRequirement.BuyerAddress = objUserRequirementViewModel.BuyerAddress;
                    objRequirement.CropName = objUserRequirementViewModel.CropName;
                    objRequirement.Variety = objUserRequirementViewModel.Variety;
                    objRequirement.Quantity = objUserRequirementViewModel.Quantity;
                    objRequirement.QualitySpecification = objUserRequirementViewModel.QualitySpecification;
                    objRequirement.DeliveryLocation = objUserRequirementViewModel.DeliveryLocation;
                    objRequirement.ExpectedPrice = objUserRequirementViewModel.ExpectedPrice;
                    objRequirement.ExpectedDate = objUserRequirementViewModel.ExpectedDate;
                    objRequirement.IsPriceNegotiable = objUserRequirementViewModel.IsPriceNegotiable;
                    objRequirement.Remarks = objUserRequirementViewModel.Remarks;
                    objRequirement.Tr_Date = DateTime.Now;

                    dbContext.Mandi_Requirement.Add(objRequirement);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        EmailController objEmailController = new EmailController();
                        EmailModel objEmailModel = new EmailModel();
                        objEmailModel.BuyerId = objUserRequirementViewModel.BuyerId;
                        objEmailModel.BuyerAddress = objUserRequirementViewModel.BuyerAddress;
                        objEmailModel.CropName = objUserRequirementViewModel.CropName;
                        objEmailModel.VarietyName = objUserRequirementViewModel.Variety;
                        objEmailModel.Quantity = objUserRequirementViewModel.Quantity;
                        objEmailModel.QualitySpecification = objUserRequirementViewModel.QualitySpecification;
                        objEmailModel.DeliveryLocation = objUserRequirementViewModel.DeliveryLocation;
                        objEmailModel.ExpectedPrice = objUserRequirementViewModel.ExpectedPrice;
                        objEmailModel.ExpectedDate = objUserRequirementViewModel.ExpectedDate;
                        objEmailModel.IsPriceNegotiable = objUserRequirementViewModel.IsPriceNegotiable;
                        objEmailModel.Remarks = objUserRequirementViewModel.Remarks;


                        objEmailController.sendEmailViaWebApi(objEmailModel, "UserRequirement");
                        objResponse.Message = "User Requirement added successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Failed to add user requiremrent.";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }


                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "AddUserRequirement");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }




     

        #endregion
    }
}