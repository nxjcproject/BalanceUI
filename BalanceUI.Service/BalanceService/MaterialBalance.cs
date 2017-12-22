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
    public class MaterialBalance
    {
        public static DataTable GetData(string OrganizationId, string startTime, string endTime)
        {
            //DataTable materialInformation = GetMaterialInformation(OrganizationId);//获取物料信息
            DataTable processInformaion = GetProcessInfomation(OrganizationId);//获取工序名称
            DataTable materialInformationData = GetmaterialInformationData(OrganizationId, processInformaion, startTime, endTime);//获取物料信息数据
            DataTable formulaDatable = GetFormula(materialInformationData);
            DataTable resultDatable = GetRatio(formulaDatable);
            return resultDatable;
        }
        #region
        //        private static DataTable GetMaterialInformation(string OrganizationId)//获取物料信息
        //        {
        //            string connectionString = ConnectionStringFactory.NXJCConnectionString;
        //            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
        //            string mySql = @"select 
        //       A.Id
        //      ,A.Name
        //      ,A.MaterialId
        //      ,A.levelcode
        //      ,B.[WarehouseId]
        //      ,B.[VariableId]
        //      ,B.[Specs]
        //      ,B.[DataBaseName]
        //      ,B.[DataTableName]
        //      ,B.[WarehousingType]
        //      ,C.[Process]
        //      ,C.[MaterialFormula]
        //      ,C.[MaterialFormulaProcess]
        //      ,C.[DisplayIndex],C.[VariableType] from [NXJC].[dbo].[inventory_Warehouse] as A,[NXJC].[dbo].[inventory_WareHousingContrast] as B,[NXJC].[dbo].[report_MaterialBalanceReport] as C
        //                  where A.Id=B.warehouseid and A.[OrganizationID]=@OrganizationID and C.keyid=B.[ItemId] order by DisplayIndex, process,WarehouseId";
        //            SqlParameter myParameter = new SqlParameter("OrganizationID", OrganizationId);
        //            DataTable table = dataFactory.Query(mySql, myParameter);
        //            return table;
        //        }
        #endregion
        private static DataTable GetmaterialInformationData(string OrganizationId, DataTable processInformaion, string startTime, string endTime)//获取物料信息数据
        {
            string process = "";//工序
            string WarehouseID = "";//厂库ID
            string WarehouseName = "";//厂库名字
            string VariableType = "";//用于判断是否为分品种
            Decimal[] processInputData = new Decimal[3];
            Decimal[] processOutputData = new Decimal[3];
            Decimal[] inputData = new Decimal[3];
            Decimal[] outputData = new Decimal[3];
            DataTable getBenchmarksInformationTable = new DataTable();//基准库存
            DataTable getInputWarehouseTable = new DataTable();//入库信息
            DataTable getOutputWarehouseTable = new DataTable();//出库信息
            DataTable getProcessInputWarehouseTable = new DataTable();//分品种入库信息
            DataTable ProcessInputCementTable = new DataTable();//分品种水泥子库信息
            DataTable getProcessOutputWarehouseTable = new DataTable();//分出库信息
            Decimal inputQuantity = 0;//入库量
            Decimal outputQuantity = 0;//出库量
            Decimal processInputQuantity = 0;//分工序入库量
            Decimal processOutputQuantity = 0;//分工序出库量
            string inputFactory = "/";//入厂量
            Decimal inputTotal = 0;//分品种入厂总和
            Decimal outputTotal = 0;//分品种出厂总和
            string outputFactory = "/";//出厂量
            string Production = "/";//生产量
            string Consumption = "/";//消耗量
            Decimal currentInventory = 0;//当前库存
            Decimal benchmarksValue = 0;//盘库值
            string benchmarksTime = "";//盘库时间
            string WarehouseID_process = "";//计算分品种消耗量时用
            string WarehouseName_process = "";//计算分品种消耗量时用
            string process_process = "";//计算分品种消耗量时用
            //Decimal Consumptionsigle = 0;
            DataTable resultDatatable = new DataTable();
            //string process_endTime = "";
            string inputWaterData = "";//计算水分时用
            string outputWaterData = "";//计算水分时用
            string inputWaterNumber = null;//消耗水分值
            string outputWaterNumber = null;//产量水分值
            string dryBasis = null;//干基
            string databaseName = GetDataBaseName(OrganizationId);
            resultDatatable.Columns.Add("Process", Type.GetType("System.String"));
            resultDatatable.Columns.Add("MaterialName", Type.GetType("System.String"));
            resultDatatable.Columns.Add("Earlyinventory", Type.GetType("System.String"));
            resultDatatable.Columns.Add("Input", Type.GetType("System.String"));
            resultDatatable.Columns.Add("Moisture", Type.GetType("System.String"));
            resultDatatable.Columns.Add("Output", Type.GetType("System.String"));
            resultDatatable.Columns.Add("DynamicInventory", Type.GetType("System.String"));
            resultDatatable.Columns.Add("Ratio", Type.GetType("System.String"));
            resultDatatable.Columns.Add("WetBase", Type.GetType("System.String"));
            resultDatatable.Columns.Add("DryBasis", Type.GetType("System.String"));
            resultDatatable.Columns.Add("Formula", Type.GetType("System.String"));
            resultDatatable.Columns.Add("Yield", Type.GetType("System.String"));
            resultDatatable.Columns.Add("ProductMoisture", Type.GetType("System.String"));
            DataTable waterInformation = GetWaterInformation(OrganizationId);
            Dictionary<string, string> dlc = GetWaterData(waterInformation, OrganizationId, startTime, endTime);
            foreach (DataRow dr in processInformaion.Rows)
            {
                DataRow m_dr = resultDatatable.NewRow();
                WarehouseID = dr["WarehouseId"].ToString();
                WarehouseName = dr["Name"].ToString();
                process = dr["Process"].ToString();
                VariableType = dr["VariableType"].ToString();
                if (VariableType == "") //正常工序时
                {
                    getBenchmarksInformationTable = GetBenchmarksInformation(startTime, WarehouseID);
                    try
                    {
                        benchmarksValue = Math.Round(Convert.ToDecimal(getBenchmarksInformationTable.Rows[0][0]), 2);
                    }
                    catch
                    {
                        m_dr["Process"] = process;
                        m_dr["MaterialName"] = WarehouseName.Remove(WarehouseName.Length - 1, 1);
                        m_dr["Earlyinventory"] = "/";
                        m_dr["Input"] = "/";
                        m_dr["Moisture"] = null;
                        m_dr["Output"] = "/";
                        m_dr["DynamicInventory"] = "/";
                        m_dr["Ratio"] = null;
                        m_dr["WetBase"] = "/";
                        m_dr["DryBasis"] = null;
                        m_dr["Yield"] = "/";
                        m_dr["ProductMoisture"] = null;
                        resultDatatable.Rows.Add(m_dr);
                        Array.Clear(inputData, 0, inputData.Length);
                        Array.Clear(outputData, 0, outputData.Length);
                        Array.Clear(processInputData, 0, processInputData.Length);
                        Array.Clear(processOutputData, 0, processOutputData.Length);
                        inputFactory = "/";
                        outputFactory = "/";
                        continue;
                    }
                    benchmarksTime = getBenchmarksInformationTable.Rows[0][1].ToString();
                    getInputWarehouseTable = GetInputWarehouse(WarehouseID);//获取入库信息
                    getOutputWarehouseTable = GetOutputWarehouse(WarehouseID);//出库信息
                    getProcessInputWarehouseTable = GetProcessInputWarehouse(WarehouseID, process);//获取分工序入库信息
                    inputData = GetInputQuantity(endTime, benchmarksTime, OrganizationId, getInputWarehouseTable, databaseName);//入库数据
                    inputQuantity = Math.Round(inputData[0], 2);//入库量
                    inputTotal = Math.Round(inputData[1], 2);//分品种入厂总和
                    getProcessOutputWarehouseTable = GetProcessOutputWarehouse(WarehouseID, process);//获取分工序出库信息
                    outputData = GetInputQuantity(endTime, benchmarksTime, OrganizationId, getOutputWarehouseTable, databaseName);//出库数据
                    outputQuantity = Math.Round(outputData[0], 2);//出库量
                    outputTotal = Math.Round(outputData[1], 2);//分品种出厂总和
                    currentInventory = benchmarksValue + inputQuantity - outputQuantity;//当前库存
                    currentInventory = Math.Round(currentInventory, 2);
                    processInputData = GetInputQuantity(endTime, startTime, OrganizationId, getProcessInputWarehouseTable, databaseName);//分工序入库数据
                    processInputQuantity = Math.Round(processInputData[0], 2);//分工序入库量
                    if (processInputData[3] != 0)
                    {
                        inputFactory = Convert.ToDecimal(processInputData[1]).ToString("0.00");//入厂量
                    }
                    else
                    {
                        inputFactory = "/";
                    }
                    if (WarehouseID == "085050D1-AB01-4FE2-9593-37A34902DF97")
                    {
                        if (processInputData[4] != 0)
                        {
                            Production = Convert.ToDecimal(processInputData[2] * ((100 - (Convert.ToDecimal(dlc["燃料原煤库Input"]) - Convert.ToDecimal(dlc["燃料煤粉仓Output"]))) / 100)).ToString("0.00");
                        }
                        else
                        {
                            Production = "/";
                        }

                    }
                    else
                    {
                        if (processInputData[4] != 0)
                        {
                            Production = Convert.ToDecimal(processInputData[2]).ToString("0.00");
                        }
                        else
                        {
                            Production = "/";
                        }
                    }
                    processOutputData = GetInputQuantity(endTime, startTime, OrganizationId, getProcessOutputWarehouseTable, databaseName);//分工序出库数据
                    processOutputQuantity = Math.Round(processOutputData[0], 2);//分工序出库量
                    if (processOutputData[3] != 0)
                    {
                        outputFactory = Convert.ToDecimal(processOutputData[1]).ToString("0.00");//出厂量
                    }
                    else
                    {
                        outputFactory = "/";
                    }
                    if (processOutputData[4] != 0)
                    {
                        Consumption = Convert.ToDecimal(processOutputData[2]).ToString("0.00");
                    }
                    else
                    {
                        Consumption = "/";
                    }
                    inputWaterData = process + WarehouseName + "Input";
                    try
                    {
                        inputWaterNumber = Convert.ToDecimal(dlc[inputWaterData]).ToString("0.00");
                        dryBasis = (Convert.ToDecimal(Consumption) * (1 - Convert.ToDecimal(dlc[inputWaterData]) / 100)).ToString("#0.00");
                    }
                    catch
                    {
                        inputWaterNumber = null;
                        dryBasis = null;
                    }
                    outputWaterData = process + WarehouseName + "Output";
                    try
                    {
                        outputWaterNumber = Convert.ToDecimal(dlc[outputWaterData]).ToString("#0.00");
                    }
                    catch
                    {
                        outputWaterNumber = null;
                    }
                    if (WarehouseName == "石灰石库(水泥)" || WarehouseName == "石灰石库(生料)")
                    {
                        m_dr["MaterialName"] = WarehouseName.Remove(WarehouseName.Length - 5, 5);
                    }
                    else
                    {
                        m_dr["MaterialName"] = WarehouseName.Remove(WarehouseName.Length - 1, 1);

                    }
                    m_dr["Process"] = process;
                    m_dr["Earlyinventory"] = benchmarksValue.ToString();
                    m_dr["Input"] = inputFactory;
                    m_dr["Moisture"] = inputWaterNumber;
                    m_dr["Output"] = outputFactory;
                    m_dr["DynamicInventory"] = currentInventory.ToString();
                    m_dr["Ratio"] = null;
                    m_dr["WetBase"] = Consumption;
                    m_dr["DryBasis"] = dryBasis;
                    m_dr["Yield"] = Production;
                    m_dr["ProductMoisture"] = outputWaterNumber;
                    Array.Clear(inputData, 0, inputData.Length);
                    Array.Clear(outputData, 0, outputData.Length);
                    Array.Clear(processInputData, 0, processInputData.Length);
                    Array.Clear(processOutputData, 0, processOutputData.Length);

                }
                else//当为分品种工序时
                {
                    if (process == WarehouseName)//当为分品种工序水泥库时
                    {
                        getBenchmarksInformationTable = GetBenchmarksInformation(startTime, WarehouseID);
                        try
                        {
                            benchmarksValue = Math.Round(Convert.ToDecimal(getBenchmarksInformationTable.Rows[0][0]), 2);
                        }
                        catch
                        {
                            m_dr["Process"] = process;
                            m_dr["MaterialName"] = WarehouseName.Remove(WarehouseName.Length - 1, 1);
                            m_dr["Earlyinventory"] = "/";
                            m_dr["Input"] = "/";
                            m_dr["Moisture"] = null;
                            m_dr["Output"] = "/";
                            m_dr["DynamicInventory"] = "/";
                            m_dr["Ratio"] = null;
                            m_dr["WetBase"] = "/";
                            m_dr["DryBasis"] = null;
                            m_dr["Yield"] = "/";
                            m_dr["ProductMoisture"] = null;
                            resultDatatable.Rows.Add(m_dr);
                            Array.Clear(inputData, 0, inputData.Length);
                            Array.Clear(outputData, 0, outputData.Length);
                            Array.Clear(processInputData, 0, processInputData.Length);
                            Array.Clear(processOutputData, 0, processOutputData.Length);
                            inputFactory = "/";
                            outputFactory = "/";
                            continue;
                        }
                        benchmarksTime = getBenchmarksInformationTable.Rows[0][1].ToString();
                        getInputWarehouseTable = GetInputWarehouse(WarehouseID);//获取入库信息
                        getProcessInputWarehouseTable = GetProcessInputWarehouse(WarehouseID, process);//获取分工序入库信息
                        inputQuantity = Math.Round(GetInputQuantity(endTime, benchmarksTime, OrganizationId, getInputWarehouseTable, databaseName)[0], 2);//入库量
                        inputFactory = "/";//入厂量
                        getOutputWarehouseTable = GetOutputWarehouse(WarehouseID);//出库信息
                        getProcessOutputWarehouseTable = GetProcessOutputWarehouse(WarehouseID, process);//获取分工序出库信息
                        outputQuantity = Math.Round(GetInputQuantity(endTime, benchmarksTime, OrganizationId, getOutputWarehouseTable, databaseName)[0], 2);//出库量
                        outputFactory = Convert.ToDecimal(GetInputQuantity(endTime, startTime, OrganizationId, getProcessOutputWarehouseTable, databaseName)[1]).ToString("0.00");//出厂量
                        currentInventory = benchmarksValue + inputQuantity - outputQuantity;//当前库存
                        currentInventory = Math.Round(currentInventory, 2);
                        ProcessInputCementTable = GetProcessInputInformation(endTime, startTime, OrganizationId, WarehouseName);//分工序水泥子库信息
                        Production = Convert.ToDecimal(GetProcessQuantity(ProcessInputCementTable, startTime, endTime)).ToString("0.00");//生产量
                        Consumption = "/";//消耗量
                        inputWaterData = process + WarehouseName + "Input";
                        try
                        {
                            inputWaterNumber = Convert.ToDecimal(dlc[inputWaterData]).ToString("0.00");
                        }
                        catch
                        {
                            inputWaterNumber = null;
                        }
                        outputWaterData = process + WarehouseName + "Output";
                        try
                        {
                            outputWaterNumber = Convert.ToDecimal(dlc[outputWaterData]).ToString("#0.00");
                        }
                        catch
                        {
                            outputWaterNumber = null;
                        }

                        m_dr["Process"] = process;
                        m_dr["MaterialName"] = "水泥";
                        m_dr["Earlyinventory"] = benchmarksValue.ToString();
                        m_dr["Input"] = inputFactory;
                        m_dr["Moisture"] = inputWaterNumber;
                        m_dr["Output"] = outputFactory;
                        m_dr["DynamicInventory"] = currentInventory.ToString();
                        m_dr["Ratio"] = null;
                        m_dr["WetBase"] = Consumption;
                        m_dr["DryBasis"] = null;
                        m_dr["Yield"] = Production;
                        m_dr["ProductMoisture"] = outputWaterNumber;
                        Array.Clear(inputData, 0, inputData.Length);
                        Array.Clear(outputData, 0, outputData.Length);
                        Array.Clear(processInputData, 0, processInputData.Length);
                        Array.Clear(processOutputData, 0, processOutputData.Length);
                        inputFactory = "/";
                        outputFactory = "/";
                    }
                    else//分品种工序其他仓库
                    {
                        Consumption = "0";//消耗量
                        foreach (DataRow dr_st in processInformaion.Rows)
                        {
                            WarehouseID_process = dr_st["WarehouseId"].ToString();
                            WarehouseName_process = dr_st["Name"].ToString();
                            process_process = dr_st["Process"].ToString();
                            if (process == WarehouseName_process)
                            {
                                ProcessInputCementTable = GetProcessInputInformation(endTime, startTime, OrganizationId, WarehouseName_process);//分工序水泥子库信息
                                break;
                            }
                        }
                        getProcessOutputWarehouseTable = GetConsumptionInformation(WarehouseID, databaseName, process);//获取消耗量信息
                        Consumption = GetConsumption(getProcessOutputWarehouseTable, ProcessInputCementTable, databaseName, endTime).ToString("0.00");//消耗量 
                        getBenchmarksInformationTable = GetBenchmarksInformation(startTime, WarehouseID);
                        try
                        {
                            benchmarksValue = Math.Round(Convert.ToDecimal(getBenchmarksInformationTable.Rows[0][0]), 2);
                        }
                        catch
                        {
                            Consumption = Convert.ToDecimal(Consumption).ToString("0.00");
                            Production = "/";
                            if (WarehouseName == "石灰石库(水泥)" || WarehouseName == "石灰石库(生料)")
                            {
                                m_dr["MaterialName"] = WarehouseName.Remove(WarehouseName.Length - 5, 5);
                            }
                            else
                            {
                                m_dr["MaterialName"] = WarehouseName.Remove(WarehouseName.Length - 1, 1);

                            }
                            m_dr["Process"] = process;
                            m_dr["Earlyinventory"] = 0;
                            m_dr["Input"] = 0;
                            m_dr["Moisture"] = null;
                            m_dr["Output"] = 0;
                            m_dr["DynamicInventory"] = 0;
                            m_dr["Ratio"] = null;
                            m_dr["WetBase"] = Consumption;
                            m_dr["DryBasis"] = null;
                            m_dr["Yield"] = Production;
                            m_dr["ProductMoisture"] = null;
                            resultDatatable.Rows.Add(m_dr);
                            Array.Clear(inputData, 0, inputData.Length);
                            Array.Clear(outputData, 0, outputData.Length);
                            Array.Clear(processInputData, 0, processInputData.Length);
                            Array.Clear(processOutputData, 0, processOutputData.Length);
                            inputFactory = "/";
                            outputFactory = "/";
                            continue;
                        }
                        Consumption = Convert.ToDecimal(Consumption).ToString("0.00");
                        Production = "/";
                        inputWaterData = process + WarehouseName + "Input";
                        try
                        {
                            inputWaterNumber = Convert.ToDecimal(dlc[inputWaterData]).ToString("0.00");
                            dryBasis = (Convert.ToDecimal(Consumption) * (1 - Convert.ToDecimal(dlc[inputWaterData]) / 100)).ToString("#0.00");
                        }
                        catch
                        {
                            inputWaterNumber = null;
                            dryBasis = null;
                        }
                        outputWaterData = process + WarehouseName + "Output";
                        try
                        {
                            outputWaterNumber = Convert.ToDecimal(dlc[outputWaterData]).ToString("#0.00");
                        }
                        catch
                        {
                            outputWaterNumber = null;
                        }
                        if (WarehouseName == "石灰石库(水泥)" || WarehouseName == "石灰石库(生料)")
                        {
                            m_dr["MaterialName"] = WarehouseName.Remove(WarehouseName.Length - 5, 5);
                        }
                        else
                        {
                            m_dr["MaterialName"] = WarehouseName.Remove(WarehouseName.Length - 1, 1);

                        }
                        m_dr["Process"] = process;
                        m_dr["Earlyinventory"] = "/";
                        m_dr["Input"] = inputFactory;
                        m_dr["Moisture"] = inputWaterNumber;
                        m_dr["Output"] = outputFactory;
                        m_dr["DynamicInventory"] = "/";
                        m_dr["Ratio"] = null;
                        m_dr["WetBase"] = Consumption;
                        m_dr["DryBasis"] = dryBasis;
                        m_dr["Yield"] = Production;
                        m_dr["ProductMoisture"] = outputWaterNumber;
                        Array.Clear(inputData, 0, inputData.Length);
                        Array.Clear(outputData, 0, outputData.Length);
                        Array.Clear(processInputData, 0, processInputData.Length);
                        Array.Clear(processOutputData, 0, processOutputData.Length);
                    }

                }

                resultDatatable.Rows.Add(m_dr);
            }
            return resultDatatable;

        }
        private static DataTable GetProcessInfomation(string OrganizationId)//获取工序名称
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select distinct A.Name,B.[WarehouseId],C.[Process],C.DisplayIndex,C.[VariableType]
                   from [NXJC].[dbo].[inventory_Warehouse] as A ,[NXJC].[dbo].[inventory_WareHousingContrast] as B,[NXJC].[dbo].[report_MaterialBalanceReport] as C
                  where A.Id=B.warehouseid and A.[OrganizationID]=@OrganizationID and B.[ItemId]=C.[KeyId] order by DisplayIndex,Process";
            SqlParameter myParameter = new SqlParameter("@OrganizationID", OrganizationId);
            DataTable table = dataFactory.Query(mySql, myParameter);
            return table;

        }
        //获取盘库基准信息
        private static DataTable GetBenchmarksInformation(string startTime, string IdInformation)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select Value,TimeStamp as TimeStamp,WarehouseId from [dbo].[inventory_CheckWarehouse] 
                                       where WarehouseId=@IdInformation and TimeStamp<=@startTime
                                      order by TimeStamp desc
                                      ";
            SqlParameter[] myParameter = { new SqlParameter("@startTime", startTime), new SqlParameter("@IdInformation", IdInformation) };
            DataTable table = dataFactory.Query(mySql, myParameter);
            return table;
        }
        //获取入库信息
        private static DataTable GetInputWarehouse(string BenchmarksId)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select VariableId,specs,DataTableName,WarehouseId,WarehousingType,Multiple,Offset from [dbo].[inventory_WareHousingContrast] 
                                       where WarehouseId=@BenchmarksId and WarehousingType='Input'
                                      ";
            SqlParameter[] myParameter = { new SqlParameter("@BenchmarksId", BenchmarksId) };
            DataTable table = dataFactory.Query(mySql, myParameter);
            return table;
        }
        //获取分工序入库信息
        private static DataTable GetProcessInputWarehouse(string BenchmarksId, string process)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.VariableId,A.specs,A.DataTableName,A.WarehouseId,A.WarehousingType,A.Multiple,A.Offset from [dbo].[inventory_WareHousingContrast] as A,[NXJC].[dbo].[report_MaterialBalanceReport] as B
                                       where A.WarehouseId=@BenchmarksId and A.WarehousingType='Input'and B.Process=@process and A.ItemId=B.KeyId
                                      ";
            SqlParameter[] myParameter = { new SqlParameter("@BenchmarksId", BenchmarksId), new SqlParameter("@process", process) };
            DataTable table = dataFactory.Query(mySql, myParameter);
            return table;
        }
        //获取出库总信息
        private static DataTable GetOutputWarehouse(string BenchmarksId)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select VariableId,specs,DataTableName, WarehouseId,WarehousingType,Multiple,Offset from [dbo].[inventory_WareHousingContrast] 
                                       where WarehouseId=@BenchmarksId and WarehousingType='Output'
                                      ";
            SqlParameter[] myParameter = { new SqlParameter("@BenchmarksId", BenchmarksId) };
            DataTable table = dataFactory.Query(mySql, myParameter);
            return table;
        }
        //获取分工序出库总信息
        private static DataTable GetProcessOutputWarehouse(string BenchmarksId, string process)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.VariableId,A.specs,A.DataTableName,A.WarehouseId,A.WarehousingType,A.Multiple,A.Offset from [dbo].[inventory_WareHousingContrast] as A,[NXJC].[dbo].[report_MaterialBalanceReport] as B
                                       where A.WarehouseId=@BenchmarksId and A.WarehousingType='Output'and B.Process=@process and A.ItemId=B.KeyId
                                      ";
            SqlParameter[] myParameter = { new SqlParameter("@BenchmarksId", BenchmarksId), new SqlParameter("@process", process) };
            DataTable table = dataFactory.Query(mySql, myParameter);
            return table;
        }
        //出入库量计算
        private static Decimal[] GetInputQuantity(string endTime, string BenchmarksTime, string organizationID, DataTable InputWarehouse, string databaseName)
        {
            int inputMark = 0;//入厂量标志
            int consumptionMark = 0;//消耗量标志
            int tableRowsCount = InputWarehouse.Rows.Count;
            Decimal[] inputNyglTotal = new Decimal[5];
            Decimal inputNygl = 0;
            Decimal inputSigleNygl = 0;
            Decimal inputWeighbridge = 0;
            Decimal inputConsumption = 0;
            for (int i = 0; i < tableRowsCount; i++)
            {
                string variableIdName = Convert.ToString(InputWarehouse.Rows[i][0]);
                string specsName = Convert.ToString(InputWarehouse.Rows[i][1]);
                string dataTableName = Convert.ToString(InputWarehouse.Rows[i][2]);
                string dataTableWarehouseId = Convert.ToString(InputWarehouse.Rows[i][3]);
                string dataTableWarehousingType = Convert.ToString(InputWarehouse.Rows[i][4]);
                string multiple = Convert.ToString(InputWarehouse.Rows[i][5]);
                string offset = Convert.ToString(InputWarehouse.Rows[i][6]);
                if (dataTableName == "material_MaterialChangeContrast")
                {

                    DataTable table = GetMaterialChangeDataTable(organizationID, "cementmill", BenchmarksTime, endTime);
                    foreach (DataRow dr in table.Rows)
                    {
                        if (dr["VariableId"].ToString() == variableIdName)
                        {
                            try
                            {
                                inputSigleNygl = Convert.ToDecimal(dr["Production"]) * Convert.ToDecimal(multiple) + Convert.ToDecimal(offset);
                            }
                            catch
                            {
                                inputSigleNygl = 0;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        inputNygl = inputNygl + inputSigleNygl;
                    }
                }
                else if (dataTableName == "VWB_WeightNYGL")
                {
                    inputMark++;
                    if (specsName == "")
                    {
                        string connectionString = ConnectionStringFactory.NXJCConnectionString;
                        ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
                        string mySql = @"select sum(Value) as InputSigle from [dbo].[VWB_WeightNYGL] 
                                       where @BenchmarksTime<StatisticalTime and StatisticalTime<@startTime and OrganizationID=@OrganizationID and VariableId=@VariableId
                                      ";
                        SqlParameter[] myParameter = { new SqlParameter("@startTime", endTime), new SqlParameter("@BenchmarksTime", BenchmarksTime), new SqlParameter("@OrganizationID", organizationID), new SqlParameter("@VariableId", variableIdName) };
                        DataTable table = dataFactoryNew.Query(mySql, myParameter);
                        try
                        {
                            inputSigleNygl = Convert.ToDecimal(table.Rows[0][0]) * Convert.ToDecimal(multiple) + Convert.ToDecimal(offset);
                        }
                        catch
                        {
                            inputSigleNygl = 0;
                        }
                        inputNygl = inputNygl + inputSigleNygl;
                        inputWeighbridge = inputWeighbridge + inputSigleNygl;

                    }
                    else
                    {
                        string connectionString = ConnectionStringFactory.NXJCConnectionString;
                        ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
                        string mySql = @"select sum(Value) as InputSigle from [dbo].[VWB_WeightNYGL] 
                                       where @BenchmarksTime<StatisticalTime and StatisticalTime<@startTime and OrganizationID=@OrganizationID and VariableId=@VariableId  and [VariableSpecs]=@specsName
                                      ";
                        SqlParameter[] myParameter = { new SqlParameter("@startTime", endTime), new SqlParameter("@BenchmarksTime", BenchmarksTime), new SqlParameter("@OrganizationID", organizationID), new SqlParameter("@VariableId", variableIdName), new SqlParameter("@specsName", specsName) };
                        DataTable table = dataFactoryNew.Query(mySql, myParameter);
                        try
                        {
                            inputSigleNygl = Convert.ToDecimal(table.Rows[0][0]) * Convert.ToDecimal(multiple) + Convert.ToDecimal(offset);
                        }
                        catch
                        {
                            inputSigleNygl = 0;
                        }
                        inputNygl = inputNygl + inputSigleNygl;
                        inputWeighbridge = inputWeighbridge + inputSigleNygl;

                    }
                }
                else if (dataTableName == "HistoryDCSIncrement")
                {
                    consumptionMark++;
                    string connectionString = ConnectionStringFactory.NXJCConnectionString;
                    ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
                    string mySql = @"select sum({0}) as InputSigle from {1}.[dbo].[HistoryDCSIncrement]
                                       where @BenchmarksTime<[vDate] and [vDate]<@startTime";
                    SqlParameter[] myParameter = { new SqlParameter("@startTime", endTime), new SqlParameter("@BenchmarksTime", BenchmarksTime) };
                    DataTable table = dataFactoryNew.Query(string.Format(mySql, variableIdName, databaseName), myParameter);
                    try
                    {
                        inputSigleNygl = Convert.ToDecimal(table.Rows[0][0]) * Convert.ToDecimal(multiple) + Convert.ToDecimal(offset);
                    }
                    catch
                    {
                        inputSigleNygl = 0;
                    }
                    inputNygl = inputNygl + inputSigleNygl;
                    inputConsumption = inputConsumption + inputSigleNygl;

                }
            }
            inputNygl = Math.Round(inputNygl, 2);
            inputNyglTotal[0] = inputNygl;
            inputNyglTotal[1] = inputWeighbridge;
            inputNyglTotal[2] = inputConsumption;
            inputNyglTotal[3] = inputMark;
            inputNyglTotal[4] = consumptionMark;
            return inputNyglTotal;
        }
        //分工序水泥子库出入库量信息
        private static DataTable GetProcessInputInformation(string endTime, string benchmarksTime, string OrganizationId, string WarehouseName)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.materialcolumn,A.materialdatabasename,A.MaterialDataTableName,B.ChangeStartTime,B.changeEndtime,B.[OrganizationID] from [NXJC].[dbo].[material_MaterialChangeContrast] as A left outer join [NXJC].[dbo].[material_MaterialChangeLog] as B
                             on A.Valid=B.EventValue and A.ContrastID=B.ContrastID
                             where A.[MaterialDataBaseName]=@OrganizationId and changestarttime>=@benchmarksTime and changestarttime<=@endTime and changeendtime<=@endTime and A.variableid=@WarehouseName or
                               (A.[MaterialDataBaseName]=@OrganizationId and changestarttime>=@benchmarksTime and changestarttime<=@endTime and A.variableid=@WarehouseName and [ChangeEndTime] is null)";
            SqlParameter[] myParameter = { new SqlParameter("@OrganizationId", OrganizationId), new SqlParameter("@benchmarksTime", benchmarksTime), new SqlParameter("@endTime", endTime), new SqlParameter("@WarehouseName", WarehouseName) };
            DataTable table = dataFactoryNew.Query(mySql, myParameter);
            return table;
        }
        private static DataTable GetConsumptionInformation(string WarehouseName, string databasename, string process)//获取消耗量信息
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
            string mySql = @"select C.VariableId,C.specs,C.DataTableName,C.WarehouseId,C.WarehousingType,C.Multiple,C.Offset,D.[OrganizationID] from      (
                     select A.VariableId,A.specs,A.DataTableName,A.WarehouseId,A.WarehousingType,A.Multiple,A.Offset from NXJC.[dbo].[inventory_WareHousingContrast]  as A  ,[NXJC].[dbo].[report_MaterialBalanceReport] as B
                                       where A.WarehouseId=@WarehouseName and A.WarehousingType='output'and B.Process=@process and A.ItemId=B.KeyId) as C left join {0}.[dbo].[View_DCSContrast] as D on C.VariableId=D.[CumulantName]";
            SqlParameter[] myParameter = { new SqlParameter("@process", process), new SqlParameter("@WarehouseName", WarehouseName) };
            DataTable table = dataFactoryNew.Query(string.Format(mySql, databasename), myParameter);
            return table;
        }
        private static Decimal GetConsumption(DataTable ConsumptionInformation, DataTable ProcessInputInformation, string databasename, string endTime)//计算消耗量
        {
            Decimal inputNygl = 0;
            foreach (DataRow dr in ProcessInputInformation.Rows)
            {
                string materialColumn = Convert.ToString(dr[0]);
                string materialDatabaseName = Convert.ToString(dr[1]);
                string materialDataTableName = Convert.ToString(dr[2]);
                string changeStartTime = Convert.ToString(dr[3]);
                string changeEndtime = Convert.ToString(dr[4]);
                string organizationID = Convert.ToString(dr[5]);
                Decimal inputSigleNygl = 0;
                foreach (DataRow st in ConsumptionInformation.Rows)
                {
                    string variableId = Convert.ToString(st[0]);
                    string multiple = Convert.ToString(st[5]);
                    string offset = Convert.ToString(st[6]);
                    string conSumptionId = Convert.ToString(st[7]);
                    if (organizationID == conSumptionId)
                    {
                        if (changeEndtime == "")
                        {
                            changeEndtime = endTime;
                        }
                        string connectionString = ConnectionStringFactory.NXJCConnectionString;
                        ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
                        string mySql = @"select sum({0}) as InputSigle from {1}.[dbo].[HistoryDCSIncrement]
                                       where @BenchmarksTime<[vDate] and [vDate]<@startTime";
                        SqlParameter[] myParameter = { new SqlParameter("@startTime", changeEndtime), new SqlParameter("@BenchmarksTime", changeStartTime) };
                        DataTable table = dataFactoryNew.Query(string.Format(mySql, variableId, databasename), myParameter);
                        try
                        {
                            inputSigleNygl = Convert.ToDecimal(table.Rows[0][0]) * Convert.ToDecimal(multiple) + Convert.ToDecimal(offset);
                        }
                        catch
                        {
                            inputSigleNygl = 0;
                        }
                        inputNygl = inputNygl + inputSigleNygl;
                    }
                }

            }
            return inputNygl;
        }
        private static Decimal GetProcessQuantity(DataTable ProcessInputCementTable, string starttime, string endtime)
        {
            Decimal input = 0;
            Decimal inputTotal = 0;
            foreach (DataRow dr in ProcessInputCementTable.Rows)
            {
                string materialColumn = Convert.ToString(dr[0]);
                string materialDatabaseName = Convert.ToString(dr[1]);
                string materialDataTableName = Convert.ToString(dr[2]);
                string changeStartTime = Convert.ToString(dr[3]);
                string changeEndtime = "";
                changeEndtime = Convert.ToString(dr[4]);
                if (changeEndtime == "")
                {
                    changeEndtime = endtime;
                }


                string connectionString = ConnectionStringFactory.NXJCConnectionString;
                ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
                string mySql = @"select sum({0}) as InputSigle from {1}.[dbo].[HistoryDCSIncrement]
                                       where @changeStartTime<[vDate] and [vDate]<@changeEndtime";
                SqlParameter[] myParameter = { new SqlParameter("@changeEndtime", changeEndtime), new SqlParameter("@changeStartTime", changeStartTime) };
                DataTable table = dataFactoryNew.Query(string.Format(mySql, materialColumn, materialDatabaseName), myParameter);
                try
                {
                    input = Convert.ToDecimal(table.Rows[0][0]);
                }
                catch
                {
                    input = 0;
                }
                inputTotal = input + inputTotal;
            }
            inputTotal = Math.Round(inputTotal, 2);
            return inputTotal;
        }

        private static DataTable GetFormula(DataTable materialInformationData)
        {
            DataTable resultTable = new DataTable();
            resultTable.Columns.Add("Process", Type.GetType("System.String"));
            resultTable.Columns.Add("MaterialName", Type.GetType("System.String"));
            resultTable.Columns.Add("Earlyinventory", Type.GetType("System.String"));
            resultTable.Columns.Add("Input", Type.GetType("System.String"));
            resultTable.Columns.Add("Moisture", Type.GetType("System.String"));
            resultTable.Columns.Add("Output", Type.GetType("System.String"));
            resultTable.Columns.Add("DynamicInventory", Type.GetType("System.String"));
            resultTable.Columns.Add("Ratio", Type.GetType("System.String"));
            resultTable.Columns.Add("WetBase", Type.GetType("System.String"));
            resultTable.Columns.Add("DryBasis", Type.GetType("System.String"));
            resultTable.Columns.Add("Formula", Type.GetType("System.String"));
            resultTable.Columns.Add("Yield", Type.GetType("System.String"));
            resultTable.Columns.Add("ProductMoisture", Type.GetType("System.String"));
            Dictionary<string, Decimal> dlc = new Dictionary<string, decimal>();
            Decimal ratioData = 0;//用于计算各工序物料消耗量
            Decimal ratioDataTotal = 0;//用于计算各工序物料消耗量之和
            int countNumber = materialInformationData.Rows.Count;
            string matarialName = "";
            string processName = materialInformationData.Rows[0]["Process"].ToString();
            for (int i = 0; i <= countNumber - 1; i++)
            {
                DataRow st = materialInformationData.Rows[i];
                if (processName == st["Process"].ToString())
                {
                    try
                    {
                        ratioData = Convert.ToDecimal(st["WetBase"]);
                    }
                    catch
                    {
                        ratioData = 0;
                    }
                    ratioDataTotal = ratioDataTotal + ratioData;
                    processName = st["Process"].ToString();
                }
                else
                {
                    dlc.Add(processName, ratioDataTotal);
                    ratioData = 0;
                    ratioDataTotal = 0;
                    try
                    {
                        ratioData = Convert.ToDecimal(st["WetBase"]);
                    }
                    catch
                    {
                        ratioData = 0;
                    }
                    ratioDataTotal = ratioDataTotal + ratioData;
                    processName = st["Process"].ToString();
                }
                if (i == countNumber - 1)
                {
                    dlc.Add(processName, ratioDataTotal);
                }

            }
            foreach (DataRow dr in materialInformationData.Rows)
            {
                DataRow m_dr = resultTable.NewRow();
                processName = dr["Process"].ToString();
                matarialName = dr["MaterialName"].ToString();
                ratioData = dlc[processName];
                if (processName == "生料" || processName == "燃料" || processName == "熟料")
                {
                    m_dr["Process"] = dr["Process"];
                    m_dr["MaterialName"] = dr["MaterialName"];
                    m_dr["Earlyinventory"] = dr["Earlyinventory"];
                    m_dr["Input"] = dr["Input"];
                    m_dr["Moisture"] = dr["Moisture"];
                    m_dr["Output"] = dr["Output"];
                    m_dr["DynamicInventory"] = dr["DynamicInventory"];
                    m_dr["Ratio"] = null;
                    m_dr["Formula"] = "/";
                    m_dr["WetBase"] = dr["WetBase"];
                    m_dr["DryBasis"] = dr["DryBasis"];
                    m_dr["Yield"] = dr["Yield"];
                    m_dr["ProductMoisture"] = dr["ProductMoisture"];
                    resultTable.Rows.Add(m_dr);
                    continue;
                }
                if (ratioData == 0)
                {
                    m_dr["Process"] = dr["Process"];
                    m_dr["MaterialName"] = dr["MaterialName"];
                    m_dr["Earlyinventory"] = dr["Earlyinventory"];
                    m_dr["Input"] = dr["Input"];
                    m_dr["Moisture"] = dr["Moisture"];
                    m_dr["Output"] = dr["Output"];
                    m_dr["DynamicInventory"] = dr["DynamicInventory"];
                    m_dr["Ratio"] = null;
                    m_dr["Formula"] = "0";
                    m_dr["WetBase"] = dr["WetBase"];
                    m_dr["DryBasis"] = dr["DryBasis"];
                    m_dr["Yield"] = dr["Yield"];
                    m_dr["ProductMoisture"] = dr["ProductMoisture"];
                    resultTable.Rows.Add(m_dr);
                    continue;
                }
                if (matarialName == "水泥")
                {
                    m_dr["Process"] = dr["Process"];
                    m_dr["MaterialName"] = dr["MaterialName"];
                    m_dr["Earlyinventory"] = dr["Earlyinventory"];
                    m_dr["Input"] = dr["Input"];
                    m_dr["Moisture"] = dr["Moisture"];
                    m_dr["Output"] = dr["Output"];
                    m_dr["DynamicInventory"] = dr["DynamicInventory"];
                    m_dr["Ratio"] = null;
                    m_dr["Formula"] = "1";
                    m_dr["WetBase"] = dr["WetBase"];
                    m_dr["DryBasis"] = dr["DryBasis"];
                    m_dr["Yield"] = dr["Yield"];
                    m_dr["ProductMoisture"] = dr["ProductMoisture"];
                    resultTable.Rows.Add(m_dr);
                    continue;
                }
                m_dr["Process"] = dr["Process"];
                m_dr["MaterialName"] = dr["MaterialName"];
                m_dr["Earlyinventory"] = dr["Earlyinventory"];
                m_dr["Input"] = dr["Input"];
                m_dr["Moisture"] = dr["Moisture"];
                m_dr["Output"] = dr["Output"];
                m_dr["DynamicInventory"] = dr["DynamicInventory"];
                m_dr["Ratio"] = null;
                try
                {
                    m_dr["Formula"] = Math.Round(Convert.ToDecimal(dr["WetBase"]) / ratioData, 2).ToString("0.00");
                }
                catch
                {
                    m_dr["Formula"] = "0";
                }
                m_dr["WetBase"] = dr["WetBase"];
                m_dr["DryBasis"] = dr["DryBasis"];
                m_dr["Yield"] = dr["Yield"];
                m_dr["ProductMoisture"] = dr["ProductMoisture"];
                resultTable.Rows.Add(m_dr);
            }
            return resultTable;
        }
        private static DataTable GetRatio(DataTable tableInformation)
        {
            DataTable resultTable = new DataTable();
            resultTable.Columns.Add("Process", Type.GetType("System.String"));
            resultTable.Columns.Add("MaterialName", Type.GetType("System.String"));
            resultTable.Columns.Add("Earlyinventory", Type.GetType("System.String"));
            resultTable.Columns.Add("Input", Type.GetType("System.String"));
            resultTable.Columns.Add("Moisture", Type.GetType("System.String"));
            resultTable.Columns.Add("Output", Type.GetType("System.String"));
            resultTable.Columns.Add("DynamicInventory", Type.GetType("System.String"));
            resultTable.Columns.Add("Ratio", Type.GetType("System.String"));
            resultTable.Columns.Add("WetBase", Type.GetType("System.String"));
            resultTable.Columns.Add("DryBasis", Type.GetType("System.String"));
            resultTable.Columns.Add("Formula", Type.GetType("System.String"));
            resultTable.Columns.Add("Yield", Type.GetType("System.String"));
            resultTable.Columns.Add("ProductMoisture", Type.GetType("System.String"));
            Dictionary<string, string> dll = new Dictionary<string, string>();
            Decimal rawMaterial = 0;
            foreach (DataRow sr in tableInformation.Rows)
            {
                if (sr[0].ToString() == "原料")
                {
                    rawMaterial = rawMaterial + Convert.ToDecimal(sr["WetBase"]);
                    continue;
                }
            }
            dll.Add("生料消耗量", rawMaterial.ToString());
            foreach (DataRow dr in tableInformation.Rows)
            {

                if (dr[0].ToString() == "熟料" && dr[1].ToString() == "熟料")
                {
                    dll.Add("熟料产量", dr["Yield"].ToString());
                    continue;
                }
                if (dr[0].ToString() == "水泥" && dr[1].ToString() == "水泥")
                {
                    dll.Add("水泥产量", dr["Yield"].ToString());
                    continue;
                }
                if (dr[0].ToString() == "水泥" && dr[1].ToString() == "熟料")
                {
                    dll.Add("熟料消耗量", dr["WetBase"].ToString());
                    continue;
                }

            }
            foreach (DataRow st in tableInformation.Rows)
            {
                DataRow m_dr = resultTable.NewRow();
                if (st[0].ToString() == "熟料" && st[1].ToString() == "熟料")
                {
                    m_dr["Process"] = st["Process"];
                    m_dr["MaterialName"] = st["MaterialName"];
                    m_dr["Earlyinventory"] = st["Earlyinventory"];
                    m_dr["Input"] = st["Input"];
                    m_dr["Moisture"] = st["Moisture"];
                    m_dr["Output"] = st["Output"];
                    m_dr["DynamicInventory"] = st["DynamicInventory"];
                    try
                    {
                        m_dr["Ratio"] = (Convert.ToDecimal(dll["熟料产量"]) / Convert.ToDecimal(dll["生料消耗量"])).ToString("0.00");
                    }
                    catch
                    {
                        m_dr["Ratio"] = "/";
                    }
                    m_dr["Formula"] = st["Formula"];
                    m_dr["WetBase"] = st["WetBase"];
                    m_dr["DryBasis"] = st["DryBasis"];
                    m_dr["Yield"] = st["Yield"];
                    m_dr["ProductMoisture"] = st["ProductMoisture"];
                    resultTable.Rows.Add(m_dr);
                    continue;

                }
                else if (st[0].ToString() == "水泥" && st[1].ToString() == "水泥")
                {
                    m_dr["Process"] = st["Process"];
                    m_dr["MaterialName"] = st["MaterialName"];
                    m_dr["Earlyinventory"] = st["Earlyinventory"];
                    m_dr["Input"] = st["Input"];
                    m_dr["Moisture"] = st["Moisture"];
                    m_dr["Output"] = st["Output"];
                    m_dr["DynamicInventory"] = st["DynamicInventory"];
                    try
                    {
                        m_dr["Ratio"] = (Convert.ToDecimal(dll["熟料消耗量"]) / Convert.ToDecimal(dll["水泥产量"])).ToString("0.00");
                    }
                    catch
                    {
                        m_dr["Ratio"] = "/";
                    }
                    m_dr["Formula"] = st["Formula"];
                    m_dr["WetBase"] = st["WetBase"];
                    m_dr["DryBasis"] = st["DryBasis"];
                    m_dr["Yield"] = st["Yield"];
                    m_dr["ProductMoisture"] = st["ProductMoisture"];
                    resultTable.Rows.Add(m_dr);
                    continue;
                }
                else
                {
                    m_dr["Process"] = st["Process"];
                    m_dr["MaterialName"] = st["MaterialName"];
                    m_dr["Earlyinventory"] = st["Earlyinventory"];
                    m_dr["Input"] = st["Input"];
                    m_dr["Moisture"] = st["Moisture"];
                    m_dr["Output"] = st["Output"];
                    m_dr["DynamicInventory"] = st["DynamicInventory"];
                    m_dr["Ratio"] = "/";
                    m_dr["Formula"] = st["Formula"];
                    m_dr["WetBase"] = st["WetBase"];
                    m_dr["DryBasis"] = st["DryBasis"];
                    m_dr["Yield"] = st["Yield"];
                    m_dr["ProductMoisture"] = st["ProductMoisture"];
                    resultTable.Rows.Add(m_dr);
                    continue;
                }
            }
            return resultTable;
        }
        private static DataTable GetWaterInformation(string OrganizationID)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
            string mySql = @"select distinct A.Name,B.[WarehouseId],B.[WarehousingType],C.[Process],C.DisplayIndex,C.[VariableType],C.[WaterVariable] from [NXJC].[dbo].[inventory_Warehouse] as A ,[NXJC].[dbo].[inventory_WareHousingContrast] as B,[NXJC].[dbo].[report_MaterialBalanceReport] as C
                  where A.Id=B.warehouseid and A.[OrganizationID]=@OrganizationID and B.[ItemId]=C.[KeyId] and C.WaterVariable is not null order by DisplayIndex,Process ";
            SqlParameter[] myParameter = { new SqlParameter("@OrganizationID", OrganizationID) };
            DataTable table = dataFactoryNew.Query(string.Format(mySql), myParameter);


            return table;
        }
        private static Dictionary<string, string> GetWaterData(DataTable waterInformation, string OrganizationID, string startTime, string endTime)
        {
            Dictionary<string, string> dlc = new Dictionary<string, string>();
            string avg = "";
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
            string mySql = @"select  avg(datavalue) as data from  [NXJC].[dbo].[system_EnergyDataManualInput] where [UpdateCycle]='day' and OrganizationID=@OrganizationID and [VariableId]=@WaterVariable and TimeStamp>=@startTime and TimeStamp<=@endTime";
            foreach (DataRow sr in waterInformation.Rows)
            {
                string key = sr["Process"].ToString() + sr["Name"] + sr["WarehousingType"];
                string WaterVariable = sr["WaterVariable"].ToString();
                SqlParameter[] myParameter = { new SqlParameter("@OrganizationID", OrganizationID), new SqlParameter("@WaterVariable", WaterVariable), new SqlParameter("@startTime", startTime), new SqlParameter("@endTime", endTime) };
                DataTable table = dataFactoryNew.Query(string.Format(mySql), myParameter);
                if (table.Rows[0]["data"].ToString() == "")
                {
                    avg = "0";

                }
                else
                {
                    avg = table.Rows[0]["data"].ToString();
                }
                dlc.Add(key, avg);
            }
            return dlc;
        }
        private static string GetDataBaseName(string OrganizationID)//获取组织机构
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
            string mySql = @"select B.[MeterDatabase] from [NXJC].[dbo].[system_Organization] as A ,[NXJC].[dbo].[system_Database] as B where A.[OrganizationID]=@OrganizationID and A.[DatabaseID]=B.[DatabaseID] ";
            SqlParameter[] myParameter = { new SqlParameter("@OrganizationID", OrganizationID) };
            DataTable table = dataFactoryNew.Query(string.Format(mySql), myParameter);
            string dabaseName = table.Rows[0][0].ToString();
            return dabaseName;

        }
        #region
        //         private static DataTable GetWaterData(DataTable waterInformation)
        //         {
        //             int rowsCount = waterInformation.Rows.Count;
        //             StringBuilder sr = new StringBuilder();
        //             for (int i = 0; i < rowsCount; i++)
        //             {

        //             }
        //             string sql = @"";
        //             sr.Append("");
        //             foreach (DataRow dr in waterInformation.Rows)
        //           { 



        //             }



        //             string connectionString = ConnectionStringFactory.NXJCConnectionString;
        //             ISqlServerDataFactory dataFactoryNew = new SqlServerDataFactory(connectionString);
        //             string mySql = @"  select  avg(A.datavalue) as PulverizedCoal_WaterContent,AVG(B.DataValue) As RawCoal_WaterContent
        //    FROM ( select * from [NXJC].[dbo].[system_EnergyDataManualInput] where [UpdateCycle]='day' and OrganizationID='zc_nxjc_byc_byf' and [VariableId]='PulverizedCoal_WaterContent')AS A,
        //  ( select * from [NXJC].[dbo].[system_EnergyDataManualInput] where [UpdateCycle]='day' and OrganizationID='zc_nxjc_byc_byf' and [VariableId]='RawCoal_WaterContent')AS B";
        //             SqlParameter[] myParameter = { new SqlParameter("@OrganizationID", OrganizationID) };
        //             DataTable table = dataFactoryNew.Query(string.Format(mySql), myParameter);
        //             return table;
        //         }
        #endregion
        //分品种库存入库（调用了徐一帅的算法有多余数据）
        private static DataTable GetMaterialChangeDataTable(string mOrganizationId, string productionLine, string startTime, string endTime)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            DataTable table = new DataTable();
            if (productionLine == "cementmill")
            {
                string Allsql = @"(SELECT 
                                     A.[OrganizationID]
                                    ,C.[Name]
		                            ,B.[MaterialColumn]
		                            ,A.[ChangeStartTime]
		                            ,A.[ChangeEndTime]
		                            ,B.[VariableId]
		                            ,B.[MaterialDataBaseName]
		                            ,B.[MaterialDataTableName]
                                    ,'' as [LevelCode]
		                            ,'leafnode' as [NodeType]
	                            FROM [NXJC].[dbo].[material_MaterialChangeLog] A,[NXJC].[dbo].[material_MaterialChangeContrast] B,[NXJC].[dbo].[system_Organization] C
	                            where A.OrganizationID like @mOrganizationId + '%'
                                    and A.OrganizationID=C.OrganizationID
	                                and B.[ContrastID]=A.[ContrastID]
		                            and A.[VariableType]='Cement'
		                            and LOWER(A.EventValue) = LOWER(B.Valid)
                                    and B.[VariableId] != '自产/外购熟料'
		                            and ((A.[ChangeStartTime]<=@startTime and A.[ChangeEndTime]>=@startTime) or
                                    (A.[ChangeStartTime]>=@startTime
                                    and A.[ChangeEndTime]<=@endTime) or (A.[ChangeStartTime]<=@startTime and A.[ChangeEndTime]>=@endTime)
	                                or (A.[ChangeStartTime]<=@endTime and A.[ChangeEndTime]>=@endTime)
                                    or (A.[ChangeStartTime]<=@endTime and A.[ChangeEndTime] is NULL))
                                )
	                            union all
	                            (SELECT  A.[OrganizationID],C.[Name],'' as [MaterialColumn],'' as [ChangeStartTime],'' as [ChangeEndTime],'' as [VariableId]
	                            ,'' as [MaterialDataBaseName],'' as [MaterialDataTableName],'' as [LevelCode], 'node' as [NodeType]
	                             from [NXJC].[dbo].[material_MaterialChangeLog] A,[NXJC].[dbo].[material_MaterialChangeContrast] B,[NXJC].[dbo].[system_Organization] C
	                            where A.OrganizationID like @mOrganizationId + '%'
                                    and A.OrganizationID=C.OrganizationID
	                                and B.[ContrastID]=A.[ContrastID]
		                            and A.[VariableType]='Cement'
		                            and LOWER(A.EventValue) = LOWER(B.Valid)
                                    and B.[VariableId] != '自产/外购熟料'
		                            and ((A.[ChangeStartTime]<=@startTime and A.[ChangeEndTime]>=@startTime) or
                                    (A.[ChangeStartTime]>=@startTime
                                    and A.[ChangeEndTime]<=@endTime) or (A.[ChangeStartTime]<=@startTime and A.[ChangeEndTime]>=@endTime)
	                                or (A.[ChangeStartTime]<=@endTime and A.[ChangeEndTime]>=@endTime)
                                    or (A.[ChangeStartTime]<=@endTime and A.[ChangeEndTime] is NULL))
		                            --order by A.[OrganizationID]
		                            group by A.[OrganizationID],C.[Name])
		                            order by A.[OrganizationID],A.[ChangeStartTime]";
                SqlParameter[] Allparameter ={
                                          new SqlParameter("mOrganizationId", mOrganizationId),
                                          new SqlParameter("startTime", startTime),
                                          new SqlParameter("endTime", endTime)
                                       };
                table = dataFactory.Query(Allsql, Allparameter);
            }
            else
            {
                string sql = @"(SELECT  
                                   A.[OrganizationID]
                                  ,C.[Name]
		                          ,B.[MaterialColumn]
		                          ,A.[ChangeStartTime]
		                          ,A.[ChangeEndTime]
		                          ,B.[VariableId]
		                          ,B.[MaterialDataBaseName]
		                          ,B.[MaterialDataTableName]
                                  ,'' as [LevelCode]
                                  ,'leafnode' as [NodeType]
	                          FROM [NXJC].[dbo].[material_MaterialChangeLog] A,[NXJC].[dbo].[material_MaterialChangeContrast] B,[NXJC].[dbo].[system_Organization] C
	                          where A.OrganizationID=@productionLine
                                    and A.OrganizationID=C.OrganizationID
	                                and B.[ContrastID]=A.[ContrastID]
			                        and A.[VariableType]='Cement'
			                        and LOWER(A.EventValue) = LOWER(B.Valid)
                                    and B.[VariableId] != '自产/外购熟料'
			                        and ((A.[ChangeStartTime]<=@startTime and A.[ChangeEndTime]>=@startTime) or
                                    (A.[ChangeStartTime]>=@startTime
                                    and A.[ChangeEndTime]<=@endTime) or (A.[ChangeStartTime]<=@startTime and A.[ChangeEndTime]>=@endTime)
	                                or (A.[ChangeStartTime]<=@endTime and A.[ChangeEndTime]>=@endTime)
                                    or (A.[ChangeStartTime]<=@endTime and A.[ChangeEndTime] is NULL))
                              union all
                              (SELECT  
                                   A.[OrganizationID]
                                  ,C.[Name]
		                          ,'' as [MaterialColumn]
		                          ,'' as [ChangeStartTime]
		                          ,'' as [ChangeEndTime]
		                          ,'' as [VariableId]
		                          ,'' as [MaterialDataBaseName]
		                          ,'' as [MaterialDataTableName]
                                  ,'' as [LevelCode]
                                  ,'node' as [NodeType]
	                          FROM [NXJC].[dbo].[material_MaterialChangeLog] A,[NXJC].[dbo].[material_MaterialChangeContrast] B,[NXJC].[dbo].[system_Organization] C
	                          where A.OrganizationID=@productionLine
                                    and A.OrganizationID=C.OrganizationID
	                                and B.[ContrastID]=A.[ContrastID]
			                        and A.[VariableType]='Cement'
			                        and LOWER(A.EventValue) = LOWER(B.Valid)
                                    and B.[VariableId] != '自产/外购熟料'
			                        and ((A.[ChangeStartTime]<=@startTime and A.[ChangeEndTime]>=@startTime) or
                                    (A.[ChangeStartTime]>=@startTime
                                    and A.[ChangeEndTime]<=@endTime) or (A.[ChangeStartTime]<=@startTime and A.[ChangeEndTime]>=@endTime)
	                                or (A.[ChangeStartTime]<=@endTime and A.[ChangeEndTime]>=@endTime)
                                    or (A.[ChangeStartTime]<=@endTime and A.[ChangeEndTime] is NULL))
                                    group by A.[OrganizationID],C.[Name]))
                              order by A.[ChangeStartTime]";
                SqlParameter[] parameter ={
                                          new SqlParameter("productionLine", productionLine),
                                          new SqlParameter("startTime", startTime),
                                          new SqlParameter("endTime", endTime)
                                       };
                table = dataFactory.Query(sql, parameter);
                DataRow row;
                row = table.NewRow();
                row["OrganizationID"] = productionLine;
                row["LevelCode"] = "M01";
            }
            table.Columns.Add("Production");
            table.Columns.Add("Formula");
            table.Columns.Add("Consumption");
            int count = table.Rows.Count;
            for (int j = 0; j < count; j++)
            {
                if (table.Rows[j]["ChangeEndTime"].ToString() == "")
                {
                    table.Rows[j]["ChangeEndTime"] = endTime;
                }
                DateTime m_startTime = Convert.ToDateTime(table.Rows[j]["ChangeStartTime"].ToString().Trim());
                DateTime m_endTime = Convert.ToDateTime(table.Rows[j]["ChangeEndTime"].ToString().Trim());
                if (DateTime.Compare(m_startTime, Convert.ToDateTime(startTime)) == -1)
                {
                    table.Rows[j]["ChangeStartTime"] = startTime;
                }
                if (DateTime.Compare(m_endTime, Convert.ToDateTime(endTime)) == 1)
                {
                    table.Rows[j]["ChangeEndTime"] = endTime;
                }
            }
            for (int i = 0; i < count; i++)
            {
                string nodeType = table.Rows[i]["NodeType"].ToString().Trim();
                if (nodeType == "leafnode")
                {
                    string materialDataBaseName = table.Rows[i]["MaterialDataBaseName"].ToString().Trim();
                    string materialDataTableName = table.Rows[i]["MaterialDataTableName"].ToString().Trim();
                    string changeStartTime = table.Rows[i]["ChangeStartTime"].ToString().Trim();
                    string changeEndTime = table.Rows[i]["ChangeEndTime"].ToString().Trim();
                    string materialColumn = table.Rows[i]["MaterialColumn"].ToString().Trim();
                    string m_productionLine = table.Rows[i]["OrganizationID"].ToString().Trim();
                    //string mProductionLine = table.Rows[i]["OrganizationID"].ToString().Trim();
                    //                string mSql = @"select cast(sum(A.{0}) as decimal(18,2)) as [MaterialProduction]
                    //                                      ,cast(sum(B.[FormulaValue]) as decimal(18,2)) as [Formula]
                    //                                from {1}.[dbo].{2} A,{1}.[dbo].[HistoryFormulaValue] B
                    //                                where A.[vDate]>=@changeStartTime
                    //                                      and A.[vDate]<=@changeEndTime
                    //                                      and B.[OrganizationID]=@productionLine
                    //                                      and B.[vDate]>=@changeStartTime
                    //                                      and B.[vDate]<=@changeEndTime";
                    string mSql = @"select cast(sum([FormulaValue]) as decimal(18,2)) as [Formula] from {0}.[dbo].[HistoryFormulaValue]
                                where vDate>=@changeStartTime
                                        and vDate<=@changeEndTime
                                        and variableId = 'cementPreparation'
	                                    and [OrganizationID]=@m_productionLine";
                    SqlParameter[] para ={
                                        new SqlParameter("m_productionLine", m_productionLine),
                                        new SqlParameter("changeStartTime", changeStartTime),
                                        new SqlParameter("changeEndTime", changeEndTime)
                                     };
                    DataTable passTable = dataFactory.Query(string.Format(mSql, materialDataBaseName), para);
                    string mFormula = passTable.Rows[0]["Formula"].ToString().Trim();
                    string mSsql = @"select cast(sum({0}) as decimal(18,2)) as [MaterialProduction] from {1}.[dbo].{2}
                                where vDate>=@changeStartTime
                                      and vDate<=@changeEndTime";
                    SqlParameter[] paras ={
                                        new SqlParameter("changeStartTime", changeStartTime),

                                        new SqlParameter("changeEndTime", changeEndTime)
                                     };
                    DataTable resultTable = dataFactory.Query(string.Format(mSsql, materialColumn, materialDataBaseName, materialDataTableName), paras);
                    string mProduction = resultTable.Rows[0]["MaterialProduction"].ToString().Trim();
                    table.Rows[i]["Production"] = mProduction;
                    table.Rows[i]["Formula"] = mFormula;
                }
            }
            //增加层次码
            int mcode = 0;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string id = table.Rows[i]["NodeType"].ToString();
                if (id == "node")
                {
                    string nodeCode = "M01" + (++mcode).ToString("00");
                    table.Rows[i]["LevelCode"] = nodeCode;
                    int mleafcode = 0;
                    for (int j = 0; j < table.Rows.Count; j++)
                    {
                        if (table.Rows[j]["OrganizationID"].ToString().Trim() == table.Rows[i]["OrganizationID"].ToString().Trim() && table.Rows[j]["NodeType"].ToString().Equals("leafnode"))
                        {
                            table.Rows[j]["LevelCode"] = nodeCode + (++mleafcode).ToString("00");
                        }
                    }
                }
            }
            DataColumn stateColumn = new DataColumn("state", typeof(string));
            table.Columns.Add(stateColumn);
            //此处代码是控制树开与闭的
            //foreach (DataRow dr in table.Rows)
            //{
            //    if (dr["NodeType"].ToString() == "node")
            //    {
            //        dr["state"] = "closed";                           
            //    }
            //    else
            //    {
            //        dr["state"] = "open";
            //    }
            //}
            //计算电耗和产线总计
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i]["Production"].ToString().Trim() != "0.00" && table.Rows[i]["Production"].ToString().Trim() != "")
                {
                    string mFormula = table.Rows[i]["Formula"].ToString().Trim();
                    if (mFormula == "")
                    {
                        mFormula = "0";
                    }
                    double lastFormula = Convert.ToDouble(mFormula);
                    string mProduction = table.Rows[i]["Production"].ToString().Trim();
                    //if (mProduction == "")
                    //{
                    //    mProduction = "0";
                    //}
                    double lastProduction = Convert.ToDouble(mProduction);
                    double mConsumption = Convert.ToDouble((lastFormula / lastProduction).ToString("0.00"));
                    //string lastConsumption = Convert.ToString(mConsumption);
                    table.Rows[i]["Consumption"] = mConsumption;
                }
                if (table.Rows[i]["NodeType"].ToString() == "leafnode" && (table.Rows[i]["Production"].ToString().Trim() == "0.00" || table.Rows[i]["Production"].ToString().Trim() == ""))
                {
                    string mConsumption = "";
                    table.Rows[i]["Consumption"] = mConsumption;
                }
                //string firstName = table.Rows[i]["Name"].ToString().Trim();
                //string secondName = table.Rows[i + 1]["Name"].ToString().Trim();                
            }
            for (int i = 0; i < table.Rows.Count; )
            {
                string m_Name = table.Rows[i]["Name"].ToString();
                DataRow[] m_SubRoot = table.Select("Name = '" + m_Name + "'");
                int length = m_SubRoot.Length;
                double sumProduction = 0;
                double sumFormula = 0;
                double sumConsumption = 0;
                for (int j = 0; j < length; j++)
                {
                    string mmProduction = m_SubRoot[j]["Production"].ToString().Trim();
                    if (mmProduction == "")
                    {
                        mmProduction = "0";
                    }
                    double m_Prodcution = Convert.ToDouble(mmProduction);
                    sumProduction = sumProduction + m_Prodcution;
                    string mmFormula = m_SubRoot[j]["Formula"].ToString().Trim();
                    if (mmFormula == "")
                    {
                        mmFormula = "0";
                    }
                    double m_formula = Convert.ToDouble(mmFormula);
                    sumFormula = sumFormula + m_formula;
                    string mmConsumption = m_SubRoot[j]["Consumption"].ToString().Trim();
                    if (mmConsumption == "")
                    {
                        mmConsumption = "0";
                    }
                    double m_consumption = Convert.ToDouble(mmConsumption);
                    sumConsumption = sumConsumption + m_consumption;
                }
                table.Rows[i]["Production"] = sumProduction;
                table.Rows[i]["Formula"] = sumFormula;
                table.Rows[i]["Consumption"] = Convert.ToDouble((sumFormula / sumProduction).ToString("0.00"));
                i = i + length;
            }
            return table;
        }
    }
}
