using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Farschidus.Web.UI.WebControls;
using BLL.BusinessEntity;

public partial class PSM_Medias_Default : BaseCP
{
    #region "Properties"

    public int? pIDMedia
    {
        get
        {
            if (ViewState["pIDMedia"] == null)
            {
                return null;
            }
            return (int)ViewState["pIDMedia"];
        }
        set
        {
            ViewState["pIDMedia"] = value;
        }
    }
    private string pFileExtension
    {
        get
        {
            return Session["pFileExtension"].ToString();
        }
        set
        {
            Session["pFileExtension"] = value;
        }
    }
    private Guid pTempMediaID
    {
        get
        {
            if (ViewState["pTempMediaID"] == null)
            {
                ViewState["pTempMediaID"] = Guid.NewGuid();
            }
            return new Guid(ViewState["pTempMediaID"].ToString());
        }
        set
        {
            ViewState["pTempMediaID"] = value;
        }
    }

    #endregion

    #region "Events"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            pShownLanguageDDL = false;
            grvList.EmptyDataText = Farschidus.Translator.AppTranslate["general.message.gridsEmptyDataText"];
            Title = Farschidus.Translator.AppTranslate["mediaManaging.default.page.title"];            
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
        pTempMediaID = Guid.NewGuid();
    }
    protected override void btnSubmit_Click(object sender, EventArgs e)
    {
        mSave();
    }
    protected override void btnDelete_Click(object sender, EventArgs e)
    {
        pMessage.Clear();
        bool hasSelect = false;
        int item;
        Medias media = new Medias();
        foreach (GridViewRow grvRow in grvList.Rows)
        {
            if (((CheckBox)grvRow.FindControl("chkList")).Checked)
            {
                item =Convert.ToInt32(grvList.DataKeys[grvRow.RowIndex][Medias.ColumnNames.IDMedia].ToString());
                media.LoadByPrimaryKey(item);
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
        // Use "e" Arquments: e.Status, e.ComandName, e.ComandName, e.DragedRowIndex, e.TargetRowIndex;
    }
    protected void grvList_RowEditing(object sender, GridViewEditEventArgs e)
    {
        mClear();
        pIDMedia = Convert.ToInt32(grvList.DataKeys[Convert.ToInt32(e.NewEditIndex)][0]);
        e.Cancel = true;
        mFillForm();
    }
    protected void grvList_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        pIDMedia = Convert.ToInt32(grvList.DataKeys[Convert.ToInt32(e.RowIndex)][0]);
        int iDMedia = Convert.ToInt32(grvList.DataKeys[Convert.ToInt32(e.RowIndex)][0]);       
        mDelete(iDMedia);        
    }
    protected void Pager_PageIndexChanged(object sender, PagerPageIndexChangeEventArgs e)
    {
        mLoadList();
    }
    protected void Pager_PageSizeChanged(object sender, PagerPageSizeChangeEventArgs e)
    {
        mLoadList();
    }
    protected void fulImages_UploadedComplete(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
    {
        string strMediaFile = string.Empty;
        string strTempFolder = string.Empty;

        if (fulImage.HasFile)
        {
            strTempFolder = System.IO.Path.Combine(Global.Constants.FOLDER_TEMP);
            if (!System.IO.Directory.Exists(MapPath(strTempFolder)))
            {
                System.IO.Directory.CreateDirectory(MapPath(strTempFolder));
            }
            pFileExtension = System.IO.Path.GetExtension(fulImage.FileName);
            strMediaFile = strTempFolder + pTempMediaID + pFileExtension;
            fulImage.SaveAs(MapPath(strMediaFile));
        }
    }

    #endregion

    #region "Methodes"

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
        Medias medias = new Medias();
        medias.LoadAll(listPager.CurrentIndex - 1, listPager.PageSize, ref itemCount, null);
        
        listPager.ItemCount = itemCount;

        medias.Sort = Medias.ColumnNames.FileName;
        grvList.DataSource = medias.DefaultView;
        grvList.DataBind();
        uplList.Update();
    }
    private void mSearch()
    {
        int itemCount = 0;
        Medias medias = new Medias();

        string fileName = "";
        if (!string.IsNullOrEmpty(txtFileNameSearch.Text))
        {
            fileName = txtFileNameSearch.Text.Trim();
        }
        string fileExtention = "";
        if (!string.IsNullOrEmpty(txtFileExtentionSearch.Text))
        {
            fileExtention = txtFileExtentionSearch.Text.Trim();
        }
        string description = "";
        if (!string.IsNullOrEmpty(txtDescriptionSearch.Text))
        {
            description = txtDescriptionSearch.Text.Trim();
        }
        string url = "";
        if (!string.IsNullOrEmpty(txtUrlSearch.Text))
        {
            url = txtUrlSearch.Text.Trim();
        }

        medias.Search(listPager.CurrentIndex - 1, listPager.PageSize, ref itemCount,
         null,
         fileName,
         fileExtention,
         description,
         null,
         null,
         url,
         null);
        listPager.ItemCount = itemCount;
        medias.Sort = Medias.ColumnNames.FileName;
        grvList.DataSource = medias.DefaultView;
        grvList.DataBind();
        uplList.Update();
    }
    private void mFillForm()
    {
        Medias medias = new Medias(pIDMedia.Value);
        txtFileName.Text = medias.pFileName;
        pFileExtension = medias.pFileExtention;
        //HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
        //TCMEValue.Value = medias.pDescription;
        txtDescription.Text = medias.pDescription;
        txtUrl.Text = medias.pUrl;
        txtDate.Text = medias.pDate.ToString(Global.MethodsAndProps.DateFormat);

        string[] imageFile = System.IO.Directory.GetFiles(MapPath(Global.Constants.FOLDER_MEDIAS), medias.pIDMedia.ToString() + ".*");
        if (imageFile.Length > 0)
            litPopupImage.Text = string.Format(@"<a href='javascript:void(0)' onclick=""window.open('/{0}?{1}', 'File', 'height=515,width=990,top=120,left=10,scrollbars=yes,resizable=yes')"">{2}</a>",
                Global.Constants.FOLDER_MEDIAS.Substring(2) + System.IO.Path.GetFileName(imageFile[0]),
                DateTime.Now.ToString(),
                Farschidus.Translator.AppTranslate["ascx.media.popupLinks.title"]);

        uplAddEdit.Update();
    }
    private bool mValidateAddEdit()
    {
        pMessage.Clear();
        bool isValid = true;        
        if (!pIDMedia.HasValue)
        {
            string strMediaTempFolder = System.IO.Path.Combine(Global.Constants.FOLDER_TEMP);
            if (System.IO.Directory.Exists(MapPath(strMediaTempFolder)))
            {
               string[] fileEntries = System.IO.Directory.GetFiles(MapPath(strMediaTempFolder), pTempMediaID.ToString() + ".*");
                if (fileEntries.Length.Equals(0))
                {
                    isValid = false;
                    pMessage.Add(Farschidus.Translator.AppTranslate["ascx.media.message.noFile"], Farschidus.Web.UI.Message.MessageTypes.Error);
                }
            }
        }
        return isValid;
    }
    private bool mValidateDelete(int iDMedia)
    {        
        pMessage.Clear();
        bool isValid = true;        
        MediaSubjects mediaSubject = new MediaSubjects();
        mediaSubject.LoadByIDMedia(iDMedia);
        mediaSubject.Sort = MediaSubjects.ColumnNames.IDMediaSubjectType;

        if (mediaSubject.RowCount > 0)
        {            
            isValid = false;

            pMessage.Add(Farschidus.Translator.AppTranslate["mediaManaging.default.message.subjectMediaRelation"], Farschidus.Web.UI.Message.MessageTypes.Warning);
            Subjects subject = new Subjects();
            SubjectTypes subjectType = new SubjectTypes();
            MediaSubjectTypes mediaSubjectType = new MediaSubjectTypes();
            BLL.Hardcodes.Tables hcTables = new BLL.Hardcodes.Tables();
            string moduleName = string.Empty;
            string usageType = string.Empty;
            do
            {              
                subject.LoadByPrimaryKey(mediaSubject.pIDSubject);
                subjectType.LoadByPrimaryKey(subject.pIDSubjectType);
                mediaSubjectType.LoadByPrimaryKey(mediaSubject.pIDMediaSubjectType);
                moduleName = hcTables.SubjectTypes.Find(item => item.ID.ToString() == ((byte)((SubjectTypes.Enum)Enum.Parse(typeof(SubjectTypes.Enum), subjectType.pTitle, true))).ToString()).Title;
                usageType = hcTables.MediaSubjectTypes.Find(item => item.ID.ToString() == ((byte)((MediaSubjectTypes.Enum)Enum.Parse(typeof(MediaSubjectTypes.Enum), mediaSubjectType.pTitle, true))).ToString()).Title;
                pMessage.Add(string.Format("{0}, {1}: {2}", moduleName, mediaSubject.Subjects.pTitle, usageType), Farschidus.Web.UI.Message.MessageTypes.Information);                
            }
            while (mediaSubject.MoveNext());
        }        
        return isValid;
    }
    private void mSave()
    {
        if (mValidateAddEdit())
        {
            DAL.GlobalCore.TransactionMgr tx = DAL.GlobalCore.TransactionMgr.ThreadTransactionMgr();
            try
            {
                tx.BeginTransaction();
                Medias media = new Medias();

                if (pIDMedia.HasValue)
                {
                    media.LoadByPrimaryKey(pIDMedia.Value);
                }
                else
                {
                    media.AddNew();
                }

                media.pFileName = txtFileName.Text;
                media.pFileExtention = pFileExtension;
                //HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
                //media.pDescription = System.Web.HttpUtility.HtmlDecode(TCMEValue.Value);
                media.pDescription = txtDescription.Text;
                media.pDate = Convert.ToDateTime(string.Format("{0} {1}", Convert.ToDateTime(txtDate.Text).ToString(Global.MethodsAndProps.DateFormat), DateTime.UtcNow.AddHours(Global.MethodsAndProps.TimeZone).ToString("HH:mm:ss")));
                media.pUrl = txtUrl.Text;

                media.Save();
                mSaveMediaFile(media.pIDMedia.ToString());
                tx.CommitTransaction();

                pMessage.Clear();
                pMessage.Add(Farschidus.Translator.AppTranslate["general.message.success"], Farschidus.Web.UI.Message.MessageTypes.Success);
                mShowMessage(pMessage);

                if (!pIDMedia.HasValue)
                {
                    mClear();
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
        else
        {
            mShowMessage(pMessage);
        }
    }
    private void mDelete(int iDMedia, bool loadList = true)
    {
        if (mValidateDelete(iDMedia))
        {
            try
            {                
                string[] mediafile = System.IO.Directory.GetFiles(MapPath(Global.Constants.FOLDER_MEDIAS), iDMedia.ToString() + ".*");
                string[] thumbFile = System.IO.Directory.GetFiles(MapPath(Global.Constants.FOLDER_THUMBS), iDMedia.ToString() + ".*");

                Medias medias = new Medias(iDMedia);
                medias.MarkAsDeleted(false);

                if (mediafile.Length > 0)
                    System.IO.File.Delete(mediafile[0]);
                if (thumbFile.Length > 0)
                    System.IO.File.Delete(thumbFile[0]);

                medias.Save();

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
        mShowMessage(pMessage);
    }
    private void mClear()
    {
        mDeleteTempFiles();
        //HiddenField TCMEValue = (HiddenField)tinyMCE.FindControl("TCMEValue");
        //TCMEValue.Value = string.Empty;
        txtDescription.Text = string.Empty;
        litPopupImage.Text = txtFileName.Text = pFileExtension = txtUrl.Text = string.Empty;
        txtDate.Text = DateTime.UtcNow.AddHours(Global.MethodsAndProps.TimeZone).ToString(Global.MethodsAndProps.DateFormat);
        pIDMedia = null;
        uplAddEdit.Update();
    }

    private void mSaveMediaFile(string mediaID)
    {
        string strMediaTempFolder = System.IO.Path.Combine(Global.Constants.FOLDER_TEMP);
        string strMediaDestinationFolder = Global.Constants.FOLDER_MEDIAS;

        if (System.IO.Directory.Exists(MapPath(strMediaTempFolder)))
        {
            string[] fileEntries = System.IO.Directory.GetFiles(MapPath(strMediaTempFolder), pTempMediaID.ToString() + ".*");
            if (fileEntries.Length > 0)
            {
                if (pIDMedia.HasValue)
                {
                    string[] file = System.IO.Directory.GetFiles(MapPath(strMediaDestinationFolder), mediaID + ".*");
                    if (file.Length > 0)
                        System.IO.File.Delete(file[0]);
                }

                if (!System.IO.Directory.Exists(MapPath(strMediaDestinationFolder)))
                    System.IO.Directory.CreateDirectory(MapPath(strMediaDestinationFolder));

                string MediaFullpath = MapPath(strMediaDestinationFolder + "/" + mediaID + System.IO.Path.GetExtension(fileEntries[0]));
                System.IO.File.Copy(fileEntries[0], MediaFullpath, true);
                Global.MethodsAndProps.mGenerateThumbnail(MediaFullpath);
                System.IO.File.Delete(fileEntries[0]);
                pTempMediaID = Guid.NewGuid();
            }
        }
    }
    private void mDeleteTempFiles()
    {
        string strMediaTempFile = Global.Constants.FOLDER_TEMP + pTempMediaID.ToString();
        if (System.IO.File.Exists(MapPath(strMediaTempFile)))
            System.IO.File.Delete(MapPath(strMediaTempFile));
    }
    public string mGetLinkToPreview(int mediaId)
    {
        Medias media = new Medias(mediaId);
        string[] imageFile = System.IO.Directory.GetFiles(MapPath(Global.Constants.FOLDER_MEDIAS), media.pIDMedia.ToString() + ".*");
        if (imageFile.Count() > 0)
        {
            return string.Format(@"<a href='javascript:void(0)' onclick=""window.open('/{0}?{1}', 'File', 'height=515,width=990,top=120,left=10,scrollbars=yes,resizable=yes')"">{2}</a>",
                Global.Constants.FOLDER_MEDIAS.Substring(2) + System.IO.Path.GetFileName(imageFile[0]),
                DateTime.Now.ToString(),
                Farschidus.Translator.AppTranslate["ascx.media.popupLinks.title"]);
        }
        return "";
    }

    #endregion
}