using BalanceUI.Service.BalanceService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using BalanceUI.Service;
namespace BalanceUI.web.UI_Balance
{
    public partial class BalanceWeightBalanceLog : WebStyleBaseForEnergy.webStyleBase
    {
        private static string user = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            base.InitComponts();
            if (!IsPostBack)
            {
#if DEBUG
                // 调试用,自定义的数据授权
                List<string> m_DataValidIdItems = new List<string>() { "zc_nxjc_byc_byf", "zc_nxjc_qtx_tys" };
                AddDataValidIdGroup("ProductionOrganization", m_DataValidIdItems);
                mPageOpPermission = "0000";
#endif
                this.OrganisationTree.Organizations = GetDataValidIdGroup("ProductionOrganization");                 //向web用户控件传递数据授权参数
                this.OrganisationTree.PageName = "BalanceWeightBalanceLog.aspx";
                this.OrganisationTree.LeveDepth = 5;
                user = mUserId;
            }
        }

        /// <summary>
        /// 获取最后一次平衡时间
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        [WebMethod]
        public static string GetLastBalanceLogTime(string organizationId)
        {
            return BalanceWeightBalanceLogService.GetLastBalanceLogTime(organizationId);
        }

        [WebMethod]
        public static string GetLogInfo(string organizationId, string locationId)
        {
            DataTable table = BalanceWeightBalanceLogService.GetLogInfo(organizationId,locationId);
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(table);
        }

        [WebMethod]
        public static string GetMaterualKind(string organizationId)
        {
            DataTable table = BalanceWeightBalanceLogService.GetMaterualKind(organizationId);
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(table);
        }

        [WebMethod]
        public static string GetBalanceData(string startTime, string endTime, string locationId, string organizationId)
        {
            DataTable table = BalanceWeightBalanceLogService.GetBalanceData(startTime, endTime, locationId, organizationId);
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(table);
        }
        [WebMethod]
        public static string SaveData(string organizationId, string datagridData, string diffValueData,string productionZoneId, string startTime, string endTime,
                                      string stocksValue, string inputValue, string outputValue, string logId,string type)
        {
            DataTable table = EasyUIJsonParser.TreeGridJsonParser.JsonToDataTable(datagridData);
            DataTable diffValueTable = EasyUIJsonParser.TreeGridJsonParser.JsonToDataTable(diffValueData);
            return BalanceWeightBalanceLogService.SaveData(organizationId, table, diffValueTable,productionZoneId, startTime, endTime, stocksValue, inputValue, outputValue, logId, type, user);
        }
        /// <summary>
        /// 获取货位
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        [WebMethod]
        public static string GetLocation(string organizationId)
        {
            DataTable table = BalanceWeightBalanceLogService.GetLocation(organizationId);
            return EasyUIJsonParser.ComboboxJsonParser.DataTableToJson(table);
        }

        [WebMethod]
        public static decimal CalculateStocks(string startTime, string endTime, string locationId, string organizationId)
        {
            return StocksService.CalculateStocks(organizationId, locationId, startTime, endTime);
        }
    }
}