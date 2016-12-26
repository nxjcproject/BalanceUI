<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BalanceMachineHalt.aspx.cs" Inherits="BalanceUI.web.UI_Balance.BalanceMachineHalt" %>

<%@ Register Src="~/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagPrefix="uc1" TagName="OrganisationTree" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>停机平衡</title>

    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/gray/easyui.css" />
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/icon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtIcon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtCss.css" />

    <script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/jquery.utility.js"></script>
    <script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>

    <script type="text/javascript" src="/UI_Balance/js/page/BalanceMachineHalt.js" charset="utf-8"></script>
</head>
<body>
    <div class="easyui-layout" data-options="fit:true,border:false" style="padding: 1px;">
        <!-- 左侧目录树开始 -->
        <div data-options="region:'west',border:false,collapsible:false" style="width: 150px;">
            <uc1:OrganisationTree runat="server" ID="OrganisationTree" />
        </div>
        <!-- 左侧目录树结束 -->
        <!-- 中央区域开始 -->
        <div data-options="region:'center',border:false,collapsible:false" style="padding-left: 3px;">
            <div id="toolbar_MasterMachineHaltInfo">
                <table>
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>当前分厂：</td>
                                    <td>
                                        <input id="Textbox_OrganizationName" class="easyui-textbox" readonly="readonly" style="width: 100px" /></td>
                                    <td>|</td>
                                    <td>时间：</td>
                                    <td>
                                        <input id="Datebox_StartTimeF" class="easyui-datebox" style="width: 100px" /></td>
                                    <td>--</td>
                                    <td>
                                        <input id="Datebox_EndTimeF" class="easyui-datebox" style="width: 100px" /></td>
                                    <td>|</td>
                                    <td>选择设备：</td>
                                    <td>
                                        <select id="Combobox_MainMachineF" class="easyui-combobox" name="MainMachine" data-options="panelHeight:'auto', valueField: 'Id',textField: 'Text',editable:false" style="width: 160px;">
                                        </select>
                                    </td>
                                    <td><a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-search'" onclick="QueryMachineHaltInfo()">查询</a></td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <a id="btnAdd" href="#" class="easyui-linkbutton" data-options="iconCls:'icon-add', plain:true" onclick ="AddMachineHaltInfo()">新建</a>
                        </td>
                    </tr>
                </table>
            </div>
            <table id="dataGrid_MasterMachineHaltInfo"></table>
        </div>
    </div>
    
    <form id="form1" runat="server">
        <div id="dlg_MasterMachineHalt" class="easyui-dialog" data-options="iconCls:'icon-edit',resizable:false,modal:true">
        <fieldset>
            <legend>设备停机编辑</legend>
            <table class="table">
                <tr>
                    <th style ="width:100px;">设备名称</th>
                    <td style="width: 160px">
                        <input id="TextBox_MasterMachineName" class="easyui-textbox" data-options="required:true,readonly:true" style="width: 140px" />
                    </td>
                    <th style ="width:100px;">组织机构</th>
                    <td style="width: 160px">
                        <input id="TextBox_OrganizationName" class="easyui-textbox" data-options="required:true,readonly:true" style="width: 140px" />
                    </td>
                </tr>
                <tr>
                    <th>上次开机时间</th>
                    <td>
                        <input id="Datetimebox_StartTime" class="easyui-datetimebox" data-options="required:true,editable:false"  style="width: 140px" />
                    </td>
                    <th>停机时间</th>
                    <td>
                        <input id="Datetimebox_HaltTime" class="easyui-datetimebox" data-options="required:true,editable:false,onChange:function(date){GetWorkingTeamShiftLog(date);}" style="width: 140px" />
                    </td>
                </tr>
                <tr>
                    <th>开机时间</th>
                    <td>
                        <input id="Datetimebox_RecoverTime" class="easyui-datetimebox" data-options="required:true,editable:false"  style="width: 140px" />
                    </td>
                    <th>停机原因</th>
                    <td>
                        <input id = "ComboTree_HaltReason" class="easyui-combotree" style="width:140px"/>
                    </td>
                </tr>
                <tr>
                    <th>交接班组</th>
                    <td>
                        <select id="Select_WorkingTeamShiftLogID" class="easyui-combobox" name="MainMachine" data-options="panelHeight:'auto', valueField: 'Id',textField: 'Text',editable:false" style="width: 140px;">
                        </select>
                    </td>
                    <th></th>
                    <td>
                    </td>
                </tr>
                <tr>
                    <th style ="width:100px;">备注</th>
                    <td colspan="3" style ="width:420px;">
                        <asp:TextBox ID="TextBox_Remark" runat="server" TextMode="MultiLine" Width="430px" Height="100px"></asp:TextBox>
                    </td>
                </tr>
            </table>
        </fieldset>
    </div>
    </form>
</body>
</html>
