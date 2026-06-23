using BLL.BusinessEntity;
using Farschidus.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

public partial class PSM_Forms_Default : BaseCP
{
    #region "Properties"

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
                subjects.LoadByIDSubjectType((byte)SubjectTypes.Enum.form);
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
    public int? pIDProperty
    {
        get
        {
            if (ViewState["pIDProperty"] == null)
            {
                return null;
            }
            return (int)ViewState["pIDProperty"];
        }
        set
        {
            ViewState["pIDProperty"] = value;
        }
    }

    #endregion

    #region "Events"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            pShowMediaManager = pShowBannerManager = pShowPluginManager = pShowPropertyManager = pShowThumbnailManager = true;
            pShownSearchButton = false;
            grvList.EmptyDataText = Farschidus.Translator.AppTranslate["general.message.gridsEmptyDataText"];
            Title = Farschidus.Translator.AppTranslate["form.default.page.title"];
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
        pListMode = ListMode.LoadAll;
        mClear();
        mLoadList();
    }
    protected override void ddlLanguages_SelectedIndexChanged(object sender, EventArgs e)
    {
        mLoadAll();
    }
    protected void grvList_RowDraged(object sender, GridViewRowDragedEventArgs e)
    {
        string daraggedPriority = grvList.DataKeys[e.DragedRowIndex][Subjects.ColumnNames.Priority].ToString();
        string targetPriority = grvList.DataKeys[e.TargetRowIndex][Subjects.ColumnNames.Priority].ToString();
        bool insertBefore = (e.Status == DragStatus.Before);
        this.ReOrder(pSubjects, daraggedPriority, targetPriority, insertBefore);
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
            (byte)SubjectTypes.Enum.form,
            null,
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

        grvList.DataSource = subjects.DefaultView;
        grvList.DataBind();
        uplList.Update();
    }
    private void mSearch()
    {
        int itemCount = 0;
        Subjects subjects = new Subjects();

        string title = "";
        if (!string.IsNullOrEmpty(txtTitleSearch.Text))
        {
            title = txtTitleSearch.Text.Trim();
        }

        subjects.Search(listPager.CurrentIndex - 1, listPager.PageSize, ref itemCount,
            null,
            Convert.ToByte(SubjectTypes.Enum.form),
            null,
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
            SubjectTypes.ColumnNames.Priority);
        listPager.ItemCount = itemCount;

        grvList.DataSource = subjects.DefaultView;
        grvList.DataBind();
        uplList.Update();
    }
    private void mFillForm()
    {
        Subjects subject = new Subjects(pIDSubject.Value);

        txtAlias.Text = subject.pAlias;
        txtTitle.Text = subject.pTitle;
        HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
        TCMEValue.Value = subject.pBody;
        mSetPopupData(subject);

        uplAddEdit.Update();
    }
    private bool mValidateAddEdit()
    {
        pMessage.Clear();
        bool isValid = true;
        if (!Global.MethodsAndProps.mIsAliasUnique(txtAlias.Text, (byte)SubjectTypes.Enum.form, pLanguageID, pIDSubject))
        {
            isValid = false;
            pMessage.Add(Farschidus.Translator.AppTranslate["general.message.aliasDuplication"], Farschidus.Web.UI.Message.MessageTypes.Error);
        }
        if (string.IsNullOrEmpty(txtAlias.Text) || string.IsNullOrEmpty(txtTitle.Text))
        {
            isValid = false;
            pMessage.Add(Farschidus.Translator.AppTranslate["general.message.fieldsEmpty"], Farschidus.Web.UI.Message.MessageTypes.Error);
        }
        return isValid;
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

                subjects.pIDSubjectType = (byte)SubjectTypes.Enum.form;
                subjects.pIDLanguage = pLanguageID;
                subjects.pTitle = txtTitle.Text;
                HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
                subjects.pBody = System.Web.HttpUtility.HtmlDecode(TCMEValue.Value);
                subjects.pIsActive = true;

                subjects.pAlias = Global.MethodsAndProps.mAliasCorrection(txtAlias.Text);
                subjects.Save();

                if (pIDSubject.HasValue)
                    Global.MethodsAndProps.mUpdateSiteMap(subjects.pIDSubject.ToString(), subjects.pAlias, Global.Constants.STRING_LIST_MODULE);

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
    private bool mValidateDelete(Guid iDSubject)
    {
        bool result = true;
        pMessage.Clear();
        //Subjects subjects = new Subjects();
        //subjects.LoadByIDParent(iDSubject);
        //if (subjects.RowCount > 0)
        //{
        //result = false;
        //pMessage.Add(Farschidus.Translator.AppTranslate["form.default.message.thereIsPage"], Farschidus.Web.UI.Message.MessageTypes.Warning);
        //pMessage.Add(Farschidus.Translator.AppTranslate["form.default.message.deletePageFirst"], Farschidus.Web.UI.Message.MessageTypes.Information);

        //mShowMessage(pMessage);
        //}
        return result;
    }
    private void mDelete(Guid iDSubject, bool loadList = true)
    {
        if (mValidateDelete(iDSubject))
        {
            try
            {
                //TODO: Alert the user for existing Properties and their Values
                SubjectPropertyValues spv = new SubjectPropertyValues();
                spv.LoadByIDSubject(iDSubject);
                spv.DeleteAll();
                spv.Save();
                SubjectProperties subjectProperties = new SubjectProperties();
                subjectProperties.LoadByIDSubject(iDSubject);
                subjectProperties.DeleteAll();
                subjectProperties.Save();
                /////////////////////////////////////////////////////////////////

                MediaSubjects mediaSubjects = new MediaSubjects();
                mediaSubjects.LoadByIDSubject(iDSubject);
                mediaSubjects.DeleteAll();
                mediaSubjects.Save();

                SubjectPlugins subjectPlugin = new SubjectPlugins();
                subjectPlugin.LoadByIDSubject(iDSubject);
                subjectPlugin.DeleteAll();
                subjectPlugin.Save();

                GalleryPlugins galleryPlugins = new GalleryPlugins();
                galleryPlugins.LoadByIDSubject(iDSubject);
                galleryPlugins.DeleteAll();
                galleryPlugins.Save();

                Subjects subjects = new Subjects(iDSubject);
                subjects.MarkAsDeleted(false);
                subjects.Save();

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
    }
    private void mClear()
    {
        HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
        txtAlias.Text = txtTitle.Text = TCMEValue.Value = string.Empty;
        pIDSubject = null;
        uplAddEdit.Update();
    }

    private int mSetPriority()
    {
        Subjects subjects = new Subjects();
        subjects.LoadByIDSubjectType((byte)SubjectTypes.Enum.form);
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
    private void ReOrder(Subjects subjects, string draggedPriority, string targetPriority, bool insertBefore)
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
        if (!string.IsNullOrEmpty(subjects.Filter))
        {
            baseFilter = subjects.Filter + " AND ";
        }

        // Step 1: Temporarily mark the dragged item to avoid conflicts during shifting
        subjects.Filter = baseFilter + string.Format("{0}={1}", Subjects.ColumnNames.Priority, draggedPriorityValue);
        if (subjects.RowCount > 0)
        {
            subjects.pPriority = 0;
        }

        // Step 2: Shift items in the affected range in one pass
        subjects.Filter = baseFilter + rangeFilter;
        if (subjects.RowCount > 0)
        {
            do
            {
                subjects.pPriority += shiftAmount;
            } while (subjects.MoveNext());
        }

        // Step 3: Place the dragged item at its final position
        subjects.Filter = baseFilter + string.Format("{0}=0", Subjects.ColumnNames.Priority);
        if (subjects.RowCount > 0)
        {
            subjects.pPriority = finalPriority;
        }

        pSubjects = subjects;
        pSubjects.Save();

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