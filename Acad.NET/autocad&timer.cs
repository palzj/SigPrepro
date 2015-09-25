using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System.Drawing;
using System.Windows.Forms;
using System;
 
namespace GetAttention
{
  public class HelpForm : Form
  {
    // Percentage of screen height for border
 
    const int bdrPrc = 20;
 
    string _message = "";
    Brush _brush = null;
 
    public HelpForm(Brush brush, string message)
    {
      _brush = brush;
      _message = message;
 
      TopMost = true;
      ShowInTaskbar = false;
      FormBorderStyle = FormBorderStyle.None;
      BackColor = Color.Plum;
      TransparencyKey = Color.Plum;
      Width = Screen.PrimaryScreen.Bounds.Width;
      Height = Screen.PrimaryScreen.Bounds.Height;
 
      Paint += new PaintEventHandler(HelpForm_Paint);
    }
 
    void HelpForm_Paint(object sender, PaintEventArgs e)
    {
      // Calculate the actual border size in pixels
 
      int bdrWid = Height * bdrPrc / 100;
 
      // Our border will be around the whole screen
 
      Rectangle border = new Rectangle(0, 0, Width, Height);
 
      // Draw the border
 
      e.Graphics.DrawRectangle(new Pen(_brush, bdrWid), border);
 
      // Our text will be centered in the border
 
      System.Drawing.Font f = new Font("Arial", bdrWid);
 
      SizeF sz = e.Graphics.MeasureString(_message, f);
      int wid = (int)sz.Width;
      int hgt = (int)sz.Height;
      Rectangle rect =
        new Rectangle(
          (Width - wid) / 2, (Height - hgt) / 2,
          (int)(wid * 1.2), hgt
        );
      e.Graphics.DrawString(_message, f, _brush, rect);
    }
  }
 
  public class MessageFlasher
  {
    private static HelpForm _form = null;
    private static Timer _timer = null;
    private static int _times = 0;
    private static int _maxTimes = 0;
 
    public static void FlashMessage(
      Brush brush, string message, int times, double secs
    )
    {
      // Create our form
 
      _form = new HelpForm(brush, message);
      _form.Show();
 
      // Start the timer, ticking as per the specified interval
 
      _maxTimes = (times * 2) - 1;
      _timer = new Timer()
      {
        Interval = (int)(secs * 1000),
        Enabled = true
      };
 
      _timer.Tick +=
        (s, e) =>
        {
          // Once the timer has ticked n times (and the form
          // displayed n/2 times), dispose of the form
 
          if (_times++ >= _maxTimes)
          {
            _form.Hide();
            _form.Dispose();
            _form = null;
 
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
 
            _times = 0;
          }
          else
          {
            // If we haven't reached ten ticks, toggle the form's
            // display on/off
 
            if (_form.Visible)
              _form.Hide();
            else
              _form.Show();
          }
        };
    }
 
    public class Commands
    {
      [CommandMethod("HELPME")]
      public void RequestHelp()
      {
        MessageFlasher.FlashMessage(Brushes.Red, "Help!", 5, 1);
      }
 
      [CommandMethod("HELPYOU")]
      public void ProvideHelp()
      {
        MessageFlasher.FlashMessage(Brushes.Green, "Answer!", 2, 1);
      }
    }
  }
}