<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BalanceWeightBalanceLog.aspx.cs" Inherits="BalanceUI.web.UI_Balance.BalanceWeightBalanceLog" %>

<%@ Register Src="~/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagPrefix="uc1" TagName="OrganisationTree" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>平衡日志</title>

    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/gray/easyui.css" />
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/icon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtIcon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtCss.css" />

    <script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/jquery.utility.js"></script>
    <script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>

    <script type="text/javascript" src="/UI_Balance/js/page/BalanceWeightBalanceLog.js" charset="utf-8"></script>
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
            <div id="toolbar">
                <table>
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>当前分厂：</td>
                                    <td>
                                        <input id="organizationName" class="easyui-textbox" readonly="readonly" style="width: 100px" /></td>
                                    <td>|</td>
                                    <td>货位：</td>
                                    <td><select id="LocationID" class="easyui-combobox" data-options="panelHeight:true,valueField:'LocationID',textField:'LocationName'" name="dept" style="width:200px;"/></td>
                                    <td>时间：</td>
                                    <td>
                                        <input id="startTime" class="easyui-datebox" style="width: 100px" /></td>
                                    <td>--</td>
                                    <td>
                                        <input id="endTime" class="easyui-datebox" style="width: 100px" /></td>
                                    <td><a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-search'" onclick="QueryLogData()">搜索</a></td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <a id="btnAdd" href="#" class="easyui-linkbutton" data-options="iconCls:'icon-add'" onclick="CreatBalanceLog()">新建</a>
                        </td>
                    </tr>
                </table>
            </div>
            <table id="dataGrid"></table>
            <div class="easyui-window" id="materialKindsEdit" title="物料种类" data-options="modal:true,closed:true,iconCls:'icon-edit',minimizable:false,maximizable:false,collapsible:false,resizable:false" style="width: 350px; height: 400px; padding: 1px 1px 1px 1px">
                <table id="dgMaterialKinds"></table>
            </div>


            <div id="balanceToolBar">
                <table>
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>库存：</td>
                                    <td><input id="stocks" class="easyui-numberbox" value="0" data-options="precision:2" style="width:80px"/></td>
                                    <td>投入总平衡数：</td>
                                    <td><input id="inputValue" class="easyui-numberbox" value="0" data-options="precision:2" style="width:80px"/></td>
                                    <td>产出总平衡数：</td>
                                    <td><input id="outputValue" class="easyui-numberbox" value="0" data-options="precision:2" style="width:80px"/></td>
                                    <td><a href="javascript:void(0)" class="easyui-linkbutton" data-options="iconCls:'icon-undo',plain:true" onclick="reject()">撤销</a></td>
                                    <td>
                                        <a href="#" id="edit" class="easyui-linkbutton" data-options="iconCls:'icon-save',plain:true" onclick="saveFun();">保存</a>
                                    </td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                </table>
            </div>
            <div class="easyui-window" id="balanceWindow" title="物料平衡" data-options="modal:true,closed:true,iconCls:'icon-edit',minimizable:false,maximizable:false,collapsible:false,resizable:false" style="width: 850px; height: 400px; padding: 1px 1px 1px 1px">
                <table id="balanceEdit"></table>
            </div>
        </div>
    </div>
</body>
</html>
