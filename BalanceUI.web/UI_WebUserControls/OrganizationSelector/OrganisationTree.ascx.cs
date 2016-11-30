using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data;
namespace WebUserContorls.Web.UI_WebUserControls.OrganizationSelector
{
    public partial class OrganisationTree : System.Web.UI.UserControl
    {
        private List<string> _Organizations;
        private string _PageName;
        protected void Page_Load(object sender, EventArgs e)
        {
            HiddenField_PageName.Value = _PageName;
            if (!IsPostBack)
            {
                string m_FunctionName = Request.Form["myFunctionName"] == null ? "" : Request.Form["myFunctionName"].ToString();             //方法名称,调用后台不同的方法
                string m_Type = Request.Form["myType"] == null ? "" : Request.Form["myType"].ToString();             //方法名称,调用后台不同的方法
                if (m_FunctionName == "GetOrganisationTree")
                {
                    string m_OrganizationTree = GetOrganisationTree(m_Type);
                    Response.Write(m_OrganizationTree);
                    Response.End();
                }
                else if (m_FunctionName == "GetProductionLineType")
                {
                    string m_ProductionLineType = GetProductionLineType();
                    Response.Write(m_ProductionLineType);
                    Response.End();
                }
            }
        }
        public List<string> Organizations
        {
            get
            {
                return _Organizations;
            }
            set
            {
                _Organizations = value;
            }
        }
        public string PageName
        {
            get
            {
                return _PageName;
            }
            set
            {
                _PageName = value;
            }
        }

        private string GetOrganisationTree(string myType)
        {
            DataTable m_OrganisationInfo = WebUserControls.Service.OrganizationSelector.OrganisationTree.GetOrganisationTree(_Organizations, myType, true);
            return EasyUIJsonParser.TreeJsonParser.DataTableToJsonByLevelCode(m_OrganisationInfo, "LevelCode", "Name", new string[] { "OrganizationId", "OrganizationType" });
        }
        private string GetProductionLineType()
        {
            DataTable m_OrganisationInfo = WebUserControls.Service.OrganizationSelector.OrganisationTree.GetProductionLineType();
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_OrganisationInfo);
        }
    }
}