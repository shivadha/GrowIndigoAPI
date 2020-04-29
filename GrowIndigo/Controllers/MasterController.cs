using GrowIndigo.Common;
using GrowIndigo.Data;
using GrowIndigo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using static GrowIndigo.Models.MasterData;
using static GrowIndigo.Models.UsersAddressViewModel;

namespace GrowIndigo.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MasterController : ApiController
    {
        GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities();
        Authentication auth = new Authentication();
        CommonClasses objCommonClasses = new CommonClasses();
        SuccessResponse objResponse = new SuccessResponse();



        [HttpGet]
        [AllowAnonymous]
        [Route("api/Master/GetStates")]
        public HttpResponseMessage GetStates()
        {
            try
            {
                StateMaster objListStateMaster = new StateMaster();
                List<State> objListState = new List<State>();
                //For getting list of states from the table.
                var getState = (from state in dbContext.State_Master select state).ToList();
                if (getState != null)
                {
                    foreach (var i in getState)
                    {
                        State objState = new State()
                        {
                            StateCode = i.StateCode,
                            StateName = i.StateName
                        };
                        objListState.Add(objState);
                    }
                    objListStateMaster.States = objListState;

                    return Request.CreateResponse(HttpStatusCode.OK, objListStateMaster);
                }
                else
                {
                    objResponse.Message = "Failed";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MasterController", "GetStates");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/Master/GetDistrict")]
        public HttpResponseMessage GetDistrict(string stateCode)
        {
            try
            {
                DistrictMaster objListDistrictMaster = new DistrictMaster();
                List<District> objListDistrict = new List<District>();
                //For getting list of districts from the table.
                var getDistrict = (from district in dbContext.District_Master where district.StateCode == stateCode select district).ToList();
                if (getDistrict != null)
                {
                    foreach (var i in getDistrict)
                    {
                        District objDistrict = new District()
                        {
                            DistrictCode = i.DistrictCode,
                            DistrictName = i.DistrictName
                        };
                        objListDistrict.Add(objDistrict);
                    }

                    objListDistrictMaster.District = objListDistrict;

                    return Request.CreateResponse(HttpStatusCode.OK, objListDistrictMaster);
                }
                else
                {
                    objResponse.Message = "Failed";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MasterController", "GetDistrict");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("api/Master/GetTaluka")]
        public HttpResponseMessage GetTaluka(string districtCode)
        {
            try
            {
                TalukaMaster objListTalukaMaster = new TalukaMaster();
                List<Taluka> objListTaluka = new List<Taluka>();
                //For getting list of Taluka from the table.
                var getTaluka = (from taluka in dbContext.Taluka_Master where taluka.DistrictCode == districtCode select taluka).ToList();
                if (getTaluka != null)
                {
                    foreach (var i in getTaluka)
                    {
                        Taluka objTaluka = new Taluka()
                        {
                            TalukaCode = i.TalukaCode,
                            TalukaName = i.TalukaName
                        };
                        objListTaluka.Add(objTaluka);
                    }
                    objListTalukaMaster.Taluka = objListTaluka;
                    return Request.CreateResponse(HttpStatusCode.OK, objListTalukaMaster);
                }
                else
                {
                    objResponse.Message = "Failed";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MasterController", "GetTaluka");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }


        [HttpPost]
        [Authorize]
        [Route("api/Master/GetProducts")]
        public HttpResponseMessage GetProducts(UserMobileNumber objUserMobileNumber)
        {
            try
            {
                ProductMaster objListProductMaster = new ProductMaster();
                List<Product> objListProduct = new List<Product>();
                string mobileNumber = objUserMobileNumber.MobileNumber;

                //get state from user table
                var getUser = (from user in dbContext.UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if(!string.IsNullOrEmpty(getUser.State))
                {
                    //For getting list of product from the table.
                    var getProducts = (from product in dbContext.Product_Master where product.PrdState == getUser.State select product).ToList();
                    if (getProducts != null)
                    {
                        foreach (var i in getProducts)
                        {
                            Product objProduct = new Product()
                            {
                                SkuId = i.SkuId,
                                SkuName = i.SkuName,
                                ItemCode = i.ItemCode,
                                Description = i.Description,
                                Price = i.Price,
                                CurrentQuantity = i.CurrentQuantity,
                                SkuCreatedDate = Convert.ToString(i.SkuCreatedDate),
                                GSTPercent = i.GSTPercent,
                                ImageUrl = i.ImageUrl,
                                MinQuantittyToBook = i.MinQuantittyToBook,
                                MaxQuantittyToBook = i.MaxQuantittyToBook,
                                PrdCompanyName = i.PrdCompanyName,
                                AllowSAPOrder = i.AllowSAPOrder,
                                PrdState = i.PrdState,
                                Status = i.Status,

                            };
                            objListProduct.Add(objProduct);
                        }
                        objListProductMaster.Products = objListProduct;
                        return Request.CreateResponse(HttpStatusCode.OK, objListProductMaster);
                    }
                    objResponse.Message = "Products not available please update profile";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
                else
                {
                    //For getting list of product from the table.
                    var getProducts = (from product in dbContext.Product_Master select product).ToList();
                    if (getProducts != null)
                    {
                        foreach (var i in getProducts)
                        {
                            Product objProduct = new Product()
                            {
                                SkuId = i.SkuId,
                                SkuName = i.SkuName,
                                ItemCode = i.ItemCode,
                                Description = i.Description,
                                Price = i.Price,
                                CurrentQuantity = i.CurrentQuantity,
                                SkuCreatedDate = Convert.ToString(i.SkuCreatedDate),
                                GSTPercent = i.GSTPercent,
                                ImageUrl = i.ImageUrl,
                                MinQuantittyToBook = i.MinQuantittyToBook,
                                MaxQuantittyToBook = i.MaxQuantittyToBook,
                                PrdCompanyName = i.PrdCompanyName,
                                AllowSAPOrder = i.AllowSAPOrder,
                                PrdState = i.PrdState,
                                Status = i.Status,

                            };
                            objListProduct.Add(objProduct);
                        }
                        objListProductMaster.Products = objListProduct;
                        return Request.CreateResponse(HttpStatusCode.OK, objListProductMaster);
                    }
                    objResponse.Message = "Product not found";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MasterController", "GetProducts");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

    }
}
