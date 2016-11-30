using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data;

namespace WebUserContorls.Web.UI_WebUserControls.TagsSelector
{
    public partial class TagsSelector_Dcs : System.Web.UI.UserControl
    {
        private List<string> _Organizations;
        private string _PageName;
        protected void Page_Load(object sender, EventArgs e)
        {
            HiddenField_PageName.Value = _PageName;
            if (!IsPostBack)
            {
                string m_FunctionName = Request.Form["myFunctionName"] == null ? "" : Request.Form["myFunctionName"].ToString();             //方法名称,调用后台不同的方法
                string m_BatchNumber = Request.Form["myBatchNumber"] == null ? "" : Request.Form["myBatchNumber"].ToString();                   //方法的参数名称1
                string m_BatchSize = Request.Form["myBatchSize"] == null ? "" : Request.Form["myBatchSize"].ToString();
                string m_DataBaseName = Request.Form["myDataBaseName"] == null ? "" : Request.Form["myDataBaseName"].ToString();
                string m_DcsTagsType = Request.Form["myDcsTagsType"] == null ? "" : Request.Form["myDcsTagsType"].ToString();
                string m_DcsTagsName = Request.Form["myDcsTagsName"] == null ? "" : Request.Form["myDcsTagsName"].ToString();

                if (m_FunctionName == "GetDCSTagsDataBase")
                {
                    string m_DcsOrganization = GetDCSTagsDataBase();
                    Response.Write(m_DcsOrganization);
                    Response.End();
                }
                else if (m_FunctionName == "GetDCSTagsByDataBase")
                {
                    string m_DcsTags = "";
                    try
                    {
                        m_DcsTags = GetDCSTagsByDataBase(Int32.Parse(m_BatchNumber), Int32.Parse(m_BatchSize), m_DataBaseName, m_DcsTagsName, m_DcsTagsType, false);
                    }
                    catch
                    {
                        Response.Write("无效的页码!");
                        Response.End();
                    }
                    Response.Write(m_DcsTags);
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
        private string GetDCSTagsDataBase()
        {
            DataTable m_DCSTagsDataBase = WebUserControls.Service.TagsSelector.TagsSelector_Dcs.GetDCSTagsDataBase(_Organizations, true);
            return EasyUIJsonParser.TreeJsonParser.DataTableToJsonByLevelCode(m_DCSTagsDataBase, "LevelCode", "Name", new string[] { "DcsProcessDatabase" });
        }
        private string GetDCSTagsByDataBase(int myBatchNumber, int myBatchSize, string myDataBaseName, string myDcsTagsName, string myDcsTagsType, bool myIsCumulant)
        {
            DataTable m_DCSTagsByDataBase = WebUserControls.Service.TagsSelector.TagsSelector_Dcs.GetDCSTagsByDataBase(myBatchNumber, myBatchSize, myDataBaseName, myDcsTagsName, myDcsTagsType, myIsCumulant);
            int m_DcsTagsCount = WebUserControls.Service.TagsSelector.TagsSelector_Dcs.GetDCSTagsCountByDataBase(myDataBaseName, myDcsTagsName, myDcsTagsType, myIsCumulant);
            return EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_DCSTagsByDataBase, m_DcsTagsCount);
        }
    }
}