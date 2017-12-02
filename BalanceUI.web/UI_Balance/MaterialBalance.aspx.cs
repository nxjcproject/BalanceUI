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
    public partial class MaterialBalance : WebStyleBaseForEnergy.webStyleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            base.InitComponts();
            if (!IsPostBack)
            {
#if DEBUG
                // 调试用,自定义的数据授权
                List<string> m_DataValidIdItems = new List<string>() { "zc_nxjc_byc_byf", "zc_nxjc_qtx_efc", "zc_nxjc_znc" };
                AddDataValidIdGroup("ProductionOrganization", m_DataValidIdItems);
                mPageOpPermission = "1111";
#endif
                this.OrganisationTree.Organizations = GetDataValidIdGroup("ProductionOrganization");                 //向web用户控件传递数据授权参数
                this.OrganisationTree.PageName = "BasicDataCorrection.aspx";
                this.OrganisationTree.LeveDepth = 5;
            }
        }
        [WebMethod]
        public static string GetMaterialBalanceReport(string OrganizationId, string startTime, string endTime)
        {
            DataTable MaterialBalanceReport = BalanceUI.Service.BalanceService.MaterialBalance.GetData(OrganizationId, startTime, endTime);
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(MaterialBalanceReport);
        }
    }
   
}