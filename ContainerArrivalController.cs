using AutoMapper;
using Gs3PLv9MOBAPI.Models;
using Gs3PLv9MOBAPI.Models.Entity;
using Gs3PLv9MOBAPI.Models.ViewModel;
using Gs3PLv9MOBAPI.Services;
using Gs3PLv9MOBAPI.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Gs3PLv9MOBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContainerArrivalController : ControllerBase
    {
        [HttpPost]
        [Route("Search")]
        public ContainerResponse DashboardConatinerArrivalGrid([FromBody] InBoundRequest obj)
        {
            var result = new ContainerResponse();
            ContainerArrival objContainerArrival = new ContainerArrival();
            IContainerArrivalService ServiceObject = new ContainerArrivalServiceService();
            objContainerArrival.cmp_id = obj.CompanyId;
            objContainerArrival.whs_id = obj.WhsId;
            objContainerArrival.cont_id = obj.ContainerId;
            objContainerArrival.mevent = obj.EventId;
            objContainerArrival = ServiceObject.GetContainerArrivalDetails(objContainerArrival);
            objContainerArrival.ListGetCarrierDetails = ServiceObject.GetCarrierDetails(objContainerArrival);
            result.CarrierList = objContainerArrival.ListGetCarrierDetails.Select(x => new ItemList { ItemId = x.carrier_id.Trim(), ItemName = x.carrier_name.Trim() }).ToList();
            //var Result = ServiceObject.GetCarrierEmailDetails(objContainerArrival.ListGetCarrierDetails[0].carrier_id, p_str_cmpid);
            objContainerArrival.ListEamilDetail = ServiceObject.GetCMBbandCarrierEMailDetails(obj.CompanyId);

            int initInt = 0;
            string strEmaillist = "";
            for (int i = 0; i < objContainerArrival.ListEamilDetail.Count; i++)
            {
                if (objContainerArrival.ListEamilDetail.Count > 0)
                {
                    if (initInt == 0)
                    {
                        strEmaillist = objContainerArrival.ListEamilDetail[i].email;
                        strEmaillist = strEmaillist.Replace("CLIENT: ", "");
                        strEmaillist = strEmaillist.Replace("CSR: ", "");
                        strEmaillist = strEmaillist.Replace("CARRIER: ", "");
                    }
                    else
                    {
                        strEmaillist = strEmaillist + ";" + objContainerArrival.ListEamilDetail[i].email;
                        strEmaillist = strEmaillist.Replace("CLIENT: ", "");
                        strEmaillist = strEmaillist.Replace("CSR: ", "");
                        strEmaillist = strEmaillist.Replace("CARRIER: ", "");
                    }
                    initInt = initInt + 1;
                }
            }
            if (strEmaillist != "")
            {
                strEmaillist = strEmaillist.TrimEnd(';');
                strEmaillist = strEmaillist.Replace(";", ",");
                // ViewBag.emailList = strEmaillist;
            }
            Mapper.CreateMap<ContainerArrival, ContainerArrivalModel>();
            ContainerArrivalModel objContainerArrivalModel = Mapper.Map<ContainerArrival, ContainerArrivalModel>(objContainerArrival);
            result.Container = objContainerArrivalModel.ListGetContainerArrivalDetails.Select(x => new ContainerItem
            {
                moveQty = x.move_qty,
                totQty = x.tot_qty,
                ibDocId = x.ib_doc_id,
                rcvdDt = x.rcvd_dt,
                contId = x.cont_id,
                lotId = x.lot_id,
                paletId = x.palet_id,
                locId = x.loc_id,
                carrierId = x.carrier_id.Trim(),
                poNum = x.po_num,
                pkgQty = x.pkg_qty,
                totCtns = x.tot_ctns,
                arrivalDt = x.arrival_dt.ToString(),
                ibDocDt = x.ib_doc_dt.ToString("MM/dd/yyyy"),
                Note = x.Note,
                SelectedType = x.type.ToString()
                //emailList
            }).ToList();

            if (objContainerArrivalModel.ListGetContainerArrivalDetails.Count > 0)
            {
                // Session["ib_doc_id"] = objContainerArrivalModel.ListGetContainerArrivalDetails[0].ib_doc_id;
                string strdate = objContainerArrivalModel.ListGetContainerArrivalDetails[0].ib_doc_dt.ToString();
                strdate = strdate.Substring(0, 10);
                // ViewBag.ib_doc_dt = strdate;
                //  ViewBag.SelectedType = objContainerArrivalModel.ListGetContainerArrivalDetails[0].type.ToString();
            }
            return result;
        }

        [HttpPost]
        [Route("Update")]
        public bool SaveContainerArrival([FromBody] ContainerModel obj)
        {
            bool flag = false;
            string SelPkgIds = string.Empty;
            string ItemCode = string.Empty;
            string l_str_save_status = string.Empty;
            ContainerArrival objContainerArrival = new ContainerArrival();
            IContainerArrivalService ServiceObject = new ContainerArrivalServiceService();
            var status = ServiceObject.SaveContainerArrival(obj.CompanyId, obj.ibDocId, obj.arrivalDt, obj.carrierId, obj.WhsId, obj.SelectedType, obj.contId, obj.EventId, obj.Note);

            if (status == "OK")
            {
                flag = true;
                var mBody = "Please note that container'" + obj.contId + "'Arrived now";
                if (obj.EventId == "Add")
                {
                    //SendMailCust_Carrier_CSR(obj.emailList, obj.contId, obj.CompanyId);
                }
            }
            return flag;

        }



    }
}
