using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PakViewer.Utility
{
    public class ViewerManager
    {
        private readonly TextBox _textViewer;
        private readonly ucImgViewer _imageViewer;
        private readonly ucSprViewer _sprViewer;
        private readonly ucTextCompare _textCompViewer;
        private readonly UIStateManager _uiStateManager;
        private InviewDataType _currentViewType;

        public ViewerManager(
            TextBox textViewer,
            ucImgViewer imageViewer,
            ucSprViewer sprViewer,
            ucTextCompare textCompViewer,
            UIStateManager uiStateManager)
        {
            _textViewer = textViewer;
            _imageViewer = imageViewer;
            _sprViewer = sprViewer;
            _textCompViewer = textCompViewer;
            _uiStateManager = uiStateManager;
        }

        public InviewDataType CurrentViewType
        {
            get => _currentViewType;
            set
            {
                _currentViewType = value;
                _uiStateManager.UpdateViewerVisibility(value);
            }
        }

        public void ClearContent()
        {
            _textViewer.Text = string.Empty;
            _textViewer.Tag = null;
            _imageViewer.Image = null;
            _sprViewer.SprFrames = null;
            _textCompViewer.SourceText = string.Empty;
            _textCompViewer.TargetText = string.Empty;
        }

        public void DisplayContent(object content, string tag = null)
        {
            try
            {
                switch (_currentViewType)
                {
                    case InviewDataType.Text:
                        DisplayText(content as string, tag);
                        break;
                    case InviewDataType.IMG:
                    case InviewDataType.BMP:
                    case InviewDataType.TBT:
                    case InviewDataType.TIL:
                        DisplayImage(content as Image);
                        break;
                    case InviewDataType.SPR:
                        DisplaySprite(content as L1Spr.Frame[]);
                        break;
                }
            }
            catch (Exception ex)
            {
                _uiStateManager.SetStatusMessage($"Error displaying content: {ex.Message}");
            }
        }

        private void DisplayText(string text, string tag)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            _textViewer.Text = text;
            _textViewer.Tag = tag;
        }

        private void DisplayImage(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            _imageViewer.Image = image;
        }

        private void DisplaySprite(L1Spr.Frame[] frames)
        {
            if (frames == null)
                throw new ArgumentNullException(nameof(frames));

            _sprViewer.SprFrames = frames;
            _sprViewer.Start();
        }

        public void CompareText(string sourceText, string targetText)
        {
            _textCompViewer.SourceText = sourceText;
            _textCompViewer.TargetText = targetText;
            _textCompViewer.Visible = true;
        }

        public bool IsTextModified => _textViewer.Modified;

        public string GetCurrentText()
        {
            return _currentViewType == InviewDataType.Text ? _textViewer.Text : null;
        }

        public Image GetCurrentImage()
        {
            return (_currentViewType == InviewDataType.IMG || _currentViewType == InviewDataType.BMP) 
                ? _imageViewer.Image 
                : null;
        }
    }
} 