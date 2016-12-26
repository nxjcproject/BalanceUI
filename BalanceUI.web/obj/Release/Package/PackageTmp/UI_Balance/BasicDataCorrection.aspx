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
            <div id="toolbar_BasicDataCorrection">
                <table>
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td style="width: 60px; height: 30px;">起止时间</td>
                                    <td style="text-align: left; width: 320px;">
                                        <input id="StartTimeF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                        <span id="InnerlLine">--</span>
                                        <input id="EndTimeF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                    </td>
                                    <td>当前分厂</td>
                                    <td style="width: 90px;">
                                        <input id="Textbox_OrganizationName" class="easyui-textbox" readonly="readonly" style="width: 80px" />
                                    </td>
                                    <td>最大偏差倍数</td>
                                    <td>
                                        <input id="DeviationMagnificationF" class="easyui-numberspinner" style="width: 80px;" data-options="min:1,max:1000,value:10,increment:1" />
                                    </td>
                                    <td style="width: 90px; text-align: right;">
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-search', width:80" onclick="QueryAbnormalData()">查询</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="width: 60px; height: 30px;">参考时间</td>
                                    <td style="text-align: left; width: 320px;">
                                        <input id="StartTimeReferenceF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                        <span id="InnerlLine1">--</span>
                                        <input id="EndTimeReferenceF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                    </td>
                                    <td>调整对象</td>
                                    <td>
                                        <select id="CorrectionObjectF" class="easyui-combobox" data-options="panelHeight:true,valueField:'id',textField:'text',editable:false" name="dept" style="width: 80px;">
                                            <option value="All" selected="selected">全部</option>
                                            <option value="MaterialWeight">产量/消耗量</option>
                                            <option value="ElectricityQuantity">电量</option>
                                        </select>
                                    </td>
                                    <td>最小有效值</td>
                                    <td>
                                        <input id="MinValidValueF" class="easyui-numberspinner" style="width: 80px;" data-options="min:0.0,max:1000.0,value:1.0,increment:0.1,precision:1" />
                                    </td>
                                    <td style="width: 90px; text-align: right;">
                                        <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-edit', width:80" onclick="DirectCorrection();">直接调整</a>
                                    </td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                </table>
            </div>
            <table id="dataGrid_BasicDataCorrection"></table>
        </div>
    </div>
    <div id="dlg_CorrectionData" class="easyui-dialog">
        <div class="easyui-layout" data-options="fit:true,border:false">
            <div data-options="region:'north',border:false,collapsible:false" style="height: 210px;">
                <div class="easyui-layout" data-options="fit:true,border:false">
                    <div data-options="region:'north',border:false,collapsible:false" style="height: 34px; background-color: #eeeeee;">
                        <table>
                            <tr>
                                <td style="width: 60px;">起止时间</td>
                                <td style="text-align: left; width: 320px;">
                                    <input id="datetimebox_TrendStartTimeF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                    <span id="Span1">--</span>
                                    <input id="datetimebox_TrendEndTimeF" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                </td>
                                <td>
                                    <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-search'" onclick="EditTrendTags();">趋势标签</a>
                                </td>
                                <td>
                                    <a href="#" class="easyui-linkbutton" data-options="iconCls:'ext-icon-chart_curve'" onclick="DisplayTrend();">趋势</a>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div data-options="region:'center',border:false,collapsible:false" style ="padding-top:3px;">
                        <div id="LineTrend_Legend" style ="width:730px; height:10px; vertical-align:bottom;">

                        </div>
                        <div id="LineTrend_Content" style ="width:720px; height:145px; ">

                        </div>
                    </div>
                </div>
            </div>
            <div data-options="region:'center',border:true,collapsible:false">
                <div class="easyui-layout" data-options="fit:true,border:false">
                    <div data-options="region:'west',border:false,collapsible:false" style="width: 245px; padding: 2px;">
                        <table>
                            <tr>
                                <td style="width: 85px; height: 30px;">异常数据时间</td>
                                <td style="width: 150px;">
                                    <input id="Text_AbnormalDataTime" class="easyui-textbox" data-options="editable:false" style="width: 145px" />
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 85px; height: 30px;">调整开始时间</td>
                                <td style="width: 150px;">
                                    <input id="datetimebox_CorrectionStartTime" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 85px; height: 30px;">调整结束时间</td>
                                <td style="width: 150px;">
                                    <input id="datetimebox_CorrectionEndTime" class="easyui-datetimebox" data-options="validType:'md[\'2014-07-28 10:10:10\']',editable:false" value="2014-07-28 12:13:56" style="width: 145px" />
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 85px; height: 30px; text-align: left;">
                                    <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-clear'" onclick="ClearTags('dataGrid_CorrectionTags');">清空</a>

                                </td>
                                <td style="width: 150px; text-align: right;">
                                    <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-add'" onclick="SelectTags('dataGrid_CorrectionTags');">添加</a>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 85px; height: 30px;"></td>
                                <td style="width: 150px;"></td>
                            </tr>
                            <tr>
                                <td style="width: 85px; height: 30px; text-align: left;">
                                    <a href="#" class="easyui-linkbutton" data-options="iconCls:'icon-edit'" onclick="CorrectionDataByTags();">修正</a>
                                </td>
                                <td style="width: 150px; text-align: right;">
                                    <a href="#" class="easyui-linkbutton" onclick="ReCaculateBalanceData();">重新生成汇总数据</a>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div data-options="region:'center',border:false,collapsible:false" style="padding: 2px;">
                        <table id="dataGrid_CorrectionTags"></table>
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
                            <option value="Abnormal" selected="selected">异常标签</option>
                            <option value="Ammeter">电表标签</option>
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
    <form id="form1" runat="server">
        <div>
        </div>
    </form>
</body>
</html>
