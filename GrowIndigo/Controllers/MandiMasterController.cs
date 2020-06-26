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
using static GrowIndigo.Models.MandiMasterData;

namespace GrowIndigo.Controllers
{
    //[Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MandiMasterController : ApiController
    {
        GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities();
        Authentication auth = new Authentication();
        CommonClasses objCommonClasses = new CommonClasses();
        SuccessResponse objResponse = new SuccessResponse();

        [HttpGet]
      
        [Route("api/MandiMaster/GetCrop")]
        public HttpResponseMessage GetCrop(int CategoryId=0 )
        {
            try
            {
                if (CategoryId == 0)
                {
                    Mandi_CropMaster Mandi_CropMaster = new Mandi_CropMaster();
                    List<MandiCrop> objListMandiCrop = new List<MandiCrop>();
                    //For getting list of crop  from the table.
                    var getCrop = (from crop in dbContext.Crop_Master select crop).ToList();
                    if (getCrop != null)
                    {
                        foreach (var i in getCrop)
                        {
                            MandiCrop objMandi_Crop = new MandiCrop()
                            {
                                CropId = i.CropId,
                                CropName = i.CropName,
                                CategoryImage=i.CropImage,
                                CropImage = i.CropImage

                            };
                            objListMandiCrop.Add(objMandi_Crop);
                        }
                        Mandi_CropMaster.MandiCrops = objListMandiCrop;

                        return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }

                }
                else
                {
                    Mandi_CropMaster Mandi_CropMaster = new Mandi_CropMaster();
                    List<MandiCrop> objListMandiCrop = new List<MandiCrop>();
                  
                        var categoryIdString = CategoryId.ToString();
                      
                   
                    //For getting list of crop  from the table.
                    var getCropbyCategoryId = (from crop in dbContext.Crop_Master where crop.CategoryId == categoryIdString select crop).ToList();
                    if (getCropbyCategoryId != null)
                    {
                        foreach (var i in getCropbyCategoryId)
                        {
                            MandiCrop objMandi_Crop = new MandiCrop()
                            {
                                CropId = i.CropId,
                                CropName = i.CropName,
                                CategoryImage = i.CropImage,
                                CropImage = i.CropImage

                            };
                            objListMandiCrop.Add(objMandi_Crop);
                        }
                        Mandi_CropMaster.MandiCrops = objListMandiCrop;

                        return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                    }
                    else
                    {
                        objResponse.Message = "Failed to get crop in reference of category ";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }




                }

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiMasterController", "GetCrop");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        [HttpGet]
        //[Authorize]
        [Route("api/MandiMaster/GetAllCategory")]
        public HttpResponseMessage GetAllCategory()
        {
            try
            {
                Mandi_CropMaster Mandi_CropMaster = new Mandi_CropMaster();
                List<MandiCrop> objListMandiCrop = new List<MandiCrop>();

                var getCategory = (from category in dbContext.Category select category).ToList();
                if (getCategory != null)
                {


                    foreach (var i in getCategory)
                    {
                        MandiCrop objMandi_Crop = new MandiCrop()
                        {
                            CategoryName = i.CategoryName,
                            CategoryId = i.CategoryId,
                            CategoryImage=i.CategoryImage


                        };
                        objListMandiCrop.Add(objMandi_Crop);
                    }
                    Mandi_CropMaster.MandiCrops = objListMandiCrop;

                    return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                }
                else
                {
                    objResponse.Message = "Failed to get categories";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }

               ;

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiMasterController", "GetCategory");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

      

        [HttpGet]
        [Authorize]
        [Route("api/MandiMaster/GetVariety")]
        public HttpResponseMessage GetVariety(int CropId)
        {
            try
            {
                Mandi_VarietyMaster objListMandi_VarietyMaster = new Mandi_VarietyMaster();
                List<MandiVariety> objListMandiVariety = new List<MandiVariety>();
                //For getting list of variety from the table.
                var getVariety = (from variety in dbContext.Variety_Master where variety.CropId == CropId select variety).ToList();
                if (getVariety != null)
                {
                    foreach (var i in getVariety)
                    {
                        MandiVariety objMandiVariety = new MandiVariety()
                        {
                            VarietyId = i.VarietyId,
                            VarietyName = i.VarietyName
                        };
                        objListMandiVariety.Add(objMandiVariety);
                    }

                    objListMandi_VarietyMaster.MandiVarieties = objListMandiVariety;

                    return Request.CreateResponse(HttpStatusCode.OK, objListMandi_VarietyMaster);
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
                objCommonClasses.InsertExceptionDetails(ex, "MandiMasterController", "GetVariety");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        public string AddNewVariety(ProductMasterViewModel objProductMasterViewModel)
        {

            var existingVarietyId = (from variety in dbContext.Variety_Master where variety.VarietyName == objProductMasterViewModel.NewVariety && variety.CropId == objProductMasterViewModel.CropId select variety.VarietyId).FirstOrDefault();

            var checkVariety = (from variety in dbContext.Variety_Master where variety.VarietyName == objProductMasterViewModel.NewVariety && variety.CropId == objProductMasterViewModel.CropId select variety.VarietyName).FirstOrDefault();
            if (string.IsNullOrEmpty(checkVariety))
            {
                Variety_Master objVariety_Master = new Variety_Master();

                objVariety_Master.VarietyName = objProductMasterViewModel.NewVariety;
                objVariety_Master.CropId = Convert.ToInt32(objProductMasterViewModel.CropId);

                dbContext.Variety_Master.Add(objVariety_Master);
                var j = dbContext.SaveChanges();
                int VarietyId = objVariety_Master.VarietyId; // Get OrderId After Save Changes
                if (j != 0)
                {
                    objProductMasterViewModel.VarietyId = VarietyId;
                    objResponse.Message = "Variety Added successfully";
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
                objProductMasterViewModel.VarietyId = existingVarietyId;
                objResponse.Message = "Variety already exists";
                return objResponse.Message;
            }


        }

    }
}
