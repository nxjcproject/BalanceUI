var OrganizationId = "";
var MasterMachineHaltLogId = "";
var hidOperation = 1;
var InValidDatetimeChange = false;   //数值变化不引发Change事件
$(function () {
    InitializeDateTime();
    InitializeMasterMachineHaltGrid({ "rows": [], "total": 0 }, "MasterMachineHaltInfo");
    loadMachineHaltReasons();
    loadMasterMachineEditDialog();
});

// 目录树选择事件
function onOrganisationTreeClick(node) {
    // 更新当前分厂名
    $('#Textbox_OrganizationName').textbox('setText', node.text);
    OrganizationId = node.OrganizationId;
    $('#Combobox_MainMachineF').combobox("clear");
    $('#dataGrid_MasterMachineHaltInfo').datagrid("loadData", { "rows": [], "total": 0 });
    GetMasterMachineInfo(OrganizationId);
}
function InitializeDateTime() {
    //StartTime; EndTime

    var lastMonthDate = new Date();  //上月日期  
    var NowDate = new Date();
    lastMonthDate.setMonth(lastMonthDate.getMonth() - 1);
    var m_LastMonthDateString = FormatDateTime(lastMonthDate);
        
    var m_DateString = FormatDateTime(NowDate);
      
    $('#Datebox_StartTimeF').datebox('setValue', m_LastMonthDateString);
    $('#Datebox_EndTimeF').datebox('setValue', m_DateString);
}
// 获取所有停机原因
function loadMachineHaltReasons() {
    var queryUrl = 'BalanceMachineHalt.aspx/GetMachineHaltReasonsWithTreeGridFormat';

    var win = $.messager.progress({
        title: '请稍后',
        msg: '数据载入中...'
    });
    $.ajax({
        type: "POST",
        url: queryUrl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData != null && m_MsgData != undefined) {
                $('#ComboTree_HaltReason').combotree({
                    data: m_MsgData,
                    dataType: "json",
                    valueField: 'id',
                    textField: 'text',
                    required: false,
                    panelHeight: 350,
                    editable: false,
                    onSelect: function (node) {
                        //返回树对象  
                        var tree = $(this).tree;
                        //选中的节点是否为叶子节点,如果不是叶子节点,清除选中  
                        var isLeaf = tree('isLeaf', node.target);
                        if (!isLeaf) {
                            alert("请选择具体停机原因!");
                            //清除选中  
                            $('#ComboTree_HaltReason').combotree('clear');
                        }
                    }
                });
                $('#ComboTree_HaltReason').combotree('tree').tree("collapseAll");
            }
            $.messager.progress('close');
        },
        error: function () {
            $.messager.progress('close');
            $.messager.alert('错误', '数据载入失败！');
        }
    });
}
function InitializeMasterMachineHaltGrid(myData, myObjId) {
    $('#dataGrid_' + myObjId).datagrid({
        title: '',
        data: myData,
        dataType: "json",
        striped: true,
        //loadMsg: '',   //设置本身的提示消息为空 则就不会提示了的。这个设置很关键的
        rownumbers: true,
        singleSelect: true,
        idField: 'MachineHaltLogId',
        fit:true,
        columns: [[{
            width: '110',
            title: '数据项ID',
            field: 'MachineHaltLogId',
            hidden: true
        }, {
            width: '90',
            title: '组织机构',
            field: 'Name'
        }, {
            width: '60',
            title: '组织机构ID',
            field: 'OrganizationId',
            hidden: true
        }, {
            width: '60',
            title: '设备ID',
            field: 'EquipmentId',
            hidden: true
        }, {
            width: '100',
            title: '设备名称',
            field: 'EquipmentName'
        }, {
            width: '60',
            title: '运行班组ID',
            field: 'WorkingTeamShiftLogId',
            hidden: true
        }, {
            width: '60',
            title: '运行班组',
            field: 'WorkingTeam'
        }, {
            width: '100',
            title: 'DCS标签',
            field: 'Label'
        }, {
            width: '60',
            title: '开机时间',
            field: 'StartTime',
            hidden: true
        }, {
            width: '120',
            title: '停机时间',
            field: 'HaltTime'
        }, {
            width: '120',
            title: '重新开机时间',
            field: 'RecoverTime'
        }, {
            width: '60',
            title: '开机时间',
            field: 'StartTime',
            hidden: true
        }, {
            width: '60',
            title: '停机原因',
            field: 'ReasonId',
            hidden: true
        }, {
            width: '130',
            title: '停机原因',
            field: 'ReasonText'
        }, {
            width: '140',
            title: '备注',
            field: 'Remarks'
        }, {
            width: '50',
            title: '操作',
            field: 'Op',
            formatter: function (value, row, index) {
                var str = '';
                str = '<img class="iconImg" src = "/lib/extlib/themes/images/ext_icons/notes/note_edit.png" title="编辑" onclick="EditMasterMachineHaltFun(\'' + row.MachineHaltLogId + '\',\'' + row.WorkingTeam + '\');"/>';
                //str = str + '<img class="iconImg" src = "/lib/extlib/themes/images/ext_icons/user/user.png" title="分配角色" onclick="GrantRoleFun(\'' + row.UserId + '\');"/>';
                //str = str + '<img class="iconImg" src = "/lib/extlib/themes/images/ext_icons/group/group.png" title="分配部门" onclick="GrantOrganizationFun(\'' + row.UserId + '\');"/>';
                str = str + '<img class="iconImg" src = "/lib/extlib/themes/images/ext_icons/notes/note_delete.png" title="删除" onclick="RemoveMasterMachineHaltFun(\'' + row.MachineHaltLogId + '\');"/>';
                return str;
            }
        }]],
        toolbar: '#toolbar_' + myObjId
    });
}

function GetMasterMachineInfo(myOrganizationId) {
    $.ajax({
        type: "POST",
        url: "BalanceMachineHalt.aspx/GetMasterMachineInfo",
        data: "{myOrganizationId:'" + myOrganizationId + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData != null && m_MsgData != undefined) {
                $('#Combobox_MainMachineF').combobox("loadData", m_MsgData.rows);
            }          
        }
    });
}
function QueryMachineHaltInfo() {

    var m_StartHaltTime = $('#Datebox_StartTimeF').datebox('getValue');
    var m_EndHaltTime = $('#Datebox_EndTimeF').datebox('getValue');
    var m_EquipmentId = $('#Combobox_MainMachineF').combobox("getValue");

    if (m_StartHaltTime == "" || m_StartHaltTime == null || m_StartHaltTime == undefined) {
        alert("请选择开始时间!");
    }
    else if (m_EndHaltTime == "" || m_EndHaltTime == null || m_EndHaltTime == undefined)
    {
        alert("请选择结束时间!");
    }
    else if (m_EquipmentId == "" || m_EquipmentId == null || m_EquipmentId == undefined) {
        alert("请选择设备!");
    }
    else {
        $.ajax({
            type: "POST",
            url: "BalanceMachineHalt.aspx/GetMachineHaltLog",
            data: "{myOrganizationId:'" + OrganizationId + "',myStartHaltTime:'" + m_StartHaltTime + "',myEndHaltTime:'" + m_EndHaltTime + "',myEquipmentId:'" + m_EquipmentId + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                if (m_MsgData != null && m_MsgData != undefined) {
                    $('#dataGrid_MasterMachineHaltInfo').datagrid("loadData", m_MsgData);
                }
            }
        });
    }
}
function AddMachineHaltInfo() {
    var m_EquipmentId = $('#Combobox_MainMachineF').combobox("getValue");
    var m_OrganizationId = OrganizationId;
    if (m_OrganizationId == null || m_OrganizationId == "" || m_OrganizationId == undefined) {
        alert("请选择组织机构!");
    }
    else if (m_EquipmentId == null || m_EquipmentId == "" || m_EquipmentId == undefined) {
        alert("请选择设备!");
    }
    else {
        hidOperation = 0;
        $('#TextBox_MasterMachineName').textbox("setText", $('#Combobox_MainMachineF').combobox("getText"));
        $('#TextBox_OrganizationName').textbox("setText", $('#Textbox_OrganizationName').combobox("getText"));
        $('#dlg_MasterMachineHalt').dialog('open');
    }
}
function EditMasterMachineHaltFun(myMachineHaltLogId, myWorkingTeamName) {
    hidOperation = 1;
    MasterMachineHaltLogId = myMachineHaltLogId;

    $.ajax({
        type: "POST",
        url: "BalanceMachineHalt.aspx/GetMachineHaltLogbyId",
        data: "{myMachineHaltLogId:'" + myMachineHaltLogId + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var data = jQuery.parseJSON(msg.d)['rows'];
            if (data != null && data != undefined && data.length) {
                InValidDatetimeChange = true;
                $('#TextBox_MasterMachineName').textbox("setText", data[0].EquipmentName);
                $('#TextBox_OrganizationName').textbox("setText", data[0].Name);
                
                $('#Datetimebox_StartTime').datetimebox("setValue", data[0].StartTime.replace(/\//g, "-"));
                $('#Datetimebox_HaltTime').datetimebox("setValue", data[0].HaltTime.replace(/\//g, "-"));
                $('#Datetimebox_RecoverTime').datetimebox("setValue", data[0].RecoverTime.replace(/\//g, "-"));
                $('#ComboTree_HaltReason').combotree("setValue", data[0].ReasonId);
                $('#TextBox_Remark').attr('value', data[0].Remarks);
                var m_WorkingTeamShiftLog = [];
                m_WorkingTeamShiftLog.push({ "Id": myMachineHaltLogId, "Text": myWorkingTeamName });
                $('#Select_WorkingTeamShiftLogID').combobox("loadData", m_WorkingTeamShiftLog);
                $('#Select_WorkingTeamShiftLogID').combobox("setValue", myMachineHaltLogId);
                InValidDatetimeChange = false;
            }
        }
    });
    $('#dlg_MasterMachineHalt').dialog('open');
}
function GetWorkingTeamShiftLog(myDate) {
    if (InValidDatetimeChange == false) {
        $.ajax({
            type: "POST",
            url: "BalanceMachineHalt.aspx/GetMachineHaltLogbyDate",
            data: "{myDateTime:'" + myDate + "',myOrganizationId:'" + OrganizationId + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var data = jQuery.parseJSON(msg.d)['rows'];
                if (data != null && data != undefined && data.length) {
                    $('#Select_WorkingTeamShiftLogID').combobox("clear");
                    $('#Select_WorkingTeamShiftLogID').combobox("loadData", data);
                }
                else {
                    $('#Select_WorkingTeamShiftLogID').combobox("clear");
                    var m_WorkingTeamShiftLog = [];
                    $('#Select_WorkingTeamShiftLogID').combobox("loadData", m_WorkingTeamShiftLog);
                }
            }
        });
    }
}


function loadMasterMachineEditDialog() {
    $('#dlg_MasterMachineHalt').dialog({
        title: '设备停机信息',
        width: 600,
        height: 360,
        closed: true,
        cache: false,
        modal: true,
        buttons: [{
            text: '保存信息',
            handler: function () {

                if (hidOperation == 0) {        //添加
                    var m_EquipmentId = $('#Combobox_MainMachineF').combobox("getValue");
                    var m_OrganizationId = OrganizationId;
                    var m_MachineStartTime = $('#Datetimebox_StartTime').datetimebox("getValue");
                    var m_MachineHaltTime = $('#Datetimebox_HaltTime').datetimebox("getValue");
                    var m_MachineRecoverTime = $('#Datetimebox_RecoverTime').datetimebox("getValue");
                    var m_MachineHaltReason = $('#ComboTree_HaltReason').combotree("getValue");
                    if (m_MachineHaltReason == undefined || m_MachineHaltReason == "" || m_MachineHaltReason == null) {
                        m_MachineHaltReason = "";
                    }
                    var m_WorkingTeamShiftLogId = $('#Select_WorkingTeamShiftLogID').combobox("getValue");
                    var m_Remark = $('#TextBox_Remark').val();
                    if (m_MachineStartTime == null || m_MachineStartTime == "" || m_MachineStartTime == undefined) {
                        alert("请输入上次开机时间!");
                    }
                    else if (m_MachineHaltTime == null || m_MachineHaltTime == "" || m_MachineHaltTime == undefined) {
                        alert("请输入停机机时间!");
                    }
                    else if (m_MachineRecoverTime == null || m_MachineRecoverTime == "" || m_MachineRecoverTime == undefined) {
                        alert("请输入开机时间!");
                    }
                    else {
                        $.ajax({
                            type: "POST",
                            url: "BalanceMachineHalt.aspx/AddMachineHalt",
                            data: "{myEquipmentId:'" + m_EquipmentId + "',myOrganizationId:'" + m_OrganizationId + "',myMachineStartTime:'" + m_MachineStartTime + "',myMachineHaltTime:'" + m_MachineHaltTime +
                                "',myMachineRecoverTime:'" + m_MachineRecoverTime + "',myWorkingTeamShiftLogId:'" + m_WorkingTeamShiftLogId + "',myMachineHaltReason:'" + m_MachineHaltReason + "',myRemark:'" + m_Remark + "'}",
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            success: function (msg) {
                                if (msg.d == "1") {
                                    alert("添加成功!");
                                    $('#dlg').dialog('close');

                                } else if (msg.d == "0") {
                                    alert("添加失败!");
                                } else if (msg.d == "-1") {
                                    alert('数据库错误!');
                                } else {
                                    alert(msg.d);
                                }
                                QueryMachineHaltInfo();
                            }
                        });
                    }
                }
                else if (hidOperation == 1) {   //修改
                    var m_MasterMachineHaltLogId = MasterMachineHaltLogId;
                    var m_MachineStartTime = $('#Datetimebox_StartTime').datetimebox("getValue");
                    var m_MachineHaltTime = $('#Datetimebox_HaltTime').datetimebox("getValue");
                    var m_MachineRecoverTime = $('#Datetimebox_RecoverTime').datetimebox("getValue");
                    var m_MachineHaltReason = $('#ComboTree_HaltReason').combotree("getValue");
                    var m_MachineHaltReasonText = $('#ComboTree_HaltReason').combotree("getText");
                    if (m_MachineHaltReason == undefined || m_MachineHaltReason == "" || m_MachineHaltReason == null) {
                        m_MachineHaltReason = "";
                        m_MachineHaltReasonText = "";
                    }
                    var m_WorkingTeamShiftLogId = $('#Select_WorkingTeamShiftLogID').combobox("getValue");
                    var m_Remark = $('#TextBox_Remark').val();
                    if (m_MachineStartTime == null || m_MachineStartTime == "" || m_MachineStartTime == undefined) {
                        alert("请输入上次开机时间!");
                    }
                    else if (m_MachineHaltTime == null || m_MachineHaltTime == "" || m_MachineHaltTime == undefined) {
                        alert("请输入停机机时间!");
                    }
                    else if (m_MachineRecoverTime == null || m_MachineRecoverTime == "" || m_MachineRecoverTime == undefined) {
                        alert("请输入开机时间!");
                    }
                    else {
                        $.ajax({
                            type: "POST",
                            url: "BalanceMachineHalt.aspx/ModifyMachineHalt",
                            data: "{myMachineHaltLogId:'" + m_MasterMachineHaltLogId + "',myMachineStartTime:'" + m_MachineStartTime + "',myMachineHaltTime:'" + m_MachineHaltTime +
                                "',myMachineRecoverTime:'" + m_MachineRecoverTime + "',myWorkingTeamShiftLogId:'" + m_WorkingTeamShiftLogId + "',myMachineHaltReason:'" + m_MachineHaltReason + "',myMachineHaltReasonText:'" + m_MachineHaltReasonText + "',myRemark:'" + m_Remark + "'}",
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            success: function (msg) {
                                if (msg.d == "1") {
                                    alert("修改成功!");
                                    $('#dlg_MasterMachineHalt').dialog('close');
                                } else if (msg.d == "0") {
                                    alert("其它用户已修改,修改失败!");
                                } else if (msg.d == "-1") {
                                    alert('数据库错误!');
                                } else {
                                    alert(msg.d);
                                }
                                QueryMachineHaltInfo();
                            }
                        });
                    }
                }
            }
        }]
    });
}

function RemoveMasterMachineHaltFun(myMachineHaltLogId) {
    parent.$.messager.confirm('询问', '您确定要删除该信息?', function (r) {
        if (r) {
            $.ajax({
                type: "POST",
                url: "BalanceMachineHalt.aspx/DeleteMachineHalt",
                data: "{myMachineHaltLogId:'" + myMachineHaltLogId + "'}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    if (msg.d == "1") {
                        alert("删除成功!");
                        $('#dlg_MasterMachineHalt').dialog('close');
                    } else if (msg.d == "0") {
                        alert("其它用户已删除,删除失败!");
                    } else if (msg.d == "-1") {
                        alert('数据库错误!');
                    } else {
                        alert(msg.d);
                    }
                    QueryMachineHaltInfo()
                }
            });
        }
    });
}

function FormatDateTime(myDate) {
    var m_DateString = myDate.getFullYear();
    if (myDate.getMonth() + 1 < 10) {
        m_DateString = m_DateString + '-0' + (myDate.getMonth() + 1);
    }
    else {
        m_DateString = m_DateString + '-' + (myDate.getMonth() + 1);
    }
    if (myDate.getDate() < 10) {
        m_DateString = m_DateString + '-0' + myDate.getDate();
    }
    else {
        m_DateString = m_DateString + '-' + myDate.getDate();
    }
    return m_DateString;
}