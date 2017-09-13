$(function () {
    InitialDate();
    LoadDataGrid("first");
});
function InitialDate() {
    var nowDate = new Date();
    var beforeDate = new Date();
    beforeDate.setDate(nowDate.getDate());
    var nowString = nowDate.getFullYear() + '-' + (nowDate.getMonth() + 1) + '-' + nowDate.getDate() + " " + nowDate.getHours() + ":" + nowDate.getMinutes() + ":" + nowDate.getSeconds();
    var beforeString = beforeDate.getFullYear() + '-' + (beforeDate.getMonth()) + '-' + beforeDate.getDate() + " " + beforeDate.getHours() + ":" + beforeDate.getMinutes() + ":" + beforeDate.getSeconds();
    $('#startTime').datetimebox('setValue', beforeString);
    $('#endTime').datetimebox('setValue', nowString);
}
function onOrganisationTreeClick(node) {
    $('#organizationName').textbox('setText', node.text);
    $('#organizationId').val(node.OrganizationId);
    m_OrganizationId = node.OrganizationId;
    //LoadProductionLine(mOrganizationId);
}
function LoadDataGrid(type, myData) {
    if (type == "first") {
        $('#grid_Main').datagrid({
            columns: [[
                  {
                      field: 'Process', title: '工序', width: 80, rowspan: 2, align: 'left'},
                  { field: 'MaterialName', title: '物料名称', width: 80, rowspan: 2, align: 'left' },
                  { field: 'Earlyinventory', title: '期初库存', width: 80, rowspan: 2, align: 'right' },
                  { field: 'Input', title: '入厂量', width: 80, rowspan: 2, align: 'right' },
                  { field: 'Moisture', title: '水分', width: 50, rowspan: 2 , align: 'right' },
                  { title: '消耗量', colspan: 3 },
                  { title: '产品', colspan: 2 },
                  { field: 'Output', title: '出厂量', width: 80, align: 'right', rowspan: 2 },
                  { field: 'DynamicInventory', title: '动态库存', width: 80, align: 'right', rowspan: 2 },
                  { field: 'Ratio', title: '料耗比', width: 80, align: 'right', rowspan: 2 }
            ],
            [
                 { field: 'WetBase', title: '湿基', width: 80, align: 'right' },
                 { field: 'DryBasis', title: '干基', width: 80, align: 'right' },
                 { field: 'Formula', title: '配比%', width: 50, align: 'right' },
                 { field: 'Yield', title: '产量', width: 80, align: 'right' },
                 { field: 'ProductMoisture', title: '水分', width: 50, align: 'right' }
            ]],
            fit: true,
            toolbar: "#toorBar",
            idField: 'Name',
            singleSelect: true,
            striped: true,
            data: [],
            rownumbers:true
        });
    }
    else {
        $('#grid_Main').datagrid('loadData', myData);
    }
}
function Query() {
    var startTime = $('#startTime').datetimebox('getValue');
    var endTime = $('#endTime').datetimebox('getValue');
    if (startTime > endTime)
    {
        $.messager.alert('提示', '请输入正确的时间');
    }
    var dataTosend = "{OrganizationId:'" + m_OrganizationId + "',startTime:'" + startTime.toString() + "',endTime:'" + endTime.toString() + "'}";
    var win = $.messager.progress(
               {
                   title: '查询时间稍长，请耐心等待',
                   msg: '数据载入中...'
               }
               );
    $.ajax({
        type: "POST",
        url: "MaterialBalance.aspx/GetMaterialBalanceReport",
        data: dataTosend,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $.messager.progress('close');
            m_Msgdata = jQuery.parseJSON(msg.d);
            if (m_Msgdata.length == 0) {
                $.messager.alert('提示', '没有查到相关库存信息！');
            }
            else {
                LoadDataGrid("", m_Msgdata)
                myMergeCell('grid_Main', "Process");
            }
        },
        error: function handleError() {
            $.messager.progress('close');
            $.messager.alert('失败', '获取数据失败');
        }
    }
        );
}
