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
        public Plugininterface.Entry UC;
        private MainForm _mainForm;
        private ConfigStore _configStore;
        private ATCStateMachine _stateMachine;
        private string _profileName;
        
        public bool LoopStop { get; private set; }
        
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
                
                _stateMachine = new ATCStateMachine(_configStore.Config, UC);
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
            Properties.pluginversion = "1.0.2";
            return Properties;
        }

        public void Configure_event()
        {
            PluginLogger.Log("Configure_event");
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
            PluginLogger.Log("Startup_event");
            try
            {
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
            PluginLogger.Log("Showup_event");
            try
            {
                EnsureMainForm();
                
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.BeginInvoke(new Action(() =>
                    {
                        _mainForm.Show();
                        _mainForm.BringToFront();
                    }));
                }
                else
                {
                    _mainForm.Show();
                    _mainForm.BringToFront();
                }
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
                _mainForm = new MainForm(this, UC, _configStore, _stateMachine);
                PluginLogger.LogFormOpen("MainForm");
            }
        }

        public void Shutdown_event()
        {
            PluginLogger.Log("Shutdown_event");
            try
            {
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

            try
            {
                if (_mainForm == null || _mainForm.IsDisposed)
                {
                    _loopBusy = false;
                    return;
                }

                _mainForm.RequestUIUpdate();
            }
            catch (Exception ex)
            {
                PluginLogger.LogLoopException(ex);
            }
            finally
            {
                _loopBusy = false;
            }
        }

        public object Informplugin_event(object Message)
        {
            PluginLogger.Log($"Informplugin_event: {Message}");
            
            try
            {
                if (Message is string)
                {
                    string msg = Message as string;
                    
                    if (msg.StartsWith("M6:"))
                    {
                        string toolStr = msg.Substring(3);
                        if (int.TryParse(toolStr, out int newTool))
                        {
                            if (_stateMachine == null)
                            {
                                PluginLogger.Log("State machine not initialized");
                                return "State machine not initialized";
                            }
                            
                            PluginLogger.LogToolChange($"Starting tool change to T{newTool} on macro thread");
                            string error = _stateMachine.StartToolChange(newTool);
                            PluginLogger.LogToolChange($"Tool change completed: {(string.IsNullOrEmpty(error) ? "OK" : error)}");
                            
                            _configStore.Save();
                            
                            return error;
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
            PluginLogger.Log($"Buttonpress_event: button={buttonnumber}, onscreen={onscreen}");
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
            PluginLogger.Log("Cyclethreadstart_event");
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
        
        public ATCStateMachine GetStateMachine()
        {
            return _stateMachine;
        }
        
        public ConfigStore GetConfigStore()
        {
            return _configStore;
        }
    }
}
