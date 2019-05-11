/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FSO.Client;
using FSO.Client.UI.Panels;
using FSO.Common.Rendering.Framework.IO;

namespace FSO.Windows
{

    public static class Program
    {

        public static bool UseDX = true;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        public static void Main(string[] args)
        {
            InitWindows();
            if (new FSOProgram().InitWithArguments(args))
                new GameStartProxy().Start(UseDX);
        }

        public static void InitWindows()
        {
            //initialize some platform specific stuff
            Files.ImageLoaderHelpers.BitmapFunction = BitmapReader;
            ClipboardHandler.Default = new WinFormsClipboard();

            var os = Environment.OSVersion;
            var pid = os.Platform;
            bool linux = pid == PlatformID.MacOSX || pid == PlatformID.Unix;
            if (!linux)
                ITTSContext.Provider = UITTSContext.PlatformProvider;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            FSOProgram.ShowDialog = ShowDialog;

        }

        public static void ShowDialog(string text)
        {
            MessageBox.Show(text);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject;
            if (exception is OutOfMemoryException)
            {
                MessageBox.Show(e.ExceptionObject.ToString(), "Out of Memory! FreeSO needs to close.");
            }
            else
            {
                MessageBox.Show(e.ExceptionObject.ToString(), "A fatal error occured! Screenshot this dialog and post it on Discord.");
            }
        }

        public static Tuple<byte[], int, int> BitmapReader(Stream str)
        {
            var image = (Bitmap)Image.FromStream(str);
            try
            {
                // Fix up the Image to match the expected format
                //image = (Bitmap)image.RGBToBGR();

                var data = new byte[image.Width * image.Height * 4];

                var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                if (bitmapData.Stride != image.Width * 4)
                    throw new NotImplementedException();
                Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);
                image.UnlockBits(bitmapData);

                for (int i = 0; i < data.Length; i += 4)
                {
                    var temp = data[i];
                    data[i] = data[i + 2];
                    data[i + 2] = temp;
                }

                return new Tuple<byte[], int, int>(data, image.Width, image.Height);
            }
            finally
            {
                image.Dispose();
            }
        }

        // RGB to BGR convert Matrix
        private static float[][] rgbtobgr = new float[][]
          {
             new float[] {0, 0, 1, 0, 0},
             new float[] {0, 1, 0, 0, 0},
             new float[] {1, 0, 0, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
          };


        internal static Image RGBToBGR(this Image bmp)
        {
            Image newBmp;
            if ((bmp.PixelFormat & PixelFormat.Indexed) != 0)
            {
                newBmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
            }
            else
            {
                // Need to clone so the call to Clear() below doesn't clear the source before trying to draw it to the target.
                newBmp = (Image)bmp.Clone();
            }

            try
            {
                var ia = new ImageAttributes();
                var cm = new ColorMatrix(rgbtobgr);

                ia.SetColorMatrix(cm);
                using (var g = Graphics.FromImage(newBmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia);
                }
            }
            finally
            {
                if (newBmp != bmp)
                {
                    bmp.Dispose();
                }
            }

            return newBmp;
        }
    }
}
