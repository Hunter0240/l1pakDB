﻿// Decompiled with JetBrains decompiler
// Type: PakViewer.ucImgViewer
// Assembly: PakViewer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1B8FBB7F-36BB-4233-90DD-580453361518
// Assembly location: C:\Users\TonyQ\Downloads\PakViewer.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PakViewer
{
  public class ucImgViewer : UserControl
  {
    private Image srcImage;
    private IContainer components;
    private PictureBox pictureBox1;
    private TrackBar tbScale;

    [Browsable(false)]
    public Image Image
    {
      get
      {
        return this.pictureBox1.Image;
      }
      set
      {
        this.srcImage = value;
        this.ShowImage(value);
      }
    }

    public ucImgViewer()
    {
      this.InitializeComponent();
    }

    private void ShowImage(Image img)
    {
      this.pictureBox1.Image = img;
      if (img == null)
        return;
      this.pictureBox1.Width = img.Width * this.tbScale.Value / 2;
      this.pictureBox1.Height = img.Height * this.tbScale.Value / 2;
      this.pictureBox1.Left = (this.Width - this.pictureBox1.Width) / 2;
      this.pictureBox1.Top = (this.Height - this.pictureBox1.Height) / 2;
    }

    private void tbScale_Scroll(object sender, EventArgs e)
    {
      this.ShowImage(this.srcImage);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.tbScale = new TrackBar();
      this.pictureBox1 = new PictureBox();
      this.tbScale.BeginInit();
      ((ISupportInitialize) this.pictureBox1).BeginInit();
      this.SuspendLayout();
      this.tbScale.LargeChange = 2;
      this.tbScale.Location = new Point(3, 3);
      this.tbScale.Minimum = 1;
      this.tbScale.Name = "tbScale";
      this.tbScale.Orientation = Orientation.Vertical;
      this.tbScale.Size = new Size(45, 180);
      this.tbScale.TabIndex = 3;
      this.tbScale.Value = 2;
      this.tbScale.Scroll += new EventHandler(this.tbScale_Scroll);
      this.pictureBox1.Location = new Point(158, 133);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new Size(100, 50);
      this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      this.AutoScaleDimensions = new SizeF(6f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.AutoScroll = true;
      this.BorderStyle = BorderStyle.Fixed3D;
      this.Controls.Add((Control) this.tbScale);
      this.Controls.Add((Control) this.pictureBox1);
      this.Name = "ucImgViewer";
      this.Size = new Size(438, 344);
      this.tbScale.EndInit();
      ((ISupportInitialize) this.pictureBox1).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
