var WindowsObjArray = new Array();
var MaxWindowsCount = 8;
$(document).ready(function () {
    for (var i = 0; i < MaxWindowsCount; i++) {
        WindowsObjArray.push("");
    }
});
function OpenWindows(myParentObjId, myTitle, myWidth, myHeight, myLeft, myTop, myDraggable, myMaximizable, myMaximized, myResizable) {
    if (arguments.length == 9) {
        myResizable = false;
    }

    var m_WindowArrayIndex = GetEmptyWindowArray();
    var m_Title = myTitle + ' 视图' + (m_WindowArrayIndex + 1);
    if (WindowsObjArray.length <= MaxWindowsCount && m_WindowArrayIndex >= 0) {

        var m_WindowsId = 'Windows' + m_WindowArrayIndex;

        //var m_HtmlString = '<div id="' + m_WindowsId + '" class="easyui-window" title="' + myTitle + '" data-options="iconCls:\'icon-save\'" style="width:' + myWidth + 'px;height:' + myHeight + 'px;padding:10px;"></div>';
        var m_HtmlString = '<div id = "' + m_WindowsId + '"></div>';
        $('#' + myParentObjId).append(m_HtmlString);                    //添加一个window

        CreateWindow(m_WindowsId, m_Title, myWidth, myHeight, myLeft, myTop, myDraggable, myMaximizable, myMaximized, myResizable);
        WindowsObjArray[m_WindowArrayIndex] = m_WindowsId;                 //window进入队列

        return m_WindowsId;
    }
    else {
        alert("最多允许" + MaxWindowsCount + "个窗口弹出!");
        return "";
    }
}

function CreateWindow(myWindowId, myTitle, myWidth, myHeight) {
    $('#' + myWindowId).window({   
        width: myWidth,
        height: myHeight,
        title: myTitle,
        collapsible: false,
        minimizable: false,
        maximizable: false,
        resizable: false,
        inline: true,
        iconCls: 'ext-icon-chart_bar',
        padding: 10

        //modal:true  
    }); 
}
function CreateWindow(myWindowId, myTitle, myWidth, myHeight, myLeft, myTop) {
    $('#' + myWindowId).window({
        width: myWidth,
        height: myHeight,
        title: myTitle,
        left: myLeft,
        top: myTop,
        collapsible: false,
        minimizable: false,
        maximizable: false,
        resizable: false,
        inline: true,
        iconCls: 'ext-icon-chart_bar',
        padding: 10

        //modal:true  
    });
} 
function CreateWindow(myWindowId, myTitle, myWidth, myHeight, myLeft, myTop, myDraggable, myMaximizable) {
    $('#' + myWindowId).window({
        width: myWidth,
        height: myHeight,
        title: myTitle,
        left: myLeft,
        top: myTop,
        collapsible: false,
        minimizable: false,
        resizable: false,
        inline: true,
        draggable: myDraggable,
        maximizable: myMaximizable, 
        iconCls: 'ext-icon-chart_bar',
        padding: 10

        //modal:true  
    });
}

function CreateWindow(myWindowId, myTitle, myWidth, myHeight, myLeft, myTop, myDraggable, myMaximizable, myMaximized, myResizable) {
    if (arguments.length == 9) {
        myResizable = false;
    }

    $('#' + myWindowId).window({
        width: myWidth,
        height: myHeight,
        title: myTitle,
        left: myLeft,
        top: myTop,
        collapsible: false,
        minimizable: false,
        resizable: myResizable,
        inline: true,
        draggable: myDraggable,
        maximizable: myMaximizable,
        maximized: myMaximized,
        iconCls: 'ext-icon-chart_bar',
        padding: 10

        //modal:true  
    });
}

function GetEmptyWindowArray() {
    var m_ArrayIndex = -1;
    for (var i = 0; i < MaxWindowsCount; i++) {
        if (WindowsObjArray[i] == "") {
            m_ArrayIndex = i;
            break;
        }
    }
    return m_ArrayIndex;
}
function CloseWindow(myObj) {
    for (var i = 0; i < WindowsObjArray.length; i++) {
        if (WindowsObjArray[i] == myObj.attr("id")) {
            WindowsObjArray[i] = "";
        }
    }
    myObj.unbind();
    myObj.parents().next('.window-shadow').remove();        //.css('background', 'red');
    myObj.window('destroy', true);
}
function CloseAllWindows() {
    for (var i = 0; i < MaxWindowsCount; i++) {
        var m_WindowObj = $('#' + WindowsObjArray[i]);
        m_WindowObj.unbind();
        m_WindowObj.parents().next('.window-shadow').remove();        //.css('background', 'red');
        m_WindowObj.window('destroy', true);
        WindowsObjArray[i] = "";
    }
}
function GetWindowIdByObj(myWindowObj) {
    if (myWindowObj != null && myWindowObj != undefined && myWindowObj != "") {
        return myWindowObj.attr("id");
    }
    else {
        return "";
    }
}
function TopWindow(myWindowId) {

    for (var i = 0; i < WindowsObjArray.length; i++) {
        if (WindowsObjArray[i] == myWindowId) {
            $('#' + myWindowId).parent().css('zIndex', 9010);
        }
        else if (WindowsObjArray[i] != "") {
            $('#' + WindowsObjArray[i]).parent().css('zIndex', 9008);
        }
        $('#' + WindowsObjArray[i]).parent().next('.window-shadow').css('zIndex', 9000);
    }
}
function BottomWindow(myWindowId) {
    for (var i = 0; i < WindowsObjArray.length; i++) {
        if (WindowsObjArray[i] == myWindowId) {
            $('#' + myWindowId).parent().css('zIndex', 9010);
        }
        else if (WindowsObjArray[i] != "") {
            $('#' + WindowsObjArray[i]).parent().css('zIndex', 9008);
        }
    }
}
function GetWindowsIdArray() {
    return WindowsObjArray;
}

