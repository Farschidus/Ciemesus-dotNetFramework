using BLL.BusinessEntity;
using Farschidus.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

public partial class PSM_List_Default : BaseCP
{
    #region "Properties"

    public Guid? pSubjectTypeID
    {
        get
        {
            if (ViewState["pSubjectTypeID"] == null)
            {
                return null;
            }
            return new Guid(ViewState["pSubjectTypeID"].ToString());
        }
        set
        {
            ViewState["pSubjectTypeID"] = value;
        }
    }
    public Guid? pIDSubject
    {
        get
        {
            if (ViewState["pIDSubject"] == null)
            {
                return null;
            }
            return new Guid(ViewState["pIDSubject"].ToString());
        }
        set
        {
            ViewState["pIDSubject"] = value;
        }
    }
    public Subjects pSubjects
    {
        get
        {
            Subjects subjects = new Subjects();
            if (ViewState["pSubjects"] == null)
            {
                subjects.LoadByIDParentAndIDSubjectTypeAndIDLanguage(pSubjectTypeID.Value, (byte)SubjectTypes.Enum.listItem, pLanguageID);
                subjects.Sort = Subjects.ColumnNames.Priority;
                ViewState["pSubjects"] = subjects.Serialize();
            }
            else
            {
                subjects.Deserialize(ViewState["pSubjects"].ToString());
            }
            return subjects;
        }
        set
        {
            ViewState["pSubjects"] = value.Serialize();
        }
    }

    #endregion

    #region "Events"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            pShowMediaManager = pShowBodyGalleryManager = pShowBannerManager = pShowPluginManager = pShowThumbnailManager = pShowPropertyValueManager = true;
            grvList.EmptyDataText = Farschidus.Translator.AppTranslate["general.message.gridsEmptyDataText"];
            Title = Farschidus.Translator.AppTranslate["listItemManaging.default.page.title"];
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
        pListMode = ListMode.LoadAll;
        mClear();
    }
    protected override void btnSubmit_Click(object sender, EventArgs e)
    {
        mSave();
        uplAddEdit.Update();
    }
    protected override void btnDelete_Click(object sender, EventArgs e)
    {
        pMessage.Clear();
        bool hasSelect = false;
        Guid item;
        Subjects subject = new Subjects();
        foreach (GridViewRow grvRow in grvList.Rows)
        {
            if (((CheckBox)grvRow.FindControl("chkList")).Checked)
            {
                item = new Guid(grvList.DataKeys[grvRow.RowIndex][Subjects.ColumnNames.IDSubject].ToString());
                subject.LoadByPrimaryKey(item);
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
        mClear();
        pListMode = ListMode.LoadAll;
        mLoadList();
    }
    protected override void ddlLanguages_SelectedIndexChanged(object sender, EventArgs e)
    {
        mDDLListTypeBinding();
        mLoadAll();
    }
    protected void ddlListType_SelectedIndexChanged(object sender, EventArgs e)
    {
        pSubjectTypeID = new Guid(ddlListType.SelectedValue);
        pListMode = ListMode.LoadAll;
        listPager.CurrentIndex = 1;
        mLoadList();
    }
    protected void grvList_RowDraged(object sender, GridViewRowDragedEventArgs e)
    {
        string daraggedPriority = grvList.DataKeys[e.DragedRowIndex][Subjects.ColumnNames.Priority].ToString();
        string targetPriority = grvList.DataKeys[e.TargetRowIndex][Subjects.ColumnNames.Priority].ToString();
        bool direction = !(e.Status == Farschidus.Web.UI.WebControls.DragStatus.After);
        this.ReOrder(pSubjects, daraggedPriority, targetPriority, direction);
    }
    protected void grvList_RowEditing(object sender, GridViewEditEventArgs e)
    {
        pIDSubject = new Guid(grvList.DataKeys[Convert.ToInt32(e.NewEditIndex)][0].ToString());
        e.Cancel = true;
        pListMode = ListMode.Edit;
        mFillForm();
    }
    protected void grvList_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        Guid iDSubject = new Guid(grvList.DataKeys[Convert.ToInt32(e.RowIndex)][0].ToString());
        if (mValidateDelete(iDSubject))
        {
            mDelete(iDSubject);
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
        mDDLListTypeBinding();
        mLoadList();
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
        Subjects subjects = new Subjects();

        subjects.Search(listPager.CurrentIndex - 1, listPager.PageSize, ref itemCount,
            null,
            (byte)SubjectTypes.Enum.listItem,
            pSubjectTypeID,
            pLanguageID,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            Subjects.ColumnNames.Priority);

        listPager.ItemCount = itemCount;
        //subjects.Sort = Subjects.ColumnNames.Date + " Desc";
        subjects.Sort = Subjects.ColumnNames.Priority;

        grvList.DataSource = subjects.DefaultView;
        grvList.DataBind();
        uplList.Update();
    }
    private void mSearch()
    {
        int itemCount = 0;
        Subjects subjects = new Subjects();

        Guid searchListTypeID = new Guid(ddlListTypeSearch.SelectedValue);

        string title = "";
        if (!string.IsNullOrEmpty(txtTitleSearch.Text))
        {
            title = txtTitleSearch.Text.Trim();
        }
        string alias = "";
        if (!string.IsNullOrEmpty(txtAliasSearch.Text))
        {
            alias = Global.MethodsAndProps.mAliasCorrection(txtAliasSearch.Text.Trim());
        }

        subjects.Search(listPager.CurrentIndex - 1, listPager.PageSize, ref itemCount,
            null,
            (byte)SubjectTypes.Enum.listItem,
            searchListTypeID,
            pLanguageID,
            null,
            title,
            null,
            alias,
            null,
            null,
            null,
            null,
            null,
            Subjects.ColumnNames.Priority);

        listPager.ItemCount = itemCount;
        //subjects.Sort = Subjects.ColumnNames.Date + " Desc";
        subjects.Sort = Subjects.ColumnNames.Priority;

        grvList.DataSource = subjects.DefaultView;
        grvList.DataBind();
        ddlListType.SelectedValue = ddlListTypeSearch.SelectedValue;
        pSubjectTypeID = searchListTypeID;
        uplList.Update();
    }
    private void mFillForm()
    {
        Subjects subject = new Subjects();
        subject.LoadByIDSubjectAndIDLanguage(pIDSubject.Value, pLanguageID);

        txtDate.Text = subject.pDate.ToString(Global.MethodsAndProps.DateFormat);
        txtAlias.Text = subject.pAlias;
        txtTitle.Text = subject.pTitle;
        HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
        TCMEValue.Value = subject.pBody;
        cbxIsActive.Checked = subject.pIsActive;

        mSetPopupData(subject);

        uplAddEdit.Update();
    }
    private bool mValidateAddEdit()
    {
        pMessage.Clear();
        bool isValid = true;
        if (!Global.MethodsAndProps.mIsAliasUnique(txtAlias.Text, (byte)SubjectTypes.Enum.listItem, pLanguageID, pIDSubject))
        {
            isValid = false;
            pMessage.Add(Farschidus.Translator.AppTranslate["general.message.aliasDuplication"], Farschidus.Web.UI.Message.MessageTypes.Error);
        }
        if (!pSubjectTypeID.HasValue)
        {
            isValid = false;
            pMessage.Add(Farschidus.Translator.AppTranslate["listItemManaging.default.message.noSubjectTypeID"], Farschidus.Web.UI.Message.MessageTypes.Error);
            pMessage.Add(Farschidus.Translator.AppTranslate["listItemManaging.default.message.createList"], Farschidus.Web.UI.Message.MessageTypes.Information);
        }
        if (string.IsNullOrEmpty(txtTitle.Text) || string.IsNullOrEmpty(txtAlias.Text))
        {
            isValid = false;
            pMessage.Add(Farschidus.Translator.AppTranslate["general.message.fieldsEmpty"], Farschidus.Web.UI.Message.MessageTypes.Error);
        }
        return isValid;
    }
    private bool mValidateDelete(Guid iDSubject)
    {
        return true;
    }
    private void mSave()
    {
        if (mValidateAddEdit())
        {
            DAL.GlobalCore.TransactionMgr tx = DAL.GlobalCore.TransactionMgr.ThreadTransactionMgr();
            bool isNew = false;
            try
            {
                tx.BeginTransaction();
                Subjects subjects = new Subjects();
                if (pIDSubject.HasValue)
                {
                    subjects.LoadByPrimaryKey(pIDSubject.Value);
                }
                else
                {
                    subjects.AddNew();
                    subjects.pPriority = mSetPriority();
                    pIDSubject = subjects.pIDSubject = Guid.NewGuid();
                    subjects.pDate = DateTime.UtcNow.AddHours(Global.MethodsAndProps.TimeZone);
                    isNew = true;
                }

                subjects.pIDSubjectType = (byte)SubjectTypes.Enum.listItem;
                subjects.pIDLanguage = pLanguageID;
                subjects.pTitle = txtTitle.Text;
                HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
                subjects.pBody = System.Web.HttpUtility.HtmlDecode(TCMEValue.Value);
                subjects.pIsActive = cbxIsActive.Checked;
                subjects.pIDParent = pSubjectTypeID.Value;
                subjects.pDate = Convert.ToDateTime(string.Format("{0} {1}", Convert.ToDateTime(txtDate.Text).ToString(Global.MethodsAndProps.DateFormat), DateTime.UtcNow.AddHours(Global.MethodsAndProps.TimeZone).ToString("HH:mm:ss")));
                subjects.pAlias = Global.MethodsAndProps.mAliasCorrection(txtAlias.Text);
                subjects.Save();

                //mUpdateSiteMap = No Need, as there is "List Name" for urls in Sitemaps
                if (isNew)
                    mSetPopupData(subjects);

                tx.CommitTransaction();
                pMessage.Clear();
                pMessage.Add(Farschidus.Translator.AppTranslate["general.message.success"], Farschidus.Web.UI.Message.MessageTypes.Success);
                mShowMessage(pMessage);
            }
            catch (Exception ex)
            {
                tx.RollbackTransaction();
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
    private void mDelete(Guid iDSubject, bool loadList = true)
    {
        DAL.GlobalCore.TransactionMgr tx = DAL.GlobalCore.TransactionMgr.ThreadTransactionMgr();
        try
        {
            tx.BeginTransaction();
            MediaSubjects mediaSubjects = new MediaSubjects();
            mediaSubjects.LoadByIDSubject(iDSubject);
            mediaSubjects.DeleteAll();
            mediaSubjects.Save();

            SubjectPlugins subjectPlugin = new SubjectPlugins();
            subjectPlugin.LoadByIDSubject(iDSubject);
            subjectPlugin.DeleteAll();
            subjectPlugin.Save();

            GalleryPlugins GalleryPlugins = new GalleryPlugins();
            GalleryPlugins.LoadByIDSubject(iDSubject);
            GalleryPlugins.DeleteAll();
            GalleryPlugins.Save();

            Subjects subject = new Subjects(iDSubject);
            subject.MarkAsDeleted(false);
            subject.Save();

            tx.CommitTransaction();
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
            tx.RollbackTransaction();
            pMessage.Clear();
            pMessage.Add(ex.Message, Farschidus.Web.UI.Message.MessageTypes.Error);
            mShowMessage(pMessage);
        }
    }
    private void mClear()
    {
        HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
        txtAlias.Text = txtTitle.Text = TCMEValue.Value = string.Empty;
        cbxIsActive.Checked = true;
        pIDSubject = null;
        txtDate.Text = DateTime.UtcNow.AddHours(Global.MethodsAndProps.TimeZone).ToString(Global.MethodsAndProps.DateFormat);
        calExtDate.Format = Global.MethodsAndProps.DateFormat;      
        uplAddEdit.Update();
    }

    private void mDDLListTypeBinding()
    {
        ddlListType.Items.Clear();
        ddlListTypeSearch.Items.Clear();
        List<BLL.Hardcodes.Item> ddlItems = new List<BLL.Hardcodes.Item>();

        Subjects subjects = new Subjects();
        subjects.LoadAllListByIDLanguage(pLanguageID);
        subjects.Sort = Subjects.ColumnNames.Priority;
        if (subjects.RowCount > 0)
        {
            do
            {
                List<string> items = new List<string>();
                mLoadRecursivlyByParentID(ref items, subjects.pIDSubject, subjects.pTitle);
                items.Reverse();
                ddlItems.Add(new BLL.Hardcodes.Item(subjects.pIDSubject, string.Join(" / ", items.ToArray())));
            }
            while (subjects.MoveNext());
            subjects.Rewind();
            pSubjectTypeID = subjects.pIDSubject;
            pShownCreateButton = true;
        }
        else
        {
            ListItem liInitial = new ListItem(Farschidus.Translator.AppTranslate["listItemManaging.default.ddlListType.noList"], "0");
            ddlListType.Items.Add(liInitial);
            ddlListTypeSearch.Items.Add(liInitial);
            pSubjectTypeID = null;
            pShownCreateButton = false;
        }
        pUpdatePanelToolbarButtons.Update();

        ddlListType.DataSource = ddlListTypeSearch.DataSource = ddlItems;
        ddlListTypeSearch.DataTextField = ddlListType.DataTextField = BLL.Hardcodes.Item.FIELD_Title;
        ddlListTypeSearch.DataValueField = ddlListType.DataValueField = BLL.Hardcodes.Item.FIELD_ID;
        ddlListType.DataBind();
        ddlListTypeSearch.DataBind();
        ddlListType.SelectedIndex = 0;
        ddlListTypeSearch.SelectedIndex = 0;
        uplSearch.Update();
    }
    private int mSetPriority()
    {
        Subjects subjects = new Subjects();
        subjects.LoadByIDParent(pSubjectTypeID.Value);
        subjects.Sort = Subjects.ColumnNames.Priority;
        if (subjects.RowCount > 0)
        {
            subjects.MoveTo(subjects.RowCount - 1);
            if (!subjects.IsColumnNull(Subjects.ColumnNames.Priority))
            {
                return subjects.pPriority + 1;
            }
            else
            {
                subjects.Rewind();
                int i = 1;
                if (subjects.RowCount > 0)
                {
                    do
                    {
                        subjects.pPriority = i;
                        i++;
                    }
                    while (subjects.MoveNext());
                }
                return i++;
            }
        }
        else
        {
            return 1;
        }
    }
    private void ReOrder(Subjects unorderedSubjects, string draggedPriority, string targetPriority, bool direction)
    {
        string initFilter = "";
        if (!string.IsNullOrEmpty(unorderedSubjects.Filter))
        {
            initFilter = unorderedSubjects.Filter + " AND ";
        }
        unorderedSubjects.Filter = initFilter + string.Format("{0}={1}", Subjects.ColumnNames.Priority, draggedPriority);
        unorderedSubjects.pPriority = -1;

        if (direction)
        {
            unorderedSubjects.Filter = initFilter + string.Format("{0}>={1} AND {0} < {2}", Subjects.ColumnNames.Priority, targetPriority, draggedPriority);
            if (unorderedSubjects.RowCount > 0)
            {
                do
                {
                    unorderedSubjects.pPriority += 1;
                } while (unorderedSubjects.MoveNext());
            }
        }
        else
        {
            unorderedSubjects.Filter = initFilter + string.Format("{0}>{1} AND {0} <= {2}", Subjects.ColumnNames.Priority, draggedPriority, targetPriority);
            if (unorderedSubjects.RowCount > 0)
            {
                do
                {
                    unorderedSubjects.pPriority -= 1;
                } while (unorderedSubjects.MoveNext());
            }
        }
        unorderedSubjects.Filter = initFilter + string.Format("{0}={1}", Subjects.ColumnNames.Priority, "-1");
        unorderedSubjects.pPriority = Convert.ToInt32(targetPriority);

        pSubjects = unorderedSubjects;
        Subjects subjects = new Subjects();
        subjects = pSubjects;
        subjects.Save();

        mLoadAll();

        pMessage.Add(Farschidus.Translator.AppTranslate["general.message.reordered"], Farschidus.Web.UI.Message.MessageTypes.Success);
        mShowMessage(pMessage);
    }
    private void mSetPopupData(Subjects subject)
    {
        hdfData.Value = string.Format("{0}|{1}", subject.pIDSubject.ToString(), pLanguageID);
        pUpdatePanelToolbarButtons.Update();
    }

    private void mLoadRecursivlyByParentID(ref List<string> ddlItems, Guid subjectID, string title)
    {
        Subjects subjects = new Subjects(subjectID);
        subjects.Sort = Subjects.ColumnNames.Priority;
        if (subjects.RowCount > 0)
        {
            do
            {
                ddlItems.Add(subjects.pTitle);
                if (!subjects.IsColumnNull(Subjects.ColumnNames.IDParent))
                    mLoadRecursivlyByParentID(ref ddlItems, subjects.pIDParent, title);
            }
            while (subjects.MoveNext());
        }
        else
        {
            ddlItems.Add(title);
        }
    }

    #endregion

    #region "Public Methodes"

    public string mGetGalleryTypeName(string type)
    {
        if (!string.IsNullOrEmpty(type))
        {
            Subjects.BannerTypes headerTypes = (Subjects.BannerTypes)Enum.Parse(typeof(Subjects.BannerTypes), type);
            return Farschidus.Translator.AppTranslate["headerTypes.title." + headerTypes.ToString()];
        }
        else
        {
            return Farschidus.Translator.AppTranslate["headerTypes.title.inActive"];
        }
    }

    #endregion
}