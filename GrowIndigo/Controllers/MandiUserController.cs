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
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Device.Location;
using static GrowIndigo.Models.UsersAddressViewModel;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using static GrowIndigo.Models.Mandi_OrderViewModel;
using System.Configuration;
using System.Web;
using System.Threading.Tasks;
using System.Net.Mail;
using static GrowIndigo.Models.MandiMasterData;

namespace GrowIndigo.Controllers
{
    //[Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MandiUserController : ApiController
    {
        #region Dependeancies
        GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities();
        Authentication auth = new Authentication();
        CommonClasses objCommonClasses = new CommonClasses();

        SuccessResponse objResponse = new SuccessResponse();

        #endregion



        #region Product

        /// <summary>
        /// For Adding Product.
        /// </summary>
        /// <param name="objProductMasterViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Route("api/MandiUser/AddProduct")]
        public HttpResponseMessage AddProduct(ProductMasterViewModel objProductMasterViewModel)
        {
            try
            {

                //get category for cropId
                var getCategoryName = (from crop in dbContext.Crop_Master where crop.CropId == objProductMasterViewModel.CropId select crop.CategoryId).FirstOrDefault();
                Mandi_ProductMaster objMandi_ProductMaster = new Mandi_ProductMaster();
                string mobileNumber = objProductMasterViewModel.MobileNumber;

                var getProductDays = (from day in dbContext.Crop_Master where day.CropId == objProductMasterViewModel.CropId select day.CropAvailableDays).FirstOrDefault();
                DateTime AfterDate = Convert.ToDateTime(objProductMasterViewModel.AvailabilityDate);
                int EndDays = Convert.ToInt32(getProductDays);
                var ExpireAfterDate = AfterDate.AddDays(EndDays);

                var getCominsoonDate = (ExpireAfterDate - DateTime.Now).TotalDays;
                double comingdays = getCominsoonDate;
                //get mobileNumber from user table
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    if (!string.IsNullOrEmpty(objProductMasterViewModel.NewVariety) && objProductMasterViewModel.VarietyId == 0)
                    {
                        MandiMasterController objMandiMasterController = new MandiMasterController();
                        objMandiMasterController.AddNewVariety(objProductMasterViewModel);

                    }

                    var CropStatus = ExpireAfterDate == null ? "Sold" : ExpireAfterDate >= DateTime.Now ? "Available" : comingdays <= 15 ? "Coming Soon" : "Sold";
                    // objMandi_ProductMaster.CropEndDate = ExpireAfterDate;
                    objMandi_ProductMaster.CropId = objProductMasterViewModel.CropId;
                    objMandi_ProductMaster.CategoryId = getCategoryName;
                    objMandi_ProductMaster.VarietyId = objProductMasterViewModel.VarietyId;
                    objMandi_ProductMaster.ProductAddress = objProductMasterViewModel.ProductAddress;
                    objMandi_ProductMaster.GeoAddress = objProductMasterViewModel.GeoAddress;
                    objMandi_ProductMaster.MobileNumber = objProductMasterViewModel.MobileNumber;
                    objMandi_ProductMaster.Product_Status = "In Review";

                    objMandi_ProductMaster.NetBankingId = objProductMasterViewModel.NetBankingId;
                    objMandi_ProductMaster.Quantity = objProductMasterViewModel.Quantity;
                    objMandi_ProductMaster.QuantityUnit = objProductMasterViewModel.QuantityUnit;
                    objMandi_ProductMaster.Price = objProductMasterViewModel.Price;
                    objMandi_ProductMaster.ServiceTax = (decimal)1.00;
                    objMandi_ProductMaster.AvailabilityDate = objProductMasterViewModel.AvailabilityDate;
                    objMandi_ProductMaster.PaymentMethod = objProductMasterViewModel.PaymentMethod;
                    objMandi_ProductMaster.IsQualityTestNeeded = objProductMasterViewModel.IsQualityTestNeeded;
                    objMandi_ProductMaster.IsLogisticNeeded = objProductMasterViewModel.IsLogisticNeeded;
                    objMandi_ProductMaster.ProductImageUrl = objProductMasterViewModel.ProductImageUrl;
                    objMandi_ProductMaster.Tr_Date = DateTime.Now;

                    objMandi_ProductMaster.State = objProductMasterViewModel.StateCode;
                    objMandi_ProductMaster.District = objProductMasterViewModel.DistrictCode;
                    objMandi_ProductMaster.Taluka = objProductMasterViewModel.TalukaCode;
                    objMandi_ProductMaster.IsActive = true;
                    objMandi_ProductMaster.IsApproved = false;
                    objMandi_ProductMaster.SecondaryProductImage = objProductMasterViewModel.SecondaryProductImage;
                    objMandi_ProductMaster.ProductDescription = objProductMasterViewModel.ProductDescription;

                    var add = dbContext.Mandi_ProductMaster.Add(objMandi_ProductMaster);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        EmailController objEmailController = new EmailController();
                        EmailModel objEmailModel = new EmailModel();
                        objEmailModel.ProductId = add.Tr_Id.ToString();
                        objEmailModel.CropId = objProductMasterViewModel.CropId;
                        objEmailModel.CategoryName = objProductMasterViewModel.CategoryName;
                        objEmailModel.VarietyId = objProductMasterViewModel.VarietyId;
                        objEmailModel.ProductAddress = objProductMasterViewModel.ProductAddress;
                        objEmailModel.GeoAddress = objProductMasterViewModel.GeoAddress;
                        objEmailModel.MobileNumber = objProductMasterViewModel.MobileNumber;
                        objEmailModel.Quantity = objProductMasterViewModel.Quantity;
                        objEmailModel.QuantityUnit = objProductMasterViewModel.QuantityUnit;
                        objEmailModel.Price = objProductMasterViewModel.Price.ToString();
                        objEmailModel.State = objProductMasterViewModel.StateCode;
                        objEmailModel.District = objProductMasterViewModel.DistrictCode;
                        objEmailModel.Taluka = objProductMasterViewModel.TalukaCode;

                        objEmailController.sendEmailViaWebApi(objEmailModel, "AddProduct");

                        objResponse.Message = "Product Added successfully";
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
                    objResponse.Message = "User number not exists.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "AddProduct");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }



        /// <summary>
        /// for getting all products 
        /// </summary>
        /// <param name="objProductFilter"></param>
        /// <returns></returns>
        [HttpPost]
        // [Authorize]
        [Route("api/MandiUser/GetProduct")]
        public HttpResponseMessage GetProduct(ProductFilter objProductFilter)
        {
            try
            {
                var ServerPath = ConfigurationManager.AppSettings["ServerPath"];
                int counter = objProductFilter.counter;
                int take = 6;
                int skip = counter;

                var categoryId = objProductFilter.SCategoryId;
                var culture = objProductFilter.culture;
                MandiProduct objListMandiProduct = new MandiProduct();
                List<ProductMasterViewModel> objListProductMasterViewModel = new List<ProductMasterViewModel>();
                var getUser = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == objProductFilter.MobileNumber select user).FirstOrDefault();
                string mobileNumber = objProductFilter.MobileNumber;
                var getUserMobileNumber = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user.MobileNumber).FirstOrDefault();
                var getOrderHistoryDetail = (from product in dbContext.Mandi_OrderDetails where product.Buyer_Mobile == objProductFilter.MobileNumber && product.TransactionStatus == "Submitted" || product.TransactionStatus == "SUCCESS" select product).ToList();
                FilterMandiProduct objFilterMandiProduct = new FilterMandiProduct();
                List<ProductMasterViewModel> listProducts = new List<ProductMasterViewModel>();


                if (objProductFilter.ProductPagination == true)
                {
                    //to get Seller's own filter/unfiltered products (Dashboard)
                    if (objProductFilter.IsAllActiveProducts == "false" && getUser.UserType == "Sell")
                    {

                        #region Query

                        var MobileNumber = objProductFilter.MobileNumber;

                        //to check if product is already bought by user or not 
                        if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                        {
                            var products = dbContext.Mandi_ProductMaster
                                      .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                      => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                      .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                      => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,

                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               ServiceTax = i.Mandi_ProductMaster.ServiceTax,

                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                         .OrderBy(x => x.ProductPriority == "2")
                                         .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();
                            #endregion

                            #region filters
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Crop from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId);
                            }
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                                products = products.Where(x => x.IsQualityTestNeeded == Quality);

                            }
                            #endregion


                            objFilterMandiProduct.Products = products.Skip(skip).Take(take).ToList();
                            return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                        }
                        else
                        {
                            #region Query

                            var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                                 join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                                 select new ProductMasterViewModel()
                                                 {
                                                     ProductId = (int)pod.Product_Id,
                                                     OrderId = (int)pod.Order_Id,
                                                     MobileNumber = o.Buyer_Mobile



                                                 }).ToList();

                            var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                            var productss = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                           .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber != getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();



                            var allProductList = productss.ToList();
                            var getBoughtProductsIds = getProductsIds.ToList();



                            var products = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();
                            #endregion

                            #region filter

                            if (objProductFilter.IsFilterApplied == "true")
                            {
                                if (objProductFilter.CropId != null)
                                {
                                    //For getting list of Crop from the table.
                                    products = products.Where(x => x.CropId == objProductFilter.CropId).ToList();
                                }
                                if (objProductFilter.VarietyId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                                }
                                if (!string.IsNullOrEmpty(objProductFilter.State))
                                {
                                    //For getting list of State from the table.
                                    products = products.Where(x => x.StateCode == objProductFilter.State).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.District))
                                {
                                    //For getting list of District from the table.
                                    products = products.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                                {
                                    //For getting list of Taluka from the table.
                                    products = products.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                                }

                                if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                                {
                                    var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                    var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                    products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                                {
                                    //For getting list of Quantity from the table.
                                    products = products.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                                {
                                    var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                    //For getting list of address from the table.
                                    products = products.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                                }
                                objFilterMandiProduct.Products = products.ToList();
                            }

                            #endregion

                            objFilterMandiProduct.Products = products.Skip(skip).Take(take).ToList();
                            return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                        }

                    }

                    //to get buyer's own filter/unfiltered products (Dashboard)
                    else if (objProductFilter.IsAllActiveProducts == "false" && getUser.UserType == "Buy")
                    {
                        #region Geo location quer
                        var geoAddress = (from user in dbContext.Mandi_ProductMaster
                                          select new
                                          {
                                              user.GeoAddress
                                          }).ToList();
                        #endregion


                        if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                        {

                            #region Query

                            var products = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                           .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               ServiceTax = i.Mandi_ProductMaster.ServiceTax,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                              .OrderBy(x => x.ProductPriority == "2")
                                              .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();
                            #endregion

                            #region filter
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Crop from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId);
                            }
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality);


                            }
                            #endregion


                            objFilterMandiProduct.Products = products.ToList();
                        }
                        else
                        {

                            #region Query 
                            // var getOrderIds = (from product in dbContext.Mandi_OrderDetails where product.Buyer_Mobile == objProductFilter.MobileNumber && product.TransactionStatus == "SUCCESS" select product).ToList();

                            var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                                 join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                                 select new ProductMasterViewModel()
                                                 {
                                                     ProductId = (int)pod.Product_Id,
                                                     OrderId = (int)pod.Order_Id,
                                                     MobileNumber = o.Buyer_Mobile



                                                 }).ToList();

                            var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                            var productss = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                           .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               ServiceTax = i.Mandi_ProductMaster.ServiceTax,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                              .OrderBy(x => x.ProductPriority == "2")
                                              .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();


                            var allProductList = productss.ToList();
                            var getBoughtProductsIds = getProductsIds.ToList();



                            var products = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();


                            #endregion

                            #region filter

                            if (objProductFilter.IsFilterApplied == "true")
                            {
                                if (objProductFilter.CropId != null)
                                {
                                    //For getting list of Crop from the table.
                                    products = products.Where(x => x.CropId == objProductFilter.CropId).ToList();
                                }
                                if (objProductFilter.VarietyId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                                }
                                if (!string.IsNullOrEmpty(objProductFilter.State))
                                {
                                    //For getting list of State from the table.
                                    products = products.Where(x => x.StateCode == objProductFilter.State).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.District))
                                {
                                    //For getting list of District from the table.
                                    products = products.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                                {
                                    //For getting list of Taluka from the table.
                                    products = products.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                                }

                                if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                                {
                                    var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                    var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                    products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                                {
                                    //For getting list of Quantity from the table.
                                    products = products.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                                {
                                    var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                    //For getting list of address from the table.
                                    products = products.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                                }
                                objFilterMandiProduct.Products = products.ToList();
                            }

                            #endregion

                            objFilterMandiProduct.Products = products.ToList();

                        }


                        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);

                    }


                    //to get all/filter products for buyer/seller except their own(internal screen)
                    else if (objProductFilter.IsAllActiveProducts == "true" /*&& getUser.UserType == "Buyer"*/)
                    {

                        #region Query

                        var MobileNumber = objProductFilter.MobileNumber;


                        if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                        {
                            var products = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })

                                          .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr })

                                             .Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,

                                               CropName = objProductFilter.culture == "En" ? i.Crop_Master.CropName : objProductFilter.culture == "Hi" ? i.Crop_Master.Hi_CropName == null ? i.Crop_Master.CropName : i.Crop_Master.Hi_CropName : objProductFilter.culture == "Mr" ? i.Crop_Master.Mr_CropName == null ? i.Crop_Master.CropName : i.Crop_Master.Mr_CropName : objProductFilter.culture == "Te" ? i.Crop_Master.Te_CropName == null ? i.Crop_Master.Te_CropName : i.Crop_Master.Te_CropName : i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               ServiceTax = i.Mandi_ProductMaster.ServiceTax,
                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                               // ProductImageUrl = i.Mandi_ProductMaster.ProductImageUrl,
                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               IsApproved = i.Mandi_ProductMaster.IsApproved,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",

                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber != getUserMobileNumber && (x.IsActive == true && x.IsApproved == true)).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();

                            #endregion
                            var test = products.ToList();
                            #region CategoryFilter


                            if (objProductFilter.csvfile != null)
                            {
                                var categories = objProductFilter.csvfile.Table1;
                                if (objProductFilter.FilterByCropId == true)
                                {
                                    if (categories.Count() > 0)
                                    {

                                        var NewProduct = new List<ProductMasterViewModel>();
                                        foreach (var category in categories)
                                        {

                                            var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();

                                            NewProduct.AddRange(product);

                                        }
                                        var catProducts = NewProduct.AsQueryable();
                                        var NewProductCrop = new List<ProductMasterViewModel>();
                                        foreach (var crops in categories)
                                        {

                                            var product = catProducts.Where(x => x.CropId == crops.FCropId).ToList();

                                            NewProductCrop.AddRange(product);

                                        }

                                        products = NewProductCrop.AsQueryable();

                                    }
                                }
                                else
                                {
                                    if (categories.Count() > 0)
                                    {

                                        var NewProduct = new List<ProductMasterViewModel>();
                                        foreach (var category in categories)
                                        {

                                            var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();
                                            if (category.FCropId != null)
                                            {
                                                product = products.Where(x => x.CropId == category.FCropId).ToList();
                                            }
                                            NewProduct.AddRange(product);

                                        }
                                        products = NewProduct.AsQueryable();

                                    }
                                }
                            }


                            #endregion

                            #region Sorting

                            if (objProductFilter.SortProduct == "true")
                            {
                                if (objProductFilter.RecentProduct == "true")
                                {
                                    products = products.OrderByDescending(x => x.Tr_Date);
                                }
                                if (objProductFilter.OldProduct == "true")
                                {
                                    products = products.OrderBy(x => x.Tr_Date);
                                }
                                if (objProductFilter.RecentAvailability == "true")
                                {
                                    products = products.OrderByDescending(x => x.AvailabilityDate);
                                }
                                if (objProductFilter.OldAvailability == "true")
                                {
                                    products = products.OrderBy(x => x.AvailabilityDate);
                                }
                            }
                            #endregion

                            #region Filters

                            if (objProductFilter.IsFilterApplied == "true")
                            {
                                //if (objProductFilter.CropId != null)
                                //{
                                //    //For getting list of Crop from the table.
                                //    products = products.Where(x => x.CropId == objProductFilter.CropId);
                                //}
                                if (objProductFilter.VarietyId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                                }
                                if (objProductFilter.CropId != null)
                                {
                                    //For getting list of Variety from the table.
                                    var productz = products.ToList();
                                    products = products.Where(x => x.CropId == objProductFilter.CropId);
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.State))
                                {
                                    //For getting list of State from the table.
                                    products = products.Where(x => x.StateCode == objProductFilter.State);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.District))
                                {
                                    //For getting list of District from the table.
                                    products = products.Where(x => x.DistrictCode == objProductFilter.District);
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                                {
                                    //For getting list of Taluka from the table.
                                    products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                                }



                                if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                                {
                                    var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                    var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                    products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                                {
                                    //For getting list of Quantity from the table.
                                    products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                                {
                                    var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                    //For getting list of address from the table.
                                    products = products.Where(x => x.IsQualityTestNeeded == Quality);
                                }
                                // var availableProductsOrder = products.Where(x => x.CropStatus=="Available").ToList();
                                //  var soldProductsOrder = products.Where(x => x.CropStatus == "Sold").Take(10).ToList();
                                //  var ProductsOrder = availableProductsOrder.Concat(soldProductsOrder).OrderBy(x => x.CropStatus).ToList();
                                var ProductsOrder = products.Where(x => x.CropStatus == "Available").ToList();
                                objFilterMandiProduct.Products = ProductsOrder.OrderBy(x => x.AvailabilityDate).Skip(skip).Take(take).ToList();
                            }

                            #endregion

                            #region Geo location Filter

                            //foreach (var productItem in products)
                            //{
                            //    if (!string.IsNullOrEmpty(objProductFilter.Latitude) && !string.IsNullOrEmpty(objProductFilter.Longitude))
                            //    {
                            //        string[] values = Convert.ToString(productItem.GeoAddress).Split('/');
                            //        string nearByProduct = Convert.ToString(productItem.GeoAddress);

                            //        double productLatitude = Convert.ToDouble(values[0]);
                            //        double productLongitude = Convert.ToDouble(values[1]);

                            //        //string[] userValues = objProductFilter.GeoAddress.Split('-');
                            //        string dashLAtitude = objProductFilter.Latitude;
                            //        string dashLongitude = objProductFilter.Longitude;


                            //        double UserLatitude = Convert.ToDouble(dashLAtitude);
                            //        double UserLongitude = Convert.ToDouble(dashLongitude);

                            //        double distance = Distance(Convert.ToDouble(UserLatitude), Convert.ToDouble(UserLongitude), Convert.ToDouble(productLatitude), Convert.ToDouble(productLongitude));
                            //        if (distance < 200000)          //nearbyplaces which are within 4 miles 
                            //        {
                            //            listProducts.Add(productItem);
                            //        }

                            //        if (objProductFilter.IsFilterApplied == "true")
                            //        {

                            //            if (objProductFilter.CropId != null)
                            //            {
                            //                //For getting list of Crop from the table.
                            //                listProducts = listProducts.Where(x => x.CropId == objProductFilter.CropId).ToList();
                            //            }
                            //            if (objProductFilter.VarietyId != null)
                            //            {
                            //                //For getting list of Variety from the table.
                            //                listProducts = listProducts.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.State))
                            //            {
                            //                //For getting list of State from the table.
                            //                listProducts = listProducts.Where(x => x.StateCode == objProductFilter.State).ToList();
                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.District))
                            //            {
                            //                //For getting list of District from the table.
                            //                listProducts = listProducts.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                            //            }

                            //            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            //            {
                            //                //For getting list of Taluka from the table.
                            //                listProducts = listProducts.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                            //            }

                            //            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            //            {
                            //                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                            //                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                            //                listProducts = listProducts.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            //            {
                            //                //For getting list of Quantity from the table.
                            //                listProducts = listProducts.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            //            {
                            //                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                            //                //For getting list of address from the table.
                            //                listProducts = listProducts.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                            //            }

                            //        }

                            //    }

                            //    else
                            //    {

                            //        objFilterMandiProduct.Products = products.ToList();
                            //        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                            //    }
                            //    objFilterMandiProduct.Products = listProducts;

                            //}

                            #endregion
                        }
                        else
                        {
                            #region Query

                            var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                                 join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                                 select new ProductMasterViewModel()
                                                 {
                                                     ProductId = (int)pod.Product_Id,
                                                     OrderId = (int)pod.Order_Id,
                                                     MobileNumber = o.Buyer_Mobile



                                                 }).ToList();

                            var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                            var productss = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })

                                          .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr })

                                             .Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,

                                               SCategoryId = i.Crop_Master.CategoryId,
                                               // FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                               
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               Price = i.Mandi_ProductMaster.Price,
                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                               //  ProductImageUrl = i.Mandi_ProductMaster.ProductImageUrl,
                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               // SecondaryProductImage = !string.IsNullOrEmpty(i.Mandi_ProductMaster.SecondaryProductImage) ? i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber != getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();





                            var allProductList = productss.ToList();
                            var getBoughtProductsIds = getProductsIds.ToList();



                            var productds = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();
                            var products = productds.AsQueryable();

                            #endregion


                            #region CategoryFilter



                            if (objProductFilter.csvfile != null)
                            {
                                var categories = objProductFilter.csvfile.Table1;
                                if (objProductFilter.FilterByCropId == true)
                                {
                                    if (categories.Count() > 0)
                                    {

                                        var NewProduct = new List<ProductMasterViewModel>();
                                        foreach (var category in categories)
                                        {

                                            var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();

                                            NewProduct.AddRange(product);

                                        }
                                        var catProducts = NewProduct.AsQueryable();
                                        var NewProductCrop = new List<ProductMasterViewModel>();
                                        foreach (var crops in categories)
                                        {

                                            var product = catProducts.Where(x => x.CropId == crops.FCropId).ToList();

                                            NewProductCrop.AddRange(product);

                                        }

                                        products = NewProductCrop.AsQueryable();

                                    }
                                }
                                else
                                {
                                    if (categories.Count() > 0)
                                    {

                                        var NewProduct = new List<ProductMasterViewModel>();
                                        foreach (var category in categories)
                                        {

                                            var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();
                                            if (category.FCropId != null)
                                            {
                                                product = products.Where(x => x.CropId == category.FCropId).ToList();
                                            }
                                            NewProduct.AddRange(product);

                                        }
                                        products = NewProduct.AsQueryable();

                                    }
                                }
                            }

                            #endregion

                            #region Sorting

                            if (objProductFilter.SortProduct == "true")
                            {
                                if (objProductFilter.RecentProduct == "true")
                                {
                                    products = products.OrderByDescending(x => x.Tr_Date);
                                }
                                if (objProductFilter.OldProduct == "true")
                                {
                                    products = products.OrderBy(x => x.Tr_Date);
                                }
                                if (objProductFilter.RecentAvailability == "true")
                                {
                                    products = products.OrderByDescending(x => x.AvailabilityDate);
                                }
                                if (objProductFilter.OldAvailability == "true")
                                {
                                    products = products.OrderBy(x => x.AvailabilityDate);
                                }
                            }
                            #endregion


                            #region filter

                            if (objProductFilter.IsFilterApplied == "true")
                            {
                                //if (objProductFilter.CropId != null)
                                //{
                                //    //For getting list of Crop from the table.
                                //    products = products.Where(x => x.CropId == objProductFilter.CropId);
                                //}
                                if (objProductFilter.VarietyId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                                }
                                if (!string.IsNullOrEmpty(objProductFilter.State))
                                {
                                    //For getting list of State from the table.
                                    products = products.Where(x => x.StateCode == objProductFilter.State);

                                }
                                if (objProductFilter.CropId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.CropId == objProductFilter.CropId);
                                }
                                if (!string.IsNullOrEmpty(objProductFilter.District))
                                {
                                    //For getting list of District from the table.
                                    products = products.Where(x => x.DistrictCode == objProductFilter.District);
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                                {
                                    //For getting list of Taluka from the table.
                                    products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                                }

                                if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                                {
                                    var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                    var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                    products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                                {
                                    //For getting list of Quantity from the table.
                                    products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                                {
                                    var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                    //For getting list of address from the table.
                                    products = products.Where(x => x.IsQualityTestNeeded == Quality);
                                }
                                //var availableProductsOrder = products.Where(x => x.CropStatus == "Available").ToList();
                                //var soldProductsOrder = products.Where(x => x.CropStatus == "Sold").Take(10).ToList();
                                //var ProductsOrder = availableProductsOrder.Concat(soldProductsOrder).OrderBy(x => x.CropStatus).ToList();
                                //objFilterMandiProduct.Products = ProductsOrder.Skip(skip).Take(take).ToList();
                                var ProductsOrder = products.Where(x => x.CropStatus == "Available").ToList();

                                objFilterMandiProduct.Products = ProductsOrder.OrderBy(x => x.AvailabilityDate).Skip(skip).Take(take).ToList();

                            }

                            #endregion

                            #region Geo location Filter

                            //foreach (var productItem in products)
                            //{
                            //    if (!string.IsNullOrEmpty(objProductFilter.Latitude) && !string.IsNullOrEmpty(objProductFilter.Longitude))
                            //    {
                            //        string[] values = Convert.ToString(productItem.GeoAddress).Split('-');
                            //        string nearByProduct = Convert.ToString(productItem.GeoAddress);

                            //        double productLatitude = Convert.ToDouble(values[0]);
                            //        double productLongitude = Convert.ToDouble(values[1]);

                            //        //string[] userValues = objProductFilter.GeoAddress.Split('-');
                            //        string dashLAtitude = objProductFilter.Latitude;
                            //        string dashLongitude = objProductFilter.Longitude;


                            //        double UserLatitude = Convert.ToDouble(dashLAtitude);
                            //        double UserLongitude = Convert.ToDouble(dashLongitude);

                            //        double distance = Distance(Convert.ToDouble(UserLatitude), Convert.ToDouble(UserLongitude), Convert.ToDouble(productLatitude), Convert.ToDouble(productLongitude));
                            //        if (distance < 200000)          //nearbyplaces which are within 4 miles 
                            //        {
                            //            listProducts.Add(productItem);
                            //        }

                            //        if (objProductFilter.IsFilterApplied == "true")
                            //        {

                            //            if (objProductFilter.CropId != null)
                            //            {
                            //                //For getting list of Crop from the table.
                            //                listProducts = listProducts.Where(x => x.CropId == objProductFilter.CropId).ToList();
                            //            }
                            //            if (objProductFilter.VarietyId != null)
                            //            {
                            //                //For getting list of Variety from the table.
                            //                listProducts = listProducts.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.State))
                            //            {
                            //                //For getting list of State from the table.
                            //                listProducts = listProducts.Where(x => x.StateCode == objProductFilter.State).ToList();
                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.District))
                            //            {
                            //                //For getting list of District from the table.
                            //                listProducts = listProducts.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                            //            }

                            //            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            //            {
                            //                //For getting list of Taluka from the table.
                            //                listProducts = listProducts.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                            //            }

                            //            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            //            {
                            //                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                            //                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                            //                listProducts = listProducts.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            //            {
                            //                //For getting list of Quantity from the table.
                            //                listProducts = listProducts.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            //            {
                            //                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                            //                //For getting list of address from the table.
                            //                listProducts = listProducts.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                            //            }

                            //        }

                            //    }

                            //    else
                            //    {

                            //        objFilterMandiProduct.Products = products.ToList();
                            //        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                            //    }
                            //    objFilterMandiProduct.Products = listProducts;

                            //}


                            #endregion
                        }





                        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                    }
                    objResponse.Message = "Product not found";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
                else
                {
                    //to get Seller's own filter/unfiltered products (Dashboard)
                    if (objProductFilter.IsAllActiveProducts == "false" && getUser.UserType == "Sell")
                    {

                        #region Query

                        var MobileNumber = objProductFilter.MobileNumber;

                        //to check if product is already bought by user or not 
                        if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                        {
                            var products = dbContext.Mandi_ProductMaster
                                      .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                      => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                      .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                      => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,

                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,

                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               ServiceTax = i.Mandi_ProductMaster.ServiceTax,

                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                         .OrderBy(x => x.ProductPriority == "2")
                                         .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();
                            #endregion

                            #region filters
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Crop from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId);
                            }
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                                products = products.Where(x => x.IsQualityTestNeeded == Quality);

                            }
                            #endregion


                            objFilterMandiProduct.Products = products.ToList();
                            return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                        }
                        else
                        {
                            #region Query

                            var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                                 join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                                 select new ProductMasterViewModel()
                                                 {
                                                     ProductId = (int)pod.Product_Id,
                                                     OrderId = (int)pod.Order_Id,
                                                     MobileNumber = o.Buyer_Mobile



                                                 }).ToList();

                            var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                            var productss = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                           .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber != getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();



                            var allProductList = productss.ToList();
                            var getBoughtProductsIds = getProductsIds.ToList();



                            var products = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();
                            #endregion

                            #region filter

                            if (objProductFilter.IsFilterApplied == "true")
                            {
                                if (objProductFilter.CropId != null)
                                {
                                    //For getting list of Crop from the table.
                                    products = products.Where(x => x.CropId == objProductFilter.CropId).ToList();
                                }
                                if (objProductFilter.VarietyId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                                }
                                if (!string.IsNullOrEmpty(objProductFilter.State))
                                {
                                    //For getting list of State from the table.
                                    products = products.Where(x => x.StateCode == objProductFilter.State).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.District))
                                {
                                    //For getting list of District from the table.
                                    products = products.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                                {
                                    //For getting list of Taluka from the table.
                                    products = products.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                                }

                                if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                                {
                                    var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                    var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                    products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                                {
                                    //For getting list of Quantity from the table.
                                    products = products.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                                {
                                    var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                    //For getting list of address from the table.
                                    products = products.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                                }
                                objFilterMandiProduct.Products = products.ToList();
                            }

                            #endregion

                            objFilterMandiProduct.Products = products.ToList();
                            return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                        }

                    }

                    //to get buyer's own filter/unfiltered products (Dashboard)
                    else if (objProductFilter.IsAllActiveProducts == "false" && getUser.UserType == "Buy")
                    {
                        #region Geo location quer
                        var geoAddress = (from user in dbContext.Mandi_ProductMaster
                                          select new
                                          {
                                              user.GeoAddress
                                          }).ToList();
                        #endregion


                        if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                        {

                            #region Query

                            var products = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                           .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               ServiceTax = i.Mandi_ProductMaster.ServiceTax,

                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                              .OrderBy(x => x.ProductPriority == "2")
                                              .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();
                            #endregion

                            #region filter
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Crop from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId);
                            }
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality);


                            }
                            #endregion


                            objFilterMandiProduct.Products = products.ToList();
                        }
                        else
                        {

                            #region Query 
                            // var getOrderIds = (from product in dbContext.Mandi_OrderDetails where product.Buyer_Mobile == objProductFilter.MobileNumber && product.TransactionStatus == "SUCCESS" select product).ToList();

                            var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                                 join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                                 select new ProductMasterViewModel()
                                                 {
                                                     ProductId = (int)pod.Product_Id,
                                                     OrderId = (int)pod.Order_Id,
                                                     MobileNumber = o.Buyer_Mobile



                                                 }).ToList();

                            var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                            var productss = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                           .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                               CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               ServiceTax = i.Mandi_ProductMaster.ServiceTax,

                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                              .OrderBy(x => x.ProductPriority == "2")
                                              .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();


                            var allProductList = productss.ToList();
                            var getBoughtProductsIds = getProductsIds.ToList();



                            var products = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();


                            #endregion

                            #region filter

                            if (objProductFilter.IsFilterApplied == "true")
                            {
                                if (objProductFilter.CropId != null)
                                {
                                    //For getting list of Crop from the table.
                                    products = products.Where(x => x.CropId == objProductFilter.CropId).ToList();
                                }
                                if (objProductFilter.VarietyId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                                }
                                if (!string.IsNullOrEmpty(objProductFilter.State))
                                {
                                    //For getting list of State from the table.
                                    products = products.Where(x => x.StateCode == objProductFilter.State).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.District))
                                {
                                    //For getting list of District from the table.
                                    products = products.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                                {
                                    //For getting list of Taluka from the table.
                                    products = products.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                                }

                                if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                                {
                                    var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                    var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                    products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                                {
                                    //For getting list of Quantity from the table.
                                    products = products.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                                {
                                    var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                    //For getting list of address from the table.
                                    products = products.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                                }
                                objFilterMandiProduct.Products = products.ToList();
                            }

                            #endregion

                            objFilterMandiProduct.Products = products.ToList();

                        }


                        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);

                    }


                    //to get all/filter products for buyer/seller except their own(internal screen)
                    else if (objProductFilter.IsAllActiveProducts == "true" /*&& getUser.UserType == "Buyer"*/)
                    {

                        #region Query

                        var MobileNumber = objProductFilter.MobileNumber;


                        if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                        {
                            var products = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })

                                          .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr })

                                             .Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,

                                               CropName = objProductFilter.culture == "En" ? i.Crop_Master.CropName : objProductFilter.culture == "Hi" ? i.Crop_Master.Hi_CropName == null ? i.Crop_Master.CropName : i.Crop_Master.Hi_CropName : objProductFilter.culture == "Mr" ? i.Crop_Master.Mr_CropName == null ? i.Crop_Master.CropName : i.Crop_Master.Mr_CropName : objProductFilter.culture == "Te" ? i.Crop_Master.Te_CropName == null ? i.Crop_Master.Te_CropName : i.Crop_Master.Te_CropName : i.Crop_Master.CropName,
                                               SCategoryId = i.Crop_Master.CategoryId,
                                               //FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                               CropStatus = i.Mandi_ProductMaster.CropEndDate == null ? "Sold" : i.Mandi_ProductMaster.CropEndDate >= DateTime.Now ? "Available" : "Sold",
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                               Price = i.Mandi_ProductMaster.Price,
                                               ServiceTax = i.Mandi_ProductMaster.ServiceTax,
                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                               // ProductImageUrl = i.Mandi_ProductMaster.ProductImageUrl,
                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               IsApproved = i.Mandi_ProductMaster.IsApproved,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",

                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber != getUserMobileNumber && (x.IsActive == true && x.IsApproved == true)).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();

                            #endregion
                            var test = products.ToList();
                            #region CategoryFilter


                            if (objProductFilter.csvfile != null)
                            {
                                var categories = objProductFilter.csvfile.Table1;
                                if (objProductFilter.FilterByCropId == true)
                                {
                                    if (categories.Count() > 0)
                                    {

                                        var NewProduct = new List<ProductMasterViewModel>();
                                        foreach (var category in categories)
                                        {

                                            var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();

                                            NewProduct.AddRange(product);

                                        }
                                        var catProducts = NewProduct.AsQueryable();
                                        var NewProductCrop = new List<ProductMasterViewModel>();
                                        foreach (var crops in categories)
                                        {

                                            var product = catProducts.Where(x => x.CropId == crops.FCropId).ToList();

                                            NewProductCrop.AddRange(product);

                                        }

                                        products = NewProductCrop.AsQueryable();

                                    }
                                }
                                else
                                {
                                    if (categories.Count() > 0)
                                    {

                                        var NewProduct = new List<ProductMasterViewModel>();
                                        foreach (var category in categories)
                                        {

                                            var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();
                                            if (category.FCropId != null)
                                            {
                                                product = products.Where(x => x.CropId == category.FCropId).ToList();
                                            }
                                            NewProduct.AddRange(product);

                                        }
                                        products = NewProduct.AsQueryable();

                                    }
                                }
                            }


                            #endregion

                            #region Sorting

                            if (objProductFilter.SortProduct == "true")
                            {
                                if (objProductFilter.RecentProduct == "true")
                                {
                                    products = products.OrderByDescending(x => x.Tr_Date);
                                }
                                if (objProductFilter.OldProduct == "true")
                                {
                                    products = products.OrderBy(x => x.Tr_Date);
                                }
                                if (objProductFilter.RecentAvailability == "true")
                                {
                                    products = products.OrderByDescending(x => x.AvailabilityDate);
                                }
                                if (objProductFilter.OldAvailability == "true")
                                {
                                    products = products.OrderBy(x => x.AvailabilityDate);
                                }
                            }
                            #endregion

                            #region Filters

                            if (objProductFilter.IsFilterApplied == "true")
                            {
                                //if (objProductFilter.CropId != null)
                                //{
                                //    //For getting list of Crop from the table.
                                //    products = products.Where(x => x.CropId == objProductFilter.CropId);
                                //}
                                if (objProductFilter.VarietyId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                                }
                                if (objProductFilter.CropId != null)
                                {
                                    //For getting list of Variety from the table.
                                    var productz = products.ToList();
                                    products = products.Where(x => x.CropId == objProductFilter.CropId);
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.State))
                                {
                                    //For getting list of State from the table.
                                    products = products.Where(x => x.StateCode == objProductFilter.State);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.District))
                                {
                                    //For getting list of District from the table.
                                    products = products.Where(x => x.DistrictCode == objProductFilter.District);
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                                {
                                    //For getting list of Taluka from the table.
                                    products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                                }



                                if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                                {
                                    var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                    var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                    products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                                {
                                    //For getting list of Quantity from the table.
                                    products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                                {
                                    var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                    //For getting list of address from the table.
                                    products = products.Where(x => x.IsQualityTestNeeded == Quality);
                                }
                                // var availableProductsOrder = products.Where(x => x.CropStatus=="Available").ToList();
                                //  var soldProductsOrder = products.Where(x => x.CropStatus == "Sold").Take(10).ToList();
                                //  var ProductsOrder = availableProductsOrder.Concat(soldProductsOrder).OrderBy(x => x.CropStatus).ToList();
                                var ProductsOrder = products.Where(x => x.CropStatus == "Available").ToList();
                                objFilterMandiProduct.Products = ProductsOrder.OrderByDescending(x => x.AvailabilityDate).ToList();
                            }

                            #endregion

                            #region Geo location Filter

                            //foreach (var productItem in products)
                            //{
                            //    if (!string.IsNullOrEmpty(objProductFilter.Latitude) && !string.IsNullOrEmpty(objProductFilter.Longitude))
                            //    {
                            //        string[] values = Convert.ToString(productItem.GeoAddress).Split('/');
                            //        string nearByProduct = Convert.ToString(productItem.GeoAddress);

                            //        double productLatitude = Convert.ToDouble(values[0]);
                            //        double productLongitude = Convert.ToDouble(values[1]);

                            //        //string[] userValues = objProductFilter.GeoAddress.Split('-');
                            //        string dashLAtitude = objProductFilter.Latitude;
                            //        string dashLongitude = objProductFilter.Longitude;


                            //        double UserLatitude = Convert.ToDouble(dashLAtitude);
                            //        double UserLongitude = Convert.ToDouble(dashLongitude);

                            //        double distance = Distance(Convert.ToDouble(UserLatitude), Convert.ToDouble(UserLongitude), Convert.ToDouble(productLatitude), Convert.ToDouble(productLongitude));
                            //        if (distance < 200000)          //nearbyplaces which are within 4 miles 
                            //        {
                            //            listProducts.Add(productItem);
                            //        }

                            //        if (objProductFilter.IsFilterApplied == "true")
                            //        {

                            //            if (objProductFilter.CropId != null)
                            //            {
                            //                //For getting list of Crop from the table.
                            //                listProducts = listProducts.Where(x => x.CropId == objProductFilter.CropId).ToList();
                            //            }
                            //            if (objProductFilter.VarietyId != null)
                            //            {
                            //                //For getting list of Variety from the table.
                            //                listProducts = listProducts.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.State))
                            //            {
                            //                //For getting list of State from the table.
                            //                listProducts = listProducts.Where(x => x.StateCode == objProductFilter.State).ToList();
                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.District))
                            //            {
                            //                //For getting list of District from the table.
                            //                listProducts = listProducts.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                            //            }

                            //            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            //            {
                            //                //For getting list of Taluka from the table.
                            //                listProducts = listProducts.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                            //            }

                            //            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            //            {
                            //                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                            //                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                            //                listProducts = listProducts.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            //            {
                            //                //For getting list of Quantity from the table.
                            //                listProducts = listProducts.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            //            {
                            //                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                            //                //For getting list of address from the table.
                            //                listProducts = listProducts.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                            //            }

                            //        }

                            //    }

                            //    else
                            //    {

                            //        objFilterMandiProduct.Products = products.ToList();
                            //        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                            //    }
                            //    objFilterMandiProduct.Products = listProducts;

                            //}

                            #endregion
                        }
                        else
                        {
                            #region Query

                            var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                                 join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                                 select new ProductMasterViewModel()
                                                 {
                                                     ProductId = (int)pod.Product_Id,
                                                     OrderId = (int)pod.Order_Id,
                                                     MobileNumber = o.Buyer_Mobile



                                                 }).ToList();

                            var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                            var productss = dbContext.Mandi_ProductMaster
                                           .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                           => new { Mandi_ProductMaster = cd, Crop_Master = cus })

                                          .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                           => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr })

                                             .Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                           => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                           {

                                               Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                               CropId = i.Mandi_ProductMaster.CropId,
                                               VarietyId = i.Mandi_ProductMaster.VarietyId,
                                               CropName = i.Crop_Master.CropName,

                                               SCategoryId = i.Crop_Master.CategoryId,
                                               // FilterCategoryName = i.Category_Master.CategoryName,
                                               FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                               ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                               VarietyName = i.Variety_Master.VarietyName,
                                               ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                               GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                               MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                               CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                               CropStatus = i.Mandi_ProductMaster.CropEndDate == null ? "Sold" : i.Mandi_ProductMaster.CropEndDate >= DateTime.Now ? "Available" : "Sold",
                                               NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                               Quantity = i.Mandi_ProductMaster.Quantity,
                                               QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,

                                               Price = i.Mandi_ProductMaster.Price,
                                               AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                               PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                               IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                               IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                               //  ProductImageUrl = i.Mandi_ProductMaster.ProductImageUrl,
                                               Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                               StateCode = i.Mandi_ProductMaster.State,
                                               DistrictCode = i.Mandi_ProductMaster.District,
                                               TalukaCode = i.Mandi_ProductMaster.Taluka,
                                               IsActive = i.Mandi_ProductMaster.IsActive,
                                               ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                               ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                               SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               // SecondaryProductImage = !string.IsNullOrEmpty(i.Mandi_ProductMaster.SecondaryProductImage) ? i.Mandi_ProductMaster.SecondaryProductImage : "",
                                               NewVariety = ""

                                           }).Where(x => x.MobileNumber != getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();





                            var allProductList = productss.ToList();
                            var getBoughtProductsIds = getProductsIds.ToList();



                            var productds = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();
                            var products = productds.AsQueryable();

                            #endregion


                            #region CategoryFilter



                            if (objProductFilter.csvfile != null)
                            {
                                var categories = objProductFilter.csvfile.Table1;
                                if (objProductFilter.FilterByCropId == true)
                                {
                                    if (categories.Count() > 0)
                                    {

                                        var NewProduct = new List<ProductMasterViewModel>();
                                        foreach (var category in categories)
                                        {

                                            var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();

                                            NewProduct.AddRange(product);

                                        }
                                        var catProducts = NewProduct.AsQueryable();
                                        var NewProductCrop = new List<ProductMasterViewModel>();
                                        foreach (var crops in categories)
                                        {

                                            var product = catProducts.Where(x => x.CropId == crops.FCropId).ToList();

                                            NewProductCrop.AddRange(product);

                                        }

                                        products = NewProductCrop.AsQueryable();

                                    }
                                }
                                else
                                {
                                    if (categories.Count() > 0)
                                    {

                                        var NewProduct = new List<ProductMasterViewModel>();
                                        foreach (var category in categories)
                                        {

                                            var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();
                                            if (category.FCropId != null)
                                            {
                                                product = products.Where(x => x.CropId == category.FCropId).ToList();
                                            }
                                            NewProduct.AddRange(product);

                                        }
                                        products = NewProduct.AsQueryable();

                                    }
                                }
                            }

                            #endregion

                            #region Sorting

                            if (objProductFilter.SortProduct == "true")
                            {
                                if (objProductFilter.RecentProduct == "true")
                                {
                                    products = products.OrderByDescending(x => x.Tr_Date);
                                }
                                if (objProductFilter.OldProduct == "true")
                                {
                                    products = products.OrderBy(x => x.Tr_Date);
                                }
                                if (objProductFilter.RecentAvailability == "true")
                                {
                                    products = products.OrderByDescending(x => x.AvailabilityDate);
                                }
                                if (objProductFilter.OldAvailability == "true")
                                {
                                    products = products.OrderBy(x => x.AvailabilityDate);
                                }
                            }
                            #endregion


                            #region filter

                            if (objProductFilter.IsFilterApplied == "true")
                            {
                                //if (objProductFilter.CropId != null)
                                //{
                                //    //For getting list of Crop from the table.
                                //    products = products.Where(x => x.CropId == objProductFilter.CropId);
                                //}
                                if (objProductFilter.VarietyId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                                }
                                if (!string.IsNullOrEmpty(objProductFilter.State))
                                {
                                    //For getting list of State from the table.
                                    products = products.Where(x => x.StateCode == objProductFilter.State);

                                }
                                if (objProductFilter.CropId != null)
                                {
                                    //For getting list of Variety from the table.
                                    products = products.Where(x => x.CropId == objProductFilter.CropId);
                                }
                                if (!string.IsNullOrEmpty(objProductFilter.District))
                                {
                                    //For getting list of District from the table.
                                    products = products.Where(x => x.DistrictCode == objProductFilter.District);
                                }

                                if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                                {
                                    //For getting list of Taluka from the table.
                                    products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                                }

                                if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                                {
                                    var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                    var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                    products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                                {
                                    //For getting list of Quantity from the table.
                                    products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                                }
                                if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                                {
                                    var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                    //For getting list of address from the table.
                                    products = products.Where(x => x.IsQualityTestNeeded == Quality);
                                }
                                //var availableProductsOrder = products.Where(x => x.CropStatus == "Available").ToList();
                                //var soldProductsOrder = products.Where(x => x.CropStatus == "Sold").Take(10).ToList();
                                //var ProductsOrder = availableProductsOrder.Concat(soldProductsOrder).OrderBy(x => x.CropStatus).ToList();
                                //objFilterMandiProduct.Products = ProductsOrder.Skip(skip).Take(take).ToList();
                                var ProductsOrder = products.Where(x => x.CropStatus == "Available").ToList();

                                objFilterMandiProduct.Products = ProductsOrder.OrderByDescending(x => x.AvailabilityDate).ToList();

                            }

                            #endregion

                            #region Geo location Filter

                            //foreach (var productItem in products)
                            //{
                            //    if (!string.IsNullOrEmpty(objProductFilter.Latitude) && !string.IsNullOrEmpty(objProductFilter.Longitude))
                            //    {
                            //        string[] values = Convert.ToString(productItem.GeoAddress).Split('-');
                            //        string nearByProduct = Convert.ToString(productItem.GeoAddress);

                            //        double productLatitude = Convert.ToDouble(values[0]);
                            //        double productLongitude = Convert.ToDouble(values[1]);

                            //        //string[] userValues = objProductFilter.GeoAddress.Split('-');
                            //        string dashLAtitude = objProductFilter.Latitude;
                            //        string dashLongitude = objProductFilter.Longitude;


                            //        double UserLatitude = Convert.ToDouble(dashLAtitude);
                            //        double UserLongitude = Convert.ToDouble(dashLongitude);

                            //        double distance = Distance(Convert.ToDouble(UserLatitude), Convert.ToDouble(UserLongitude), Convert.ToDouble(productLatitude), Convert.ToDouble(productLongitude));
                            //        if (distance < 200000)          //nearbyplaces which are within 4 miles 
                            //        {
                            //            listProducts.Add(productItem);
                            //        }

                            //        if (objProductFilter.IsFilterApplied == "true")
                            //        {

                            //            if (objProductFilter.CropId != null)
                            //            {
                            //                //For getting list of Crop from the table.
                            //                listProducts = listProducts.Where(x => x.CropId == objProductFilter.CropId).ToList();
                            //            }
                            //            if (objProductFilter.VarietyId != null)
                            //            {
                            //                //For getting list of Variety from the table.
                            //                listProducts = listProducts.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.State))
                            //            {
                            //                //For getting list of State from the table.
                            //                listProducts = listProducts.Where(x => x.StateCode == objProductFilter.State).ToList();
                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.District))
                            //            {
                            //                //For getting list of District from the table.
                            //                listProducts = listProducts.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                            //            }

                            //            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            //            {
                            //                //For getting list of Taluka from the table.
                            //                listProducts = listProducts.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                            //            }

                            //            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            //            {
                            //                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                            //                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                            //                listProducts = listProducts.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            //            {
                            //                //For getting list of Quantity from the table.
                            //                listProducts = listProducts.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                            //            }
                            //            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            //            {
                            //                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                            //                //For getting list of address from the table.
                            //                listProducts = listProducts.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                            //            }

                            //        }

                            //    }

                            //    else
                            //    {

                            //        objFilterMandiProduct.Products = products.ToList();
                            //        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                            //    }
                            //    objFilterMandiProduct.Products = listProducts;

                            //}


                            #endregion
                        }





                        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                    }
                    objResponse.Message = "Product not found";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
            }





            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUSerController", "GetProduct");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }







        //to get allsold Products
        [HttpPost]
        // [Authorize]
        [Route("api/MandiUser/GetAllSoldProduct")]
        public HttpResponseMessage GetAllSoldProduct(ProductFilter objProductFilter)
        {
            try
            {
                var ServerPath = ConfigurationManager.AppSettings["ServerPath"];
                int counter = objProductFilter.counter;
                int take = 6;
                int skip = counter;

                var categoryId = objProductFilter.SCategoryId;
                var culture = objProductFilter.culture;
                MandiProduct objListMandiProduct = new MandiProduct();
                List<ProductMasterViewModel> objListProductMasterViewModel = new List<ProductMasterViewModel>();
                var getUser = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == objProductFilter.MobileNumber select user).FirstOrDefault();
                string mobileNumber = objProductFilter.MobileNumber;
                var getUserMobileNumber = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user.MobileNumber).FirstOrDefault();
                var getOrderHistoryDetail = (from product in dbContext.Mandi_OrderDetails where product.Buyer_Mobile == objProductFilter.MobileNumber && product.TransactionStatus == "Submitted" || product.TransactionStatus == "SUCCESS" select product).ToList();
                FilterMandiProduct objFilterMandiProduct = new FilterMandiProduct();
                List<ProductMasterViewModel> listProducts = new List<ProductMasterViewModel>();


                //to get Seller's own filter/unfiltered products (Dashboard)
                if (objProductFilter.IsAllActiveProducts == "false" && getUser.UserType == "Sell")
                {

                    #region Query

                    var MobileNumber = objProductFilter.MobileNumber;

                    //to check if product is already bought by user or not 
                    if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                    {
                        var products = dbContext.Mandi_ProductMaster
                                  .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                  => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                  .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                  => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,

                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           ServiceTax = i.Mandi_ProductMaster.ServiceTax,

                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                     .OrderBy(x => x.ProductPriority == "2")
                                     .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();
                        #endregion

                        #region filters
                        if (objProductFilter.CropId != null)
                        {
                            //For getting list of Crop from the table.
                            products = products.Where(x => x.CropId == objProductFilter.CropId);
                        }
                        if (objProductFilter.VarietyId != null)
                        {
                            //For getting list of Variety from the table.
                            products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                        }
                        if (!string.IsNullOrEmpty(objProductFilter.State))
                        {
                            //For getting list of State from the table.
                            products = products.Where(x => x.StateCode == objProductFilter.State);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.District))
                        {
                            //For getting list of District from the table.
                            products = products.Where(x => x.DistrictCode == objProductFilter.District);
                        }

                        if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                        {
                            //For getting list of Taluka from the table.
                            products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                        }

                        if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                        {
                            var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                            var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                            products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                        {
                            //For getting list of Quantity from the table.
                            products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                        {
                            var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                            products = products.Where(x => x.IsQualityTestNeeded == Quality);

                        }
                        #endregion


                        objFilterMandiProduct.Products = products.Skip(skip).Take(take).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                    }
                    else
                    {
                        #region Query

                        var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                             join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                             select new ProductMasterViewModel()
                                             {
                                                 ProductId = (int)pod.Product_Id,
                                                 OrderId = (int)pod.Order_Id,
                                                 MobileNumber = o.Buyer_Mobile



                                             }).ToList();

                        var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                        var productss = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                       .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,

                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber != getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                      .OrderBy(x => x.ProductPriority == "2")
                                      .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();



                        var allProductList = productss.ToList();
                        var getBoughtProductsIds = getProductsIds.ToList();



                        var products = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();
                        #endregion

                        #region filter

                        if (objProductFilter.IsFilterApplied == "true")
                        {
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Crop from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId).ToList();
                            }
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                            }
                            objFilterMandiProduct.Products = products.ToList();
                        }

                        #endregion

                        objFilterMandiProduct.Products = products.Skip(skip).Take(take).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                    }

                }

                //to get buyer's own filter/unfiltered products (Dashboard)
                else if (objProductFilter.IsAllActiveProducts == "false" && getUser.UserType == "Buy")
                {
                    #region Geo location quer
                    var geoAddress = (from user in dbContext.Mandi_ProductMaster
                                      select new
                                      {
                                          user.GeoAddress
                                      }).ToList();
                    #endregion


                    if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                    {

                        #region Query

                        var products = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                       .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           ServiceTax = i.Mandi_ProductMaster.ServiceTax,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();
                        #endregion

                        #region filter
                        if (objProductFilter.CropId != null)
                        {
                            //For getting list of Crop from the table.
                            products = products.Where(x => x.CropId == objProductFilter.CropId);
                        }
                        if (objProductFilter.VarietyId != null)
                        {
                            //For getting list of Variety from the table.
                            products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                        }
                        if (!string.IsNullOrEmpty(objProductFilter.State))
                        {
                            //For getting list of State from the table.
                            products = products.Where(x => x.StateCode == objProductFilter.State);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.District))
                        {
                            //For getting list of District from the table.
                            products = products.Where(x => x.DistrictCode == objProductFilter.District);
                        }

                        if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                        {
                            //For getting list of Taluka from the table.
                            products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                        }

                        if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                        {
                            var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                            var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                            products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                        {
                            //For getting list of Quantity from the table.
                            products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                        {
                            var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                            //For getting list of address from the table.
                            products = products.Where(x => x.IsQualityTestNeeded == Quality);


                        }
                        #endregion


                        objFilterMandiProduct.Products = products.ToList();
                    }
                    else
                    {

                        #region Query 
                        // var getOrderIds = (from product in dbContext.Mandi_OrderDetails where product.Buyer_Mobile == objProductFilter.MobileNumber && product.TransactionStatus == "SUCCESS" select product).ToList();

                        var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                             join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                             select new ProductMasterViewModel()
                                             {
                                                 ProductId = (int)pod.Product_Id,
                                                 OrderId = (int)pod.Order_Id,
                                                 MobileNumber = o.Buyer_Mobile



                                             }).ToList();

                        var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                        var productss = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                       .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           ServiceTax = i.Mandi_ProductMaster.ServiceTax,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();


                        var allProductList = productss.ToList();
                        var getBoughtProductsIds = getProductsIds.ToList();



                        var products = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();


                        #endregion

                        #region filter

                        if (objProductFilter.IsFilterApplied == "true")
                        {
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Crop from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId).ToList();
                            }
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                            }
                            objFilterMandiProduct.Products = products.ToList();
                        }

                        #endregion

                        objFilterMandiProduct.Products = products.ToList();

                    }


                    return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);

                }


                //to get all/filter products for buyer/seller except their own(internal screen)
                else if (objProductFilter.IsAllActiveProducts == "true" /*&& getUser.UserType == "Buyer"*/)
                {

                    #region Query

                    var MobileNumber = objProductFilter.MobileNumber;


                    if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                    {
                        var products = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })

                                      .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr })

                                         .Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,

                                           CropName = objProductFilter.culture == "En" ? i.Crop_Master.CropName : objProductFilter.culture == "Hi" ? i.Crop_Master.Hi_CropName == null ? i.Crop_Master.CropName : i.Crop_Master.Hi_CropName : objProductFilter.culture == "Mr" ? i.Crop_Master.Mr_CropName == null ? i.Crop_Master.CropName : i.Crop_Master.Mr_CropName : objProductFilter.culture == "Te" ? i.Crop_Master.Te_CropName == null ? i.Crop_Master.Te_CropName : i.Crop_Master.Te_CropName : i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           ServiceTax = i.Mandi_ProductMaster.ServiceTax,
                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                           // ProductImageUrl = i.Mandi_ProductMaster.ProductImageUrl,
                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           IsApproved = i.Mandi_ProductMaster.IsApproved,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",

                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber != getUserMobileNumber && (x.IsActive == true && x.IsApproved == true)).OrderBy(x => x.ProductPriority == "1")
                                      .OrderBy(x => x.ProductPriority == "2")
                                      .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();

                        #endregion
                        var test = products.ToList();
                        #region CategoryFilter



                        var categories = objProductFilter.csvfile.Table1;
                        if (objProductFilter.FilterByCropId == true)
                        {
                            if (categories.Count() > 0)
                            {

                                var NewProduct = new List<ProductMasterViewModel>();
                                foreach (var category in categories)
                                {

                                    var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();

                                    NewProduct.AddRange(product);

                                }
                                var catProducts = NewProduct.AsQueryable();
                                var NewProductCrop = new List<ProductMasterViewModel>();
                                foreach (var crops in categories)
                                {

                                    var product = catProducts.Where(x => x.CropId == crops.FCropId).ToList();

                                    NewProductCrop.AddRange(product);

                                }

                                products = NewProductCrop.AsQueryable();

                            }
                        }
                        else
                        {
                            if (categories.Count() > 0)
                            {

                                var NewProduct = new List<ProductMasterViewModel>();
                                foreach (var category in categories)
                                {

                                    var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();
                                    if (category.FCropId != null)
                                    {
                                        product = products.Where(x => x.CropId == category.FCropId).ToList();
                                    }
                                    NewProduct.AddRange(product);

                                }
                                products = NewProduct.AsQueryable();

                            }
                        }

                        #endregion

                        #region Sorting

                        if (objProductFilter.SortProduct == "true")
                        {
                            if (objProductFilter.RecentProduct == "true")
                            {
                                products = products.OrderByDescending(x => x.Tr_Date);
                            }
                            if (objProductFilter.OldProduct == "true")
                            {
                                products = products.OrderBy(x => x.Tr_Date);
                            }
                            if (objProductFilter.RecentAvailability == "true")
                            {
                                products = products.OrderByDescending(x => x.AvailabilityDate);
                            }
                            if (objProductFilter.OldAvailability == "true")
                            {
                                products = products.OrderBy(x => x.AvailabilityDate);
                            }
                        }
                        #endregion

                        #region Filters

                        if (objProductFilter.IsFilterApplied == "true")
                        {
                            //if (objProductFilter.CropId != null)
                            //{
                            //    //For getting list of Crop from the table.
                            //    products = products.Where(x => x.CropId == objProductFilter.CropId);
                            //}
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                            }
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Variety from the table.
                                var productz = products.ToList();
                                products = products.Where(x => x.CropId == objProductFilter.CropId);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                            }



                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality);
                            }
                            // var availableProductsOrder = products.Where(x => x.CropStatus=="Available").ToList();
                            //  var soldProductsOrder = products.Where(x => x.CropStatus == "Sold").Take(10).ToList();
                            //  var ProductsOrder = availableProductsOrder.Concat(soldProductsOrder).OrderBy(x => x.CropStatus).ToList();
                            var ProductsOrder = products.Where(x => x.CropStatus == "Sold").ToList();
                            objFilterMandiProduct.Products = ProductsOrder.Skip(skip).Take(10).ToList();
                        }

                        #endregion

                        #region Geo location Filter

                        //foreach (var productItem in products)
                        //{
                        //    if (!string.IsNullOrEmpty(objProductFilter.Latitude) && !string.IsNullOrEmpty(objProductFilter.Longitude))
                        //    {
                        //        string[] values = Convert.ToString(productItem.GeoAddress).Split('/');
                        //        string nearByProduct = Convert.ToString(productItem.GeoAddress);

                        //        double productLatitude = Convert.ToDouble(values[0]);
                        //        double productLongitude = Convert.ToDouble(values[1]);

                        //        //string[] userValues = objProductFilter.GeoAddress.Split('-');
                        //        string dashLAtitude = objProductFilter.Latitude;
                        //        string dashLongitude = objProductFilter.Longitude;


                        //        double UserLatitude = Convert.ToDouble(dashLAtitude);
                        //        double UserLongitude = Convert.ToDouble(dashLongitude);

                        //        double distance = Distance(Convert.ToDouble(UserLatitude), Convert.ToDouble(UserLongitude), Convert.ToDouble(productLatitude), Convert.ToDouble(productLongitude));
                        //        if (distance < 200000)          //nearbyplaces which are within 4 miles 
                        //        {
                        //            listProducts.Add(productItem);
                        //        }

                        //        if (objProductFilter.IsFilterApplied == "true")
                        //        {

                        //            if (objProductFilter.CropId != null)
                        //            {
                        //                //For getting list of Crop from the table.
                        //                listProducts = listProducts.Where(x => x.CropId == objProductFilter.CropId).ToList();
                        //            }
                        //            if (objProductFilter.VarietyId != null)
                        //            {
                        //                //For getting list of Variety from the table.
                        //                listProducts = listProducts.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                        //            }
                        //            if (!string.IsNullOrEmpty(objProductFilter.State))
                        //            {
                        //                //For getting list of State from the table.
                        //                listProducts = listProducts.Where(x => x.StateCode == objProductFilter.State).ToList();
                        //            }
                        //            if (!string.IsNullOrEmpty(objProductFilter.District))
                        //            {
                        //                //For getting list of District from the table.
                        //                listProducts = listProducts.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                        //            }

                        //            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                        //            {
                        //                //For getting list of Taluka from the table.
                        //                listProducts = listProducts.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                        //            }

                        //            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                        //            {
                        //                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                        //                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                        //                listProducts = listProducts.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                        //            }
                        //            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                        //            {
                        //                //For getting list of Quantity from the table.
                        //                listProducts = listProducts.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                        //            }
                        //            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                        //            {
                        //                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                        //                //For getting list of address from the table.
                        //                listProducts = listProducts.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                        //            }

                        //        }

                        //    }

                        //    else
                        //    {

                        //        objFilterMandiProduct.Products = products.ToList();
                        //        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                        //    }
                        //    objFilterMandiProduct.Products = listProducts;

                        //}

                        #endregion
                    }
                    else
                    {
                        #region Query

                        var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                             join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                             select new ProductMasterViewModel()
                                             {
                                                 ProductId = (int)pod.Product_Id,
                                                 OrderId = (int)pod.Order_Id,
                                                 MobileNumber = o.Buyer_Mobile



                                             }).ToList();

                        var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                        var productss = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })

                                      .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr })

                                         .Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,

                                           SCategoryId = i.Crop_Master.CategoryId,
                                           // FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                           //CropStatus = i.Mandi_ProductMaster.CropEndDate == null ? "Sold" : i.Mandi_ProductMaster.CropEndDate >= DateTime.Now ? "Available" : "Sold",
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,

                                           Price = i.Mandi_ProductMaster.Price,
                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                           //  ProductImageUrl = i.Mandi_ProductMaster.ProductImageUrl,
                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           // SecondaryProductImage = !string.IsNullOrEmpty(i.Mandi_ProductMaster.SecondaryProductImage) ? i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber != getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                      .OrderBy(x => x.ProductPriority == "2")
                                      .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();





                        var allProductList = productss.ToList();
                        var getBoughtProductsIds = getProductsIds.ToList();



                        var productds = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();
                        var products = productds.AsQueryable();

                        #endregion


                        #region CategoryFilter



                        var categories = objProductFilter.csvfile.Table1;
                        if (objProductFilter.FilterByCropId == true)
                        {
                            if (categories.Count() > 0)
                            {

                                var NewProduct = new List<ProductMasterViewModel>();
                                foreach (var category in categories)
                                {

                                    var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();

                                    NewProduct.AddRange(product);

                                }
                                var catProducts = NewProduct.AsQueryable();
                                var NewProductCrop = new List<ProductMasterViewModel>();
                                foreach (var crops in categories)
                                {

                                    var product = catProducts.Where(x => x.CropId == crops.FCropId).ToList();

                                    NewProductCrop.AddRange(product);

                                }

                                products = NewProductCrop.AsQueryable();

                            }
                        }
                        else
                        {
                            if (categories.Count() > 0)
                            {

                                var NewProduct = new List<ProductMasterViewModel>();
                                foreach (var category in categories)
                                {

                                    var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();
                                    if (category.FCropId != null)
                                    {
                                        product = products.Where(x => x.CropId == category.FCropId).ToList();
                                    }
                                    NewProduct.AddRange(product);

                                }
                                products = NewProduct.AsQueryable();

                            }
                        }

                        #endregion

                        #region Sorting

                        if (objProductFilter.SortProduct == "true")
                        {
                            if (objProductFilter.RecentProduct == "true")
                            {
                                products = products.OrderByDescending(x => x.Tr_Date);
                            }
                            if (objProductFilter.OldProduct == "true")
                            {
                                products = products.OrderBy(x => x.Tr_Date);
                            }
                            if (objProductFilter.RecentAvailability == "true")
                            {
                                products = products.OrderByDescending(x => x.AvailabilityDate);
                            }
                            if (objProductFilter.OldAvailability == "true")
                            {
                                products = products.OrderBy(x => x.AvailabilityDate);
                            }
                        }
                        #endregion


                        #region filter

                        if (objProductFilter.IsFilterApplied == "true")
                        {
                            //if (objProductFilter.CropId != null)
                            //{
                            //    //For getting list of Crop from the table.
                            //    products = products.Where(x => x.CropId == objProductFilter.CropId);
                            //}
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State);

                            }
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId);
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality);
                            }
                            //var availableProductsOrder = products.Where(x => x.CropStatus == "Available").ToList();
                            //var soldProductsOrder = products.Where(x => x.CropStatus == "Sold").Take(10).ToList();
                            //var ProductsOrder = availableProductsOrder.Concat(soldProductsOrder).OrderBy(x => x.CropStatus).ToList();
                            //objFilterMandiProduct.Products = ProductsOrder.Skip(skip).Take(take).ToList();
                            var ProductsOrder = products.Where(x => x.CropStatus == "Sold").ToList();
                            objFilterMandiProduct.Products = ProductsOrder.Skip(skip).Take(10).ToList();

                        }

                        #endregion

                        #region Geo location Filter

                        //foreach (var productItem in products)
                        //{
                        //    if (!string.IsNullOrEmpty(objProductFilter.Latitude) && !string.IsNullOrEmpty(objProductFilter.Longitude))
                        //    {
                        //        string[] values = Convert.ToString(productItem.GeoAddress).Split('-');
                        //        string nearByProduct = Convert.ToString(productItem.GeoAddress);

                        //        double productLatitude = Convert.ToDouble(values[0]);
                        //        double productLongitude = Convert.ToDouble(values[1]);

                        //        //string[] userValues = objProductFilter.GeoAddress.Split('-');
                        //        string dashLAtitude = objProductFilter.Latitude;
                        //        string dashLongitude = objProductFilter.Longitude;


                        //        double UserLatitude = Convert.ToDouble(dashLAtitude);
                        //        double UserLongitude = Convert.ToDouble(dashLongitude);

                        //        double distance = Distance(Convert.ToDouble(UserLatitude), Convert.ToDouble(UserLongitude), Convert.ToDouble(productLatitude), Convert.ToDouble(productLongitude));
                        //        if (distance < 200000)          //nearbyplaces which are within 4 miles 
                        //        {
                        //            listProducts.Add(productItem);
                        //        }

                        //        if (objProductFilter.IsFilterApplied == "true")
                        //        {

                        //            if (objProductFilter.CropId != null)
                        //            {
                        //                //For getting list of Crop from the table.
                        //                listProducts = listProducts.Where(x => x.CropId == objProductFilter.CropId).ToList();
                        //            }
                        //            if (objProductFilter.VarietyId != null)
                        //            {
                        //                //For getting list of Variety from the table.
                        //                listProducts = listProducts.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                        //            }
                        //            if (!string.IsNullOrEmpty(objProductFilter.State))
                        //            {
                        //                //For getting list of State from the table.
                        //                listProducts = listProducts.Where(x => x.StateCode == objProductFilter.State).ToList();
                        //            }
                        //            if (!string.IsNullOrEmpty(objProductFilter.District))
                        //            {
                        //                //For getting list of District from the table.
                        //                listProducts = listProducts.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                        //            }

                        //            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                        //            {
                        //                //For getting list of Taluka from the table.
                        //                listProducts = listProducts.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                        //            }

                        //            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                        //            {
                        //                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                        //                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                        //                listProducts = listProducts.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                        //            }
                        //            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                        //            {
                        //                //For getting list of Quantity from the table.
                        //                listProducts = listProducts.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                        //            }
                        //            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                        //            {
                        //                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                        //                //For getting list of address from the table.
                        //                listProducts = listProducts.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                        //            }

                        //        }

                        //    }

                        //    else
                        //    {

                        //        objFilterMandiProduct.Products = products.ToList();
                        //        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                        //    }
                        //    objFilterMandiProduct.Products = listProducts;

                        //}


                        #endregion
                    }





                    return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                }
                objResponse.Message = "Product not found";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUSerController", "GetProduct");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        [HttpPost]
        // [Authorize]
        [Route("api/MandiUser/GetAllComingSoonProduct")]
        public HttpResponseMessage GetAllComingSoonProduct(ProductFilter objProductFilter)
        {
            try
            {
                var ServerPath = ConfigurationManager.AppSettings["ServerPath"];
                int counter = objProductFilter.counter;
                int take = 6;
                int skip = counter;

                var categoryId = objProductFilter.SCategoryId;
                var culture = objProductFilter.culture;
                MandiProduct objListMandiProduct = new MandiProduct();
                List<ProductMasterViewModel> objListProductMasterViewModel = new List<ProductMasterViewModel>();
                var getUser = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == objProductFilter.MobileNumber select user).FirstOrDefault();
                string mobileNumber = objProductFilter.MobileNumber;
                var getUserMobileNumber = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user.MobileNumber).FirstOrDefault();
                var getOrderHistoryDetail = (from product in dbContext.Mandi_OrderDetails where product.Buyer_Mobile == objProductFilter.MobileNumber && product.TransactionStatus == "Submitted" || product.TransactionStatus == "SUCCESS" select product).ToList();
                FilterMandiProduct objFilterMandiProduct = new FilterMandiProduct();
                List<ProductMasterViewModel> listProducts = new List<ProductMasterViewModel>();


                //to get Seller's own filter/unfiltered products (Dashboard)
                if (objProductFilter.IsAllActiveProducts == "false" && getUser.UserType == "Sell")
                {

                    #region Query

                    var MobileNumber = objProductFilter.MobileNumber;

                    //to check if product is already bought by user or not 
                    if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                    {
                        var products = dbContext.Mandi_ProductMaster
                                  .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                  => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                  .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                  => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,

                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           ServiceTax = i.Mandi_ProductMaster.ServiceTax,

                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                     .OrderBy(x => x.ProductPriority == "2")
                                     .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();
                        #endregion

                        #region filters
                        if (objProductFilter.CropId != null)
                        {
                            //For getting list of Crop from the table.
                            products = products.Where(x => x.CropId == objProductFilter.CropId);
                        }
                        if (objProductFilter.VarietyId != null)
                        {
                            //For getting list of Variety from the table.
                            products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                        }
                        if (!string.IsNullOrEmpty(objProductFilter.State))
                        {
                            //For getting list of State from the table.
                            products = products.Where(x => x.StateCode == objProductFilter.State);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.District))
                        {
                            //For getting list of District from the table.
                            products = products.Where(x => x.DistrictCode == objProductFilter.District);
                        }

                        if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                        {
                            //For getting list of Taluka from the table.
                            products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                        }

                        if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                        {
                            var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                            var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                            products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                        {
                            //For getting list of Quantity from the table.
                            products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                        {
                            var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                            products = products.Where(x => x.IsQualityTestNeeded == Quality);

                        }
                        #endregion


                        objFilterMandiProduct.Products = products.Skip(skip).Take(take).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                    }
                    else
                    {
                        #region Query

                        var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                             join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                             select new ProductMasterViewModel()
                                             {
                                                 ProductId = (int)pod.Product_Id,
                                                 OrderId = (int)pod.Order_Id,
                                                 MobileNumber = o.Buyer_Mobile



                                             }).ToList();

                        var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                        var productss = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                       .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber != getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                      .OrderBy(x => x.ProductPriority == "2")
                                      .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();



                        var allProductList = productss.ToList();
                        var getBoughtProductsIds = getProductsIds.ToList();



                        var products = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();
                        #endregion

                        #region filter

                        if (objProductFilter.IsFilterApplied == "true")
                        {
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Crop from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId).ToList();
                            }
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                            }
                            objFilterMandiProduct.Products = products.ToList();
                        }

                        #endregion

                        objFilterMandiProduct.Products = products.Skip(skip).Take(take).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                    }

                }

                //to get buyer's own filter/unfiltered products (Dashboard)
                else if (objProductFilter.IsAllActiveProducts == "false" && getUser.UserType == "Buy")
                {
                    #region Geo location quer
                    var geoAddress = (from user in dbContext.Mandi_ProductMaster
                                      select new
                                      {
                                          user.GeoAddress
                                      }).ToList();
                    #endregion


                    if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                    {

                        #region Query

                        var products = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                       .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           ServiceTax = i.Mandi_ProductMaster.ServiceTax,

                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();
                        #endregion

                        #region filter
                        if (objProductFilter.CropId != null)
                        {
                            //For getting list of Crop from the table.
                            products = products.Where(x => x.CropId == objProductFilter.CropId);
                        }
                        if (objProductFilter.VarietyId != null)
                        {
                            //For getting list of Variety from the table.
                            products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                        }
                        if (!string.IsNullOrEmpty(objProductFilter.State))
                        {
                            //For getting list of State from the table.
                            products = products.Where(x => x.StateCode == objProductFilter.State);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.District))
                        {
                            //For getting list of District from the table.
                            products = products.Where(x => x.DistrictCode == objProductFilter.District);
                        }

                        if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                        {
                            //For getting list of Taluka from the table.
                            products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                        }

                        if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                        {
                            var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                            var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                            products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                        {
                            //For getting list of Quantity from the table.
                            products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                        }
                        if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                        {
                            var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);
                            //For getting list of address from the table.
                            products = products.Where(x => x.IsQualityTestNeeded == Quality);


                        }
                        #endregion


                        objFilterMandiProduct.Products = products.ToList();
                    }
                    else
                    {

                        #region Query 
                        // var getOrderIds = (from product in dbContext.Mandi_OrderDetails where product.Buyer_Mobile == objProductFilter.MobileNumber && product.TransactionStatus == "SUCCESS" select product).ToList();

                        var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                             join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                             select new ProductMasterViewModel()
                                             {
                                                 ProductId = (int)pod.Product_Id,
                                                 OrderId = (int)pod.Order_Id,
                                                 MobileNumber = o.Buyer_Mobile



                                             }).ToList();

                        var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                        var productss = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })
                                       .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr }).Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           ServiceTax = i.Mandi_ProductMaster.ServiceTax,

                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,

                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,

                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber == getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                          .OrderBy(x => x.ProductPriority == "2")
                                          .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).Skip(skip).Take(take).AsQueryable();


                        var allProductList = productss.ToList();
                        var getBoughtProductsIds = getProductsIds.ToList();



                        var products = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();


                        #endregion

                        #region filter

                        if (objProductFilter.IsFilterApplied == "true")
                        {
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Crop from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId).ToList();
                            }
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId).ToList();
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District).ToList();
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka).ToList();

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity).ToList();

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality).ToList();
                            }
                            objFilterMandiProduct.Products = products.ToList();
                        }

                        #endregion

                        objFilterMandiProduct.Products = products.ToList();

                    }


                    return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);

                }


                //to get all/filter products for buyer/seller except their own(internal screen)
                else if (objProductFilter.IsAllActiveProducts == "true" /*&& getUser.UserType == "Buyer"*/)
                {

                    #region Query

                    var MobileNumber = objProductFilter.MobileNumber;


                    if (getOrderHistoryDetail == null || getOrderHistoryDetail.Count == 0)
                    {
                        var products = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })

                                      .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr })

                                         .Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,

                                           CropName = objProductFilter.culture == "En" ? i.Crop_Master.CropName : objProductFilter.culture == "Hi" ? i.Crop_Master.Hi_CropName == null ? i.Crop_Master.CropName : i.Crop_Master.Hi_CropName : objProductFilter.culture == "Mr" ? i.Crop_Master.Mr_CropName == null ? i.Crop_Master.CropName : i.Crop_Master.Mr_CropName : objProductFilter.culture == "Te" ? i.Crop_Master.Te_CropName == null ? i.Crop_Master.Te_CropName : i.Crop_Master.Te_CropName : i.Crop_Master.CropName,
                                           SCategoryId = i.Crop_Master.CategoryId,
                                           //FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,
                                           Price = i.Mandi_ProductMaster.Price,
                                           ServiceTax = i.Mandi_ProductMaster.ServiceTax,
                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                           // ProductImageUrl = i.Mandi_ProductMaster.ProductImageUrl,
                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           IsApproved = i.Mandi_ProductMaster.IsApproved,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",

                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber != getUserMobileNumber && (x.IsActive == true && x.IsApproved == true)).OrderBy(x => x.ProductPriority == "1")
                                      .OrderBy(x => x.ProductPriority == "2")
                                      .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();

                        #endregion
                        var test = products.ToList();
                        #region CategoryFilter



                        var categories = objProductFilter.csvfile.Table1;
                        if (objProductFilter.FilterByCropId == true)
                        {
                            if (categories.Count() > 0)
                            {

                                var NewProduct = new List<ProductMasterViewModel>();
                                foreach (var category in categories)
                                {

                                    var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();

                                    NewProduct.AddRange(product);

                                }
                                var catProducts = NewProduct.AsQueryable();
                                var NewProductCrop = new List<ProductMasterViewModel>();
                                foreach (var crops in categories)
                                {

                                    var product = catProducts.Where(x => x.CropId == crops.FCropId).ToList();

                                    NewProductCrop.AddRange(product);

                                }

                                products = NewProductCrop.AsQueryable();

                            }
                        }
                        else
                        {
                            if (categories.Count() > 0)
                            {

                                var NewProduct = new List<ProductMasterViewModel>();
                                foreach (var category in categories)
                                {

                                    var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();
                                    if (category.FCropId != null)
                                    {
                                        product = products.Where(x => x.CropId == category.FCropId).ToList();
                                    }
                                    NewProduct.AddRange(product);

                                }
                                products = NewProduct.AsQueryable();

                            }
                        }

                        #endregion

                        #region Sorting

                        if (objProductFilter.SortProduct == "true")
                        {
                            if (objProductFilter.RecentProduct == "true")
                            {
                                products = products.OrderByDescending(x => x.Tr_Date);
                            }
                            if (objProductFilter.OldProduct == "true")
                            {
                                products = products.OrderBy(x => x.Tr_Date);
                            }
                            if (objProductFilter.RecentAvailability == "true")
                            {
                                products = products.OrderByDescending(x => x.AvailabilityDate);
                            }
                            if (objProductFilter.OldAvailability == "true")
                            {
                                products = products.OrderBy(x => x.AvailabilityDate);
                            }
                        }
                        #endregion

                        #region Filters

                        if (objProductFilter.IsFilterApplied == "true")
                        {
                            //if (objProductFilter.CropId != null)
                            //{
                            //    //For getting list of Crop from the table.
                            //    products = products.Where(x => x.CropId == objProductFilter.CropId);
                            //}
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                            }
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Variety from the table.
                                var productz = products.ToList();
                                products = products.Where(x => x.CropId == objProductFilter.CropId);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                            }



                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality);
                            }
                            // var availableProductsOrder = products.Where(x => x.CropStatus=="Available").ToList();
                            //  var soldProductsOrder = products.Where(x => x.CropStatus == "Sold").Take(10).ToList();
                            //  var ProductsOrder = availableProductsOrder.Concat(soldProductsOrder).OrderBy(x => x.CropStatus).ToList();
                            var ProductsOrder = products.Where(x => x.CropStatus == "Coming Soon").ToList();
                            objFilterMandiProduct.Products = ProductsOrder.Skip(skip).Take(10).ToList();
                        }

                        #endregion

                        
                    }
                    else
                    {
                        #region Query

                        var getProductsId = (from o in dbContext.Mandi_OrderDetails
                                             join pod in dbContext.Mandi_OrderProductDetails on o.Order_Id equals pod.Order_Id

                                             select new ProductMasterViewModel()
                                             {
                                                 ProductId = (int)pod.Product_Id,
                                                 OrderId = (int)pod.Order_Id,
                                                 MobileNumber = o.Buyer_Mobile



                                             }).ToList();

                        var getProductsIds = getProductsId.Where(x => x.MobileNumber == objProductFilter.MobileNumber).ToList();

                        var productss = dbContext.Mandi_ProductMaster
                                       .Join(dbContext.Crop_Master, cd => cd.CropId, cus => cus.CropId, (cd, cus)
                                       => new { Mandi_ProductMaster = cd, Crop_Master = cus })

                                      .Join(dbContext.Variety_Master, x => x.Mandi_ProductMaster.VarietyId, cr => cr.VarietyId, (x, cr)
                                       => new { x.Mandi_ProductMaster, x.Crop_Master, Variety_Master = cr })

                                         .Join(dbContext.Category, c => c.Mandi_ProductMaster.CategoryId, cat => cat.CategoryId, (c, cat)
                                       => new { c.Mandi_ProductMaster, c.Crop_Master, c.Variety_Master, Category_Master = cat }).Select(i => new ProductMasterViewModel()
                                       {

                                           Tr_Id = i.Mandi_ProductMaster.Tr_Id,
                                           CropId = i.Mandi_ProductMaster.CropId,
                                           VarietyId = i.Mandi_ProductMaster.VarietyId,
                                           CropName = i.Crop_Master.CropName,

                                           SCategoryId = i.Crop_Master.CategoryId,
                                           // FilterCategoryName = i.Category_Master.CategoryName,
                                           FilterCategoryName = objProductFilter.culture == "En" ? i.Category_Master.CategoryName : objProductFilter.culture == "Hi" ? i.Category_Master.Hi_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Hi_CategoryName : objProductFilter.culture == "Mr" ? i.Category_Master.Mr_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Mr_CategoryName : objProductFilter.culture == "Te" ? i.Category_Master.Te_CategoryName == null ? i.Category_Master.CategoryName : i.Category_Master.Te_CategoryName : i.Category_Master.CategoryName,
                                           ProductDescription = objProductFilter.culture == "En" ? i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Hi" ? i.Mandi_ProductMaster.Hi_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Hi_ProductDescription : objProductFilter.culture == "Mr" ? i.Mandi_ProductMaster.Mr_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.ProductDescription : objProductFilter.culture == "Te" ? i.Mandi_ProductMaster.Te_ProductDescription == null ? i.Mandi_ProductMaster.ProductDescription : i.Mandi_ProductMaster.Te_ProductDescription : i.Mandi_ProductMaster.ProductDescription,
                                           VarietyName = i.Variety_Master.VarietyName,
                                           ProductAddress = i.Mandi_ProductMaster.ProductAddress,
                                           GeoAddress = i.Mandi_ProductMaster.GeoAddress,
                                           MobileNumber = i.Mandi_ProductMaster.MobileNumber,
                                           CropEndDate = i.Mandi_ProductMaster.CropEndDate,
                                           CropStatus = i.Mandi_ProductMaster.Product_Status == null ? "Sold" : i.Mandi_ProductMaster.Product_Status,
                                           NetBankingId = i.Mandi_ProductMaster.NetBankingId,
                                           Quantity = i.Mandi_ProductMaster.Quantity,
                                           QuantityUnit = i.Mandi_ProductMaster.QuantityUnit,

                                           Price = i.Mandi_ProductMaster.Price,
                                           AvailabilityDate = i.Mandi_ProductMaster.AvailabilityDate,
                                           PaymentMethod = i.Mandi_ProductMaster.PaymentMethod,
                                           IsQualityTestNeeded = i.Mandi_ProductMaster.IsQualityTestNeeded,
                                           IsLogisticNeeded = i.Mandi_ProductMaster.IsLogisticNeeded,
                                           //  ProductImageUrl = i.Mandi_ProductMaster.ProductImageUrl,
                                           Tr_Date = i.Mandi_ProductMaster.Tr_Date,
                                           StateCode = i.Mandi_ProductMaster.State,
                                           DistrictCode = i.Mandi_ProductMaster.District,
                                           TalukaCode = i.Mandi_ProductMaster.Taluka,
                                           IsActive = i.Mandi_ProductMaster.IsActive,
                                           ProductPriority = i.Mandi_ProductMaster.ProductPriority,
                                           ProductImageUrl = ServerPath + i.Mandi_ProductMaster.ProductImageUrl,
                                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + i.Mandi_ProductMaster.SecondaryProductImage) ? ServerPath + i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           // SecondaryProductImage = !string.IsNullOrEmpty(i.Mandi_ProductMaster.SecondaryProductImage) ? i.Mandi_ProductMaster.SecondaryProductImage : "",
                                           NewVariety = ""

                                       }).Where(x => x.MobileNumber != getUserMobileNumber && x.IsActive == true).OrderBy(x => x.ProductPriority == "1")
                                      .OrderBy(x => x.ProductPriority == "2")
                                      .OrderBy(x => x.ProductPriority == "0").OrderByDescending(x => x.Tr_Date).AsQueryable();





                        var allProductList = productss.ToList();
                        var getBoughtProductsIds = getProductsIds.ToList();



                        var productds = allProductList.Where(x => !getBoughtProductsIds.Any(y => y.ProductId == x.Tr_Id)).ToList();
                        var products = productds.AsQueryable();

                        #endregion


                        #region CategoryFilter



                        var categories = objProductFilter.csvfile.Table1;
                        if (objProductFilter.FilterByCropId == true)
                        {
                            if (categories.Count() > 0)
                            {

                                var NewProduct = new List<ProductMasterViewModel>();
                                foreach (var category in categories)
                                {

                                    var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();

                                    NewProduct.AddRange(product);

                                }
                                var catProducts = NewProduct.AsQueryable();
                                var NewProductCrop = new List<ProductMasterViewModel>();
                                foreach (var crops in categories)
                                {

                                    var product = catProducts.Where(x => x.CropId == crops.FCropId).ToList();

                                    NewProductCrop.AddRange(product);

                                }

                                products = NewProductCrop.AsQueryable();

                            }
                        }
                        else
                        {
                            if (categories.Count() > 0)
                            {

                                var NewProduct = new List<ProductMasterViewModel>();
                                foreach (var category in categories)
                                {

                                    var product = products.Where(x => x.SCategoryId == category.SCategoryId).ToList();
                                    if (category.FCropId != null)
                                    {
                                        product = products.Where(x => x.CropId == category.FCropId).ToList();
                                    }
                                    NewProduct.AddRange(product);

                                }
                                products = NewProduct.AsQueryable();

                            }
                        }

                        #endregion

                        #region Sorting

                        if (objProductFilter.SortProduct == "true")
                        {
                            if (objProductFilter.RecentProduct == "true")
                            {
                                products = products.OrderByDescending(x => x.Tr_Date);
                            }
                            if (objProductFilter.OldProduct == "true")
                            {
                                products = products.OrderBy(x => x.Tr_Date);
                            }
                            if (objProductFilter.RecentAvailability == "true")
                            {
                                products = products.OrderByDescending(x => x.AvailabilityDate);
                            }
                            if (objProductFilter.OldAvailability == "true")
                            {
                                products = products.OrderBy(x => x.AvailabilityDate);
                            }
                        }
                        #endregion


                        #region filter

                        if (objProductFilter.IsFilterApplied == "true")
                        {
                            //if (objProductFilter.CropId != null)
                            //{
                            //    //For getting list of Crop from the table.
                            //    products = products.Where(x => x.CropId == objProductFilter.CropId);
                            //}
                            if (objProductFilter.VarietyId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.VarietyId == objProductFilter.VarietyId);
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.State))
                            {
                                //For getting list of State from the table.
                                products = products.Where(x => x.StateCode == objProductFilter.State);

                            }
                            if (objProductFilter.CropId != null)
                            {
                                //For getting list of Variety from the table.
                                products = products.Where(x => x.CropId == objProductFilter.CropId);
                            }
                            if (!string.IsNullOrEmpty(objProductFilter.District))
                            {
                                //For getting list of District from the table.
                                products = products.Where(x => x.DistrictCode == objProductFilter.District);
                            }

                            if (!string.IsNullOrEmpty(objProductFilter.Taluka))
                            {
                                //For getting list of Taluka from the table.
                                products = products.Where(x => x.TalukaCode == objProductFilter.Taluka);

                            }

                            if (!string.IsNullOrEmpty(objProductFilter.MaxPrice) && !string.IsNullOrEmpty(objProductFilter.MinPrice))
                            {
                                var minPrice = Convert.ToInt32(objProductFilter.MinPrice);
                                var maxPrice = Convert.ToInt32(objProductFilter.MaxPrice);
                                products = products.Where(x => x.Price >= minPrice && x.Price <= maxPrice);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.Quantity))
                            {
                                //For getting list of Quantity from the table.
                                products = products.Where(x => x.Quantity == objProductFilter.Quantity);

                            }
                            if (!string.IsNullOrEmpty(objProductFilter.IsQualityTestNeeded) && objProductFilter.IsQualityTestNeeded != "false")
                            {
                                var Quality = Convert.ToBoolean(objProductFilter.IsQualityTestNeeded);

                                //For getting list of address from the table.
                                products = products.Where(x => x.IsQualityTestNeeded == Quality);
                            }
                           
                            var ProductsOrder = products.Where(x => x.CropStatus == "Coming Soon").ToList();
                            objFilterMandiProduct.Products = ProductsOrder.Skip(skip).Take(10).ToList();

                        }

                        #endregion

                        
                    }





                    return Request.CreateResponse(HttpStatusCode.OK, objFilterMandiProduct);
                }
                objResponse.Message = "Product not found";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUSerController", "GetProduct");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }



        [Route("api/MandiUser/ImageUpload")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> ImageUpload()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {


                var httpRequest = HttpContext.Current.Request;
                if (httpRequest != null)
                {

                    foreach (string file in httpRequest.Files)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                        var postedFile = httpRequest.Files[file];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {

                            int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                            IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                            var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                            var extension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(extension))
                            {

                                var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                                dict.Add("error", message);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                            }
                            else if (postedFile.ContentLength > MaxContentLength)
                            {

                                var message = string.Format("Please Upload a file upto 1 mb.");

                                dict.Add("error", message);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                            }
                            else
                            {

                                var filePath = HttpContext.Current.Server.MapPath("~/Images/Products/" + postedFile.FileName);
                                postedFile.SaveAs(filePath);


                            }

                        }


                    }
                    var message1 = string.Format("Image Updated Successfully.");
                    return Request.CreateErrorResponse(HttpStatusCode.Created, message1);
                }
                else
                {
                    var res = string.Format("Please Upload a image.");
                    dict.Add("error", res);
                    return Request.CreateResponse(HttpStatusCode.NotFound, dict);

                }
            }
            catch (Exception ex)
            {
                var res = ex;
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }

















        /// <summary>
        /// / 
        /// </summary>
        /// <param name="categoryID"></param>
        /// <param name="Culture"></param>
        /// <returns></returns>
        public string GetCategoryNameBasedonCategoryId(long categoryID = 0, string Culture = "")
        {
            try
            {
                string getCategoryName = "";
                if (categoryID != 0)
                {
                    if (Culture == "En")
                    {
                        getCategoryName = (from cat in dbContext.Category where cat.CategoryId == categoryID select cat.CategoryName == "" ? cat.CategoryName : cat.CategoryName).FirstOrDefault();
                    }
                    else if (Culture == "Hi")
                    {
                        getCategoryName = (from cat in dbContext.Category where cat.CategoryId == categoryID select cat.CategoryName == "" ? cat.CategoryName : cat.Hi_CategoryName).FirstOrDefault();
                    }
                    else if (Culture == "Mr")
                    {
                        getCategoryName = (from cat in dbContext.Category where cat.CategoryId == categoryID select cat.CategoryName == "" ? cat.CategoryName : cat.Mr_CategoryName).FirstOrDefault();
                    }

                    else
                    {
                        getCategoryName = (from cat in dbContext.Category where cat.CategoryId == categoryID select cat.CategoryName == "" ? cat.CategoryName : cat.Te_CategoryName).FirstOrDefault();

                    }

                    return getCategoryName;
                }
                else
                {
                    return "N/A";
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// For getting product details
        /// </summary>
        /// <param name="objProductDetail"></param>
        /// <returns></returns>
        [HttpPost]
        // [Authorize]
        [Route("api/MandiUser/GetProductDetails")]
        public HttpResponseMessage GetProductDetails(ProductDetail objProductDetail)
        {
            try
            {
                var ServerPath = ConfigurationManager.AppSettings["ServerPath"];
                string mobileNumber = objProductDetail.MobileNumber;
                int productId = objProductDetail.Tr_Id;

                var sellerProduct = (from product in dbContext.Mandi_ProductMaster.Where(product => product.Tr_Id == productId)
                                     join crop in dbContext.Crop_Master on product.CropId equals crop.CropId
                                     join variety in dbContext.Variety_Master on product.VarietyId equals variety.VarietyId


                                     select new
                                     {

                                         crop.CropName,
                                         variety.VarietyName,
                                         product.Tr_Id,
                                         product.CropId,
                                         product.VarietyId,
                                         product.ProductAddress,
                                         product.GeoAddress,
                                         product.MobileNumber,
                                         product.Quantity,
                                         product.QuantityUnit,
                                         product.Price,
                                         product.ServiceTax,
                                         product.CategoryId,
                                         crop.CategoryName,

                                         product.AvailabilityDate,
                                         product.IsQualityTestNeeded,
                                         product.ProductImageUrl,
                                         product.Tr_Date,
                                         product.State,
                                         product.District,
                                         product.Taluka,
                                         product.PaymentMethod,
                                         product.IsLogisticNeeded,
                                         product.IsActive,
                                         product.SecondaryProductImage,
                                         product.ProductDescription

                                     }).FirstOrDefault();

                //var sellerProduct = (from product in dbContext.Mandi_ProductMaster where product.MobileNumber == mobileNumber && product.Tr_Id == productId select product).FirstOrDefault();

                if (sellerProduct != null)
                {
                    ProductInfo objProductInfo = new ProductInfo()
                    {
                        Tr_Id = sellerProduct.Tr_Id,
                        MobileNumber = sellerProduct.MobileNumber,
                        CropId = sellerProduct.CropId,
                        VarietyId = sellerProduct.VarietyId,
                        CategoryName = sellerProduct.CategoryName,
                        CropName = sellerProduct.CropName,
                        //CategoryName = sellerProduct.CategoryName == "" ? "N/A" : sellerProduct.CategoryName,
                        CategoryId = sellerProduct.CategoryId,
                        VarietyName = sellerProduct.VarietyName,
                        ProductAddress = sellerProduct.ProductAddress,
                        GeoAddress = sellerProduct.GeoAddress,
                        Quantity = sellerProduct.Quantity,
                        QuantityUnit = sellerProduct.QuantityUnit,
                        Price = sellerProduct.Price,
                        ServiceTax = sellerProduct.Price == null ? 0 : sellerProduct.ServiceTax,

                        AvailabilityDate = sellerProduct.AvailabilityDate,
                        IsQualityTestNeeded = sellerProduct.IsQualityTestNeeded,
                        //NetBankingId=sellerProduct.NetBankingId,
                        StateCode = sellerProduct.State,
                        DistrictCode = sellerProduct.District,
                        TalukaCode = sellerProduct.Taluka,
                        ProductImageUrl = ServerPath + sellerProduct.ProductImageUrl,
                        PaymentMethod = sellerProduct.PaymentMethod,
                        IsLogisticNeeded = sellerProduct.IsLogisticNeeded,
                        IsActive = sellerProduct.IsActive,
                        SecondaryProductImage = sellerProduct.SecondaryProductImage,
                        ProductDescription = sellerProduct.ProductDescription


                    };
                    return Request.CreateResponse(HttpStatusCode.OK, objProductInfo);
                }
                objResponse.Message = "product Not Exist";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUserController", "GetProductDetails");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        /// <summary>
        /// For Update Product.
        /// </summary>
        /// <param name="objProductMasterViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/MandiUser/UpdateProduct")]
        public HttpResponseMessage UpdateProduct(ProductMasterViewModel objProductMasterViewModel)
        {
            try
            {
                int productId = objProductMasterViewModel.Tr_Id;


                Mandi_ProductMaster objMandi_ProductMaster = new Mandi_ProductMaster();

                string mobileNumber = objProductMasterViewModel.MobileNumber;
                //get mobileNumber from user table
                //var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                var sellerProduct = (from product in dbContext.Mandi_ProductMaster where product.Tr_Id == productId select product).FirstOrDefault();
                if (sellerProduct != null)
                {

                    sellerProduct.CropId = objProductMasterViewModel.CropId;
                    sellerProduct.VarietyId = objProductMasterViewModel.VarietyId;
                    sellerProduct.ProductAddress = objProductMasterViewModel.ProductAddress;
                    sellerProduct.GeoAddress = objProductMasterViewModel.GeoAddress;
                    sellerProduct.MobileNumber = objProductMasterViewModel.MobileNumber;
                    sellerProduct.NetBankingId = objProductMasterViewModel.NetBankingId;
                    sellerProduct.Quantity = objProductMasterViewModel.Quantity;
                    sellerProduct.QuantityUnit = objProductMasterViewModel.QuantityUnit;
                    sellerProduct.Price = objProductMasterViewModel.Price;

                    sellerProduct.ProductDescription = objProductMasterViewModel.ProductDescription;
                    sellerProduct.AvailabilityDate = objProductMasterViewModel.AvailabilityDate;
                    sellerProduct.PaymentMethod = objProductMasterViewModel.PaymentMethod;
                    sellerProduct.IsQualityTestNeeded = objProductMasterViewModel.IsQualityTestNeeded;
                    sellerProduct.IsLogisticNeeded = objProductMasterViewModel.IsLogisticNeeded;
                    sellerProduct.ProductImageUrl = objProductMasterViewModel.ProductImageUrl;
                    //sellerProduct.Tr_Date = DateTime.Now;
                    sellerProduct.State = objProductMasterViewModel.StateCode;
                    sellerProduct.District = objProductMasterViewModel.DistrictCode;
                    sellerProduct.Taluka = objProductMasterViewModel.TalukaCode;
                    sellerProduct.SecondaryProductImage = objProductMasterViewModel.SecondaryProductImage;

                    //to modify product in database.
                    dbContext.Entry(sellerProduct).State = EntityState.Modified;

                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "Product Updated successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Product Updation Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "No product Found";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "UpdateProduct");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        /// <summary>
        /// For Deletion of Product.
        /// </summary>
        /// <param name="objProductMasterViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/MandiUser/DeleteProduct")]
        public HttpResponseMessage DeleteProduct(ProductMasterViewModel objProductMasterViewModel)
        {
            try
            {
                int productId = objProductMasterViewModel.Tr_Id;


                Mandi_ProductMaster objMandi_ProductMaster = new Mandi_ProductMaster();

                string mobileNumber = objProductMasterViewModel.MobileNumber;
                //get product from database.
                var sellerProduct = (from product in dbContext.Mandi_ProductMaster where product.Tr_Id == productId select product).FirstOrDefault();
                if (sellerProduct != null)
                {
                    sellerProduct.IsActive = false;
                    sellerProduct.Product_Status = "Sold";
                    dbContext.Entry(sellerProduct).State = EntityState.Modified;
                    //dbContext.Mandi_ProductMaster.Remove(sellerProduct);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "Product Deactivated successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Product Deactivation Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "No product Found";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "DeleteProduct");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        [HttpGet]
        [Authorize]
        [Route("api/MandiUser/GetPickupAddresstBasedOnProductId")]
        public HttpResponseMessage GetPickupAddresstBasedOnProductId(int ProductId = 0)
        {
            try
            {



                var getPickUpAddress = (from product in dbContext.Mandi_ProductMaster where product.Tr_Id == ProductId select product.ProductAddress).FirstOrDefault();
                if (getPickUpAddress != null)
                {
                    ProductMasterViewModel objProductMasterViewModel = new ProductMasterViewModel()
                    {
                        ProductAddress = getPickUpAddress


                    };
                    return Request.CreateResponse(HttpStatusCode.OK, getPickUpAddress);
                }
                else
                {

                    objResponse.Message = "Pickup-address does not exist for particular product.";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }

            }

            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUserController", "GetPickupAddresstBasedOnProductId");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }



        #endregion

        #region Interested Product

        [HttpPost]
        //[Authorize]
        [Route("api/MandiUser/AddInterestedProduct")]
        public string AddInterestedProductForUser(InterestedProductsViewModel objInterestedProductsViewModel)
        {
            try
            {
                string mobileNumber = objInterestedProductsViewModel.BuyerId;
                Mandi_InterestedProductForUser objMandi_InterestedProductForUser = new Mandi_InterestedProductForUser();

                //CHECK RECORD EXIXT IN DB 
                var interestedProduct = (from product in dbContext.Mandi_InterestedProductForUser where product.Fk_MobileNumber == mobileNumber && product.ProductId == objInterestedProductsViewModel.ProductId select product).FirstOrDefault();
                if (interestedProduct != null)
                {

                    var checkEnquiryInCart = (from interest in dbContext.Mandi_CartInfo where interest.Fk_InterestedProductId == interestedProduct.Id && interest.BuyerNumber == mobileNumber && interest.Product_Id == objInterestedProductsViewModel.Tr_Id select interest).FirstOrDefault();
                    if (checkEnquiryInCart != null)
                    {
                        string MessageInterest = "This Product with productId is " + objInterestedProductsViewModel.ProductId + " already added in Cart";
                        return MessageInterest;
                    }
                    else
                    {
                        var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                        if (number != null)
                        {
                            Mandi_CartInfo objMandi_CartInfo = new Mandi_CartInfo();

                            objMandi_CartInfo.CartType = "InterestedProduct";
                            objMandi_CartInfo.Fk_InterestedProductId = interestedProduct.Id;


                            //get Product detail



                            #region Product detail
                            var productId = objInterestedProductsViewModel.Tr_Id;
                            var getProductDetail = (from product in dbContext.Mandi_ProductMaster.Where(product => product.Tr_Id == productId)
                                                    join crop in dbContext.Crop_Master on product.CropId equals crop.CropId
                                                    join variety in dbContext.Variety_Master on product.VarietyId equals variety.VarietyId


                                                    select new
                                                    {

                                                        crop.CropName,
                                                        product.Tr_Id,
                                                        product.VarietyId,
                                                        product.ProductAddress,
                                                        product.GeoAddress,
                                                        product.MobileNumber,
                                                        product.Quantity,
                                                        product.QuantityUnit,
                                                        product.Price,
                                                        product.ProductImageUrl


                                                    }).FirstOrDefault();


                            objMandi_CartInfo.Product = getProductDetail.CropName;
                            objMandi_CartInfo.Product_Id = objInterestedProductsViewModel.Tr_Id;
                            objMandi_CartInfo.Quantity = getProductDetail.Quantity;
                            objMandi_CartInfo.QuantityUnit = getProductDetail.QuantityUnit;
                            objMandi_CartInfo.Price = getProductDetail.Price;
                            objMandi_CartInfo.ProductAddress = getProductDetail.ProductAddress;
                            objMandi_CartInfo.ProductImage = getProductDetail.ProductImageUrl;

                            #endregion




                            //get buyer details
                            objMandi_CartInfo.BuyerName = objInterestedProductsViewModel.BuyerName;
                            objMandi_CartInfo.BuyerNumber = objInterestedProductsViewModel.BuyerContact;
                            objMandi_CartInfo.BuyerAddress = number.State + "," + number.District + "," + number.Taluka + "," + number.Pincode;

                            //to get seller details

                            objMandi_CartInfo.Seller_MobileNumber = getProductDetail.MobileNumber;
                            var sellerMobileNumber = getProductDetail.MobileNumber;
                            var sellerDetail = (from seller in dbContext.Mandi_UserInfo where seller.MobileNumber == sellerMobileNumber select seller).FirstOrDefault();
                            objMandi_CartInfo.SellerAddress = sellerDetail.State + "," + sellerDetail.District + "," + sellerDetail.Taluka + "," + sellerDetail.Pincode;
                            objMandi_CartInfo.SellerName = sellerDetail.FullName;

                            objMandi_CartInfo.Fk_ProductStatus = 4;






                            dbContext.Mandi_CartInfo.Add(objMandi_CartInfo);
                            var i = dbContext.SaveChanges();
                            if (i != 0)
                            {
                                string Message = "This Product with productId is " + objInterestedProductsViewModel.ProductId + " already added in Cart";
                                return Message;

                            }
                            else
                            {
                                string Message = "This Product with productId is " + objInterestedProductsViewModel.ProductId + " already added in Cart";
                                return Message;

                            }

                        }
                        else
                        {
                            string FailMessage = "Mobile number not exists.";
                            return FailMessage;
                        }
                    }

                }
                else
                {
                    var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                    if (number != null)
                    {


                        objMandi_InterestedProductForUser.Id = objInterestedProductsViewModel.Id;
                        objMandi_InterestedProductForUser.Fk_MobileNumber = objInterestedProductsViewModel.BuyerId;
                        objMandi_InterestedProductForUser.BuyerId = objInterestedProductsViewModel.BuyerId;
                        objMandi_InterestedProductForUser.ProductId = objInterestedProductsViewModel.ProductId;
                        objMandi_InterestedProductForUser.CreatedDate = DateTime.Now;
                        objMandi_InterestedProductForUser.BuyerAddress = objInterestedProductsViewModel.BuyerAddress;
                        objMandi_InterestedProductForUser.CropName = objInterestedProductsViewModel.CropName;
                        objMandi_InterestedProductForUser.VarietyName = objInterestedProductsViewModel.VarietyName;
                        objMandi_InterestedProductForUser.Quantity = objInterestedProductsViewModel.Quantity;
                        objMandi_InterestedProductForUser.QualitySpecification = objInterestedProductsViewModel.QualitySpecification;
                        objMandi_InterestedProductForUser.DeliveryLocation = objInterestedProductsViewModel.DeliveryLocation;
                        objMandi_InterestedProductForUser.ExpectedPrice = objInterestedProductsViewModel.ExpectedPrice;
                        objMandi_InterestedProductForUser.IsPriceNegotiable = objInterestedProductsViewModel.IsPriceNegotiable;
                        objMandi_InterestedProductForUser.Remarks = objInterestedProductsViewModel.Remarks;
                        objMandi_InterestedProductForUser.ProductId = objInterestedProductsViewModel.Tr_Id.ToString();


                        var add = dbContext.Mandi_InterestedProductForUser.Add(objMandi_InterestedProductForUser);
                        var i = dbContext.SaveChanges();

                        if (i != 0)
                        {
                            //objResponse.Message = "Interested Product Added successfully";
                            //return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            var getSellerMobileNumber = (from user in dbContext.Mandi_ProductMaster where user.Tr_Id == objInterestedProductsViewModel.Tr_Id select user.MobileNumber).FirstOrDefault();
                            var getSellerMobileNumberByNumber = (from seller in dbContext.Mandi_UserInfo where seller.MobileNumber == getSellerMobileNumber select seller).FirstOrDefault();


                            string Title = "Buyer is interested in your product";
                            OrderBookingViewModel objOrderBookingViewModel = new OrderBookingViewModel();

                            if (getSellerMobileNumberByNumber != null)
                            {
                                if (getSellerMobileNumberByNumber.DeviceToken != null)
                                {
                                    objOrderBookingViewModel.Buyer_Mobile = getSellerMobileNumberByNumber.MobileNumber;
                                    string Message = "Dear Customer, " + getSellerMobileNumberByNumber.FullName + " Buyers from " + getSellerMobileNumberByNumber.State + "," + getSellerMobileNumberByNumber.District + " region have shown interest in your product. ";
                                    var addnotification = AddNotification(objOrderBookingViewModel, Message);
                                    if (addnotification == "true")
                                    {

                                        SendFCMNotificationToUsers(getSellerMobileNumberByNumber.DeviceToken, Message, Title);

                                        objResponse.Message = add.Id.ToString();
                                        return objResponse.Message;
                                    }
                                    else
                                    {

                                        //objResponse.Message = "Interested product has been saved in db but notification is not added";
                                        objResponse.Message = add.Id.ToString();
                                        return objResponse.Message;

                                    }

                                }
                                else
                                {
                                    //return true;
                                    // string Message = "Interested product has been successfully saved in db but Device Token  is null";
                                    string Message = add.Id.ToString();
                                    return Message;
                                }
                            }
                            else
                            {

                                //string Message = "Interested product has been saved";
                                string Message = add.Id.ToString();
                                return Message;
                            }


                        }
                        else
                        {


                            // string Message = "Interested product failed to save in database. Please contact admin";
                            string Message = add.Id.ToString();
                            return Message;
                        }

                    }
                    else
                    {
                        string Message = "User number does not exists.";

                        return Message;

                    }
                }

                //get mobileNumber from user table

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "AddInterestedProduct");
                return "Error has been occured";
                //return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);

            }

        }

        #endregion

        #region Mandi_Banner


        [HttpGet]
        // [Authorize]
        [Route("api/MandiUser/GetMandiBanner")]
        public HttpResponseMessage GetMandiBanner(string ImageType = "")
        {
            try
            {
                Mandi_Banner objMandi_Banner = new Mandi_Banner();
                string BannerImageType = ImageType.Trim();


                Mandi_BannerViewModel Mandi_BannerViewModel = new Mandi_BannerViewModel();
                List<MandiBanners> objListMandiBanners = new List<MandiBanners>();


                if (ImageType == "HomePage")
                {
                    var getBannerDetail = (from banner in dbContext.Mandi_Banner where banner.IsActive == true select banner).ToList();
                    if (getBannerDetail != null)
                    {
                        foreach (var i in getBannerDetail)
                        {
                            MandiBanners objMandiBanners = new MandiBanners()
                            {


                                Id = i.Id,
                                BannerTitle = i.BannerTitle,
                                Description = i.Description,
                                BannerImage = "http://growmandi.growindigo.co.in/api/Images/Banner/" + i.BannerImage,
                                IsDefault = i.IsDefault,
                                IsActive = i.IsActive,
                                ImageType = i.ImageType,
                            };
                            objListMandiBanners.Add(objMandiBanners);
                        }
                        Mandi_BannerViewModel.Mandi_Banner = objListMandiBanners.OrderBy(x => x.IsDefault).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, Mandi_BannerViewModel);

                    }
                    else
                    {
                        objResponse.Message = "No Banner found for particular Type.";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "No Banner found ";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }




            }

            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUserController", "GetUserBankAccountDetails");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }
        #endregion

        #region User_CategoryMapping

        [HttpGet]
        // [Authorize]
        [Route("api/MandiUser/GetUserCategoryMappingDetail")]
        public HttpResponseMessage GetUserCategoryMappingDetail(string mobileNumber = "")
        {
            try
            {
                UserCategoryMapping objUserCategoryMapping = new UserCategoryMapping();
                string MobileNumber = mobileNumber.Trim();
                UserCategoryList objUserCategoryList = new UserCategoryList();

                var UserCategory = dbContext.UserCategoryMapping.Select(i => new UserCategoryMappingViewModel()
                {
                    Id = i.Id,
                    CategoryId = i.CategoryId,
                    CategoryName = i.CategoryName,
                    Fk_MobileNumber = i.Fk_MobileNumber,
                    CreatedDate = i.CreatedDate

                }).Where(x => x.Fk_MobileNumber == mobileNumber).AsQueryable();

                //var getUserCategoryDetails = (from user in dbContext.UserCategoryMapping where user.Fk_MobileNumber == MobileNumber select user).ToList();
                if (UserCategory.Count() > 0)
                {
                    objUserCategoryList.UserCategory = UserCategory.ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, objUserCategoryList);
                }

                else
                {
                    objResponse.Message = "No Category found for particular user.";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }



            }

            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUserController", "GetUserBankAccountDetails");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }


        [HttpPost]
        // [Authorize]
        [Route("api/MandiUser/AddUserCategoryMappingDetail")]
        public HttpResponseMessage AddUserCategoryMapping(UserCategoryMappingViewModel objUserCategoryMappingViewModel)
        {


            try
            {
                var i = 0;
                SuccessResponse objResponse = new SuccessResponse();
                //get mobileNumber from user table
                string mobileNumber = objUserCategoryMappingViewModel.Fk_MobileNumber;
                var number = (from user in dbContext.UserCategoryMapping where user.Fk_MobileNumber == mobileNumber select user).ToList();
                //Update
                if (number.Count() > 0)
                {
                    var getuserCategory = (from user in dbContext.UserCategoryMapping where user.Fk_MobileNumber == mobileNumber select user).ToList();
                    foreach (var c in getuserCategory)
                    {
                        dbContext.UserCategoryMapping.Remove(c);
                        i = dbContext.SaveChanges();
                    }
                    if (i != 0)
                    {
                        var getcsvfile = objUserCategoryMappingViewModel.csvfile.Table1;
                        UserCategoryMapping objUserCategoryMapping = new UserCategoryMapping();
                        foreach (var j in getcsvfile)
                        {
                            //objUserCategoryMapping.Id = objUserCategoryMappingViewModel.Id;
                            objUserCategoryMapping.Fk_MobileNumber = j.Fk_MobileNumber;
                            objUserCategoryMapping.CategoryId = j.CategoryId;
                            objUserCategoryMapping.CategoryName = j.CategoryName;
                            objUserCategoryMapping.ModifiedDate = DateTime.Now;


                            dbContext.Entry(objUserCategoryMapping).State = EntityState.Added;
                            i = dbContext.SaveChanges();
                            dbContext.Entry(objUserCategoryMapping).State = EntityState.Detached;
                        }
                        if (i != 0)
                        {
                            objResponse.Message = "Successfully updated record";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            objResponse.Message = "Failed to update record";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }


                    }
                    else
                    {
                        objResponse.Message = "Failed to update records for user category";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }



                }
                //Addition
                else
                {
                    UserCategoryMapping objUserCategoryMapping = new UserCategoryMapping();
                    var getcsvfile = objUserCategoryMappingViewModel.csvfile.Table1;

                    foreach (var j in getcsvfile)
                    {

                        objUserCategoryMapping.Fk_MobileNumber = j.Fk_MobileNumber;
                        objUserCategoryMapping.CategoryId = j.CategoryId;
                        objUserCategoryMapping.CategoryName = j.CategoryName;
                        objUserCategoryMapping.CreatedDate = DateTime.Now;

                        // dbContext.UserCategoryMapping.Add(objUserCategoryMapping);


                        //   i = dbContext.SaveChanges();

                        dbContext.Entry(objUserCategoryMapping).State = EntityState.Added;
                        i = dbContext.SaveChanges();
                        dbContext.Entry(objUserCategoryMapping).State = EntityState.Detached;
                    }
                    if (i != 0)
                    {
                        objResponse.Message = "Successfuly added ";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "failed to add record";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }




                }




            }
            catch (Exception ex)
            {
                throw ex;

            }


        }


        #endregion

        #region Common methods

        /// <summary>
        /// This method is used to get distance by passing latitude and longitude
        /// Date: 30 jan 2019
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <returns></returns>
        private double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(Degree2Radians(lat1)) * Math.Sin(Degree2Radians(lat2)) + Math.Cos(Degree2Radians(lat1)) * Math.Cos(Degree2Radians(lat2)) * Math.Cos(Degree2Radians(theta));
            dist = Math.Acos(dist);
            dist = Radians2Degree(dist);
            dist = (dist * 60 * 1.1515) / 0.6213711922;          //miles to kms
            return (dist);
        }

        /// <summary>
        /// This method is used to convert Degree to Radians
        /// Date: 30 jan 2019
        /// </summary>
        /// <param name="deg"></param>
        /// <returns></returns>
        private double Degree2Radians(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        /// <summary>
        /// This method is used to convert Radians to Degree
        /// Date: 30 jan 2019
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        private double Radians2Degree(double rad)
        {
            return (rad * 180.0 / Math.PI);
        }

        #endregion

        #region User Registration, Login and userdetails

        /// <summary>
        /// For registration of a new user.
        /// </summary>
        /// <param name="objMandiUserInfoViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/MandiUser/MandiUserRegistration")]
        public HttpResponseMessage MandiUserRegistration(MandiUserInfoViewModel objMandiUserInfoViewModel)
        {
            try
            {
                Mandi_UserInfo objMandi_UserInfo = new Mandi_UserInfo();

                string mobileNumber = objMandiUserInfoViewModel.MobileNumber;
                //For getting alredy registered user details. 
                var number = (from mandiUser in dbContext.Mandi_UserInfo where mandiUser.MobileNumber == mobileNumber select mandiUser).FirstOrDefault();
                if (number != null)
                {
                    objResponse.Message = "Mobile number already exists.";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
                else
                {
                    objMandi_UserInfo.FullName = objMandiUserInfoViewModel.FullName;
                    objMandi_UserInfo.CreatedDate = DateTime.Now;
                    objMandi_UserInfo.State = objMandiUserInfoViewModel.State;
                    objMandi_UserInfo.District = objMandiUserInfoViewModel.District;
                    objMandi_UserInfo.Taluka = objMandiUserInfoViewModel.Taluka;
                    objMandi_UserInfo.Pincode = objMandiUserInfoViewModel.Pincode;
                    objMandi_UserInfo.UserType = objMandiUserInfoViewModel.UserType;
                    objMandi_UserInfo.AdharId = objMandiUserInfoViewModel.AdharId;
                    objMandi_UserInfo.MobileNumber = objMandiUserInfoViewModel.MobileNumber;
                    objMandi_UserInfo.ProfilePictureUrl = objMandiUserInfoViewModel.ProfilePictureUrl;
                    //objMandi_UserInfo.RoleId = objMandiUserInfoViewModel.RoleId;
                    objMandi_UserInfo.IsProfileUpdated = false;
                    objMandi_UserInfo.DeviceToken = objMandiUserInfoViewModel.DeviceToken;



                    dbContext.Mandi_UserInfo.Add(objMandi_UserInfo);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        Mandi_UserRoles objMandi_UserRoles = new Mandi_UserRoles();
                        objMandi_UserRoles.RoleId = objMandiUserInfoViewModel.RoleId;
                        objMandi_UserRoles.MobileNumber = objMandiUserInfoViewModel.MobileNumber;
                        dbContext.Mandi_UserRoles.Add(objMandi_UserRoles);
                        i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            objResponse.Message = "Registered successfully";
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
                        objResponse.Message = "Registration Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUserController", "MandiUserRegistration");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        /// <summary>
        /// For verifying mobile number of a exsisting user
        /// </summary>
        /// <param name="objMandiUserMobileNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/MandiUser/VerifyMobileNo")]
        public HttpResponseMessage VerifyMobileNo(MandiUserVerification objMandiUserMobileNumber)
        {
            try
            {
                //Session["DeviceToken"] = objMandiUserMobileNumber.DeviceToken;
                string mobileNumber = objMandiUserMobileNumber.MobileNumber;

                //For genrating OTP and saving it into UserOTPInfo table
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    string otpNumber = mobileNumber == "8888899999" ? "1234" : objCommonClasses.GenerateOTP();

                    //otpNumber = objCommonClasses.GenerateOTP();
                    bool sendOTP = mobileNumber == "8888899999" ? true : objCommonClasses.SendOTP(otpNumber, mobileNumber);
                    if (sendOTP)
                    {
                        //For inserting a new mobile number into UserOTPInfo table
                        UserOTPInfo objUserOTPInfo = new UserOTPInfo();
                        objUserOTPInfo.MobileNumber = mobileNumber;
                        objUserOTPInfo.OTP = otpNumber;
                        objUserOTPInfo.GenratedDate = DateTime.Now;

                        //var update = (from u in dbContext.UserOTPInfo where u.MobileNumber == mobileNumber select u).SingleOrDefault();
                        dbContext.UserOTPInfo.Add(objUserOTPInfo);
                        dbContext.SaveChanges();
                        number.DeviceToken = objMandiUserMobileNumber.DeviceToken;
                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            UpdateDeviceToken(objMandiUserMobileNumber);
                        }

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


        [HttpPost]
        //[Authorize]
        [Route("api/MandiUser/UpdateDeviceToken")]
        public HttpResponseMessage UpdateDeviceToken(MandiUserVerification objMandiUserVerification)
        {
            try
            {
                Mandi_UserInfo objMandi_UserInfo = new Mandi_UserInfo();
                string mobileNumber = objMandiUserVerification.MobileNumber;
                //get mobileNumber from user table
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    objMandi_UserInfo.DeviceToken = objMandiUserVerification.DeviceToken;


                    dbContext.Mandi_UserInfo.Add(objMandi_UserInfo);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        string message = "Your Device Toekn has been  successfully saved";

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
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "DeviceTokenNotSaved");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }



        /// <summary>
        /// For getting user details, after verfication of the OTP
        /// </summary>
        /// <param name="objUserVerification"></param>
        /// <returns></returns>
        [HttpPost]

        [Route("api/MandiUser/GetMandiUserInfo")]
        public HttpResponseMessage GetMandiUserInfo(MandiUserVerification objMandiUserVerification)
        {
            try
            {
                string mobileNumber = objMandiUserVerification.MobileNumber;

                var userDetails = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                var userRoles = (from userRole in dbContext.Mandi_UserRoles where userRole.MobileNumber == mobileNumber select userRole).FirstOrDefault();
                var userAddress = (from address in dbContext.UsersAddress where address.MobileNumberForMandi == mobileNumber && address.IsActive == true select address).FirstOrDefault();
                MandiUserInfoViewModel objMandiUserInfoViewModel = new MandiUserInfoViewModel();

                if (userDetails != null)
                {

                    objMandiUserInfoViewModel.MobileNumber = userDetails.MobileNumber;
                    objMandiUserInfoViewModel.FullName = userDetails.FullName;
                    objMandiUserInfoViewModel.State = userDetails.State;
                    objMandiUserInfoViewModel.District = userDetails.District;
                    objMandiUserInfoViewModel.Taluka = userDetails.Taluka;
                    objMandiUserInfoViewModel.Pincode = userDetails.Pincode;
                    objMandiUserInfoViewModel.UserType = userDetails.UserType;
                    objMandiUserInfoViewModel.AdharId = userDetails.AdharId;
                    objMandiUserInfoViewModel.ProfilePictureUrl = userDetails.ProfilePictureUrl;
                    objMandiUserInfoViewModel.IsProfileUpdated = userDetails.IsProfileUpdated;
                    objMandiUserInfoViewModel.RoleId = userRoles.RoleId;

                    if (userAddress != null)
                    {
                        objMandiUserInfoViewModel.ReceiverName = !string.IsNullOrEmpty(userAddress.reciver_name) ? userAddress.reciver_name : "";
                        //objMandiUserInfoViewModel.ReceiverName = userAddress.reciver_name;
                        objMandiUserInfoViewModel.ShipAddress = userAddress.ship_address;
                        objMandiUserInfoViewModel.City = userAddress.city;
                        objMandiUserInfoViewModel.ShipMobile = !string.IsNullOrEmpty(userAddress.ship_mobile) ? userAddress.ship_mobile : "";
                        objMandiUserInfoViewModel.EmailId = userAddress.email_id;
                        objMandiUserInfoViewModel.PanNumber = userAddress.Pan_number;
                        objMandiUserInfoViewModel.IsMandiUser = userAddress.IsMandiUser;
                        objMandiUserInfoViewModel.IsPermanentAddress = userAddress.IsPermanentAddress;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, objMandiUserInfoViewModel);
                }
                objResponse.Message = "User Not Exist";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);

            }


            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUserController", "GetMandiUserInfo");
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
        [Route("api/MandiUser/UpdateMandiUserDetails")]
        public HttpResponseMessage UpdateMandiUserDetails(MandiUserInfoViewModel objMandiUserInfoViewModel)
        {
            try
            {
                Mandi_UserInfo objMandi_UserInfo = new Mandi_UserInfo();
                string mobileNumber = objMandiUserInfoViewModel.MobileNumber;
                //to get userinfo 
                var userDetails = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                var userAddress = (from address in dbContext.UsersAddress where address.MobileNumberForMandi == mobileNumber select address).FirstOrDefault();

                var userRole = (from role in dbContext.Mandi_UserRoles where role.MobileNumber == mobileNumber select role).FirstOrDefault();
                if (userDetails != null)
                {
                    {
                        userDetails.FullName = !string.IsNullOrEmpty(objMandiUserInfoViewModel.FullName) ? objMandiUserInfoViewModel.FullName : userDetails.FullName;
                        userDetails.State = !string.IsNullOrEmpty(objMandiUserInfoViewModel.State) ? objMandiUserInfoViewModel.State : userDetails.State;
                        userDetails.District = !string.IsNullOrEmpty(objMandiUserInfoViewModel.District) ? objMandiUserInfoViewModel.District : userDetails.District;
                        userDetails.Taluka = !string.IsNullOrEmpty(objMandiUserInfoViewModel.Taluka) ? objMandiUserInfoViewModel.Taluka : userDetails.Taluka;
                        userDetails.Pincode = !string.IsNullOrEmpty(objMandiUserInfoViewModel.Pincode) ? objMandiUserInfoViewModel.Pincode : userDetails.Pincode;
                        userDetails.UserType = !string.IsNullOrEmpty(objMandiUserInfoViewModel.UserType) ? objMandiUserInfoViewModel.UserType : userDetails.UserType;
                        userDetails.AdharId = !string.IsNullOrEmpty(objMandiUserInfoViewModel.AdharId) ? objMandiUserInfoViewModel.AdharId : userDetails.AdharId;
                        userDetails.MobileNumber = !string.IsNullOrEmpty(objMandiUserInfoViewModel.MobileNumber) ? objMandiUserInfoViewModel.MobileNumber : userDetails.MobileNumber;
                        userDetails.ProfilePictureUrl = !string.IsNullOrEmpty(objMandiUserInfoViewModel.ProfilePictureUrl) ? objMandiUserInfoViewModel.ProfilePictureUrl : userDetails.ProfilePictureUrl;
                        userDetails.IsProfileUpdated = true;
                        userRole.RoleId = objMandiUserInfoViewModel.RoleId != 0 ? objMandiUserInfoViewModel.RoleId : userRole.RoleId;


                        //to modify product in database.
                        dbContext.Entry(userDetails).State = EntityState.Modified;
                        dbContext.Entry(userRole).State = EntityState.Modified;
                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            if (userAddress == null)
                            {

                                AddShippingAddress(objMandiUserInfoViewModel);
                                objResponse.Message = "Added successfully";
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }

                            else if (userAddress.IsPermanentAddress == true && userAddress.IsMandiUser == true && userAddress.IsPermanentAddress != null && userAddress.IsMandiUser != null)
                            {
                                userAddress.retailer_id = "123456";
                                userAddress.MobileNumberForMandi = objMandiUserInfoViewModel.MobileNumber;
                                userAddress.reciver_name = objMandiUserInfoViewModel.ReceiverName;
                                userAddress.ship_address = objMandiUserInfoViewModel.ShipAddress;
                                userAddress.city = objMandiUserInfoViewModel.City;
                                userAddress.pincode = objMandiUserInfoViewModel.Pincode;
                                userAddress.ship_mobile = objMandiUserInfoViewModel.ShipMobile;
                                userAddress.email_id = objMandiUserInfoViewModel.EmailId;
                                userAddress.Pan_number = objMandiUserInfoViewModel.PanNumber;
                                userAddress.tr_date = DateTime.Now;
                                //userAddress.IsPermanentAddress = objMandiUserInfoViewModel.IsPermanentAddress;
                                //userAddress.IsMandiUser = true;

                                var j = dbContext.SaveChanges();
                                if (j != 0)
                                {
                                    objResponse.Message = "Updated successfully";
                                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                                }
                                else
                                {
                                    objResponse.Message = "Failed";
                                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                                }

                            }

                        }
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
                objCommonClasses.InsertExceptionDetails(ex, "MandiUserController", "UpdateMandiUserDetails");
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
        [Route("api/MandiUser/AddShippingAddress")]
        public HttpResponseMessage AddShippingAddress(MandiUserInfoViewModel objMandiUserInfoViewModel)
        {
            try
            {
                UsersAddress objUsersAddress = new UsersAddress();
                string mobileNumber = objMandiUserInfoViewModel.MobileNumber;
                //get mobileNumber from user table
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    objUsersAddress.retailer_id = "123456";
                    objUsersAddress.MobileNumberForMandi = objMandiUserInfoViewModel.MobileNumber;
                    objUsersAddress.reciver_name = objMandiUserInfoViewModel.ReceiverName;
                    objUsersAddress.ship_address = objMandiUserInfoViewModel.ShipAddress;
                    objUsersAddress.city = objMandiUserInfoViewModel.City;
                    objUsersAddress.pincode = objMandiUserInfoViewModel.Pincode;
                    objUsersAddress.ship_mobile = objMandiUserInfoViewModel.ShipMobile;
                    objUsersAddress.email_id = objMandiUserInfoViewModel.EmailId;
                    objUsersAddress.Pan_number = objMandiUserInfoViewModel.PanNumber;
                    objUsersAddress.tr_date = DateTime.Now;
                    objUsersAddress.IsPermanentAddress = objMandiUserInfoViewModel.IsPermanentAddress;
                    objUsersAddress.IsMandiUser = true;
                    objUsersAddress.IsActive = true;

                    dbContext.UsersAddress.Add(objUsersAddress);
                    var j = dbContext.SaveChanges();
                    if (j != 0)
                    {
                        objResponse.Message = "Shipping Address Added successfully";
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
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "AddShippingAddress");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        [HttpPost]
        [Authorize]
        [Route("api/MandiUser/GetShippingAddress")]
        public HttpResponseMessage GetShippingAddress(UserMobileNumber objUserMobileNumber)
        {
            try
            {
                AddressMaster objListAddressMaster = new AddressMaster();
                List<Address> objListAddress = new List<Address>();
                string mobileNumber = objUserMobileNumber.MobileNumber;

                //get mobileNumber from user table
                var getUserMobileNumber = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user.MobileNumber).FirstOrDefault();

                //For getting list of address from the table.
                var getaddress = (from address in dbContext.UsersAddress where address.MobileNumberForMandi == getUserMobileNumber && address.IsActive == true select address).ToList();
                if (getaddress != null)
                {
                    foreach (var i in getaddress)
                    {
                        Address objAddress = new Address()
                        {
                            tr_id = i.tr_id,
                            retailer_id = i.retailer_id,
                            retailer_mobile = i.MobileNumberForMandi,
                            reciver_name = i.reciver_name,
                            ship_address = i.ship_address,
                            city = i.city,
                            pincode = i.pincode,
                            ship_mobile = i.ship_mobile,
                            email_id = i.email_id,
                            PanNumber = i.Pan_number,
                            tr_date = Convert.ToString(i.tr_date),
                            IsPermanentAddress = i.IsPermanentAddress,
                            IsMandiUser = i.IsMandiUser,


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
        /// For active/inactive address
        /// </summary>
        /// <param name="objMandiUserInfoViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/MandiUser/DeleteAddress")]
        public HttpResponseMessage DeleteAddress(MandiUserInfoViewModel objMandiUserInfoViewModel)
        {
            try
            {
                int addressId = objMandiUserInfoViewModel.Tr_Id;
                //get address from database.
                var userAddress = (from address in dbContext.UsersAddress where address.tr_id == addressId select address).FirstOrDefault();
                if (userAddress != null)
                {
                    userAddress.IsActive = false;
                    dbContext.Entry(userAddress).State = EntityState.Modified;
                    //dbContext.Mandi_ProductMaster.Remove(sellerProduct);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "address Deactivated successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "address Deactivation Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "No address Found";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "DeleteAddress");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="objUserRequirementViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Route("api/MandiUser/AddMandiUserRequirement")]
        public HttpResponseMessage AddMandiUserRequirement(MandiUserRequirementViewModel objUserRequirementViewModel)
        {
            try
            {
                Mandi_Requirement objRequirement = new Mandi_Requirement();
                string mobileNumber = objUserRequirementViewModel.MobileNumber;
                //get mobileNumber from user table
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
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

                    dbContext.Mandi_Requirement.Add(objRequirement);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {

                        string FromMail = "";
                        string subject = "";
                        string body = "";

                        body = " Hello Admin,\r\n";
                        body += " There is new user requirement . Details of particular Product are as follows \r\n";
                        body += "1.BuyerId: " + objUserRequirementViewModel.BuyerId + "\r\n";
                        body += "2.BuyerAddress: " + objUserRequirementViewModel.BuyerAddress + "\r\n";
                        body += "3.CropName : " + objUserRequirementViewModel.CropName + "\r\n";
                        body += "4.VarietyName: " + objUserRequirementViewModel.Variety + "\r\n";
                        body += "5.Quantity: " + objUserRequirementViewModel.Quantity + "\r\n";
                        body += "6.QualitySpecification: " + objUserRequirementViewModel.QualitySpecification + "\r\n";
                        body += "7.DeliveryLocation: " + objUserRequirementViewModel.DeliveryLocation + "\r\n";
                        body += "8.ExpectedPrice: " + objUserRequirementViewModel.ExpectedPrice + "\r\n";
                        body += "9.ExpectedDate: " + objUserRequirementViewModel.ExpectedDate + "\r\n";
                        body += "10.IsPriceNegotiable" + objUserRequirementViewModel.IsPriceNegotiable + "\r\n";
                        body += "11.Remarks" + objUserRequirementViewModel.Remarks + "\r\n";

                        body += "\r\n";
                        body += "warm regards";


                        FromMail = "developer@growindigo.co.in";
                        subject = "Grow Mandi App: New UserRequirement Detail ";



                        MailMessage maile = new MailMessage();
                        SmtpClient SmtpServere = new SmtpClient("smtp.gmail.com");
                        maile.From = new MailAddress(FromMail);


                        maile.To.Add("Shivam.Dhagat@systematixindia.com");
                        maile.To.Add("Ashish.Agrawal@systematixindia.com");
                        maile.To.Add("Shivamdhagat1@gmail.com");


                        maile.Subject = subject;
                        maile.Body = body;
                        SmtpServere.Port = 587;
                        SmtpServere.Credentials = new System.Net.NetworkCredential("developer@growindigo.co.in", "phoansnuhfutodwq");
                        SmtpServere.EnableSsl = true;
                        SmtpServere.Send(maile);

                        objResponse.Message = "User Requirement is been successfully saved. ";

                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "User Requirement is saved successfully but failed to shot mail";

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

        #region Notification 
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="DeviceToken"></param>
        /// <param name="Message"></param>
        /// <param name="Title"></param>
        /// <returns></returns>
        public string SendFCMNotificationToUsers(string DeviceToken = "", string Message = "", string Title = "")
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
        public string SendFCMNotificationWithDataToUsers(string DeviceToken = "", string Message = "", string Title = "", string MobileNumber="", string Type="cart")
        {
            string notificationjson = string.Empty;
            var mobileNumber = "";
            
            if (!string.IsNullOrEmpty(DeviceToken))
            {
                notificationjson = "{ \"to\": \"" + DeviceToken + "\" ,\"data\":{\"MobileNumber\":\"" + MobileNumber + "\",\"Type\":\"" + Type + "\"},\"notification\":{\"type\":\"" + Title + "\",\"body\":\"" + Message + "\"}}";
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objOrderBookingViewModel"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public string AddNotification(OrderBookingViewModel objOrderBookingViewModel, string Message)
        {


            try
            {
                //get mobileNumber from user table
                string mobileNumber = objOrderBookingViewModel.Buyer_Mobile;
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    Mandi_Notification objMandi_Notification = new Mandi_Notification();
                    objMandi_Notification.MobileNumber = objOrderBookingViewModel.Buyer_Mobile;
                    objMandi_Notification.Message = Message;
                    objMandi_Notification.Tr_Date = DateTime.Now;

                    dbContext.Mandi_Notification.Add(objMandi_Notification);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        return "true";

                    }
                    else
                    {
                        return "false";

                    }


                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";
                    return Message;
                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                throw ex;

            }
        }





        #endregion

        #region Enquiry and feedback

        /// <summary>
        /// To add Users Enquiry.
        /// </summary>
        /// <param name="objUserEnquiryViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Route("api/MandiUser/UserEnquiry")]
        public string UserEnquiry(UserEnquiryViewModel objUserEnquiryViewModel)
        {
            try
            {
                Mandi_UserEnquiry objMandi_UserEnquiry = new Mandi_UserEnquiry();
                string mobileNumber = objUserEnquiryViewModel.MobileNumber;

                //var checkEnquiry = (from enquiry in dbContext.Mandi_UserEnquiry where enquiry.MobileNumber == mobileNumber && enquiry.ProductId == objUserEnquiryViewModel.ProductId select enquiry).FirstOrDefault();
                //if (checkEnquiry != null)
                //{
                //    string msg = "This Product with ProductId " + objUserEnquiryViewModel.ProductId + "is already added in cart";
                //    return msg;
                //}
                //else
                //{
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    objMandi_UserEnquiry.ProductId = objUserEnquiryViewModel.ProductId;
                    objMandi_UserEnquiry.Enquiry = objUserEnquiryViewModel.Enquiry;
                    objMandi_UserEnquiry.EmailId = objUserEnquiryViewModel.EmailId;
                    objMandi_UserEnquiry.MobileNumber = objUserEnquiryViewModel.MobileNumber;
                    objMandi_UserEnquiry.Tr_Date = DateTime.Now;

                    var add = dbContext.Mandi_UserEnquiry.Add(objMandi_UserEnquiry);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {

                        #region Email and notification
                        string message = "Your Enquiry has been received Successfully";
                        objCommonClasses.SendSMS(mobileNumber, message);

                        EmailController objEmailController = new EmailController();
                        EmailModel objEmailModel = new EmailModel();
                        objEmailModel.BuyerId = objUserEnquiryViewModel.BuyerId.ToString();
                        objEmailModel.BuyerName = objUserEnquiryViewModel.BuyerName;
                        objEmailModel.BuyerContact = objUserEnquiryViewModel.MobileNumber;
                        objEmailModel.BuyerAddress = objUserEnquiryViewModel.BuyerAddress;
                        objEmailModel.ProductId = objUserEnquiryViewModel.ProductId.ToString();
                        objEmailModel.CropName = objUserEnquiryViewModel.CropName;
                        objEmailModel.Qty = objUserEnquiryViewModel.Qty;
                        objEmailModel.Rate = objUserEnquiryViewModel.Rate;
                        objEmailController.sendEmailViaWebApi(objEmailModel, "Enquiry");
                        message = "Your Enquiry has been received Successfully";
                        objResponse.Message = "Enquiry Submitted successfully";


                        //Add and Send notification

                        var getSellerMobileNumber = (from user in dbContext.Mandi_ProductMaster where user.Tr_Id == objUserEnquiryViewModel.ProductId select user.MobileNumber).FirstOrDefault();
                        var getSellerMobileNumberByNumber = (from seller in dbContext.Mandi_UserInfo where seller.MobileNumber == getSellerMobileNumber select seller).FirstOrDefault();


                        string Title = "Buyer is enquiry regarding  your product";
                        OrderBookingViewModel objOrderBookingViewModel = new OrderBookingViewModel();

                        if (getSellerMobileNumberByNumber != null)
                        {
                            if (getSellerMobileNumberByNumber.DeviceToken != null)
                            {
                                objOrderBookingViewModel.Buyer_Mobile = getSellerMobileNumberByNumber.MobileNumber;
                                string Message = "Dear Customer, buyers are enquiring about your product.We will soon be contacting you";
                                var addnotification = AddNotification(objOrderBookingViewModel, Message);
                                if (addnotification == "true")
                                {

                                    SendFCMNotificationToUsers(getSellerMobileNumberByNumber.DeviceToken, Message, Title);
                                    // string msg = "Enquiry has been saved successfully";
                                    string msg = add.Tr_Id.ToString();
                                    return msg;
                                }
                                else
                                {
                                    //  objResponse.Message = "Enquiry has been saved successfully";
                                    string msg = add.Tr_Id.ToString();
                                    return msg;
                                }


                            }

                            else
                            {
                                //  objResponse.Message = "Enquiry has been saved successfully";
                                string msg = add.Tr_Id.ToString();
                                return msg;
                            }

                        }
                        else
                        {
                            //  objResponse.Message = "Enquiry has been saved successfully";
                            string msg = add.Tr_Id.ToString();
                            return msg;
                        }
                    }

                    else
                    {
                        string Message = "Failed";
                        return Message;
                    }
                    #endregion



                }
                else
                {
                    string Message = "Mobile number not exists.";
                    return Message;

                }
                //}
                //get mobileNumber from user table

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "UserEnquiry");
                return "Error";
            }

        }


        /// <summary>
        /// To add Users Enquiry.
        /// </summary>
        /// <param name="objUserEnquiryViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/MandiUser/Mandi_UserFeedback")]
        public HttpResponseMessage Mandi_UserFeedback(Mandi_UserFeedbackViewModel objMandi_UserFeedbackViewModel)
        {
            try
            {
                Mandi_UserFeedback objMandi_UserFeedback = new Mandi_UserFeedback();
                string mobileNumber = objMandi_UserFeedbackViewModel.MobileNumber;
                //get mobileNumber from user table
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    objMandi_UserFeedback.Usercode = objMandi_UserFeedbackViewModel.MobileNumber;
                    objMandi_UserFeedback.VarietyName = objMandi_UserFeedbackViewModel.VarietyName;
                    objMandi_UserFeedback.Feedback = objMandi_UserFeedbackViewModel.Feedback;
                    objMandi_UserFeedback.Tr_Date = DateTime.Now;

                    dbContext.Mandi_UserFeedback.Add(objMandi_UserFeedback);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        //string message = "Your Feedback has been received Successfully";
                        //objCommonClasses.SendSMS(mobileNumber, message);

                        objResponse.Message = "Feedback Submitted successfully";
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
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "Mandi_UserFeedback");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        #endregion

        #region Account Detail Crud

        /// <summary>
        /// For Adding Bank Account Details.
        /// </summary>
        /// <param name="objProductMasterViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/MandiUser/AddUpdateUserBankAccountDetails")]
        public HttpResponseMessage AddUpdateUserBankAccountDetails(UsersBankAccountDetailsViewModel obUsersBankAccountDetailsViewModel)
        {
            try
            {
                //Create
                if (obUsersBankAccountDetailsViewModel.Id == 0 || obUsersBankAccountDetailsViewModel.Id == null)
                {
                    UsersBankAccountDetails objUsersBankAccountDetails = new UsersBankAccountDetails();
                    string mobileNumber = obUsersBankAccountDetailsViewModel.MobileNumber;
                    //get mobileNumber from user table
                    var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                    if (number != null)
                    {
                        //objUsersBankAccountDetails.NetBankingId = obUsersBankAccountDetailsViewModel.NetBankingId;
                        objUsersBankAccountDetails.AccountNumber = obUsersBankAccountDetailsViewModel.AccountNumber;
                        objUsersBankAccountDetails.AccountHolderName = obUsersBankAccountDetailsViewModel.AccountHolderName == null ? "N/A" : obUsersBankAccountDetailsViewModel.AccountHolderName;
                        objUsersBankAccountDetails.BankName = obUsersBankAccountDetailsViewModel.BankName;
                        objUsersBankAccountDetails.Branch = obUsersBankAccountDetailsViewModel.Branch;
                        objUsersBankAccountDetails.AccountType = obUsersBankAccountDetailsViewModel.AccountType;
                        objUsersBankAccountDetails.IFSC_Code = obUsersBankAccountDetailsViewModel.IFSC_Code;
                        objUsersBankAccountDetails.MobileNumber = obUsersBankAccountDetailsViewModel.MobileNumber;
                        objUsersBankAccountDetails.Status = true;
                        objUsersBankAccountDetails.CreatedDate = DateTime.Now;

                        dbContext.UsersBankAccountDetails.Add(objUsersBankAccountDetails);
                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            objResponse.Message = "Bank Account Detail are added successfully for user";
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
                        objResponse.Message = "User number not exists.";

                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                    }
                }
                //update
                else
                {
                    UsersBankAccountDetails objUsersBankAccountDetails = new UsersBankAccountDetails();
                    string MobileNumber = obUsersBankAccountDetailsViewModel.MobileNumber.Trim();

                    var userBankDetails = (from user in dbContext.UsersBankAccountDetails where user.MobileNumber == MobileNumber select user).FirstOrDefault();
                    if (userBankDetails != null)
                    {
                        {
                            userBankDetails.AccountNumber = !string.IsNullOrEmpty(obUsersBankAccountDetailsViewModel.AccountNumber) ? obUsersBankAccountDetailsViewModel.AccountNumber : userBankDetails.AccountNumber;
                            userBankDetails.AccountHolderName = !string.IsNullOrEmpty(obUsersBankAccountDetailsViewModel.AccountHolderName) ? obUsersBankAccountDetailsViewModel.AccountHolderName : userBankDetails.AccountHolderName;
                            userBankDetails.BankName = !string.IsNullOrEmpty(obUsersBankAccountDetailsViewModel.BankName) ? obUsersBankAccountDetailsViewModel.BankName : userBankDetails.BankName;
                            userBankDetails.Branch = !string.IsNullOrEmpty(obUsersBankAccountDetailsViewModel.Branch) ? obUsersBankAccountDetailsViewModel.Branch : userBankDetails.Branch;
                            userBankDetails.AccountType = !string.IsNullOrEmpty(obUsersBankAccountDetailsViewModel.AccountType) ? obUsersBankAccountDetailsViewModel.AccountType : userBankDetails.AccountType;
                            userBankDetails.IFSC_Code = !string.IsNullOrEmpty(obUsersBankAccountDetailsViewModel.IFSC_Code) ? obUsersBankAccountDetailsViewModel.IFSC_Code : userBankDetails.IFSC_Code;
                            userBankDetails.ModifiedDate = DateTime.Now;
                            userBankDetails.Status = true;


                        }

                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            objResponse.Message = "Bank Account Detail are updated successfully";
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
                        objResponse.Message = "Bank Account  not found.";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "AddUserBankAccountDetails");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }


        [HttpGet]
        // [Authorize]
        [Route("api/MandiUser/GetUserBankAccountDetails")]
        public HttpResponseMessage GetUserBankAccountDetails(string mobileNumber = "")
        {
            try
            {
                UsersBankAccountDetails objUsersBankAccountDetails = new UsersBankAccountDetails();
                string MobileNumber = mobileNumber.Trim();

                var userBankDetails = (from user in dbContext.UsersBankAccountDetails where user.MobileNumber == MobileNumber select user).FirstOrDefault();
                if (userBankDetails != null)
                {
                    UsersBankAccountDetailsViewModel objUsersBankAccountDetailsViewModel = new UsersBankAccountDetailsViewModel()
                    {
                        Id = userBankDetails.Id,
                        MobileNumber = userBankDetails.MobileNumber,
                        AccountType = userBankDetails.AccountType,
                        AccountHolderName = userBankDetails.AccountHolderName,
                        AccountNumber = userBankDetails.AccountNumber,
                        BankName = userBankDetails.BankName,
                        Branch = userBankDetails.Branch,
                        IFSC_Code = userBankDetails.IFSC_Code,
                        CreatedDate = userBankDetails.CreatedDate,


                    };
                    return Request.CreateResponse(HttpStatusCode.OK, objUsersBankAccountDetailsViewModel);
                }
                objResponse.Message = "Bank-Detail does not exist for particular user.";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }

            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUserController", "GetUserBankAccountDetails");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }


        /// <summary>
        /// Soft delete for bank details 
        /// </summary>
        /// <param name="mobileNumber"></param>
        [HttpPost]
        [Authorize]
        [Route("api/MandiUser/DeleteBankAccountDetail")]
        public HttpResponseMessage DeleteBankAccountDetail(string mobileNumber = "")
        {
            try
            {

                //get address from database.
                var getuserbankdetail = (from bankdetail in dbContext.UsersBankAccountDetails where bankdetail.MobileNumber == mobileNumber select bankdetail).FirstOrDefault();
                if (getuserbankdetail != null)
                {
                    getuserbankdetail.Status = false;
                    getuserbankdetail.ModifiedDate = DateTime.Now;

                    dbContext.Entry(getuserbankdetail).State = EntityState.Modified;
                    //dbContext.Mandi_ProductMaster.Remove(sellerProduct);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "Bank Detail Deactivated successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Bank Detail  Deactivation Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "No address Found";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "DeleteAddress");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        #endregion

        #region Schedulers , Daily Api and count API
        [HttpPost]

        [Route("api/MandiUser/SendDailyProductNotificationToUser")]
        public HttpResponseMessage SendDailyProductNotificationToUser()
        {
            try
            {

                //get new Product from database.
                var getYestetdayDate = DateTime.Now.AddDays(-1);
                // string getYestetdayDate = Convert.ToDateTime("09/25/2011").ToString("dd/MM/yyyy");
                var currentDatetime = DateTime.Now;

                var getNewProduct = (from product in dbContext.Mandi_ProductMaster where product.Tr_Date >= getYestetdayDate && product.Tr_Date < currentDatetime select product.CategoryId).Distinct().ToList();
                if (getNewProduct.Count() > 0)
                {
                    //to get All user In mandi for notification
                    //where devicetoken is not null 
                    //categoryId is not null
                    //and where categoryId matched for category mapping and product mapping 

                    var getUsersForNotification = dbContext.UserCategoryMapping.Where(x => x.CategoryId != null && x.Mandi_UserInfo.DeviceToken != null && getNewProduct.Contains(x.CategoryId)).Select(y => new { y.Fk_MobileNumber, y.Mandi_UserInfo.DeviceToken, y.Mandi_UserInfo.FullName }).ToList();
                    OrderBookingViewModel objOrderBookingViewModel = new OrderBookingViewModel();
                    foreach (var users in getUsersForNotification)
                    {
                        objOrderBookingViewModel.Buyer_Mobile = users.Fk_MobileNumber;
                        string Message = "Dear " + users.FullName + ", new products have been listed. Keep on checking in Grow Mandi and let us know your requirements";
                        var addnotification = AddNotification(objOrderBookingViewModel, Message);
                        if (addnotification == "true")
                        {
                            string Title = "New Products has been Added in Grow Mandi";
                            SendFCMNotificationToUsers(users.DeviceToken, Message, Title);
                            //objResponse.Message = "Notification has been send to all users";
                            //return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            //objResponse.Message = "Error while Sending notification";
                            //return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    objResponse.Message = "Notification has been send to all users";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
                else
                {
                    objResponse.Message = "No new Product added on " + getYestetdayDate;
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "SendDailyProductNotificationToUser");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }

        [HttpPost]
        [Route("api/MandiUser/GetCount")]
        public HttpResponseMessage GetCount(CountViewModel objCountViewModel)
        {
            try
            {

                if (objCountViewModel.CountType == "Seller")
                {
                    //seller
                    var getAllUser = (from seller in dbContext.Mandi_UserInfo where seller.UserType == "Sell" select seller).ToList();
                    var count = getAllUser.Count();

                    objCountViewModel.SellerCount = count;
                    return Request.CreateResponse(HttpStatusCode.OK, objCountViewModel);
                }
                else
                {
                    //Prodduct
                    var getAllProduct = (from product in dbContext.Mandi_ProductMaster select product).ToList();
                    var count = getAllProduct.Count();

                    objCountViewModel.ProductCount = count;
                    return Request.CreateResponse(HttpStatusCode.OK, objCountViewModel);
                }


            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "GetCount");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }


        [HttpPost]
        [Route("api/MandiUser/UpdateProductStatus")]
        public HttpResponseMessage UpdateProductStatus()
        {


            try
            {
                string productStatus = "";
                var getALLProducts = (from product in dbContext.Mandi_ProductMaster select product).Distinct().ToList();
                if (getALLProducts.Count() > 0)
                {

                    foreach (var pro in getALLProducts)
                    {
                        
                        DateTime AvailabilityDate = Convert.ToDateTime(pro.AvailabilityDate);
                        DateTime SoleDate = Convert.ToDateTime(pro.AvailabilityDate).AddDays(30);
                        DateTime CurrentDate = DateTime.Now;
                        DateTime ComingSoonDate;
                        if (pro.AvailabilityDate == null)
                        {
                            productStatus = "Sold";
                        }
                        else
                        {
                            if (AvailabilityDate != null)
                            {
                                ComingSoonDate = Convert.ToDateTime(AvailabilityDate).AddDays(-15);

                                if (DateTime.Now <= ComingSoonDate)
                                {
                                    productStatus = "Coming Soon";
                                }
                                else if (DateTime.Now > SoleDate)
                                {
                                    productStatus = "Sold";
                                }

                                else if (DateTime.Now > ComingSoonDate)
                                {
                                    productStatus = "Available";
                                }
                                else
                                {
                                    productStatus = "In Review";
                                }
                            }
                            else
                            {
                                productStatus = "In Review";
                            }
                        }

                        Mandi_ProductMaster objMandi_ProductMaster = new Mandi_ProductMaster();
                        objMandi_ProductMaster.Product_Status = productStatus;
                        var id = pro.Tr_Id;
                        var getProductDetails = dbContext.Mandi_ProductMaster.Where(x => x.Tr_Id == pro.Tr_Id).FirstOrDefault();
                        getProductDetails.Product_Status = productStatus;
                        dbContext.SaveChanges();
                    }
                    objResponse.Message = "Product status for all products has been updated.";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }



                else
                {
                    objResponse.Message = "No new Product added on ";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {

                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "UpdateProductStatus");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }


        }


        [HttpPost]
        [Route("api/MandiUser/SendNotificaionCheckOut")]
        public string SendNotificaionCheckOut(OrderBookingViewModel objOrderBookingViewModel)
        {
            var getDeviceToken = (from buyer in dbContext.Mandi_UserInfo where buyer.MobileNumber == objOrderBookingViewModel.Buyer_Mobile select buyer).FirstOrDefault();
            if (getDeviceToken != null)
            {
                if (objOrderBookingViewModel.Type == "Deal")
                {
                    string Message = "Dear Customer, your deal of " + objOrderBookingViewModel.CropName + " is approved.Please proceed to checkout.";
                    var addnotification = AddNotification(objOrderBookingViewModel, Message);
                    if (addnotification == "true")
                    {
                        string Title = "Grow Mandi, Deal is approved";
                        SendFCMNotificationWithDataToUsers(getDeviceToken.DeviceToken, Message, Title, objOrderBookingViewModel.Buyer_Mobile);

                        objResponse.Message = "Success";
                        objResponse.MobileNumber = objOrderBookingViewModel.Buyer_Mobile;
                        return objResponse.MobileNumber;
                    }
                    else
                    {

                        //objResponse.Message = "Interested product has been saved in db but notification is not added";
                        objResponse.Message = "Failed";
                        return objResponse.Message;

                    }
                }
                else
                {
                   
                    string Message = "Dear Customer, your enquiry of " + objOrderBookingViewModel.CropName + " is resolved. Please proceed to checkout.";
                    var addnotification = AddNotification(objOrderBookingViewModel, Message);
                    if (addnotification == "true")
                    {
                        string Title = "Grow Mandi, Enquiry is resolved";
                        SendFCMNotificationWithDataToUsers(getDeviceToken.DeviceToken, Message, Title, objOrderBookingViewModel.Buyer_Mobile);

                        objResponse.MobileNumber = objOrderBookingViewModel.Buyer_Mobile;
                        return objResponse.MobileNumber;
                    }
                    else
                    {

                        //objResponse.Message = "Interested product has been saved in db but notification is not added";
                        objResponse.Message = "Failed";
                        return objResponse.Message;

                    }
                }
            }
            else 
            {
                objResponse.Message = "No DeviceToken Found for this buyer";
                return objResponse.Message;
            }
           
              
           
        }

        [HttpPost]
        [Route("api/MandiUser/UpdateDealIdInCart")]
        public HttpResponseMessage UpdateDealIdInCart( CartInfoViewModel objCartInfoViewModel)
        {
            var checkCartId = (from cart in dbContext.Mandi_CartInfo where cart.CartId == objCartInfoViewModel.CartId  select cart).FirstOrDefault();
            if (checkCartId != null)
            {
                checkCartId.Deal_Id = objCartInfoViewModel.Deal_Id;
                dbContext.SaveChanges();
                objResponse.Message = "OrderId saved";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
            else {
                objResponse.Message = "No cartId found";
                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
            }
        }

        #endregion

        #region Search


        [HttpPost]

        [Route("api/MandiUser/SearchProducts")]
        public HttpResponseMessage SearchProducts(SearchViewModelForProducts objSearchViewModel)
        {

            var ServerPath = ConfigurationManager.AppSettings["ServerPath"];
            MandiProduct objListMandiProduct = new MandiProduct();
            var data = from p in dbContext.Mandi_ProductMaster
                       join cr in dbContext.Crop_Master on p.CropId equals cr.CropId
                       join v in dbContext.Variety_Master on p.VarietyId equals v.VarietyId
                       join cat in dbContext.Category on p.CategoryId equals cat.CategoryId
                       //join sc in dbContext.SubCategories on tm.SubCategoryId equals sc.Id
                       where p.IsActive == true && p.IsApproved == true
                       select new ProductMasterViewModel
                       {
                           Tr_Id = p.Tr_Id,
                           CropId = p.CropId,
                           VarietyId = p.VarietyId,
                           CropName = objSearchViewModel.culture == "En" ? cr.CropName : objSearchViewModel.culture == "Hi" ? cr.Hi_CropName == null ? cr.CropName : cr.Hi_CropName : objSearchViewModel.culture == "Mr" ? cr.Mr_CropName == null ? cr.CropName : cr.Mr_CropName : objSearchViewModel.culture == "Te" ? cr.Te_CropName == null ? cr.Te_CropName : cr.Te_CropName : cr.CropName,
                           FilterCropName = cr.CropName,
                           SCategoryId = cr.CategoryId,
                           FilterCategoryName = objSearchViewModel.culture == "En" ? cat.CategoryName : objSearchViewModel.culture == "Hi" ? cat.Hi_CategoryName == null ? cat.CategoryName : cat.Hi_CategoryName : objSearchViewModel.culture == "Mr" ? cat.Mr_CategoryName == null ? cat.CategoryName : cat.Mr_CategoryName : objSearchViewModel.culture == "Te" ? cat.Te_CategoryName == null ? cat.CategoryName : cat.Te_CategoryName : cat.CategoryName,
                           FCategoryName = cat.CategoryName,
                           ProductDescription = objSearchViewModel.culture == "En" ? p.ProductDescription : objSearchViewModel.culture == "Hi" ? p.Hi_ProductDescription == null ? p.ProductDescription : p.Hi_ProductDescription : objSearchViewModel.culture == "Mr" ? p.Mr_ProductDescription == null ? p.ProductDescription : p.ProductDescription : objSearchViewModel.culture == "Te" ? p.Te_ProductDescription == null ? p.ProductDescription : p.Te_ProductDescription : p.ProductDescription,
                           FilterProductDescription = p.ProductDescription,
                           CropEndDate = p.CropEndDate,
                           CropStatus = p.CropEndDate >= DateTime.Now ? "Available" : "Sold",
                           VarietyName = v.VarietyName,
                           ProductAddress = p.ProductAddress,
                           GeoAddress = p.GeoAddress,
                           MobileNumber = p.MobileNumber,
                           NetBankingId = p.NetBankingId,
                           Quantity = p.Quantity,
                           QuantityUnit = p.QuantityUnit,
                           Price = p.Price,
                           ServiceTax = p.ServiceTax,
                           AvailabilityDate = p.AvailabilityDate,
                           PaymentMethod = p.PaymentMethod,
                           IsQualityTestNeeded = p.IsQualityTestNeeded,
                           IsLogisticNeeded = p.IsLogisticNeeded,
                           Tr_Date = p.Tr_Date,
                           StateCode = p.State,
                           DistrictCode = p.District,
                           TalukaCode = p.Taluka,
                           IsActive = p.IsActive,
                           IsApproved = p.IsApproved,
                           ProductPriority = p.ProductPriority,
                           ProductImageUrl = ServerPath + p.ProductImageUrl,
                           SecondaryProductImage = !string.IsNullOrEmpty(ServerPath + p.SecondaryProductImage) ? ServerPath + p.SecondaryProductImage : "",

                       };

            if (!string.IsNullOrEmpty(objSearchViewModel.searchtxt))
            {
                data = data.Where(x => x.FilterCropName.Contains(objSearchViewModel.searchtxt) || x.FilterProductDescription.Contains(objSearchViewModel.searchtxt) || x.FCategoryName.Contains(objSearchViewModel.searchtxt) || x.VarietyName.Contains(objSearchViewModel.searchtxt) || x.ProductAddress.Contains(objSearchViewModel.searchtxt) || x.StateCode.Contains(objSearchViewModel.searchtxt) || x.DistrictCode.Contains(objSearchViewModel.searchtxt));
            }
            // data = data.Where(c => c.StateId == stateId);




            // var result= data.Where(x=>x.CropStatus=="Available").ToList();
            objListMandiProduct.Products = data.Where(x => x.CropStatus == "Available").ToList();
            return Request.CreateResponse(HttpStatusCode.OK, objListMandiProduct);
            // return Request.CreateResponse(HttpStatusCode.OK, result);
        }










        #endregion


        #region Cart Info

        /// <summary>
        /// for adding enquiry or interested product in cart
        /// </summary>
        /// <param name="objMandiCartViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/MandiUser/AddProductInCart")]
        public HttpResponseMessage AddProductInCart(MandiCartViewModel objMandiCartViewModel)
        {


            try
            {
                string mobileNumber = objMandiCartViewModel.BuyerContact;
                // to check if existing in cart
                if (objMandiCartViewModel.Fk_EnquiryId != null)
                {
                    if (objMandiCartViewModel.Fk_EnquiryId != 0)
                    {
                        var checkEnquiryInCart = (from enquiry in dbContext.Mandi_CartInfo where enquiry.CartType == "Enquiry" && enquiry.BuyerNumber == mobileNumber && enquiry.Product_Id == objMandiCartViewModel.Tr_Id select enquiry).FirstOrDefault();
                        if (checkEnquiryInCart != null)
                        {
                            objResponse.Message = "This Product is already added in cart";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            objMandiCartViewModel.Fk_InterestedProductId = 0;
                            var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                            if (number != null)
                            {
                                Mandi_CartInfo objMandi_CartInfo = new Mandi_CartInfo();

                                objMandi_CartInfo.CartType = objMandiCartViewModel.CartType;
                                if (objMandiCartViewModel.Fk_EnquiryId != 0)
                                {
                                    objMandi_CartInfo.Fk_EnquiryId = objMandiCartViewModel.Fk_EnquiryId;
                                }
                                else
                                {
                                    objMandi_CartInfo.Fk_InterestedProductId = objMandiCartViewModel.Fk_InterestedProductId;
                                }

                                //get Product detail



                                #region Product detail
                                var productId = objMandiCartViewModel.Tr_Id;
                                var getProductDetail = (from product in dbContext.Mandi_ProductMaster.Where(product => product.Tr_Id == productId)
                                                        join crop in dbContext.Crop_Master on product.CropId equals crop.CropId
                                                        join variety in dbContext.Variety_Master on product.VarietyId equals variety.VarietyId


                                                        select new
                                                        {

                                                            crop.CropName,
                                                            //variety.VarietyName,
                                                            product.Tr_Id,
                                                            //product.CropId,
                                                            product.VarietyId,
                                                            product.ProductAddress,
                                                            product.GeoAddress,
                                                            product.MobileNumber,
                                                            product.Quantity,
                                                            product.QuantityUnit,
                                                            product.Price,
                                                            //product.ServiceTax,
                                                            //product.CategoryId,
                                                            //crop.CategoryName,

                                                            //product.AvailabilityDate,
                                                            //product.IsQualityTestNeeded,
                                                            //product.ProductImageUrl,
                                                            //product.Tr_Date,
                                                            //product.State,
                                                            //product.District,
                                                            //product.Taluka,
                                                            //product.PaymentMethod,
                                                            //product.IsLogisticNeeded,
                                                            //product.IsActive,
                                                            //product.SecondaryProductImage,
                                                            //product.ProductDescription

                                                        }).FirstOrDefault();


                                objMandi_CartInfo.Product = getProductDetail.CropName;
                                objMandi_CartInfo.Product_Id = objMandiCartViewModel.Tr_Id;
                                objMandi_CartInfo.Quantity = getProductDetail.Quantity;
                                objMandi_CartInfo.QuantityUnit = getProductDetail.QuantityUnit;
                                objMandi_CartInfo.Price = getProductDetail.Price;
                                objMandi_CartInfo.ProductAddress = getProductDetail.ProductAddress;
                                objMandi_CartInfo.ProductImage = objMandiCartViewModel.ProductImage;

                                #endregion




                                //get buyer details
                                objMandi_CartInfo.BuyerName = objMandiCartViewModel.BuyerName;
                                objMandi_CartInfo.BuyerNumber = objMandiCartViewModel.BuyerContact;
                                objMandi_CartInfo.BuyerAddress = number.State + "," + number.District + "," + number.Taluka + "," + number.Pincode;

                                //to get seller details

                                objMandi_CartInfo.Seller_MobileNumber = getProductDetail.MobileNumber;
                                var sellerMobileNumber = getProductDetail.MobileNumber;
                                var sellerDetail = (from seller in dbContext.Mandi_UserInfo where seller.MobileNumber == sellerMobileNumber select seller).FirstOrDefault();
                                objMandi_CartInfo.SellerAddress = sellerDetail.State + "," + sellerDetail.District + "," + sellerDetail.Taluka + "," + sellerDetail.Pincode;
                                objMandi_CartInfo.SellerName = sellerDetail.FullName;

                                objMandi_CartInfo.Fk_ProductStatus = 4;






                                dbContext.Mandi_CartInfo.Add(objMandi_CartInfo);
                                var i = dbContext.SaveChanges();
                                if (i != 0)
                                {
                                    objResponse.Message = "Product has been successfully saved in cart";
                                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                                }
                                else
                                {
                                    objResponse.Message = "Failed to save product in cart";
                                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                                }


                            }
                            else
                            {
                                objResponse.Message = "Mobile number not exists.";
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }


                        }
                    }
                    else
                    {
                        objMandiCartViewModel.Fk_InterestedProductId = 0;
                        var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                        if (number != null)
                        {
                            Mandi_CartInfo objMandi_CartInfo = new Mandi_CartInfo();

                            objMandi_CartInfo.CartType = objMandiCartViewModel.CartType;
                            if (objMandiCartViewModel.Fk_EnquiryId != 0)
                            {
                                objMandi_CartInfo.Fk_EnquiryId = objMandiCartViewModel.Fk_EnquiryId;
                            }
                            else
                            {
                                objMandi_CartInfo.Fk_InterestedProductId = objMandiCartViewModel.Fk_InterestedProductId;
                            }

                            //get Product detail



                            #region Product detail
                            var productId = objMandiCartViewModel.Tr_Id;
                            var getProductDetail = (from product in dbContext.Mandi_ProductMaster.Where(product => product.Tr_Id == productId)
                                                    join crop in dbContext.Crop_Master on product.CropId equals crop.CropId
                                                    join variety in dbContext.Variety_Master on product.VarietyId equals variety.VarietyId


                                                    select new
                                                    {

                                                        crop.CropName,
                                                        //variety.VarietyName,
                                                        product.Tr_Id,
                                                        //product.CropId,
                                                        product.VarietyId,
                                                        product.ProductAddress,
                                                        product.GeoAddress,
                                                        product.MobileNumber,
                                                        product.Quantity,
                                                        product.QuantityUnit,
                                                        product.Price,
                                                        //product.ServiceTax,
                                                        //product.CategoryId,
                                                        //crop.CategoryName,

                                                        //product.AvailabilityDate,
                                                        //product.IsQualityTestNeeded,
                                                        //product.ProductImageUrl,
                                                        //product.Tr_Date,
                                                        //product.State,
                                                        //product.District,
                                                        //product.Taluka,
                                                        //product.PaymentMethod,
                                                        //product.IsLogisticNeeded,
                                                        //product.IsActive,
                                                        //product.SecondaryProductImage,
                                                        //product.ProductDescription

                                                    }).FirstOrDefault();


                            objMandi_CartInfo.Product = getProductDetail.CropName;
                            objMandi_CartInfo.Quantity = getProductDetail.Quantity;
                            objMandi_CartInfo.QuantityUnit = getProductDetail.QuantityUnit;
                            objMandi_CartInfo.Price = getProductDetail.Price;
                            objMandi_CartInfo.ProductAddress = getProductDetail.ProductAddress;
                            objMandi_CartInfo.ProductImage = objMandiCartViewModel.ProductImage;

                            #endregion




                            //get buyer details
                            objMandi_CartInfo.BuyerName = objMandiCartViewModel.BuyerName;
                            objMandi_CartInfo.BuyerNumber = objMandiCartViewModel.BuyerContact;
                            objMandi_CartInfo.BuyerAddress = number.State + "," + number.District + "," + number.Taluka + "," + number.Pincode;
                            objMandi_CartInfo.Product_Id = objMandiCartViewModel.Tr_Id;
                            //to get seller details

                            objMandi_CartInfo.Seller_MobileNumber = getProductDetail.MobileNumber;
                            var sellerMobileNumber = getProductDetail.MobileNumber;
                            var sellerDetail = (from seller in dbContext.Mandi_UserInfo where seller.MobileNumber == sellerMobileNumber select seller).FirstOrDefault();
                            objMandi_CartInfo.SellerAddress = sellerDetail.State + "," + sellerDetail.District + "," + sellerDetail.Taluka + "," + sellerDetail.Pincode;
                            objMandi_CartInfo.SellerName = sellerDetail.FullName;


                            objMandi_CartInfo.Fk_ProductStatus = 4;






                            dbContext.Mandi_CartInfo.Add(objMandi_CartInfo);
                            var i = dbContext.SaveChanges();
                            if (i != 0)
                            {
                                objResponse.Message = "Product has been successfully saved in cart";
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                            }
                            else
                            {
                                objResponse.Message = "Failed to save product in cart";
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                            }


                        }
                        else
                        {
                            objResponse.Message = "Mobile number not exists.";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }


                }
                else
                {
                    var checkInterestedProductInCart = (from interestProduct in dbContext.Mandi_CartInfo where interestProduct.Fk_InterestedProductId == objMandiCartViewModel.Fk_InterestedProductId && interestProduct.BuyerNumber == mobileNumber && interestProduct.Product_Id == objMandiCartViewModel.Tr_Id select interestProduct).FirstOrDefault();

                    if (checkInterestedProductInCart != null)
                    {
                        objResponse.Message = "This Product is already been  added in cart";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objMandiCartViewModel.Fk_EnquiryId = 0;
                        var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                        if (number != null)
                        {
                            Mandi_CartInfo objMandi_CartInfo = new Mandi_CartInfo();

                            objMandi_CartInfo.CartType = objMandiCartViewModel.CartType;
                            if (objMandiCartViewModel.Fk_EnquiryId != 0)
                            {
                                objMandi_CartInfo.Fk_EnquiryId = objMandiCartViewModel.Fk_EnquiryId;
                            }
                            else
                            {
                                objMandi_CartInfo.Fk_InterestedProductId = objMandiCartViewModel.Fk_InterestedProductId;
                            }

                            //get Product detail



                            #region Product detail
                            var productId = objMandiCartViewModel.Tr_Id;
                            var getProductDetail = (from product in dbContext.Mandi_ProductMaster.Where(product => product.Tr_Id == productId)
                                                    join crop in dbContext.Crop_Master on product.CropId equals crop.CropId
                                                    join variety in dbContext.Variety_Master on product.VarietyId equals variety.VarietyId


                                                    select new
                                                    {

                                                        crop.CropName,
                                                        //variety.VarietyName,
                                                        product.Tr_Id,
                                                        //product.CropId,
                                                        product.VarietyId,
                                                        product.ProductAddress,
                                                        product.GeoAddress,
                                                        product.MobileNumber,
                                                        product.Quantity,
                                                        product.QuantityUnit,
                                                        product.Price,
                                                        //product.ServiceTax,
                                                        //product.CategoryId,
                                                        //crop.CategoryName,

                                                        //product.AvailabilityDate,
                                                        //product.IsQualityTestNeeded,
                                                        //product.ProductImageUrl,
                                                        //product.Tr_Date,
                                                        //product.State,
                                                        //product.District,
                                                        //product.Taluka,
                                                        //product.PaymentMethod,
                                                        //product.IsLogisticNeeded,
                                                        //product.IsActive,
                                                        //product.SecondaryProductImage,
                                                        //product.ProductDescription

                                                    }).FirstOrDefault();


                            objMandi_CartInfo.Product = getProductDetail.CropName;
                            objMandi_CartInfo.Product_Id = objMandiCartViewModel.Tr_Id;
                            objMandi_CartInfo.Quantity = getProductDetail.Quantity;
                            objMandi_CartInfo.QuantityUnit = getProductDetail.QuantityUnit;

                            objMandi_CartInfo.Price = getProductDetail.Price;
                            objMandi_CartInfo.ProductAddress = getProductDetail.ProductAddress;
                            objMandi_CartInfo.ProductImage = objMandiCartViewModel.ProductImage;


                            #endregion




                            //get buyer details
                            objMandi_CartInfo.BuyerName = objMandiCartViewModel.BuyerName;
                            objMandi_CartInfo.BuyerNumber = objMandiCartViewModel.BuyerContact;
                            objMandi_CartInfo.BuyerAddress = number.State + "," + number.District + "," + number.Taluka + "," + number.Pincode;

                            //to get seller details

                            objMandi_CartInfo.Seller_MobileNumber = getProductDetail.MobileNumber;
                            var sellerMobileNumber = getProductDetail.MobileNumber;
                            var sellerDetail = (from seller in dbContext.Mandi_UserInfo where seller.MobileNumber == sellerMobileNumber select seller).FirstOrDefault();
                            objMandi_CartInfo.SellerAddress = sellerDetail.State + "," + sellerDetail.District + "," + sellerDetail.Taluka + "," + sellerDetail.Pincode;
                            objMandi_CartInfo.SellerName = sellerDetail.FullName;


                            objMandi_CartInfo.Fk_ProductStatus = 4;






                            var add = dbContext.Mandi_CartInfo.Add(objMandi_CartInfo);
                            var i = dbContext.SaveChanges();
                            if (i != 0)
                            {
                                objResponse.Message = "Product has been successfully saved in cart";
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                            }
                            else
                            {
                                objResponse.Message = "Failed to save product in cart";
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                            }


                        }
                        else
                        {
                            objResponse.Message = "Mobile number not exists.";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                }





            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                throw ex;

            }
        }



        /// <summary>
        /// to get list of cart item for resp user
        /// </summary>
        /// <param name="objMandiCartViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/MandiUser/GetCartDetailForUser")]
        public HttpResponseMessage GetCartDetailForUser(MandiCartViewModel objMandiCartViewModel)
        {
            try
            {

                var ServerPath = ConfigurationManager.AppSettings["ServerPath"];
                Mandi_CartInfoViewModel Mandi_CartInfoViewModel = new Mandi_CartInfoViewModel();
                List<Cart_InfoForUser> objListCart_InfoForUser = new List<Cart_InfoForUser>();

                var getCartDetails = (from cart in dbContext.Mandi_CartInfo where cart.BuyerNumber == objMandiCartViewModel.BuyerContact select cart).ToList();
              
                if (objMandiCartViewModel.CartType == "Enquiry")
                {
                    if (getCartDetails != null)
                    {
                        foreach (var i in getCartDetails)
                        {
                            var getProductTerm = (from terms in dbContext.Mandi_ProductMaster where terms.Tr_Id == i.Product_Id select terms.TermsAndCondition).FirstOrDefault();
                            Cart_InfoForUser objCart_InfoForUser = new Cart_InfoForUser()
                            {
                                CartId = i.CartId,
                                CartType = i.CartType,
                                Fk_EnquiryId = i.Fk_EnquiryId,
                                Fk_InterestedProductId = i.Fk_InterestedProductId,
                                Product = i.Product,
                                Quantity = i.Quantity,
                                QuantityUnit = i.QuantityUnit,
                                Price = i.Price,
                                Taxes = i.Taxes,
                                ServiceTax = i.ServiceTax,
                                SellerName = i.SellerName,
                                TermsAndCondition= getProductTerm,
                                Seller_MobileNumber = i.SellerName,
                                Logistics_Cost = i.Logistics_Cost,
                                Other_Charges = i.Other_Charges,
                                ProductAddress = i.ProductAddress,
                                ProductImage = i.ProductImage,
                                Product_Id = i.Product_Id,
                                ProducStatusId = i.Mandi_CartProductStatus.Id,
                                ProductStatus = i.Mandi_CartProductStatus.ProductStatus

                            };
                            objListCart_InfoForUser.Add(objCart_InfoForUser);
                        }
                        Mandi_CartInfoViewModel.MandiCart_Info = objListCart_InfoForUser.Where(x => x.CartType == "Enquiry" && x.Fk_EnquiryId != null).ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, Mandi_CartInfoViewModel);
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {

                    {
                        if (getCartDetails != null)
                        {
                            foreach (var i in getCartDetails)
                            {
                                var getProductTerm = (from terms in dbContext.Mandi_ProductMaster where terms.Tr_Id == i.Product_Id select terms.TermsAndCondition).FirstOrDefault();

                                Cart_InfoForUser objCart_InfoForUser = new Cart_InfoForUser()
                                {
                                    CartId = i.CartId,
                                    CartType = i.CartType,
                                    Fk_EnquiryId = i.Fk_EnquiryId,
                                    Fk_InterestedProductId = i.Fk_InterestedProductId,
                                    Product = i.Product,
                                    TermsAndCondition= getProductTerm,
                                    Quantity = i.Quantity,
                                    QuantityUnit = i.QuantityUnit,
                                    Price = i.Price,
                                    Taxes = i.Taxes,
                                    ServiceTax = i.ServiceTax,
                                    Product_Id = i.Product_Id,
                                    SellerName = i.SellerName,
                                    Logistics_Cost = i.Logistics_Cost,
                                    Other_Charges = i.Other_Charges,
                                    ProducStatusId = i.Mandi_CartProductStatus.Id,
                                    ProductAddress = i.ProductAddress,
                                    ProductImage = i.ProductImage,
                                    ProductStatus = i.Mandi_CartProductStatus.ProductStatus


                                };
                                objListCart_InfoForUser.Add(objCart_InfoForUser);
                            }
                            Mandi_CartInfoViewModel.MandiCart_Info = objListCart_InfoForUser.Where(x => x.CartType == "InterestedProduct" && x.Fk_InterestedProductId != null).ToList();

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CartInfoViewModel);
                        }
                        else
                        {
                            objResponse.Message = "Failed";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                }













            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "GetCartInfo");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }


        [HttpPost]
        //[Authorize]
        [Route("api/MandiUser/DeleteCartProduct")]
        public HttpResponseMessage DeleteCartProduct(MandiCartViewModel objMandiCartViewModel)
        {
            try
            {
                long cartId = objMandiCartViewModel.CartId;


                Mandi_CartInfo objMandi_CartInfo = new Mandi_CartInfo();


                //get product from database.
                var sellerProduct = (from cart in dbContext.Mandi_CartInfo where cart.CartId == cartId select cart).FirstOrDefault();
                if (sellerProduct != null)
                {
                    //sellerProduct.IsActive = false;
                    //dbContext.Entry(sellerProduct).State = EntityState.Modified;

                    dbContext.Mandi_CartInfo.Remove(sellerProduct);
                    var i = dbContext.SaveChanges();
                    if (i != 0)
                    {
                        objResponse.Message = "Product deleted from cart successfully";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                    else
                    {
                        objResponse.Message = "Product deletion Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "No product Found";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }
            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiUser", "DeletecartInProduct");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

        }





        #endregion

        #region Commented Code
        //public List<ProductMasterViewModel> SellersDashboard(string mobileNumber)
        //{
        //    MandiProduct objListMandiProduct = new MandiProduct();
        //    List<ProductMasterViewModel> objListProductMasterViewModel = new List<ProductMasterViewModel>();

        //    var getproduct = (from product in dbContext.Mandi_ProductMaster.Where(product => product.IsActive == true && product.MobileNumber == mobileNumber)
        //                      join crop in dbContext.Crop_Master on product.CropId equals crop.CropId
        //                      join variety in dbContext.Variety_Master on product.VarietyId equals variety.VarietyId
        //                      orderby product.ProductPriority == "1"
        //                      orderby product.ProductPriority == "2"
        //                      orderby product.ProductPriority == "0"
        //                      select new
        //                      {
        //                          crop.CropName,
        //                          variety.VarietyName,
        //                          product.Tr_Id,
        //                          product.CropId,
        //                          product.VarietyId,
        //                          product.ProductAddress,
        //                          product.GeoAddress,
        //                          product.MobileNumber,
        //                          product.NetBankingId,
        //                          product.Quantity,
        //                          product.QuantityUnit,
        //                          product.Price,
        //                          product.AvailabilityDate,
        //                          product.IsQualityTestNeeded,
        //                          product.IsLogisticNeeded,
        //                          product.ProductImageUrl,
        //                          product.Tr_Date,
        //                          product.State,
        //                          product.District,
        //                          product.Taluka,
        //                          product.IsActive,
        //                          product.ProductDescription,
        //                          product.SecondaryProductImage
        //                      }).ToList();


        //    if (getproduct != null)
        //    {
        //        foreach (var i in getproduct)
        //        {
        //            ProductMasterViewModel objProductMasterViewModel = new ProductMasterViewModel()
        //            {
        //                Tr_Id = i.Tr_Id,
        //                CropId = i.CropId,
        //                VarietyId = i.VarietyId,
        //                CropName = i.CropName,
        //                VarietyName = i.VarietyName,
        //                ProductAddress = i.ProductAddress,
        //                GeoAddress = i.GeoAddress,
        //                MobileNumber = i.MobileNumber,
        //                NetBankingId = i.NetBankingId,
        //                Quantity = i.Quantity,
        //                QuantityUnit = i.QuantityUnit,
        //                Price = Convert.ToInt32(i.Price),
        //                AvailabilityDate = i.AvailabilityDate,
        //                //PaymentMethod = i.PaymentMethod,
        //                IsQualityTestNeeded = i.IsQualityTestNeeded,
        //                IsLogisticNeeded = i.IsLogisticNeeded,
        //                ProductImageUrl = i.ProductImageUrl,
        //                Tr_Date = i.Tr_Date,
        //                StateCode = i.State,
        //                DistrictCode = i.District,
        //                TalukaCode = i.Taluka,
        //                IsActive = i.IsActive,
        //                ProductDescription = !string.IsNullOrEmpty(i.ProductDescription) ? i.ProductDescription : "",
        //                SecondaryProductImage = !string.IsNullOrEmpty(i.SecondaryProductImage) ? i.SecondaryProductImage : "",
        //                NewVariety = ""
        //            };
        //            objListProductMasterViewModel.Add(objProductMasterViewModel);
        //        }
        //        objListMandiProduct.Products = objListProductMasterViewModel;
        //        return objListMandiProduct.Products;
        //    }
        //    return objListMandiProduct.Products;

        //}





        //var productss = (from c in dbContext.Crop_Master
        //               join v in dbContext.Variety_Master on c.CropId equals v.CropId
        //               join p in dbContext.Mandi_ProductMaster on v.VarietyId equals p.VarietyId
        //               join pd in dbContext.Mandi_OrderProductDetails on p.Tr_Id equals pd.Product_Id
        //               join o in dbContext.OrderDetails on pd.Order_Id equals o.Order_Id

        //               select new ProductMasterViewModel()
        //               {
        //                   Tr_Id = p.Tr_Id,
        //                   CropId = c.CropId,
        //                   VarietyId = v.VarietyId,
        //                   CropName = c.CropName,
        //                   VarietyName = v.VarietyName,
        //                   ProductAddress = p.ProductAddress,
        //                   GeoAddress = p.GeoAddress,
        //                   MobileNumber = p.MobileNumber,
        //                   NetBankingId = p.NetBankingId,
        //                   Quantity = p.Quantity,
        //                   QuantityUnit = p.QuantityUnit,
        //                   Price = p.Price,
        //                   ServiceTax = p.ServiceTax,
        //                   AvailabilityDate = p.AvailabilityDate,
        //                   PaymentMethod = p.PaymentMethod,
        //                   IsQualityTestNeeded = p.IsQualityTestNeeded,
        //                   IsLogisticNeeded = p.IsLogisticNeeded,
        //                   ProductImageUrl = p.ProductImageUrl,
        //                   Tr_Date = p.Tr_Date,
        //                   StateCode = p.State,
        //                   DistrictCode = p.District,
        //                   TalukaCode = p.Taluka,

        //                   IsActive = p.IsActive,
        //                   ProductPriority = p.ProductPriority,
        //                   ProductDescription = !string.IsNullOrEmpty(p.ProductDescription) ? p.ProductDescription : "",
        //                   SecondaryProductImage = !string.IsNullOrEmpty(p.SecondaryProductImage) ? p.SecondaryProductImage : "",
        //                   NewVariety = ""
        //               }).ToList(
        #endregion


    }


}
