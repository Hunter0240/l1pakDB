using System;
using System.Windows.Forms;
using System.Drawing;

namespace PakViewer.Utility
{
    public class UIStateManager
    {
        private readonly TextBox _textViewer;
        private readonly ucImgViewer _imageViewer;
        private readonly ucSprViewer _sprViewer;
        private readonly ucTextCompare _textCompViewer;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly ToolStripStatusLabel _progressLabel;
        private readonly ToolStripProgressBar _progressBar;
        private readonly ListView _listView;
        private readonly MenuStrip _menuStrip;

        public UIStateManager(
            TextBox textViewer,
            ucImgViewer imageViewer,
            ucSprViewer sprViewer,
            ucTextCompare textCompViewer,
            ToolStripStatusLabel statusLabel,
            ToolStripStatusLabel progressLabel,
            ToolStripProgressBar progressBar,
            ListView listView,
            MenuStrip menuStrip)
        {
            _textViewer = textViewer;
            _imageViewer = imageViewer;
            _sprViewer = sprViewer;
            _textCompViewer = textCompViewer;
            _statusLabel = statusLabel;
            _progressLabel = progressLabel;
            _progressBar = progressBar;
            _listView = listView;
            _menuStrip = menuStrip;
        }

        public void UpdateViewerVisibility(InviewDataType viewType)
        {
            _textCompViewer.Visible = false;
            _textViewer.Visible = false;
            _imageViewer.Visible = false;
            _sprViewer.Visible = false;

            switch (viewType)
            {
                case InviewDataType.Text:
                    _textViewer.Visible = true;
                    break;
                case InviewDataType.IMG:
                case InviewDataType.BMP:
                case InviewDataType.TBT:
                case InviewDataType.TIL:
                    _imageViewer.Visible = true;
                    break;
                case InviewDataType.SPR:
                    _sprViewer.Visible = true;
                    break;
            }
        }

        public void UpdateProgressDisplay(string text, bool visible)
        {
            _progressLabel.Text = text;
            _progressLabel.Visible = visible;
            _progressBar.Visible = visible;
            if (!visible)
            {
                _progressBar.Value = 0;
            }
        }

        public void SetStatusMessage(string message)
        {
            _statusLabel.Text = message;
        }

        public void UpdateMenuState(bool fileLoaded, bool isProtected)
        {
            var fillerMenu = _menuStrip.Items.Find("mnuFiller", true)[0] as ToolStripMenuItem;
            var rebuildMenu = _menuStrip.Items.Find("mnuRebuild", true)[0] as ToolStripMenuItem;
            if (fillerMenu != null) fillerMenu.Enabled = fileLoaded;
            if (rebuildMenu != null) rebuildMenu.Enabled = fileLoaded;
        }

        public void UpdateToolsMenuState()
        {
            var toolsMenu = _menuStrip.Items.Find("mnuTools", true)[0] as ToolStripMenuItem;
            if (toolsMenu == null) return;

            var exportMenu = toolsMenu.DropDownItems.Find("mnuTools_Export", true)[0] as ToolStripMenuItem;
            var exportToMenu = toolsMenu.DropDownItems.Find("mnuTools_ExportTo", true)[0] as ToolStripMenuItem;
            var deleteMenu = toolsMenu.DropDownItems.Find("mnuTools_Delete", true)[0] as ToolStripMenuItem;
            var addMenu = toolsMenu.DropDownItems.Find("mnuTools_Add", true)[0] as ToolStripMenuItem;
            var updateMenu = toolsMenu.DropDownItems.Find("mnuTools_Update", true)[0] as ToolStripMenuItem;

            bool hasCheckedItems = _listView.CheckedItems.Count > 0;
            bool hasItems = _listView.Items.Count > 0;

            if (exportMenu != null) exportMenu.Enabled = hasCheckedItems;
            if (exportToMenu != null) exportToMenu.Enabled = hasCheckedItems;
            if (deleteMenu != null) deleteMenu.Enabled = hasCheckedItems;
            if (addMenu != null) addMenu.Enabled = hasItems;
            if (updateMenu != null) updateMenu.Enabled = _textViewer.Modified;
        }

        public void UpdateListView(ListViewItem[] items)
        {
            _listView.SuspendLayout();
            _listView.Items.Clear();
            _listView.Items.AddRange(items);
            _listView.ResumeLayout();
        }

        public void SetCursor(Cursor cursor)
        {
            if (_listView.InvokeRequired)
            {
                _listView.Invoke(new Action(() => _listView.Cursor = cursor));
            }
            else
            {
                _listView.Cursor = cursor;
            }
        }
    }
} 