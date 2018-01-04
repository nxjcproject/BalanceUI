using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using BalanceUI.Infrastruture.Configuration;
using SqlServerDataAdapter;

namespace BalanceUI.Service
{
    public class BalanceMachineHaltService
    {
        private static readonly string _connStr = ConnectionStringFactory.NXJCConnectionString;
        private static readonly ISqlServerDataFactory _dataFactory = new SqlServerDataFactory(_connStr);
        /// <summary>
        /// 获取最后一次平衡时间
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public static DataTable GetMachineHaltLog(string myOrganizationId, string myStartHaltTime, string myEndHaltTime, string myEquipmentId)
        {
            string m_Sql = @"SELECT  A.MachineHaltLogID as MachineHaltLogId
                                  ,A.OrganizationID as OrganizationId
	                              ,B.Name as Name
                                  ,A.EquipmentID as EquipmentId
                                  ,A.WorkingTeamShiftLogID as WorkingTeamShiftLogId
	                              ,D.WorkingTeam as WorkingTeam
                                  ,A.Label as Label
                                  ,A.EquipmentName as EquipmentName
                                  ,A.StartTime as StartTime
                                  ,A.HaltTime as HaltTime
                                  ,A.RecoverTime as RecoverTime
                                  ,A.ReasonID as ReasonId
                                  ,A.ReasonText as ReasonText
                                  ,A.Remarks as Remarks
                              FROM shift_MachineHaltLog A
                              left join shift_WorkingTeamShiftLog D on A.WorkingTeamShiftLogID = D.WorkingTeamShiftLogID
                              , system_Organization B, system_Organization C 
                              where C.OrganizationID = '{0}'
                              and B.LevelCode like C.LevelCode + '%'
                              and A.OrganizationID = B.OrganizationID
                              and ((A.HaltTime >= '{1} 00:00:00' and A.HaltTime <= '{2} 23:59:59')
                                  or (A.StartTime >= '{1} 00:00:00' and A.StartTime <= '{2} 23:59:59'))
                              {3}
                              order by A.EquipmentID, A.StartTime desc";
            string m_Condition = "";
            if (myEquipmentId != "")
            {
                m_Condition = string.Format(" and A.EquipmentID = '{0}' ", myEquipmentId);
            }
            m_Sql = string.Format(m_Sql, myOrganizationId, myStartHaltTime, myEndHaltTime, m_Condition);
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
        public static DataTable GetMachineHaltLogbyId(string myMachineHaltLogId)
        {
            string m_Sql = @"SELECT  A.MachineHaltLogID as MachineHaltLogId
                                  ,A.OrganizationID as OrganizationId
                                  ,B.Name as Name
                                  ,A.EquipmentID as EquipmentId
                                  ,A.WorkingTeamShiftLogID as WorkingTeamShiftLogId
                                  ,A.Label as Label
                                  ,A.EquipmentName as EquipmentName
                                  ,A.StartTime as StartTime
                                  ,A.HaltTime as HaltTime
                                  ,A.RecoverTime as RecoverTime
                                  ,A.ReasonID as ReasonId
                                  ,A.ReasonText as ReasonText
                                  ,A.Remarks as Remarks
                              FROM shift_MachineHaltLog A, system_Organization B
                              where A.MachineHaltLogID = '{0}'
                              and A.OrganizationID = B.OrganizationID";
            m_Sql = string.Format(m_Sql, myMachineHaltLogId);
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
        public static DataTable GetMasterMachineInfo(string myOrganizationId)
        {
            //,B.Name + A.VariableDescription as Text
            string m_Sql = @"SELECT A.ID as Id
                                ,A.OrganizationID as OrganizationId
                                ,A.VariableDescription as Text
                            FROM system_MasterMachineDescription A, system_Organization B, system_Organization C
                            where C.OrganizationID = '{0}'
                            and B.LevelCode like C.LevelCode + '%'
                            and A.OrganizationID = B.OrganizationID
                            order by A.VariableDescription";
            m_Sql = string.Format(m_Sql, myOrganizationId);
            try
            {
                DataTable m_MasterMachineTable = _dataFactory.Query(m_Sql);
                return m_MasterMachineTable;
            }
            catch
            {
                return null;   //"{\"rows\":[],\"total\":0}";
            }
        }
        /// <summary>
        /// 获取所有的停机原因
        /// </summary>
        /// <returns></returns>
        public static DataTable GetMachineHaltReasons()
        {
            Query query = new Query("system_MachineHaltReason");
            query.AddOrderByClause(new SqlServerDataAdapter.Infrastruction.OrderByClause("MachineHaltReasonID", false));
            return _dataFactory.Query(query);
        }
        public static DataTable GetMachineHaltLogbyDate(string myDate, string myOrganizationId)
        {
            string m_Sql = @"SELECT A.WorkingTeamShiftLogID as Id
                                  ,A.WorkingTeam + '(' + CONVERT(varchar(32), A.ShiftDate, 120) + ')' as Text
                              FROM shift_WorkingTeamShiftLog A
                              where  CONVERT(varchar(10), A.ShiftDate, 23) = '{0}'
                              and OrganizationID = '{1}'
                              order by ShiftDate";
            m_Sql = string.Format(m_Sql, myDate, myOrganizationId);
            try
            {
                DataTable m_MachineHaltLogTable = _dataFactory.Query(m_Sql);
                return m_MachineHaltLogTable;
            }
            catch
            {
                return null;
            }
        }
        public static int RemoveMachineHaltLogById(string myMachineHaltLogId)
        {
            string m_Sql = @"delete FROM shift_MachineHaltLog where MachineHaltLogID = '{0}'";
            m_Sql = string.Format(m_Sql, myMachineHaltLogId);
            try
            {
                int m_RowCount = _dataFactory.ExecuteSQL(m_Sql);
                return m_RowCount;
            }
            catch
            {
                return -1;
            }
        }
        public static int AddMachineHalt(string myEquipmentId, string myOrganizationId, string myMachineStartTime, string myMachineHaltTime, string myMachineRecoverTime, string myWorkingTeamShiftLogId, string myMachineHaltReason, string myRemark)
        {
            string m_Sql = @"insert into shift_MachineHaltLog 
                    (MachineHaltLogID,OrganizationID,EquipmentID,WorkingTeamShiftLogID,Label,EquipmentName,StartTime,HaltTime,RecoverTime,ReasonID,ReasonText,Remarks)
                    (select '{0}',A.OrganizationID,'{1}',{2},A.VariableName as Label,A.VariableDescription as EquipmentName,'{3}','{4}','{5}',{6},B.ReasonText as ReasonText,'{7}'
                    from system_MasterMachineDescription A
                    left join system_MachineHaltReason B on B.MachineHaltReasonID = rtrim({6}) 
                    where A.ID = '{1}')";
            try
            {
                string m_MachineHaltLogID = Guid.NewGuid().ToString();

                m_Sql = m_Sql.Replace("{0}", m_MachineHaltLogID);
                m_Sql = m_Sql.Replace("{1}", myEquipmentId);
                if (myWorkingTeamShiftLogId != "")
                {
                    m_Sql = m_Sql.Replace("{2}", "'" + myWorkingTeamShiftLogId + "'");
                }
                else
                {
                    m_Sql = m_Sql.Replace("{2}", "NULL");
                }
                m_Sql = m_Sql.Replace("{3}", myMachineStartTime);
                m_Sql = m_Sql.Replace("{4}", myMachineHaltTime);
                m_Sql = m_Sql.Replace("{5}", myMachineRecoverTime);
                if (myMachineHaltReason != "")
                {
                    m_Sql = m_Sql.Replace("{6}", "'" + myMachineHaltReason + "'");
                }
                else
                {
                    m_Sql = m_Sql.Replace("{6}", "NULL");
                }
                m_Sql = m_Sql.Replace("{7}", myRemark);
                int m_RowCount = _dataFactory.ExecuteSQL(m_Sql);
                return m_RowCount;
            }
            catch
            {
                return -1;
            }
        }
        public static int ModifyMachineHalt(string myMachineHaltLogId, string myMachineStartTime, string myMachineHaltTime, string myMachineRecoverTime, string myWorkingTeamShiftLogId, string myMachineHaltReason, string myMachineHaltReasonText, string myRemark)
        {
            string m_Sql = @"update shift_MachineHaltLog set
                    WorkingTeamShiftLogID = {1},StartTime = '{2}',HaltTime = '{3}',RecoverTime = '{4}',ReasonID = {5}, ReasonText = {6}, Remarks = '{7}'
                    where MachineHaltLogID = '{0}'";
            try
            {
                m_Sql = m_Sql.Replace("{0}", myMachineHaltLogId);
                if (myWorkingTeamShiftLogId != "")
                {
                    m_Sql = m_Sql.Replace("{1}", "'" + myWorkingTeamShiftLogId + "'");
                }
                else
                {
                    m_Sql = m_Sql.Replace("{1}", "NULL");
                }
                m_Sql = m_Sql.Replace("{2}", myMachineStartTime);
                m_Sql = m_Sql.Replace("{3}", myMachineHaltTime);
                m_Sql = m_Sql.Replace("{4}", myMachineRecoverTime);
                if (myMachineHaltReason != "")
                {
                    m_Sql = m_Sql.Replace("{5}", "'" + myMachineHaltReason + "'");
                }
                else
                {
                    m_Sql = m_Sql.Replace("{5}", "NULL");
                }
                if (myMachineHaltReasonText != "")
                {
                    m_Sql = m_Sql.Replace("{6}", "'" + myMachineHaltReasonText + "'");
                }
                else
                {
                    m_Sql = m_Sql.Replace("{6}", "NULL");
                }
                m_Sql = m_Sql.Replace("{7}", myRemark);
                int m_RowCount = _dataFactory.ExecuteSQL(m_Sql);
                return m_RowCount;
            }
            catch
            {
                return -1;
            }
        }
    }
}
