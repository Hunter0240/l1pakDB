using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace PakViewer.Utility
{
    public class PakFileManager
    {
        private string _packFilePath;
        private bool _isProtected;
        private L1PakTools.IndexRecord[] _indexRecords;

        public bool IsProtected => _isProtected;
        public L1PakTools.IndexRecord[] IndexRecords 
        { 
            get => _indexRecords;
            set => _indexRecords = value;
        }
        public string PackFilePath => _packFilePath;

        public PakFileManager(string packFilePath)
        {
            _packFilePath = packFilePath.ToLower();
        }

        public bool LoadIndexFile()
        {
            try
            {
                byte[] indexData = LoadIndexData(_packFilePath);
                if (indexData == null)
                    return false;

                _indexRecords = CreateIndexRecords(indexData);
                return _indexRecords != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private byte[] LoadIndexData(string indexFile)
        {
            byte[] data = File.ReadAllBytes(indexFile);
            int recordCount = (data.Length - 4) / 28;

            if (data.Length < 32 || (data.Length - 4) % 28 != 0)
                return null;

            if (BitConverter.ToUInt32(data, 0) != recordCount)
                return null;

            _isProtected = false;

            if (!Regex.IsMatch(Encoding.Default.GetString(data, 8, 20), "^([a-zA-Z0-9_\\-\\.']+)", RegexOptions.IgnoreCase))
            {
                if (!Regex.IsMatch(L1PakTools.Decode_Index_FirstRecord(data).FileName, "^([a-zA-Z0-9_\\-\\.']+)", RegexOptions.IgnoreCase))
                    return null;

                _isProtected = true;
                data = L1PakTools.Decode(data, 4);
            }

            return data;
        }

        private L1PakTools.IndexRecord[] CreateIndexRecords(byte[] indexData)
        {
            if (indexData == null)
                return null;

            int offset = _isProtected ? 0 : 4;
            int length = (indexData.Length - offset) / 28;
            var records = new L1PakTools.IndexRecord[length];

            for (int i = 0; i < length; i++)
            {
                int index = offset + i * 28;
                records[i] = new L1PakTools.IndexRecord(indexData, index);
            }

            return records;
        }

        public object LoadPakData(int recordIndex)
        {
            if (_indexRecords == null || recordIndex < 0 || recordIndex >= _indexRecords.Length)
                return null;

            var record = _indexRecords[recordIndex];
            string pakFile = Path.ChangeExtension(_packFilePath, ".pak");
            
            using (var fs = new FileStream(pakFile, FileMode.Open, FileAccess.Read))
            {
                return LoadPakData(fs, record);
            }
        }

        private object LoadPakData(FileStream fs, L1PakTools.IndexRecord record)
        {
            byte[] data = LoadPakBytes(fs, record);
            if (data == null) return null;

            string extension = Path.GetExtension(record.FileName).ToLower();
            
            // Return raw bytes for now - in a future refactoring we can add specific file type handlers
            return data;
        }

        private byte[] LoadPakBytes(FileStream fs, L1PakTools.IndexRecord record)
        {
            try
            {
                byte[] buffer = new byte[record.FileSize];
                fs.Seek(record.Offset, SeekOrigin.Begin);
                fs.Read(buffer, 0, record.FileSize);

                if (_isProtected)
                    return L1PakTools.Decode(buffer, record.Offset);

                return buffer;
            }
            catch
            {
                return null;
            }
        }
    }
} 