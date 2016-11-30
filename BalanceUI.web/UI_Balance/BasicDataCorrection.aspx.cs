using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data;
using BalanceUI.Service;
namespace BalanceUI.web.UI_Balance
{
    public partial class BasicDataCorrection : WebStyleBaseForEnergy.webStyleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            base.InitComponts();
            if (!IsPostBack)
            {
#if DEBUG
                // 调试用,自定义的数据授权
                List<string> m_DataValidIdItems = new List<string>() { "zc_nxjc_byc_byf", "zc_nxjc_qtx_efc","zc_nxjc_znc"};
                AddDataValidIdGroup("ProductionOrganization", m_DataValidIdItems);
                mPageOpPermission = "1111";
#endif
                this.OrganisationTree.Organizations = GetDataValidIdGroup("ProductionOrganization");                 //向web用户控件传递数据授权参数
                this.OrganisationTree.PageName = "BasicDataCorrection.aspx";
                this.OrganisationTree.LeveDepth = 5;
            }
        }
        [WebMethod]
        public static string GetAbnormalData(string myOrganizationId, string myDeviationMagnification, string myCorrectionObject, string myMinValidValue, string myStartTime, string myEndTime, string myStartTimeReference, string myEndTimeReference)
        {
            DataTable m_ResultTable = BalanceUI.Service.BasicDataCorrectionService.GetAbnormalData(myOrganizationId, myDeviationMagnification, myCorrectionObject, myMinValidValue, myStartTime, myEndTime, myStartTimeReference, myEndTimeReference);
            string m_ColumnString = BalanceUI.Service.BasicDataCorrectionService.GetColumnJsonString(m_ResultTable);
            string m_ResultString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_ResultTable);
            m_ResultString = m_ResultString.Remove(m_ResultString.Length - 1, 1);
            m_ResultString = m_ResultString + "," + m_ColumnString + "}";
            return m_ResultString;
        }
        [WebMethod]
        public static string GetAmmeterTags(string myOrganizationId)
        {
            string m_DataBaseName = BalanceUI.Service.BasicDataCorrectionService.GetFactoryDataBase(myOrganizationId);
            DataTable m_AmmeterTagsTable = BalanceUI.Service.BasicDataCorrectionService.GetAmmeterInfoTable(m_DataBaseName);
            string m_ResultString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_AmmeterTagsTable);
            return m_ResultString;
        }
        [WebMethod]
        public static string GetDCSTags(string myOrganizationId)
        {
            string m_DataBaseName = BalanceUI.Service.BasicDataCorrectionService.GetFactoryDataBase(myOrganizationId);
            DataTable m_DCSTagsTable = BalanceUI.Service.BasicDataCorrectionService.GetDCSInfoTable(m_DataBaseName);
            string m_ResultString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_DCSTagsTable);
            return m_ResultString;
        }
        [WebMethod]
        public static string GetTrend(string myOrganizationId, string myStartTime, string myEndTime, string myAmmeterFieldNames, string myDCSFieldNames)
        {
            DataTable m_ResultTable = BalanceUI.Service.BasicDataCorrectionService.GetTrend(myOrganizationId, myStartTime, myEndTime, myAmmeterFieldNames, myDCSFieldNames);
            string m_ResultString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_ResultTable); 
            return m_ResultString;
        }
        [WebMethod]
        public static string CorrectionDataByTags(string myOrganizationId, string myStartTime, string myEndTime, string myAbnormalDataTime, string myTagsInfo)
        {
            string m_ResultString = BalanceUI.Service.BasicDataCorrectionService.CorrectionDataByTags(myOrganizationId, myStartTime, myEndTime, myAbnormalDataTime, myTagsInfo);
            return m_ResultString;
        }
        [WebMethod]
        public static string ReGenerateDataByDay(string myOrganizationId, string myStartTime, string myEndTime)
        {
            if (myStartTime != "" && myEndTime != "")
            {
                string m_ResultString = BalanceUI.Service.BasicDataCorrectionService.ReGenerateDataByDay(myOrganizationId, myStartTime, myEndTime);
                return m_ResultString;
            }
            else
            {
                return "0";
            }

        }
    }
}