
var m_organizationId = '';
//没有平衡前的数据
var m_originalData;
//投入总产量
var m_inputValue = 0.00;
//产出总产量
var m_outputValue = 0.00;
//logId
var m_logId;
//类型：新建或者编辑
var m_LogType;

$(function () {
    //initDatatime();
    
    LoadDataGrid('first');
    $('#startTime').datebox({
        disabled: true
    });
    LoadMaterialKinds("first");
    LoadBalanceMaterial("first");
});
function initLocation() {
    var dataSend = {};
    dataSend.organizationId = m_organizationId;
    $.ajax({
        type: "POST",
        url: "BalanceWeightBalanceLog.aspx/GetLocation",
        data: JSON.stringify(dataSend),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var myData = JSON.parse(msg.d);
            if (myData.length == 0) {
                $('#LocationID').combobox('loadData', []);
            }
            else {
                $('#LocationID').combobox('loadData', myData);
            }
        }
    });
}
//查询平衡日志
function QueryLogData() {
    var dataSend = {};
    var locationId = $('#LocationID').combobox('getValue');
    if ('' == locationId) {
        return;
    }
    dataSend.organizationId = m_organizationId;
    dataSend.locationId = locationId;
    $.ajax({
        type: "POST",
        url: "BalanceWeightBalanceLog.aspx/GetLogInfo",
        data: JSON.stringify(dataSend),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            LoadDataGrid("last",JSON.parse(msg.d));
        }
    });
}

// 目录树选择事件
function onOrganisationTreeClick(node) {
    // 更新当前分厂名
    $('#organizationName').textbox('setText', node.text);
    m_organizationId = node.OrganizationId;
    initDatatime();
    //初始化货位combobox
    initLocation();
}
//初始化日期时间框
function initDatatime() {
    var dataSend = {};
    dataSend.organizationId = m_organizationId;
    $.ajax({
        type: "POST",
        url: "BalanceWeightBalanceLog.aspx/GetLastBalanceLogTime",
        data: JSON.stringify(dataSend),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            if (msg.d == "") {
                $.messager.alert("提示", "没有平衡日志");
                $('#startTime').datebox('setValue', "");
            }
            else {
                $('#startTime').datebox('setValue', msg.d);
            }
        }
    });
    var endTime = new Date();
    //var startTime = new Date();
    //startTime.setDate(startTime.getDate() - 1);
    var endTimeStr = myDateFormat(endTime);
    //var startTimeStr = myDateFormat(startTime);
    $('#endTime').datebox('setValue', endTimeStr);
   // $('#startTime').datebox('setValue', startTimeStr);
}

function editInfo(isInsert, materialId) {
   // m_editMaterialId = materialId;
    //如果为插入
    if (isInsert) {
        //TODO...
    }
        //
    else {
        GetMaterialKinds();
        $('#materialKindsEdit').window('open');
    }
}
//加载物料种类
function GetMaterialKinds() {
    var dataSend = {};
    dataSend.organizationId = m_organizationId;
    $.ajax({
        type: "POST",
        url: "BalanceWeightBalanceLog.aspx/GetMaterualKind",
        data: JSON.stringify(dataSend),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            LoadMaterialKinds("last",JSON.parse(msg.d));
        }
    });
}

function myDateFormat(date) {
    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    var d = date.getDate();
    return y + '-' + (m < 10 ? ('0' + m) : m) + '-' + (d < 10 ? ('0' + d) : d);
}

function LoadMaterialKinds(type, myData) {
    if ('first' == type) {
        $('#dgMaterialKinds').datagrid({
            //data: myData,
            singleSelect: true, rownumbers: true, striped: true, border: true,// toolbar: '#tb',
            fit: true,
            //toolbar: "#toolbar",
            columns: [[
                        {
                            field: 'VariableId', title: '物料编号', width: '90', align: 'center'
                        },
                        {
                            field: 'Name', title: '名称', width: '90', align: 'center'
                        },
                        {
                            field: 'OperateColumn', title: '操作', width: '90', align: 'center',
                            formatter: function (val, row) {
                                return '<a href="#" onclick="editBalance(\''+row.VariableId+'\',\' ' + row.Name + '\');">平衡</a>';
                            }
                        }
            ]]
        })
    }
    else {
        $('#dgMaterialKinds').datagrid("loadData", myData);
    }

}
function LoadBalanceMaterial(type, myData) {
    if ('first' == type) {
        $('#balanceEdit').datagrid({
            //data: myData,
            singleSelect: true, rownumbers: true, striped: true, border: true,// toolbar: '#tb',
            fit: true,
            toolbar: "#balanceToolBar",
            onClickRow: onClickRow,
            columns: [[
                        {
                            field: 'TimeStamp', title: '时间', width: '90', align: 'center'
                        },
                        {
                            field: 'TotalPeakValleyFlatB', title: '总量', width: '90', align: 'center'
                        },
                        {
                            field: 'VariableId', title: '变量', width: '90', align: 'center'
                        },
                        {
                            field: 'Type', title: '类型', width: '90', align: 'center'
                        }
                        ,
                        {
                            field: 'FirstB', title: '甲班', width: '90', align: 'center', editor: { type: 'numberbox', options: { precision: 2 } }
                        },
                        {
                            field: 'SecondB', title: '乙班', width: '90', align: 'center', editor: { type: 'numberbox', options: { precision: 2 } }
                        },
                        {
                            field: 'ThirdB', title: '丙班', width: '90', align: 'center', editor: { type: 'numberbox', options: { precision: 2 } }
                        },
                        {
                            field: 'PeakB', title: '峰期', width: '90', align: 'center', editor: { type: 'numberbox', options: { precision: 2} }
                        },
                        {
                            field: 'ValleyB', title: '谷期', width: '90', align: 'center', editor: { type: 'numberbox', options: { precision: 2 } }
                        },
                        {
                            field: 'FlatB', title: '平期', width: '90', align: 'center', editor: { type: 'numberbox', options: { precision: 2 } }
                        }
            ]]
        })
    }
    else {
        $('#balanceEdit').datagrid("loadData", myData);
    }

}

//编辑
var editIndex = undefined;
function endEditing() {
    if (editIndex == undefined) { return true }
    if ($('#balanceEdit').datagrid('validateRow', editIndex)) {
        var edFirstB = $('#balanceEdit').datagrid('getEditor', { index: editIndex, field: 'FirstB' });
        var FirstB = $(edFirstB.target).numberbox('getValue');
        var edSecondB = $('#balanceEdit').datagrid('getEditor', { index: editIndex, field: 'SecondB' });
        var SecondB = $(edSecondB.target).numberbox('getValue');
        var edThirdB = $('#balanceEdit').datagrid('getEditor', { index: editIndex, field: 'ThirdB' });
        var ThirdB = $(edThirdB.target).numberbox('getValue');
        $('#balanceEdit').datagrid('getRows')[editIndex]['TotalPeakValleyFlatB'] = (Number(FirstB) + Number(SecondB) + Number(ThirdB)).toFixed(2).toString();
        $('#balanceEdit').datagrid('endEdit', editIndex);
        editIndex = undefined;

        //计算投入总产量、产出总产量和库存
        if (m_type == "Input") {
            m_inputValue = m_inputValue + (Number(FirstB) + Number(SecondB) + Number(ThirdB) - m_oldValue);
            $("#inputValue").numberbox("setValue", m_inputValue);
            //var stocksValue = $("#stocks").numberbox("getValue");
            $("#stocks").numberbox("setValue", (Number($("#stocks").numberbox("getValue")) + (Number(FirstB) + Number(SecondB) + Number(ThirdB) - m_oldValue)));
        }
        else if (m_type == "Output") {
            m_outputValue = m_outputValue + (Number(FirstB) + Number(SecondB) + Number(ThirdB) - m_soldValue);
            $("#outputValue").numberbox("setValue", m_outputValue);
            //var stocksValue = $("#stocks").numberbox("getValue");
            $("#stocks").numberbox("setValue", Number($("#stocks").numberbox("getValue")) - (Number(FirstB) + Number(SecondB) + Number(ThirdB) - m_soldValue));
        }

        return true;
    } else {
        return false;
    }
}

//保存修改
function saveFun() {
    endEditing();           //关闭正在编辑

    var changeRows = getChanges();
    if (changeRows.length == 0) {
        alert('没有修改的数据！');
        return;
    }
    var diffValue = getDiffValue(changeRows);
    accept();
    //var organizationId = $('#organizationId').val();
    //var m_DataGridData = $('#gridMain_ReportTemplate').treegrid('getData');
    //var m_DataGridData = $('#gridMain_ReportTemplate').datagrid('getChanges', 'updated');
    //for (var i = 0; i < m_DataGridData.length; i++) {
    //    m_DataGridData[i]['children'] = [];
    //}
    // if (m_DataGridData.length > 0) {
    var m_DataGridDataJson = JSON.stringify(changeRows);
    var diffValueData = JSON.stringify(diffValue);
    var startTime = $('#startTime').datebox('getValue');
    var endTime = $('#endTime').datebox('getValue');
    var stocksValue = $("#stocks").numberbox("getValue");
    var inputValue = $("#inputValue").numberbox("getValue");
    var outputValue = $("#outputValue").numberbox("getValue");
    var productionZoneId = $('#LocationID').combobox('getValue');

    $.ajax({
        type: "POST",
        url: "BalanceWeightBalanceLog.aspx/SaveData",
        data: "{organizationId:'" + m_organizationId + "',datagridData:'" + m_DataGridDataJson + "',diffValueData:'" + diffValueData + "',productionZoneId:'" + productionZoneId +
           "',startTime:'" + startTime + "',endTime:'" + endTime + "',stocksValue:'" + stocksValue + "',inputValue:'" + inputValue +
           "',outputValue:'" + outputValue + "',logId:'" + m_logId + "',type:'" + m_LogType + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_Msg = msg.d;
            $.messager.alert('提示', m_Msg);
        }
    });
    //}
    //else {
    //    $.messager.alert('提示', '没有任何数据需要保存');
    //}
}


//获取变化之后和变化之前的差值
function getDiffValue(changRows) {
    var diffValue = {};// = changRows.concat();
    extend(diffValue, changRows);
    for (var i = 0; i < changRows.length; i++) {
        for (var j = 0; j < m_originalData.rows.length; j++) {
            if (changRows[i]['VariableItemId'] == m_originalData.rows[j]['VariableItemId']) {
                diffValue[i]['TotalPeakValleyFlatB'] = (Number(changRows[i]['TotalPeakValleyFlatB']) - Number(m_originalData.rows[j]['TotalPeakValleyFlatB'])).toString();
                diffValue[i]['MorePeakB'] = (Number(changRows[i]['MorePeakB']) - Number(m_originalData.rows[j]['MorePeakB'])).toString();
                diffValue[i]['PeakB'] = (Number(changRows[i]['PeakB']) - Number(m_originalData.rows[j]['PeakB'])).toString();
                diffValue[i]['ValleyB'] =( Number(changRows[i]['ValleyB']) - Number(m_originalData.rows[j]['ValleyB'])).toString();
                diffValue[i]['MoreValleyB'] = (Number(changRows[i]['MoreValleyB']) - Number(m_originalData.rows[j]['MoreValleyB'])).toString();
                diffValue[i]['FlatB'] =( Number(changRows[i]['FlatB']) - Number(m_originalData.rows[j]['FlatB'])).toString();
                diffValue[i]['FirstB'] = (Number(changRows[i]['FirstB']) - Number(m_originalData.rows[j]['FirstB'])).toString();
                diffValue[i]['SecondB'] =( Number(changRows[i]['SecondB']) - Number(m_originalData.rows[j]['SecondB'])).toString();
                diffValue[i]['ThirdB'] =( Number(changRows[i]['ThirdB']) - Number(m_originalData.rows[j]['ThirdB'])).toString();
            }
        }
    }

    return diffValue;
}

function getChanges() {
    var rows = $('#balanceEdit').datagrid('getChanges');//获取最后一次提交以来更改的行，type 参数表示更改的行的类型，可能的值是：inserted、deleted、updated，等等。
    //当 type 参数没有分配时，返回所有改变的行。
    
    return rows;
}
function accept() {
    if (endEditing()) {
        $('#balanceEdit').datagrid('acceptChanges');//提交自从被加载以来或最后一次调用 acceptChanges 以来所有更改的数据。
    }
}

//
var m_oldValue = 0.00;
var m_type = undefined;
function onClickRow(index, rowData) {

    //var datagridData = $("#balanceEdit").datagrid('getData');
    //var oldValue=
    //var oldValue = Number(rowData["TotalPeakValleyFlatB"]);
    //var type=rowData["Type"];
    if (editIndex != index) {
        if (endEditing()) {
            $('#balanceEdit').datagrid('selectRow', index)
                    .datagrid('beginEdit', index);
            editIndex = index;
            m_oldValue = Number(rowData["TotalPeakValleyFlatB"]);
            m_type = rowData["Type"];
        } else {
            $('#balanceEdit').datagrid('selectRow', editIndex);
        }
    }
}

//撤销修改
function reject() {
    if (editIndex != undefined) {
        $('#balanceEdit').datagrid('cancelEdit', editIndex);
        editIndex = undefined;
    }
}

function CreatBalanceLog() {
   // m_LogType = 'insert';
    var locationId = $('#LocationID').combobox("getValue");
    var starttime = $('#startTime').datebox('getValue');
    var endtime = $('#endTime').datebox('getValue');
    editBalance(locationId, starttime, endtime, 'insert');
}

//平衡
function editBalance(locationId,startTime,endTime, type,logId) {
    m_logId = logId;
    m_LogType = type;
    

    var dataSend = {};
    
    dataSend.startTime = startTime; //$('#startTime').datebox('getValue');
    dataSend.endTime = endTime;//$('#endTime').datebox('getValue');
    dataSend.locationId = locationId;
    dataSend.organizationId = m_organizationId;

    //计算库存量
    $.ajax({
        type: "POST",
        url: "BalanceWeightBalanceLog.aspx/CalculateStocks",
        data: JSON.stringify(dataSend),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $("#stocks").numberbox("setValue",msg.d);
        }
    });

    $.ajax({
        type: "POST",
        url: "BalanceWeightBalanceLog.aspx/GetBalanceData",
        data: JSON.stringify(dataSend),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            m_originalData = JSON.parse(msg.d);
            LoadBalanceMaterial("last", JSON.parse(msg.d));
        }
    });
    $('#balanceWindow').window('open');
}

function LoadDataGrid(type, myData) {
    if ('first' == type) {
        $('#dataGrid').datagrid({
            //data: myData,
            singleSelect: true, rownumbers: true, striped: true, border: true,// toolbar: '#tb',
            fit: true,
            toolbar: "#toolbar",
            columns: [[
                        {
                            field: 'LogId', title: '日志ID', width: '90', align: 'center',hidden:true
                        },
                        {
                            field: 'VariableItemId', title: 'VariableItemId', width: '90', align: 'center', hidden: true
                        },
                        {
                            field: 'LocationID', title: '货位ID', width: '90', align: 'center', hidden: true
                        },
                        {
                            field: 'LocationName', title: '货位', width: '90', align: 'center'
                        },
                        {
                            field: 'StartTime', title: '开始时间', width: '90', align: 'center'
                        },
                        {
                            field: 'EndTime', title: '结束时间', width: '90', align: 'center'
                        },
                        {
                            field: 'WareStoreValue', title: '期末库存量', width: '90', align: 'center'
                        },
                        {
                            field: 'OffsetInputValue', title: '投入总平衡数', width: '90', align: 'center'
                        },
                        {
                            field: 'OffsetOutputValue', title: '产出总平衡数', width: '90', align: 'center'
                        },
                        {
                            field: 'Creator', title: '编辑人', width: '90', align: 'center'
                        },
                        {
                            field: 'CreateTime', title: '编辑时间', width: '90', align: 'center'
                        },

                        {
                            field: 'Remark', title: '备注', width: '90', align: 'center'
                        },
                        {
                            field: 'OperateColumn', title: '操作', width: '90', align: 'center',
                            formatter: function (val, row) {
                                return '<a href="#" onclick="editBalance(\'' + row.LocationID + '\',\'' + row.StartTime + '\',\'' + row.EndTime + '\',\'edut\',\'' + row.LogId + '\');">编辑</a>';
                            }
                        }
            ]]
        })
    }
    else {
        $('#dataGrid').datagrid("loadData", myData);
    }
}



//实现深复制
function getType(o) {
    var _t;
    return ((_t = typeof (o)) == "object" ? o == null && "null" || Object.prototype.toString.call(o).slice(8, -1) : _t).toLowerCase();
}
function extend(destination, source) {
    for (var p in source) {
        if (getType(source[p]) == "array" || getType(source[p]) == "object") {
            destination[p] = getType(source[p]) == "array" ? [] : {};
            arguments.callee(destination[p], source[p]);
        }
        else {
            destination[p] = source[p];
        }
    }
}
