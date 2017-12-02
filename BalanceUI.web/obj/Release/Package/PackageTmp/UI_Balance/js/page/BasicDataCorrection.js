var editIndex = undefined;
var OrganizationId = "";
var OrganizationType = "";
var SelectedTargetId = "";
var PlotChartObj = undefined;
$(function () {
    InitializeDateTimePickers();
    InitializeCorrectionGrid("CorrectionTags", { "rows": [], "total": 0 });
    InitializeDataTagsGrid("SelectTagsList", { "rows": [], "total": 0 });
    InitializeTrendTagsGrid("TrendTagsList", { "rows": [], "total": 0 });
    LoadDataCorrectionDialog();
    LoadTagsSelectDialog();
    LoadAlarmInfoDialog();
    LoadSelectedTrendTags();
<<<<<<< HEAD
    InitializeAlarmTypeData();
=======
>>>>>>> 2856e1c547f65348950fe227c9a829f8bf44b569
    initPageAuthority();
});
function LoadDataCorrectionDialog() {
    $('#dlg_CorrectionData').dialog({
        title: '数据修正对话框',
        left: 200,
        top: 10,
        width: 750,
        height: 500,
        closed: true,
        cache: false,
        modal: true,
        iconCls: 'icon-edit',
        resizable: false
    });
}
function LoadTagsSelectDialog() {
    $('#dlg_SelectDataTags').dialog({
        title: '标签筛选对话框',
        left: 200,
        top: 10,
        width: 450,
        height: 450,
        closed: true,
        cache: false,
        modal: true,
        iconCls: 'icon-search',
        resizable: false
    });
}
function LoadSelectedTrendTags() {
    $('#dlg_SelectedTrendTags').dialog({
        title: '趋势标签',
        left: 200,
        top: 10,
        width: 400,
        height: 450,
        closed: true,
        cache: false,
        modal: true,
        iconCls: 'icon-search',
        resizable: false
    });
}
function LoadAlarmInfoDialog() {
    $('#dlg_AlarmInfo').dialog({
        title: '报警记录',
        left: 400,
        top: 10,
        width: 660,
        height: 450,
        closed: true,
        cache: false,
        modal: true,
        iconCls: 'icon-search',
        resizable: false
    });
}
// 目录树选择事件
function onOrganisationTreeClick(node) {
    // 更新当前分厂名
    $('#Textbox_OrganizationName').textbox('setText', node.text);
    OrganizationId = node.OrganizationId;
    OrganizationType = node.OrganizationType;
    ClearTags('dataGrid_CorrectionTags');
}
//初始化页面的增删改查权限
function initPageAuthority() {
    $.ajax({
        type: "POST",
        url: "BasicDataCorrection.aspx/AuthorityControl",
        data: "",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,//同步执行
        success: function (msg) {
            PageOpPermission = msg.d;
            ////增加
            //if (PageOpPermission[1] == '0') {
            //    $("#btnAdd").linkbutton('disable');
            //}
            //修改
<<<<<<< HEAD
            if (PageOpPermission[2] == '0') {
=======
            if (authArray[2] == '0') {
>>>>>>> 2856e1c547f65348950fe227c9a829f8bf44b569
                $("#adjust").linkbutton('disable');
            }
            //删除
            //if (PageOpPermission[3] == '0') {
            //    $("#id_deleteAll").linkbutton('disable');
            //}
        }
    });
}
function InitializeDateTimePickers() {
    var m_DateTime = new Date();
    var m_NowStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
    m_DateTime.setDate(m_DateTime.getDate() - 1);
    var m_YestedayStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
    $('#datetimebox_TrendStartTimeF').datetimebox('setValue', m_YestedayStr);
    $('#datetimebox_TrendEndTimeF').datetimebox('setValue', m_NowStr);
    $('#datetimebox_CorrectionStartTime').datetimebox('setValue', m_YestedayStr);
    $('#datetimebox_CorrectionEndTime').datetimebox('setValue', m_NowStr);
    $('#datetimebox_AlarmStartTimeF').datetimebox('setValue', m_YestedayStr);
    $('#datetimebox_AlarmEndTimeF').datetimebox('setValue', m_NowStr);
}
function SetChartContentSize(width, height) {
    $('#LineTrend_Legend').css('width', width - 5);
    $('#LineTrend_Content').css('width', width - 30);
    $('#LineTrend_Content').css('height', height - 35);
}

function InitializeDataTagsGrid(myObjId, myData) {
    $('#dataGrid_' + myObjId).datagrid({
        title: '',
        data: myData,
        dataType: "json",
        striped: true,
        //loadMsg: '',   //设置本身的提示消息为空 则就不会提示了的。这个设置很关键的
        rownumbers: true,
        singleSelect: true,
        idField: 'id',
        fit: true,
        columns: [[{
            width: '110',
            title: '数据项ID',
            field: 'id',
            hidden: true
        }, {
            width: '230',
            title: '标签名称',
            field: 'text'
        }, {
            width: '80',
            title: '字段名称',
            field: 'FieldName'
        }, {
            width: '65',
            title: '对象类型',
            field: 'Type'
        }]],
        onDblClickRow: function (rowIndex, rowData) {
            AddTagsToTargetDataGrid(rowData);
        },
        toolbar: '#toolbar_' + myObjId
    });
}
function InitializeCorrectionGrid(myObjId, myData) {
    $('#dataGrid_' + myObjId).datagrid({
        title: '',
        data: myData,
        dataType: "json",
        striped: true,
        //loadMsg: '',   //设置本身的提示消息为空 则就不会提示了的。这个设置很关键的
        rownumbers: true,
        singleSelect: true,
        idField: 'id',
        fit: true,
        columns: [[{
            width: '110',
            title: '数据项ID',
            field: 'id',
            hidden: true
        }, {
            width: '160',
            title: '数据项名称',
            field: 'text'
        }, {
            width: '80',
            title: '字段名称',
            field: 'FieldName'
        }, {
            width: '80',
            title: '修改对象',
            field: 'Type'
        }, {
            width: '120',
            title: '平均值',
            field: 'AvgValue'
        }, {
            width: '120',
            title: '修正值',
            field: 'CorrectionValue',
            editor: {
                type: 'numberbox',
                options: {
                    precision: 2
                }
            }
        }]],
        onClickCell: onClickCell,
        onDblClickRow: function (rowIndex, rowData) {
            $('#dataGrid_' + myObjId).datagrid('deleteRow', rowIndex);
        }
    });
}
function endEditing() {
    if (editIndex == undefined) { return true }
    if ($('#dataGrid_CorrectionTags').datagrid('validateRow', editIndex)) {
        $('#dataGrid_CorrectionTags').datagrid('endEdit', editIndex);

        //MathSumColumnsValue('grid_ProductionPlanInfo', editIndex);             //计算合计列
        editIndex = undefined;
        return true;
    } else {
        return false;
    }
}

function onClickCell(index, field) {
    if (endEditing()) {
        //var m_Rows = $('#grid_ProductionPlanInfo').datagrid("getRows")
        //var m_Formula = m_Rows[index]["StatisticalFormula"];         //屏蔽根据行的内容进行计算的行，这些行自动改变值
        //if (m_Formula.indexOf("{Line|") == -1) {
        $('#dataGrid_CorrectionTags').datagrid('selectRow', index)
                        .datagrid('editCell', { index: index, field: field });
        editIndex = index;
        //
        //     SelectedTagValue = GetTagValue('grid_ProductionPlanInfo', index);
        //}
    }
}
function InitializeTrendTagsGrid(myObjId, myData) {
    $('#dataGrid_' + myObjId).datagrid({
        title: '',
        data: myData,
        dataType: "json",
        striped: true,
        //loadMsg: '',   //设置本身的提示消息为空 则就不会提示了的。这个设置很关键的
        rownumbers: true,
        singleSelect: true,
        idField: 'id',
        fit: true,
        columns: [[{
            width: '110',
            title: '数据项ID',
            field: 'id',
            hidden: true
        }, {
            width: '200',
            title: '标签名称',
            field: 'text'
        }, {
            width: '65',
            title: '字段名称',
            field: 'FieldName'
        }, {
            width: '65',
            title: '对象类型',
            field: 'Type'
        }]],
        onDblClickRow: function (rowIndex, rowData) {
            $('#dataGrid_' + myObjId).datagrid('deleteRow', rowIndex);
        },
        toolbar: '#toolbar_' + myObjId
    });
}
////////////////////获得报警列表///////////////////////
function InitializeAlarmTypeData() {
    $.ajax({
        type: "POST",
        url: 'BasicDataCorrection.aspx/GetAlarmType',
        data: "",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d)['rows'];
            var m_ResultData = [];
            m_ResultData.push({ "id": "All", "text": "全部" });
            if (m_MsgData != undefined && m_MsgData != null && m_MsgData.length > 0) {
                for (var i = 0; i < m_MsgData.length; i++) {
                    m_ResultData.push(m_MsgData[i]);
                }
            }
            $('#combobox_AlarmTypeF').combobox('loadData', m_ResultData);
            $('#combobox_AlarmTypeF').combobox("setValue", m_ResultData[0].id);
        },
        error: function () {
        }
    });
}
function SetTrendTime(myRowData) {
    var m_StartTime = myRowData["StartTime"].replace(/\//g, "-");
    var m_EndTime = myRowData["EndTime"].replace(/\//g, "-");
    if (m_StartTime == "") {
        alert("无报警开始时间!");
    }
    else {
        if(m_EndTime == "")
        {
            $('#datetimebox_TrendStartTimeF').datetimebox('setValue', m_StartTime);
            var m_DateTime = new Date();
            var m_NowStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
            $('#datetimebox_TrendEndTimeF').datetimebox('setValue', m_NowStr);
        }
        else
        {
            $('#datetimebox_TrendStartTimeF').datetimebox('setValue', m_StartTime);
            $('#datetimebox_TrendEndTimeF').datetimebox('setValue', m_EndTime);
        }
    }
}
function AddTagsToTargetDataGrid(myValue) {
    var m_DataRows = $('#' + SelectedTargetId).datagrid("getRows");
    var m_TagAlreadyExist = false;
    for (var i = 0; i < m_DataRows.length; i++) {
        if (myValue["id"] == m_DataRows[i]["id"]) {
            m_TagAlreadyExist = true;
            break;
        }
    }
    if (m_TagAlreadyExist == true) {
        alert("列表中已存在该标签!");
    }
    else {
        var m_TagsDetailTemp = [];
        m_TagsDetailTemp["id"] = myValue["id"];
        m_TagsDetailTemp["text"] = myValue["text"];
        m_TagsDetailTemp["Type"] = myValue["Type"];
        m_TagsDetailTemp["FieldName"] = myValue["FieldName"];
        m_TagsDetailTemp["AvgValue"] = myValue["AvgValue"];
        m_TagsDetailTemp["CorrectionValue"] = myValue["CorrectionValue"];

        $('#' + SelectedTargetId).datagrid("appendRow", m_TagsDetailTemp);
        alert("标签添加成功!");
    }
}
function AddAllTags() {
    var m_DataRows = $('#' + SelectedTargetId).datagrid("getRows");
    var m_AllTagsRows = $('#dataGrid_SelectTagsList').datagrid("getRows");
    for (var j = 0; j < m_AllTagsRows.length; j++) {
        var m_TagAlreadyExist = false;
        var m_Value = m_AllTagsRows[j];
        for (var i = 0; i < m_DataRows.length; i++) {
            if (m_Value["id"] == m_DataRows[i]["id"]) {
                m_TagAlreadyExist = true;
                break;
            }
        }
        if (m_TagAlreadyExist == false) {
            var m_TagsDetailTemp = [];
            m_TagsDetailTemp["id"] = m_Value["id"];
            m_TagsDetailTemp["text"] = m_Value["text"];
            m_TagsDetailTemp["Type"] = m_Value["Type"];
            m_TagsDetailTemp["FieldName"] = m_Value["FieldName"];
            m_TagsDetailTemp["AvgValue"] = m_Value["AvgValue"];
            m_TagsDetailTemp["CorrectionValue"] = m_Value["CorrectionValue"];
            $('#' + SelectedTargetId).datagrid("appendRow", m_TagsDetailTemp);
        }
    }
}
function GetDateTimeString(myDateTimeValue) {
    var m_DateTimeString = myDateTimeValue.substr(0, 4) + "-" + myDateTimeValue.substr(4, 2) + "-" + myDateTimeValue.substr(6, 2)
                           + " " + myDateTimeValue.substr(8, 2) + ":" + myDateTimeValue.substr(10, 2) + ":" + myDateTimeValue.substr(12, 2);
    return m_DateTimeString;
}
function GetDataTrend(rowIndex, field, value) {
    if (field != "id" && field != "text" && field != "Type" && field != "AvgValue") {
        var m_DataRows = $('#dataGrid_BasicDataCorrection').datagrid("getRows");

        //SelectedTrendTags = [];           //首先清空列表

        //var m_TrendTagDetail = [];
        //m_TrendTagDetail["id"] = m_DataRows[rowIndex]["id"];
        //m_TrendTagDetail["text"] = m_DataRows[rowIndex]["text"];
        //m_TrendTagDetail["Type"] = m_DataRows[rowIndex]["Type"];
        //m_TrendTagDetail["FieldName"] = m_DataRows[rowIndex]["FieldName"];
        //SelectedTrendTags.push(m_TrendTagDetail);
        //var m_TrendTagText = "";
        //for (var i = 0; i < SelectedTrendTags.length; i++) {
        //    if (i == 0) {
        //        m_TrendTagText = SelectedTrendTags[i].text;
        //    }
        //    else {
        //        m_TrendTagText = m_TrendTagText + "," + SelectedTrendTags[i].text;
        //    }
        //}

        var m_CorrentDateString = GetDateTimeString(field);
        m_CorrentDateString = m_CorrentDateString.replace(/-/g, "/");
        var m_DateTime = new Date(m_CorrentDateString);
        var m_NowStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
        m_DateTime.setDate(m_DateTime.getDate() - 1);
        var m_YestedayStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
        $('#datetimebox_TrendStartTimeF').datetimebox('setValue', m_YestedayStr);
        $('#datetimebox_TrendEndTimeF').datetimebox('setValue', m_NowStr);
        $('#datetimebox_CorrectionStartTime').datetimebox('setValue', m_YestedayStr);
        $('#datetimebox_CorrectionEndTime').datetimebox('setValue', m_NowStr);


        $('#dlg_CorrectionData').dialog('open');
    }
}
function SelectTags(mySelectedTargetId) {
    if (OrganizationId == "") {
        alert("请选择分厂!");
    }
    else {
        SelectedTargetId = mySelectedTargetId;
        var m_DataTagsFilter = $('#Combobox_DataTagsFilter').combobox('getValue');
        if (m_DataTagsFilter == "Ammeter") {
            GetAmmeterTagsList();
        }
        else if (m_DataTagsFilter == "DCS") {
            GetDCSTagsList();
        }
        $('#dlg_SelectDataTags').dialog('open');
    }
}
function ClearTags(mySelectedTargetId) {
    $('#' + mySelectedTargetId).datagrid("loadData", { "rows": [], "total": 0 });
}
function EditTrendTags() {
    if (OrganizationId == "") {
        alert("请选择分厂!");
    }
    else {
        $('#dlg_SelectedTrendTags').dialog('open');
    }
}
function ChangeTagsList(myRecord) {
    if (myRecord.Id == "Ammeter") {
        GetAmmeterTagsList();
    }
    else if (myRecord.Id == "DCS") {
        GetDCSTagsList();
    }
}
function GetAmmeterTagsList() {
    var win = $.messager.progress({
        title: '请稍后',
        msg: '数据载入中...'
    });
    $.ajax({
        type: "POST",
        url: 'BasicDataCorrection.aspx/GetAmmeterTags',
        data: "{myOrganizationId:'" + OrganizationId + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData != null && m_MsgData != undefined) {
                $('#dataGrid_SelectTagsList').datagrid("loadData", m_MsgData);
            }
            $.messager.progress('close');
        },
        error: function () {
            $.messager.progress('close');
            $.messager.alert('错误', '数据载入失败！');
        }
    });
}
function GetDCSTagsList() {
    var win = $.messager.progress({
        title: '请稍后',
        msg: '数据载入中...'
    });
    $.ajax({
        type: "POST",
        url: 'BasicDataCorrection.aspx/GetDCSTags',
        data: "{myOrganizationId:'" + OrganizationId + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData != null && m_MsgData != undefined) {
                $('#dataGrid_SelectTagsList').datagrid("loadData", m_MsgData);
            }
            $.messager.progress('close');
        },
        error: function () {
            $.messager.progress('close');
            $.messager.alert('错误', '数据载入失败！');
        }
    });
}
function DisplayTrend() {
    var m_TrendTagRows = $('#dataGrid_TrendTagsList').datagrid("getRows");
    var m_StartTime = $('#datetimebox_TrendStartTimeF').datetimebox('getValue');
    var m_EndTime = $('#datetimebox_TrendEndTimeF').datetimebox('getValue');

    var m_AmmeterFieldNames = "";
    var m_DCSFieldNames = "";

    for (var i = 0; i < m_TrendTagRows.length; i++) {
        if (m_TrendTagRows[i]["Type"] == "Ammeter") {
            if (m_AmmeterFieldNames == "") {
                m_AmmeterFieldNames = m_TrendTagRows[i]["text"] + ',' + m_TrendTagRows[i]["FieldName"];
            }
            else {
                m_AmmeterFieldNames = m_AmmeterFieldNames + ';' + m_TrendTagRows[i]["text"] + ',' + m_TrendTagRows[i]["FieldName"];
            }
        }
        else if (m_TrendTagRows[i]["Type"] == "DCS") {
            if (m_DCSFieldNames == "") {
                m_DCSFieldNames = m_TrendTagRows[i]["text"] + ',' + m_TrendTagRows[i]["FieldName"];
            }
            else {
                m_DCSFieldNames = m_DCSFieldNames + ';' + m_TrendTagRows[i]["text"] + ',' + m_TrendTagRows[i]["FieldName"];
            }
        }
    }
    var m_ChartContentWidth = $('#ChartContentDiv').layout('panel', 'center').panel('options').width;
    var m_ChartContentheight = $('#ChartContentDiv').layout('panel', 'center').panel('options').height;
    SetChartContentSize(m_ChartContentWidth, m_ChartContentheight);
    if (OrganizationId == "") {
        alert("请选择分厂!");
    }
    else if (m_AmmeterFieldNames == "" && m_DCSFieldNames == "") {
        alert("请添加趋势标签!");
    }
    else {
        $.ajax({
            type: "POST",
            url: 'BasicDataCorrection.aspx/GetTrend',
            data: "{myOrganizationId:'" + OrganizationId + "',myStartTime:'" + m_StartTime + "',myEndTime:'" + m_EndTime
                + "',myAmmeterFieldNames:'" + m_AmmeterFieldNames + "',myDCSFieldNames:'" + m_DCSFieldNames + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                //GetLineChart({ "rows": [], "total": 0 });
                if (m_MsgData != null && m_MsgData != undefined) {
                    GetLineChart(m_MsgData);
                }
                $.messager.progress('close');
            },
            error: function () {
                $.messager.progress('close');
                $.messager.alert('错误', '数据载入失败！');
            }
        });
    }
}
function CorrectionDataByTags() {
    endEditing();           //关闭正在编辑
    var m_AllTagsValueValid = true;
    parent.$.messager.confirm('询问', '是否已经确认时间范围和数值<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;确定修改数据?', function (r) {
        if (r) {
            var m_CorrectionTagRows = $('#dataGrid_CorrectionTags').datagrid("getRows");
            var m_StartTime = $('#datetimebox_CorrectionStartTime').datetimebox('getValue');
            var m_EndTime = $('#datetimebox_CorrectionEndTime').datetimebox('getValue');
            var m_TagsInfo = "";
            if (m_CorrectionTagRows != null && m_CorrectionTagRows != undefined) {
                for (var i = 0; i < m_CorrectionTagRows.length; i++) {
                    if (m_CorrectionTagRows[i]["CorrectionValue"] == undefined || m_CorrectionTagRows[i]["CorrectionValue"] == "") {
                        m_AllTagsValueValid = false;
                        break;
                    }
                    else {
                        var m_CorrectionValue = m_CorrectionTagRows[i]["CorrectionValue"] != undefined ? m_CorrectionTagRows[i]["CorrectionValue"] : "0";
                        if (i == 0) {
                            m_TagsInfo = m_CorrectionTagRows[i]["FieldName"] + "," + m_CorrectionTagRows[i]["Type"] + "," + m_CorrectionValue;
                        }
                        else {
                            m_TagsInfo = m_TagsInfo + ";" + m_CorrectionTagRows[i]["FieldName"] + "," + m_CorrectionTagRows[i]["Type"] + "," + m_CorrectionValue;
                        }
                    }
                }
            }
            if (m_AllTagsValueValid == false) {
                alert("要修正的标签中需要全部计算出平均值和修正值!");
            }
            else if (m_CorrectionTagRows.length > 0) {
                var win = $.messager.progress({
                    title: '请稍后',
                    msg: '数据处理中...'
                });
                $.ajax({
                    type: "POST",
                    url: 'BasicDataCorrection.aspx/CorrectionDataByTags',
                    data: "{myOrganizationId:'" + OrganizationId + "',myStartTime:'" + m_StartTime + "',myEndTime:'" + m_EndTime
                        +  "',myTagsInfo:'" + m_TagsInfo + "'}",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (msg) {
                        var m_MsgData = msg.d;    //jQuery.parseJSON(msg.d);
                        if (m_MsgData != null && m_MsgData != undefined) {
                            if (m_MsgData == "1") {
                                ClearTags('dataGrid_CorrectionTags');
                                alert("修改成功!");
                            }
                            else if (m_MsgData == "0") {
                                ClearTags('dataGrid_CorrectionTags');
                                alert("无可更新数据!");
                            }
                            else {
                                alert("更新失败!");
                            }
                        }
                        $.messager.progress('close');
                    },
                    error: function () {
                        $.messager.progress('close');
                        $.messager.alert('错误', '数据载入失败！');
                    }
                });
            }
            else {
                alert("请选择修正数据的标签!");
            }
        }
    });
}

function ReCaculateBalanceData() {
    if (OrganizationId == "") {
        alert("请选择分厂!");
    }
    else {
        parent.$.messager.confirm('询问', '是否已经确认时间范围<br/>&nbsp;&nbsp;&nbsp;确定重新生成数据?', function (r) {
            if (r) {
                var m_StartTime = $('#datetimebox_CorrectionStartTime').datetimebox('getValue');
                var m_EndTime = $('#datetimebox_CorrectionEndTime').datetimebox('getValue');
                var win = $.messager.progress({
                    title: '请稍后',
                    msg: '数据处理中...'
                });
                $.ajax({
                    type: "POST",
                    url: 'BasicDataCorrection.aspx/ReGenerateDataByDay',
                    data: "{myOrganizationId:'" + OrganizationId + "',myStartTime:'" + m_StartTime + "',myEndTime:'" + m_EndTime + "'}",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (msg) {
                        var m_MsgData = msg.d;    //jQuery.parseJSON(msg.d);
                        if (m_MsgData != null && m_MsgData != undefined) {
                            if (m_MsgData == "1") {
                                alert("修改成功!");
                            }
                            else if (m_MsgData == "0") {
                                alert("无可更新数据!");
                            }
                            else {
                                alert("更新失败!");
                            }
                        }
                        $.messager.progress('close');
                    },
                    error: function () {
                        $.messager.progress('close');
                        $.messager.alert('错误', '数据载入失败！');
                    }
                });
            }
        });
    }
}
function CaculateAvgValue() {
    var m_DataGridData = $('#dataGrid_CorrectionTags').datagrid('getData');
    if (m_DataGridData['rows'].length > 0) {
        var m_DataGridDataFormat = { "rows": [], "total": m_DataGridData['rows'].length };
        for (var i = 0; i < m_DataGridData['rows'].length; i++) {
            m_DataGridDataFormat["rows"].push({ "id": m_DataGridData['rows'][i].id, "text": m_DataGridData['rows'][i].text, "FieldName": m_DataGridData['rows'][i].FieldName, "Type": m_DataGridData['rows'][i].Type })
        }
        var m_DataGridDataJson = JSON.stringify(m_DataGridDataFormat);
        var m_StartTime = $('#datetimebox_CorrectionStartTime').datetimebox('getValue');
        var m_EndTime = $('#datetimebox_CorrectionEndTime').datetimebox('getValue');
        $.ajax({
            type: "POST",
            url: "BasicDataCorrection.aspx/CaculateAvgValue",
            data: "{myOrganizationId:'" + OrganizationId + "',myStartTime:'" + m_StartTime + "',myEndTime:'" + m_EndTime + "',myDataGridData:'" + m_DataGridDataJson + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                $('#dataGrid_CorrectionTags').datagrid("loadData", m_MsgData);
            },
            error: function (error) {
                $('#dataGrid_CorrectionTags').datagrid("loadData", { "rows": [], "total": 0 });
            }
        });
    }
    else {
        alert("请选择需要调整数据的标签!");
    }
}
function AlarmInfoQuery() {
    if (OrganizationId == "") {
        alert("请选择分厂");
    }
    else {
        $('#dlg_AlarmInfo').dialog('open');
    }
}
function AlarmDetailQuery() {
    var m_StartTime = $('#datetimebox_AlarmStartTimeF').datetimebox('getValue');
    var m_EndTime = $('#datetimebox_AlarmEndTimeF').datetimebox('getValue');
    var m_AlarmType = $('#combobox_AlarmTypeF').combobox('getValue');   
    $.ajax({
        type: "POST",
        url: 'BasicDataCorrection.aspx/GetAlarmDetail',
        data: "{myOrganizationId:'" + OrganizationId + "',myAlarmType:'" + m_AlarmType + "',myStartTime:'" + m_StartTime + "',myEndTime:'" + m_EndTime + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d);
            $('#dataGrid_AlarmInfo').datagrid("loadData", m_MsgData);
        },
        error: function (error) {
            $('#dataGrid_AlarmInfo').datagrid("loadData", { "rows": [], "total": 0 });
        }
    });
}
function GetLineChart(myData) {
    var m_ChartData = [];
    var m_legendData = [];
    var m_MaxValue = 0;
    for (var i = 0; i < myData["rows"].length; i++) {
        /////////////遍历每一列数据
        var j = 0;
        m_ChartData[i] = [];
        $.each(myData["rows"][i], function (myKey, myValue) {          //遍历采集服务器
            if (j == 0) {

            }
            else if (j == 1) {
                m_legendData[i] = myValue;
            }
            else {
                var m_Date = myKey.replace('_', ' ');
                var m_PointValue = parseFloat(myValue);
                //m_ChartData[i][j - 2] = [m_Date[1] + '/' + '01/' + m_Date[0], m_PointValue];
                m_ChartData[i][j - 2] = [m_Date, m_PointValue];
                if (m_PointValue > m_MaxValue) {
                    m_MaxValue = m_PointValue;        //找出最大的纵坐标
                }
            }
            j = j + 1;
        });
    }
    if (m_MaxValue <= 1) {
        m_MaxValue = 1;
    }
    else if (m_MaxValue > 1 && m_MaxValue < 20) {
        m_MaxValue = 20;
    }
    else if (m_MaxValue >= 20 && m_MaxValue < 40) {
        m_MaxValue = 40;
    }
    else if (m_MaxValue >= 40 && m_MaxValue < 60) {
        m_MaxValue = 60;
    }
    else if (m_MaxValue >= 60 && m_MaxValue < 80) {
        m_MaxValue = 80;
    }
    else if (m_MaxValue >= 80 && m_MaxValue < 100) {
        m_MaxValue = 100;
    }
    else if (m_MaxValue >= 100 && m_MaxValue < 120) {
        m_MaxValue = 120;
    }
    else if (m_MaxValue >= 120 && m_MaxValue < 140) {
        m_MaxValue = 140;
    }
    else if (m_MaxValue >= 140 && m_MaxValue < 160) {
        m_MaxValue = 160;
    }
    else {
        m_MaxValue = m_MaxValue + 10;
    }

    //if (PlotChartObj == undefined) {

    //}
    //else {
    //    //PlotChartObj.series[0].data = m_ChartData;
    //    //PlotChartObj.data = m_ChartData;
    //    //PlotChartObj.replot();

    //    DrawLineChart("LineTrend", m_ChartData, m_MaxValue);
    //}
    DrawLineChart("LineTrend", m_ChartData, m_MaxValue);
    DrawLineChartLegend("LineTrend", m_legendData);
}
function DrawLineChartLegend(myContentId, mylegendData) {
    var m_SeriesColors = ["#01b3f9", "#fef102", "#f8000e", "#a400ed", "#aaf900", "#fe0072", "#0c6c92", "#fea002", "#c1020a", "#62008d", "#3c8300"];
    var m_LegendObjId = myContentId + "_Legend";
    var m_LegendObj = $('#' + m_LegendObjId);
    m_LegendObj.empty();
    var m_LegendHtml = '<table id="' + m_LegendObjId + '_DefinedLegendTable" style="bottom: 0px;"><tbody><tr class="jqplot-table-legend"><td id="' + m_LegendObjId + '_DefinedLegendBlankTd"></td>';
    for (var i = 0; i < mylegendData.length; i++) {
        m_LegendHtml = m_LegendHtml + '<td style="text-align: center; padding-top: 0px;" class="jqplot-table-legend jqplot-table-legend-swatch"><div class="jqplot-table-legend-swatch-outline"><div style="background-color: ' + m_SeriesColors[i] + '; border-color:  ' + m_SeriesColors[i] + '" class="jqplot-table-legend-swatch"></div></div></td><td style="padding-top: 0px; padding-right:4px; color:#555555; font-size:8pt; font-family: SimSun;">' + mylegendData[i] + '</td>';
    }
    m_LegendHtml = m_LegendHtml + '</tr></tbody></table>';
    m_LegendObj.append($(m_LegendHtml));
    var m_LegendBlankTdWidth = ($('#' + m_LegendObjId).width() - $('#' + m_LegendObjId + '_DefinedLegendTable').width()) / 2;
    $('#' + m_LegendObjId + '_DefinedLegendBlankTd').css("width", m_LegendBlankTdWidth);
    $.parser.parse('#' + m_LegendObjId);
}
function DrawLineChart(myContentId, myData, myMaxValue) {
    //var line3 = [[['01/01/2008 09:01:01', 0.42], ['02/01/2008 09:01:01', 0.80], ['03/01/2008 09:01:01', 0.56], ['04/01/2008 09:01:01', 0.68],
    //            ['05/01/2008 09:01:01', 0.43], ['06/01/2008 09:01:01', 0.87]]];
    //myData = line3;
    if (PlotChartObj != undefined) {
        var m_ChartObjId = myContentId + "_Content";
        var m_ChartObj = $('#' + m_ChartObjId);
        m_ChartObj.empty();
    }
    var m_SeriesColors = ["#01b3f9", "#fef102", "#f8000e", "#a400ed", "#aaf900", "#fe0072", "#0c6c92", "#fea002", "#c1020a", "#62008d", "#3c8300"];
    PlotChartObj = $.jqplot(myContentId + "_Content", myData, {
        animate: true,
        seriesColors: m_SeriesColors,
        title: "",
        animateReplot: true,
        seriesDefaults: {
            lineWidth: 1,
            markerOptions: { size: 0 }
        },
        axes: {
            xaxis: {
                renderer: $.jqplot.DateAxisRenderer,
                tickOptions: {
                    formatString: "%F\n   %H:%M:%S" //   %Y-%m-%d
                },
                labelOptions: {
                    fontFamily: 'Helvetica',
                    fontSize: '8pt'
                },
            },
            yaxis: {
                tickOptions: {
                    formatString: "%.2f"
                },
                labelOptions: {
                    fontFamily: 'Helvetica',
                    fontSize: '8pt'
                },
                min: 0,
                max: myMaxValue,
                numberTicks: 5,
            }
        },
        highlighter: {
            show: true,
            sizeAdjust: 15
        },
        //cursor: {
        //    show: true,
        //    tooltipLocation: 'sw'
        //},
        grid: {
            drawGridLines: true, // wether to draw lines across the grid or not.
            gridLineColor: '#cccccc', // 设置整个图标区域网格背景线的颜色
            background: '#f3f6fd', // 设置整个图表区域的背景色
            borderColor: '#999999', // 设置图表的(最外侧)边框的颜色
            borderWidth: 2.0, //设置图表的（最外侧）边框宽度
            shadow: false, // 为整个图标（最外侧）边框设置阴影，以突出其立体效果
            shadowAngle: 45, // 设置阴影区域的角度，从x轴顺时针方向旋转
            shadowOffset: 1.5, // 设置阴影区域偏移出图片边框的距离
            shadowWidth: 3, // 设置阴影区域的宽度
            shadowDepth: 3, // 设置影音区域重叠阴影的数量
            shadowAlpha: 0.07, // 设置阴影区域的透明度
            renderer: $.jqplot.CanvasGridRenderer, // renderer to use to draw the grid.
            rendererOptions: {} // options to pass to the renderer. Note, the default
            // CanvasGridRenderer takes no additional options.
        }
    });
}