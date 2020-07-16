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
        public HttpResponseMessage GetCrop(int CategoryId = 0, int Counter = 0, string culture="")
        {
            try
            {
                int counter = Counter;
                int take = 6;
                int skip = counter;

                Mandi_CropMaster Mandi_CropMaster = new Mandi_CropMaster();
                List<MandiCrop> objListMandiCrop = new List<MandiCrop>();

                if (CategoryId == 0)
                {
                    if (culture == "En")
                    {

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
                                    CategoryImage = i.CropImage,
                                    EnCropName=i.CropName,
                                    CropImage = "https://mahycoapp.siplsolutions.com/Images/SubCategories/" + i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList();

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else if (culture == "Hi")
                    {
                        //For getting list of crop  from the table.
                        var getCrop = (from crop in dbContext.Crop_Master select crop).ToList();
                        if (getCrop != null)
                        {
                            foreach (var i in getCrop)
                            {
                                MandiCrop objMandi_Crop = new MandiCrop()
                                {
                                    CropId = i.CropId,
                                    EnCropName = i.CropName,
                                    CropName = i.Hi_CropName == null ? i.CropName : i.Hi_CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = "https://mahycoapp.siplsolutions.com/Images/SubCategories/"+ i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList();

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else if (culture == "Mr")
                    {
                        //For getting list of crop  from the table.
                        var getCrop = (from crop in dbContext.Crop_Master select crop).ToList();
                        if (getCrop != null)
                        {
                            foreach (var i in getCrop)
                            {
                                MandiCrop objMandi_Crop = new MandiCrop()
                                {
                                    CropId = i.CropId,
                                    EnCropName = i.CropName,
                                    CropName = i.Mr_CropName == null ? i.CropName : i.Mr_CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = "https://mahycoapp.siplsolutions.com/Images/SubCategories/" + i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList();

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else if (culture == "Te")
                    {
                        //For getting list of crop  from the table.
                        var getCrop = (from crop in dbContext.Crop_Master select crop).ToList();
                        if (getCrop != null)
                        {
                            foreach (var i in getCrop)
                            {
                                MandiCrop objMandi_Crop = new MandiCrop()
                                {
                                    CropId = i.CropId,
                                    CropName = i.Te_CropName == null ? i.CropName : i.Te_CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList();

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
                                    EnCropName = i.CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = "https://mahycoapp.siplsolutions.com/Images/SubCategories/" + i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList();

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }





                }
                else
                {

                    if (culture == "En")
                    {
                        var categoryIdString = CategoryId;


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
                                    EnCropName = i.CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = "https://mahycoapp.siplsolutions.com/Images/SubCategories/" + i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList(); ;

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed to get crop in reference of category ";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else if (culture == "Hi")
                    {
                        var categoryIdString = CategoryId;


                        //For getting list of crop  from the table.
                        var getCropbyCategoryId = (from crop in dbContext.Crop_Master where crop.CategoryId == categoryIdString select crop).ToList();
                        if (getCropbyCategoryId != null)
                        {
                            foreach (var i in getCropbyCategoryId)
                            {
                                MandiCrop objMandi_Crop = new MandiCrop()
                                {
                                    CropId = i.CropId,
                                    EnCropName = i.CropName,
                                    CropName = i.Hi_CropName == null ? i.CropName : i.Hi_CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = "https://mahycoapp.siplsolutions.com/Images/SubCategories/" + i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList(); ;

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed to get crop in reference of category ";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else if (culture == "Mr")
                    {
                        var categoryIdString = CategoryId;


                        //For getting list of crop  from the table.
                        var getCropbyCategoryId = (from crop in dbContext.Crop_Master where crop.CategoryId == categoryIdString select crop).ToList();
                        if (getCropbyCategoryId != null)
                        {
                            foreach (var i in getCropbyCategoryId)
                            {
                                MandiCrop objMandi_Crop = new MandiCrop()
                                {
                                    CropId = i.CropId,
                                    EnCropName = i.CropName,
                                    CropName = i.Mr_CropName == null ? i.CropName : i.Mr_CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = "https://mahycoapp.siplsolutions.com/Images/SubCategories/" + i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList(); ;

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed to get crop in reference of category ";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else if (culture == "Te")
                    {
                        var categoryIdString = CategoryId;


                        //For getting list of crop  from the table.
                        var getCropbyCategoryId = (from crop in dbContext.Crop_Master where crop.CategoryId == categoryIdString select crop).ToList();
                        if (getCropbyCategoryId != null)
                        {
                            foreach (var i in getCropbyCategoryId)
                            {
                                MandiCrop objMandi_Crop = new MandiCrop()
                                {
                                    CropId = i.CropId,
                                    CropName = i.Te_CropName == null ? i.CropName : i.Te_CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList(); ;

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed to get crop in reference of category ";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                    }
                    else
                    {
                        var categoryIdString = CategoryId;


                        //For getting list of crop  from the table.
                        var getCropbyCategoryId = (from crop in dbContext.Crop_Master where crop.CategoryId == categoryIdString select crop).ToList();
                        if (getCropbyCategoryId != null)
                        {
                            foreach (var i in getCropbyCategoryId)
                            {
                                MandiCrop objMandi_Crop = new MandiCrop()
                                {
                                    CropId = i.CropId,
                                    EnCropName = i.CropName,
                                    CropName = i.CropName,
                                    CategoryImage = i.CropImage,
                                    CropImage = "https://mahycoapp.siplsolutions.com/Images/SubCategories/" + i.CropImage

                                };
                                objListMandiCrop.Add(objMandi_Crop);
                            }
                            Mandi_CropMaster.MandiCrops = objListMandiCrop.OrderBy(x => x.CropName).Skip(skip).Take(take).ToList(); ;

                            return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);
                        }
                        else
                        {
                            objResponse.Message = "Failed to get crop in reference of category ";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
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
        [Route("api/MandiMaster/GetAllRoles")]
        public HttpResponseMessage GetAllRoles()
        {
            try
            {

                MandiUserRoles Mandi_CropMaster = new MandiUserRoles();
                List<MandiRolesForUser> objListMandiCrop = new List<MandiRolesForUser>();

                var getRoles = (from role in dbContext.Roles select role).ToList();
                if (getRoles != null)
                {


                    foreach (var i in getRoles)
                    {
                        MandiRolesForUser objMandi_Crop = new MandiRolesForUser()
                        {
                            RoleName = i.RoleName
                           


                        };
                        objListMandiCrop.Add(objMandi_Crop);
                    }
                }
                else
                {
                    objResponse.Message = "Failed to get categories";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }





                return Request.CreateResponse(HttpStatusCode.OK, "Success");





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
        //[Authorize]
        [Route("api/MandiMaster/GetAllCategory")]
        public HttpResponseMessage GetAllCategory(string culture="")
        {
            try
            { 

                
                Mandi_CropMaster Mandi_CropMaster = new Mandi_CropMaster();
                List<MandiCrop> objListMandiCrop = new List<MandiCrop>();
                if (culture == "En")
                {
                    var getCategory = (from category in dbContext.Category select category).ToList();
                    if (getCategory != null)
                    {


                        foreach (var i in getCategory)
                        {
                            MandiCrop objMandi_Crop = new MandiCrop()
                            {
                                CategoryName = i.CategoryName,
                                CategoryId = i.CategoryId,
                                EnCategoryName=i.CategoryName,
                                CategoryImage = "https://mahycoapp.siplsolutions.com/Images/Categories/" + i.CategoryImage


                            };
                            objListMandiCrop.Add(objMandi_Crop);
                        }
                    }
                    else
                    {
                        objResponse.Message = "Failed to get categories";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else if (culture == "Hi")
                {
                    var getCategory = (from category in dbContext.Category select category).ToList();
                    if (getCategory != null)
                    {
                        foreach (var i in getCategory)
                        {
                            MandiCrop objMandi_Crop = new MandiCrop()
                            {
                                CategoryName = i.Hi_CategoryName,
                                EnCategoryName = i.CategoryName,
                                CategoryId = i.CategoryId,
                                CategoryImage = "https://mahycoapp.siplsolutions.com/Images/Categories/" + i.CategoryImage


                            };
                            objListMandiCrop.Add(objMandi_Crop);
                        }
                    }
                    else
                    {
                        objResponse.Message = "Failed to get categories";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else if (culture == "Mr")
                {
                    var getCategory = (from category in dbContext.Category select category).ToList();
                    if (getCategory != null)
                    {
                        foreach (var i in getCategory)
                        {
                            MandiCrop objMandi_Crop = new MandiCrop()
                            {
                                CategoryName = i.Mr_CategoryName,
                                EnCategoryName = i.CategoryName,
                                CategoryId = i.CategoryId,
                                CategoryImage = "https://mahycoapp.siplsolutions.com/Images/Categories/" + i.CategoryImage


                            };
                            objListMandiCrop.Add(objMandi_Crop);
                        }
                    }
                    else
                    {
                        objResponse.Message = "Failed to get categories";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else if (culture == "Te")
                {
                    var getCategory = (from category in dbContext.Category select category).ToList();
                    if (getCategory != null)
                    {
                        foreach (var i in getCategory)
                        {
                            MandiCrop objMandi_Crop = new MandiCrop()
                            {
                                CategoryName = i.Te_CategoryName,
                                EnCategoryName = i.CategoryName,
                                CategoryId = i.CategoryId,
                                CategoryImage ="https://mahycoapp.siplsolutions.com/Images/Categories/" + i.CategoryImage


                            };
                            objListMandiCrop.Add(objMandi_Crop);
                        }

                    }
                    else
                    {
                        objResponse.Message = "Failed to get categories";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }


                Mandi_CropMaster.MandiCrops = objListMandiCrop;

                return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);



                

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
        //[Authorize]
        [Route("api/MandiMaster/GetAllCategories")]
        public HttpResponseMessage GetAllCategories()
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
                                CategoryImage = i.CategoryImage


                            };
                            objListMandiCrop.Add(objMandi_Crop);
                        }
                    }
                    else
                    {
                        objResponse.Message = "Failed to get categories";
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                
               


                Mandi_CropMaster.MandiCrops = objListMandiCrop;

                return Request.CreateResponse(HttpStatusCode.OK, Mandi_CropMaster);





            }
            catch (Exception ex)
            { 
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "MandiMasterController", "GetCategory");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.InnerException);
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
