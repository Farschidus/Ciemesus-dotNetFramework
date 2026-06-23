using BLL.BusinessEntity;
using Farschidus.Web.UI.WebControls;
using System;
using System.IO;
using System.Web.UI.WebControls;
using System.Xml;

public partial class PSM_Languages_Default : BaseCP
{
    #region "Properties"

    public byte? pIDLanguage
    {
        get
        {
            if (ViewState["pIDLanguage"] == null)
            {
                return null;
            }
            return (byte)ViewState["pIDLanguage"];
        }
        set
        {
            ViewState["pIDLanguage"] = value;
        }
    }
    public Languages pLanguages
    {
        get
        {
            Languages languages = new Languages();
            if (ViewState["pLanguages"] == null)
            {
                languages.LoadAll();
                ViewState["pLanguages"] = languages.Serialize();
            }
            else
            {
                languages.Deserialize(ViewState["pLanguages"].ToString());
            }
            return languages;
        }
        set
        {
            ViewState["pLanguages"] = value.Serialize();
        }
    }
    // private Farschidus.Web.UI.Message pMessage = new Farschidus.Web.UI.Message();

    #endregion

    #region "Events"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            pShownLanguageDDL = false;
            Title = Farschidus.Translator.AppTranslate["languages.default.page.title"];
            mInitialBindings();
        }
        listPager.PageSizeSelectClause = Farschidus.Translator.AppTranslate["general.label.pager.pageSizeSelectClause"];
        listPager.GoToClause = Farschidus.Translator.AppTranslate["general.label.pager.goToClause"];
    }
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        pListMode = ListMode.Search;
        listPager.CurrentIndex = 1;

        mLoadList();
    }
    protected override void btnLoadAll_Click(object sender, EventArgs e)
    {
        pListMode = ListMode.LoadAll;
        listPager.CurrentIndex = 1;
        mLoadList();
    }
    protected override void btnCreate_Click(object sender, EventArgs e)
    {
        mClear();
    }
    protected override void btnSubmit_Click(object sender, EventArgs e)
    {
        mSave();
    }
    protected override void btnDelete_Click(object sender, EventArgs e)
    {
        pMessage.Clear();
        bool hasSelect = false;
        byte item;
        Languages lang = new Languages();
        foreach (GridViewRow grvRow in grvList.Rows)
        {
            if (((CheckBox)grvRow.FindControl("chkList")).Checked)
            {
                //item = grvList.DataKeys[grvRow.RowIndex][Subjects.ColumnNames.IDSubject].ToString());
                item = Convert.ToByte(grvList.DataKeys[grvRow.RowIndex][Languages.ColumnNames.IDLanguage].ToString());
                lang.LoadByPrimaryKey(item);
                if (mValidateDelete(item))
                {
                    mDelete(item, false);
                    hasSelect = true;
                }
            }
        }
        if (hasSelect)
        {
            mLoadAll();
            pMessage.Add(Farschidus.Translator.AppTranslate["general.message.deleted"], Farschidus.Web.UI.Message.MessageTypes.Success);
        }
        mShowMessage(pMessage);
    }
    protected override void btnCancel_Click(object sender, EventArgs e)
    {
        mLoadList();
    }
    protected void grvList_RowDraged(object sender, GridViewRowDragedEventArgs e)
    {
        string daraggedPriority = grvList.DataKeys[e.DragedRowIndex][Languages.ColumnNames.Priority].ToString();
        string targetPriority = grvList.DataKeys[e.TargetRowIndex][Languages.ColumnNames.Priority].ToString();
        bool insertBefore = (e.Status == DragStatus.Before);
        this.ReOrder(pLanguages, daraggedPriority, targetPriority, insertBefore);
    }
    protected void grvList_RowEditing(object sender, GridViewEditEventArgs e)
    {
        mClear();
        pIDLanguage = Convert.ToByte(grvList.DataKeys[Convert.ToInt32(e.NewEditIndex)][0]);
        e.Cancel = true;
        mFillForm();
    }
    protected void grvList_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        byte iDLanguage = Convert.ToByte(grvList.DataKeys[Convert.ToInt32(e.RowIndex)][0]);
        if (mValidateDelete(iDLanguage))
        {
            mDelete(iDLanguage);
        }
    }
    protected void Pager_PageIndexChanged(object sender, PagerPageIndexChangeEventArgs e)
    {
        mLoadList();
    }
    protected void Pager_PageSizeChanged(object sender, PagerPageSizeChangeEventArgs e)
    {
        mLoadList();
    }

    #endregion

    #region "Private Methodes"

    private void mInitialBindings()
    {
        mLoadList();

        //// Put ddls loading here like the example below:
        //PhaseStatuses statuses = new PhaseStatuses();
        //statuses.LoadAll();
        //ddlPhaseStatuses.DataSource = statuses.DefaultView;
        //ddlPhaseStatuses.DataTextField = DAL.DataAccess._PhaseStatuses.ColumnNames.Title;
        //ddlPhaseStatuses.DataValueField = DAL.DataAccess._PhaseStatuses.ColumnNames.IDPhaseStatus;
        //ddlPhaseStatuses.DataBind();
    }
    private void mLoadList()
    {
        switch (pListMode)
        {
            case ListMode.LoadAll:
                mLoadAll();
                break;
            case ListMode.Search:
                mSearch();
                break;
        }
    }
    private void mLoadAll()
    {
        int itemCount = 0;
        Languages languages = new Languages();
        languages.LoadAll(listPager.CurrentIndex - 1, listPager.PageSize, ref itemCount, null);
        languages.Sort = Languages.ColumnNames.Priority;

        listPager.ItemCount = itemCount;

        grvList.DataSource = languages.DefaultView;
        grvList.DataBind();
        uplList.Update();
    }
    private void mSearch()
    {
        int itemCount = 0;
        Languages languages = new Languages();

        string title = "";
        if (!string.IsNullOrEmpty(txtTitleSearch.Text))
        {
            title = txtTitleSearch.Text.Trim();
        }
        string code = "";
        if (!string.IsNullOrEmpty(txtCodeSearch.Text))
        {
            code = txtCodeSearch.Text.Trim();
        }
        bool? isRTL = cbxIsRTLSearch.Checked;
        bool? isActive = cbxIsActiveSearch.Checked;
        bool? isDefault = cbxIsDefaultSearch.Checked;

        languages.Search(listPager.CurrentIndex - 1, listPager.PageSize, ref itemCount,
             null,
             title,
             code,
             isRTL,
             isActive,
             isDefault,
             null,
             null);
        listPager.ItemCount = itemCount;

        grvList.DataSource = languages.DefaultView;
        grvList.DataBind();
        uplList.Update();
    }
    private void mFillForm()
    {
        Languages languages = new Languages(pIDLanguage.Value);

        txtTitle.Text = languages.pTitle;
        txtCode.Text = languages.pCode;
        cbxIsRTL.Checked = languages.pIsRTL;
        cbxIsActive.Checked = languages.pIsActive;
        cbxIsDefault.Checked = languages.pIsDefault;

        uplAddEdit.Update();
    }
    private bool mValidateAddEdit()
    {
        pMessage.Clear();
        bool isValid = true;
        // Validation logic goes here (Validate Example is in Phases Default page)
        return isValid;
    }
    private bool mValidateDelete(byte iDLanguage)
    {
        return true;
    }
    private void mSave()
    {
        if (mValidateAddEdit())
        {
            try
            {
                Languages languages = new Languages();
                if (pIDLanguage.HasValue)
                {
                    languages.LoadByPrimaryKey(pIDLanguage.Value);
                }
                else
                {
                    languages.AddNew();
                    languages.pPriority = mSetPriority();
                }

                languages.pTitle = txtTitle.Text;
                languages.pCode = txtCode.Text;
                languages.pIsRTL = cbxIsRTL.Checked;
                languages.pIsActive = cbxIsActive.Checked;
                languages.pIsDefault = cbxIsDefault.Checked;
                if (cbxIsDefault.Checked)
                    mFalseAllRecordsForDefault();

                if (cbxIsActive.Checked)
                {
                    string LocaleFile = string.Format("{0}{1}.xml", Global.Constants.FOLDER_CLIENT_LOCALE, txtCode.Text);
                    if (!File.Exists(Server.MapPath(LocaleFile)))
                    {
                        string translateTemplateFile = string.Format("{0}{1}", Global.Constants.FOLDER_CLIENT_LOCALE, Global.Constants.FILE_TRANSLATOR_TEMPLATE_XML);
                        XmlDocument doc = new XmlDocument();
                        doc.Load(Server.MapPath(translateTemplateFile));
                        XmlNode root = doc.DocumentElement;
                        root.Attributes["name"].Value = txtCode.Text;
                        root.Attributes["full_name"].Value = txtTitle.Text;
                        doc.Save(Server.MapPath(LocaleFile));
                    }
                }

                languages.Save();

                if (cbxIsDefault.Checked)
                    Farschidus.Translator.SetPublicDefaultLanguage(languages.pIDLanguage.ToString());

                pMessage.Clear();
                pMessage.Add(Farschidus.Translator.AppTranslate["general.message.success"], Farschidus.Web.UI.Message.MessageTypes.Success);
                mShowMessage(pMessage);

                if (!pIDLanguage.HasValue)
                {
                    mClear();
                }
            }
            catch (Exception ex)
            {
                pMessage.Clear();
                pMessage.Add(ex.Message, Farschidus.Web.UI.Message.MessageTypes.Error);
                mShowMessage(pMessage);
            }
        }
        else
        {
            mShowMessage(pMessage);
        }
    }
    private void mDelete(byte iDLanguage, bool loadList = true)
    {
        try
        {
            Languages languages = new Languages(iDLanguage);
            languages.MarkAsDeleted(false);

            languages.Save();

            if (loadList)
            {
                pMessage.Clear();
                pMessage.Add(Farschidus.Translator.AppTranslate["general.message.deleted"], Farschidus.Web.UI.Message.MessageTypes.Success);
                mShowMessage(pMessage);

                mLoadList();
            }
        }
        catch (Exception ex)
        {
            pMessage.Clear();
            pMessage.Add(ex.Message, Farschidus.Web.UI.Message.MessageTypes.Error);
            mShowMessage(pMessage);
        }
    }
    private void mClear()
    {
        txtTitle.Text = txtCode.Text = string.Empty;
        cbxIsRTL.Checked = cbxIsActive.Checked = cbxIsDefault.Checked = false;
        pIDLanguage = null;
        uplAddEdit.Update();
    }

    private void mFalseAllRecordsForDefault()
    {
        Languages languages = new Languages();
        languages.LoadDefaultLanguage();
        if (languages.RowCount > 0)
        {
            languages.pIsDefault = false;
            languages.Save();
        }
    }
    private int mSetPriority()
    {
        Languages languages = new Languages();
        languages.LoadAll();
        languages.Sort = Languages.ColumnNames.Priority;
        if (languages.RowCount > 0)
        {
            languages.MoveTo(languages.RowCount - 1);
            if (!languages.IsColumnNull(Languages.ColumnNames.Priority))
            {
                return languages.pPriority + 1;
            }
            else
            {
                languages.Rewind();
                int i = 1;
                if (languages.RowCount > 0)
                {
                    do
                    {
                        languages.pPriority = i;
                        i++;
                    }
                    while (languages.MoveNext());
                }
                return i++;
            }
        }
        else
        {
            return 1;
        }

    }
    private void ReOrder(Languages languages, string draggedPriority, string targetPriority, bool insertBefore)
    {
        int draggedPriorityValue = Convert.ToInt32(draggedPriority);
        int targetPriorityValue = Convert.ToInt32(targetPriority);

        // Calculate the actual final priority and affected range based on move direction and insertBefore
        int finalPriority;
        string rangeFilter;
        int shiftAmount;

        if (draggedPriorityValue < targetPriorityValue)
        {
            // Moving DOWN: insertBefore=true means land just before target (t-1), false means land on target (t)
            finalPriority = insertBefore ? targetPriorityValue - 1 : targetPriorityValue;
            rangeFilter = string.Format("{0}>{1} AND {0}<={2}",
                Subjects.ColumnNames.Priority, draggedPriorityValue, finalPriority);
            shiftAmount = -1;
        }
        else
        {
            // Moving UP: insertBefore=true means land on target (t), false means land just after target (t+1)
            finalPriority = insertBefore ? targetPriorityValue : targetPriorityValue + 1;
            rangeFilter = string.Format("{0}>={1} AND {0}<{2}",
                Subjects.ColumnNames.Priority, finalPriority, draggedPriorityValue);
            shiftAmount = 1;
        }

        // No-op if the effective final position is the same as the current position
        if (finalPriority == draggedPriorityValue)
        {
            return;
        }

        string baseFilter = "";
        if (!string.IsNullOrEmpty(languages.Filter))
        {
            baseFilter = languages.Filter + " AND ";
        }

        // Step 1: Temporarily mark the dragged item to avoid conflicts during shifting
        languages.Filter = baseFilter + string.Format("{0}={1}", Languages.ColumnNames.Priority, draggedPriorityValue);
        if (languages.RowCount > 0)
        {
            languages.pPriority = 0;
        }

        // Step 2: Shift items in the affected range in one pass
        languages.Filter = baseFilter + rangeFilter;
        if (languages.RowCount > 0)
        {
            do
            {
                languages.pPriority += shiftAmount;
            } while (languages.MoveNext());
        }

        // Step 3: Place the dragged item at its final position
        languages.Filter = baseFilter + string.Format("{0}=0", Languages.ColumnNames.Priority);
        if (languages.RowCount > 0)
        {
            languages.pPriority = finalPriority;
        }

        pLanguages = languages;
        pLanguages.Save();

        mLoadAll();

        pMessage.Add(Farschidus.Translator.AppTranslate["general.message.reordered"], Farschidus.Web.UI.Message.MessageTypes.Success);
        mShowMessage(pMessage);
    }

    #endregion

    #region "Public Methodes"

    public string mGetDirection(bool isRTL)
    {
        if (isRTL)
            return Farschidus.Translator.AppTranslate["languages.default.labels.rtl"];
        else
            return Farschidus.Translator.AppTranslate["languages.default.labels.ltr"];
    }

    #endregion
}