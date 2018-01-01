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
    public partial class BalanceMachineHalt : WebStyleBaseForEnergy.webStyleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            base.InitComponts();
            if (!IsPostBack)
            {
#if DEBUG
                // 调试用,自定义的数据授权
                List<string> m_DataValidIdItems = new List<string>() { "zc_nxjc_byc_byf", "zc_nxjc_qtx_tys" };
                AddDataValidIdGroup("ProductionOrganization", m_DataValidIdItems);
                mPageOpPermission = "1111";
#endif
                this.OrganisationTree.Organizations = GetDataValidIdGroup("ProductionOrganization");                 //向web用户控件传递数据授权参数
                this.OrganisationTree.PageName = "BalanceMachineHalt.aspx";
                this.OrganisationTree.LeveDepth = 5;
            }
        }
        [WebMethod]
        public static char[] AuthorityControl()
        {
            return mPageOpPermission.ToArray();
        }
        [WebMethod]
        public static string GetMachineHaltLog(string myOrganizationId, string myStartHaltTime, string myEndHaltTime, string myEquipmentId)
        {
            DataTable m_ResultTable = BalanceMachineHaltService.GetMachineHaltLog(myOrganizationId, myStartHaltTime, myEndHaltTime, myEquipmentId);
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_ResultTable);
        }
        [WebMethod]
        public static string GetMachineHaltLogbyId(string myMachineHaltLogId)
        {
            DataTable m_ResultTable = BalanceMachineHaltService.GetMachineHaltLogbyId(myMachineHaltLogId);
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_ResultTable);
        }
        [WebMethod]
        public static string GetMasterMachineInfo(string myOrganizationId)
        {
            DataTable m_ResultTable = BalanceMachineHaltService.GetMasterMachineInfo(myOrganizationId);
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_ResultTable);
        }
        [WebMethod]
        public static string GetMachineHaltReasonsWithTreeGridFormat()
        {
            DataTable dt = BalanceMachineHaltService.GetMachineHaltReasons();
            DataColumn parentIdColumn = new DataColumn("ParentID");
            dt.Columns.Add(parentIdColumn);

            foreach (DataRow row in dt.Rows)
            {
                string levelcode = row["MachineHaltReasonID"].ToString().Trim();
                row["MachineHaltReasonID"] = row["MachineHaltReasonID"].ToString().Trim();
                if (levelcode.Length > 3)
                {
                    row["ParentID"] = levelcode.Substring(0, levelcode.Length - 2);
                }
                else
                {
                    row["ParentID"] = 0;
                }
            }
            string m_ValueString = EasyUIJsonParser.TreeJsonParser.DataTableToJson(dt, "MachineHaltReasonID", "ReasonText", "ParentID", "0");
            return m_ValueString;
        }
        [WebMethod]
        public static string ModifyMachineHalt(string myMachineHaltLogId, string myMachineStartTime, string myMachineHaltTime, string myMachineRecoverTime, string myWorkingTeamShiftLogId, string myMachineHaltReason, string myMachineHaltReasonText, string myRemark)
        {
            if (mUserId != "")
            {
                string m_ModiyInfoJson = BalanceMachineHaltService.ModifyMachineHalt(myMachineHaltLogId, myMachineStartTime, myMachineHaltTime, myMachineRecoverTime, myWorkingTeamShiftLogId, myMachineHaltReason, myMachineHaltReasonText, myRemark).ToString();
                return m_ModiyInfoJson;
            }
            else
            {
                return "非法的用户操作!";
            }
        }
        [WebMethod]
        public static string AddMachineHalt(string myEquipmentId, string myOrganizationId, string myMachineStartTime, string myMachineHaltTime, string myMachineRecoverTime, string myWorkingTeamShiftLogId, string myMachineHaltReason, string myRemark)
        {
            if (mUserId != "")
            {
                string m_AddInfoJson = BalanceMachineHaltService.AddMachineHalt(myEquipmentId, myOrganizationId, myMachineStartTime, myMachineHaltTime, myMachineRecoverTime, myWorkingTeamShiftLogId, myMachineHaltReason, myRemark).ToString();
                return m_AddInfoJson;
            }
            else
            {
                return "非法的用户操作!";
            }
        }
        [WebMethod]
        public static string DeleteMachineHalt(string myMachineHaltLogId)
        {
            if (mUserId != "")
            {
                string m_DeleteInfoJson = BalanceMachineHaltService.RemoveMachineHaltLogById(myMachineHaltLogId).ToString();
                return m_DeleteInfoJson;
            }
            else
            {
                return "非法的用户操作!";
            }
        }
        [WebMethod]
        public static string GetMachineHaltLogbyDate(string myDateTime, string myOrganizationId)
        {
            if (myDateTime != "")
            {
                string m_Date = DateTime.Parse(myDateTime).ToString("yyyy-MM-dd");
                DataTable m_ResultTable = BalanceMachineHaltService.GetMachineHaltLogbyDate(m_Date, myOrganizationId);
                return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_ResultTable);
            }
            else
            {
                return "{\"rows\":[],\"total\":0}";
            }
        }
    }
}