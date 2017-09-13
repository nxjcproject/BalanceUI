//合并单元格
function myMergeCell(myDatagridId, columnName) {
    merges = getMergeCellArray(myDatagridId, columnName);
    doMergeCell(myDatagridId, columnName, merges);
}
//获取需要合并单元格的数组信息
function getMergeCellArray(myDatagridId, columnName) {
    var myDatagrid = $('#' + myDatagridId);
    var merges = [];
    var myDatas = myDatagrid.datagrid('getData');
    var myRows = myDatas["rows"];
    var length = myRows.length;
    var beforeValue;
    //参数
    var count = 0;//merges数组个数
    var rowspan = 0;
    var index = 0;
    for (var i = 0; i < length; i++) {
        var currentValue = myRows[i][columnName];
        //第一个要特殊处理
        if (i == 0) {
            beforeValue = currentValue;
        }
        //前一行和后一行相同时累加数加一
        if (currentValue == beforeValue) {
            rowspan++;
        }
        else {
            //当rowspan为1时不用合并单元格
            if (rowspan > 1) {
                merges.push({ "rowspan": rowspan, "index": index });
            }
            beforeValue = currentValue;
            index = i;
            //初始化rowspan
            rowspan = 1;
        }
        //最后一个也要特殊处理
        if ((length - 1) == i && rowspan > 1) {
            merges.push({ "rowspan": rowspan, "index": index });
        }
    }
    return merges;
}
//合并单元格
function doMergeCell(myDatagridId, columnName, merges) {
    var myDatagrid = $('#' + myDatagridId);
    var myLength = merges.length;
    for (var i = 0; i < myLength; i++) {
        myDatagrid.datagrid('mergeCells', {
            index: merges[i].index,
            field: columnName,
            rowspan: merges[i].rowspan
        });
    }
}