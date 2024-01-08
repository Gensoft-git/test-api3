
using Gs3PLv9MOBAPI.Models.Entity;
using Gs3PLv9MOBAPI.Models.ViewModel;
using Gs3PLv9MOBAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Gs3PLv9MOBAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : Controller
    {
        CompanyService objCompanyService = new CompanyService();
        [HttpPost]
        public CompanyObject Company([FromBody] LoginResult objCompany)
        {
            CompanyObject _obj = new CompanyObject();
            Company _objcompay = new Company() { user_id = objCompany.UserId };
            var result = objCompanyService.GetPickCompanyDetails(_objcompay);
            if (result.ListCompanyPickDtl != null)
            {
                var _objList = new List<ItemList>();
                for (int i = 0; i < result.ListCompanyPickDtl.Count; i++)
                {
                    ItemList _item = new ItemList();
                    _item.ItemId = result.ListCompanyPickDtl[i].cmp_id;
                    _item.ItemName = result.ListCompanyPickDtl[i].cmp_name;
                    _objList.Add(_item);
                }
                _obj.CompanyList = _objList;
                _obj.WhSCode = objCompanyService.GetWhsIdDetails(objCompany.CompanyId);

            }
            return _obj;
        }

    }
}
