using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace PakViewer.Utility
{
    public class FileOperationsManager
    {
        private readonly PakFileManager _pakFileManager;
        private readonly Action<string> _setProgressText;
        private readonly Action<int> _setProgressValue;
        private readonly Action<int> _setProgressMaximum;

        public FileOperationsManager(
            PakFileManager pakFileManager, 
            Action<string> setProgressText,
            Action<int> setProgressValue,
            Action<int> setProgressMaximum)
        {
            _pakFileManager = pakFileManager;
            _setProgressText = setProgressText;
            _setProgressValue = setProgressValue;
            _setProgressMaximum = setProgressMaximum;
        }

        public (object content, InviewDataType viewType) LoadPakData(FileStream fs, ListViewItem lvItem)
        {
            L1PakTools.IndexRecord indexRecord = _pakFileManager.IndexRecords[int.Parse(lvItem.Text) - 1];
            return LoadPakData(fs, indexRecord);
        }

        public (object content, InviewDataType viewType) LoadPakData(FileStream fs, L1PakTools.IndexRecord IdxRec)
        {
            string[] array = new string[13]
            {
                ".img", ".png", ".tbt", ".til", ".html",
                ".tbl", ".spr", ".bmp", ".h", ".ht",
                ".htm", ".txt", ".def"
            };

            InviewDataType[] inviewDataTypeArray = new InviewDataType[13]
            {
                InviewDataType.IMG, InviewDataType.BMP, InviewDataType.TBT,
                InviewDataType.Empty, InviewDataType.Text, InviewDataType.Text,
                InviewDataType.SPR, InviewDataType.BMP, InviewDataType.Text,
                InviewDataType.Text, InviewDataType.Text, InviewDataType.Text,
                InviewDataType.Text
            };

            int index = Array.IndexOf(array, Path.GetExtension(IdxRec.FileName).ToLower());
            InviewDataType viewType = index != -1 ? inviewDataTypeArray[index] : InviewDataType.Empty;

            if (IdxRec.FileName.IndexOf("list.spr") != -1)
            {
                viewType = InviewDataType.Text;
            }

            byte[] numArray = new byte[IdxRec.FileSize];
            fs.Seek(IdxRec.Offset, SeekOrigin.Begin);
            fs.Read(numArray, 0, IdxRec.FileSize);

            if (_pakFileManager.IsProtected)
            {
                _setProgressText("Decoding...");
                numArray = L1PakTools.Decode(numArray, 0);
                _setProgressText("");
                if (viewType == InviewDataType.SPR)
                    viewType = InviewDataType.Text;
            }

            object result = numArray;
            try
            {
                switch (viewType)
                {
                    case InviewDataType.Text:
                        result = GetEncodedText(IdxRec.FileName, numArray);
                        break;
                    case InviewDataType.IMG:
                        result = ImageConvert.Load_IMG(numArray);
                        break;
                    case InviewDataType.BMP:
                        using (MemoryStream memoryStream = new MemoryStream(numArray))
                        {
                            result = Image.FromStream(memoryStream);
                        }
                        break;
                    case InviewDataType.SPR:
                        result = L1Spr.Load(numArray);
                        break;
                    case InviewDataType.TIL:
                        result = ImageConvert.Load_TIL(numArray);
                        break;
                    case InviewDataType.TBT:
                        result = ImageConvert.Load_TBT(numArray);
                        break;
                }
            }
            catch
            {
                viewType = InviewDataType.Empty;
                throw new Exception("Can't open this file!");
            }

            return (result, viewType);
        }

        private string GetEncodedText(string fileName, byte[] data)
        {
            fileName = fileName.ToLower();
            if (fileName.IndexOf("-k.") >= 0)
                return Encoding.GetEncoding("euc-kr").GetString(data);
            if (fileName.IndexOf("-j.") >= 0)
                return Encoding.GetEncoding("shift_jis").GetString(data);
            if (fileName.IndexOf("-h.") >= 0)
                return Encoding.GetEncoding("euc-cn").GetString(data);
            if (fileName.IndexOf("-c.") >= 0)
                return Encoding.GetEncoding("big5").GetString(data);
            return Encoding.Default.GetString(data);
        }

        public void ExportData(string path, ListViewItem lvItem, object data, byte[] originBytes, InviewDataType viewType)
        {
            string filePath = _pakFileManager.IndexRecords[int.Parse(lvItem.Text) - 1].FileName;
            if (path != null)
                filePath = Path.Combine(path, filePath);

            switch (viewType)
            {
                case InviewDataType.Text:
                    File.WriteAllText(filePath, (string)data);
                    break;
                case InviewDataType.IMG:
                    ((Image)data).Save(filePath.Replace(".img", ".bmp"), ImageFormat.Bmp);
                    break;
                case InviewDataType.BMP:
                    ((Image)data).Save(filePath, ImageFormat.Png);
                    break;
                case InviewDataType.TBT:
                    ((Image)data).Save(filePath.Replace(".tbt", ".gif"), ImageFormat.Gif);
                    break;
                case InviewDataType.SPR:
                    ExportSpriteData(filePath, data, originBytes);
                    break;
                default:
                    File.WriteAllBytes(filePath, originBytes);
                    break;
            }
        }

        private void ExportSpriteData(string filePath, object data, byte[] originBytes)
        {
            L1Spr.Frame[] frameArray = null;
            if (data is byte[])
                frameArray = L1Spr.Load((byte[])data);
            else if (data is L1Spr.Frame[])
                frameArray = (L1Spr.Frame[])data;

            if (frameArray != null)
            {
                for (int index = 0; index < frameArray.Length; ++index)
                {
                    if (frameArray[index].image != null)
                    {
                        string framePath = filePath.Replace(".spr", $"-{index:D2}(view).bmp");
                        frameArray[index].image.Save(framePath, ImageFormat.Bmp);
                    }
                }
            }
            File.WriteAllBytes(filePath, originBytes);
        }

        public void RebuildAll(string packFileName, int[] deleteIds)
        {
            var currentRecords = _pakFileManager.IndexRecords;
            int length = currentRecords.Length - deleteIds.Length;
            L1PakTools.IndexRecord[] items = new L1PakTools.IndexRecord[length];
            int[] keys = new int[length];
            int index1 = 0;
            int index2 = 0;

            for (; index1 < currentRecords.Length; ++index1)
            {
                if (Array.IndexOf(deleteIds, index1) == -1)
                {
                    items[index2] = currentRecords[index1];
                    keys[index2] = currentRecords[index1].Offset;
                    ++index2;
                }
            }

            _pakFileManager.IndexRecords = items;
            _setProgressText("Creating new PAK file...");
            _setProgressMaximum(length);

            string pakFile = packFileName.Replace(".idx", ".pak");
            string backupFile = packFileName.Replace(".idx", ".pa_");

            if (File.Exists(backupFile))
                File.Delete(backupFile);

            File.Move(pakFile, backupFile);

            using (FileStream fileStream1 = File.OpenRead(backupFile))
            using (FileStream fileStream2 = File.OpenWrite(pakFile))
            {
                for (int index3 = 0; index3 < length; ++index3)
                {
                    byte[] buffer = new byte[items[index3].FileSize];
                    fileStream1.Seek(items[index3].Offset, SeekOrigin.Begin);
                    fileStream1.Read(buffer, 0, items[index3].FileSize);
                    items[index3].Offset = (int)fileStream2.Seek(0L, SeekOrigin.End);
                    fileStream2.Write(buffer, 0, items[index3].FileSize);
                    _setProgressValue(index3 + 1);
                }
            }

            _pakFileManager.IndexRecords = items;
            RebuildIndex(packFileName);
        }

        public void RebuildIndex(string packFileName)
        {
            string backupFile = packFileName.Replace(".idx", ".id_");
            if (File.Exists(backupFile))
                File.Delete(backupFile);

            File.Move(packFileName, backupFile);

            byte[] numArray = new byte[4 + _pakFileManager.IndexRecords.Length * 28];
            Array.Copy(BitConverter.GetBytes(_pakFileManager.IndexRecords.Length), 0, numArray, 0, 4);

            _setProgressText("Creating new IDX file...");
            _setProgressMaximum(_pakFileManager.IndexRecords.Length);

            for (int index = 0; index < _pakFileManager.IndexRecords.Length; ++index)
            {
                int destinationIndex = 4 + index * 28;
                Array.Copy(BitConverter.GetBytes(_pakFileManager.IndexRecords[index].Offset), 0, numArray, destinationIndex, 4);
                Encoding.Default.GetBytes(
                    _pakFileManager.IndexRecords[index].FileName, 
                    0, 
                    _pakFileManager.IndexRecords[index].FileName.Length, 
                    numArray, 
                    destinationIndex + 4);
                Array.Copy(
                    BitConverter.GetBytes(_pakFileManager.IndexRecords[index].FileSize), 
                    0, 
                    numArray, 
                    destinationIndex + 24, 
                    4);
                _setProgressValue(index + 1);
            }

            if (_pakFileManager.IsProtected)
            {
                _setProgressText("Encoding...");
                Array.Copy(L1PakTools.Encode(numArray, 4), 0, numArray, 4, numArray.Length - 4);
            }

            File.WriteAllBytes(packFileName, numArray);
            _setProgressText("");
        }

        public void ExportSelectedFiles(ListView.SelectedListViewItemCollection selectedItems)
        {
            string pakDirectory = Path.GetDirectoryName(_pakFileManager.PackFilePath);
            foreach (ListViewItem item in selectedItems)
            {
                string exportPath = Path.Combine(pakDirectory, _pakFileManager.IndexRecords[int.Parse(item.Text) - 1].FileName);
                ExportSelected(exportPath, item);
            }
        }

        public void ExportSelectedFilesTo(ListView.SelectedListViewItemCollection selectedItems)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                foreach (ListViewItem item in selectedItems)
                {
                    string exportPath = Path.Combine(dialog.SelectedPath, _pakFileManager.IndexRecords[int.Parse(item.Text) - 1].FileName);
                    ExportSelected(exportPath, item);
                }
            }
        }

        private void ExportSelected(string path, ListViewItem lvItem)
        {
            using (FileStream fs = File.Open(_pakFileManager.PackFilePath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    (object content, InviewDataType viewType) = LoadPakData(fs, lvItem);
                    ExportData(path, lvItem, content, new byte[0], viewType);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting file: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    public enum InviewDataType
    {
        Empty,
        Text,
        IMG,
        BMP,
        SPR,
        TIL,
        TBT,
    }
} 