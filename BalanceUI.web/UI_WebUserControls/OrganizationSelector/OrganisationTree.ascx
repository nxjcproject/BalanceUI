<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrganisationTree.ascx.cs" Inherits="WebUserContorls.Web.UI_WebUserControls.OrganizationSelector.OrganisationTree" %>
<div class="easyui-layout" data-options="fit:true,border:false" >
    <div data-options="region:'north',border:true,collapsible:false" style="height: 50px; background-color:#FAFAFA; padding-top:10px;">
        <table>
            <tr>
                <td style="width: 90px; height: 30px;">选择产线类型</td>
                <td style="width: 120px; text-align: left;">
                    <select id="combobox_OrganisationTree_ProductionTypeF" class="easyui-combobox" data-options="panelHeight: 'auto', editable: false" style="width: 110px;">
                    </select>
                </td>
            </tr>
        </table>
    </div>

    <div data-options="region:'center',border:true,collapsible:false">
        <ul id="organisationTree" class="easyui-tree"></ul>
    </div>
</div>
<input id="HiddenField_PageName" style="width: 200px; visibility: hidden;" runat="server" />

<script>
    $(document).ready(function () {
        LoadProductionType('first');
        loadOrganisationTree('first');

    });

    function loadOrganisationTree(myLoadType) {
        var m_Type = $('#combobox_OrganisationTree_ProductionTypeF').combobox('getValue');
        var m_FunctionName = "GetOrganisationTree";
        var queryUrl = $('#TagsSelector_DcsTags_HiddenField_PageName').val();
        var m_Parmaters = { "myFunctionName": m_FunctionName, "myType": m_Type };
        $.ajax({
            type: "POST",
            url: queryUrl,
            data: m_Parmaters,
            //contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                if (myLoadType == 'first') {
                    initializeOrganisationTree(msg);
                }
                else {
                    $('#organisationTree').tree('loadData', msg);
                }
            },
            error: function () {
                $.messager.alert('错误', '数据载入失败！');
            }
        });
    }

    // 初始化组织结构树
    function initializeOrganisationTree(jsonData) {
        $('#organisationTree').tree({
            data: jsonData,
            animate: true,
            lines: true,
            onDblClick: function (node) {
                if (typeof (onOrganisationTreeClick) == "function") {
                    onOrganisationTreeClick(node);
                }
            }
        });
    }
    function LoadProductionType(myLoadType) {
        var m_FunctionName = "GetProductionLineType";
        var queryUrl = $('#TagsSelector_DcsTags_HiddenField_PageName').val();
        var m_Parmaters = { "myFunctionName": m_FunctionName };
        $.ajax({
            type: "POST",
            url: queryUrl,
            data: m_Parmaters,
            //contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = msg['rows'];
                m_MsgData.unshift({ "ProductionLineId": "", "ProductionLineText": "全部" });
                if (myLoadType == 'first') {
                    initializeProductionType(m_MsgData);
                }
                else {
                    $('#combobox_OrganisationTree_ProductionTypeF').combobox('loadData', m_MsgData);
                }
            },
            error: function () {
                $.messager.alert('错误', '数据载入失败！');
            }
        });
    }
    function initializeProductionType(jsonData) {
        $('#combobox_OrganisationTree_ProductionTypeF').combobox({
            data: jsonData,
            dataType: "json",
            valueField: 'ProductionLineId',
            textField: 'ProductionLineText',
            required: false,
            panelHeight: 'auto',
            editable: false,
            onSelect: function (myRecord) {
                loadOrganisationTree('last');
            }
        });
    }
</script>
