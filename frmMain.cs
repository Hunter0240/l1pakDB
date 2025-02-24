using PakViewer.Properties;
using PakViewer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace PakViewer
{
  public class frmMain : Form
  {
    private PakFileManager _pakFileManager;
    private FileFilterManager _filterManager;
    private FileOperationsManager _fileOperationsManager;
    private UIStateManager _uiStateManager;
    private ViewerManager _viewerManager;
    private string _PackFileName;
    private string _TextLanguage;
    private IContainer components;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem mnuFile;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem mnuQuit;
    private OpenFileDialog dlgOpenFile;
    private SplitContainer splitContainer1;
    private ListView lvIndexInfo;
    private FolderBrowserDialog dlgOpenFolder;
    private ToolStripMenuItem mnuEdit;
    private ToolStripMenuItem mnuCreatResource;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem mnuLanguage;
    private ToolStripMenuItem mnuTools;
    private TextBox TextViewer;
    private ToolStripMenuItem mnuOpen;
    private ToolStripMenuItem mnuFiller;
    private ToolStripMenuItem mnuFiller_Text_html;
    private ToolStripSeparator toolStripSeparator5;
    private ToolStripMenuItem mnuFiller_Text_C;
    private ToolStripMenuItem mnuFiller_Text_H;
    private ToolStripMenuItem mnuFiller_Text_J;
    private ToolStripMenuItem mnuFiller_Text_K;
    private ToolStripSeparator toolStripSeparator7;
    private ToolStripMenuItem mnuFiller_Sprite_spr;
    private ToolStripMenuItem mnuFiller_Tile_til;
    private ToolStripMenuItem mnuFiller_Sprite_img;
    private ToolStripMenuItem mnuFiller_Sprite_png;
    private ToolStripMenuItem mnuFiller_Sprite_tbt;
    private ToolStripMenuItem mnuTools_Export;
    private ToolStripMenuItem mnuTools_ExportTo;
    private ToolStripMenuItem mnuTools_Delete;
    private ToolStripSeparator toolStripSeparator8;
    private ToolStripStatusLabel tssLocker;
    private ToolStripMenuItem mnuTools_ClearSelect;
    private ToolStripMenuItem mnuTools_SelectAll;
    private ToolStripMenuItem mnuTools_Add;
    private ToolStripSeparator toolStripSeparator9;
    private ToolStripMenuItem mnuTools_Update;
    private ToolStripStatusLabel tssRecordCount;
    private ToolStripStatusLabel tssShowInListView;
    private ToolStripStatusLabel tssCheckedCount;
    private ToolStripMenuItem mnuRebuild;
    private OpenFileDialog dlgAddFiles;
    private ToolStripMenuItem mnuLanguage_TW;
    private ToolStripMenuItem mnuLanguage_EN;
    private ucSprViewer SprViewer;
    private FlowLayoutPanel palSearch;
    private Label label1;
    private TextBox txtSearch;
    private SplitContainer splitContainer2;
    private ucImgViewer ImageViewer;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripMenuItem tsmCompare;
    private ToolStripMenuItem tsmCompTW;
    private ToolStripMenuItem tsmCompHK;
    private ToolStripMenuItem tsmCompJP;
    private ToolStripMenuItem tsmCompKO;
    private ucTextCompare TextCompViewer;
    private ToolStripSeparator toolStripSeparator4;
    private ToolStripSeparator toolStripSeparator6;
    private ContextMenuStrip ctxMenu;
    private ToolStripMenuItem tsmExport;
    private ToolStripMenuItem tsmExportTo;
    private ToolStripMenuItem tsmUnselectAll;
    private ToolStripMenuItem tsmSelectAll;
    private StatusStrip statusStrip1;
    private ToolStripStatusLabel tssMessage;
    private ToolStripStatusLabel tssProgressName;
    private ToolStripProgressBar tssProgress;
    private Dictionary<int, string> _recordSourceFiles = new Dictionary<int, string>();

    public frmMain()
    {
      this.InitializeComponent();
      this.SetupUI();
      this._filterManager = new FileFilterManager();
      InitializeManagers();
    }

    private void InitializeManagers()
    {
      _fileOperationsManager = new FileOperationsManager(
        _pakFileManager,
        text => this.tssProgressName.Text = text,
        value => this.tssProgress.Value = value,
        max => this.tssProgress.Maximum = max
      );

      _uiStateManager = new UIStateManager(
        this.TextViewer,
        this.ImageViewer,
        this.SprViewer,
        this.TextCompViewer,
        this.tssMessage,
        this.tssProgressName,
        this.tssProgress,
        this.lvIndexInfo,
        this.menuStrip1
      );

      _viewerManager = new ViewerManager(
        this.TextViewer,
        this.ImageViewer,
        this.SprViewer,
        this.TextCompViewer,
        _uiStateManager
      );
    }

    private void SetupUI()
    {
      this.TextViewer.Dock = DockStyle.Fill;
      this.ImageViewer.Dock = DockStyle.Fill;
      this.SprViewer.Dock = DockStyle.Fill;
      this.TextCompViewer.Dock = DockStyle.Fill;
      string defaultLang = Settings.Default.DefaultLang;
      this.mnuFiller_Text_C.Checked = defaultLang.Contains("-c");
      this.mnuFiller_Text_H.Checked = defaultLang.Contains("-h");
      this.mnuFiller_Text_J.Checked = defaultLang.Contains("-j");
      this.mnuFiller_Text_K.Checked = defaultLang.Contains("-k");
      this.lvIndexInfo.Columns.Add("No.", 70, HorizontalAlignment.Right);
      this.lvIndexInfo.Columns.Add("FileName", 150, HorizontalAlignment.Left);
      this.lvIndexInfo.Columns.Add("Size", 70, HorizontalAlignment.Right);
      this.lvIndexInfo.Columns.Add("Position", 70, HorizontalAlignment.Right);
      this.splitContainer1.SplitterDistance = 390;
      this.SetupMenuHandlers();
    }

    private void SetupMenuHandlers()
    {
      this.mnuTools.Click += (sender, e) =>
      {
        this.mnuTools_Export.Enabled = this.lvIndexInfo.CheckedItems.Count > 0;
        this.mnuTools_ExportTo.Enabled = this.mnuTools_Export.Enabled;
        this.mnuTools_Delete.Enabled = this.mnuTools_Export.Enabled;
        this.mnuTools_Add.Enabled = this.lvIndexInfo.Items.Count > 0;
        this.mnuTools_Update.Enabled = _viewerManager.CurrentViewType == Utility.InviewDataType.Text && this.TextViewer.Modified;
      };
      this.mnuQuit.Click += (sender, e) => this.Close();
      L1PakTools.ShowProgress(this.tssProgress);
    }

    private void mnuOpen_Click(object sender, EventArgs e)
    {
      this.dlgOpenFile.Filter = "Pack Index files (*.idx)|*.idx";
      this.dlgOpenFile.DefaultExt = "idx";
      this.dlgOpenFile.AddExtension = true;
      this.dlgOpenFile.FileName = "";
      this.dlgOpenFile.Multiselect = true;
      if (this.dlgOpenFile.ShowDialog(this) != DialogResult.OK || this.dlgOpenFile.FileNames.Length == 0)
        return;

      _uiStateManager.SetCursor(Cursors.WaitCursor);
      _uiStateManager.UpdateListView(new ListViewItem[0]);
      _viewerManager.ClearContent();

      var allRecords = new List<L1PakTools.IndexRecord>();
      _recordSourceFiles.Clear();
      bool anyFileLoaded = false;
      int currentIndex = 0;

      foreach (string fileName in this.dlgOpenFile.FileNames)
      {
        this._PackFileName = fileName.ToLower();
        this._pakFileManager = new PakFileManager(this._PackFileName);
        InitializeManagers();
        
        if (!this._pakFileManager.LoadIndexFile())
        {
          MessageBox.Show($"The file {fileName} can't be parsed. It might be broken or not correct idx file.");
          continue;
        }

        // Store the source file for each record
        for (int i = 0; i < this._pakFileManager.IndexRecords.Length; i++)
        {
          _recordSourceFiles[currentIndex + i] = fileName;
        }
        
        allRecords.AddRange(this._pakFileManager.IndexRecords);
        currentIndex += this._pakFileManager.IndexRecords.Length;
        anyFileLoaded = true;
      }

      if (anyFileLoaded)
      {
        // Update the records in the current PakFileManager
        this._pakFileManager.IndexRecords = allRecords.ToArray();
        this.ShowRecords(this._pakFileManager.IndexRecords);
        _uiStateManager.UpdateMenuState(true, this._pakFileManager.IsProtected);
      }
      
      _uiStateManager.SetCursor(Cursors.Default);
    }

    private void mnuFiller_Text_Language(object sender, EventArgs e)
    {
      this._TextLanguage = (this.mnuFiller_Text_C.Checked ? "-c" : "") + (this.mnuFiller_Text_H.Checked ? "-h" : "") + (this.mnuFiller_Text_J.Checked ? "-j" : "") + (this.mnuFiller_Text_K.Checked ? "-k" : "");
      Settings.Default.DefaultLang = this._TextLanguage;
      Settings.Default.Save();
      this.UpdateLanguageFilters();
      if (this._pakFileManager?.IndexRecords != null)
      {
        this.ShowRecords(this._pakFileManager.IndexRecords);
      }
    }

    private void UpdateLanguageFilters()
    {
      if (_filterManager == null)
      {
        _filterManager = new FileFilterManager();
      }

      _filterManager.ClearFilters();
      if (this.mnuFiller_Text_C.Checked)
        _filterManager.SetLanguageFilter("-c", true);
      if (this.mnuFiller_Text_H.Checked)
        _filterManager.SetLanguageFilter("-h", true);
      if (this.mnuFiller_Text_J.Checked)
        _filterManager.SetLanguageFilter("-j", true);
      if (this.mnuFiller_Text_K.Checked)
        _filterManager.SetLanguageFilter("-k", true);
      if (this.mnuFiller_Text_html.Checked)
        _filterManager.SetExtensionFilter(".html", true);
      if (this.mnuFiller_Sprite_spr.Checked)
        _filterManager.SetExtensionFilter(".spr", true);
      if (this.mnuFiller_Tile_til.Checked)
        _filterManager.SetExtensionFilter(".til", true);
      if (this.mnuFiller_Sprite_img.Checked)
        _filterManager.SetExtensionFilter(".img", true);
      if (this.mnuFiller_Sprite_png.Checked)
        _filterManager.SetExtensionFilter(".png", true);
      if (this.mnuFiller_Sprite_tbt.Checked)
        _filterManager.SetExtensionFilter(".tbt", true);
    }

    private void ShowRecords(L1PakTools.IndexRecord[] records)
    {
      ListViewItem[] items = new ListViewItem[records.Length];
      for (int i = 0; i < records.Length; i++)
      {
        items[i] = this.CreateListViewItem(i, records[i]);
      }
      _uiStateManager.UpdateListView(items);
    }

    private ListViewItem CreateListViewItem(int id, L1PakTools.IndexRecord record)
    {
      return new ListViewItem(string.Format("{0,5}", id + 1))
      {
        SubItems = {
          record.FileName,
          record.FileSize.ToString("N00"),
          record.Offset.ToString("X8")
        }
      };
    }

    private void lvIndexInfo_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      int column = e.Column;
      if (this.lvIndexInfo.Tag == null)
        this.lvIndexInfo.Tag = (object) 0;
      if (Math.Abs((int) this.lvIndexInfo.Tag) == column)
        column = -(int) this.lvIndexInfo.Tag;
      this.lvIndexInfo.Tag = (object) column;
      this.lvIndexInfo.ListViewItemSorter = (IComparer) new frmMain.ListViewItemComparer(column);
    }

    private void lvIndexInfo_MouseClick(object sender, MouseEventArgs e)
    {
      if (this.lvIndexInfo.HitTest(e.Location).Item != null)
        this.lvIndexInfo.ContextMenuStrip = this.ctxMenu;
      else
        this.lvIndexInfo.ContextMenuStrip = (ContextMenuStrip) null;
    }

    private void lvIndexInfo_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
    }

    private void lvIndexInfo_SelectedIndexChanged(object sender, EventArgs e)
    {
      _uiStateManager.SetStatusMessage("");
      _viewerManager.ClearContent();

      if (this.lvIndexInfo.SelectedItems.Count == 0)
        return;

      // Group selected items by source file
      var itemsBySourceFile = new Dictionary<string, List<ListViewItem>>();
      foreach (ListViewItem item in this.lvIndexInfo.SelectedItems)
      {
        int recordIndex = int.Parse(item.Text) - 1;
        string sourceFile = _recordSourceFiles[recordIndex];
        if (!itemsBySourceFile.ContainsKey(sourceFile))
        {
          itemsBySourceFile[sourceFile] = new List<ListViewItem>();
        }
        itemsBySourceFile[sourceFile].Add(item);
      }

      try
      {
        // Load all selected items and determine the common type
        var selectedContents = new List<(object content, InviewDataType viewType, string tag)>();
        InviewDataType commonType = InviewDataType.Empty;

        // Process items from each source file
        foreach (var sourceGroup in itemsBySourceFile)
        {
          string pakFile = sourceGroup.Key.Replace(".idx", ".pak");
          using (FileStream fs = File.Open(pakFile, FileMode.Open, FileAccess.Read))
          {
            foreach (ListViewItem item in sourceGroup.Value)
            {
              var (content, viewType) = this._fileOperationsManager.LoadPakData(fs, item);
              selectedContents.Add((content, viewType, item.Text));
              
              // Set the common type to the first non-empty type we find
              if (commonType == InviewDataType.Empty && viewType != InviewDataType.Empty)
              {
                commonType = viewType;
              }
              // If types don't match, default to text view
              else if (commonType != viewType && viewType != InviewDataType.Empty)
              {
                commonType = InviewDataType.Text;
              }
            }
          }
        }

        // Set the view type first
        _viewerManager.CurrentViewType = commonType;

        // Combine the content based on type
        switch (commonType)
        {
          case InviewDataType.Text:
            var combinedText = new StringBuilder();
            foreach (var (content, _, tag) in selectedContents)
            {
              if (content is string text)
              {
                combinedText.AppendLine($"=== {tag} ===");
                combinedText.AppendLine(text);
                combinedText.AppendLine();
              }
            }
            _viewerManager.DisplayContent(combinedText.ToString());
            break;

          case InviewDataType.IMG:
          case InviewDataType.BMP:
          case InviewDataType.TBT:
          case InviewDataType.TIL:
            // For images, show the first one
            var firstImage = selectedContents.FirstOrDefault(x => x.content is Image);
            if (firstImage.content != null)
            {
              _viewerManager.DisplayContent(firstImage.content);
            }
            break;

          case InviewDataType.SPR:
            // For sprites, show the first one
            var firstSprite = selectedContents.FirstOrDefault(x => x.content is L1Spr.Frame[]);
            if (firstSprite.content != null)
            {
              _viewerManager.DisplayContent(firstSprite.content);
            }
            break;
        }
      }
      catch (Exception ex)
      {
        _uiStateManager.SetStatusMessage($"Error: {ex.Message}");
      }
    }

    private void ExportSelected(string path, ListViewItem lvItem)
    {
      int recordIndex = int.Parse(lvItem.Text) - 1;
      string sourceFile = _recordSourceFiles[recordIndex];
      string pakFile = sourceFile.Replace(".idx", ".pak");

      using (FileStream fs = File.Open(pakFile, FileMode.Open, FileAccess.Read))
      {
        try
        {
          var (content, viewType) = this._fileOperationsManager.LoadPakData(fs, lvItem);
          object data;
          if (_viewerManager.CurrentViewType == Utility.InviewDataType.Text)
          {
              data = _viewerManager.GetCurrentText();
          }
          else if (_viewerManager.CurrentViewType == Utility.InviewDataType.IMG || _viewerManager.CurrentViewType == Utility.InviewDataType.BMP)
          {
              data = _viewerManager.GetCurrentImage();
          }
          else
          {
              data = content;
          }

          this._fileOperationsManager.ExportData(path, lvItem, data, content as byte[], viewType);
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Error exporting file: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    private void mnuTools_Delete_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("Please delete the former backup of your original PAK files!\n\nAre you sure to delete ?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
        return;
      int[] DeleteID = new int[this.lvIndexInfo.CheckedItems.Count];
      int num = 0;
      foreach (ListViewItem checkedItem in this.lvIndexInfo.CheckedItems)
      {
        DeleteID[num++] = int.Parse(checkedItem.Text) - 1;
      }
      try
      {
        this._fileOperationsManager.RebuildAll(this._PackFileName, DeleteID);
        this.ShowRecords(this._pakFileManager.IndexRecords);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error deleting files: {ex.Message}", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void mnuRebuild_Click(object sender, EventArgs e)
    {
      try
      {
        this._fileOperationsManager.RebuildAll(this._PackFileName, new int[0]);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error rebuilding files: {ex.Message}", "Rebuild Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void tssProgressName_TextChanged(object sender, EventArgs e)
    {
      if (this.tssProgressName.Text != "")
      {
        this.tssProgressName.Visible = true;
        this.tssProgress.Visible = true;
        this.tssProgress.Value = 0;
      }
      else
      {
        this.tssProgressName.Visible = false;
        this.tssProgress.Visible = false;
      }
      this.statusStrip1.Refresh();
    }

    private void mnuTools_Add_Click(object sender, EventArgs e)
    {
      this.dlgAddFiles.Filter = "All files (*.*)|*.*";
      this.dlgAddFiles.FileName = "";
      if (this.dlgAddFiles.ShowDialog((IWin32Window) this) != DialogResult.OK)
        return;

      try
      {
        using (FileStream pakStream = File.OpenWrite(this._PackFileName.Replace(".idx", ".pak")))
        using (FileStream idxStream = File.OpenWrite(this._PackFileName))
        {
          foreach (string fileName in this.dlgAddFiles.FileNames)
          {
            byte[] fileData = File.ReadAllBytes(fileName);
            if (this._pakFileManager.IsProtected)
            {
              this.tssProgressName.Text = string.Format("{0} Encoding...", (object) fileName);
              fileData = L1PakTools.Encode(fileData, 0);
              this.tssProgressName.Text = "";
            }

            int offset = (int) pakStream.Seek(0L, SeekOrigin.End);
            pakStream.Write(fileData, 0, fileData.Length);

            string filename = fileName.Substring(fileName.LastIndexOf('\\') + 1);

            bool replace = false;
            int index = -1;
            for (int i = 0; i < this._pakFileManager.IndexRecords.Length; ++i)
            {
              L1PakTools.IndexRecord record = this._pakFileManager.IndexRecords[i];
              if (record.FileName.Equals(filename))
              {
                replace = true;
                index = i;
                break;
              }
            }

            L1PakTools.IndexRecord record2 = new L1PakTools.IndexRecord(filename, fileData.Length, offset);

            if (replace)
            {
              this._pakFileManager.IndexRecords[index] = record2;
            }
            else
            {
              var records = this._pakFileManager.IndexRecords;
              Array.Resize<L1PakTools.IndexRecord>(ref records, records.Length + 1);
              records[records.Length - 1] = record2;
              this._pakFileManager.IndexRecords = records;
            }
          }

          if (this._pakFileManager.IsProtected)
          {
            idxStream.Close();
          }
          else
          {
            idxStream.Seek(0L, SeekOrigin.Begin);
            idxStream.Write(BitConverter.GetBytes(this._pakFileManager.IndexRecords.Length), 0, 4);
          }
        }

        _fileOperationsManager.RebuildIndex(_PackFileName);
        this.ShowRecords(this._pakFileManager.IndexRecords);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error adding files: {ex.Message}", "Add Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
    {
      if ((int) e.KeyChar != 13)
        return;
      TextBox textBox = (TextBox) sender;
      if (this._pakFileManager.IndexRecords == null || this._pakFileManager.IndexRecords.Length <= 0)
        return;
      List<ListViewItem> listViewItemList = new List<ListViewItem>();
      for (int ID = 0; ID < this._pakFileManager.IndexRecords.Length; ++ID)
      {
        L1PakTools.IndexRecord indexRecord = this._pakFileManager.IndexRecords[ID];
        if ((textBox.Text != "") )
        {
            if (textBox.Text.StartsWith("^")) {
                if(indexRecord.FileName.IndexOf(textBox.Text.Substring(1), StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    listViewItemList.Add(this.CreateListViewItem(ID, indexRecord));
                }
            }
            else if (indexRecord.FileName.IndexOf(textBox.Text, StringComparison.CurrentCultureIgnoreCase) != -1){
                listViewItemList.Add(this.CreateListViewItem(ID, indexRecord));
            }
        }
        else
        {
            listViewItemList.Add(this.CreateListViewItem(ID, indexRecord));

        }
    }
      if (listViewItemList.Count > 0)
        listViewItemList[0].Selected = true;
      this.lvIndexInfo.SuspendLayout();
      this.lvIndexInfo.Items.Clear();
      this.lvIndexInfo.Items.AddRange(listViewItemList.ToArray());
      this.lvIndexInfo.ResumeLayout();
      this.lvIndexInfo.Focus();
      this.tssRecordCount.Text = string.Format("All:{0}", (object) this._pakFileManager.IndexRecords.Length);
      this.tssShowInListView.Text = string.Format("Showing:{0}", (object) this.lvIndexInfo.Items.Count);
    }

    private void tsmCompare_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem) sender;
      this.TextCompViewer.Visible = true;
      this.TextCompViewer.SourceText = this.TextViewer.Text;
      FileStream fs = File.Open(this._PackFileName.Replace(".idx", ".pak"), FileMode.Open, FileAccess.Read);
      var result = this._fileOperationsManager.LoadPakData(fs, (L1PakTools.IndexRecord) toolStripMenuItem.Tag);
      this.TextCompViewer.TargetText = (string)result.content;
      fs.Close();
    }

    private void tsmCompare_DropDownOpening(object sender, EventArgs e)
    {
      string fileName = this._pakFileManager.IndexRecords[int.Parse(this.TextViewer.Tag.ToString()) - 1].FileName;
      int num1 = fileName.LastIndexOf('-');
      this.tsmCompTW.Enabled = false;
      this.tsmCompHK.Enabled = false;
      this.tsmCompJP.Enabled = false;
      this.tsmCompKO.Enabled = false;
      if (num1 < 0)
        return;
      string str1 = fileName.Substring(num1, 2);
      foreach (L1PakTools.IndexRecord indexRecord in this._pakFileManager.IndexRecords)
      {
        int num2 = indexRecord.FileName.LastIndexOf('-');
        if (num2 >= 0)
        {
          string str2 = indexRecord.FileName.Substring(num2, 2);
          if (indexRecord.FileName.Substring(0, num2) == fileName.Substring(0, num1))
          {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem) null;
            if (str2 == "-c" && str1 != str2)
              toolStripMenuItem = this.tsmCompTW;
            if (str2 == "-h" && str1 != str2)
              toolStripMenuItem = this.tsmCompHK;
            if (str2 == "-j" && str1 != str2)
              toolStripMenuItem = this.tsmCompJP;
            if (str2 == "-k" && str1 != str2)
              toolStripMenuItem = this.tsmCompKO;
            if (toolStripMenuItem != null)
            {
              toolStripMenuItem.Enabled = true;
              toolStripMenuItem.Tag = (object) indexRecord;
            }
          }
        }
      }
    }

    private void ctxMenu_Opening(object sender, CancelEventArgs e)
    {
      this.tsmCompare.Enabled = _viewerManager.CurrentViewType == Utility.InviewDataType.Text;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (frmMain));
      this.menuStrip1 = new MenuStrip();
      this.mnuFile = new ToolStripMenuItem();
      this.mnuOpen = new ToolStripMenuItem();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.mnuCreatResource = new ToolStripMenuItem();
      this.mnuRebuild = new ToolStripMenuItem();
      this.toolStripSeparator2 = new ToolStripSeparator();
      this.mnuQuit = new ToolStripMenuItem();
      this.mnuEdit = new ToolStripMenuItem();
      this.mnuFiller = new ToolStripMenuItem();
      this.mnuFiller_Text_html = new ToolStripMenuItem();
      this.mnuFiller_Sprite_spr = new ToolStripMenuItem();
      this.mnuFiller_Tile_til = new ToolStripMenuItem();
      this.toolStripSeparator5 = new ToolStripSeparator();
      this.mnuFiller_Text_C = new ToolStripMenuItem();
      this.mnuFiller_Text_H = new ToolStripMenuItem();
      this.mnuFiller_Text_J = new ToolStripMenuItem();
      this.mnuFiller_Text_K = new ToolStripMenuItem();
      this.toolStripSeparator7 = new ToolStripSeparator();
      this.mnuFiller_Sprite_img = new ToolStripMenuItem();
      this.mnuFiller_Sprite_png = new ToolStripMenuItem();
      this.mnuFiller_Sprite_tbt = new ToolStripMenuItem();
      this.toolStripSeparator4 = new ToolStripSeparator();
      this.mnuLanguage = new ToolStripMenuItem();
      this.mnuLanguage_TW = new ToolStripMenuItem();
      this.mnuLanguage_EN = new ToolStripMenuItem();
      this.mnuTools = new ToolStripMenuItem();
      this.mnuTools_Export = new ToolStripMenuItem();
      this.mnuTools_ExportTo = new ToolStripMenuItem();
      this.mnuTools_Delete = new ToolStripMenuItem();
      this.toolStripSeparator8 = new ToolStripSeparator();
      this.mnuTools_Add = new ToolStripMenuItem();
      this.mnuTools_Update = new ToolStripMenuItem();
      this.toolStripSeparator9 = new ToolStripSeparator();
      this.mnuTools_ClearSelect = new ToolStripMenuItem();
      this.mnuTools_SelectAll = new ToolStripMenuItem();
      this.dlgOpenFile = new OpenFileDialog();
      this.splitContainer1 = new SplitContainer();
      this.splitContainer2 = new SplitContainer();
      this.palSearch = new FlowLayoutPanel();
      this.label1 = new Label();
      this.txtSearch = new TextBox();
      this.lvIndexInfo = new ListView();
      this.TextCompViewer = new ucTextCompare();
      this.TextViewer = new TextBox();
      this.ImageViewer = new ucImgViewer();
      this.SprViewer = new ucSprViewer();
      this.dlgOpenFolder = new FolderBrowserDialog();
      this.ctxMenu = new ContextMenuStrip(this.components);
      this.tsmExport = new ToolStripMenuItem();
      this.tsmExportTo = new ToolStripMenuItem();
      this.toolStripSeparator6 = new ToolStripSeparator();
      this.tsmUnselectAll = new ToolStripMenuItem();
      this.tsmSelectAll = new ToolStripMenuItem();
      this.toolStripSeparator3 = new ToolStripSeparator();
      this.tsmCompare = new ToolStripMenuItem();
      this.tsmCompTW = new ToolStripMenuItem();
      this.tsmCompHK = new ToolStripMenuItem();
      this.tsmCompJP = new ToolStripMenuItem();
      this.tsmCompKO = new ToolStripMenuItem();
      this.statusStrip1 = new StatusStrip();
      this.tssLocker = new ToolStripStatusLabel();
      this.tssRecordCount = new ToolStripStatusLabel();
      this.tssShowInListView = new ToolStripStatusLabel();
      this.tssCheckedCount = new ToolStripStatusLabel();
      this.tssMessage = new ToolStripStatusLabel();
      this.tssProgressName = new ToolStripStatusLabel();
      this.tssProgress = new ToolStripProgressBar();
      this.dlgAddFiles = new OpenFileDialog();
      this.menuStrip1.SuspendLayout();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.palSearch.SuspendLayout();
      this.ctxMenu.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      this.menuStrip1.Items.AddRange(new ToolStripItem[3]
      {
        (ToolStripItem) this.mnuFile,
        (ToolStripItem) this.mnuEdit,
        (ToolStripItem) this.mnuTools
      });
      this.menuStrip1.Location = new Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new Size(792, 24);
      this.menuStrip1.TabIndex = 2;
      this.menuStrip1.Text = "menuStrip1";
      this.mnuFile.DropDownItems.AddRange(new ToolStripItem[6]
      {
        (ToolStripItem) this.mnuOpen,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.mnuCreatResource,
        (ToolStripItem) this.mnuRebuild,
        (ToolStripItem) this.toolStripSeparator2,
        (ToolStripItem) this.mnuQuit
      });
      this.mnuFile.Name = "mnuFile";
      this.mnuFile.Size = new Size(34, 20);
      this.mnuFile.Text = "&File";
      this.mnuOpen.Image = (Image) Resources.My_Documents;
      this.mnuOpen.Name = "mnuOpen";
      this.mnuOpen.Size = new Size(130, 22);
      this.mnuOpen.Text = "&Open";
      this.mnuOpen.Click += new EventHandler(this.mnuOpen_Click);
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size((int) sbyte.MaxValue, 6);
      this.mnuCreatResource.Name = "mnuCreatResource";
      this.mnuCreatResource.Size = new Size(130, 22);
      this.mnuCreatResource.Text = "create new source file ";
      this.mnuCreatResource.Visible = false;
      this.mnuRebuild.Enabled = false;
      this.mnuRebuild.Name = "mnuRebuild";
      this.mnuRebuild.Size = new Size(130, 22);
      this.mnuRebuild.Text = "&Rebuild";
      this.mnuRebuild.Click += new EventHandler(this.mnuRebuild_Click);
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new Size((int) sbyte.MaxValue, 6);
      this.mnuQuit.Image = (Image) Resources.ArreterSZ;
      this.mnuQuit.Name = "mnuQuit";
      this.mnuQuit.Size = new Size(130, 22);
      this.mnuQuit.Text = "&Quit";
      this.mnuEdit.DropDownItems.AddRange(new ToolStripItem[3]
      {
        (ToolStripItem) this.mnuFiller,
        (ToolStripItem) this.toolStripSeparator4,
        (ToolStripItem) this.mnuLanguage
      });
      this.mnuEdit.Name = "mnuEdit";
      this.mnuEdit.Size = new Size(36, 20);
      this.mnuEdit.Text = "&Edit";
      this.mnuFiller.DropDownItems.AddRange(new ToolStripItem[12]
      {
        (ToolStripItem) this.mnuFiller_Text_html,
        (ToolStripItem) this.mnuFiller_Sprite_spr,
        (ToolStripItem) this.mnuFiller_Tile_til,
        (ToolStripItem) this.toolStripSeparator5,
        (ToolStripItem) this.mnuFiller_Text_C,
        (ToolStripItem) this.mnuFiller_Text_H,
        (ToolStripItem) this.mnuFiller_Text_J,
        (ToolStripItem) this.mnuFiller_Text_K,
        (ToolStripItem) this.toolStripSeparator7,
        (ToolStripItem) this.mnuFiller_Sprite_img,
        (ToolStripItem) this.mnuFiller_Sprite_png,
        (ToolStripItem) this.mnuFiller_Sprite_tbt
      });
      this.mnuFiller.Enabled = false;
      this.mnuFiller.Name = "mnuFiller";
      this.mnuFiller.Size = new Size(116, 22);
      this.mnuFiller.Text = "&Filler";
      this.mnuFiller_Text_html.Checked = true;
      this.mnuFiller_Text_html.CheckOnClick = true;
      this.mnuFiller_Text_html.CheckState = CheckState.Checked;
      this.mnuFiller_Text_html.Name = "mnuFiller_Text_html";
      this.mnuFiller_Text_html.Size = new Size(129, 22);
      this.mnuFiller_Text_html.Text = "*.HTML";
      this.mnuFiller_Text_html.CheckedChanged += new EventHandler(this.mnuFiller_FileStyle);
      this.mnuFiller_Sprite_spr.Checked = true;
      this.mnuFiller_Sprite_spr.CheckOnClick = true;
      this.mnuFiller_Sprite_spr.CheckState = CheckState.Checked;
      this.mnuFiller_Sprite_spr.Name = "mnuFiller_Sprite_spr";
      this.mnuFiller_Sprite_spr.Size = new Size(129, 22);
      this.mnuFiller_Sprite_spr.Text = "*.SPR";
      this.mnuFiller_Sprite_spr.Click += new EventHandler(this.mnuFiller_FileStyle);
      this.mnuFiller_Tile_til.Checked = true;
      this.mnuFiller_Tile_til.CheckOnClick = true;
      this.mnuFiller_Tile_til.CheckState = CheckState.Checked;
      this.mnuFiller_Tile_til.Name = "mnuFiller_Tile_til";
      this.mnuFiller_Tile_til.Size = new Size(129, 22);
      this.mnuFiller_Tile_til.Text = "*.TIL";
      this.mnuFiller_Tile_til.Click += new EventHandler(this.mnuFiller_FileStyle);
      this.toolStripSeparator5.Name = "toolStripSeparator5";
      this.toolStripSeparator5.Size = new Size(126, 6);
      this.mnuFiller_Text_C.CheckOnClick = true;
      this.mnuFiller_Text_C.Name = "mnuFiller_Text_C";
      this.mnuFiller_Text_C.Size = new Size(129, 22);
      this.mnuFiller_Text_C.Text = "Taiwan";
      this.mnuFiller_Text_C.CheckedChanged += new EventHandler(this.mnuFiller_Text_Language);
      this.mnuFiller_Text_H.CheckOnClick = true;
      this.mnuFiller_Text_H.Name = "mnuFiller_Text_H";
      this.mnuFiller_Text_H.Size = new Size(129, 22);
      this.mnuFiller_Text_H.Text = "Chian && HK";
      this.mnuFiller_Text_H.CheckedChanged += new EventHandler(this.mnuFiller_Text_Language);
      this.mnuFiller_Text_J.CheckOnClick = true;
      this.mnuFiller_Text_J.Name = "mnuFiller_Text_J";
      this.mnuFiller_Text_J.Size = new Size(129, 22);
      this.mnuFiller_Text_J.Text = "Japan";
      this.mnuFiller_Text_J.CheckedChanged += new EventHandler(this.mnuFiller_Text_Language);
      this.mnuFiller_Text_K.CheckOnClick = true;
      this.mnuFiller_Text_K.Name = "mnuFiller_Text_K";
      this.mnuFiller_Text_K.Size = new Size(129, 22);
      this.mnuFiller_Text_K.Text = "Korean";
      this.mnuFiller_Text_K.CheckedChanged += new EventHandler(this.mnuFiller_Text_Language);
      this.toolStripSeparator7.Name = "toolStripSeparator7";
      this.toolStripSeparator7.Size = new Size(126, 6);
      this.mnuFiller_Sprite_img.Checked = true;
      this.mnuFiller_Sprite_img.CheckOnClick = true;
      this.mnuFiller_Sprite_img.CheckState = CheckState.Checked;
      this.mnuFiller_Sprite_img.Name = "mnuFiller_Sprite_img";
      this.mnuFiller_Sprite_img.Size = new Size(129, 22);
      this.mnuFiller_Sprite_img.Text = "*.IMG";
      this.mnuFiller_Sprite_img.Click += new EventHandler(this.mnuFiller_FileStyle);
      this.mnuFiller_Sprite_png.Checked = true;
      this.mnuFiller_Sprite_png.CheckOnClick = true;
      this.mnuFiller_Sprite_png.CheckState = CheckState.Checked;
      this.mnuFiller_Sprite_png.Name = "mnuFiller_Sprite_png";
      this.mnuFiller_Sprite_png.Size = new Size(129, 22);
      this.mnuFiller_Sprite_png.Text = "*.PNG";
      this.mnuFiller_Sprite_png.Click += new EventHandler(this.mnuFiller_FileStyle);
      this.mnuFiller_Sprite_tbt.Checked = true;
      this.mnuFiller_Sprite_tbt.CheckOnClick = true;
      this.mnuFiller_Sprite_tbt.CheckState = CheckState.Checked;
      this.mnuFiller_Sprite_tbt.Name = "mnuFiller_Sprite_tbt";
      this.mnuFiller_Sprite_tbt.Size = new Size(129, 22);
      this.mnuFiller_Sprite_tbt.Text = "*.TBT";
      this.mnuFiller_Sprite_tbt.Click += new EventHandler(this.mnuFiller_FileStyle);
      this.toolStripSeparator4.Name = "toolStripSeparator4";
      this.toolStripSeparator4.Size = new Size(113, 6);
      this.mnuLanguage.DropDownItems.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.mnuLanguage_TW,
        (ToolStripItem) this.mnuLanguage_EN
      });
      this.mnuLanguage.Enabled = false;
      this.mnuLanguage.Image = (Image) Resources.Settings1;
      this.mnuLanguage.Name = "mnuLanguage";
      this.mnuLanguage.Size = new Size(116, 22);
      this.mnuLanguage.Text = "&Language";
      this.mnuLanguage_TW.Checked = true;
      this.mnuLanguage_TW.CheckState = CheckState.Checked;
      this.mnuLanguage_TW.Name = "mnuLanguage_TW";
      this.mnuLanguage_TW.Size = new Size(133, 22);
      this.mnuLanguage_TW.Text = "繁體中文(zh_tw)";
      this.mnuLanguage_EN.Name = "mnuLanguage_EN";
      this.mnuLanguage_EN.Size = new Size(133, 22);
      this.mnuLanguage_EN.Text = "&English";
      this.mnuTools.DropDownItems.AddRange(new ToolStripItem[9]
      {
        (ToolStripItem) this.mnuTools_Export,
        (ToolStripItem) this.mnuTools_ExportTo,
        (ToolStripItem) this.mnuTools_Delete,
        (ToolStripItem) this.toolStripSeparator8,
        (ToolStripItem) this.mnuTools_Add,
        (ToolStripItem) this.mnuTools_Update,
        (ToolStripItem) this.toolStripSeparator9,
        (ToolStripItem) this.mnuTools_ClearSelect,
        (ToolStripItem) this.mnuTools_SelectAll
      });
      this.mnuTools.Name = "mnuTools";
      this.mnuTools.Size = new Size(43, 20);
      this.mnuTools.Text = "&Tools";
      this.mnuTools_Export.Enabled = false;
      this.mnuTools_Export.Image = (Image) Resources.Save;
      this.mnuTools_Export.Name = "mnuTools_Export";
      this.mnuTools_Export.Size = new Size((int) sbyte.MaxValue, 22);
      this.mnuTools_Export.Text = "&Export";
      this.mnuTools_Export.ToolTipText = "Export selected file to the pak folder";
      this.mnuTools_Export.Click += new EventHandler(this.mnuTools_Export_Click);
      this.mnuTools_ExportTo.Enabled = false;
      this.mnuTools_ExportTo.Name = "mnuTools_ExportTo";
      this.mnuTools_ExportTo.Size = new Size((int) sbyte.MaxValue, 22);
      this.mnuTools_ExportTo.Text = "Export &To...";
      this.mnuTools_ExportTo.ToolTipText = "Export selected file to specific folder";
      this.mnuTools_ExportTo.Click += new EventHandler(this.mnuTools_ExportTo_Click);
      this.mnuTools_Delete.Enabled = false;
      this.mnuTools_Delete.Image = (Image) Resources.Trashcan_empty;
      this.mnuTools_Delete.Name = "mnuTools_Delete";
      this.mnuTools_Delete.Size = new Size((int) sbyte.MaxValue, 22);
      this.mnuTools_Delete.Text = "&Delete";
      this.mnuTools_Delete.Click += new EventHandler(this.mnuTools_Delete_Click);
      this.toolStripSeparator8.Name = "toolStripSeparator8";
      this.toolStripSeparator8.Size = new Size(124, 6);
      this.mnuTools_Add.Enabled = false;
      this.mnuTools_Add.Name = "mnuTools_Add";
      this.mnuTools_Add.Size = new Size((int) sbyte.MaxValue, 22);
      this.mnuTools_Add.Text = "&Add..";
      this.mnuTools_Add.Click += new EventHandler(this.mnuTools_Add_Click);
      this.mnuTools_Update.Enabled = false;
      this.mnuTools_Update.Name = "mnuTools_Update";
      this.mnuTools_Update.Size = new Size((int) sbyte.MaxValue, 22);
      this.mnuTools_Update.Text = "&Update";
      this.mnuTools_Update.ToolTipText = "The function only work in text file.";
      this.mnuTools_Update.Click += new EventHandler(this.mnuTools_Update_Click);
      this.toolStripSeparator9.Name = "toolStripSeparator9";
      this.toolStripSeparator9.Size = new Size(124, 6);
      this.mnuTools_ClearSelect.Name = "mnuTools_ClearSelect";
      this.mnuTools_ClearSelect.Size = new Size((int) sbyte.MaxValue, 22);
      this.mnuTools_ClearSelect.Text = "Unselect All";
      this.mnuTools_ClearSelect.Click += new EventHandler(this.tsmUnselectAll_Click);
      this.mnuTools_SelectAll.Name = "mnuTools_SelectAll";
      this.mnuTools_SelectAll.Size = new Size((int) sbyte.MaxValue, 22);
      this.mnuTools_SelectAll.Text = "Select All";
      this.mnuTools_SelectAll.Click += new EventHandler(this.tsmSelectAll_Click);
      this.dlgOpenFile.FileName = "openFileDialog1";
      this.dlgOpenFile.Multiselect = true;
      this.splitContainer1.Dock = DockStyle.Fill;
      this.splitContainer1.FixedPanel = FixedPanel.Panel1;
      this.splitContainer1.Location = new Point(0, 24);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Panel1.Controls.Add((Control) this.splitContainer2);
      this.splitContainer1.Panel2.Controls.Add((Control) this.TextCompViewer);
      this.splitContainer1.Panel2.Controls.Add((Control) this.TextViewer);
      this.splitContainer1.Panel2.Controls.Add((Control) this.ImageViewer);
      this.splitContainer1.Panel2.Controls.Add((Control) this.SprViewer);
      this.splitContainer1.Size = new Size(792, 520);
      this.splitContainer1.SplitterDistance = 297;
      this.splitContainer1.TabIndex = 2;
      this.splitContainer2.Dock = DockStyle.Fill;
      this.splitContainer2.FixedPanel = FixedPanel.Panel1;
      this.splitContainer2.Location = new Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = Orientation.Horizontal;
      this.splitContainer2.Panel1.Controls.Add((Control) this.palSearch);
      this.splitContainer2.Panel2.Controls.Add((Control) this.lvIndexInfo);
      this.splitContainer2.Size = new Size(297, 520);
      this.splitContainer2.SplitterDistance = 25;
      this.splitContainer2.TabIndex = 2;
      this.palSearch.AutoSize = true;
      this.palSearch.Controls.Add((Control) this.label1);
      this.palSearch.Controls.Add((Control) this.txtSearch);
      this.palSearch.Dock = DockStyle.Top;
      this.palSearch.Location = new Point(0, 0);
      this.palSearch.Name = "palSearch";
      this.palSearch.Size = new Size(297, 28);
      this.palSearch.TabIndex = 1;
      this.palSearch.WrapContents = false;
      this.label1.Location = new Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new Size(62, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "Condition : ";
      this.label1.TextAlign = ContentAlignment.MiddleLeft;
      this.txtSearch.Location = new Point(71, 3);
      this.txtSearch.Name = "txtSearch";
      this.txtSearch.Size = new Size(223, 22);
      this.txtSearch.TabIndex = 1;
      this.txtSearch.KeyPress += new KeyPressEventHandler(this.txtSearch_KeyPress);
      this.lvIndexInfo.CheckBoxes = true;
      this.lvIndexInfo.Dock = DockStyle.Fill;
      this.lvIndexInfo.Font = new Font("細明體", 9f);
      this.lvIndexInfo.FullRowSelect = true;
      this.lvIndexInfo.HideSelection = false;
      this.lvIndexInfo.Location = new Point(0, 0);
      this.lvIndexInfo.Name = "lvIndexInfo";
      this.lvIndexInfo.Size = new Size(297, 491);
      this.lvIndexInfo.TabIndex = 0;
      this.lvIndexInfo.UseCompatibleStateImageBehavior = false;
      this.lvIndexInfo.View = View.Details;
      this.lvIndexInfo.MouseClick += new MouseEventHandler(this.lvIndexInfo_MouseClick);
      this.lvIndexInfo.SelectedIndexChanged += new EventHandler(this.lvIndexInfo_SelectedIndexChanged);
      this.lvIndexInfo.ColumnClick += new ColumnClickEventHandler(this.lvIndexInfo_ColumnClick);
      this.TextCompViewer.Location = new Point(0, 0);
      this.TextCompViewer.Name = "TextCompViewer";
      this.TextCompViewer.Size = new Size(184, 162);
      this.TextCompViewer.TabIndex = 5;
      this.TextCompViewer.Visible = false;
      this.TextViewer.Font = new Font("新細明體", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 136);
      this.TextViewer.Location = new Point(0, 0);
      this.TextViewer.Multiline = true;
      this.TextViewer.Name = "TextViewer";
      this.TextViewer.ScrollBars = ScrollBars.Both;
      this.TextViewer.Size = new Size(228, 200);
      this.TextViewer.TabIndex = 1;
      this.TextViewer.WordWrap = false;
      this.ImageViewer.AutoScroll = true;
      this.ImageViewer.BackColor = Color.Black;
      this.ImageViewer.BorderStyle = BorderStyle.Fixed3D;
      this.ImageViewer.Image = (Image) null;
      this.ImageViewer.Location = new Point(0, 0);
      this.ImageViewer.Name = "ImageViewer";
      this.ImageViewer.Size = new Size(267, 240);
      this.ImageViewer.TabIndex = 4;
      this.SprViewer.AutoScroll = true;
      this.SprViewer.BackColor = Color.Red;
      this.SprViewer.BorderStyle = BorderStyle.FixedSingle;
      this.SprViewer.Location = new Point(0, 0);
      this.SprViewer.Name = "SprViewer";
      this.SprViewer.Size = new Size(324, 277);
      this.SprViewer.SprFrames = (L1Spr.Frame[]) null;
      this.SprViewer.TabIndex = 3;
      this.SprViewer.Visible = false;
      this.ctxMenu.Items.AddRange(new ToolStripItem[7]
      {
        (ToolStripItem) this.tsmExport,
        (ToolStripItem) this.tsmExportTo,
        (ToolStripItem) this.toolStripSeparator6,
        (ToolStripItem) this.tsmUnselectAll,
        (ToolStripItem) this.tsmSelectAll,
        (ToolStripItem) this.toolStripSeparator3,
        (ToolStripItem) this.tsmCompare
      });
      this.ctxMenu.Name = "ctxMenu";
      this.ctxMenu.Size = new Size(137, 126);
      this.ctxMenu.Opening += new CancelEventHandler(this.ctxMenu_Opening);
      this.tsmExport.Image = (Image) Resources.Save;
      this.tsmExport.Name = "tsmExport";
      this.tsmExport.Size = new Size(136, 22);
      this.tsmExport.Text = "&Export";
      this.tsmExport.ToolTipText = "export the file to the pak folder ";
      this.tsmExport.Click += new EventHandler(this.tsmExport_Click);
      this.tsmExportTo.Name = "tsmExportTo";
      this.tsmExportTo.Size = new Size(136, 22);
      this.tsmExportTo.Text = "Export &To...";
      this.tsmExportTo.ToolTipText = "Export the file to specific folder";
      this.tsmExportTo.Click += new EventHandler(this.tsmExportTo_Click);
      this.toolStripSeparator6.Name = "toolStripSeparator6";
      this.toolStripSeparator6.Size = new Size(133, 6);
      this.tsmUnselectAll.Name = "tsmUnselectAll";
      this.tsmUnselectAll.Size = new Size(136, 22);
      this.tsmUnselectAll.Text = "Unselect All";
      this.tsmUnselectAll.Click += new EventHandler(this.tsmUnselectAll_Click);
      this.tsmSelectAll.Name = "tsmSelectAll";
      this.tsmSelectAll.Size = new Size(136, 22);
      this.tsmSelectAll.Text = "Select All";
      this.tsmSelectAll.Click += new EventHandler(this.tsmSelectAll_Click);
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new Size(133, 6);
      this.tsmCompare.DropDownItems.AddRange(new ToolStripItem[4]
      {
        (ToolStripItem) this.tsmCompTW,
        (ToolStripItem) this.tsmCompHK,
        (ToolStripItem) this.tsmCompJP,
        (ToolStripItem) this.tsmCompKO
      });
      this.tsmCompare.Name = "tsmCompare";
      this.tsmCompare.Size = new Size(136, 22);
      this.tsmCompare.Text = "Compare with";
      this.tsmCompare.DropDownOpening += new EventHandler(this.tsmCompare_DropDownOpening);
      this.tsmCompTW.Name = "tsmCompTW";
      this.tsmCompTW.Size = new Size(129, 22);
      this.tsmCompTW.Text = "Taiwan";
      this.tsmCompTW.Click += new EventHandler(this.tsmCompare_Click);
      this.tsmCompHK.Name = "tsmCompHK";
      this.tsmCompHK.Size = new Size(129, 22);
      this.tsmCompHK.Text = "China && HK";
      this.tsmCompHK.Click += new EventHandler(this.tsmCompare_Click);
      this.tsmCompJP.Name = "tsmCompJP";
      this.tsmCompJP.Size = new Size(129, 22);
      this.tsmCompJP.Text = "Japan";
      this.tsmCompJP.Click += new EventHandler(this.tsmCompare_Click);
      this.tsmCompKO.Name = "tsmCompKO";
      this.tsmCompKO.Size = new Size(129, 22);
      this.tsmCompKO.Text = "Korean";
      this.tsmCompKO.Click += new EventHandler(this.tsmCompare_Click);
      this.statusStrip1.Items.AddRange(new ToolStripItem[7]
      {
        (ToolStripItem) this.tssLocker,
        (ToolStripItem) this.tssRecordCount,
        (ToolStripItem) this.tssShowInListView,
        (ToolStripItem) this.tssCheckedCount,
        (ToolStripItem) this.tssMessage,
        (ToolStripItem) this.tssProgressName,
        (ToolStripItem) this.tssProgress
      });
      this.statusStrip1.Location = new Point(0, 544);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new Size(792, 22);
      this.statusStrip1.TabIndex = 5;
      this.statusStrip1.Text = "statusStrip1";
      this.tssLocker.BorderSides = ToolStripStatusLabelBorderSides.All;
      this.tssLocker.BorderStyle = Border3DStyle.SunkenOuter;
      this.tssLocker.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tssLocker.Image = (Image) Resources.Locker;
      this.tssLocker.Name = "tssLocker";
      this.tssLocker.Size = new Size(20, 20);
      this.tssLocker.Visible = false;
      this.tssRecordCount.BorderSides = ToolStripStatusLabelBorderSides.Right;
      this.tssRecordCount.Name = "tssRecordCount";
      this.tssRecordCount.Size = new Size(56, 17);
      this.tssRecordCount.Text = "Records:0";
      this.tssShowInListView.BorderSides = ToolStripStatusLabelBorderSides.Right;
      this.tssShowInListView.Name = "tssShowInListView";
      this.tssShowInListView.Size = new Size(50, 17);
      this.tssShowInListView.Text = "Shown:0";
      this.tssCheckedCount.BorderSides = ToolStripStatusLabelBorderSides.Right;
      this.tssCheckedCount.Name = "tssCheckedCount";
      this.tssCheckedCount.Size = new Size(54, 20);
      this.tssCheckedCount.Text = "Selected :0";
      this.tssCheckedCount.Visible = false;
      this.tssMessage.Name = "tssMessage";
      this.tssMessage.Size = new Size(671, 17);
      this.tssMessage.Spring = true;
      this.tssMessage.TextAlign = ContentAlignment.MiddleLeft;
      this.tssProgressName.BorderSides = ToolStripStatusLabelBorderSides.Left;
      this.tssProgressName.Name = "tssProgressName";
      this.tssProgressName.Size = new Size(57, 20);
      this.tssProgressName.Text = "Loading...";
      this.tssProgressName.Visible = false;
      this.tssProgressName.TextChanged += new EventHandler(this.tssProgressName_TextChanged);
      this.tssProgress.Name = "tssProgress";
      this.tssProgress.Size = new Size(200, 19);
      this.tssProgress.Visible = false;
      this.dlgAddFiles.FileName = "openFileDialog1";
      this.dlgAddFiles.Multiselect = true;
      this.AutoScaleDimensions = new SizeF(6f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(792, 566);
      this.Controls.Add((Control) this.splitContainer1);
      this.Controls.Add((Control) this.statusStrip1);
      this.Controls.Add((Control) this.menuStrip1);
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "frmMain";
      this.Text = "Lineage I Pack File Viewer";
      this.WindowState = FormWindowState.Maximized;
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.Panel2.PerformLayout();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel1.PerformLayout();
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.ResumeLayout(false);
      this.palSearch.ResumeLayout(false);
      this.palSearch.PerformLayout();
      this.ctxMenu.ResumeLayout(false);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private class ListViewItemComparer : IComparer
    {
      private int col;
      private int sorting;

      public ListViewItemComparer()
      {
        this.col = 0;
        this.sorting = 1;
      }

      public ListViewItemComparer(int column)
      {
        this.sorting = column >= 0 ? 1 : -1;
        this.col = column * this.sorting;
      }

      public int Compare(object x, object y)
      {
        string text1 = ((ListViewItem) x).SubItems[this.col].Text;
        string text2 = ((ListViewItem) y).SubItems[this.col].Text;
        if (this.col == 1 || this.col == 3)
          return this.sorting * string.Compare(text1, text2);
        return this.sorting * (int.Parse(text1.Replace(",", "")) - int.Parse(text2.Replace(",", "")));
      }
    }

    private void mnuFiller_FileStyle(object sender, EventArgs e)
    {
      UpdateLanguageFilters();
      if (_pakFileManager?.IndexRecords != null)
      {
        ShowRecords(_pakFileManager.IndexRecords);
      }
    }

    private void mnuTools_Update_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(_PackFileName))
        return;
      _fileOperationsManager.RebuildIndex(_PackFileName);
    }

    private void tsmSelectAll_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in lvIndexInfo.Items)
        item.Selected = true;
    }

    private void tsmUnselectAll_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in lvIndexInfo.Items)
        item.Selected = false;
    }

    private void mnuTools_Export_Click(object sender, EventArgs e)
    {
      _fileOperationsManager.ExportSelectedFiles(lvIndexInfo.SelectedItems);
    }

    private void mnuTools_ExportTo_Click(object sender, EventArgs e)
    {
      _fileOperationsManager.ExportSelectedFilesTo(lvIndexInfo.SelectedItems);
    }

    private void tsmExport_Click(object sender, EventArgs e)
    {
      _fileOperationsManager.ExportSelectedFiles(lvIndexInfo.SelectedItems);
    }

    private void tsmExportTo_Click(object sender, EventArgs e)
    {
      _fileOperationsManager.ExportSelectedFilesTo(lvIndexInfo.SelectedItems);
    }
  }
}
