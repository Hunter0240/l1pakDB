using System;
using System.IO;
using System.Collections.Generic;

namespace PakViewer.Utility
{
    public class FileFilterManager
    {
        private HashSet<string> _enabledExtensions;
        private HashSet<string> _enabledLanguages;

        public FileFilterManager()
        {
            _enabledExtensions = new HashSet<string>();
            _enabledLanguages = new HashSet<string>();
        }

        public void SetExtensionFilter(string extension, bool enabled)
        {
            extension = extension.ToLower();
            if (enabled)
                _enabledExtensions.Add(extension);
            else
                _enabledExtensions.Remove(extension);
        }

        public void SetLanguageFilter(string language, bool enabled)
        {
            language = language.ToLower();
            if (enabled)
                _enabledLanguages.Add(language);
            else
                _enabledLanguages.Remove(language);
        }

        public bool ShouldShowFile(L1PakTools.IndexRecord record)
        {
            string fileName = record.FileName.ToLower();
            string extension = Path.GetExtension(fileName);
            
            // If no extensions are enabled, show all files
            if (_enabledExtensions.Count > 0 && !_enabledExtensions.Contains(extension))
                return false;

            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            if (nameWithoutExt.LastIndexOf("-") < 0 || nameWithoutExt.Length < 2)
                return true;

            string lang = nameWithoutExt.Substring(nameWithoutExt.Length - 2);
            
            // If no languages are enabled, show all files
            if (_enabledLanguages.Count > 0 && !_enabledLanguages.Contains(lang))
                return false;

            return true;
        }

        public void ClearFilters()
        {
            _enabledExtensions.Clear();
            _enabledLanguages.Clear();
        }
    }
} 