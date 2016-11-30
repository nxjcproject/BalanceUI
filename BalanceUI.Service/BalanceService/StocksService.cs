using BalanceUI.Infrastruture.Configuration;
using SqlServerDataAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BalanceUI.Service.BalanceService
{
    /// <summary>
    /// 计算库存量的类
    /// </summary>
    public class StocksService
    {
        private static readonly string _connStr = ConnectionStringFactory.NXJCConnectionString;
        private static SqlConnection con = new SqlConnection(_connStr);
        private static readonly ISqlServerDataFactory _dataFactory = new SqlServerDataFactory(_connStr);

        /// <summary>
        /// 计算本期库存
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="locationId">货位ID</param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static decimal CalculateStocks(string organizationId, string locationId, string startTime, string endTime)
        {
            string formula = GetFormula(organizationId, locationId);
            FormulaParser formulaParser = new FormulaParser(formula);
            IList<string> sInputList = formulaParser.GetInputFactorList("S");
            IList<string> mInputList = formulaParser.GetInputFactorList("M");

            IList<string> sOutList = formulaParser.GetOutputFactorList("S");
            IList<string> mOutList = formulaParser.GetOutputFactorList("M");
            //上期库存
            decimal lastTimeStocksValue = GetLastTimeStocks(organizationId, locationId, startTime);
            //本期投入
            decimal inputValue = CalculateInput(startTime, endTime, organizationId, sInputList, mInputList);
            //本期消耗
            decimal outputValue = CalculateOutput(startTime, endTime, organizationId, sOutList, mOutList);

            return lastTimeStocksValue+inputValue - outputValue;
        }

        private static decimal GetLastTimeStocks(string organizationId, string locationId,string endTime)
        {
            decimal lastTimeStocksValue = 0;
            string sql = @"select WareStoreValue
                            from balance_WeightBalanceLog
                            where OrganizationID=@organizationId
                            and ProductionZoneId=@locationId
                            and Type='MaterialWeight'
                            and CONVERT(varchar(10), EndTime,120)=@endTime";
            SqlParameter[] parameters = { new SqlParameter("organizationId", organizationId),
                                        new SqlParameter("locationId", locationId),
                                        new SqlParameter("endTime", endTime)};
            DataTable table = _dataFactory.Query(sql, parameters);
            if (table.Rows.Count == 1)
            {
                lastTimeStocksValue = Convert.ToDecimal(table.Rows[0]["WareStoreValue"]);
            }
            return lastTimeStocksValue;
        }
        /// <summary>
        /// 获取库存计算公式
        /// </summary>
        /// <param name="locationId">货位ID</param>
        /// <returns></returns>
        public static string GetFormula(string organizationId,string locationId)
        {
            string sql = @"select Formula
                            from [dbo].[interface_FinishedProductsLocations]
                            where OrganizationID=@organizationId
                            and LocationID=@locationId
                            and Enabled='true'";
            SqlParameter[] parameters = { new SqlParameter("organizationId",organizationId),
                                        new SqlParameter("locationId",locationId)};
            DataTable table = _dataFactory.Query(sql, parameters);
            if (table.Rows.Count > 1)
            {
                throw new Exception("数据库表[interface_FinishedProductsLocations]错误，该货位不止对应一条可用记录！");
            }
            else if (table.Rows.Count == 0)
            {
                return "";
            }
            else
            {
                return table.Rows[0]["Formula"].ToString();
            }
        }
        /// <summary>
        /// 计算产量值
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="organizationId"></param>
        /// <param name="sInputList"></param>
        /// <param name="mInputList"></param>
        /// <returns></returns>
        private static decimal CalculateInput(string startTime, string endTime, string organizationId, IList<string> sInputList, IList<string> mInputList)
        {
//            string m_sql = @"select A.*
//                                from [dbo].[interface_ WeightNYGL] A,
//	                                (select MaterialID
//	                                from [dbo].[interface_MaterialInfo]
//	                                where {0})B
//                                where A.Material=B.MaterialID
//                                and A.OrganizationID=@organizationId
//                                and A.vDate>=@startTime
//                                and A.vDate<=@endTime";
            decimal s_value = 0, m_value = 0;
            if (mInputList.Count > 0)
            {
                string m_sql = @"select Sum(A.Suttle) as Value
                                from [dbo].[interface_ WeightNYGL] A,
	                                (select MaterialID
	                                from [dbo].[interface_MaterialInfo]
	                                where ({0}))B
                                where A.Material=B.MaterialID
                                and A.OrganizationID=@organizationId
                                and A.vDate>=@startTime
                                and A.vDate<=@endTime
                                and (A.Type='0' or (A.Type='5' and A.reservationchar7='11'))";
                StringBuilder m_builder = new StringBuilder();
                foreach (string item in mInputList)
                {
                    m_builder.Append("VariableID='" + item + "' or ");
                }
                m_builder = m_builder.Remove(m_builder.Length - 4, 4);
                SqlParameter[] m_parameters = { new SqlParameter("organizationId", organizationId), 
                                        new SqlParameter("startTime", startTime),
                                        new SqlParameter("endTime", endTime)};
                DataTable m_table = _dataFactory.Query(string.Format(m_sql, m_builder.ToString()), m_parameters);
                if (m_table.Rows.Count > 0)
                {
                    m_value = Convert.ToDecimal(m_table.Rows[0]["Value"] is DBNull?0: m_table.Rows[0]["Value"]);
                }
            }
            //decimal m_value = Convert.ToDecimal(m_table.Compute("Sum(Suttle)", "Type='3' or (Type='5' and reservationchar7='11')"));
            if (sInputList.Count > 0)
            {
                string s_sql = @"select Sum(B.TotalPeakValleyFlatB) as Value
                                from [dbo].[tz_Balance] A,[dbo].[balance_Production] B
                                where A.BalanceId=B.KeyId
                                and A.OrganizationID=@organizationId
                                and A.TimeStamp>=@startTime
                                and A.TimeStamp<=@endTime
                                and ({0})";
                StringBuilder s_builder = new StringBuilder();
                foreach (string s in sInputList)
                {
                    s_builder.Append("B.VariableID='" + s + "' or ");
                }
                s_builder = s_builder.Remove(s_builder.Length - 4, 4);
                SqlParameter[] s_parameters = { new SqlParameter("organizationId", organizationId), 
                                        new SqlParameter("startTime", startTime),
                                        new SqlParameter("endTime", endTime)};
                DataTable s_table = _dataFactory.Query(string.Format(s_sql, s_builder.ToString()), s_parameters);
                if (s_table.Rows.Count > 0)
                {
                    s_value = Convert.ToDecimal(s_table.Rows[0]["Value"] is DBNull?0: s_table.Rows[0]["Value"]);
                }
            }
            return m_value+s_value;
        }

        /// <summary>
        /// 计算消耗量
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="organizationId"></param>
        /// <param name="sInputList"></param>
        /// <param name="mInputList"></param>
        /// <returns></returns>
        private static decimal CalculateOutput(string startTime, string endTime, string organizationId, IList<string> sInputList, IList<string> mInputList)
        {
            decimal s_value = 0, m_value = 0;
            if (mInputList.Count > 0)
            {
                string m_sql = @"select Sum(A.Suttle) as Value
                                from [dbo].[interface_ WeightNYGL] A,
	                                (select MaterialID
	                                from [dbo].[interface_MaterialInfo]
	                                where ({0}))B
                                where A.Material=B.MaterialID
                                and A.OrganizationID=@organizationId
                                and A.vDate>=@startTime
                                and A.vDate<=@endTime
                                and (A.Type='3' or (A.Type='5' and A.reservationchar7='10'))";
                StringBuilder m_builder = new StringBuilder();
                foreach (string item in mInputList)
                {
                    m_builder.Append("VariableID='" + item + "' or ");
                }
                m_builder = m_builder.Remove(m_builder.Length - 4, 4);
                SqlParameter[] m_parameters = { new SqlParameter("organizationId", organizationId), 
                                        new SqlParameter("startTime", startTime),
                                        new SqlParameter("endTime", endTime)};
                DataTable m_table = _dataFactory.Query(string.Format(m_sql, m_builder.ToString()), m_parameters);
                if (m_table.Rows.Count > 0)
                {
                    m_value = Convert.ToDecimal(m_table.Rows[0]["Value"] is DBNull?0: m_table.Rows[0]["Value"]);
                }
            }
            //decimal m_value = Convert.ToDecimal(m_table.Compute("Sum(Suttle)", "Type='3' or (Type='5' and reservationchar7='11')"));
            if (sInputList.Count > 0)
            {
                string s_sql = @"select Sum(B.TotalPeakValleyFlatB) as Value
                                from [dbo].[tz_Balance] A,[dbo].[balance_Production] B
                                where A.BalanceId=B.KeyId
                                and A.OrganizationID=@organizationId
                                and A.TimeStamp>=@startTime
                                and A.TimeStamp<=@endTime
                                and ({0})";
                StringBuilder s_builder = new StringBuilder();
                foreach (string s in sInputList)
                {
                    s_builder.Append("B.VariableID='" + s + "' or ");
                }
                s_builder = s_builder.Remove(s_builder.Length - 4, 4);
                SqlParameter[] s_parameters = { new SqlParameter("organizationId", organizationId), 
                                        new SqlParameter("startTime", startTime),
                                        new SqlParameter("endTime", endTime)};
                DataTable s_table = _dataFactory.Query(string.Format(s_sql, s_builder.ToString()), s_parameters);
                if (s_table.Rows.Count > 0)
                {
                    s_value = Convert.ToDecimal(s_table.Rows[0]["Value"] is DBNull ? 0 : s_table.Rows[0]["Value"]);
                }
            }
            return m_value + s_value;
        }
    }
}
