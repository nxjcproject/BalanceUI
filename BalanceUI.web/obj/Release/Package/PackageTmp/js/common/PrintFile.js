
function doPrint(myTableString) {

    var m_Div = $('<div></div');
    var m_TableObj = $(myTableString);
    m_TableObj.appendTo(m_Div);
    //m_Div.append(myTableString);

    //m_PrintDialog.document.write(tableString);
    
    //var m_Options = { mode: 'popup', popClose: false};
    //m_Div.printArea();
    m_Div.jqprint();
}

function PrintHtml(myDataTable) {
    var m_TableString = myDataTable;
    doPrint(m_TableString);
}
