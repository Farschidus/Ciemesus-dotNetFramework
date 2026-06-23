using BLL.BusinessEntity;
using Farschidus.Web.UI.WebControls;
using System;
using System.Data;
using System.IO;
using System.Web.UI.WebControls;
using System.Xml;

public partial class PSM_Contacts_Default : BaseCP
{
    #region "Properties"

    public string pIDDepartment
    {
        get
        {
            if (ViewState["pIDDepartment"] == null)
            {
                return null;
            }
            return ViewState["pIDDepartment"].ToString();
        }
        set
        {
            ViewState["pIDDepartment"] = value;
        }
    }
    public Subjects pSubjects
    {
        get
        {
            Subjects subjects = new Subjects();
            if (ViewState["pSubjects"] == null)
            {
                subjects.LoadByIDSubjectType((byte)SubjectTypes.Enum.list);
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
            pShownSearchButton = false;
            grvList.EmptyDataText = Farschidus.Translator.AppTranslate["general.message.gridsEmptyDataText"];
            Title = Farschidus.Translator.AppTranslate["contactManaging.default.page.title"];
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

        foreach (GridViewRow grvRow in grvList.Rows)
        {
            if (((CheckBox)grvRow.FindControl("chkList")).Checked)
            {
                pIDDepartment = grvList.DataKeys[grvRow.RowIndex].Values[0].ToString();
                mDelete(pIDDepartment, false);
                hasSelect = true;
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
        pIDDepartment = grvList.DataKeys[Convert.ToInt32(e.NewEditIndex)][0].ToString();
        e.Cancel = true;
        pListMode = ListMode.Edit;
        mFillForm();
    }
    protected void grvList_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        pIDDepartment = grvList.DataKeys[Convert.ToInt32(e.RowIndex)][0].ToString();
        mDelete(pIDDepartment);
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
        DataSet dsXml = new DataSet();
        Languages lang = new Languages(pLanguageID);

        string xmlMapPath = Server.MapPath(string.Format("{0}{1}.xml", Global.Constants.FOLDER_CONTACTS, lang.pCode));

        if (File.Exists(xmlMapPath))
        {
            dsXml.ReadXml(xmlMapPath);
            grvList.DataSource = dsXml;
        }
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
            Convert.ToByte(SubjectTypes.Enum.list),
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
        DataSet dsXml = new DataSet();
        Languages lang = new Languages(pLanguageID);


        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Server.MapPath(string.Format("{0}{1}.xml", Global.Constants.FOLDER_CONTACTS, lang.pCode)));

        XmlNode root = xmlDoc.DocumentElement;
        XmlNode departmentNode = root.SelectSingleNode(string.Format("//Departments/Department[@id='{0}']", pIDDepartment));

        txtTitle.Text = departmentNode.SelectSingleNode("title").InnerText;
        txtSubject.Text = departmentNode.SelectSingleNode("subject").InnerText;
        txtEmail.Text = departmentNode.SelectSingleNode("email").InnerText;
        txtPhone.Text = departmentNode.SelectSingleNode("phone").InnerText;
        txtFax.Text = departmentNode.SelectSingleNode("fax").InnerText;
        txtAddress.Text = departmentNode.SelectSingleNode("address").InnerText;

        uplAddEdit.Update();
    }
    private bool mValidateAddEdit()
    {
        pMessage.Clear();
        bool isValid = true;
        pMessage.Add("False in Validator by Farschidus", Farschidus.Web.UI.Message.MessageTypes.Error);
        return isValid;
    }
    private bool mValidateDelete()
    {
        return true;
    }
    private void mSave()
    {
        if (mValidateAddEdit())
        {
            DAL.GlobalCore.TransactionMgr tx = DAL.GlobalCore.TransactionMgr.ThreadTransactionMgr();
            try
            {
                tx.BeginTransaction();

                Languages lang = new Languages(pLanguageID);
                string xmlMapPath = Server.MapPath(string.Format("{0}{1}.xml", Global.Constants.FOLDER_CONTACTS, lang.pCode));
                if (!File.Exists(xmlMapPath))
                    mCreateXML(xmlMapPath);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlMapPath);
                XmlNode root = xmlDoc.DocumentElement;
                XmlNode departmentNode = null;
                if (!string.IsNullOrEmpty(pIDDepartment))
                {
                    departmentNode = root.SelectSingleNode(string.Format("//Departments/Department[@id='{0}']", pIDDepartment));

                    departmentNode.SelectSingleNode("title").InnerText = txtTitle.Text;
                    departmentNode.SelectSingleNode("subject").InnerText = txtSubject.Text;
                    departmentNode.SelectSingleNode("email").InnerText = txtEmail.Text;
                    departmentNode.SelectSingleNode("phone").InnerText = txtPhone.Text;
                    departmentNode.SelectSingleNode("fax").InnerText = txtFax.Text;
                    departmentNode.SelectSingleNode("address").InnerText = txtAddress.Text;
                }
                else
                {
                    int newID = 1;
                    if (root.LastChild != null)
                    {
                        departmentNode = root.LastChild;
                        newID = Convert.ToInt32(departmentNode.Attributes["id"].Value) + 1;
                    }

                    XmlElement DepartmentNode = xmlDoc.CreateElement("Department");
                    XmlElement DepartmentTitle = xmlDoc.CreateElement("title");
                    XmlElement DepartmentSubject = xmlDoc.CreateElement("subject");
                    XmlElement DepartmentEmail = xmlDoc.CreateElement("email");
                    XmlElement DepartmentPhone = xmlDoc.CreateElement("phone");
                    XmlElement DepartmentFax = xmlDoc.CreateElement("fax");
                    XmlElement DepartmentAddress = xmlDoc.CreateElement("address");
                    XmlAttribute DepartmentIdAttr = xmlDoc.CreateAttribute("id");

                    DepartmentIdAttr.Value = newID.ToString();
                    DepartmentTitle.InnerText = txtTitle.Text;
                    DepartmentSubject.InnerText = txtSubject.Text;
                    DepartmentEmail.InnerText = txtEmail.Text;
                    DepartmentPhone.InnerText = txtPhone.Text;
                    DepartmentFax.InnerText = txtFax.Text;
                    DepartmentAddress.InnerText = txtAddress.Text;

                    DepartmentNode.Attributes.Append(DepartmentIdAttr);
                    DepartmentNode.AppendChild(DepartmentTitle);
                    DepartmentNode.AppendChild(DepartmentSubject);
                    DepartmentNode.AppendChild(DepartmentEmail);
                    DepartmentNode.AppendChild(DepartmentPhone);
                    DepartmentNode.AppendChild(DepartmentFax);
                    DepartmentNode.AppendChild(DepartmentAddress);

                    root.AppendChild(DepartmentNode);
                }



                xmlDoc.Save(Server.MapPath(string.Format("{0}{1}.xml", Global.Constants.FOLDER_CONTACTS, lang.pCode)));
                mLoadAll();

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

    private void mDelete(string pIDDepartment, bool loadList = true)
    {
        DAL.GlobalCore.TransactionMgr tx = DAL.GlobalCore.TransactionMgr.ThreadTransactionMgr();

        Languages lang = new Languages(pLanguageID);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Server.MapPath(string.Format("{0}{1}.xml", Global.Constants.FOLDER_CONTACTS, lang.pCode)));
        XmlNode root = xmlDoc.DocumentElement;
        XmlNode departmentNode = null;

        departmentNode = root.SelectSingleNode(string.Format("//Departments/Department[@id='{0}']", pIDDepartment));
        if (!string.IsNullOrEmpty(pIDDepartment))
        {
            int count = xmlDoc.SelectNodes("Departments/Department").Count;
            try
            {
                tx.BeginTransaction();
                if (count < 2)
                {
                    File.Delete(Server.MapPath(string.Format("{0}{1}.xml", Global.Constants.FOLDER_CONTACTS, lang.pCode)));
                }
                else
                {
                    departmentNode.ParentNode.RemoveChild(departmentNode);
                    xmlDoc.Save(Server.MapPath(string.Format("{0}{1}.xml", Global.Constants.FOLDER_CONTACTS, lang.pCode)));
                }

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
    }
    private void mClear()
    {
        txtEmail.Text = txtTitle.Text = txtSubject.Text = txtPhone.Text = txtFax.Text = txtAddress.Text = string.Empty;
        pIDDepartment = string.Empty;
        uplAddEdit.Update();
    }

    private int mSetPriority()
    {
        Subjects subjects = new Subjects();
        subjects.LoadByIDSubjectType((byte)SubjectTypes.Enum.list);
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
    private void mCreateXML(string xmlMapPath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        string strXml = "<?xml version='1.0' encoding='utf-8'?><Departments></Departments>";
        xmlDoc.LoadXml(strXml);
        xmlDoc.Save(xmlMapPath);

        //XElement.Load(xmlMapPath);
        //XElement Departments =
        //    new XElement("Departments",
        //        new XElement("Department",
        //            new XAttribute("id", "1"),
        //            new XElement("title", "Please Edit Contact Info"),
        //            new XElement("subject", ""),
        //            new XElement("email", ""),
        //            new XElement("phone", ""),
        //            new XElement("fax", ""),
        //            new XElement("address", "")));
        //Departments.Save(xmlMapPath);
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