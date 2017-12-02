<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BasicDataCorrection.aspx.cs" Inherits="BalanceUI.web.UI_Balance.BasicDataCorrection" %>

<%@ Register Src="~/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagPrefix="uc1" TagName="OrganisationTree" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>基础数据修正</title>

    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/gray/easyui.css" />
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/icon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtIcon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtCss.css" />

    <link rel="stylesheet" type="text/css" href="/lib/pllib/themes/jquery.jqplot.min.css" />
    <link type="text/css" rel="stylesheet" href="/lib/pllib/syntaxhighlighter/styles/shCoreDefault.min.css" />
    <link type="text/css" rel="stylesheet" href="/lib/pllib/syntaxhighlighter/styles/shThemejqPlot.min.css" />
    <link type="text/css" rel="stylesheet" href="/css/common/charts.css" />
    <link type="text/css" rel="stylesheet" href="/css/common/NormalPage.css" />

    <script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/jquery.utility.js"></script>
    <script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/extend/editCell.js" charset="utf-8"></script>

    <!--[if lt IE 9]><script type="text/javascript" src="/lib/pllib/excanvas.js"></script><![endif]-->
    <script type="text/javascript" src="/lib/pllib/jquery.jqplot.min.js"></script>
    <!--<script type="text/javascript" src="/lib/pllib/syntaxhighlighter/scripts/shCore.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/syntaxhighlighter/scripts/shBrushJScript.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/syntaxhighlighter/scripts/shBrushXml.min.js"></script>-->

    <!-- Additional plugins go here -->
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.barRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.pieRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.categoryAxisRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.pointLabels.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.cursor.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.canvasTextRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.canvasAxisTickRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.dateAxisRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.highlighter.min.js"></script>


    <%--<script type="text/javascript" src="/lib/pllib/themes/jquery.jqplot.js"></script>
    <script type="text/javascript" src="/lib/pllib/themes/jjquery.jqplot.min.js"></script>--%>
    <!--[if lt IE 8 ]><script type="text/javascript" src="/js/common/json2.min.js"></script><![endif]-->

    <script type="text/javascript" src="/js/common/format/DateTimeFormat.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/components/Charts.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/components/DataGrid.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/components/WindowsDialog.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/components/GridChart.js" charset="utf-8"></script>

    <script type="text/javascript" src="/js/common/format/DateTimeFormat.js" charset="utf-8"></script>
    <script type="text/javascript" src="/UI_Balance/js/page/BasicDataCorrection.js" charset="utf-8"></script>
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
            <div class="easyui-layout" data-options="fit:true,border:false">
                <div data-options="region:'north',border:false,collapsible:false" style="height: 210px;" aria-checked="true">
                    <div id="ChartContentDiv" class="easyui-layout" data-options="fit:true,border:false">
                        <div data-options="region:'north',border:true,collapsible:false" style="height: 34px; background-color: #eeeeee; padding-top: 2px;">
                            <table>
                                <tr>
                                    <td style="width: 60px; text-align: right;">当前分厂</td>
                                    <td style="width: 80px;">
                                        <input id="Textbox_OrganizationName" class="easyui-textbox" readonly="readonly" style="width: 75px" />
                                    </td>
                                    <td style="width: 60px; text-align: right;">开始时间</td>
                                    <td style="width: 150px;">
                                        <input id="datetimebox_TrendStartTimeF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                    </td>
                                    <td style="width: 60px; text-align: right;">结束时间</td>
                                    <td style="width: 150px;">
                                        <input id="datetimebox_TrendEndTimeF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                    </td>
                                    <td>
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-search'" onclick="EditTrendTags();">趋势标签</a>
                                    </td>
                                    <td>
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'ext-icon-chart_curve'" onclick="DisplayTrend();">趋势</a>
                                    </td>
                                    <td style="text-align: center;">
                                        <div class="datagrid-btn-separator"></div>
                                    </td>
                                    <td>
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'ext-icon-feed'" onclick="AlarmInfoQuery();">报警信息</a>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div data-options="region:'center',border:true,collapsible:false" style="padding-top: 3px;">
                            <div style="width: 630px; height: 8px; vertical-align: bottom;">
                            </div>
                            <div id="LineTrend_Content" style="width: 620px; height: 125px;">
                            </div>
                            <div id="LineTrend_Legend" style="width: 630px; height: 10px; vertical-align: bottom; margin-top: 5px;">
                            </div>
                        </div>
                    </div>
                </div>
                <div data-options="region:'center',border:false,collapsible:false" style="padding-top: 3px;">
                    <div class="easyui-layout" data-options="fit:true,border:false">
                        <div data-options="region:'west',border:true,collapsible:false" style="width: 250px;">
                            <table>
                                <tr>
                                    <td style="width: 100px; height: 30px; text-align: center;">调整开始时间</td>
                                    <td style="width: 150px;">
                                        <input id="datetimebox_CorrectionStartTime" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                    </td>
                                </tr>
                                <tr>
                                    <td style="height: 30px; text-align: center;">调整结束时间</td>
                                    <td>
                                        <input id="datetimebox_CorrectionEndTime" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                    </td>
                                </tr>
                                <tr>
                                    <td style="height: 40px; text-align: left; padding-left: 5px;">
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-edit'" onclick="CorrectionDataByTags();">修正</a>
                                    </td>
                                    <td style="text-align: right; padding-right: 5px;">
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-clear'" onclick="ClearTags('dataGrid_CorrectionTags');">清空标签</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="height: 40px; text-align: left; padding-left: 5px;"></td>
                                    <td style="text-align: right; padding-right: 5px;">
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-add'" onclick="SelectTags('dataGrid_CorrectionTags');">添加标签</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="height: 40px; text-align: left; padding-left: 5px;">
                                        <a href="#" class="easyui-linkbutton" onclick="ReCaculateBalanceData();">生成汇总数据</a>
                                    </td>
                                    <td style="text-align: right; padding-right: 5px;">
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'ext-icon-calculator'" onclick="CaculateAvgValue();">计算均值</a>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div data-options="region:'center',border:false,collapsible:false" style="padding-left: 3px;">
                            <table id="dataGrid_CorrectionTags"></table>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div id="dlg_SelectDataTags" class="easyui-dialog">
        <div id="toolbar_SelectTagsList">
            <table>
                <tr>
                    <td style="width: 60px; height: 30px;">标签筛选
                    </td>
                    <td style="width: 100px;">
                        <select id="Combobox_DataTagsFilter" class="easyui-combobox" name="MainMachine" data-options="panelHeight:'auto', valueField: 'Id',textField: 'Text',editable:false,onSelect:function(record){ChangeTagsList(record);}" style="width: 90px;">
                            <option value="Ammeter" selected="selected">电表标签</option>
                            <option value="DCS">DCS标签</option>
                        </select>
                    </td>
                    <td>
                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-add'" onclick="AddAllTags();">添加全部</a>
                    </td>
                </tr>
            </table>
        </div>
        <table id="dataGrid_SelectTagsList"></table>
    </div>
    <div id="dlg_SelectedTrendTags" class="easyui-dialog">
        <div id="toolbar_TrendTagsList">
            <table>
                <tr>
                    <td style="width: 70px; height: 30px;">
                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-add'" onclick="SelectTags('dataGrid_TrendTagsList');">添加</a>
                    </td>
                    <td style="width: 70px;">
                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-clear'" onclick="ClearTags('dataGrid_TrendTagsList');">清空</a>
                    </td>
                </tr>
            </table>
        </div>
        <table id="dataGrid_TrendTagsList"></table>
    </div>
    <div id="dlg_AlarmInfo" class="easyui-dialog">
        <div id="toolbar_AlarmInfo">
            <table>
                <tr>
                    <td style="width: 55px; text-align: right;">开始时间</td>
                    <td style="width: 150px;">
                        <input id="datetimebox_AlarmStartTimeF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                    </td>
                    <td style="width: 55px; text-align: right;">结束时间</td>
                    <td style="width: 150px;">
                        <input id="datetimebox_AlarmEndTimeF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                    </td>
                    <td style="width: 55px; text-align: right;">报警类别</td>
                    <td style="width: 90px;">
                        <input id="combobox_AlarmTypeF" class="easyui-combobox" data-options="valueField:'id',textField:'text',panelHeight:'auto',required: false,editable: false" style="width: 85px" />
                    </td>
                    <td style="height: 30px; text-align:left;">
                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-search'" onclick="AlarmDetailQuery();">查询</a>
                    </td>
                </tr>
            </table>
        </div>
        <table id="dataGrid_AlarmInfo"  class="easyui-datagrid"
            data-options="rownumbers:true,singleSelect:true,border:false,fit:true,toolbar:'#toolbar_AlarmInfo',onDblClickRow:function(rowIndex, rowData){SetTrendTime(rowData);}">
               <thead>
                    <tr>
                        <!--<th data-options="field:'ID',width:200,hidden: true">设备ID</th>-->
                        <th data-options="field:'AlarmId',width:110,hidden: true">报警ID</th>
                        <th data-options="field:'AlarmText',width:300">报警名称</th>
                        <th data-options="field:'StartTime',width:150">开始时间</th>
                        <th data-options="field:'EndTime',width:150">结束时间</th>
                    </tr>
               </thead>
        </table>
    </div>
    <form id="MainForm" runat="server">
        <div>
        </div>
    </form>
</body>
</html>
