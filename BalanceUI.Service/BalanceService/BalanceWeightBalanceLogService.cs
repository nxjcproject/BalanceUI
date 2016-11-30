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
    public class BalanceWeightBalanceLogService
    {
        private static readonly string _connStr = ConnectionStringFactory.NXJCConnectionString;
        private static SqlConnection con = new SqlConnection(_connStr);
        private static readonly ISqlServerDataFactory _dataFactory = new SqlServerDataFactory(_connStr);
        /// <summary>
        /// 获取最后一次平衡时间
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public static string GetLastBalanceLogTime(string organizationId)
        {
            string sql = @"select top(1) A.EndTime
                            from balance_WeightBalanceLog A
                            where A.OrganizationID=@organizationId
                            order by A.EndTime desc";
            SqlParameter parameter = new SqlParameter("organizationId", organizationId);
            DataTable table = _dataFactory.Query(sql, parameter);
            if (table.Rows.Count == 1)
            {
                return Convert.ToDateTime(table.Rows[0]["EndTime"]).ToString("yyyy-MM-dd");
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 获取平衡日志
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public static DataTable GetLogInfo(string organizationId, string locationId)
        {
            string sql = @"select B.LocationName,B.LocationID ,A.*
                            from balance_WeightBalanceLog A
                            left join
                            interface_FinishedProductsLocations B
                            on A.ProductionZoneId=B.LocationID
                            where A.OrganizationID=@organizationId
                                    and A.ProductionZoneId=@locationId
                            order by A.EndTime desc";
            SqlParameter[] parameters = {new SqlParameter("organizationId", organizationId)
                                        ,new SqlParameter("locationId",locationId)};
            return _dataFactory.Query(sql, parameters);
        }
        /// <summary>
        /// 获取物料种类
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public static DataTable GetMaterualKind(string organizationId)
        {
            string sql = @"select Distinct B.VariableId,M.Name
                                from tz_balance A,[dbo].[balance_Production] B,
		                                (select B.Name,B.Formula
		                                from tz_Material A,[dbo].[material_MaterialDetail] B,
		                                (select OrganizationID
		                                from system_Organization
		                                where LevelCode like
			                                (select LevelCode
			                                from system_Organization
			                                where OrganizationID=@organizationId)+'%'
		                                ) D
		                                where A.KeyId=B.KeyId
		                                and A.OrganizationID = D.OrganizationID)M
                                where A.BalanceId=B.KeyId
                                and B.VariableType='DirectTag'
                                and B.ValueType='Increment'
                                and A.OrganizationId=@organizationId
                                and B.VariableId=M.Formula
                                order by B.VariableId";
            SqlParameter parameter = new SqlParameter("organizationId", organizationId);
            return _dataFactory.Query(sql, parameter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="variableId"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public static DataTable GetBalanceData(string startTime, string endTime, string locationId, string organizationId)
        {
            DataTable resultTable = new DataTable();

            string formula = StocksService.GetFormula(organizationId, locationId);
            FormulaParser formulaParser = new FormulaParser(formula);
            IList<string> sList = formulaParser.GetSList();
            IList<string> sInputList = formulaParser.GetInputFactorList("S");
            IList<string> sOutputList = formulaParser.GetInputFactorList("S");

            if (sList.Count == 0)
            {
                return resultTable;
            }
//            string sql = @"select B.VariableItemId,TimeStamp ,VariableId,TotalPeakValleyFlatB, MorePeakB, 
//                                            PeakB, ValleyB, MoreValleyB, FlatB, FirstB, SecondB, ThirdB
//                            from tz_Balance A,balance_Production B,interface_FinishedProductsLocations C
//                            where A.BalanceId=B.KeyId
//                            and B.VariableId=C.MaterialId
//                            and A.TimeStamp>@startTime
//                            and A.TimeStamp<=@endTime
//                            and C.LocationID=@locationId
//                            and A.OrganizationID=@organizationId";
            string sql = @"select B.VariableItemId,TimeStamp ,VariableId,TotalPeakValleyFlatB, MorePeakB, 
                                            PeakB, ValleyB, MoreValleyB, FlatB, FirstB, SecondB, ThirdB
                            from tz_Balance A,balance_Production B
                            where A.BalanceId=B.KeyId                           
                            and A.TimeStamp>@startTime
                            and A.TimeStamp<=@endTime
                            and A.OrganizationID=@organizationId
                            and ({0})
                            order by A.TimeStamp,VariableId";
            StringBuilder sBuilder = new StringBuilder();
            foreach (string s in sList)
            {
                sBuilder.Append("B.VariableId='"+s+"' or ");
            }
            sBuilder = sBuilder.Remove(sBuilder.Length - 4, 4);
            SqlParameter[] parameters = { new SqlParameter("startTime",startTime) ,
                                        new SqlParameter("endTime",endTime) ,
                                        new SqlParameter("locationId",locationId) ,
                                        new SqlParameter("organizationId" ,organizationId)};
            resultTable = _dataFactory.Query(string.Format(sql, sBuilder.ToString()), parameters);
            DataColumn dc = new DataColumn("Type", typeof(string));
            resultTable.Columns.Add(dc);
            foreach (DataRow dr in resultTable.Rows)
            {
                if (sInputList.Contains(dr["VariableId"].ToString().Trim()))
                {
                    dr["Type"] = "Input";
                }
                else
                {
                    dr["Type"] = "Output";
                }
            }
            return resultTable;
        }
        public static string SaveData(string organizationId, DataTable table, DataTable diffValueTable,string productionZoneId, string startTime, string endTime,
                                      string stocksValue, string inputValue, string outputValue, string logId, string type,string creator)
        {
            //建立事务
            con.Open();
            using (SqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    if (type == "edit")
                    {
                        UpdateBalanceLog(stocksValue, inputValue, outputValue, logId, transaction);
                    }
                    if ("insert" == type)
                    {
                        //新建操作
                        InsertBalanceLog(organizationId,productionZoneId, startTime, endTime, stocksValue, inputValue, outputValue, creator, transaction);
                    }
                    UpdateBalanceProductData(table, transaction);
                    UpdateBalanceEnergyData(organizationId, diffValueTable, transaction);
                    transaction.Commit();
                    return "保存成功";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return "保存失败";
                }
                finally
                {
                    con.Close();
                }
            }
        }
        /// <summary>
        /// 更新平衡日志
        /// </summary>
        private static void UpdateBalanceLog(string wareStoreValue, string offsetInputValue, string offsetOutputValue,string logId, SqlTransaction transaction)
        {
            SqlCommand cmd = con.CreateCommand();
            cmd.Transaction = transaction;
            string sql = @"update [dbo].balance_WeightBalanceLog
                            set WareStoreValue='{0}',OffsetInputValue='{1}',
	                            OffsetOutputValue='{2}',CreateTime='{3}'
                            where LogId='{4}'";
            cmd.CommandText = string.Format(sql,wareStoreValue,offsetInputValue,offsetOutputValue,DateTime.Now.ToString("yyyy-MM-dd"),logId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 新建平衡日志
        /// </summary>
        private static void InsertBalanceLog(string organizationId,string productionZoneId, string startTime, string endTime, string wareStoreValue, string offsetInputValue, string offsetOutputValue, string creator, SqlTransaction transaction)
        {
            SqlCommand cmd = con.CreateCommand();
            cmd.Transaction = transaction;
            string sql = @"insert into balance_WeightBalanceLog
                            (LogId,ProductionZoneId,OrganizationID,Type,StartTime,EndTime,WareStoreValue,OffsetInputValue,OffsetOutputValue,Creator,CreateTime)
                            values
                            ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}')";
            cmd.CommandText = string.Format(sql, Guid.NewGuid().ToString(), productionZoneId,organizationId, "MaterialWeight", startTime, endTime, wareStoreValue, offsetInputValue, offsetOutputValue, creator, DateTime.Now.ToString("yyyy-MM-dd"));
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 更新balance_Production中的数据
        /// </summary>
        private static void UpdateBalanceProductData(DataTable table, SqlTransaction transaction)
        {
            //using (SqlConnection con = new SqlConnection(_connStr))
            //{
            SqlCommand cmd = con.CreateCommand();
            cmd.Transaction = transaction;
            string sql = @"update balance_Production
                                        set TotalPeakValleyFlatB='{0}', MorePeakB='{1}', 
                                            PeakB='{2}', ValleyB='{3}', MoreValleyB='{4}', FlatB='{5}', FirstB='{6}', SecondB='{7}', ThirdB='{8}'

                                        where VariableItemId='{9}'";
            //con.Open();
            foreach (DataRow dr in table.Rows)
            {

                cmd.CommandText = string.Format(sql, dr["TotalPeakValleyFlatB"].ToString(), dr["MorePeakB"].ToString(), dr["PeakB"].ToString(),
                    dr["ValleyB"].ToString(), dr["MoreValleyB"].ToString(), dr["FlatB"].ToString(), dr["FirstB"].ToString(),
                    dr["SecondB"].ToString(), dr["ThirdB"].ToString(), dr["VariableItemId"].ToString());
                cmd.ExecuteNonQuery();
            }
            //con.Close();
            //}

        }
        private static void UpdateBalanceEnergyData(string organizationId, DataTable diffValueTable, SqlTransaction transaction)
        {
            foreach (DataRow dr in diffValueTable.Rows)
            {
                //找出包含S**的变量
                string sql = @"select B.*
                            from tz_Balance A,balance_Energy B,
	                            (select B.VariableId,B.Name,B.Formula
	                            from tz_Material A,material_MaterialDetail B,system_Organization C,
	                            (select LevelCode from system_Organization where organizationId=@organizationId) D
	                            where A.keyid=B.keyid
	                            and A.OrganizationID=C.OrganizationID
	                            and C.LevelCode like D.LevelCode+'%'
	                            and CHARINDEX('{0}',B.Formula)>0) C
                            where A.BalanceId=B.KeyId
                            and B.VariableId=C.VariableId
                            and A.TimeStamp=@timestamp
                            and A.OrganizationId=@organizationId";
                string timestamp = dr["TimeStamp"].ToString().Trim();
                string sMaterial = dr["VariableId"].ToString().Trim();
                SqlParameter[] parameters = { new SqlParameter("organizationId", organizationId), 
                                            new SqlParameter("timestamp", timestamp)};
                DataTable table = _dataFactory.Query(string.Format(sql, sMaterial), parameters);
                string[] columnArray = { "TotalPeakValleyFlatB", "MorePeakB", "PeakB", "ValleyB", "MoreValleyB", "FlatB", "FirstB", "SecondB", "ThirdB" };
                foreach (DataRow row in table.Rows)
                {
                    foreach (string item in columnArray)
                    {
                        row[item] = Convert.ToDecimal(row[item]) + Convert.ToDecimal(dr[item]);
                    }
                }
                //using (SqlConnection con = new SqlConnection(_connStr))
                //{
                SqlCommand cmd = con.CreateCommand();

                string mysql = @"update balance_Energy
                                        set TotalPeakValleyFlatB='{0}', MorePeakB='{1}', 
                                            PeakB='{2}', ValleyB='{3}', MoreValleyB='{4}', FlatB='{5}', FirstB='{6}', SecondB='{7}', ThirdB='{8}'

                                        where VariableItemId='{9}'";
                //con.Open();
                cmd.Transaction = transaction;
                foreach (DataRow trow in table.Rows)
                {

                    cmd.CommandText = string.Format(mysql, trow["TotalPeakValleyFlatB"].ToString(), trow["MorePeakB"].ToString(), trow["PeakB"].ToString(),
                        trow["ValleyB"].ToString(), trow["MoreValleyB"].ToString(), trow["FlatB"].ToString(), trow["FirstB"].ToString(),
                        trow["SecondB"].ToString(), trow["ThirdB"].ToString(), trow["VariableItemId"].ToString());

                    cmd.ExecuteNonQuery();
                }
                // con.Close();
                //}
            }
        }
        private static void UpdateBalanceEnergyData(string organizationId, string sMaterial, string time)
        {
            //查找出几个S变量相加的
            string sql = @"select B.VariableId,B.Name,B.Formula
                            from tz_Material A,material_MaterialDetail B,system_Organization C,
                            (select LevelCode from system_Organization where organizationId=@organizationId) D
                            where A.keyid=B.keyid
                            and A.OrganizationID=C.OrganizationID
                            and C.LevelCode like D.LevelCode+'%'
                            and len(ltrim(rtrim(B.Formula)))>4";
            SqlParameter parameter = new SqlParameter("organizationId", organizationId);
            DataTable table = _dataFactory.Query(sql, parameter);
            foreach (DataRow dr in table.Rows)
            {
                string formula = dr["Formula"].ToString().Trim();
                //只有该公式包含平衡的S变量时才做处理
                if (formula.Contains(sMaterial))
                {
                    string[] factorArray = formula.Split('+');
                    for (int i = 0; i < factorArray.Count(); i++)
                    {
                        factorArray[i] = factorArray[i].Trim();
                    }
                    List<string> factorList = factorArray.ToList();
                    factorList.Remove(sMaterial);
                    StringBuilder sBuilder = new StringBuilder();
                    foreach (string s in factorList)
                    {
                        sBuilder.Append("B.Formula='" + s + "' or ");
                    }
                    sBuilder.Remove(sBuilder.Length - 4, 4);

                    string mySql = @"select  B.*
                                        from tz_Balance A,balance_Energy B,
                                        (select B.VariableId,B.Name,B.Formula
                                        from tz_Material A,material_MaterialDetail B,system_Organization C,
                                        (select LevelCode from system_Organization where organizationId=@organizationId) D
                                        where A.keyid=B.keyid
                                        and A.OrganizationID=C.OrganizationID
                                        and C.LevelCode like D.LevelCode+'%'
                                        and ({0})
                                        ) C
                                        where A.BalanceId=B.KeyId
                                        and A.OrganizationID=@organizationId
                                        and B.VariableId=C.VariableId
                                        and A.TimeStamp=@time";
                    SqlParameter[] parameters = { new SqlParameter("organizationId", organizationId),
                                                new SqlParameter("time",time)};
                    DataTable myTable = _dataFactory.Query(string.Format(sql, sBuilder.ToString()), parameters);
                }
            }
        }
        /// <summary>
        /// 查询货位
        /// </summary>
        /// <param name="organiationId"></param>
        /// <returns></returns>
        public static DataTable GetLocation(string organiationId)
        {
            string sql = @"select A.*
                            from interface_FinishedProductsLocations A,system_Organization B,
                            (select Levelcode from system_Organization where OrganizationID=@organiationId) C
                            where A.OrganizationID=B.OrganizationID
                            and B.LevelCode like C.LevelCode+'%'";
            SqlParameter parameter = new SqlParameter("organiationId", organiationId);
            DataTable table = _dataFactory.Query(sql, parameter);
            return table;
        }
    }
}
