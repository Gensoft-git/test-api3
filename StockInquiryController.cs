using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

using Gs3PLv9MOBAPI.Models.ViewModel;
using Gs3PLv9MOBAPI.Services.Interface;
using Gs3PLv9MOBAPI.Models;
using Gs3PLv9MOBAPI.Services;

namespace Gs3PLv9MOBAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockInquiryController : Controller
    {
        IStockInquiryService objStockInquiryService = new StockInquiryService();

        //[HttpPost]
        //public StockInquiryDtlModel GetStockInquiry([FromBody] StockSearchRequest obj)
        //{
        //    return objStockInquiryService.DashboardStockInquiryGrid(obj.CompanyId, obj.LocId, obj.Style, obj.Color, obj.Size);

        //}
        [HttpGet]
        public IEnumerable<string> GetStyleList(string companyId)
        {

        //        public string itm_num { get; set; }
        //public string itm_color { get; set; }
        //public string itm_size { get; set; }
        //public string Itmdtl { get; set; }
        //public string itm_name { get; set; }
        //public string itm_code { get; set; }
        StockChangeService ServiceObject = new StockChangeService();
            var result = ServiceObject.ItemXGetitmDetails(string.Empty, companyId).LstItmxCustdtl.
                Select(x => x.itm_num).Distinct();
            return result;
        }

       



        [HttpPost]
        public StockRespModel GetStockInquiry([FromBody] StockSearchRequest obj)
        {
            
            var rObj = objStockInquiryService.DashboardStockInquiryGrid(obj.CompanyId, obj.LocId, obj.Style, obj.Color, obj.Size);

            StockRespModel respObj = new StockRespModel()
            {
                TotalRecords = rObj.ListStockInquiryGrid.Count,
                TotalAvlQty = rObj.AvlQty,
                TotalAlocQty = rObj.LstStockInquirystock.Sum(x => x.AlocQty)

            };
            respObj.StyleList = rObj.ListStockInquiryGrid.GroupBy(x => x.itm_num).
                OrderBy(y => y.Key).Select(x => new StyleItem
                {
                    ItemName = x.Key,
                    ItemList = x.Select(y => new StockDetail
                    {

                        Color = y.itm_color,
                        Size = y.itm_size,
                        LocId = y.loc_id,
                        PPK = y.pkg_qty,
                        AvlCtn = y.TOT_CTN,
                        AvlQty = y.tot_qty,
                        RecvdDt = y.rcvd_dt,
                        PoNum = y.po_num
                    }).ToList()
                }).ToList();

            return respObj;

        }


        //[HttpPost]
        //public StockChangeModel GetStockInquiry([FromBody] StockInquiryModel objInput)
        //{
        //    objInput.cmp_id = "JF2015";
        //    StockChangeModel result = new StockChangeModel();

        //    LookUp objLookUp = new LookUp();
        //    objLookUp.id = "5";
        //    objLookUp.lookuptype = "INVENTORYINQ";


        //    return result;
        //}


    }
}
