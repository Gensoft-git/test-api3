using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gs3PLv9MOBAPI.Common;
using Gs3PLv9MOBAPI.Models;
using Gs3PLv9MOBAPI.Models.Entity;
using Gs3PLv9MOBAPI.Models.ViewModel;
using Gs3PLv9MOBAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gs3PLv9MOBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockChangeController : ControllerBase
    {
        [HttpPost]
        [Route("Search")]
        public StockChangeModel DashboardItemMoveGrid([FromBody] StockSearchRequest obj)
        {
            StockChangeModel StockChangeModel;
            string p_str_cmpid = obj.CompanyId;
            string p_str_itm_code = string.Empty;
            string p_str_paletid = string.Empty;
            string p_str_ponum = string.Empty;
            string p_str_LocId = obj.LocId;
            string p_str_style = obj.Style;
            string p_str_Color = obj.Color;
            string p_str_Size = obj.Size;

            StockChange objStockChange = new StockChange();
            StockChangeService objService = new StockChangeService();
            objStockChange.cmp_id = p_str_cmpid.Trim();
            objStockChange.itm_num = p_str_style.Trim();
            objStockChange.itm_color = p_str_Color.Trim();
            objStockChange.itm_size = p_str_Size.Trim();
            objStockChange = objService.GetItemCode(objStockChange);
            if (objStockChange.LstItmCodetdtl.Count > 0)
            {
                objStockChange.ItmCode = objStockChange.LstItmCodetdtl[0].ItmCode;
                objStockChange.itm_name = objStockChange.LstItmCodetdtl[0].itm_name;
            }
            p_str_itm_code = objStockChange.ItmCode;
            string p_str_success = string.Empty;
            objStockChange.itm_code = p_str_itm_code.Trim();
            objStockChange.palet_id = p_str_paletid.Trim();
            objStockChange.po_num = p_str_ponum.Trim();
            objStockChange.to_loc = p_str_LocId.Trim();
            // Session["g_str_Search_flag"] = "True";
            objStockChange = objService.GetStockChangeDetails(objStockChange);
            Company objCompany = new Company();
            CompanyService ServiceObjectCompany = new CompanyService();
            objCompany.cmp_id = p_str_cmpid;
            objCompany.user_id = obj.UserId;
            objCompany.cust_cmp_id = p_str_cmpid;
            objCompany = ServiceObjectCompany.GetPickCompanyDetails(objCompany);
            objStockChange.ListCompanyPickDtl = objCompany.ListCompanyPickDtl;
            objCompany = ServiceObjectCompany.GetLocIdDetails(objCompany);
            objStockChange.ListLocPickDtl = objCompany.ListLocPickDtl;
            objStockChange = objService.CheckLotStatus(objStockChange);
            if (objStockChange.ListCheckLotStatus.Count > 0)
            {
                objStockChange.status = objStockChange.ListCheckLotStatus[0].status;
            }
            if (objStockChange.status == "TEMP")
            {
                p_str_success = "0";
            }
            else
            {
                objStockChange = objService.GetItemMoveGridLoadItem(objStockChange);
                if (objStockChange.ListGetItemMoveDetails.Count > 0)
                {
                    objStockChange.ib_doc_id = objStockChange.ListGetItemMoveDetails[0].ib_doc_id;
                    objStockChange.lot_id = objStockChange.ListGetItemMoveDetails[0].lot_id;
                    objStockChange.date = Convert.ToDateTime(objStockChange.ListGetItemMoveDetails[0].rcvd_dt).ToString("MM/dd/yyyy");
                    objStockChange.whs_id = objStockChange.ListGetItemMoveDetails[0].whs_id;
                    objStockChange.frm_loc = objStockChange.ListGetItemMoveDetails[0].loc_id;
                    objStockChange.ib_doc_id = objStockChange.ListGetItemMoveDetails[0].ib_doc_id;
                    objStockChange.po_num = objStockChange.ListGetItemMoveDetails[0].po_num;
                    objStockChange.lot_num = p_str_paletid;
                }
            }
            Mapper.CreateMap<StockChange, StockChangeModel>();
            StockChangeModel = Mapper.Map<StockChange, StockChangeModel>(objStockChange);
            return StockChangeModel;
        }



        [HttpPost]
        [Route("SaveNew")]
        public bool SaveItemMove([FromBody] NewClass obj)
        {
          
            string SelPkgIds = string.Empty;
            string ItemCode = string.Empty;
            string l_str_save_status = string.Empty;
            StockChange objStockChange = new StockChange();
            StockChangeService objService = new StockChangeService();
            DataTable dt_item_stock_move = new DataTable();
            dt_item_stock_move = Utility.ConvertListToDataTable(obj.ListItemStockMove);
            l_str_save_status = objService.SaveStkMove(obj.companyId, obj.userId, dt_item_stock_move);
            var result = l_str_save_status == "OK" ? true : false;
            return result;
        }

        [HttpPost]
        [Route("SaveMove")]
        public bool SaveItemMove(string companyId, string userId, List<cls_temp_iv_stk_move> ListItemStockMove)
        {
         
            string SelPkgIds = string.Empty;
            string ItemCode = string.Empty;
            string l_str_save_status = string.Empty;
            StockChange objStockChange = new StockChange();
            StockChangeService objService = new StockChangeService();
            DataTable dt_item_stock_move = new DataTable();
            dt_item_stock_move = Utility.ConvertListToDataTable(ListItemStockMove);
            l_str_save_status = objService.SaveStkMove(companyId, userId, dt_item_stock_move);
            var result = l_str_save_status == "OK" ? true : false;
            return result;
        }

        [HttpGet]
        [Route("GetLocation")]
        public IEnumerable<ItemList> GetLocationList(string companyId)
        {
            StockChangeService ServiceObject = new StockChangeService();
            var result = ServiceObject.ItemXGetLocDetails(string.Empty, companyId).LstItmxlocdtl.
                Select(x => new ItemList { ItemId = x.loc_id, ItemName = x.loc_id }).Distinct();
            return result;
        }

        [HttpGet]
        [Route("GetStyle")]
        public IEnumerable<ItemList> GetStyleList(string companyId, string term)
        {
            StockChangeService ServiceObject = new StockChangeService();
            var result = ServiceObject.ItemXGetitmDetails(string.Empty, companyId).LstItmxCustdtl.
                Select(x => new ItemList { ItemId = x.itm_num, ItemName = x.Itmdtl }).Distinct();
            return result;
        }
        [HttpGet]
        [Route("GetLocationList")]
        public IEnumerable<ItemList> GetLocationList(string companyId, string term)
        {
            StockChangeService ServiceObject = new StockChangeService();
            var result = ServiceObject.ItemXGetitmDetails(string.Empty, companyId).LstItmxCustdtl.
                Select(x => new ItemList { ItemId = x.itm_num, ItemName = x.Itmdtl }).Distinct();
            return result;
        }








    }
}
