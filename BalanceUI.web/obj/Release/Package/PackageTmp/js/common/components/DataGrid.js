
function InitializeGrid(myGridObjId, myData) {

    var m_FrozenCount = myData['FrozenCount'];
    var m_IdColumn = myData['columns'][0].field;

    var m_Columns = jQuery.extend(true, {}, [myData['columns']]);
    var m_FrozenColumns = m_Columns[0].splice(0, m_FrozenCount);

    var m_Grid = $('#' + myGridObjId).datagrid({
        title: '',
        data: myData,
        dataType: "json",
        striped: true,
        idField: m_IdColumn,
        frozenColumns: [m_FrozenColumns],
        columns: [m_Columns[0]],
        //loadMsg: '',   //设置本身的提示消息为空 则就不会提示了的。这个设置很关键的
        rownumbers: true,
        //pagination: true,
        singleSelect: true
        //onClickCell: onClickCell
        //idField: m_IdAndNameColumn[0].field,
        //pageSize: 20,
        //pageList: [20, 50, 100, 500],

        //toolbar: '#toolbar'
    });
    return m_Grid
}
function ReleaseGird(myContainerId, myGrid) {
    if (myGrid) {
        //myGrid.destroy();

        var elementId = '#' + myContainerId;
        $(elementId).unbind(); // for iexplorer  
        $(elementId).empty();

        //myGrid = null;
    }
}
