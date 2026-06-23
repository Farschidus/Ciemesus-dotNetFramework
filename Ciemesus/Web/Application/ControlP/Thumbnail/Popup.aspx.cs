using System;
using System.Web.UI.WebControls;
using Farschidus.Web.UI.WebControls;
using BLL.BusinessEntity;

public partial class ControlP_Thumbnail_Popup : BasePage
{
    #region "Properties"

    public int? pIDMedia
    {
        get
        {
            if (Session["pIDMedia"] == null)
            {
                return null;
            }
            return (int)Session["pIDMedia"];
        }
        set
        {
            Session["pIDMedia"] = value;
        }
    }
    private Guid pSubjectID;
    private byte pMediaSubjectTypeID;
    public MediaSubjects pMediaSubjects
    {
        get
        {
            MediaSubjects mediaSubjects = new MediaSubjects();
            if (ViewState["pMediaSubjects"] == null)
            {
                mediaSubjects.LoadByIDSubjectAndIDMediaSubjectType(pSubjectID, pMediaSubjectTypeID);
                ViewState["pMediaSubjects"] = mediaSubjects.Serialize();
            }
            else
            {
                mediaSubjects.Deserialize(ViewState["pMediaSubjects"].ToString());
            }
            return mediaSubjects;
        }
        set
        {
            ViewState["pMediaSubjects"] = value.Serialize();
        }
    }

    #endregion

    #region "Events"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            if (!hdfRefresh.Value.Equals("1001"))
                hdfRefresh.Value = "1000";
            //hdfData.Value is ready now
            mInitialBindings();
            hdfRefresh.Value = "1001";
        }
        else
        {
            this.DataBind();
            grvPageList.EmptyDataText = grvList.EmptyDataText = Farschidus.Translator.AppTranslate["general.message.gridsEmptyDataText"];
        }
    }
    protected void grvPageList_RowDraged(object sender, GridViewRowDragedEventArgs e)
    {
        string daraggedPriority = grvPageList.DataKeys[e.DragedRowIndex][MediaSubjects.ColumnNames.Priority].ToString();
        string targetPriority = grvPageList.DataKeys[e.TargetRowIndex][MediaSubjects.ColumnNames.Priority].ToString();
        bool insertBefore = (e.Status == DragStatus.Before);
        this.ReOrder(pMediaSubjects, daraggedPriority, targetPriority, insertBefore);
    }

    protected void Pager_PageIndexChanged(object sender, PagerPageIndexChangeEventArgs e)
    {
        mLoadAllMedias();
    }
    protected void Pager_PageSizeChanged(object sender, PagerPageSizeChangeEventArgs e)
    {
        mLoadAllMedias();
    }

    protected void btnAddToPage_Click(object sender, EventArgs e)
    {
        try
        {
            pMessage.Clear();
            bool hasSelect = false;
            int item;
            MediaSubjects mediaSubjects = new MediaSubjects();
            foreach (GridViewRow grvRow in grvList.Rows)
            {
                if (((CheckBox)grvRow.FindControl("chkList")).Checked)
                {
                    item = Convert.ToInt32(grvList.DataKeys[grvRow.RowIndex][Medias.ColumnNames.IDMedia].ToString());
                    mediaSubjects.LoadByPrimaryKey(item, pSubjectID, pMediaSubjectTypeID);
                    if (mediaSubjects.RowCount == 0)
                    {
                        mediaSubjects.AddNew();
                        mediaSubjects.pIDMedia = item;
                        mediaSubjects.pIDSubject = pSubjectID;
                        mediaSubjects.pIDMediaSubjectType = pMediaSubjectTypeID;
                        mediaSubjects.pPriority = mSetPriority();
                        mediaSubjects.Save();
                        hasSelect = true;
                    }
                    else
                    {
                        pMessage.Add(Farschidus.Translator.AppTranslate["thumbnail.popup.message.alreadyExist"] + ": " + mediaSubjects.Medias.pFileName, Farschidus.Web.UI.Message.MessageTypes.Information);
                    }
                }
            }
            if (hasSelect)
            {
                mLoadAll();
                pMessage.Add(Farschidus.Translator.AppTranslate["general.message.success"], Farschidus.Web.UI.Message.MessageTypes.Success);
            }
        }
        catch (Exception ex)
        {
            pMessage.Add(ex.Message, Farschidus.Web.UI.Message.MessageTypes.Error);
        }
        finally
        {
            mShowMessage(pMessage);
            uplAddEdit.Update();
        }

    }
    protected void btnRemoveFromPage_Click(object sender, EventArgs e)
    {
        try
        {
            pMessage.Clear();
            bool hasSelect = false;
            int item;
            MediaSubjects mediaSubjects = new MediaSubjects();
            foreach (GridViewRow grvRow in grvPageList.Rows)
            {
                if (((CheckBox)grvRow.FindControl("chkPageList")).Checked)
                {
                    item = Convert.ToInt32(grvPageList.DataKeys[grvRow.RowIndex][Medias.ColumnNames.IDMedia].ToString());
                    mediaSubjects.LoadByPrimaryKey(item, pSubjectID, pMediaSubjectTypeID);
                    if (mediaSubjects.RowCount > 0)
                    {
                        mediaSubjects.MarkAsDeleted(false);
                        mediaSubjects.Save();
                        hasSelect = true;
                    }
                }
            }
            if (hasSelect)
            {
                reorderMediaSubjects(mediaSubjects);
                mLoadAll();
                pMessage.Add(Farschidus.Translator.AppTranslate["general.message.deleted"], Farschidus.Web.UI.Message.MessageTypes.Success);
            }
        }
        catch (Exception ex)
        {
            pMessage.Add(ex.Message, Farschidus.Web.UI.Message.MessageTypes.Error);
        }
        finally
        {
            mShowMessage(pMessage);
        }
        uplAddEdit.Update();
    }

    #endregion

    #region "Methodes"

    private void mInitialBindings()
    {
        if (!hdfData.Value.Equals(string.Empty))
        {
            string[] Data = hdfData.Value.Split('|');
            pSubjectID = new Guid(Data[0]);
            pMediaSubjectTypeID = (byte)MediaSubjectTypes.Enum.thumbnail;
            if (hdfRefresh.Value.Equals("1000"))
                mLoadAll();
        }
    }
    private void mLoadAll()
    {
        mLoadAllPageMedias();
        mLoadAllMedias();
    }
    private void mLoadAllPageMedias()
    {
        MediaSubjects mediaSubjects = new MediaSubjects();
        mediaSubjects.LoadByIDSubjectAndIDMediaSubjectType(pSubjectID, pMediaSubjectTypeID);
        mediaSubjects.Sort = MediaSubjects.ColumnNames.Priority;
        grvPageList.DataSource = mediaSubjects.DefaultView;
        grvPageList.DataBind();
        uplPageList.Update();
    }
    private void mLoadAllMedias()
    {
        int itemCount = 0;
        Medias medias = new Medias();
        medias.Search(Pager.CurrentIndex - 1, Pager.PageSize, ref itemCount,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        Pager.ItemCount = itemCount;

        grvList.DataSource = medias.DefaultView;
        grvList.DataBind();
        uplList.Update();
    }

    private void reorderMediaSubjects(MediaSubjects mediaSubjects)
    {
        mediaSubjects.LoadByIDSubjectAndIDMediaSubjectType(pSubjectID, pMediaSubjectTypeID);
        mediaSubjects.Sort = MediaSubjects.ColumnNames.Priority;
        int i = 1;
        if (mediaSubjects.RowCount > 0)
        {
            do
            {
                mediaSubjects.pPriority = i;
                i++;
            }
            while (mediaSubjects.MoveNext());
            mediaSubjects.Save();
        }
    }
    private int mSetPriority()
    {
        MediaSubjects mediaSubjects = new MediaSubjects();
        mediaSubjects.LoadByIDSubjectAndIDMediaSubjectType(pSubjectID, pMediaSubjectTypeID);
        mediaSubjects.Sort = MediaSubjects.ColumnNames.Priority;
        if (mediaSubjects.RowCount > 0)
        {
            mediaSubjects.MoveTo(mediaSubjects.RowCount - 1);
            if (!mediaSubjects.IsColumnNull(MediaSubjects.ColumnNames.Priority))
            {
                return mediaSubjects.pPriority + 1;
            }
            else
            {
                mediaSubjects.Rewind();
                int i = 1;
                if (mediaSubjects.RowCount > 0)
                {
                    do
                    {
                        mediaSubjects.pPriority = i;
                        i++;
                    }
                    while (mediaSubjects.MoveNext());
                    mediaSubjects.Save();
                }
                return i++;
            }
        }
        else
        {
            return 1;
        }
    }
    private void ReOrder(MediaSubjects mediaSubjects, string draggedPriority, string targetPriority, bool insertBefore)
    {
        try
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
            if (!string.IsNullOrEmpty(mediaSubjects.Filter))
            {
                baseFilter = mediaSubjects.Filter + " AND ";
            }

            // Step 1: Temporarily mark the dragged item to avoid conflicts during shifting
            mediaSubjects.Filter = baseFilter + string.Format("{0}={1}", MediaSubjects.ColumnNames.Priority, draggedPriorityValue);
            if (mediaSubjects.RowCount > 0)
            {
                mediaSubjects.pPriority = 0;
            }

            // Step 2: Shift items in the affected range in one pass
            mediaSubjects.Filter = baseFilter + rangeFilter;
            if (mediaSubjects.RowCount > 0)
            {
                do
                {
                    mediaSubjects.pPriority += shiftAmount;
                } while (mediaSubjects.MoveNext());
            }

            // Step 3: Place the dragged item at its final position
            mediaSubjects.Filter = baseFilter + string.Format("{0}=0", MediaSubjects.ColumnNames.Priority);
            if (mediaSubjects.RowCount > 0)
            {
                mediaSubjects.pPriority = finalPriority;
            }

            pMediaSubjects = mediaSubjects;
            pMediaSubjects.Save();

            mLoadAll();

            pMessage.Add(Farschidus.Translator.AppTranslate["general.message.reordered"], Farschidus.Web.UI.Message.MessageTypes.Success);
        }
        catch (Exception ex)
        {
            pMessage.Add(ex.Message, Farschidus.Web.UI.Message.MessageTypes.Error);
        }
        finally
        {
            mShowMessage(pMessage);
            uplAddEdit.Update();
        }
    }
    protected string mGetImageLink(object IDMedia)
    {
        string output = string.Empty;
        Medias medias = new Medias((int)IDMedia);

        string[] imageFile = System.IO.Directory.GetFiles(MapPath(Global.Constants.FOLDER_MEDIAS), medias.pIDMedia.ToString() + ".*");
        if (imageFile.Length > 0)
            output = string.Format(@"<a href='javascript:void(0)' onclick=""window.open('/{0}?{1}', 'File', 'height=515,width=990,top=120,left=10,scrollbars=yes,resizable=yes')"">{2}</a>",
                Global.Constants.FOLDER_MEDIAS.Substring(2) + System.IO.Path.GetFileName(imageFile[0]),
                DateTime.Now.ToString(),
                Farschidus.Translator.AppTranslate["thumbnail.popup.links.showFile"]);
        else
            output = Farschidus.Translator.AppTranslate["general.label.fileNotExist"];

        return output;
    }

    #endregion
}