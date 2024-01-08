using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Gs3PLv9MOBAPI.Models;
using Gs3PLv9MOBAPI.Models.Entity;

using Gs3PLv9MOBAPI.Models.ViewModel;
using Gs3PLv9MOBAPI.Services;
using Gs3PLv9MOBAPI.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
//using iText.Kernel.Pdf;
//using iText.Layout;
//using iText.Layout.Element;
//using iText.IO.Image;
//using iText.Kernel.Geom;
//using iText.Kernel.Utils;
//using iText.Kernel.Pdf;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;
//using Microsoft.CodeAnalysis;

namespace Gs3PLv9MOBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PickScanOutController : Controller
    {
        private Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly string DocPath;
        string ordernum = string.Empty;
        public PickScanOutController(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [Route("ItemPrintFromGrid")]
        public PickScanOutPrint ItemPrintFromGrid([FromBody] PickScanOutPrintReq objInput) 
        {
            PickScanOutPrint resp = new PickScanOutPrint();
            try
            {
                string p_str_cmp_id = objInput.companyId; string lstrSoNum = objInput.srnum;
                string mstrDocPath = _config.GetValue<string>("MobSettings:DocPath").ToString();
                string lstrFilePath = Path.Combine(p_str_cmp_id, "OB", lstrSoNum.Substring(0, 3), lstrSoNum);
                string lstrMergeFilePath = Path.Combine(p_str_cmp_id, "PNP-PRINT" ,"OB", lstrSoNum.Substring(0, 3));
                string strFetchPath = Path.Combine(mstrDocPath, lstrFilePath);
                string strMergeDestPath = Path.Combine(mstrDocPath, lstrMergeFilePath);

                string lstrPackingSlipFileName = String.Format("2-PACKING-SLIP-A6-LBL-{0}-{1}.pdf", p_str_cmp_id, lstrSoNum);
                string lstrShippingLblFileName = String.Format("3-SHIPPING-LABEL-A6-LBL-{0}-{1}.pdf", p_str_cmp_id, lstrSoNum);
                string lstrMergeFileName = String.Format("{0}-SR-{1}-PACK-SHIP.pdf", p_str_cmp_id, lstrSoNum);
                string str_packing_slip_full_path = Path.Combine((strFetchPath), lstrPackingSlipFileName);
                string str_shipping_lbl_full_path = Path.Combine((strFetchPath), lstrShippingLblFileName);
                string str_merge_file_path = Path.Combine((strMergeDestPath), lstrMergeFileName);
                if (!Directory.Exists(strMergeDestPath))
                {
                    Directory.CreateDirectory(strMergeDestPath);
                }
                if (System.IO.File.Exists(Path.Combine(strMergeDestPath, str_merge_file_path)))
                {
                    System.IO.File.Delete(str_merge_file_path);
                }
                if (!System.IO.File.Exists(str_packing_slip_full_path) && !System.IO.File.Exists(str_shipping_lbl_full_path))
                {
                    resp.returnvalue = string.Empty;
                }
                else if (!System.IO.File.Exists(str_packing_slip_full_path) && System.IO.File.Exists(str_shipping_lbl_full_path))
                {
                    //Shipping Lable available
                    string file = lstrShippingLblFileName;
                    string strMergeFileName = file.Replace(lstrShippingLblFileName, lstrMergeFileName).ToString();
                    if (!System.IO.File.Exists(str_merge_file_path))
                    {
                        System.IO.File.Copy(str_shipping_lbl_full_path, str_merge_file_path);
                    }
                }
                else if (System.IO.File.Exists(str_packing_slip_full_path) && !System.IO.File.Exists(str_shipping_lbl_full_path))
                {
                    //Packing Slip available
                    string file = lstrPackingSlipFileName;
                    string strMergeFileName = file.Replace(lstrPackingSlipFileName, lstrMergeFileName).ToString();
                    if (!System.IO.File.Exists(str_merge_file_path))
                    {
                        System.IO.File.Copy(str_packing_slip_full_path, str_merge_file_path);
                    }
                }
                else
                    MergePDF(str_packing_slip_full_path, str_shipping_lbl_full_path, str_merge_file_path);

                if (System.IO.File.Exists(Path.Combine(strMergeDestPath, str_merge_file_path)))
                {
                    resp.returnvalue = "success";
                    byte[] bytes = System.IO.File.ReadAllBytes(str_merge_file_path);
                    string file = Convert.ToBase64String(bytes);
                    resp.filename = file;
                }
                else
                {
                    resp.returnvalue = string.Empty;
                }
                
                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void MergePDF(string File1, string File2, string outFileName)
        {
            string[] fileArray;
            fileArray = new string[3];
            fileArray[0] = File1;
            fileArray[1] = File2;
            
            PdfReader reader = null;
            Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;
            string outputPdfPath = outFileName;

            sourceDocument = new Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();

            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {

                int pages = TotalPageCount(fileArray[f]);

                reader = new PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }


                reader.Close();
            }
            //save the output file  
            sourceDocument.Close();
        }
        private static int TotalPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }

        [HttpPost]
        [Route("PickSearch")]
        public PickScanOutResponse OBPickScanItemGrid([FromBody] PickScanOutRequest obj)
        {
            string p_str_cmpid = obj.CompanyId;
            string p_str_whsId = obj.WhsId;
            string p_str_docid = obj.ObDocId;

            //--string p_str_contId = obj.ContainerId;
            // string p_str_event = obj.EventId;

            PickScanHeader objOBScanOut = new PickScanHeader();
            IPickScanOutService ObjObScanOutService = new PickScanOutService();

            objOBScanOut.cmp_id = p_str_cmpid;
            objOBScanOut.whs_id = p_str_whsId;

            objOBScanOut.doc_id = p_str_docid;
            objOBScanOut.PickScanOut = new PickScanHeader();
            objOBScanOut.PickScanOut.cmp_id = p_str_cmpid;
            //objIBScanIn.PickScanOut.cont_id = p_str_contId;
            // objIBScanIn.cont_id = p_str_contId;
            // objOBScanOut.mevent = p_str_event;
            objOBScanOut.PickScanOut.doc_id = p_str_docid;
            objOBScanOut.PickScanOut.aloc_doc_id = obj.alocDocId;

            OutboundInq objOutboundInq = new OutboundInq();

            objOBScanOut.PickScanOut = ObjObScanOutService.GetOutboundOrderDtl(objOBScanOut.PickScanOut);

            Mapper.CreateMap<PickScanHeader, PickScanHeaderModel>();
            PickScanHeaderModel objContainerArrivalModel = Mapper.Map<PickScanHeader, PickScanHeaderModel>(objOBScanOut);
            if (objContainerArrivalModel != null)
            {
                if (objContainerArrivalModel.ListGetContainerArrivalDetails.Count > 0)
                {
                    string strdate = objContainerArrivalModel.ListGetContainerArrivalDetails[0].ib_doc_dt.ToString();
                    strdate = strdate.Substring(0, 10);

                }
            }
            var result = new PickScanOutResponse
            {
                ItemList = objContainerArrivalModel.PickScanOut.ListAckRptDetails.Select(x => new PickScanItem
                {
                    CompanyId = x.CompID,
                    ObDocId = x.ShipReqID,
                    soline = x.soline,
                    ItmCode = x.Itm_Code,
                    Style = x.Style,
                    Color = x.Color,
                    Size = x.Size,
                    ItmName = x.itm_name,
                    Po_num = x.po_num,
                    Location = x.loc_id,
                    //Ctn = x.ctn,
                    //Ppk = x.ppk,
                    TotalQty = x.OrdQty,
                    aloc_doc_id = x.aloc_doc_id,
                    aloc_scan_sts = "",
                    scan_action_sts = x.scan_action_sts,
                    pickreason = x.pickreason,
                    trackingId = x.trackingId,
                    ListPickedScanItem = x.ListPickedScanItem
                }).ToList()
            };
            return result;
        }


        [HttpPost]
        [Route("LoadPickScan")]
        public PickScamModel LoadPickScanDetails(PickScanItem obj)
        {
            // InboundInquiry objInboundInquiry = new InboundInquiry();
            //IIBScanInService ServiceObject = new IBScanInService();

            PickScanHeader objPickScanHeader = new PickScanHeader();
            IPickScanOutService ServiceObject = new PickScanOutService();

            objPickScanHeader.CompID = obj.CompanyId;
            objPickScanHeader.ob_doc_id = obj.ObDocId;
            objPickScanHeader.LineNum = obj.soline;

            ////objPickScanHeader = ServiceObject.GetOutboundOrderDtl(objPickScanHeader);

            ////if (objPickScanHeader.ListAckRptDetails.Any())
            ////{
            ////    //objPickScanHeader.Container = (objPickScanHeader.ListAckRptDetails[0].Container == null || objPickScanHeader.ListAckRptDetails[0].Container == string.Empty ? string.Empty : objPickScanHeader.ListAckRptDetails[0].Container.Trim());
            ////    //objPickScanHeader.status = (objPickScanHeader.ListAckRptDetails[0].status == null || objPickScanHeader.ListAckRptDetails[0].status == string.Empty ? string.Empty : objPickScanHeader.ListAckRptDetails[0].status.Trim());
            ////    //objPickScanHeader.InboundRcvdDt = (objPickScanHeader.ListAckRptDetails[0].InboundRcvdDt == null || objPickScanHeader.ListAckRptDetails[0].InboundRcvdDt == string.Empty ? string.Empty : objPickScanHeader.ListAckRptDetails[0].InboundRcvdDt.Trim());
            ////    ////objOutboundInquiry.vend_id = objOutboundInquiry.ListAckRptDetails[0].vend_id.Trim();

            ////    //objPickScanHeader.vend_id = (objPickScanHeader.ListAckRptDetails[0].vend_id == null || objPickScanHeader.ListAckRptDetails[0].vend_id == string.Empty ? string.Empty : objPickScanHeader.ListAckRptDetails[0].vend_id.Trim());
            ////    //objPickScanHeader.vend_name = (objPickScanHeader.ListAckRptDetails[0].vend_name == null || objPickScanHeader.ListAckRptDetails[0].vend_name == string.Empty ? string.Empty : objPickScanHeader.ListAckRptDetails[0].vend_name.Trim());
            ////    //objPickScanHeader.FOB = (objPickScanHeader.ListAckRptDetails[0].FOB == null || objPickScanHeader.ListAckRptDetails[0].FOB == string.Empty ? string.Empty : objPickScanHeader.ListAckRptDetails[0].FOB.Trim());
            ////    //objPickScanHeader.refno = (objPickScanHeader.ListAckRptDetails[0].refno == null || objPickScanHeader.ListAckRptDetails[0].refno == string.Empty ? string.Empty : objPickScanHeader.ListAckRptDetails[0].refno.Trim());
            ////}



            objPickScanHeader.ItemScanOUT = new ItemScanOUT();
            objPickScanHeader.ItemScanOUT.cmp_id = obj.CompanyId;
            objPickScanHeader.ItemScanOUT.ob_doc_id = obj.ObDocId;
            objPickScanHeader.ItemScanOUT.itm_code = obj.ItmCode;
            objPickScanHeader.ItemScanOUT.itm_num = obj.Style;
            objPickScanHeader.ItemScanOUT.itm_color = obj.Color;
            objPickScanHeader.ItemScanOUT.itm_size = obj.Size;
            objPickScanHeader.ItemScanOUT.itm_name = obj.ItmName;
            objPickScanHeader.ItemScanOUT.ppk = obj.Ppk.ToString();
            objPickScanHeader.ItemScanOUT.ctn = obj.Ctn.ToString();
            objPickScanHeader.ItemScanOUT.TotalQty = obj.TotalQty.ToString();
            objPickScanHeader.ItemScanOUT.ib_doc_dt = null;
            objPickScanHeader.ItemScanOUT.ob_doc_dt = null;
            //string p_str_sonum, string p_str_itmline, string itm_code
            objPickScanHeader.ListPickedScanItem = ServiceObject.getScanOutDetailsByItemCode(obj.CompanyId, obj.ObDocId, obj.soline, obj.ItmCode, null, null, null, obj.aloc_doc_id);

            Mapper.CreateMap<PickScanHeader, PickScanHeaderModel>();
            PickScanHeaderModel objContainerArrivalModel = Mapper.Map<PickScanHeader, PickScanHeaderModel>(objPickScanHeader);
            if (objContainerArrivalModel != null)
            {
                if (objContainerArrivalModel.ListGetContainerArrivalDetails.Count > 0)
                {
                    string strdate = objContainerArrivalModel.ListGetContainerArrivalDetails[0].ib_doc_dt.ToString();
                    strdate = strdate.Substring(0, 10);

                }
            }


            //if (objPickScanHeader.ListItemScanOUT.Count > 0)  {
            //OBScanModel result = new OBScanModel()
            //{
            //    //TotalQty = Convert.ToInt32(objPickScanHeader.ItemScanIN.TotalQty),
            //    //Remaining = objPickScanHeader.ItemScanIN.balanceScan,
            //    //Scaned = objPickScanHeader.ListItemScanIN.Count(),


            //    TotalQty = 0,
            //    Remaining = 0,
            //    Scaned = 0,

            //    //OBScanList = objPickScanHeader.ListItemScanOUT.Select(x => new OBScanItem
            //    //{
            //    //    ItemCode = x.itm_code,
            //    //    SerialNo = x.itm_serial_num

            //    //}).ToList()
            //};

            PickScamModel result = new PickScamModel()
            {
                TotalQty = Convert.ToInt32(objPickScanHeader.ListPickedScanItem.Sum(x => x.Aloc)),
                PickScaned = objPickScanHeader.ListPickedScanItem.Count(),
                PickedScanItem = objPickScanHeader.ListPickedScanItem.Select(x => new PickedScanItem
                {
                    AlcLn = x.AlcLn,
                    CtnLn = x.CtnLn,
                    ItmLn = x.ItmLn,
                    so_num = x.so_num,
                    itm_code = x.itm_code,
                    itm_num = x.itm_num,
                    itm_color = x.itm_color,
                    itm_size = x.itm_size,
                    whs_id = x.whs_id,
                    loc_id = x.loc_id,
                    rcvd_dt = x.rcvd_dt,
                    due_qty = x.due_qty,
                    pkg_qty = x.pkg_qty,
                    avail_qty = x.avail_qty,
                    Aloc = x.Aloc,
                    Bal = x.Bal,
                    lot_id = x.lot_id,
                    Palet_id = x.Palet_id,
                    soline = x.soline,
                    due_line = x.due_line,
                    po_num = x.po_num,
                    CTNNumber = x.CTNNumber,
                    LotNumber = x.LotNumber
                }).ToList()
            };
            return result;
        }


        [HttpPost]
        [Route("PickSaveScan")]
        public PickScamModel SaveScanInDetails([FromBody] OutBoundScanSaveItem obj)
        {
            OutboundInq objOutboundInquiry = new OutboundInq();
            //IIBScanInService ServiceObject = new IBScanInService();
            IOutboundInqService ServiceObject = new OutboundInqService();

            OutboundInq getResults = new OutboundInq();
            var flag = false;
            objOutboundInquiry.CompID = obj.CompanyId;
            objOutboundInquiry.ob_doc_id = obj.ObDocId;
            objOutboundInquiry.LineNum = 1;
            objOutboundInquiry.aloc_doc_id = obj.AlocDocId;
            // objOutboundInquiry.ibdocid = obj.IbDocId;
            //objOutboundInquiry = ServiceObject.GetDocEntryId(objOutboundInquiry);
            //objOutboundInquiry.doc_entry_id = objOutboundInquiry.doc_entry_id;
            objOutboundInquiry.cmp_id = obj.CompanyId;
            //objOutboundInquiry = ServiceObject.GetInboundHdrDtl(objOutboundInquiry);temp_alloc_detail

            string first = "one";
            string second = "second";

            // Need to validate the obj Object lotNumber and PackageID (PKGid) against tbl_so_dtl
            // 

            if (first != first)
            {
                PickScamModel result;
                result = new PickScamModel()
                {
                    isError = true,
                    error_message = "Stock not available",
                    TotalQty = Convert.ToInt32(getResults?.LstAlocDtl?.Sum(x => x.Aloc)),
                    PickScaned = getResults?.LstAlocDtl?.Count() ?? 0,
                    PickedScanItem = getResults?.LstAlocDtl?.Select(x => new PickedScanItem
                    {
                        AlcLn = x.AlcLn,
                        CtnLn = x.CtnLn,
                        ItmLn = x.ItmLn,
                        so_num = x.so_num,
                        itm_code = x.itm_code,
                        itm_num = x.itm_num,
                        itm_color = x.itm_color,
                        itm_size = x.itm_size,
                        whs_id = x.whs_id,
                        loc_id = x.loc_id,
                        rcvd_dt = x.rcvd_dt,
                        due_qty = x.due_qty,
                        pkg_qty = x.pkg_qty,
                        avail_qty = x.avail_qty,
                        Aloc = x.Aloc,
                        Bal = x.Bal,
                        lot_id = x.lot_id,
                        Palet_id = x.Palet_id,
                        soline = x.soline,
                        due_line = x.due_line,
                        po_num = x.po_num,
                        CTNNumber = x.CTNNumber,
                        LotNumber = x.LotNumber
                    }).ToList()
                };

                return result;
            }
            else if (obj.CompanyId == "A2ZUSA")
            {
                OutboundInq objOutboundInq = new OutboundInq();
                objOutboundInq.aloc_line = obj.soline;
                objOutboundInq.ctn_line = 1;
                objOutboundInq.line_num = 1;
                objOutboundInq.due_line = 1;
                objOutboundInq.itm_num = obj.Style;
                objOutboundInq.itm_color = obj.Color;
                objOutboundInq.itm_size = obj.Size;
                objOutboundInq.itm_code = obj.ItmCode;
                objOutboundInq.soline = obj.soline;
                objOutboundInq.whs_id = "SNS";
                objOutboundInq.loc_id = "FLOOR";
                objOutboundInq.so_num = obj.ObDocId;
                //objOutboundInq.pkg_qty = Convert.ToInt32(obj.TotalQty);
                objOutboundInq.Palet_id = "1";
                objOutboundInq.po_num = obj.po_num;
                objOutboundInq.lot_id = "A2Z-";
                objOutboundInq.LotNumber = "A2Z-";
                objOutboundInq.CTNNumber = obj.Ctn.ToString();
                objOutboundInq.rcvd_dt = Convert.ToDateTime(DateTime.Now);
                //objOutboundInq.back_ordr_qty = 0;
                //objOutboundInq.aloc_qty = Convert.ToInt32(obj.TotalQty);
                objOutboundInq.avail_qty = 0;
                objOutboundInq.cmp_id = obj.CompanyId;

                objOutboundInq.CompID = obj.CompanyId;
                objOutboundInq.AlocdocId = obj.AlocDocId;
                objOutboundInq.aloc_doc_id = obj.AlocDocId;
                objOutboundInq.aloc_scan_sts = obj.aloc_scan_sts;
                objOutboundInq.pickreason = obj.pickreason;


                ServiceObject.InsertTempAlocdtl(objOutboundInq);
                getResults = ServiceObject.OutboundGETTEMPALOCDTL(objOutboundInq);
                flag = true;
                PickScamModel result;
                //PdfDocument doc = new PdfDocument();
                ////Load a PDF file
                //string mstrDocPath = _config.GetValue<string>("MobSettings:PdfPath").ToString();
                //doc.LoadFromFile(mstrDocPath);
                ////Specify printer name
                ////doc.PrintSettings.PrinterName = "HP LaserJet P1007";
                ////Silent printing
                //doc.PrintSettings.PrintController = new StandardPrintController();
                ////Print document
                //doc.Print();
                return result = new PickScamModel()
                {
                    isError = false,
                    error_message = string.Empty,
                    TotalQty = Convert.ToInt32(getResults.LstAlocDtl.Sum(x => x.Aloc)),
                    PickScaned = getResults.LstAlocDtl.Count(),
                    PickedScanItem = getResults.LstAlocDtl.Select(x => new PickedScanItem
                    {
                        AlcLn = x.AlcLn,
                        CtnLn = x.CtnLn,
                        ItmLn = x.ItmLn,
                        so_num = x.so_num,
                        itm_code = x.itm_code,
                        itm_num = x.itm_num,
                        itm_color = x.itm_color,
                        itm_size = x.itm_size,
                        whs_id = x.whs_id,
                        loc_id = x.loc_id,
                        rcvd_dt = x.rcvd_dt,
                        due_qty = x.due_qty,
                        pkg_qty = x.pkg_qty,
                        avail_qty = x.avail_qty,
                        Aloc = x.Aloc,
                        Bal = x.Bal,
                        lot_id = x.lot_id,
                        Palet_id = x.Palet_id,
                        soline = x.soline,
                        due_line = x.due_line,
                        po_num = x.po_num,
                        CTNNumber = x.CTNNumber,
                        LotNumber = x.LotNumber
                    }).ToList()
                };
            }
            else
            {
                // PICK PkgID - Seperated with '|' chracter ...
                if (obj.inputScanModelObject.IndexOf('|') > -1)
                {
                    string[] Lst_itm = obj.inputScanModelObject.Split('|');
                    List<string> exceptionList = new List<string>();
                    // separate the scan data  as  in SEQ -  comp-ID|Style|Color|Size|Po-num|LotNum|Pkg-ID|Pkg-Qty ]
                    // AlcLn	CtnLn	ItmLn	so_num	itm_code	itm_num	itm_color	itm_size	whs_id	loc_id	rcvd_dt	due_qty	pkg_qty	avail_qty	Aloc	Bal	lot_id	Palet_id	soline	due_line	po_num	CTNNumber	LotNumber

                    string strscan_cmpid = Lst_itm[0];
                    string strscan_Style = Lst_itm[1];
                    string strscan_color = Lst_itm[2];
                    string strscan_size = Lst_itm[3];
                    string strscan_ponum = Lst_itm[4];
                    string strscan_lotnum = Lst_itm[5];
                    string strscan_pkgid = Lst_itm[6];
                    string strscan_pkgqty = Lst_itm[7];

                    OutboundInq objOutboundInq = new OutboundInq();
                    objOutboundInq.aloc_line = obj.soline;
                    objOutboundInq.ctn_line = 1;
                    objOutboundInq.line_num = 1;
                    objOutboundInq.due_line = 1;
                    objOutboundInq.itm_num = strscan_Style;
                    objOutboundInq.itm_color = strscan_color;
                    objOutboundInq.itm_size = strscan_size;
                    objOutboundInq.itm_code = obj.ItmCode;
                    objOutboundInq.soline = obj.soline;
                    objOutboundInq.whs_id = "SNS";
                    objOutboundInq.loc_id = "FLOOR";
                    objOutboundInq.so_num = obj.ObDocId;
                    objOutboundInq.pkg_qty = Convert.ToInt32(strscan_pkgqty);
                    objOutboundInq.Palet_id = "1";
                    objOutboundInq.po_num = strscan_ponum;
                    objOutboundInq.lot_id = strscan_lotnum;
                    objOutboundInq.LotNumber = strscan_lotnum;
                    objOutboundInq.CTNNumber = strscan_pkgid;
                    objOutboundInq.rcvd_dt = Convert.ToDateTime(DateTime.Now);
                    objOutboundInq.back_ordr_qty = 0;
                    objOutboundInq.aloc_qty = Convert.ToInt32(strscan_pkgqty);
                    objOutboundInq.avail_qty = 0;
                    objOutboundInq.cmp_id = obj.CompanyId;

                    objOutboundInq.CompID = obj.CompanyId;
                    objOutboundInq.AlocdocId = obj.AlocDocId;
                    objOutboundInq.aloc_doc_id = obj.AlocDocId;



                    ServiceObject.InsertTempAlocdtl(objOutboundInq);
                    getResults = ServiceObject.OutboundGETTEMPALOCDTL(objOutboundInq);

                    //objOutboundInquiry.ItemScanIN = new ItemScanIN();
                    //objOutboundInquiry.ItemScanIN.cmp_id = obj.CompanyId;
                    //objOutboundInquiry.ItemScanIN.ib_doc_id = obj.IbDocId;
                    //objOutboundInquiry.ItemScanIN.itm_code = obj.ItmCode;
                    //objOutboundInquiry.ItemScanIN.itm_num = obj.Style;
                    //objOutboundInquiry.ItemScanIN.itm_color = obj.Color;
                    //objOutboundInquiry.ItemScanIN.itm_size = obj.Size;
                    //objOutboundInquiry.ItemScanIN.itm_name = obj.ItmName;
                    //objOutboundInquiry.ItemScanIN.status = "Avail";
                    //objOutboundInquiry.ItemScanIN.ppk = obj.Ppk.ToString();
                    //objOutboundInquiry.ItemScanIN.ctn = obj.Ctn.ToString();
                    //objOutboundInquiry.ItemScanIN.TotalQty = obj.TotalQty.ToString();
                    //objOutboundInquiry.ItemScanIN.itm_serial_num = item;
                    //objOutboundInquiry.ItemScanIN.ib_doc_dt = null;
                    //objOutboundInquiry.ItemScanIN.ob_doc_dt = null;
                    //objOutboundInquiry.ItemScanIN.itm_serial_num_exist = obj.ExistSerialNo;

                    //objOutboundInquiry.ItemScanIN.itm_serial_num = item;

                    //if (!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, item).Any() && obj.ExistSerialNo.ToString() == string.Empty)
                    //ServiceObject.InsertScanInDetails(objOutboundInquiry);
                    //  ServiceObject.InsertTempAllocSummary(objOutboundInquiry);

                    // Validate is the scan item valid to the SR Order & PO_NUM
                    //   validation process ...

                    //  if not valid return as Not valid
                    //  In-VALID PkgID reponse process ...

                    // If valid add record to temp-alloc-detail
                    // temp-alloc-detail - 
                    // and update related#s on temp-alloc-summary
                    // temp-alloc-summary - 



                    //    foreach (var item in Lst_itm_serial)
                    //{
                    //    if (!string.IsNullOrEmpty(item))
                    //    {
                    //        objInboundInquiry.ItemScanIN = new ItemScanIN();
                    //        objInboundInquiry.ItemScanIN.cmp_id = obj.CompanyId; 
                    //        objInboundInquiry.ItemScanIN.ib_doc_id = obj.IbDocId;
                    //        objInboundInquiry.ItemScanIN.itm_code = obj.ItmCode;
                    //        objInboundInquiry.ItemScanIN.itm_num = obj.Style;
                    //        objInboundInquiry.ItemScanIN.itm_color = obj.Color;
                    //        objInboundInquiry.ItemScanIN.itm_size = obj.Size;
                    //        objInboundInquiry.ItemScanIN.itm_name = obj.ItmName;
                    //        objInboundInquiry.ItemScanIN.status = "Avail";
                    //        objInboundInquiry.ItemScanIN.ppk = obj.Ppk.ToString();
                    //        objInboundInquiry.ItemScanIN.ctn = obj.Ctn.ToString();
                    //        objInboundInquiry.ItemScanIN.TotalQty = obj.TotalQty.ToString(); 
                    //        objInboundInquiry.ItemScanIN.itm_serial_num = item;
                    //        objInboundInquiry.ItemScanIN.ib_doc_dt = null;
                    //        objInboundInquiry.ItemScanIN.ob_doc_dt = null;
                    //        objInboundInquiry.ItemScanIN.itm_serial_num_exist = obj.ExistSerialNo;

                    //        objInboundInquiry.ItemScanIN.itm_serial_num = item;
                    //        if (!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, item).Any() && obj.ExistSerialNo.ToString() == string.Empty)
                    //            //ServiceObject.InsertScanInDetails(objInboundInquiry);
                    //            ServiceObject.InsertTempAllocSummary(objInboundInquiry);
                    //        else if (obj.ExistSerialNo.ToString() != string.Empty)
                    //        {
                    //            if ((!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, item).Any()) || item == obj.ExistSerialNo)
                    //                ServiceObject.EditScanInDetails(objInboundInquiry);
                    //            else
                    //                exceptionList.Add(item);
                    //        }
                    //        else
                    //            exceptionList.Add(item);
                    //    }
                    //}

                    //if (exceptionList.Any())
                    //    return Json(exceptionList, JsonRequestBehavior.AllowGet);
                    // return Json(true, JsonRequestBehavior.AllowGet);

                    flag = true;
                }
                else
                {
                    //objOutboundInquiry.ItemScanIN = new ItemScanIN();
                    //objOutboundInquiry.ItemScanIN.cmp_id = obj.CompanyId;
                    //objOutboundInquiry.ItemScanIN.ib_doc_id = obj.IbDocId;
                    //objOutboundInquiry.ItemScanIN.itm_code = obj.ItmCode;
                    //objOutboundInquiry.ItemScanIN.itm_num = obj.Style;
                    //objOutboundInquiry.ItemScanIN.itm_color = obj.Color;
                    //objOutboundInquiry.ItemScanIN.itm_size = obj.Size;
                    //objOutboundInquiry.ItemScanIN.itm_name = obj.ItmName;
                    //objOutboundInquiry.ItemScanIN.status = "Avail";
                    //objOutboundInquiry.ItemScanIN.ppk = obj.Ppk.ToString();
                    //objOutboundInquiry.ItemScanIN.ctn = obj.Ctn.ToString();
                    //objOutboundInquiry.ItemScanIN.TotalQty = obj.TotalQty.ToString();
                    //objOutboundInquiry.ItemScanIN.itm_serial_num = obj.SerialNo;
                    //objOutboundInquiry.ItemScanIN.ib_doc_dt = null;
                    //objOutboundInquiry.ItemScanIN.ob_doc_dt = null;
                    //objOutboundInquiry.ItemScanIN.itm_serial_num_exist = obj.ExistSerialNo;

                    //objOutboundInquiry.ItemScanIN.itm_serial_num = obj.SerialNo;
                    //if (!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, obj.SerialNo).Any() && obj.ExistSerialNo.ToString() == string.Empty)
                    //    ServiceObject.InsertScanInDetails(objOutboundInquiry);
                    //else if (obj.ExistSerialNo.ToString() != string.Empty)
                    //{
                    //    if ((!ServiceObject.getScanInDetailsByItemCode(obj.CompanyId, obj.ItmCode, obj.SerialNo).Any()) || obj.SerialNo== obj.ExistSerialNo)
                    //        ServiceObject.EditScanInDetails(objOutboundInquiry);
                    //    else
                    //        flag = false;
                    //}
                    //else
                    flag = false;
                }
                flag = true;
                PickScamModel result;
                return result = new PickScamModel()
                {
                    isError = false,
                    error_message = string.Empty,
                    TotalQty = Convert.ToInt32(getResults.LstAlocDtl.Sum(x => x.Aloc)),
                    PickScaned = getResults.LstAlocDtl.Count(),
                    PickedScanItem = getResults.LstAlocDtl.Select(x => new PickedScanItem
                    {
                        AlcLn = x.AlcLn,
                        CtnLn = x.CtnLn,
                        ItmLn = x.ItmLn,
                        so_num = x.so_num,
                        itm_code = x.itm_code,
                        itm_num = x.itm_num,
                        itm_color = x.itm_color,
                        itm_size = x.itm_size,
                        whs_id = x.whs_id,
                        loc_id = x.loc_id,
                        rcvd_dt = x.rcvd_dt,
                        due_qty = x.due_qty,
                        pkg_qty = x.pkg_qty,
                        avail_qty = x.avail_qty,
                        Aloc = x.Aloc,
                        Bal = x.Bal,
                        lot_id = x.lot_id,
                        Palet_id = x.Palet_id,
                        soline = x.soline,
                        due_line = x.due_line,
                        po_num = x.po_num,
                        CTNNumber = x.CTNNumber,
                        LotNumber = x.LotNumber
                    }).ToList()
                };
            }
            //  return result;
            //   return flag;

        }

        [HttpPost]
        [Route("PickPartialSaveScan")]
        public PickScamModel SaveScanPartialDetails([FromBody] List<OutBoundScanSaveItem> lstOutBoundScanSave)
        {         
            IOutboundInqService ServiceObject = new OutboundInqService();
            List<OutboundInq> lstOutboundInq = new List<OutboundInq>();
            
            lstOutBoundScanSave?.ForEach(obj =>
            {
                OutboundInq objOutboundInq = new OutboundInq();
                if (obj.CompanyId == "A2ZUSA")
                {                    
                    objOutboundInq.aloc_line = obj.soline;
                    objOutboundInq.ctn_line = 1;
                    objOutboundInq.line_num = 1;
                    objOutboundInq.due_line = 1;
                    objOutboundInq.itm_num = obj.Style;
                    objOutboundInq.itm_color = obj.Color;
                    objOutboundInq.itm_size = obj.Size;
                    objOutboundInq.itm_code = obj.ItmCode;
                    objOutboundInq.soline = obj.soline;
                    objOutboundInq.whs_id = "SNS";
                    objOutboundInq.loc_id = "FLOOR";
                    objOutboundInq.so_num = obj.ObDocId;
                    //objOutboundInq.pkg_qty = Convert.ToInt32(obj.TotalQty);
                    objOutboundInq.Palet_id = "1";
                    objOutboundInq.po_num = obj.po_num;
                    objOutboundInq.lot_id = "A2Z-";
                    objOutboundInq.LotNumber = "A2Z-";
                    objOutboundInq.CTNNumber = obj.Ctn.ToString();
                    objOutboundInq.rcvd_dt = Convert.ToDateTime(DateTime.Now);
                    //objOutboundInq.back_ordr_qty = 0;
                    //objOutboundInq.aloc_qty = Convert.ToInt32(obj.TotalQty);
                    objOutboundInq.avail_qty = 0;
                    objOutboundInq.cmp_id = obj.CompanyId;

                    objOutboundInq.CompID = obj.CompanyId;
                    objOutboundInq.AlocdocId = obj.aloc_doc_id;
                    objOutboundInq.aloc_doc_id = obj.aloc_doc_id;
                    objOutboundInq.aloc_scan_sts = obj.aloc_scan_sts;
                    objOutboundInq.pickreason = obj.pickreason;                    
                }
                lstOutboundInq.Add(objOutboundInq);
            });
            ServiceObject.InsertTempPickReason(lstOutboundInq);

            PickScamModel result;
            return result = new PickScamModel();
        }

        [HttpPost]
        [Route("SaveScanTrackingId")]
        public PickScamModel SaveScanTrackingId([FromBody] OutBoundScanSaveItem obj)
        {
            IOutboundInqService ServiceObject = new OutboundInqService();
            List<OutboundInq> lstOutboundInq = new List<OutboundInq>();
            OutboundInq objOutboundInq = new OutboundInq();
            if (obj.CompanyId == "A2ZUSA")
            {
                objOutboundInq.itm_code = obj.ItmCode;
                objOutboundInq.cmp_id = obj.CompanyId;
                objOutboundInq.aloc_doc_id = obj.AlocDocId;
                objOutboundInq.trackingId = obj.trackingId;
            }
            string returnval = ServiceObject.UpdateAlocTrackingDetail(objOutboundInq);

            PickScamModel result = new PickScamModel();
            if (string.Equals(returnval, "Tracking Id not matched")) {
                result.error_message = returnval;
                result.isError = true;
            }
            return result;
        }

        [HttpPost]
        [Route("PickDelete")]
        public bool DeleteScanInDetails([FromBody] InBoundSaveItem obj)
        {
            var flag = false;
            InboundInquiry objOutboundInquiry = new InboundInquiry();
            IIBScanInService ServiceObject = new IBScanInService();
            objOutboundInquiry = ServiceObject.GetInboundHdrDtl(objOutboundInquiry);
            objOutboundInquiry.ItemScanIN = new ItemScanIN();
            objOutboundInquiry.ItemScanIN.cmp_id = obj.CompanyId;
            objOutboundInquiry.ItemScanIN.itm_code = obj.ItmCode;
            objOutboundInquiry.ItemScanIN.itm_serial_num = obj.SerialNo;
            objOutboundInquiry.ItemScanIN.ib_doc_dt = null;
            objOutboundInquiry.ItemScanIN.ob_doc_dt = null;
            ServiceObject.DeleteScanInDetails(objOutboundInquiry);
            flag = true;
            return flag;
        }

        [HttpPost]
        [Route("PickComplete")]
        public bool SaveAlocEntry([FromBody] List<PickScanItem> obj)//string p_str_cmp_id, string p_str_Alocdocid, string p_str_Alocdt, string p_str_Alocshiprqfm, string p_str_Alocdeldtfm,
        {
            IPickScanOutService ServiceObject = new PickScanOutService();

            if (1 != 1)
            {
                return false;
            }
            else
            {
                ServiceObject.AlocPickComplete(obj);
                GenerateShipingConfirmationReport(obj);
                string response = ServiceObject.OBBulkAlocPostSave(obj);
                return true;
            }
        }


        private void GenerateShipingConfirmationReport(List<PickScanItem> obj)
        {
            string pstrCmpId = string.Empty;
            string SelectedSRList = string.Empty;
            foreach (var val in obj)
            {
                pstrCmpId = val.CompanyId;
                SelectedSRList = val.ObDocId.ToString();
                //if (string.IsNullOrEmpty(SelectedSRList))
                //    SelectedSRList = val.ObDocId.ToString();
                //else
                //    SelectedSRList = SelectedSRList + ", " + val.ObDocId.ToString();
            }
            //List<clsOB945ShipConfirmation> li = new List<clsOB945ShipConfirmation>();
            //OutboundShipInqService ServiceObject = new OutboundShipInqService();
            List<string> IdenticalColletion = new List<string>();
            //li = ServiceObject.fnGet940ShipConfirmationRpt(pstrCmpId, SelectedSRList);

          
            StringBuilder strFTPFileUploadAckContentBuilder = new StringBuilder();
            List<string> contentChecker = new List<string>();
            if (!string.IsNullOrEmpty(SelectedSRList))
            {
                strFTPFileUploadAckContentBuilder.Append("FILE_FORMAT,COMP_ID,ORDER_ID,ITEM, LOCATION, QUANTITY, SERIAL_NUMBERS, SHIPPING_CARRIER, SHIPPING_METHOD, PACKAGE_TRACKING_NUMBER");
                if (SelectedSRList.IndexOf(",") > -1)
                {
                    foreach (var item in SelectedSRList.Split(','))
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            StringBuilder returnvalue = ShipAckReportGenerationAndFTPFileUpload(pstrCmpId, item, "");
                            if (!contentChecker.Contains(returnvalue.ToString()))
                            {
                                contentChecker.Add(returnvalue.ToString());
                                strFTPFileUploadAckContentBuilder.Append(returnvalue);
                            }
                        }
                    }
                }
                else
                {
                    strFTPFileUploadAckContentBuilder.Append(ShipAckReportGenerationAndFTPFileUpload(pstrCmpId, SelectedSRList, ""));
                }
                FTPFileUploader(pstrCmpId, SelectedSRList, "", strFTPFileUploadAckContentBuilder);
            }
        }

        private StringBuilder ShipAckReportGenerationAndFTPFileUpload(string p_str_cmp_id, string lstrSoNum, string p_str_track_num)
        {
            IPickScanOutService ServiceObject = new PickScanOutService();
            IList<clsOB940ShipAck> ListOB940ShipAck = ServiceObject.fnGet940ShipAckDtl(p_str_cmp_id, lstrSoNum);
            IOutboundInqService ServiceTrackObject = new OutboundInqService();
            OutboundInq objOutboundInq = ServiceTrackObject.fnGetTrackDetailsBySoNumber(p_str_cmp_id, lstrSoNum);
            p_str_track_num = string.IsNullOrEmpty(objOutboundInq?.ListSOTracking.FirstOrDefault()?.track_num) ? string.Empty : objOutboundInq?.ListSOTracking.FirstOrDefault()?.track_num;
            if (ListOB940ShipAck.Count > 0)
            {                
                StringBuilder strAck = new StringBuilder();
                StringBuilder strFTPAck = new StringBuilder();
                string lstrFiledSeperator = ",";

                strAck.Append("FILE_FORMAT,COMP_ID,ORDER_ID,ITEM, LOCATION, QUANTITY, SERIAL_NUMBERS, SHIPPING_CARRIER, SHIPPING_METHOD, PACKAGE_TRACKING_NUMBER");
                ordernum = string.Empty;
                foreach (clsOB940ShipAck OB940ShipAck in ListOB940ShipAck)
                {
                    ordernum = OB940ShipAck.OrderNumber;
                    strAck.AppendLine();

                    strAck.Append(string.Concat(OB940ShipAck.FileFormat, lstrFiledSeperator, OB940ShipAck.CmpId, lstrFiledSeperator, OB940ShipAck.OrderNumber, lstrFiledSeperator,
                        OB940ShipAck.ItmNumber, lstrFiledSeperator, OB940ShipAck.WhsLocation, lstrFiledSeperator, OB940ShipAck.ShipQty,
                        lstrFiledSeperator, OB940ShipAck.SerialNumber, lstrFiledSeperator, OB940ShipAck.CarrierCode, lstrFiledSeperator, OB940ShipAck.CarrierMerthod, lstrFiledSeperator, p_str_track_num));

                    strFTPAck.AppendLine();
                    strFTPAck.Append(string.Concat(OB940ShipAck.FileFormat, lstrFiledSeperator, OB940ShipAck.CmpId, lstrFiledSeperator, OB940ShipAck.OrderNumber, lstrFiledSeperator,
                        OB940ShipAck.ItmNumber, lstrFiledSeperator, OB940ShipAck.WhsLocation, lstrFiledSeperator, OB940ShipAck.ShipQty,
                        lstrFiledSeperator, OB940ShipAck.SerialNumber, lstrFiledSeperator, OB940ShipAck.CarrierCode, lstrFiledSeperator, OB940ShipAck.CarrierMerthod, lstrFiledSeperator, p_str_track_num));
                }
                                
                //string mstrDocPath = ConfigurationManager.AppSettings["Docpath"].ToString().Trim();
                string mstrDocPath = _config.GetValue<string>("MobSettings:DocPath").ToString();
                string lstrFilePath = Path.Combine(p_str_cmp_id, "OB", lstrSoNum.Substring(0, 3), lstrSoNum);
                string strDestPath = Path.Combine(mstrDocPath, lstrFilePath);

                string lstrCsvFileName = String.Format("4-SHP-CNF-{0}-SR-{1}-SO-{2}.csv", p_str_cmp_id, lstrSoNum, ordernum);
                string l_str_folder_full_path = Path.Combine((mstrDocPath), lstrCsvFileName);
                if (!Directory.Exists(strDestPath))
                {
                    Directory.CreateDirectory(strDestPath);
                }
                if (System.IO.File.Exists(Path.Combine(strDestPath, lstrCsvFileName)))
                {
                    System.IO.File.Delete(Path.Combine(strDestPath, lstrCsvFileName));
                }

                string lstrFullFileName = Path.Combine(strDestPath, lstrCsvFileName);
                System.IO.File.WriteAllText(lstrFullFileName, strAck.ToString());

                DocumentUpload objDocumentUpload = new DocumentUpload();
                objDocumentUpload.cmp_id = p_str_cmp_id;
                objDocumentUpload.doc_type = "OB";
                objDocumentUpload.doc_id = lstrSoNum;
                objDocumentUpload.upload_by = "SYSTEM";
                objDocumentUpload.comments = "Shipping Acknowledgement";
                objDocumentUpload.upload_dt = DateTime.Now;
                objDocumentUpload.file_path = lstrFilePath;
                objDocumentUpload.orig_file_name = lstrCsvFileName;
                objDocumentUpload.upload_file_name = lstrCsvFileName;
                ServiceObject.SaveDocumentUpload(objDocumentUpload);

                return strFTPAck;
            }
            return null;
        }

        private void FTPFileUploader(string p_str_cmp_id, string lstrSoNum, string p_str_track_num, StringBuilder strAck)
        {
            string strGlobalCmpId = _config.GetValue<string>("MobSettings:DefaultCompID").ToString();
            if (strGlobalCmpId.Length > 0)
            {
                strGlobalCmpId = strGlobalCmpId.Substring(0, 3);
            }
            try
            {
                //string mstrDocPath = System.Configuration.ConfigurationManager.AppSettings["Docpath"].ToString().Trim();
                string mstrDocPath = _config.GetValue<string>("MobSettings:FTPDocpath").ToString();
                string lstrFilePath = "";// Path.Combine(p_str_cmp_id, "OB", lstrSoNum.Substring(0, 3), lstrSoNum);
                string strDestPath = Path.Combine(mstrDocPath, lstrFilePath);

                //string lstrCsvFileName = String.Format("4-SHP-CNF-FTP-{0}-{1}.csv", p_str_cmp_id, lstrSoNum);
                string lstrCsvFileName = String.Format("945-SHP-RDY-FTP-{0}-SR-{1}-SO-{2}.csv", p_str_cmp_id, lstrSoNum, ordernum);
                string l_str_folder_full_path = Path.Combine((mstrDocPath), lstrCsvFileName);
                if (!Directory.Exists(strDestPath))
                {
                    Directory.CreateDirectory(strDestPath);
                }
                if (System.IO.File.Exists(Path.Combine(strDestPath, lstrCsvFileName)))
                {
                    System.IO.File.Delete(Path.Combine(strDestPath, lstrCsvFileName));
                }

                string lstrFullFileName1 = Path.Combine(strDestPath, lstrCsvFileName);
                System.IO.File.WriteAllText(lstrFullFileName1, strAck.ToString());

                //string UploadFTPReqd = System.Configuration.ConfigurationManager.AppSettings["UploadFTPReqd"].ToString().Trim();
                string UploadFTPReqd = _config.GetValue<string>("MobSettings:UploadFTPReqd").ToString();
                if (UploadFTPReqd == "Y")
                {

                    //string pstrsftpConfigPath = System.Configuration.ConfigurationManager.AppSettings["NetSuiteOrderPathFileName"].ToString().Trim();
                    string pstrsftpConfigPath = _config.GetValue<string>("MobSettings:NetSuiteOrderPathFileName").ToString();
                    // string pstrsftpConfigPath = System.Configuration.ConfigurationManager.AppSettings[NetSuiteOrderPathFileName].ToString().Trim();

                    DataSet dsNetSuitePath = new DataSet();
                    dsNetSuitePath.ReadXml(pstrsftpConfigPath);
                    DataTable dtNetSuitePath = new DataTable();
                    dtNetSuitePath = dsNetSuitePath.Tables[0];

                    for (int i = 0; i < dtNetSuitePath.Rows.Count; i++)
                    {
                        if (p_str_cmp_id == dtNetSuitePath.Rows[i]["CompId"].ToString())
                        {
                            clsSftp objFTP = new clsSftp();
                            objFTP.CompId = dtNetSuitePath.Rows[i]["CompId"].ToString();
                            objFTP.FTPIP = dtNetSuitePath.Rows[i]["FTPIP"].ToString();
                            objFTP.UserId = dtNetSuitePath.Rows[i]["UserId"].ToString();
                            objFTP.UserPwd = dtNetSuitePath.Rows[i]["UserPwd"].ToString();
                            objFTP.FTPPort = dtNetSuitePath.Rows[i]["FTPPort"].ToString();
                            objFTP.IsSSH = dtNetSuitePath.Rows[i]["IsSSH"].ToString();
                            objFTP.OutBoundFTPAckFolder = dtNetSuitePath.Rows[i]["OutBoundFTPAckFolder"].ToString();
                            //string strGlobalCmpId = System.Configuration.ConfigurationManager.AppSettings["DefaultCompID"].ToString().Trim();

                            string lstrFullFileName = Path.Combine(strDestPath, lstrCsvFileName);
                            System.IO.File.WriteAllText(lstrFullFileName, strAck.ToString());

                            string lstrCsvftpFileName = String.Format("945-SHP-CNF-{0}-{1}-SR-{2}.csv", p_str_cmp_id, strGlobalCmpId, lstrSoNum);
                            //objFTP.FTPFileUpload(lstrFullFileName, objFTP.OutBoundFTPAckFolder, lstrCsvftpFileName);
                            objFTP.sFTPFileUpload(lstrFullFileName, objFTP.OutBoundFTPAckFolder, lstrCsvftpFileName);

                            break;
                        }
                    }
                }
                else
                {
                    //string mstrDocFTPPath = System.Configuration.ConfigurationManager.AppSettings["FTPDocpath"].ToString().Trim();
                    string mstrDocFTPPath = _config.GetValue<string>("MobSettings:FTPDocpath").ToString();
                    string lstrFTPFilePath = Path.Combine(p_str_cmp_id, "OB", lstrSoNum.Substring(0, 3), lstrSoNum);
                    string strFTPDestPath = Path.Combine(mstrDocFTPPath, lstrFTPFilePath);

                    //string lstrCsvFtpFileName = String.Format("4-SHP-CNF-FTP-{0}-{1}.csv", p_str_cmp_id, lstrSoNum);
                    string lstrCsvFtpFileName = String.Format("945-SHP-RDY-FTP-{0}-SR-{1}-SO-{2}.csv", p_str_cmp_id, lstrSoNum, ordernum);
                    string l_str_ftp_folder_full_path = Path.Combine((mstrDocFTPPath), lstrCsvFtpFileName);
                    if (!Directory.Exists(mstrDocFTPPath))
                    {
                        Directory.CreateDirectory(mstrDocFTPPath);
                    }
                    if (System.IO.File.Exists(Path.Combine(mstrDocFTPPath, lstrCsvFtpFileName)))
                    {
                        System.IO.File.Delete(Path.Combine(mstrDocFTPPath, lstrCsvFtpFileName));
                    }

                    string lstrFTPFullFileName = Path.Combine(mstrDocFTPPath, lstrCsvFtpFileName);
                    System.IO.File.WriteAllText(lstrFTPFullFileName, strAck.ToString());

                }

            }

            catch (Exception ex)
            {

            }


        }

        [HttpPost]
        [Route("PickPost")]
        public bool PostAlocEntry([FromBody] PickScanOutRequest obj)//string p_str_cmp_id, string p_str_Alocdocid
        {
            IPickScanOutService ServiceObjectAloc = new PickScanOutService();
            OutboundInq objOutboundInq = new OutboundInq();
            OutboundInqService ServiceObject = new OutboundInqService();
            string l_str_bol_num = string.Empty;
            objOutboundInq.cmp_id = obj.CompanyId;
            objOutboundInq.aloc_doc_id = obj.alocDocId;
            objOutboundInq.aloc_dt = DateTime.Now.ToString("MM/dd/yyyy"); //SSystem.get;
            obj.aloc_dt = DateTime.Now;
            objOutboundInq = ServiceObject.GetShipNum(objOutboundInq);
            objOutboundInq.bol_num = objOutboundInq.ShipDocNum;
            l_str_bol_num = objOutboundInq.ShipDocNum;
            //Session["l_str_bol_num"] = l_str_bol_num;
            objOutboundInq.bol_num = l_str_bol_num;
            objOutboundInq = ServiceObject.GetAlocType(objOutboundInq);
            objOutboundInq.aloc_type = (objOutboundInq.LstGetAlocType[0].aloc_type == null || objOutboundInq.LstGetAlocType[0].aloc_type.Trim() == "" ? string.Empty : objOutboundInq.LstGetAlocType[0].aloc_type.Trim());
            objOutboundInq.ship_to_name = (objOutboundInq.LstGetAlocType[0].ship_to_name == null || objOutboundInq.LstGetAlocType[0].ship_to_name.Trim() == "" ? string.Empty : objOutboundInq.LstGetAlocType[0].ship_to_name.Trim());
            objOutboundInq.cntr_num = (objOutboundInq.LstGetAlocType[0].cntr_num == null || objOutboundInq.LstGetAlocType[0].cntr_num.Trim() == "" ? string.Empty : objOutboundInq.LstGetAlocType[0].cntr_num.Trim());
            objOutboundInq.ship_to_id = (objOutboundInq.LstGetAlocType[0].ship_to_id == null || objOutboundInq.LstGetAlocType[0].ship_to_id.Trim() == "" ? string.Empty : objOutboundInq.LstGetAlocType[0].ship_to_id.Trim());

            if (1 != 1)
            {
                return false;
            }
            else
            {
                ServiceObjectAloc.AlocPickPost(obj.CompanyId, obj.alocDocId, obj.ObDocId, obj.aloc_dt, obj.ship_to_id);
                ServiceObject.UpdateStatusInAlocHdr(objOutboundInq);
                ServiceObject.UpdateStatusInAlocDtl(objOutboundInq);
                objOutboundInq = ServiceObject.GetCustId(objOutboundInq);

                objOutboundInq.cust_id = (objOutboundInq.ListCustId[0].cust_id == null || objOutboundInq.ListCustId[0].cust_id.Trim() == "" ? string.Empty : objOutboundInq.ListCustId[0].cust_id.Trim());
                objOutboundInq.ship_to_id = (objOutboundInq.ListCustId[0].ship_to_id == null || objOutboundInq.ListCustId[0].ship_to_id.Trim() == "" ? string.Empty : objOutboundInq.ListCustId[0].ship_to_id.Trim());
                objOutboundInq.whs_id = (objOutboundInq.ListCustId[0].whs_id == null || objOutboundInq.ListCustId[0].whs_id.Trim() == "" ? string.Empty : objOutboundInq.ListCustId[0].whs_id.Trim());
                objOutboundInq.note = "Ship Confirmation On:" + objOutboundInq.aloc_dt.Trim();
                objOutboundInq = ServiceObject.GetShipToAddress(objOutboundInq);
                if (objOutboundInq.ListShipToAddress.Count() == 0)
                {
                    objOutboundInq.mail_name = "";
                    objOutboundInq.addr_line1 = "";
                    objOutboundInq.addr_line2 = "";
                    objOutboundInq.city = "";
                    objOutboundInq.state_id = "";
                    objOutboundInq.post_code = "";
                    objOutboundInq.cntry_id = "";
                }
                else
                {
                    //(objOutboundInq.lstobjOutboundInq[0].so_dt == null || objOutboundInq.lstobjOutboundInq[0].so_dt == "" ? string.Empty : Convert.ToDateTime(objOutboundInq.lstobjOutboundInq[0].so_dt).ToString("MM/dd/yyyy"));
                    objOutboundInq.mail_name = (objOutboundInq.ListShipToAddress[0].mail_name == null || objOutboundInq.ListShipToAddress[0].mail_name.Trim() == "" ? string.Empty : objOutboundInq.ListShipToAddress[0].mail_name);
                    objOutboundInq.addr_line1 = (objOutboundInq.ListShipToAddress[0].addr_line1 == null || objOutboundInq.ListShipToAddress[0].addr_line1.Trim() == "" ? string.Empty : objOutboundInq.ListShipToAddress[0].addr_line1);
                    objOutboundInq.addr_line2 = (objOutboundInq.ListShipToAddress[0].addr_line2 == null || objOutboundInq.ListShipToAddress[0].addr_line2.Trim() == "" ? string.Empty : objOutboundInq.ListShipToAddress[0].addr_line2.Trim());
                    objOutboundInq.city = (objOutboundInq.ListShipToAddress[0].city == null || objOutboundInq.ListShipToAddress[0].city.Trim() == "" ? string.Empty : objOutboundInq.ListShipToAddress[0].city);
                    objOutboundInq.state_id = (objOutboundInq.ListShipToAddress[0].state_id == null || objOutboundInq.ListShipToAddress[0].state_id.Trim() == "" ? string.Empty : objOutboundInq.ListShipToAddress[0].state_id);
                    objOutboundInq.post_code = (objOutboundInq.ListShipToAddress[0].post_code == null || objOutboundInq.ListShipToAddress[0].post_code.Trim() == "" ? string.Empty : objOutboundInq.ListShipToAddress[0].post_code);
                    objOutboundInq.cntry_id = (objOutboundInq.ListShipToAddress[0].cntry_id == null || objOutboundInq.ListShipToAddress[0].cntry_id.Trim() == "" ? string.Empty : objOutboundInq.ListShipToAddress[0].cntry_id);
                }

                ServiceObject.InsertShipHdr(objOutboundInq);
                objOutboundInq = ServiceObject.GetUnPickQty(objOutboundInq);
                for (int i = 0; i < objOutboundInq.LstOutboundUnPickQty.Count(); i++)
                {
                    objOutboundInq.itm_qty = objOutboundInq.LstOutboundUnPickQty[i].itm_qty;
                    objOutboundInq.itm_code = objOutboundInq.LstOutboundUnPickQty[i].itm_code.Trim();
                    objOutboundInq.whs_id = (objOutboundInq.LstOutboundUnPickQty[i].whs_id.Trim() == null || objOutboundInq.LstOutboundUnPickQty[i].whs_id.Trim() == "") ? "" : objOutboundInq.LstOutboundUnPickQty[i].whs_id.Trim();
                    objOutboundInq.lot_id = (objOutboundInq.LstOutboundUnPickQty[i].lot_id.Trim() == null || objOutboundInq.LstOutboundUnPickQty[i].lot_id.Trim() == "") ? "" : objOutboundInq.LstOutboundUnPickQty[i].lot_id.Trim();
                    objOutboundInq.loc_id = (objOutboundInq.LstOutboundUnPickQty[i].loc_id.Trim() == null || objOutboundInq.LstOutboundUnPickQty[i].loc_id.Trim() == "") ? "" : objOutboundInq.LstOutboundUnPickQty[i].loc_id.Trim();
                    objOutboundInq.rcvd_dt = objOutboundInq.LstOutboundUnPickQty[0].rcvd_dt;
                    ServiceObject.UpdateTrnHdr(objOutboundInq);
                }

                ServiceObject.InsertShipDtl(objOutboundInq);

                /* -- Commented out due to SNS Changes and need to know what is impacted on the flow
                if (ServiceObject.fnGenerateIBfromOB(p_str_cmp_id, Session["l_str_so_num"].ToString(), p_str_alocdocid) == false)
                {

                }
                */

                //CR20180813-001 Added By Nithya
                objOutboundInq.cmp_id = obj.CompanyId;
                objOutboundInq.Sonum = obj.ObDocId;// Session["l_str_so_num"].ToString();
                objOutboundInq.aloc_doc_id = obj.alocDocId;
                objOutboundInq.shipdocid = "";
                objOutboundInq.mode = "ALOC-POST";
                objOutboundInq.maker = "";//Session["UserID"].ToString().Trim();
                objOutboundInq.makerdt = DateTime.Now.ToString("MM/dd/yyyy");
                objOutboundInq.Auditcomment = "Posted";
                objOutboundInq = ServiceObject.Add_To_proc_save_audit_trail(objOutboundInq);
                //END         
                Mapper.CreateMap<OutboundInq, OutboundInqModel>();
                OutboundInqModel objOutboundInqModel = Mapper.Map<OutboundInq, OutboundInqModel>(objOutboundInq);

                return true;
            }
        }

        [HttpGet]
        [Route("GetPickReason")]
        public List<PickReason> GetPickReason()
        {
            IPickScanOutService ObjObScanOutService = new PickScanOutService();
            var result = ObjObScanOutService.GetPickReason();
            
            return result;
        }

        [HttpGet]
        [Route("GetOpenOrderDetails")]
        public List<OpenAlocOrders> GetOpenOrderDetails()
        {
            IPickScanOutService ObjObScanOutService = new PickScanOutService();
            var result = ObjObScanOutService.GetOpenOrderDetails();

            return result;
        }

        [HttpPost]
        [Route("GetPendingOrderDetails")]
        public List<OpenAlocOrders> GetPendingOrderDetails([FromBody] PendingOrderRequest obj)
        {
            IPickScanOutService ObjObScanOutService = new PickScanOutService();
            var result = ObjObScanOutService.GetPendingOrderDetails(obj);

            return result;
        }

        [HttpGet]
        [Route("GetProcessedOrderDetails")]
        public List<OpenAlocOrders> GetProcessedOrderDetails()
        {
            IPickScanOutService ObjObScanOutService = new PickScanOutService();
            var result = ObjObScanOutService.GetProcessedOrderDetails();

            return result;
        }

        [HttpPost]
        [Route("SubmitPendingOrderReason")]
        public string SubmitPendingOrderReason([FromBody] PendingOrderRequest obj)
        {   
            IPickScanOutService ObjObScanOutService = new PickScanOutService();
            string _return = ObjObScanOutService.SubmitPendingOrderReason(obj);

            return _return;
        }

    }
}
