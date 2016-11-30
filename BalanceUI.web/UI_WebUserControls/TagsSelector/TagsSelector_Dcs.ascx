<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TagsSelector_Dcs.ascx.cs" Inherits="WebUserContorls.Web.UI_WebUserControls.TagsSelector.TagsSelector_Dcs" %>
 <div id="ToolBar_TagsSelector_Dcs_DcsTags" style="display:none; text-align:center; padding-top:10px;">
        <table>
            <tr>
				<td style = "width:65px; height:30px;">选择DCS</td>
		        <td style = "width:170px; text-align:left;">
                    <input id="Combobox_TagsSelector_Dcs_DcsDataF" class="easyui-combotree" style="width: 160px;" />
                </td>
                <td style = "width:65px;">标签名</td>
                <td style = "width:150px; text-align:left;">
                    <input id="textbox_TagsSelector_Dcs_DcsTagsNameF" class="easyui-textbox" style="width: 140px;" />
                </td>
                <td style = "width:65px;">标签类型</td>
                <td style = "width:120px; text-align:left;">
                    <select id="combobox_TagsSelector_Dcs_DcsTagsTypeF" class="easyui-combobox" data-options="panelHeight: 'auto', editable: false" style="width: 110px;" >
                        <option value="real" selected>实数</option>
		                <option value="int">整数</option>
		                <option value="bit">开关量</option>
                    </select>

                </td>
                <td style ="width:55px;">
                    <a href="javascript:void(0);" class="easyui-linkbutton" data-options="iconCls:'icon-search',plain:true" 
                                        onclick="ButtonQueryDcsTagsFun();">查询</a>
                </td>
			</tr>
	    </table>
    </div>
    <table id ="DataGrid_TagsSelector_Dcs_DcsTags"></table>
    <input id="HiddenField_PageName" style="width: 200px; visibility:hidden;" runat="server"/>


<script type="text/javascript"  charset="utf-8">
    ///////////////////DCSGrid分页//////////////////
    var BatchSize = 100;                         //后台每组数据1000行
    var DefaultPageSize = 20;                     //默认每页20项
    var LastBatchIndex = 1;                       //最近数据组索引
    var MsgDCSData;                                  //数据项列表
    var DcsDataBaseName;                         //标签数据库
    var DcsOrganizationIdQuery;                   //当点击查找时DCS组织机构
    var DcsDataBaseNameQuery                     //当点击查找时标签
    $(function () {
        LoadDcsOrganzation('first');
        //LoadLinesData();
        //GetWindowPostion();
    });

    ////////////////////////////////////////DCS数据生成/////////////////////////////////////
    function LoadDcsOrganzation(myLoadType) {                                      //装载可比数据
        var m_PageName = $('#TagsSelector_DcsTags_HiddenField_PageName').val();
        var m_FunctionName = "GetDCSTagsDataBase";
        var m_Parmaters = { "myFunctionName": m_FunctionName };
        $.ajax({
            type: "POST",
            url: m_PageName,
            data: m_Parmaters,
            //contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                if (myLoadType == 'first') {
                    InitializeDcsComboxTree(msg);
                }
                else if (myLoadType == 'last') {
                    $('#ToolBar_TagsSelector_Dcs_DcsTags').datagrid('loadData', msg);
                }
            }
        });
    }

    function InitializeDcsComboxTree(myData) {
        $('#Combobox_TagsSelector_Dcs_DcsDataF').combotree({
            data: myData,
            dataType: "json",
            valueField: 'id',
            textField: 'text',
            required: false,
            panelHeight: 'auto',
            editable: false,
            onSelect: function (myRecord) {
                DcsDataBaseName = myRecord.DcsProcessDatabase;
            }
        }); //m_DataGridId
        //QueryDcsTagsFun(LastBatchIndex, 0, DefaultPageSize, 'first');
        InitializeDCSDataGrid("DataGrid_TagsSelector_Dcs_DcsTags", { "rows": [], "total": 0 });
    }
    function ButtonQueryDcsTagsFun() {
        if (DcsDataBaseName != "" && DcsDataBaseName != null && DcsDataBaseName != undefined) {
            DcsOrganizationIdQuery = $('#Combobox_TagsSelector_Dcs_DcsDataF').combobox('getValue');
            DcsDataBaseNameQuery = DcsDataBaseName;
            QueryDcsTagsFun(LastBatchIndex, 0, DefaultPageSize, 'last');
        }
        else {
            alert("请选择DCS!");
        }
    }
    function QueryDcsTagsFun(myBatchNum, myStartIndex, myPageSize, myLoadType) {
        var m_PageName = $('#TagsSelector_DcsTags_HiddenField_PageName').val();
        var m_DcsTagsName = $('#textbox_TagsSelector_Dcs_DcsTagsNameF').textbox('getText');
        var m_DcsTagsType = $('#combobox_TagsSelector_Dcs_DcsTagsTypeF').combobox('getValue');
        var m_FunctionName = "GetDCSTagsByDataBase";
        var m_Parmaters = { "myFunctionName": m_FunctionName, "myBatchNumber": myBatchNum, "myBatchSize": BatchSize, "myDataBaseName": DcsDataBaseName, "myDcsTagsName": m_DcsTagsName, "myDcsTagsType": m_DcsTagsType };
        $.ajax({
            type: "POST",
            url: m_PageName,
            data: m_Parmaters,
            //contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                MsgDCSData = msg;
                var m_SubJson = "";
                if (MsgDCSData != null) {
                    m_SubJson = GetSubJson(MsgDCSData, myStartIndex, myPageSize);
                }
                if (myLoadType == 'first') {
                    InitializeDCSDataGrid('DataGrid_TagsSelector_Dcs_DcsTags', m_SubJson);
                }
                else if (myLoadType = 'last') {
                    $('#DataGrid_TagsSelector_Dcs_DcsTags').datagrid('loadData', m_SubJson);             //树形结构
                }
            }
        });
    }

    function InitializeDCSDataGrid(myGridId, myData) {
        $('#' + myGridId).datagrid({
            title: '',
            fit: true,
            data: myData,
            dataType: "json",
            striped: true,
            //loadMsg: '',   //设置本身的提示消息为空 则就不会提示了的。这个设置很关键的
            rownumbers: true,
            pagination: true,
            singleSelect: true,
            idField: 'VariableName',
            pageSize: DefaultPageSize,
            pageList: [10, 20, 50],
            collapsible: true,
            columns: [[{
                width: '260',
                title: '标签描述',
                field: 'VariableDescription'
            }, {
                width: '150',
                title: '标签名',
                field: 'VariableName'
            }, {
                width: '100',
                title: '数据表名',
                field: 'TableName',
                hidden: true
            }, {
                width: '100',
                title: '数据字段名',
                field: 'FieldName',
                hidden: true
            }]],
            toolbar: '#ToolBar_TagsSelector_Dcs_DcsTags',
            onDblClickRow: function (rowIndex, rowData) {
                if (typeof (GetTagInfo) == "function") {
                    GetTagInfo(rowData, DcsDataBaseNameQuery, DcsOrganizationIdQuery);
                }
            }
        });
        var p = $('#' + myGridId).datagrid('getPager');
        if (p) {
            $(p).pagination({
                //分页栏下方文字显示
                displayMsg: '当前显示从第{from}条到{to}条 共{total}条记录',

                onBeforeRefresh: function (pageNumber, pageSize) {
                    PaginationRefresh(pageNumber, pageSize);
                    $(this).pagination('loading');
                    $(this).pagination('loaded');
                },
                onSelectPage: function (pageNumber, pageSize) {//分页触发  
                    ChangePageNumber(pageNumber, pageSize);
                }
            });
        }
    }
    function SetPageIndex(myIndex) {
        var p = $('#DataGrid_TagsSelector_Dcs_DcsTags').datagrid('getPager');
        if (p) {
            $(p).pagination({
                pageNumber: myIndex //默认索引页
            });
        }
    }

    function PaginationRefresh(myPageNumber, myPageSize) {

        var m_CurrentStartIndex = (myPageNumber - 1) * myPageSize % BatchSize;
        QueryDcsTagsFun(LastBatchIndex, m_CurrentStartIndex, myPageSize, 'last');
    }
    function DataRefresh() {
        LastBatchIndex = 1;
        QueryDcsTagsFun(LastBatchIndex, 0, DefaultPageSize, 'last');
        SetPageIndex(1);
    }
    function ChangePageNumber(myPageNum, myPageSize) {
        if (DefaultPageSize != myPageSize) {
            LastBatchIndex = -1;
            DefaultPageSize = myPageSize;
        }
        var m_CurrentBatchIndex = Math.floor((myPageNum - 1) * myPageSize / BatchSize + 1);

        var m_CurrentStartIndex = (myPageNum - 1) * myPageSize % BatchSize;

        if (m_CurrentBatchIndex != LastBatchIndex) {
            QueryDcsTagsFun(m_CurrentBatchIndex, m_CurrentStartIndex, myPageSize, 'last');
            LastBatchIndex = m_CurrentBatchIndex;
        }
        else {
            var m_SubJson = GetSubJson(MsgDCSData, m_CurrentStartIndex, myPageSize);
            $('#DataGrid_TagsSelector_Dcs_DcsTags').datagrid('loadData', m_SubJson);
        }

    }
    function GetSubJson(myJsonData, myStartIndex, myPagSize) {
        var m_SubJson = jQuery.extend(true, {}, myJsonData);
        m_SubJson['rows'].splice(0, myStartIndex);
        m_SubJson['rows'].splice(myPagSize, m_SubJson['rows'].length - myPagSize);

        //var aa = m_SubJson['rows'].splice(myStartIndex * myPagSize, myPagSize);

        return m_SubJson;
    }
</script>