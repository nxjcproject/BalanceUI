var editIndex = undefined;
var OrganizationId = "";
var OrganizationType = "";
var SelectedTargetId = "";
var PlotChartObj = undefined;
$(function () {
    InitializeDateTimePickers();
    InitializeBasicDataCorrectionGrid("BasicDataCorrection", { "rows": [], "total": 0 });
    InitializeCorrectionGrid("CorrectionTags", { "rows": [], "total": 0 });
    InitializeDataTagsGrid("SelectTagsList", { "rows": [], "total": 0 });
    InitializeTrendTagsGrid("TrendTagsList", { "rows": [], "total": 0 });
    LoadDataCorrectionDialog();
    LoadTagsSelectDialog();
    LoadSelectedTrendTags();
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
// 目录树选择事件
function onOrganisationTreeClick(node) {
    // 更新当前分厂名
    $('#Textbox_OrganizationName').textbox('setText', node.text);
    OrganizationId = node.OrganizationId;
    OrganizationType = node.OrganizationType;
    ClearTags('dataGrid_CorrectionTags');
}
function InitializeDateTimePickers() {
    var m_DateTime = new Date();
    var m_NowStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
    m_DateTime.setDate(m_DateTime.getDate() - 1);
    var m_YestedayStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
    $('#StartTimeF').datetimebox('setValue', m_YestedayStr);
    $('#EndTimeF').datetimebox('setValue', m_NowStr);

    //////////参考时间取前一个月的数据的平均值//////////
    m_DateTime.setMonth(m_DateTime.getMonth() - 1);
    var m_LastMonthEndStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
    m_DateTime.setMonth(m_DateTime.getMonth() - 1);
    var m_LastMonthStartStr = m_DateTime.format("yyyy-MM-dd hh:mm:ss");
    $('#StartTimeReferenceF').datetimebox('setValue', m_LastMonthStartStr);
    $('#EndTimeReferenceF').datetimebox('setValue', m_LastMonthEndStr);

}

function QueryAbnormalData() {
    var m_StartTime = $('#StartTimeF').datetimebox("getValue");
    var m_EndTime = $('#EndTimeF').datetimebox('getValue');
    var m_StartTimeReference = $('#StartTimeReferenceF').datetimebox("getValue");
    var m_EndTimeReference = $('#EndTimeReferenceF').datetimebox('getValue');
    var m_DeviationMagnification = $('#DeviationMagnificationF').numberspinner("getValue");
    var m_MinValidValue = $('#MinValidValueF').numberspinner("getValue");
    var m_CorrectionObject = $('#CorrectionObjectF').combobox("getValue");
    if (OrganizationType == "" || OrganizationType != "分厂") {
        alert("请首先选择分厂!");
    }
    else {
        var win = $.messager.progress({
            title: '请稍后',
            msg: '数据载入中...'
        });
        $.ajax({
            type: "POST",
            url: 'BasicDataCorrection.aspx/GetAbnormalData',
            data: "{myOrganizationId:'" + OrganizationId + "',myDeviationMagnification:'" + m_DeviationMagnification + "',myCorrectionObject:'" + m_CorrectionObject + "',myMinValidValue:'" + m_MinValidValue +
                "',myStartTime:'" + m_StartTime + "',myEndTime:'" + m_EndTime + "',myStartTimeReference:'" + m_StartTimeReference + "',myEndTimeReference:'" + m_EndTimeReference + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                if (m_MsgData != null && m_MsgData != undefined) {
                    var m_BaseColunms = [];
                    try {
                        if (m_MsgData["columns"].length > 0) {
                            for (var i = 0; i < m_MsgData["columns"].length; i++) {
                                var m_ColumnTemp = [];
                                m_ColumnTemp["width"] = "120";
                                m_ColumnTemp["title"] = GetDateTimeString(m_MsgData["columns"][i]);
                                m_ColumnTemp["field"] = m_MsgData["columns"][i];
                                m_BaseColunms.push(m_ColumnTemp);
                            }
                            var options = $("#dataGrid_BasicDataCorrection").datagrid("options");                   //取出当前datagrid的配置     
                            options.columns = [m_BaseColunms];                            //添加服务器端返回的columns配置信息
                            //options.queryParams = getQueryParams("id");                                //添加查询参数  
                            $("#dataGrid_BasicDataCorrection").datagrid(options);
                            $("#dataGrid_BasicDataCorrection").datagrid("load");
                            $('#dataGrid_BasicDataCorrection').datagrid("loadData", m_MsgData);
                        }
                        else {
                            var options = $("#dataGrid_BasicDataCorrection").datagrid("options");                   //取出当前datagrid的配置     
                            options.columns = [m_BaseColunms];                            //添加服务器端返回的columns配置信息
                            //options.queryParams = getQueryParams("id");                                //添加查询参数  
                            $("#dataGrid_BasicDataCorrection").datagrid(options);
                            $("#dataGrid_BasicDataCorrection").datagrid("load");
                            $('#dataGrid_BasicDataCorrection').datagrid("loadData", { "rows": [], "total": 0 });
                        }
                    }
                    catch (err) {
                        $.messager.progress('close');
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
}

function InitializeBasicDataCorrectionGrid(myObjId, myData) {
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
        frozenColumns: [[{
            width: '110',
            title: '数据项ID',
            field: 'id',
            hidden: true
        }, {
            width: '100',
            title: '字段名称',
            field: 'FieldName',
            hidden: true
        }, {
            width: '100',
            title: '数据项名称',
            field: 'text'
        }, {
            width: '90',
            title: '地址',
            field: 'Address'
        }, {
            width: '60',
            title: '修改对象',
            field: 'Type'
        }, {
            width: '60',
            title: '平均值',
            field: 'AvgValue'
        }]],
        onDblClickCell: function (rowIndex, field, value) {
            if (field != 'id' && field != 'FieldName' && field != 'text' && field != 'Address' && field != 'Type' && field != 'AvgValue') {
                GetDataTrend(rowIndex, field, value);
            }
        },
        toolbar: '#toolbar_' + myObjId
    });
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
            width: '110',
            title: '标签名称',
            field: 'text'
        }, {
            width: '70',
            title: '字段名称',
            field: 'FieldName'
        }, {
            width: '65',
            title: '对象类型',
            field: 'Type'
        }, {
            width: '70',
            title: '平均值',
            field: 'AvgValue'
        }, {
            width: '70',
            title: '修正值',
            field: 'CorrectionValue'
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
            width: '145',
            title: '数据项名称',
            field: 'text'
        }, {
            width: '70',
            title: '字段名称',
            field: 'FieldName'
        }, {
            width: '60',
            title: '修改对象',
            field: 'Type'
        }, {
            width: '80',
            title: '平均值',
            field: 'AvgValue'
        }, {
            width: '80',
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
        $('#Text_AbnormalDataTime').textbox('setValue', GetDateTimeString(field));
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
function DirectCorrection() {
    if (OrganizationType == "" || OrganizationType != "分厂") {
        alert("请首先选择分厂!");
    }
    else {
        $('#Text_AbnormalDataTime').textbox('setValue', "");

        var m_DateTime = new Date();
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
    SelectedTargetId = mySelectedTargetId;
    var m_DataTagsFilter = $('#Combobox_DataTagsFilter').combobox('getValue');
    if (m_DataTagsFilter == "Abnormal") {
        GetAbnormalTagsList();
    }
    $('#dlg_SelectDataTags').dialog('open');
}
function ClearTags(mySelectedTargetId) {
    $('#' + mySelectedTargetId).datagrid("loadData", { "rows": [], "total": 0 });
}
function EditTrendTags() {
    $('#dlg_SelectedTrendTags').dialog('open');
}
function ChangeTagsList(myRecord) {
    if (myRecord.Id == "Abnormal") {
        GetAbnormalTagsList();
    }
    else if (myRecord.Id == "Ammeter") {
        GetAmmeterTagsList();
    }
    else if (myRecord.Id == "DCS") {
        GetDCSTagsList();
    }
}
function GetAbnormalTagsList() {
    var m_AbnormalList = [];
    var m_AbnormalColumn = $('#Text_AbnormalDataTime').textbox('getValue');
    if (m_AbnormalColumn != "") {
        m_AbnormalColumn = m_AbnormalColumn.replace(/-/g, "");
        m_AbnormalColumn = m_AbnormalColumn.replace(/ /g, "");
        m_AbnormalColumn = m_AbnormalColumn.replace(/:/g, "");
        var m_DataRows = $('#dataGrid_BasicDataCorrection').datagrid("getRows");
        for (var i = 0; i < m_DataRows.length; i++) {
            if (m_DataRows[i][m_AbnormalColumn] != "") {
                var m_AbnormalTemp = [];
                m_AbnormalTemp["id"] = m_DataRows[i]["id"];
                m_AbnormalTemp["FieldName"] = m_DataRows[i]["FieldName"];
                m_AbnormalTemp["text"] = m_DataRows[i]["text"];
                m_AbnormalTemp["Address"] = m_DataRows[i]["Address"];
                m_AbnormalTemp["Type"] = m_DataRows[i]["Type"];
                m_AbnormalTemp["AvgValue"] = m_DataRows[i]["AvgValue"];
                m_AbnormalTemp["CorrectionValue"] = m_DataRows[i][m_AbnormalColumn];

                m_AbnormalList.push(m_AbnormalTemp);
            }
        }
    }
    else {
        var m_DataRows = $('#dataGrid_BasicDataCorrection').datagrid("getRows");
        for (var i = 0; i < m_DataRows.length; i++) {
            var m_AbnormalTemp = [];
            m_AbnormalTemp["id"] = m_DataRows[i]["id"];
            m_AbnormalTemp["FieldName"] = m_DataRows[i]["FieldName"];
            m_AbnormalTemp["text"] = m_DataRows[i]["text"];
            m_AbnormalTemp["Address"] = m_DataRows[i]["Address"];
            m_AbnormalTemp["Type"] = m_DataRows[i]["Type"];
            m_AbnormalTemp["AvgValue"] = m_DataRows[i]["AvgValue"];
            m_AbnormalTemp["CorrectionValue"] = "";

            m_AbnormalList.push(m_AbnormalTemp);
        }
    }
    $('#dataGrid_SelectTagsList').datagrid("loadData", { "rows": m_AbnormalList, "total": m_AbnormalList.length });
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
function CorrectionDataByTags() {
    endEditing();           //关闭正在编辑
    parent.$.messager.confirm('询问', '是否已经确认时间范围和数值<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;确定修改数据?', function (r) {
        if (r) {
            var m_CorrectionTagRows = $('#dataGrid_CorrectionTags').datagrid("getRows");
            var m_StartTime = $('#datetimebox_CorrectionStartTime').datetimebox('getValue');
            var m_EndTime = $('#datetimebox_CorrectionEndTime').datetimebox('getValue');
            var m_AbnormalDataTime = $('#Text_AbnormalDataTime').textbox('getValue');
            var m_TagsInfo = "";
            if (m_CorrectionTagRows != null && m_CorrectionTagRows != undefined) {
                for (var i = 0; i < m_CorrectionTagRows.length; i++) {
                    var m_CorrectionValue = m_CorrectionTagRows[i]["CorrectionValue"] != undefined ? m_CorrectionTagRows[i]["CorrectionValue"] : "0";
                    if (i == 0) {
                        m_TagsInfo = m_CorrectionTagRows[i]["FieldName"] + "," + m_CorrectionTagRows[i]["Type"] + "," + m_CorrectionValue;
                    }
                    else {
                        m_TagsInfo = m_TagsInfo + ";" + m_CorrectionTagRows[i]["FieldName"] + "," + m_CorrectionTagRows[i]["Type"] + "," + m_CorrectionValue;
                    }
                }
            }
            if (m_CorrectionTagRows.length > 0) {
                var win = $.messager.progress({
                    title: '请稍后',
                    msg: '数据处理中...'
                });
                $.ajax({
                    type: "POST",
                    url: 'BasicDataCorrection.aspx/CorrectionDataByTags',
                    data: "{myOrganizationId:'" + OrganizationId + "',myStartTime:'" + m_StartTime + "',myEndTime:'" + m_EndTime
                        + "',myAbnormalDataTime:'" + m_AbnormalDataTime + "',myTagsInfo:'" + m_TagsInfo + "'}",
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
                var m_Date = myKey.replace('_',' ');
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