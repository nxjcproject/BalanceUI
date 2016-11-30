using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BalanceUI.Infrastruture.Configuration;
using SqlServerDataAdapter;
using Balance.Infrastructure;
using Balance.Model;
using Balance.Infrastructure.BasicDate;

namespace BalanceUI.Service
{
    public class BasicDataCorrectionService
    {
        private const int UpdateToDataBasePageItemCount = 2000;
        private const int CollectionInterval = 10;          //采集间隔设定每5分钟采集一次
        private const int DataColumnIndex = 6;
        private static readonly string _connStr = ConnectionStringFactory.NXJCConnectionString;
        private static readonly ISqlServerDataFactory _dataFactory = new SqlServerDataFactory(_connStr);
        public static DataTable GetAbnormalData(string myOrganizationId, string myDeviationMagnification, string myCorrectionObject, string myMinValidValue, string myStartTime, string myEndTime, string myStartTimeReference, string myEndTimeReference)
        {           
            string m_FactoryDataBase = GetFactoryDataBase(myOrganizationId);
            DataTable m_DeviationDataTable = new DataTable();
            m_DeviationDataTable.Columns.Add("id",typeof(string));
            m_DeviationDataTable.Columns.Add("text",typeof(string));
            m_DeviationDataTable.Columns.Add("FieldName", typeof(string));
            m_DeviationDataTable.Columns.Add("Address", typeof(string));
            m_DeviationDataTable.Columns.Add("Type", typeof(string));
            m_DeviationDataTable.Columns.Add("AvgValue", typeof(decimal));
            if (m_FactoryDataBase != "")
            {
                DataTable m_DCSColumnNameTable = GetDCSInfoTable(m_FactoryDataBase);
                DataTable m_AmmeterColumnNameTable = GetAmmeterInfoTable(m_FactoryDataBase);
                DataTable m_DCSIncrementTable = GetDataIncrementInfo(m_FactoryDataBase + ".dbo.HistoryDCSIncrement", myDeviationMagnification, myMinValidValue, myStartTime, myEndTime, myStartTimeReference, myEndTimeReference);
                DataTable m_AmmeterIncrementTable = GetDataIncrementInfo(m_FactoryDataBase + ".dbo.HistoryAmmeterIncrement", myDeviationMagnification, myMinValidValue, myStartTime, myEndTime, myStartTimeReference, myEndTimeReference);
                if (myCorrectionObject == "All")
                {
                    GetDeviationData(m_DCSColumnNameTable, m_AmmeterColumnNameTable, m_DCSIncrementTable, m_AmmeterIncrementTable, myDeviationMagnification, ref m_DeviationDataTable);
                }
                else if (myCorrectionObject == "MaterialWeight")
                {
                    GetDeviationData(m_DCSColumnNameTable, null, m_DCSIncrementTable, null, myDeviationMagnification, ref m_DeviationDataTable);
                }
                else if (myCorrectionObject == "ElectricityQuantity")
                {
                    GetDeviationData(null, m_AmmeterColumnNameTable, null, m_AmmeterIncrementTable, myDeviationMagnification, ref m_DeviationDataTable);
                }
            }
            return m_DeviationDataTable;
        }
        public static string GetFactoryDataBase(string myOrganizationId)
        {
            string m_Sql = @"Select B.MeterDatabase, A.OrganizationId from system_Organization A, system_Database B
                              where A.OrganizationID = '{0}'
                              and A.DataBaseID = B.DatabaseID";
            m_Sql = string.Format(m_Sql, myOrganizationId);
            try
            {
                DataTable m_FactoryDataBaseTable = _dataFactory.Query(m_Sql);
                if (m_FactoryDataBaseTable != null && m_FactoryDataBaseTable.Rows.Count > 0)
                {
                    return m_FactoryDataBaseTable.Rows[0]["MeterDatabase"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }
        public static DataTable GetDCSInfoTable(string myDataBaseName)
        {
            string m_Sql = @"SELECT A.TagName as id
                                ,Rtrim(A.CumulantName) as FieldName
                                ,A.FieldName as Address
                                ,A.VariableDescription as text
                                ,'DCS' as Type
                                ,A.DCSName
                                ,A.DBName
                                ,A.VariableDescription
                                FROM {0}.dbo.View_DCSContrast A
                                where A.IsCumulant = 1
                                order by A.CumulantName";
            m_Sql = string.Format(m_Sql, myDataBaseName);
            try
            {
                DataTable m_DCSColumnNameTable = _dataFactory.Query(m_Sql);
                return m_DCSColumnNameTable;
            }
            catch
            {
                return null;
            }
        }
        public static DataTable GetAmmeterInfoTable(string myDataBaseName)
        {
            string m_Sql = @"SELECT A.AmmeterNumber as id
                                  ,Rtrim(A.ElectricEnergyFieldNameSave) as FieldName
                                  ,A.AmmeterAddress as Address
                                  ,A.ElectricRoom
	                              ,A.AmmeterName as text
                                  ,'Ammeter' as Type
	                              ,A.AmmeterNumber as ColumnName
                              FROM {0}.dbo.AmmeterContrast A
                              where A.EnabledFlag = 1
                              order by A.AmmeterNumber";
            m_Sql = string.Format(m_Sql, myDataBaseName);
            try
            {
                DataTable m_AmmeterColumnNameTable = _dataFactory.Query(m_Sql);
                return m_AmmeterColumnNameTable;
            }
            catch
            {
                return null;
            }
        }
        private static DataTable GetDataIncrementInfo(string myDataTableName, string myDeviationMagnification, string myMinValidValue, string myStartTime, string myEndTime, string myStartTimeReference, string myEndTimeReference)
        {
            DataTable m_DataIncrementColumnsTable = GetDataIncrementColumns(myDataTableName);
            //通过ColumnName统计查询的字段信息
            string m_Sql = @"Select 'Current' as Flag, A.vDate {5} from {0} A,
                                        (Select {6} from {0} B
                                          where B.vDate >='{3}'
                                          and B.vDate <= '{4}') C
                                        where A.vDate >='{1}'
                                          and A.vDate <= '{2}'
                                          {7}
                                        union all
                                        Select 'Avg' as Flag, null as vDate, {6} from {0} B
                                          where B.vDate >='{3}'
                                          and B.vDate <= '{4}'
                                          order by vDate";

            string m_SelectColumn = "";
            string m_SubSelectColumn = "";
            string m_DiffCondition = "";
            if (m_DataIncrementColumnsTable != null && m_DataIncrementColumnsTable.Columns.Count > 1)
            {
                for (int i = 1; i < m_DataIncrementColumnsTable.Columns.Count; i++)
                {
                    m_SelectColumn = m_SelectColumn + string.Format(", case when A.{0} is null then 0.0 else A.{0} end as {0}", m_DataIncrementColumnsTable.Columns[i].ColumnName, myDeviationMagnification);
                    if (i == 1)
                    {
                        m_SubSelectColumn = string.Format(" case when avg(case when B.{0} < {1} then null else B.{0} end) is null then {1} else avg(case when B.{0} < {1} then null else B.{0} end) end as {0}", m_DataIncrementColumnsTable.Columns[i].ColumnName, myMinValidValue);
                        m_DiffCondition = string.Format(" and ((abs(A.{0}) - C.{0} * {1}) > 0", m_DataIncrementColumnsTable.Columns[i].ColumnName, myDeviationMagnification);
                    }
                    else
                    {
                        m_SubSelectColumn = m_SubSelectColumn + string.Format(", case when avg(case when B.{0} < {1} then null else B.{0} end) is null then {1} else avg(case when B.{0} < {1} then null else B.{0} end) end as {0}", m_DataIncrementColumnsTable.Columns[i].ColumnName, myMinValidValue);
                        m_DiffCondition = m_DiffCondition + string.Format(" or (abs(A.{0}) - C.{0} * {1}) > 0", m_DataIncrementColumnsTable.Columns[i].ColumnName, myDeviationMagnification);
                    }
                }
                if (m_DiffCondition != "")
                {
                    m_DiffCondition = m_DiffCondition + ")";
                }
                m_Sql = m_Sql.Replace("{0}", myDataTableName);
                m_Sql = m_Sql.Replace("{1}", myStartTime);
                m_Sql = m_Sql.Replace("{2}", myEndTime);
                m_Sql = m_Sql.Replace("{3}", myStartTimeReference);
                m_Sql = m_Sql.Replace("{4}", myEndTimeReference);
                m_Sql = m_Sql.Replace("{5}", m_SelectColumn);
                m_Sql = m_Sql.Replace("{6}", m_SubSelectColumn);
                m_Sql = m_Sql.Replace("{7}", m_DiffCondition);
                try
                {
                    DataTable m_MachineHaltTable = _dataFactory.Query(m_Sql);
                    return m_MachineHaltTable;
                }
                catch
                {
                    return null;   //"{\"rows\":[],\"total\":0}";
                }
            }
            else
            {
                return null;
            }


        }
        private static DataTable GetDataIncrementColumns(string myDataTableName)
        {
            string m_Sql = @"SELECT top 0 A.* FROM {0} A";
            m_Sql = string.Format(m_Sql, myDataTableName);
            try
            {
                DataTable m_DataIncrementTable = _dataFactory.Query(m_Sql);
                return m_DataIncrementTable;
            }
            catch
            {
                return null;
            }
        }
        private static void GetDeviationData(DataTable myDCSColumnNameTable, DataTable myAmmeterColumnNameTable, DataTable myDCSIncrementTable, DataTable myAmmeterIncrementTable, string myDeviationMagnification, ref DataTable myDeviationDataTable)
        {
            List<string> m_DateTimeArray = new List<string>();
            if (myAmmeterColumnNameTable != null && myAmmeterIncrementTable != null)
            {
                for (int i = 0; i < myAmmeterIncrementTable.Rows.Count; i++)
                {
                    if (myAmmeterIncrementTable.Rows[i]["Flag"].ToString() == "Current" && myAmmeterIncrementTable.Rows[i]["vDate"] != DBNull.Value)
                    {
                        if (!m_DateTimeArray.Contains(((DateTime)myAmmeterIncrementTable.Rows[i]["vDate"]).ToString("yyyyMMddHHmmss")))
                        {
                            m_DateTimeArray.Add(((DateTime)myAmmeterIncrementTable.Rows[i]["vDate"]).ToString("yyyyMMddHHmmss"));
                        }
                    }
                }
            }
            if (myDCSColumnNameTable != null && myDCSIncrementTable != null)
            {
                for (int i = 0; i < myDCSIncrementTable.Rows.Count; i++)
                {
                    if (myDCSIncrementTable.Rows[i]["Flag"].ToString() == "Current" && myDCSIncrementTable.Rows[i]["vDate"] != DBNull.Value)
                    {
                        if (!m_DateTimeArray.Contains(((DateTime)myDCSIncrementTable.Rows[i]["vDate"]).ToString("yyyyMMddHHmmss")))
                        {
                            m_DateTimeArray.Add(((DateTime)myDCSIncrementTable.Rows[i]["vDate"]).ToString("yyyyMMddHHmmss"));
                        }
                    }
                }
            }
            m_DateTimeArray.Sort();
            for (int i = 0; i < m_DateTimeArray.Count; i++)
            {
                myDeviationDataTable.Columns.Add(m_DateTimeArray[i],typeof(decimal));
            }
            if (myAmmeterColumnNameTable != null && myAmmeterIncrementTable != null)
            {
                DataRow[] m_AvgValueRow = myAmmeterIncrementTable.Select("Flag = 'Avg' and vDate is null");
                if (m_AvgValueRow.Length > 0)
                {
                    for (int j = 2; j < myAmmeterIncrementTable.Columns.Count; j++)
                    {
                        DataRow[] m_AmmeterNameRow = GetTargetName(myAmmeterIncrementTable.Columns[j].ColumnName, myAmmeterColumnNameTable);
                        if (m_AmmeterNameRow.Length > 0)
                        {
                            DataRow m_NewRow = myDeviationDataTable.NewRow();      //一个字段对应的添加一行
                            m_NewRow["id"] = m_AmmeterNameRow[0]["id"].ToString();
                            m_NewRow["FieldName"] = m_AmmeterNameRow[0]["FieldName"].ToString();
                            m_NewRow["Address"] = m_AmmeterNameRow[0]["Address"].ToString();
                            m_NewRow["text"] = m_AmmeterNameRow[0]["text"].ToString();
                            m_NewRow["Type"] = "Ammeter";
                            m_NewRow["AvgValue"] = m_AvgValueRow[0][j];
                            for (int i = 0; i < myAmmeterIncrementTable.Rows.Count; i++)
                            {
                                if (myAmmeterIncrementTable.Rows[i]["vDate"] != DBNull.Value)
                                {
                                    string m_ColumnNameTemp = ((DateTime)myAmmeterIncrementTable.Rows[i]["vDate"]).ToString("yyyyMMddHHmmss");
                                    if ((decimal)myAmmeterIncrementTable.Rows[i][j] - (decimal)m_AvgValueRow[0][j] * decimal.Parse(myDeviationMagnification) > 0)
                                    {
                                        if (myDeviationDataTable.Columns.Contains(m_ColumnNameTemp))
                                        {
                                            m_NewRow[m_ColumnNameTemp] = myAmmeterIncrementTable.Rows[i][j];
                                        }
                                    }
                                }
                            }
                            myDeviationDataTable.Rows.Add(m_NewRow);
                        }
                    }
                }
            }
            if (myDCSColumnNameTable != null && myDCSIncrementTable != null)
            {
                DataRow[] m_AvgValueRow = myDCSIncrementTable.Select("Flag = 'Avg' and vDate is null");
                if (m_AvgValueRow.Length > 0)
                {
                    for (int j = 2; j < myDCSIncrementTable.Columns.Count; j++)
                    {
                        DataRow[] m_DCSNameRow = GetTargetName(myDCSIncrementTable.Columns[j].ColumnName, myDCSColumnNameTable);
                        if (m_DCSNameRow.Length > 0)
                        {
                            DataRow m_NewRow = myDeviationDataTable.NewRow();      //一个字段对应的添加一行
                            m_NewRow["id"] = m_DCSNameRow[0]["id"].ToString();
                            m_NewRow["FieldName"] = m_DCSNameRow[0]["FieldName"].ToString();
                            m_NewRow["Address"] = m_DCSNameRow[0]["Address"].ToString();
                            m_NewRow["text"] = m_DCSNameRow[0]["text"].ToString();
                            m_NewRow["Type"] = "DCS";
                            m_NewRow["AvgValue"] = m_AvgValueRow[0][j];
                            for (int i = 0; i < myDCSIncrementTable.Rows.Count; i++)
                            {
                                if (myDCSIncrementTable.Rows[i]["vDate"] != DBNull.Value)
                                {
                                    string m_ColumnNameTemp = ((DateTime)myDCSIncrementTable.Rows[i]["vDate"]).ToString("yyyyMMddHHmmss");
                                    if ((decimal)myDCSIncrementTable.Rows[i][j] - (decimal)m_AvgValueRow[0][j] * decimal.Parse(myDeviationMagnification) > 0)
                                    {
                                        if (myDeviationDataTable.Columns.Contains(m_ColumnNameTemp))
                                        {
                                            m_NewRow[m_ColumnNameTemp] = myDCSIncrementTable.Rows[i][j];
                                        }
                                    }
                                }
                            }
                            myDeviationDataTable.Rows.Add(m_NewRow);
                        }
                    }
                }
            }

            //去掉不用改变数据的标签
            int m_RowIndex = 0;
            while (m_RowIndex < myDeviationDataTable.Rows.Count)
            {
                bool m_IsAllRowNull = true;
                for (int j = DataColumnIndex; j < myDeviationDataTable.Columns.Count; j++)        //检验是否都是null
                {
                    if (myDeviationDataTable.Rows[m_RowIndex][j] != DBNull.Value)
                    {
                        m_IsAllRowNull = false;
                        break;
                    }
                }
                if (m_IsAllRowNull)
                {
                    myDeviationDataTable.Rows.RemoveAt(m_RowIndex);
                }
                else
                {
                    m_RowIndex = m_RowIndex + 1;
                }
            }
        }

        public static string GetColumnJsonString(DataTable myResultTable)
        {
            string m_ColumnsTemp = "";
            if (myResultTable.Columns.Count > DataColumnIndex)
            {
                for (int i = DataColumnIndex; i < myResultTable.Columns.Count; i++)
                {
                    if (i == DataColumnIndex)
                    {
                        m_ColumnsTemp = "\"" + myResultTable.Columns[i].ColumnName + "\"";
                    }
                    else
                    {
                        m_ColumnsTemp = m_ColumnsTemp + "," + "\"" + myResultTable.Columns[i].ColumnName + "\"";
                    }
                }
            }
            string m_ColumnsString = string.Format("\"columns\":[{0}]", m_ColumnsTemp);
            return m_ColumnsString;
        }
        private static DataRow[] GetTargetName(string myColumnName, DataTable myAmmeterColumnNameTable)
        {
            DataRow[] m_TargetName = myAmmeterColumnNameTable.Select(string.Format("FieldName = '{0}'", myColumnName));
            return m_TargetName;
        }
        public static DataTable GetTrend(string myOrganizationId, string myStartTime, string myEndTime, string myAmmeterFieldNames, string myDCSFieldNames)
        {
            string m_FactoryDataBase = GetFactoryDataBase(myOrganizationId);
            List<string> m_AmmeterFieldArray = new List<string>();
            List<string> m_AmmeterTextArray = new List<string>();
            List<string> m_DCSFieldArray = new List<string>();
            List<string> m_DCSTextArray = new List<string>();
            if (myAmmeterFieldNames != "")
            {
                string[] m_AmmeterFieldNames = myAmmeterFieldNames.Split(';');
                for (int i = 0; i < m_AmmeterFieldNames.Length; i++)
                {
                    m_AmmeterFieldArray.Add(m_AmmeterFieldNames[i].Split(',')[1]);
                    m_AmmeterTextArray.Add(m_AmmeterFieldNames[i].Split(',')[0]);
                }
            }
            if (myDCSFieldNames != "")
            {
                string[] m_DCSFieldNames = myDCSFieldNames.Split(';');
                for (int i = 0; i < m_DCSFieldNames.Length; i++)
                {
                    m_DCSFieldArray.Add(m_DCSFieldNames[i].Split(',')[1]);
                    m_DCSTextArray.Add(m_DCSFieldNames[i].Split(',')[0]);
                }
            }
            DataTable m_TrendSourceDataTable = GetTrendData(m_FactoryDataBase, m_AmmeterFieldArray.ToArray(), m_DCSFieldArray.ToArray(), myStartTime, myEndTime);
            
            DataTable m_TrendDataTable= GetTrendDataTable(m_TrendSourceDataTable);
            return m_TrendDataTable;
        }
        private static DataTable GetTrendData(string myDataBaseName, string[] myAmmeterFieldArray, string[] myDCSFieldArray, string myStartTime, string myEndTime)
        {
            string m_Sql = @"SELECT A.vDate {0} FROM {1}
                              where A.vDate >= '{2}'
                              and A.vDate <= '{3}'
                              {4}
                              order by A.vDate";
            string m_FieldCloumns = "";
            string m_DataTable = "";
            string m_ExtendCondition = "";
            if (myAmmeterFieldArray.Length > 0 && myDCSFieldArray.Length > 0)
            {
                m_DataTable = string.Format(" {0}.dbo.HistoryAmmeterIncrement A, {0}.dbo.HistoryDCSIncrement B", myDataBaseName);
                for (int i = 0; i < myAmmeterFieldArray.Length; i++)
                {
                    m_FieldCloumns = m_FieldCloumns + ",A." + myAmmeterFieldArray[i];
                }
                for (int i = 0; i < myDCSFieldArray.Length; i++)
                {
                    m_FieldCloumns = m_FieldCloumns + ",B." + myDCSFieldArray[i];
                }
                m_ExtendCondition = " and A.vDate = B.vDate";
            }
            else if (myAmmeterFieldArray.Length > 0)
            {
                m_DataTable = string.Format(" {0}.dbo.HistoryAmmeterIncrement A", myDataBaseName);
                for (int i = 0; i < myAmmeterFieldArray.Length; i++)
                {
                    m_FieldCloumns = m_FieldCloumns + ",A." + myAmmeterFieldArray[i];
                }
            }
            else if (myDCSFieldArray.Length > 0)
            {
                m_DataTable = string.Format("{0}.dbo.HistoryDCSIncrement A", myDataBaseName);
                for (int i = 0; i < myDCSFieldArray.Length; i++)
                {
                    m_FieldCloumns = m_FieldCloumns + ",A." + myDCSFieldArray[i];
                }
            }


            m_Sql = string.Format(m_Sql, m_FieldCloumns, m_DataTable, myStartTime, myEndTime, m_ExtendCondition);
            try
            {
                DataTable m_TrendDataTable = _dataFactory.Query(m_Sql);
                return m_TrendDataTable;
            }
            catch
            {
                return null;
            }
        }
        private static DataTable GetTrendDataTable(DataTable myTrendSourceDataTable)
        {
            DataTable m_TrendDataTable = new DataTable();
            m_TrendDataTable.Columns.Add("id", typeof(string));
            m_TrendDataTable.Columns.Add("text", typeof(string));
            if (myTrendSourceDataTable != null)
            {
                for (int i = 0; i < myTrendSourceDataTable.Rows.Count; i++)
                {
                    m_TrendDataTable.Columns.Add(((DateTime)myTrendSourceDataTable.Rows[i]["vDate"]).ToString("MM/dd/yyyy_HH:mm:ss"), typeof(decimal));
                }
                for (int j = 1; j < myTrendSourceDataTable.Columns.Count; j++)
                {
                    DataRow m_NewDataRow = m_TrendDataTable.NewRow();
                    m_NewDataRow["id"] = myTrendSourceDataTable.Columns[j].ColumnName;
                    m_NewDataRow["text"] = myTrendSourceDataTable.Columns[j].ColumnName;
                    for (int i = 0; i < myTrendSourceDataTable.Rows.Count; i++)
                    {
                        m_NewDataRow[((DateTime)myTrendSourceDataTable.Rows[i]["vDate"]).ToString("MM/dd/yyyy_HH:mm:ss")] = (decimal)myTrendSourceDataTable.Rows[i][j];
                    }
                    m_TrendDataTable.Rows.Add(m_NewDataRow);            //增加一行
                }
            }
            return m_TrendDataTable;
        }

        //////////////////////////////修正数据/////////////////////////////
        public static string CorrectionDataByTags(string myOrganizationId, string myStartTime, string myEndTime, string myAbnormalDataTime, string myTagsInfo)
        {
            int m_ObjectRowsCount = 0;
            List<string> m_AmmeterFieldArray = new List<string>();
            List<string> m_AmmeterCorrectionDataArray = new List<string>();
            List<string> m_DCSFieldArray = new List<string>();
            List<string> m_DCSCorrectionDataArray = new List<string>();
            if (myTagsInfo != "")
            {
                string[] m_TagItemList = myTagsInfo.Split(';');
                for (int i = 0; i < m_TagItemList.Length; i++)
                {
                    string[] m_TagDetail = m_TagItemList[i].Split(',');
                    if (m_TagDetail[1] == "Ammeter")
                    {
                        m_AmmeterFieldArray.Add(m_TagDetail[0]);
                        m_AmmeterCorrectionDataArray.Add(m_TagDetail[2]);
                    }
                    else if (m_TagDetail[1] == "DCS")
                    {
                        m_DCSFieldArray.Add(m_TagDetail[0]);
                        m_DCSCorrectionDataArray.Add(m_TagDetail[2]);
                    }
                }
                string m_FactoryDataBaseName = GetFactoryDataBase(myOrganizationId);
                DataTable m_AmmeterDataTable = GetSourceData(myStartTime, myEndTime, m_AmmeterFieldArray, m_FactoryDataBaseName + ".dbo.HistoryAmmeterIncrement");  //获得电表数据查询结果
                DataTable m_DCSDataTable = GetSourceData(myStartTime, myEndTime, m_DCSFieldArray, m_FactoryDataBaseName + ".dbo.HistoryDCSIncrement");              //获得DCS数据查询结果
                m_ObjectRowsCount = SetCorrectionData(m_AmmeterDataTable, m_AmmeterFieldArray, m_AmmeterCorrectionDataArray, myStartTime, myEndTime, myAbnormalDataTime, m_FactoryDataBaseName + ".dbo.HistoryAmmeterIncrement");              //获得需要插入/更新增量历史表的数据
                m_ObjectRowsCount = m_ObjectRowsCount + SetCorrectionData(m_DCSDataTable, m_DCSFieldArray, m_DCSCorrectionDataArray, myStartTime, myEndTime, myAbnormalDataTime, m_FactoryDataBaseName + ".dbo.HistoryDCSIncrement");              //获得需要插入/更新增量历史表的数据
                if (m_ObjectRowsCount > 0)
                {
                    m_ObjectRowsCount = m_ObjectRowsCount + UpdateHistoryFormulaValue(myOrganizationId, m_FactoryDataBaseName, myStartTime, myEndTime, m_AmmeterFieldArray, m_DCSFieldArray);       //刷新公式历史数据和公式设备历史数据
                }
            }
            if (m_ObjectRowsCount > 0)
            {
                return "1";
            }
            else if (m_ObjectRowsCount < 0)
            {
                return "-1";
            }
            else
            {
                return "0";
            }


        }
        private static DataTable GetSourceData(string myStartTime, string myEndTime, List<string> mySourceFieldArray, string myFactoryDataTableName)
        {
            if (mySourceFieldArray.Count > 0)
            {
                string m_Sql = @"SELECT A.vDate {0}
                              FROM {1} A
                              where A.vDate >= '{2}'
                              and A.vDate <= '{3}'
                              order by A.vDate";
                string m_Columns = "";
                for (int i = 0; i < mySourceFieldArray.Count; i++)
                {
                    m_Columns = m_Columns + "," + mySourceFieldArray[i];
                }
                m_Sql = string.Format(m_Sql, m_Columns, myFactoryDataTableName, myStartTime, myEndTime);
                try
                {
                    DataTable m_SourceDataTable = _dataFactory.Query(m_Sql);
                    return m_SourceDataTable;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private static int SetCorrectionData(DataTable mySourceDataTable, List<string> myCorrectionFieldArray, List<string> myCorrectionDataArray, string myStartTime, string myEndTime, string myAbnormalDataTime, string myFactoryDataTableName)
        {
            if (mySourceDataTable != null)
            {
                ///////////////////////找出缺少的行////////////////////
                string m_Sql = @"SELECT top 0 A.* FROM {0} A";
                m_Sql = string.Format(m_Sql, myFactoryDataTableName);
                try
                {
                    DataTable m_InsertDataTable = _dataFactory.Query(m_Sql);
                    DateTime m_StartTime = DateTime.Parse(myStartTime);
                    DateTime m_EndTime = DateTime.Parse(myEndTime);
                    for (int i = 0; i < mySourceDataTable.Rows.Count + 1; i++)
                    {
                        if (i == 0)
                        {
                            DateTime m_vDateTemp = (DateTime)mySourceDataTable.Rows[i]["vDate"];
                            double m_DiffSecond = (m_vDateTemp - m_StartTime).TotalSeconds;        //判断一共相差多少秒
                            if (m_DiffSecond > CollectionInterval * 2 * 60)    //差值大于10分钟的情况
                            {
                                int m_InsertCount = (int)(m_DiffSecond / (CollectionInterval * 60)) - 1;
                                for (int j = 1; j <= m_InsertCount; j++)
                                {
                                    DataRow m_NewRowInsert = m_InsertDataTable.NewRow();
                                    m_NewRowInsert["vDate"] = m_StartTime.AddMinutes(CollectionInterval * j);
                                    for (int w = 1; w < m_InsertDataTable.Columns.Count; w++)
                                    {
                                        m_NewRowInsert[w] = 0.0m;
                                    }
                                    m_InsertDataTable.Rows.Add(m_NewRowInsert);
                                }
                            }
                        }
                        if (i == mySourceDataTable.Rows.Count)
                        {
                            DateTime m_vDateTemp = (DateTime)mySourceDataTable.Rows[i - 1]["vDate"];
                            double m_DiffSecond = (m_EndTime - m_vDateTemp).TotalSeconds;        //判断一共相差多少秒
                            if (m_DiffSecond > CollectionInterval * 2 * 60)    //差值大于10分钟的情况
                            {
                                int m_InsertCount = (int)(m_DiffSecond / (CollectionInterval * 60)) - 1;
                                for (int j = 1; j <= m_InsertCount; j++)
                                {
                                    DataRow m_NewRowInsert = m_InsertDataTable.NewRow();
                                    m_NewRowInsert["vDate"] = m_vDateTemp.AddMinutes(CollectionInterval * j);
                                    for (int w = 1; w < m_InsertDataTable.Columns.Count; w++)
                                    {
                                        m_NewRowInsert[w] = 0.0m;
                                    }
                                    m_InsertDataTable.Rows.Add(m_NewRowInsert);
                                }
                            }
                        }
                        if (i > 0 && i < mySourceDataTable.Rows.Count)
                        {
                            DateTime m_vDateTemp1 = (DateTime)mySourceDataTable.Rows[i - 1]["vDate"];
                            DateTime m_vDateTemp2 = (DateTime)mySourceDataTable.Rows[i]["vDate"];
                            double m_DiffSecond = (m_vDateTemp2 - m_vDateTemp1).TotalSeconds;        //判断一共相差多少秒
                            if (m_DiffSecond > CollectionInterval * 2 * 60)    //差值大于10分钟的情况
                            {
                                int m_InsertCount = (int)(m_DiffSecond / (CollectionInterval * 60)) - 1;
                                for (int j = 1; j <= m_InsertCount; j++)
                                {
                                    DataRow m_NewRowInsert = m_InsertDataTable.NewRow();
                                    m_NewRowInsert["vDate"] = m_vDateTemp1.AddMinutes(CollectionInterval * j);
                                    for (int w = 1; w < m_InsertDataTable.Columns.Count; w++)
                                    {
                                        m_NewRowInsert[w] = 0.0m;
                                    }
                                    m_InsertDataTable.Rows.Add(m_NewRowInsert);
                                }
                            }
                        }

                    }
                    ///////////////////////////补全数据/////////////////////////
                    int m_TotalCorrectDataCount = mySourceDataTable.Rows.Count + m_InsertDataTable.Rows.Count;
                    if (m_TotalCorrectDataCount > 0)          //存在需要更新的行
                    {
                        if (myAbnormalDataTime != "")         //当均摊某列数据不包括在选定的时间内
                        {
                            DataRow[] m_ContainDateRow = mySourceDataTable.Select(string.Format("vDate = '{0}'", myAbnormalDataTime));
                            if (m_ContainDateRow.Length == 0)           //当前时间范围没有包括该行数据,总行数加1
                            {
                                m_TotalCorrectDataCount = m_TotalCorrectDataCount + 1;             //总行数加1

                                DataRow m_NewRowTemp = mySourceDataTable.NewRow();
                                m_NewRowTemp["vDate"] = DateTime.Parse(myAbnormalDataTime);
                                for (int i = 0; i < myCorrectionDataArray.Count; i++)
                                {
                                    //////////需要减去该列所有均摊给其它列的值
                                    m_NewRowTemp[myCorrectionFieldArray[i]] = decimal.Parse(myCorrectionDataArray[i]) - (decimal.Parse(myCorrectionDataArray[i]) / m_TotalCorrectDataCount) * (m_TotalCorrectDataCount - 1);
                                }
                                mySourceDataTable.Rows.Add(m_NewRowTemp);
                            }
                            else                                    //当前时间范围包括该行数据,则直接调整数据
                            {
                                for (int i = 0; i < myCorrectionDataArray.Count; i++)
                                {
                                    //////////需要减去该列所有均摊给其它列的值
                                    m_ContainDateRow[0][myCorrectionFieldArray[i]] = decimal.Parse(myCorrectionDataArray[i]) - (decimal.Parse(myCorrectionDataArray[i]) / m_TotalCorrectDataCount) * (m_TotalCorrectDataCount - 1);
                                }

                            }
                        }
                        for (int i = 0; i < m_InsertDataTable.Rows.Count; i++)
                        {
                            for (int j = 0; j < myCorrectionDataArray.Count; j++)
                            {
                                m_InsertDataTable.Rows[i][myCorrectionFieldArray[j]] = decimal.Parse(myCorrectionDataArray[j]) / m_TotalCorrectDataCount;
                            }
                        }
                        for (int i = 0; i < mySourceDataTable.Rows.Count; i++)
                        {
                            for (int j = 0; j < myCorrectionDataArray.Count; j++)
                            {
                                if (((DateTime)mySourceDataTable.Rows[i]["vDate"]).ToString("yyyy-MM-dd HH:mm:ss") != myAbnormalDataTime)
                                {
                                    mySourceDataTable.Rows[i][myCorrectionFieldArray[j]] = (decimal)mySourceDataTable.Rows[i][myCorrectionFieldArray[j]] + decimal.Parse(myCorrectionDataArray[j]) / m_TotalCorrectDataCount;
                                }
                            }
                        }
                    }
                    List<string> m_ExcludeColumns = new List<string>();
                    int m_UpdateRowsCount = 0;
                    ///////////////////////////////分批更新///////////////////////////
                    int m_InsertToDataBasePageRowCount = (int)(UpdateToDataBasePageItemCount / m_InsertDataTable.Columns.Count);
                    int m_InsertPageCount = (int)(m_InsertDataTable.Rows.Count / m_InsertToDataBasePageRowCount) + 1;
                    for (int i = 0; i < m_InsertPageCount; i++)         //分批进行更新
                    {
                        DataTable m_InsertTableTemp = m_InsertDataTable.Clone();
                        if (i == m_InsertPageCount - 1)   //最后一批数据
                        {
                            for (int j = 0; j < m_InsertDataTable.Rows.Count - i * m_InsertToDataBasePageRowCount; j++)
                            {
                                m_InsertTableTemp.Rows.Add(m_InsertDataTable.Rows[i * m_InsertToDataBasePageRowCount + j].ItemArray);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < m_InsertToDataBasePageRowCount; j++)
                            {
                                m_InsertTableTemp.Rows.Add(m_InsertDataTable.Rows[i * m_InsertToDataBasePageRowCount + j].ItemArray);
                            }
                        }
                        m_UpdateRowsCount = m_UpdateRowsCount + _dataFactory.Insert(myFactoryDataTableName, m_InsertTableTemp, new string[0]);
                    }

                    int m_UpdateToDataBasePageRowCount = (int)(UpdateToDataBasePageItemCount / mySourceDataTable.Columns.Count);
                    int m_UpdatePageCount = (int)(mySourceDataTable.Rows.Count / m_UpdateToDataBasePageRowCount) + 1;
                    for (int i = 0; i < m_UpdatePageCount; i++)
                    {
                        DataTable m_UpdateTableTemp = mySourceDataTable.Clone();
                        if (i == m_UpdatePageCount - 1)   //最后一批数据
                        {
                            for (int j = 0; j < mySourceDataTable.Rows.Count - i * m_UpdateToDataBasePageRowCount; j++)
                            {
                                m_UpdateTableTemp.Rows.Add(mySourceDataTable.Rows[i * m_UpdateToDataBasePageRowCount + j].ItemArray);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < m_UpdateToDataBasePageRowCount; j++)
                            {
                                m_UpdateTableTemp.Rows.Add(mySourceDataTable.Rows[i * m_UpdateToDataBasePageRowCount + j].ItemArray);
                            }
                        }
                        m_UpdateRowsCount = m_UpdateRowsCount + _dataFactory.Update(myFactoryDataTableName, m_UpdateTableTemp, new string[] { "vDate" });
                    }
                    return m_UpdateRowsCount;
                }
                catch
                {
                    return -1;
                }
            }
            else
            {
                return 0;
            }
        }
        private static int UpdateHistoryFormulaValue(string myOrganizationId, string myDataBaseName, string myStartTime, string myEndTime, List<string> myAmmeterFieldArray, List<string> myDCSFieldArray)
        {
            DataTable m_FormulaTable = GetFormulaInfo(myOrganizationId, myAmmeterFieldArray, myDCSFieldArray);   //获得需要改变值的公式
            int m_ReturntValue = ReCaculateHistoryFormulaValue(m_FormulaTable.Select("LevelType <> 'MainMachine'"), myDataBaseName, string.Format("{0}.dbo.HistoryFormulaValue", myDataBaseName), myStartTime, myEndTime);
            //m_ReturntValue = m_ReturntValue + ReCaculateHistoryFormulaValue(m_FormulaTable.Select("LevelType = 'MainMachine'"), myDataBaseName, string.Format("{0}.dbo.HistoryMainMachineFormulaValue", myDataBaseName), myStartTime, myEndTime);
            return m_ReturntValue;
        }
        private static DataTable GetFormulaInfo(string myOrganizationId, List<string> myAmmeterFieldArray, List<string> myDCSFieldArray)
        {
            string m_Sql = @"Select A.OrganizationID as OrganizationId
                                ,B.VariableId as VariableId
                                ,B.LevelCode as LevelCode
                                ,B.LevelType as LevelType
                                ,B.Formula as Formula
                                ,B.Denominator as Denominator
                                ,B.CoalDustConsumption as CoalDustConsumption
                                from tz_Formula A, formula_FormulaDetail B, system_Organization C, system_Organization D
                                where A.OrganizationID = C.OrganizationID
                                and A.ENABLE = 1
                                and A.State = 0
                                and A.KeyID = B.KeyID
                                and B.SaveToHistory = 1
                                and D.OrganizationID = '{0}'
                                and C.LevelCode like D.LevelCode + '%'
                                {1}
                                order by B.LevelCode";
            string m_DataTags = "";
            for (int i = 0; i < myAmmeterFieldArray.Count; i++)
            {
                if (m_DataTags == "")
                {
                    m_DataTags = string.Format(" and (B.Formula like '%{0}%' or B.Denominator like '%{0}%' or B.Denominator like '%{0}%' ", myAmmeterFieldArray[i].Replace("Energy", ""));
                }
                else
                {
                    m_DataTags = m_DataTags + string.Format(" or B.Formula like '%{0}%' or B.Denominator like '%{0}%' or B.Denominator like '%{0}%' ", myAmmeterFieldArray[i].Replace("Energy", ""));
                }
            }
            for (int i = 0; i < myDCSFieldArray.Count; i++)
            {
                if (m_DataTags == "")
                {
                    m_DataTags = string.Format(" and (B.Formula like '%{0}%' or B.Denominator like '%{0}%' or B.Denominator like '%{0}%' ", myDCSFieldArray[i]);
                }
                else
                {
                    m_DataTags = m_DataTags + string.Format(" or B.Formula like '%{0}%' or B.Denominator like '%{0}%' or B.Denominator like '%{0}%' ", myDCSFieldArray[i]);
                }
            }
            if (m_DataTags == "")
            {
                m_DataTags = "B.Formula <> B.Formula ";
            }
            else
            {
                m_DataTags = m_DataTags + ")";
            }
            m_Sql = string.Format(m_Sql, myOrganizationId, m_DataTags);
            try
            {
                DataTable m_FormulaInfoTable = _dataFactory.Query(m_Sql);
                return m_FormulaInfoTable;
            }
            catch
            {
                return null;
            }
        }
        private static int ReCaculateHistoryFormulaValue(DataRow[] myFormulaRows, string myDataBaseName, string myTableName, string myStartTime, string myEndTime)
        {
            if (myFormulaRows.Length > 0)
            {
                string m_Sql = @"delete from {0} where vDate >='{1}' and vDate <= '{2}' and ({3}) ";
                string m_DataConditions = "";
                for (int i = 0; i < myFormulaRows.Length; i++)
                {
                    if (i == 0)
                    {
                        m_DataConditions = string.Format("(OrganizationId = '{0}' and VariableId = '{1}')", myFormulaRows[i]["OrganizationId"].ToString(), myFormulaRows[i]["VariableId"].ToString());
                    }
                    else
                    {
                        m_DataConditions = m_DataConditions + string.Format(" or (OrganizationId = '{0}' and VariableId = '{1}')", myFormulaRows[i]["OrganizationId"].ToString(), myFormulaRows[i]["VariableId"].ToString());
                    }
                }
                m_Sql = string.Format(m_Sql, myTableName, myStartTime, myEndTime, m_DataConditions);
                try
                {
                    DataTable m_HistoryFormulaValueTable = GetReCaculateHistoryFormulaValue(myFormulaRows, myDataBaseName, myTableName, myStartTime, myEndTime);
                    _dataFactory.ExecuteSQL(m_Sql);
                    int m_InsertToDataBasePageRowCount = (int)(UpdateToDataBasePageItemCount / m_HistoryFormulaValueTable.Columns.Count);
                    int m_InsertPageCount = (int)(m_HistoryFormulaValueTable.Rows.Count / m_InsertToDataBasePageRowCount) + 1;
                    for (int i = 0; i < m_InsertPageCount; i++)         //分批进行更新
                    {
                        DataTable m_InsertTableTemp = m_HistoryFormulaValueTable.Clone();
                        if (i == m_InsertPageCount - 1)   //最后一批数据
                        {
                            for (int j = 0; j < m_HistoryFormulaValueTable.Rows.Count - i * m_InsertToDataBasePageRowCount; j++)
                            {
                                m_InsertTableTemp.Rows.Add(m_HistoryFormulaValueTable.Rows[i * m_InsertToDataBasePageRowCount + j].ItemArray);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < m_InsertToDataBasePageRowCount; j++)
                            {
                                m_InsertTableTemp.Rows.Add(m_HistoryFormulaValueTable.Rows[i * m_InsertToDataBasePageRowCount + j].ItemArray);
                            }
                        }
                        _dataFactory.Insert(myTableName, m_InsertTableTemp, new string[0]);         //更新公式数据
                    }

                    return 1;
                }
                catch
                {
                    return -99;
                }
            }
            else
            {
                return 0;
            }
        }
        private static DataTable GetReCaculateHistoryFormulaValue(DataRow[] myFormulaRows, string myDataBaseName, string myTableName, string myStartTime, string myEndTime)
        {
            DataTable m_HistoryFormulaValueTable = new DataTable();
            m_HistoryFormulaValueTable.Columns.Add("vDate",typeof(DateTime));
            m_HistoryFormulaValueTable.Columns.Add("OrganizationID",typeof(string));
            m_HistoryFormulaValueTable.Columns.Add("VariableID",typeof(string));
            m_HistoryFormulaValueTable.Columns.Add("LevelCode",typeof(string));
            m_HistoryFormulaValueTable.Columns.Add("FormulaValue",typeof(decimal));
            m_HistoryFormulaValueTable.Columns.Add("Power",typeof(decimal));
            m_HistoryFormulaValueTable.Columns.Add("DenominatorValue",typeof(decimal));
            m_HistoryFormulaValueTable.Columns.Add("CoalDustConsumption",typeof(decimal));
            m_HistoryFormulaValueTable.Columns.Add("CementTypes",typeof(string));

            DataTable m_TagDataValueTable = GetTagIncrementValue(myStartTime, myEndTime, myDataBaseName);
            DataTable m_TagPowerDataValueTable = GetTagPowerValue(myStartTime, myEndTime, myDataBaseName);
            FormulaParser m_FormulaParser = new FormulaParser();
            List<string[]> m_FormulaFactors = new List<string[]>();           //公式计算因子
            List<string[]> m_DenominatorFactors = new List<string[]>();       //分母计算因子
            List<string[]> m_CoalDustFactors = new List<string[]>();          //煤耗计算因子
            for (int i = 0; i < myFormulaRows.Length; i++)
            {
                if (myFormulaRows[i]["Formula"] != DBNull.Value && myFormulaRows[i]["Formula"].ToString() != "")
                {
                    string[] m_FormulaTemp = m_FormulaParser.GetAllFactorList(myFormulaRows[i]["Formula"].ToString()).ToArray();
                    m_FormulaFactors.Add(m_FormulaTemp);
                }
                else
                {
                    m_FormulaFactors.Add(new string[0]);
                }
                if (myFormulaRows[i]["Denominator"] != DBNull.Value && myFormulaRows[i]["Denominator"].ToString() != "")
                {
                    string[] m_DenominatorTemp = m_FormulaParser.GetAllFactorList(myFormulaRows[i]["Denominator"].ToString()).ToArray();
                    m_DenominatorFactors.Add(m_DenominatorTemp);
                }
                else
                {
                    m_DenominatorFactors.Add(new string[0]);
                }
                if (myFormulaRows[i]["CoalDustConsumption"] != DBNull.Value && myFormulaRows[i]["CoalDustConsumption"].ToString() != "")
                {
                    string[] m_CoalDustConsumptionTemp = m_FormulaParser.GetAllFactorList(myFormulaRows[i]["CoalDustConsumption"].ToString()).ToArray();
                    m_CoalDustFactors.Add(m_CoalDustConsumptionTemp);
                }
                else
                {
                    m_CoalDustFactors.Add(new string[0]);
                }
            }
            if (m_TagDataValueTable != null)
            {
                for (int i = 0; i < m_TagDataValueTable.Rows.Count; i++)
                {
                    for (int j = 0; j < myFormulaRows.Length; j++)
                    {
                        DataRow m_NewRowTemp = m_HistoryFormulaValueTable.NewRow();
                        m_NewRowTemp["vDate"] = (DateTime)m_TagDataValueTable.Rows[i]["vDate"];
                        m_NewRowTemp["OrganizationID"] = myFormulaRows[j]["OrganizationId"];
                        m_NewRowTemp["VariableID"] = myFormulaRows[j]["VariableId"];
                        m_NewRowTemp["LevelCode"] = myFormulaRows[j]["LevelCode"];
                        if (myFormulaRows[j]["Formula"] != DBNull.Value)
                        {
                            m_NewRowTemp["FormulaValue"] = CaculateFormulaValue(m_TagDataValueTable.Rows[i], myFormulaRows[j]["Formula"].ToString(), m_FormulaFactors[j]);

                            DataRow[] m_TagPowerRows = m_TagPowerDataValueTable.Select(string.Format("vDate = '{0}'", ((DateTime)m_NewRowTemp["vDate"]).ToString("yyyy-MM-dd HH:mm:ss")));
                            if (m_TagPowerRows.Length > 0)
                            {
                                m_NewRowTemp["Power"] = CaculateFormulaValue(m_TagPowerRows[0], myFormulaRows[j]["Formula"].ToString(), m_FormulaFactors[j]);
                            }
                            else
                            {
                                m_NewRowTemp["Power"] = 0.0m;
                            }
                        }
                        else
                        {
                            m_NewRowTemp["FormulaValue"] = DBNull.Value;
                            m_NewRowTemp["Power"] = DBNull.Value;
                        }
                        if (myFormulaRows[j]["Denominator"] != DBNull.Value)
                        {
                            m_NewRowTemp["DenominatorValue"] = CaculateFormulaValue(m_TagDataValueTable.Rows[i], myFormulaRows[j]["Denominator"].ToString(), m_DenominatorFactors[j]);
                        }
                        else
                        {
                            m_NewRowTemp["DenominatorValue"] = DBNull.Value;
                        }
                        if (myFormulaRows[j]["CoalDustConsumption"] != DBNull.Value)
                        {
                            m_NewRowTemp["CoalDustConsumption"] = CaculateFormulaValue(m_TagDataValueTable.Rows[i], myFormulaRows[j]["CoalDustConsumption"].ToString(), m_CoalDustFactors[j]);
                        }
                        else
                        {
                            m_NewRowTemp["CoalDustConsumption"] = DBNull.Value;
                        }
                        m_NewRowTemp["CementTypes"] = DBNull.Value;
                        m_HistoryFormulaValueTable.Rows.Add(m_NewRowTemp);
                    }
                }
            }
            return m_HistoryFormulaValueTable;
        }
        private static DataTable GetTagIncrementValue(string myStartTime, string myEndTime, string myDataBaseName)
        {
            string m_Sql = @"Select A.*, B.* 
                             from {0}.dbo.HistoryAmmeterIncrement A, {0}.dbo.HistoryDCSIncrement B 
                             where A.vDate = B.vDate
                             and A.vDate >= '{1}' 
                             and A.vDate <= '{2}'
                             order by A.vDate";
            m_Sql = string.Format(m_Sql, myDataBaseName, myStartTime, myEndTime);
            try
            {
                DataTable m_TagDataValueTable = _dataFactory.Query(m_Sql);
                if (m_TagDataValueTable != null)
                {
                    int m_Index = 1;
                    while (m_Index < m_TagDataValueTable.Columns.Count)
                    {
                        m_TagDataValueTable.Columns[m_Index].ColumnName = m_TagDataValueTable.Columns[m_Index].ColumnName.Replace("Energy", "");
                        if (m_TagDataValueTable.Columns[m_Index].ColumnName == "vDate")
                        {
                            m_TagDataValueTable.Columns.RemoveAt(m_Index);
                        }
                        else
                        {
                            m_Index = m_Index + 1;
                        }
                    }
                }
                return m_TagDataValueTable;
            }
            catch
            {
                return null;
            }
        }
        private static DataTable GetTagPowerValue(string myStartTime, string myEndTime, string myDataBaseName)
        {
            string m_Sql = @"Select A.* 
                             from {0}.dbo.HistoryAmmeter A
                             where A.vDate >= '{1}' 
                             and A.vDate <= '{2}'
                             order by A.vDate";
            m_Sql = string.Format(m_Sql, myDataBaseName, myStartTime, myEndTime);
            try
            {
                DataTable m_TagDataValueTable = _dataFactory.Query(m_Sql);
                if (m_TagDataValueTable != null)
                {
                    int m_Index = 1;
                    while(m_Index< m_TagDataValueTable.Columns.Count)
                    {
                        if (m_TagDataValueTable.Columns[m_Index].ColumnName.Contains("Energy"))  //删除电量数据
                        {
                            m_TagDataValueTable.Columns.RemoveAt(m_Index);
                        }
                        else
                        {
                            m_TagDataValueTable.Columns[m_Index].ColumnName = m_TagDataValueTable.Columns[m_Index].ColumnName.Replace("Power", "");
                            m_Index = m_Index + 1;
                        }
                    }
                }
                return m_TagDataValueTable;
            }
            catch
            {
                return null;
            }
        }
        private static decimal CaculateFormulaValue(DataRow myTagDataValueRow, string myFormulaString, string[] myCaculateFactors)
        {
            DataTable m_CaculateTable = new DataTable();
            decimal m_ReturnValue = 0.0m;
            string m_FormulaString = myFormulaString;
            for (int i = 0; i < myCaculateFactors.Length; i++)
            {
                if (myTagDataValueRow.Table.Columns.Contains(myCaculateFactors[i]))
                {
                    m_FormulaString = m_FormulaString.Replace(myCaculateFactors[i], myTagDataValueRow[myCaculateFactors[i]].ToString());
                }
                else
                {
                    break;
                }
                if (i == myCaculateFactors.Length - 1)
                {
                    try
                    {
                        m_ReturnValue = (decimal)m_CaculateTable.Compute(m_FormulaString,"");
                    }
                    catch
                    {

                    }
                }
            }
            return m_ReturnValue;
        }
        public static string ReGenerateDataByDay(string myOrganizationId, string myStartTime, string myEndTime)
        {
            List<DateTime> m_GenerateDayTimeList = new List<DateTime>();
            List<DateTime> m_GenerateMonthTimeList = new List<DateTime>();
            DateTime m_StartTime = DateTime.Parse(myStartTime.Substring(0, 10) + " 00:00:00.000");
            DateTime m_EndTimeTemp = DateTime.Parse(myEndTime.Substring(0, 10) + " 00:00:00.000");
            DateTime m_CurrentTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00.000"));
            DateTime m_EndTime;
            int m_CurentMonthValue = Int32.Parse(DateTime.Now.ToString("yyyyMM"));
            if (m_EndTimeTemp >= m_CurrentTime)      //汇总软件生成,必须小于今天
            {
                m_EndTime = m_CurrentTime;
            }
            else
            {
                m_EndTime = m_EndTimeTemp.AddDays(1);
            }

            while (m_StartTime < m_EndTime)
            {
                m_GenerateDayTimeList.Add(m_StartTime);
                m_StartTime = m_StartTime.AddDays(1);
            }
            for (int i = 0; i < m_GenerateDayTimeList.Count; i++)       //解决跨月的情况,删除月数据
            {
                int m_MonthValue = Int32.Parse(m_GenerateDayTimeList[i].ToString("yyyyMM"));
                if (m_MonthValue < m_CurentMonthValue)
                {
                    if (!m_GenerateMonthTimeList.Contains(DateTime.Parse(m_GenerateDayTimeList[i].ToString("yyyy-MM-01 00:00:00.000"))))
                    {
                        m_GenerateMonthTimeList.Add(DateTime.Parse(m_GenerateDayTimeList[i].ToString("yyyy-MM-01 00:00:00.000")));
                    }
                }
            }
            ////////////////////////////以下是重新生成汇总数据
            try
            {
                
                DeleteBalanceData(myOrganizationId, myStartTime, myEndTime);
                SingleBasicData singleBasicData = SingleBasicData.Creat();
                for (int i = 0; i < m_GenerateDayTimeList.Count; i++)         //生成日数据
                {
                    Balance.Model.BalanceService.SaveDayBalanceDetailData(myOrganizationId, m_GenerateDayTimeList[i]);
                }
                for (int i = 0; i < m_GenerateMonthTimeList.Count; i++)       //生成月数据
                {
                    MonthlyBalanceService.SaveMonthBalanceDetailData(myOrganizationId, m_GenerateMonthTimeList[i]);
                }
                return "1";
            }
            catch
            {
                return "-1";
            }
        }
        private static void DeleteBalanceData(string myOrganizationId, string myStartTime, string myEndTime)
        {
            string m_Sql = @"delete from balance_Energy where KeyId in (Select A.BalanceId from tz_Balance A where A.OrganizationID = '{0}' and A.TimeStamp >= '{1}' and A.TimeStamp <= '{2}' and A.StaticsCycle = 'day');
                                delete from balance_Energy where KeyId in (Select A.BalanceId from tz_Balance A where A.OrganizationID = '{0}' and A.TimeStamp >= '{3}' and A.TimeStamp <= '{4}' and A.StaticsCycle = 'month');
                                delete from balance_Production where KeyId in (Select A.BalanceId from tz_Balance A where A.OrganizationID = '{0}' and A.TimeStamp >= '{1}' and A.TimeStamp <= '{2}' and A.StaticsCycle = 'day');
                                delete from balance_Production where KeyId in (Select A.BalanceId from tz_Balance A where A.OrganizationID = '{0}' and A.TimeStamp >= '{3}' and A.TimeStamp <= '{4}' and A.StaticsCycle = 'month');
                                delete from tz_Balance where OrganizationID = '{0}' and TimeStamp >= '{1}' and TimeStamp <= '{2}' and StaticsCycle = 'day';
                                delete from tz_Balance where OrganizationID = '{0}' and TimeStamp >= '{3}' and TimeStamp <= '{4}' and StaticsCycle = 'month'";
            string m_StartTimeDay = DateTime.Parse(myStartTime).ToString("yyyy-MM-dd");
            string m_EndTimeDay = DateTime.Parse(myEndTime).ToString("yyyy-MM-dd");
            string m_StartTimeMonth = DateTime.Parse(myStartTime).ToString("yyyy-MM");
            string m_EndTimeMonth = DateTime.Parse(myEndTime).ToString("yyyy-MM");
            m_Sql = string.Format(m_Sql, myOrganizationId, m_StartTimeDay, m_EndTimeDay, m_StartTimeMonth, m_EndTimeMonth);
            _dataFactory.ExecuteSQL(m_Sql);
        }
    }
}