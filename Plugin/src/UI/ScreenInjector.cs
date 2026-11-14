using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using HTM_PRO.Utils;

namespace HTM_PRO.UI
{
    /// <summary>
    /// Injects native OpenGL button & version label into UCCNC's CONFIGURATION tab bar (Layer 6)
    /// so the user can launch the HTM_PRO Machine Setup Wizard directly from UCCNC's main screen.
    /// Generates its own crisp button bitmaps at runtime to remain 100% self-contained.
    /// </summary>
    public class ScreenInjector
    {
        public const int WizardLaunchButtonId = 31000;
        private const int WizardButtonPictureId = 910;

        private readonly Plugininterface.Entry _uc;
        private readonly string _startupPath;
        private readonly Action _openWizardCallback;
        private bool _injected = false;

        public ScreenInjector(Plugininterface.Entry uc, string startupPath, Action openWizardCallback)
        {
            _uc = uc;
            _startupPath = startupPath;
            _openWizardCallback = openWizardCallback;
        }

        public Form GetMainForm()
        {
            try
            {
                if (_uc != null)
                {
                    FieldInfo execField = _uc.GetType().GetField("exec", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    object execObj = execField?.GetValue(_uc);
                    if (execObj != null)
                    {
                        FieldInfo mfField = execObj.GetType().GetField("mainform", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (mfField?.GetValue(execObj) is Form frm) return frm;
                    }
                }

                foreach (Form f in Application.OpenForms)
                {
                    if (f.GetType().Name == "Form1") return f;
                }
            }
            catch { }
            return null;
        }

        public void InjectConfigurationButton()
        {
            if (_injected || _uc == null) return;

            try
            {
                Form mainForm = GetMainForm();
                if (mainForm == null)
                {
                    PluginLogger.Log("InjectConfigurationButton: mainForm not ready yet.");
                    return;
                }

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(InjectConfigurationButton));
                    return;
                }

                EnsureWizardButtonBitmaps(out string relUp, out string relDown);

                FieldInfo as3Field = mainForm.GetType().GetField("AS3", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object as3Obj = as3Field?.GetValue(mainForm);
                if (as3Obj == null)
                {
                    PluginLogger.Log("InjectConfigurationButton: mainForm.AS3 not found.");
                    return;
                }

                MethodInfo loadPic = as3Obj.GetType().GetMethod(
                    "Loadpicture",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(string), typeof(string), typeof(int), typeof(bool) },
                    null);

                if (loadPic != null)
                {
                    loadPic.Invoke(as3Obj, new object[] { relUp, relDown, WizardButtonPictureId, false });
                }

                // Layer 6 is CONFIGURATION sidebar (sidebar items use x=38, width=154, height=40; y=359 is unoccupied)
                _uc.Addbutton(true, 38, 359, 154, 40, false, false, WizardButtonPictureId, WizardLaunchButtonId, 6);
                _uc.Addlabel(true, PluginVersion.FullTitle, "Arial", "left", 15, -256, 38, 412, 6);

                _injected = true;
                PluginLogger.Log($"Injected HTM_PRO Wizard button (#{WizardLaunchButtonId}) and version label on Layer 6.");
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("InjectConfigurationButton", ex);
            }
        }

        public void HandleButtonPress(int buttonNumber)
        {
            if (buttonNumber == WizardLaunchButtonId)
            {
                _openWizardCallback?.Invoke();
            }
        }

        private void EnsureWizardButtonBitmaps(out string relUp, out string relDown)
        {
            relUp = "BMP/HTM_PRO/wizard_up.png";
            relDown = "BMP/HTM_PRO/wizard_down.png";

            try
            {
                string bmpDir = Path.Combine(_startupPath, "Flashscreen", "BMP", "HTM_PRO");
                if (!Directory.Exists(bmpDir)) Directory.CreateDirectory(bmpDir);

                string upPath = Path.Combine(bmpDir, "wizard_up.png");
                string downPath = Path.Combine(bmpDir, "wizard_down.png");

                if (!File.Exists(upPath)) CreateButtonPng(upPath, pressed: false);
                if (!File.Exists(downPath)) CreateButtonPng(downPath, pressed: true);
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("EnsureWizardButtonBitmaps", ex);
            }
        }

        private static void CreateButtonPng(string path, bool pressed)
        {
            using var bmp = new Bitmap(154, 40);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color cTop = pressed ? Color.FromArgb(24, 52, 92) : Color.FromArgb(42, 74, 122);
            Color cBot = pressed ? Color.FromArgb(35, 68, 118) : Color.FromArgb(28, 44, 72);
            using (var br = new LinearGradientBrush(new Rectangle(0, 0, 154, 40), cTop, cBot, LinearGradientMode.Vertical))
            {
                g.FillRectangle(br, 0, 0, 154, 40);
            }

            using (var pen = new Pen(pressed ? Color.FromArgb(255, 210, 60) : Color.FromArgb(95, 165, 245), 2))
            {
                g.DrawRectangle(pen, 1, 1, 151, 37);
            }

            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var fnt = new Font("Arial", 10f, FontStyle.Bold);
            using var txtBr = new SolidBrush(pressed ? Color.FromArgb(255, 225, 90) : Color.White);
            g.DrawString("HTM_PRO WIZARD", fnt, txtBr, new RectangleF(0, 0, 154, 40), sf);

            bmp.Save(path, ImageFormat.Png);
        }
    }
}
