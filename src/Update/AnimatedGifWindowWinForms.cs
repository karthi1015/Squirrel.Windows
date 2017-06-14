﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Squirrel.Update
{
    public class AnimatedGifWindowWinForms : Form
    {
        PictureBox pictureBox;

        AnimatedGifWindowWinForms()
        {
            var source = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "background.gif");

            pictureBox = new PictureBox();
            this.Controls.Add(pictureBox);

            if (File.Exists(source)) {
                pictureBox.ImageLocation = source;
            }

            this.WindowState = FormWindowState.Minimized;
            Action size = () => { pictureBox.Width = this.Width; pictureBox.Height = this.Height; pictureBox.Left = 0; pictureBox.Top = 0; };
            pictureBox.LoadCompleted += (o, e) => {
                if (pictureBox.Image == null) return;
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

                this.SizeChanged += (_o, _e) => size();

                this.Width = pictureBox.Image.Width / 2;
                this.Height = pictureBox.Image.Height / 2;
                this.CenterToScreen();
            };

            this.FormBorderStyle = FormBorderStyle.None;
            this.Width = 1;
            this.Height = 1;
        }

        public static void ShowWindow(TimeSpan initialDelay, CancellationToken token, ProgressSource progressSource)
        {
            var thread = new Thread(() => {
                if (token.IsCancellationRequested) return;

                try {
                    Task.Delay(initialDelay, token).ContinueWith(_ => { return true; }).Wait();
                } catch (Exception) {
                    // NB: Cancellation will end up here, so we'll bail out
                    return;
                }

                var wnd = new AnimatedGifWindowWinForms();
                wnd.Show();

                token.Register(() => wnd.Invoke(new Action(() => wnd.Close())));

                var t = new System.Windows.Forms.Timer();
                t.Tick += (o, e) => {
                    wnd.WindowState = FormWindowState.Normal;
                    t.Stop();
                };

                t.Interval = 400;
                t.Start();

                Application.Run(wnd);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
