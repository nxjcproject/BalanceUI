using BalanceUI.Service;
using BalanceUI.Service.BalanceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BalanceUI.web
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            FormulaParser formulaParser = new FormulaParser(" S101  + S107-S180+M123-M342");
            IList<string> list = formulaParser.GetAllFactorList();
            IList<string> s_List = formulaParser.GetSList();
            IList<string> m_List = formulaParser.GetMList();
            IList<string> inputList = formulaParser.GetInputFactorList();
            IList<string> outputList = formulaParser.GetOutputFactorList();

            IList<string> s_inputList = formulaParser.GetInputFactorList("S");
            IList<string> m_inputList = formulaParser.GetInputFactorList("M");

            IList<string> s_outputList = formulaParser.GetOutputFactorList("S");
            IList<string> m_outputList = formulaParser.GetOutputFactorList("M");

            List<string> mlist=new List<string>();
            mlist.Add("M001");
            mlist.Add("M002");
            List<string> sList=new List<string>();
            //StocksService.CalculateInput("2016-04-01", "2016-06-01", "zc_nxjc_byc_byf", s_List, mlist);
        }
    }
}