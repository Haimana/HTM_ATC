using System;
using System.IO;
using System.Windows.Forms;
using HTM_PRO.Core;
using HTM_PRO.UI;
using HTM_PRO.Utils;

namespace Plugins
{
    /// <summary>
    /// Unified HTM_PRO UCCNC plugin entry point.
    /// Hosts Stage 1 (Complete Machine Setup Wizard) and will host Stage 2 (ATC Setup & Runtime).
    /// </summary>
    public class UCCNCplugin
    {
        public Plugininterface.Entry UC;

        private string _startupPath;
        private string _profileName;
        private ProfileService _profileService;
        private ScreenInjector _screenInjector;
        private MachineSetupWizardForm _wizardForm;

        private volatile bool _startupCompleted = false;
        private bool _firstLoopPass = true;
        private bool _autoOpenWizardPending = false;
        private bool _loopStop = false;

        public UCCNCplugin()
        {
        }

        public void Init_event(Plugininterface.Entry UC)
        {
            this.UC = UC;
            try
            {
                _startupPath = Application.StartupPath;
                _profileName = ResolveProfileName();
                string profilesDir = Path.Combine(_startupPath, "Profiles");

                PluginLogger.Initialize(profilesDir, _profileName);
                _profileService = new ProfileService(_startupPath, _profileName, UC);
                _screenInjector = new ScreenInjector(UC, _startupPath, ShowSetupWizard);
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("Init_event", ex);
            }
        }

        public Plugininterface.Entry.Pluginproperties Getproperties_event(Plugininterface.Entry.Pluginproperties Properties)
        {
            Properties.author = "HTM_PRO Project";
            Properties.pluginname = "HTM_PRO";
            Properties.pluginversion = PluginVersion.Current;
            return Properties;
        }

        public void Configure_event()
        {
            PluginLogger.Log("Configure_event called");
            ShowSetupWizard();
        }

        public void Startup_event()
        {
            PluginLogger.Log("Startup_event called");
            try
            {
                Form mainForm = _screenInjector?.GetMainForm();
                _profileService?.Load(mainForm);
                _screenInjector?.InjectConfigurationButton();

                if (_profileService != null && !_profileService.Config.Stage1Completed)
                {
                    PluginLogger.Log("Stage1Completed=False -> Scheduling automatic launch of Machine Setup Wizard.");
                    _autoOpenWizardPending = true;
                }

                _startupCompleted = true;
            }
            catch (Exception ex)
            {
                _startupCompleted = true;
                PluginLogger.LogException("Startup_event", ex);
            }
        }

        public void Showup_event()
        {
            PluginLogger.Log("Showup_event called");
            ShowSetupWizard();
        }

        public void Shutdown_event()
        {
            PluginLogger.Log("Shutdown_event called");
            _loopStop = true;
            try
            {
                if (_wizardForm != null && !_wizardForm.IsDisposed)
                {
                    _wizardForm.Close();
                }
            }
            catch { }
        }

        public void Loop_event()
        {
            if (_loopStop || !_startupCompleted) return;

            if (_firstLoopPass)
            {
                _firstLoopPass = false;
                _screenInjector?.InjectConfigurationButton();

                if (_autoOpenWizardPending)
                {
                    _autoOpenWizardPending = false;
                    Form mainForm = _screenInjector?.GetMainForm();
                    if (mainForm != null && !mainForm.IsDisposed)
                    {
                        mainForm.BeginInvoke(new Action(ShowSetupWizard));
                    }
                    else
                    {
                        ShowSetupWizard();
                    }
                }
            }
        }

        public object Informplugin_event(object Message) => null;
        public void Informplugins_event(object Message) { }

        public void Buttonpress_event(int buttonnumber, bool onscreen)
        {
            if (onscreen)
            {
                _screenInjector?.HandleButtonPress(buttonnumber);
            }
        }

        public void Toolpathclick_event(double X, double Y, bool Istopview) { }
        public void Textfieldclick_event(int labelnumber, bool Ismainscreen) { }
        public void Textfieldtexttyped_event(int labelnumber, bool Ismainscreen, string text) { }
        public void Imageviewclick_event(MouseEventArgs e, int labelnumber, bool Ismainscreen) { }
        public void Cyclethreadstart_event() { }

        private void ShowSetupWizard()
        {
            try
            {
                Form mainForm = _screenInjector?.GetMainForm();
                if (mainForm != null && mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(ShowSetupWizard));
                    return;
                }

                if (_profileService == null) return;

                if (_wizardForm == null || _wizardForm.IsDisposed)
                {
                    _wizardForm = new MachineSetupWizardForm(_profileService, mainForm);
                    _wizardForm.Show(mainForm);
                }
                else
                {
                    if (_wizardForm.WindowState == FormWindowState.Minimized)
                    {
                        _wizardForm.WindowState = FormWindowState.Normal;
                    }
                    _wizardForm.BringToFront();
                    _wizardForm.Activate();
                }
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("ShowSetupWizard", ex);
            }
        }

        private string ResolveProfileName()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.Equals(args[i], "/p", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    {
                        return args[i + 1].Trim();
                    }
                }
            }
            catch { }

            try
            {
                string name = UC?.Getprofilename();
                if (!string.IsNullOrEmpty(name))
                {
                    return Path.GetFileNameWithoutExtension(name);
                }
            }
            catch { }

            return "Default";
        }
    }
}
