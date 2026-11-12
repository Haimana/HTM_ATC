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
        private string _profileName;
        
        public bool LoopStop { get; private set; }
        public bool LoopWorking { get; private set; }
        
        private volatile bool _loopBusy;
        private readonly object _loopLock = new object();

        public UCCNCplugin()
        {
        }

        public void Init_event(Plugininterface.Entry UC)
        {
            this.UC = UC;
            
            try
            {
                string uccncPath = Application.StartupPath;
                _profileName = GetCurrentProfileName();
                
                string profilesFolder = Path.Combine(uccncPath, "Profiles");
                PluginLogger.Initialize(profilesFolder, _profileName);
                PluginLogger.Log($"Init_event: profile={_profileName}");
                
                _configStore = new ConfigStore(uccncPath, _profileName);
                _configStore.Load();
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("Init_event", ex);
            }
        }

        public Plugininterface.Entry.Pluginproperties Getproperties_event(Plugininterface.Entry.Pluginproperties Properties)
        {
            Properties.author = "HTM_ATC Project";
            Properties.pluginname = "HTM_ATC";
            Properties.pluginversion = "1.0.1";
            return Properties;
        }

        public void Configure_event()
        {
            try
            {
                EnsureMainForm();
                using (var wizard = new SetupWizard(_configStore, UC))
                {
                    wizard.ShowDialog(_mainForm);
                }
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("Configure_event", ex);
            }
        }

        public void Startup_event()
        {
            try
            {
                PluginLogger.Log("Startup_event");
                EnsureMainForm();
                _mainForm.Show();
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("Startup_event", ex);
            }
        }

        public void Showup_event()
        {
            try
            {
                PluginLogger.Log("Showup_event");
                EnsureMainForm();
                _mainForm.Show();
                _mainForm.BringToFront();
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("Showup_event", ex);
            }
        }

        private void EnsureMainForm()
        {
            if (_mainForm == null || _mainForm.IsDisposed)
            {
                _mainForm = new MainForm(this, UC, _configStore);
                PluginLogger.LogFormOpen("MainForm");
            }
        }

        public void Shutdown_event()
        {
            try
            {
                PluginLogger.Log("Shutdown_event");
                LoopStop = true;
                if (_mainForm != null && !_mainForm.IsDisposed)
                {
                    _mainForm.ForceClose();
                    PluginLogger.LogFormClose("MainForm");
                }
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("Shutdown_event", ex);
            }
        }

        public void Loop_event()
        {
            if (LoopStop)
                return;

            if (_loopBusy)
                return;

            lock (_loopLock)
            {
                if (_loopBusy)
                    return;
                _loopBusy = true;
            }

            LoopWorking = true;

            try
            {
                if (_mainForm == null || _mainForm.IsDisposed)
                {
                    LoopWorking = false;
                    _loopBusy = false;
                    return;
                }

                if (_firstRun)
                {
                    _firstRun = false;
                    PluginLogger.Log("First Loop_event cycle");
                }

                _mainForm.RequestUIUpdate();
            }
            catch (Exception ex)
            {
                PluginLogger.LogLoopException(ex);
            }
            finally
            {
                LoopWorking = false;
                _loopBusy = false;
            }
        }

        public object Informplugin_event(object Message)
        {
            try
            {
                if (Message is string)
                {
                    string msg = Message as string;
                    
                    if (msg.StartsWith("M6:"))
                    {
                        PluginLogger.Log($"Informplugin_event: {msg}");
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
            }
            catch (Exception ex)
            {
                PluginLogger.LogException("Informplugin_event", ex);
                return ex.Message;
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
            int timeout = 1000;
            int elapsed = 0;
            while (LoopWorking && elapsed < timeout)
            {
                System.Threading.Thread.Sleep(10);
                elapsed += 10;
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
