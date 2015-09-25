// N.Poleshchuk, 2010.
// Wform.cs
//
// http://poleshchuk.spb.ru/cad/2010/TrSplashCse.htm
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace Book16cs
{
    public class Wform16 : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components;
        public Wform16()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Wform16));

            // Dialog box parameters t
            his.Size = new System.Drawing.Size(370, 300);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.Aquamarine;
            this.Opacity = 1.0;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            // Font System.Drawing.FontFamily 
            Ff = new FontFamily("Arial");
            System.Drawing.Font Font = new System.Drawing.Font(Ff, 36, FontStyle.Bold);

            // Label in the window center 
            System.Windows.Forms.Label Txt1 = new Label();
            Txt1.Text = "Book16cs";
            Txt1.Location = new Point(60, 110);
            Txt1.AutoSize = true;
            Txt1.ForeColor = Color.Black;
            Txt1.Font = Font;
            this.Controls.Add(Txt1);

            // Add Click event handler for the form 
            this.Click += new System.EventHandler(this.Wform16_OnClick);
            // Timer creation 
            System.Windows.Forms.Timer tm = new System.Windows.Forms.Timer();
            tm.Interval = 100; // signal interval = 0.1 sec. 
            tm.Tick += new System.EventHandler(this.Wform16_OnTimerTick);
            tm.Enabled = true; //or tm.Start();
        }
        void Wform16_OnClick(object sender, System.EventArgs e)
        {
            // on click close window 
            this.Close();
        }
        // Reaction on timer signals 
        void Wform16_OnTimerTick(object sender, System.EventArgs ea)
        {
            System.Windows.Forms.Timer t1 = (System.Windows.Forms.Timer)sender;
            // Change window transparency in 0.1 sec. 
            this.Opacity -= 0.02;
            // Stop timer on window disappearance (Opacity = 0) 
            if (this.Opacity <= 0.0)
            {
                t1.Stop(); // Close window this.Close(); 
            }
        }
    }
}