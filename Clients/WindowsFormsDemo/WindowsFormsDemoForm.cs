﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using ZXing;
using ZXing.Common;

namespace WindowsFormsDemo
{
   public partial class WindowsFormsDemoForm : Form
   {
      private WebCam wCam;
      private Timer webCamTimer;

      public WindowsFormsDemoForm()
      {
         InitializeComponent();
      }

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);

         foreach (var format in MultiFormatWriter.SupportedWriters)
            cmbEncoderType.Items.Add(format);
      }

      private void btnClose_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void btnSelectBarcodeImageFileForDecoding_Click(object sender, EventArgs e)
      {
         using (var openDlg = new OpenFileDialog())
         {
            openDlg.FileName = txtBarcodeImageFile.Text;
            openDlg.Multiselect = false;
            if (openDlg.ShowDialog(this) == DialogResult.OK)
            {
               txtBarcodeImageFile.Text = openDlg.FileName;
            }
         }
      }

      private void btnStartDecoding_Click(object sender, EventArgs e)
      {
         var fileName = txtBarcodeImageFile.Text;
         if (!File.Exists(fileName))
         {
            MessageBox.Show(this, String.Format("File not found: {0}", fileName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         var timerStart = DateTime.Now.Ticks;
         var image = (Bitmap)Bitmap.FromFile(fileName);
         var imageSource = new RGBLuminanceSource(image, image.Width, image.Height);
         var binarizer = new HybridBinarizer(imageSource);
         var binaryBitmap = new BinaryBitmap(binarizer);
         var reader = new MultiFormatReader();
         var result = reader.decode(binaryBitmap);
         var timerStop = DateTime.Now.Ticks;
         if (result == null)
         {
            txtContent.Text = "No barcode recognized";
         }
         else
         {
            txtType.Text = result.BarcodeFormat.ToString();
            txtContent.Text = result.Text;
         }
         labDuration.Text = new TimeSpan(timerStop - timerStart).Milliseconds.ToString("0 ms");
      }

      private void txtBarcodeImageFile_TextChanged(object sender, EventArgs e)
      {
         var fileName = txtBarcodeImageFile.Text;
         if (File.Exists(fileName))
            picBarcode.Load(fileName);
      }

      private void btnDecodeWebCam_Click(object sender, EventArgs e)
      {
         if (wCam == null)
         {
            wCam = new WebCam {Container = picWebCam};

            wCam.OpenConnection();

            webCamTimer = new Timer();
            webCamTimer.Tick += webCamTimer_Tick;
            webCamTimer.Interval = 500;
            webCamTimer.Start();
         }
         else
         {
            webCamTimer.Stop();
            webCamTimer = null;
            wCam.Dispose();
            wCam = null;
         }
      }

      void webCamTimer_Tick(object sender, EventArgs e)
      {
         var bitmap = wCam.GetCurrentImage();
         if (bitmap == null)
            return;
         var imageSource = new RGBLuminanceSource(bitmap, bitmap.Width, bitmap.Height);
         var binarizer = new HybridBinarizer(imageSource);
         var binaryBitmap = new BinaryBitmap(binarizer);
         var reader = new MultiFormatReader();
         var result = reader.decode(binaryBitmap);
         if (result != null)
         {
            txtTypeWebCam.Text = result.BarcodeFormat.ToString();
            txtContentWebCam.Text = result.Text;
         }
      }

      private void btnEncode_Click(object sender, EventArgs e)
      {
         try
         {
            var encoder = new MultiFormatWriter();
            var bitMatrix = encoder.encode(txtEncoderContent.Text, (BarcodeFormat)cmbEncoderType.SelectedItem, picEncodedBarCode.Width, picEncodedBarCode.Height);
            picEncodedBarCode.Image = bitMatrix.ToBitmap();
         }
         catch (Exception exc)
         {
            MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }
   }
}
