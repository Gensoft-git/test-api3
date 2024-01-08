using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gs3PLv9MOBAPI.Models;
using Gs3PLv9MOBAPI.Models.Entity;
using Gs3PLv9MOBAPI.Models.ViewModel;
using Gs3PLv9MOBAPI.Services;
using Gs3PLv9MOBAPI.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gs3PLv9MOBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InBoundScanInController : Controller
    {
        [HttpPost]
        [Route("Search")]
        public InBoundResponse DashboardInBoundSerialGrid([FromBody] InBoundRequest obj)
        {
            string p_str_cmpid = obj.CompanyId;
            string p_str_whsId = obj.WhsId;
            string p_str_contId = obj.ContainerId;
            string p_str_event = obj.EventId;
            string p_str_docid = obj.DocId;

            InBoundScanHeader objIBScanIn = new InBoundScanHeader();
            IContainerArrivalService ServiceObject = new ContainerArrivalServiceService();
            IIBScanInService ObjIBScanInService = new IBScanInService();
            objIBScanIn.cmp_id = p_str_cmpid;
            objIBScanIn.whs_id = p_str_whsId;
            objIBScanIn.cont_id = p_str_contId;
            objIBScanIn.mevent = p_str_event;
            objIBScanIn.ib_doc_id = p_str_docid;
            objIBScanIn.InboundInquiry = new InboundInquiry();
            objIBScanIn.InboundInquiry.cmp_id = p_str_cmpid;
            objIBScanIn.InboundInquiry.cont_id = p_str_contId;
            objIBScanIn.InboundInquiry.ib_doc_id = p_str_docid;
            objIBScanIn.InboundInquiry = ObjIBScanInService.GetInboundHdrDtl(objIBScanIn.InboundInquiry);
            //objContainerArrival = ServiceObject.GetContainerArrivalDetails(objContainerArrival);
            //objContainerArrival.ListGetCarrierDetails = ServiceObject.GetCarrierDetails(objContainerArrival);
            objIBScanIn.ListEamilDetail = ServiceObject.GetCMBbandCarrierEMailDetails(p_str_cmpid);


            int initInt = 0;
            string strEmaillist = "";
            for (int i = 0; i < objIBScanIn.ListEamilDetail.Count; i++)
            {
                if (objIBScanIn.ListEamilDetail.Count > 0)
                {
                    if (initInt == 0)
                    {
                        strEmaillist = objIBScanIn.ListEamilDetail[i].email;
                        strEmaillist = strEmaillist.Replace("CLIENT: ", "");
                        strEmaillist = strEmaillist.Replace("CSR: ", "");
                        strEmaillist = strEmaillist.Replace("CARRIER: ", "");
                    }
                    else
                    {
                        strEmaillist = strEmaillist + ";" + objIBScanIn.ListEamilDetail[i].email;
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
                //ViewBag.emailList = strEmaillist;//Rakesh
            }
            Mapper.CreateMap<InBoundScanHeader, InBoundScanHeaderModel>();
            InBoundScanHeaderModel objContainerArrivalModel = Mapper.Map<InBoundScanHeader, InBoundScanHeaderModel>(objIBScanIn);
            if (objContainerArrivalModel != null)
            {
                if (objContainerArrivalModel.ListGetContainerArrivalDetails.Count > 0)
                {
                    // Session["ib_doc_id"] = objContainerArrivalModel.ListGetContainerArrivalDetails[0].ib_doc_id;;//Rakesh
                    string strdate = objContainerArrivalModel.ListGetContainerArrivalDetails[0].ib_doc_dt.ToString();
                    strdate = strdate.Substring(0, 10);
                    // ViewBag.ib_doc_dt = strdate;;//Rakesh
                    //  ViewBag.SelectedType = objContainerArrivalModel.ListGetContainerArrivalDetails[0].type.ToString();;//Rakesh
                }
            }

            var result = new InBoundResponse
            {
                ItemList = objContainerArrivalModel.InboundInquiry.ListAckRptDetails.Select(x => new InBoundItem
                {
                    CompanyId = x.cmp_id,
                    Style = x.Style,
                    Color = x.Color,
                    Size = x.Size,
                    IbDocId = x.ib_doc_id,
                    ItmCode = x.Itm_Code,
                    ItmName = x.itm_name,
                    Ctn = x.ctn,
                    Ppk = x.ppk,
                    TotalQty = x.TotalQty
                }).ToList()
            };

            return result;
        }

        [HttpPost]
        [Route("LoadScan")]
        public ScanModel LoadScanSerialDetails(InBoundItem obj)
        {
            InboundInquiry objInboundInquiry = new InboundInquiry();
            //InboundInquiryService ServiceObject = new InboundInquiryService();
            //IContainerArrivalService ServiceObject = new ContainerArrivalServiceService();
            IIBScanInService ServiceObject = new IBScanInService();

            objInboundInquiry.CompID = obj.CompanyId;
            objInboundInquiry.ib_doc_id = obj.IbDocId;
            objInboundInquiry.LineNum = 1;
            objInboundInquiry = ServiceObject.GetInboundHdrDtl(objInboundInquiry);
            objInboundInquiry.Container = (objInboundInquiry.ListAckRptDetails[0].Container == null || objInboundInquiry.ListAckRptDetails[0].Container == string.Empty ? string.Empty : objInboundInquiry.ListAckRptDetails[0].Container.Trim());
            objInboundInquiry.status = (objInboundInquiry.ListAckRptDetails[0].status == null || objInboundInquiry.ListAckRptDetails[0].status == string.Empty ? string.Empty : objInboundInquiry.ListAckRptDetails[0].status.Trim());
            objInboundInquiry.InboundRcvdDt = (objInboundInquiry.ListAckRptDetails[0].InboundRcvdDt == null || objInboundInquiry.ListAckRptDetails[0].InboundRcvdDt == string.Empty ? string.Empty : objInboundInquiry.ListAckRptDetails[0].InboundRcvdDt.Trim());
            //objInboundInquiry.vend_id = objInboundInquiry.ListAckRptDetails[0].vend_id.Trim();

            objInboundInquiry.vend_id = (objInboundInquiry.ListAckRptDetails[0].vend_id == null || objInboundInquiry.ListAckRptDetails[0].vend_id == string.Empty ? string.Empty : objInboundInquiry.ListAckRptDetails[0].vend_id.Trim());
            objInboundInquiry.vend_name = (objInboundInquiry.ListAckRptDetails[0].vend_name == null || objInboundInquiry.ListAckRptDetails[0].vend_name == string.Empty ? string.Empty : objInboundInquiry.ListAckRptDetails[0].vend_name.Trim());
            objInboundInquiry.FOB = (objInboundInquiry.ListAckRptDetails[0].FOB == null || objInboundInquiry.ListAckRptDetails[0].FOB == string.Empty ? string.Empty : objInboundInquiry.ListAckRptDetails[0].FOB.Trim());
            objInboundInquiry.refno = (objInboundInquiry.ListAckRptDetails[0].refno == null || objInboundInquiry.ListAckRptDetails[0].refno == string.Empty ? string.Empty : objInboundInquiry.ListAckRptDetails[0].refno.Trim());

            //objInboundInquiry.ibdocid = Id;
            //objInboundInquiry = ServiceObject.GetDocEntryId(objInboundInquiry);
            //objInboundInquiry.doc_entry_id = objInboundInquiry.doc_entry_id;
            //objInboundInquiry.cmp_id = cmp_id;
            //objInboundInquiry = ServiceObject.GetInboundDtl(objInboundInquiry);
            objInboundInquiry.ItemScanIN = new ItemScanIN();
            objInboundInquiry.ItemScanIN.cmp_id = obj.CompanyId;
            objInboundInquiry.ItemScanIN.ib_doc_id = obj.IbDocId;
            objInboundInquiry.ItemScanIN.itm_code = obj.ItmCode;
            objInboundInquiry.ItemScanIN.itm_num = obj.Style;
            objInboundInquiry.ItemScanIN.itm_color = obj.Color;
            objInboundInquiry.ItemScanIN.itm_size = obj.Size;
            objInboundInquiry.ItemScanIN.itm_name = obj.ItmName;
            objInboundInquiry.ItemScanIN.ppk = obj.Ppk.ToString();
            objInboundInquiry.ItemScanIN.ctn = obj.Ctn.ToString();
            objInboundInquiry.ItemScanIN.TotalQty = obj.TotalQty.ToString();
            objInboundInquiry.ItemScanIN.ib_doc_dt = null;
            objInboundInquiry.ItemScanIN.ob_doc_dt = null;

            objInboundInquiry.ListItemScanIN = ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, string.Empty);
            objInboundInquiry.ItemScanIN.balanceScan = Convert.ToInt32(objInboundInquiry.ItemScanIN.TotalQty) - objInboundInquiry.ListItemScanIN.Count();
          //  Mapper.CreateMap<InboundInquiry, InboundInquiryModel>();
            //InboundInquiryModel InboundInquiryModel = Mapper.Map<InboundInquiry, InboundInquiryModel>(objInboundInquiry);

            ScanModel result = new ScanModel()
            {
                TotalQty = Convert.ToInt32(objInboundInquiry.ItemScanIN.TotalQty),
                Remaining = objInboundInquiry.ItemScanIN.balanceScan,
                Scaned = objInboundInquiry.ListItemScanIN.Count(),
                ScanList = objInboundInquiry.ListItemScanIN.Select(x => new ScanItem
                {
                    ItemCode = x.itm_code,
                    SerialNo = x.itm_serial_num
                }).ToList()

            };







            return result;
        }

        [HttpPost]
        [Route("SaveScan")]
        public bool SaveScanInDetails([FromBody] InBoundSaveItem obj)
        {
            InboundInquiry objInboundInquiry = new InboundInquiry();
            IIBScanInService ServiceObject = new IBScanInService();
            var flag = false;
            objInboundInquiry.CompID = obj.CompanyId;
            objInboundInquiry.ib_doc_id = obj.IbDocId;
            objInboundInquiry.LineNum = 1;
            objInboundInquiry.ibdocid = obj.IbDocId;
            //objInboundInquiry = ServiceObject.GetDocEntryId(objInboundInquiry);
            //objInboundInquiry.doc_entry_id = objInboundInquiry.doc_entry_id;
            objInboundInquiry.cmp_id = obj.CompanyId;
            objInboundInquiry = ServiceObject.GetInboundHdrDtl(objInboundInquiry);
            if ((obj.SerialNo.IndexOf(';') > -1) || (obj.SerialNo.IndexOf(',') > -1))
            {
                string[] Lst_itm_serial = obj.SerialNo.IndexOf(';') > -1 ? obj.SerialNo.Split(';') : obj.SerialNo.Split(',');
                List<string> exceptionList = new List<string>();
                foreach (var item in Lst_itm_serial)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        objInboundInquiry.ItemScanIN = new ItemScanIN();
                        objInboundInquiry.ItemScanIN.cmp_id = obj.CompanyId; 
                        objInboundInquiry.ItemScanIN.ib_doc_id = obj.IbDocId;
                        objInboundInquiry.ItemScanIN.itm_code = obj.ItmCode;
                        objInboundInquiry.ItemScanIN.itm_num = obj.Style;
                        objInboundInquiry.ItemScanIN.itm_color = obj.Color;
                        objInboundInquiry.ItemScanIN.itm_size = obj.Size;
                        objInboundInquiry.ItemScanIN.itm_name = obj.ItmName;
                        objInboundInquiry.ItemScanIN.status = "Avail";
                        objInboundInquiry.ItemScanIN.ppk = obj.Ppk.ToString();
                        objInboundInquiry.ItemScanIN.ctn = obj.Ctn.ToString();
                        objInboundInquiry.ItemScanIN.TotalQty = obj.TotalQty.ToString(); 
                        objInboundInquiry.ItemScanIN.itm_serial_num = item;
                        objInboundInquiry.ItemScanIN.ib_doc_dt = null;
                        objInboundInquiry.ItemScanIN.ob_doc_dt = null;
                        objInboundInquiry.ItemScanIN.itm_serial_num_exist = obj.ExistSerialNo;

                        objInboundInquiry.ItemScanIN.itm_serial_num = item;
                        if (!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, item).Any() && obj.ExistSerialNo.ToString() == string.Empty)
                            ServiceObject.InsertScanInDetails(objInboundInquiry);
                        else if (obj.ExistSerialNo.ToString() != string.Empty)
                        {
                            if ((!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, item).Any()) || item == obj.ExistSerialNo)
                                ServiceObject.EditScanInDetails(objInboundInquiry);
                            else
                                exceptionList.Add(item);
                        }
                        else
                            exceptionList.Add(item);
                    }
                }

                //if (exceptionList.Any())
                //    return Json(exceptionList, JsonRequestBehavior.AllowGet);
                //return Json(true, JsonRequestBehavior.AllowGet);

                flag = true;
            }
            else
            {
                objInboundInquiry.ItemScanIN = new ItemScanIN();
                objInboundInquiry.ItemScanIN.cmp_id = obj.CompanyId;
                objInboundInquiry.ItemScanIN.ib_doc_id = obj.IbDocId;
                objInboundInquiry.ItemScanIN.itm_code = obj.ItmCode;
                objInboundInquiry.ItemScanIN.itm_num = obj.Style;
                objInboundInquiry.ItemScanIN.itm_color = obj.Color;
                objInboundInquiry.ItemScanIN.itm_size = obj.Size;
                objInboundInquiry.ItemScanIN.itm_name = obj.ItmName;
                objInboundInquiry.ItemScanIN.status = "Avail";
                objInboundInquiry.ItemScanIN.ppk = obj.Ppk.ToString();
                objInboundInquiry.ItemScanIN.ctn = obj.Ctn.ToString();
                objInboundInquiry.ItemScanIN.TotalQty = obj.TotalQty.ToString();
                objInboundInquiry.ItemScanIN.itm_serial_num = obj.SerialNo;
                objInboundInquiry.ItemScanIN.ib_doc_dt = null;
                objInboundInquiry.ItemScanIN.ob_doc_dt = null;
                objInboundInquiry.ItemScanIN.itm_serial_num_exist = obj.ExistSerialNo;

                objInboundInquiry.ItemScanIN.itm_serial_num = obj.SerialNo;
                if (!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, obj.SerialNo).Any() && obj.ExistSerialNo.ToString() == string.Empty)
                    ServiceObject.InsertScanInDetails(objInboundInquiry);
                else if (obj.ExistSerialNo.ToString() != string.Empty)
                {
                    if ((!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, obj.SerialNo).Any()) || obj.SerialNo== obj.ExistSerialNo)
                        ServiceObject.EditScanInDetails(objInboundInquiry);
                    else
                        flag = false;
                }
                else
                    flag = false;
            }
            flag = true;
            return flag;

        }
        [HttpPost]
        [Route("Delete")]
        public bool DeleteScanInDetails([FromBody] InBoundSaveItem obj)
        {
            var flag = false;
            InboundInquiry objInboundInquiry = new InboundInquiry();
            IIBScanInService ServiceObject = new IBScanInService();
            objInboundInquiry = ServiceObject.GetInboundHdrDtl(objInboundInquiry);
            objInboundInquiry.ItemScanIN = new ItemScanIN();
            objInboundInquiry.ItemScanIN.cmp_id =obj.CompanyId;
            objInboundInquiry.ItemScanIN.itm_code =obj.ItmCode;
            objInboundInquiry.ItemScanIN.itm_serial_num =obj.SerialNo;
            objInboundInquiry.ItemScanIN.ib_doc_dt = null;
            objInboundInquiry.ItemScanIN.ob_doc_dt = null;
            ServiceObject.DeleteScanInDetails(objInboundInquiry);
            flag = true;
            return flag;
        }



    }
}
