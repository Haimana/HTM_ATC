using System;
using System.IO;
using System.Windows.Forms;
using Plugins.Core;
using Plugins.Forms;
using Plugins.Persistence;

namespace Plugins
{
    public class UCCNCplugin
    {
        private bool _firstRun = true;
        public Plugininterface.Entry UC;
        private MainForm _mainForm;
        private ConfigStore _configStore;
        
        public bool LoopStop { get; private set; }
        public bool LoopWorking { get; private set; }

        public UCCNCplugin()
        {
        }

        public void Init_event(Plugininterface.Entry UC)
        {
            this.UC = UC;
            
            string uccncPath = Application.StartupPath;
            string profileName = GetCurrentProfileName();
            
            _configStore = new ConfigStore(uccncPath, profileName);
            _configStore.Load();
            
            _mainForm = new MainForm(this, UC, _configStore);
        }

        public Plugininterface.Entry.Pluginproperties Getproperties_event(Plugininterface.Entry.Pluginproperties Properties)
        {
            Properties.author = "HTM_ATC Project";
            Properties.pluginname = "HTM_ATC";
            Properties.pluginversion = "1.0.0";
            return Properties;
        }

        public void Configure_event()
        {
            using (var wizard = new SetupWizard(_configStore, UC))
            {
                wizard.ShowDialog(_mainForm);
            }
        }

        public void Startup_event()
        {
            if (_mainForm == null || _mainForm.IsDisposed)
            {
                _mainForm = new MainForm(this, UC, _configStore);
            }
            _mainForm.Show();
        }

        public void Showup_event()
        {
            if (_mainForm == null || _mainForm.IsDisposed)
            {
                _mainForm = new MainForm(this, UC, _configStore);
            }
            _mainForm.Show();
            _mainForm.BringToFront();
        }

        public void Shutdown_event()
        {
            try
            {
                LoopStop = true;
                if (_mainForm != null && !_mainForm.IsDisposed)
                {
                    _mainForm.Close();
                }
            }
            catch { }
        }

        public void Loop_event()
        {
            if (LoopStop)
                return;

            LoopWorking = true;

            if (_mainForm == null || _mainForm.IsDisposed)
            {
                LoopWorking = false;
                return;
            }

            if (_firstRun)
            {
                _firstRun = false;
            }

            try
            {
                _mainForm.UpdateLoop();
            }
            catch { }

            LoopWorking = false;
        }

        public object Informplugin_event(object Message)
        {
            if (Message is string)
            {
                string msg = Message as string;
                
                if (msg.StartsWith("M6:"))
                {
                    string toolStr = msg.Substring(3);
                    if (int.TryParse(toolStr, out int newTool))
                    {
                        if (_mainForm != null && !_mainForm.IsDisposed)
                        {
                            string error = _mainForm.ExecuteToolChange(newTool);
                            return error;
                        }
                        return "Plugin form not available";
                    }
                    return "Invalid tool number";
                }
            }
            
            return null;
        }

        public void Informplugins_event(object Message)
        {
        }

        public void Buttonpress_event(int buttonnumber, bool onscreen)
        {
        }

        public void Toolpathclick_event(double X, double Y, bool Istopview)
        {
        }

        public void Textfieldclick_event(int labelnumber, bool Ismainscreen)
        {
        }

        public void Textfieldtexttyped_event(int labelnumber, bool Ismainscreen, string text)
        {
        }

        public void Imageviewclick_event(MouseEventArgs e, int labelnumber, bool Ismainscreen)
        {
        }

        public void Cyclethreadstart_event()
        {
        }

        public void StopLoop()
        {
            LoopStop = true;
            while (LoopWorking)
            {
                System.Threading.Thread.Sleep(10);
            }
        }

        private string GetCurrentProfileName()
        {
            try
            {
                string profilesPath = Path.Combine(Application.StartupPath, "Profiles");
                if (Directory.Exists(profilesPath))
                {
                    var proFiles = Directory.GetFiles(profilesPath, "*.pro");
                    if (proFiles.Length > 0)
                    {
                        return Path.GetFileNameWithoutExtension(proFiles[0]);
                    }
                }
            }
            catch { }
            
            return "Default";
        }
    }
}
