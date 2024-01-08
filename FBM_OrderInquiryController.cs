using Gs3PLv9MOBAPI.Models.Entity;
using Gs3PLv9MOBAPI.Models.ViewModel;
using Gs3PLv9MOBAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Gs3PLv9MOBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FBM_OrderInquiryController : ControllerBase
    {
        
        [HttpGet]
        [Route("GetFBMOrderInquiryDetails")]
        public FBMOutboundResult GetFBMOrderInquiryDetails()
        {
            FBMOutboundResult responseModel = new FBMOutboundResult();
            LoginResult objCompany = new LoginResult();
            List<ItemList> item = new List<ItemList>();
            objCompany.UserId = "pm";
            //item = this.Company(objCompany);  
            responseModel.Itemlist = this.Company(objCompany);
            FBMOrderInquiryServices ObjObScanOutService = new FBMOrderInquiryServices();
            responseModel.Orderresult = ObjObScanOutService.GetProcessedOrderDetails();
            responseModel.PrintOrderresult = ObjObScanOutService.GetPrintOrderDetails();
            responseModel.ProcessOrderresult = ObjObScanOutService.GetProcessOrderDetails();
            return responseModel; 
        }

        CompanyService objCompanyService = new CompanyService();
        [HttpGet]
        public List<ItemList> Company([FromBody] LoginResult objCompany)
        {
            CompanyObject _obj = new CompanyObject();
            Company _objcompay = new Company() { user_id = objCompany.UserId };
            var result = objCompanyService.GetPickCompanyDetails(_objcompay);
            var _objList = new List<ItemList>();
            if (result.ListCompanyPickDtl != null)
            {
                
                for (int i = 0; i < result.ListCompanyPickDtl.Count; i++)
                {
                    ItemList _item = new ItemList();
                    _item.ItemId = result.ListCompanyPickDtl[i].cmp_id;
                    _item.ItemName = result.ListCompanyPickDtl[i].cmp_name;
                    _objList.Add(_item);
                }
                _obj.CompanyList = _objList;
                //_obj.WhSCode = objCompanyService.GetWhsIdDetails(objCompany.CompanyId);

            }
            return _objList;
        }

        [HttpPost]
        [Route("GetFBMOrderFilter")]
        public List<FBMOrderInquiryModel> GetFBMOrderFilter(SearchFilter model)
        {           
            FBMOrderInquiryServices ObjObScanOutService = new FBMOrderInquiryServices();
            var result = ObjObScanOutService.GetFilterDetails(model);

            return result;
        }
    } 
}
