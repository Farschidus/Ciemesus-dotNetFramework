<%@ Page Language="C#" MasterPageFile="~/Application/Masters/CP.master" AutoEventWireup="true"
    CodeFile="Default.aspx.cs" Inherits="PSM_Medias_Default" %>

<%@ Register Assembly="Farschidus" Namespace="Farschidus.Web.UI.WebControls" TagPrefix="Farschidus" %>
<%@ Register Src="~/Application/Ascx/tinyMCE.ascx" TagPrefix="uc1" TagName="tinyMCE" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="cphHeader" runat="Server">
    <script type="text/javascript">
        var deleteConfirmMessage = '<%=Farschidus.Translator.AppTranslate["general.message.confirmDelete"]%>';
        var deleteConfirmButtons = { '<%=Farschidus.Translator.AppTranslate["general.button.ok"]%>': true, '<%=Farschidus.Translator.AppTranslate["general.button.cancel"]%>': false };
    </script>
</asp:Content>
<asp:Content ID="SearchContent" ContentPlaceHolderID="cphSearch" runat="Server">
    <div id="Search">
        <table width="730px" style="display: inline-block">
            <tr>
                <td class="right rtl">
                    <%=Farschidus.Translator.AppTranslate["mediaManaging.default.search.fileName"]%>
                </td>
                <td class="left CpLtr">
                    <asp:TextBox ID="txtFileNameSearch" runat="server"></asp:TextBox>
                </td>
                <td class="right rtl">
                    <%=Farschidus.Translator.AppTranslate["mediaManaging.default.search.fileExtention"]%>
                </td>
                <td class="left CpLtr">
                    <asp:TextBox ID="txtFileExtentionSearch" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="right rtl">
                    <%=Farschidus.Translator.AppTranslate["mediaManaging.default.search.description"]%>
                </td>
                <td class="left CpLtr">
                    <asp:TextBox ID="txtDescriptionSearch" runat="server"></asp:TextBox>
                </td>
                <td class="right rtl">
                    <%=Farschidus.Translator.AppTranslate["mediaManaging.default.search.url"]%>
                </td>
                <td class="left ltr">
                    <asp:TextBox ID="txtUrlSearch" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="right rtl top" colspan="4">
                    <asp:LinkButton ID="btnSearch" runat="server" OnClick="btnSearch_Click" CssClass="search">
                <%= Farschidus.Translator.AppTranslate["general.button.search"] %></asp:LinkButton>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="cphMain" runat="server">
    <div id="List">
        <table style="margin: 10px auto; width: 95%;">
            <tr>
                <td align="left">
                    <asp:UpdatePanel ID="uplList" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <ContentTemplate>
                            <Farschidus:GridView ID="grvList" runat="server" ViewStateMode="Enabled" AutoGenerateColumns="false"
                                CssClass="gridView" DataKeyNames="IDMedia" OnRowDraged="grvList_RowDraged" OnRowEditing="grvList_RowEditing"
                                OnRowDeleting="grvList_RowDeleting">
                                <Columns>
                                    <Farschidus:CheckAllBoxField ID="chkList" ItemStyle-CssClass="gridCheckbox" />
                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            <%=Farschidus.Translator.AppTranslate["mediaManaging.default.grid.fileName"]%>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <%# Eval("FileName") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            <%=Farschidus.Translator.AppTranslate["mediaManaging.default.grid.preview"]%>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <%# mGetLinkToPreview((int)Eval("IDMedia"))%>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            <%=Farschidus.Translator.AppTranslate["mediaManaging.default.grid.fileExtention"]%>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <%# Eval("FileExtention").ToString().Replace(".","") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            <%=Farschidus.Translator.AppTranslate["mediaManaging.default.grid.date"]%>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <%# Eval("Date") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="edit">
                                        <HeaderTemplate>
                                            <%=Farschidus.Translator.AppTranslate["mediaManaging.default.grid.edit"]%>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnEditRow" runat="server" CommandName="Edit" CssClass="gridEdit"></asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="delete">
                                        <HeaderTemplate>
                                            <%=Farschidus.Translator.AppTranslate["mediaManaging.default.grid.delete"]%>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnDeleteRow" runat="server" CommandName="Delete" CssClass="gridDelete"
                                                OnClientClick="return Farschidus.confirmDelete(this ,deleteConfirmMessage, deleteConfirmButtons);"></asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </Farschidus:GridView>
                            <Farschidus:Pager ID="listPager" runat="server" PageSize="10" CompactModePageCount="7" NormalModePageCount="7"
                                GenerateFirstLastSection="True" GeneratePagerInfoSection="False" GenerateGoToSelect="Static"
                                GeneratePageSizeSelect="true" PageSizeSelectStart="10" PageSizeSelectInterval="5" MaximumPageSizeSelect="50"
                                OnPageSizeChanged="Pager_PageSizeChanged" OnPageIndexChanged="Pager_PageIndexChanged"
                                GenerateSmartShortCuts="false"></Farschidus:Pager>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="listPager" EventName="PageSizeChanged" />
                            <asp:AsyncPostBackTrigger ControlID="listPager" EventName="PageIndexChanged" />
                        </Triggers>
                    </asp:UpdatePanel>
                </td>
            </tr>
        </table>
    </div>
    <div id="AddEdit">
        <asp:UpdatePanel ID="uplAddEdit" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table width="500px" style="display: inline-block">
                    <tr>
                        <td class="right rtl">
                            <%= Farschidus.Translator.AppTranslate["mediaManaging.default.addEdit.date"]%>
                        </td>
                        <td class="left ltr" colspan="2">
                            <asp:TextBox ID="txtDate" runat="server" Width="100px"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvDate" runat="server" ErrorMessage="*" ControlToValidate="txtDate"
                                ValidationGroup="ValGroup"></asp:RequiredFieldValidator>
                            <ajax:MaskedEditExtender ID="DateExtender" runat="server" TargetControlID="txtDate"
                                Mask="9999/99/99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                OnInvalidCssClass="MaskedEditError" MaskType="Date" DisplayMoney="Left" AcceptNegative="Left"
                                ErrorTooltipEnabled="True" />
                            <ajax:CalendarExtender ID="calExtDate" runat="server" TargetControlID="txtDate">
                            </ajax:CalendarExtender>
                        </td>
                    </tr>
                    <tr>
                        <td class="right rtl">
                            <%= Farschidus.Translator.AppTranslate["mediaManaging.default.addEdit.selectFile"]%>
                        </td>
                        <td class="left ltr">
                            <ajax:AsyncFileUpload ID="fulImage" runat="server" Width="250px" OnUploadedComplete="fulImages_UploadedComplete"
                                ThrobberID="myThrobber" />
                        </td>
                        <td class="right rtl">
                            <asp:Literal ID="litImage" runat="server"></asp:Literal>
                            <asp:Label ID="myThrobber" runat="server"><img alt="uploading" src='/Application/Images/General/uploading.gif' align="absmiddle" /> Please Wait...</asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="right rtl">
                            <%= Farschidus.Translator.AppTranslate["mediaManaging.default.addEdit.url"]%>
                        </td>
                        <td class="left ltr" colspan="2">
                            <asp:TextBox ID="txtUrl" runat="server" Width="320px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="right rtl">
                            <%= Farschidus.Translator.AppTranslate["mediaManaging.default.addEdit.fileName"]%>
                        </td>
                        <td class="left languageRtl">
                            <asp:TextBox ID="txtFileName" runat="server" Width="320px"></asp:TextBox>
                        </td>
                        <td>
                            <asp:RequiredFieldValidator ID="rfvFileName" runat="server" ErrorMessage="*" ControlToValidate="txtFileName"
                                ValidationGroup="ValGroup"></asp:RequiredFieldValidator>
                        </td>
                    </tr>

                    <tr>
                        <td class="right rtl top">
                            <%= Farschidus.Translator.AppTranslate["mediaManaging.default.addEdit.description"]%>
                        </td>
                        <td class="left languageRtl" colspan="2">
                            <%--<uc1:tinyMCE runat="server" ID="tinyMCE" />--%>
                            <asp:TextBox ID="txtDescription" runat="server" Width="320px" TextMode="MultiLine"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="right languageRtl" colspan="3">
                            <asp:Literal ID="litPopupImage" runat="server"></asp:Literal>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
