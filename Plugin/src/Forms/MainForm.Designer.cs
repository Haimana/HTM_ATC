namespace Plugins.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabStatus = new System.Windows.Forms.TabPage();
            this.tabToolRack = new System.Windows.Forms.TabPage();
            this.tabSetup = new System.Windows.Forms.TabPage();
            this.tabDiagnostics = new System.Windows.Forms.TabPage();
            this.tabSimulation = new System.Windows.Forms.TabPage();
            
            this.lblCurrentToolLabel = new System.Windows.Forms.Label();
            this.lblCurrentTool = new System.Windows.Forms.Label();
            this.ledHomed = new System.Windows.Forms.Panel();
            this.lblHomed = new System.Windows.Forms.Label();
            this.ledSafety = new System.Windows.Forms.Panel();
            this.lblSafety = new System.Windows.Forms.Label();
            this.btnGoToTCP = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            
            this.panelSlots = new System.Windows.Forms.Panel();
            this.slotPanels = new System.Windows.Forms.Panel[10];
            
            this.btnSetupWizard = new System.Windows.Forms.Button();
            this.btnImportProfile = new System.Windows.Forms.Button();
            
            this.lblMachX = new System.Windows.Forms.Label();
            this.lblMachY = new System.Windows.Forms.Label();
            this.lblMachZ = new System.Windows.Forms.Label();
            this.ledDrawbar = new System.Windows.Forms.Panel();
            this.lblDrawbar = new System.Windows.Forms.Label();
            this.ledAirBlow = new System.Windows.Forms.Panel();
            this.lblAirBlow = new System.Windows.Forms.Label();
            this.ledPressure = new System.Windows.Forms.Panel();
            this.lblPressure = new System.Windows.Forms.Label();
            this.btnTestDrawbar = new System.Windows.Forms.Button();
            this.btnTestAirBlow = new System.Windows.Forms.Button();
            
            this.chkSimulation = new System.Windows.Forms.CheckBox();
            this.lblSimSlots = new System.Windows.Forms.Label();
            this.chkSimSlots = new System.Windows.Forms.CheckBox[10];
            this.chkSimPressureOK = new System.Windows.Forms.CheckBox();
            this.chkSimOssdOK = new System.Windows.Forms.CheckBox();
            this.btnRunTestCycle = new System.Windows.Forms.Button();

            this.tabControl.SuspendLayout();
            this.tabStatus.SuspendLayout();
            this.tabToolRack.SuspendLayout();
            this.tabSetup.SuspendLayout();
            this.tabDiagnostics.SuspendLayout();
            this.tabSimulation.SuspendLayout();
            this.SuspendLayout();

            // tabControl
            this.tabControl.Controls.Add(this.tabStatus);
            this.tabControl.Controls.Add(this.tabToolRack);
            this.tabControl.Controls.Add(this.tabSetup);
            this.tabControl.Controls.Add(this.tabDiagnostics);
            this.tabControl.Controls.Add(this.tabSimulation);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(800, 500);

            // tabStatus
            this.tabStatus.Text = "Status";
            this.tabStatus.Padding = new System.Windows.Forms.Padding(10);
            
            // Current Tool Display
            this.lblCurrentToolLabel.Text = "Current Tool:";
            this.lblCurrentToolLabel.Location = new System.Drawing.Point(20, 20);
            this.lblCurrentToolLabel.Size = new System.Drawing.Size(100, 30);
            this.lblCurrentToolLabel.Font = new System.Drawing.Font("Arial", 14F);
            this.tabStatus.Controls.Add(this.lblCurrentToolLabel);

            this.lblCurrentTool.Text = "---";
            this.lblCurrentTool.Location = new System.Drawing.Point(130, 10);
            this.lblCurrentTool.Size = new System.Drawing.Size(100, 50);
            this.lblCurrentTool.Font = new System.Drawing.Font("Arial", 28F, System.Drawing.FontStyle.Bold);
            this.lblCurrentTool.ForeColor = System.Drawing.Color.Gray;
            this.tabStatus.Controls.Add(this.lblCurrentTool);

            // Status LEDs
            this.lblHomed.Text = "Homed:";
            this.lblHomed.Location = new System.Drawing.Point(300, 20);
            this.lblHomed.Size = new System.Drawing.Size(60, 20);
            this.tabStatus.Controls.Add(this.lblHomed);

            this.ledHomed.Location = new System.Drawing.Point(365, 18);
            this.ledHomed.Size = new System.Drawing.Size(24, 24);
            this.ledHomed.BackColor = System.Drawing.Color.Gray;
            this.tabStatus.Controls.Add(this.ledHomed);

            this.lblSafety.Text = "Safety:";
            this.lblSafety.Location = new System.Drawing.Point(410, 20);
            this.lblSafety.Size = new System.Drawing.Size(50, 20);
            this.tabStatus.Controls.Add(this.lblSafety);

            this.ledSafety.Location = new System.Drawing.Point(465, 18);
            this.ledSafety.Size = new System.Drawing.Size(24, 24);
            this.ledSafety.BackColor = System.Drawing.Color.Gray;
            this.tabStatus.Controls.Add(this.ledSafety);

            // Go to TCP button
            this.btnGoToTCP.Text = "Go to TCP";
            this.btnGoToTCP.Location = new System.Drawing.Point(550, 15);
            this.btnGoToTCP.Size = new System.Drawing.Size(100, 35);
            this.btnGoToTCP.Click += new System.EventHandler(this.btnGoToTCP_Click);
            this.tabStatus.Controls.Add(this.btnGoToTCP);

            // Log area
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.ReadOnly = true;
            this.txtLog.Location = new System.Drawing.Point(20, 70);
            this.txtLog.Size = new System.Drawing.Size(740, 380);
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.tabStatus.Controls.Add(this.txtLog);

            // tabToolRack
            this.tabToolRack.Text = "Tool Rack";
            this.tabToolRack.Padding = new System.Windows.Forms.Padding(10);
            
            this.panelSlots.Location = new System.Drawing.Point(10, 50);
            this.panelSlots.Size = new System.Drawing.Size(760, 400);
            this.tabToolRack.Controls.Add(this.panelSlots);

            // Tool Rack buttons
            this.btnRemoveToolFromSpindle = new System.Windows.Forms.Button();
            this.btnRemoveToolFromSpindle.Text = "Remove Tool from Spindle";
            this.btnRemoveToolFromSpindle.Location = new System.Drawing.Point(10, 10);
            this.btnRemoveToolFromSpindle.Size = new System.Drawing.Size(180, 30);
            this.btnRemoveToolFromSpindle.Click += new System.EventHandler(this.btnRemoveToolFromSpindle_Click);
            this.tabToolRack.Controls.Add(this.btnRemoveToolFromSpindle);

            this.btnEditTool = new System.Windows.Forms.Button();
            this.btnEditTool.Text = "Edit Tool...";
            this.btnEditTool.Location = new System.Drawing.Point(200, 10);
            this.btnEditTool.Size = new System.Drawing.Size(100, 30);
            this.btnEditTool.Click += new System.EventHandler(this.btnEditTool_Click);
            this.tabToolRack.Controls.Add(this.btnEditTool);

            var lblClickHint = new System.Windows.Forms.Label();
            lblClickHint.Text = "Click a slot to assign a tool";
            lblClickHint.Location = new System.Drawing.Point(320, 15);
            lblClickHint.Size = new System.Drawing.Size(200, 20);
            lblClickHint.ForeColor = System.Drawing.Color.Gray;
            this.tabToolRack.Controls.Add(lblClickHint);

            for (int i = 0; i < 10; i++)
            {
                var panel = new System.Windows.Forms.Panel();
                panel.Location = new System.Drawing.Point((i % 5) * 150 + 5, (i / 5) * 190 + 5);
                panel.Size = new System.Drawing.Size(140, 180);
                panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                panel.Cursor = System.Windows.Forms.Cursors.Hand;
                panel.Click += new System.EventHandler(this.SlotPanel_Click);
                
                var lblSlot = new System.Windows.Forms.Label();
                lblSlot.Text = $"Slot {i + 1}";
                lblSlot.Location = new System.Drawing.Point(5, 5);
                lblSlot.Size = new System.Drawing.Size(60, 20);
                lblSlot.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
                panel.Controls.Add(lblSlot);

                var ledEnabled = new System.Windows.Forms.Panel();
                ledEnabled.Name = "ledEnabled";
                ledEnabled.Location = new System.Drawing.Point(110, 5);
                ledEnabled.Size = new System.Drawing.Size(20, 20);
                ledEnabled.BackColor = System.Drawing.Color.Gray;
                panel.Controls.Add(ledEnabled);

                var lblTool = new System.Windows.Forms.Label();
                lblTool.Name = "lblTool";
                lblTool.Text = "-";
                lblTool.Location = new System.Drawing.Point(5, 28);
                lblTool.Size = new System.Drawing.Size(130, 45);
                lblTool.Font = new System.Drawing.Font("Arial", 26F, System.Drawing.FontStyle.Bold);
                lblTool.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                panel.Controls.Add(lblTool);

                var lblToolInfo = new System.Windows.Forms.Label();
                lblToolInfo.Name = "lblToolInfo";
                lblToolInfo.Text = "";
                lblToolInfo.Location = new System.Drawing.Point(5, 75);
                lblToolInfo.Size = new System.Drawing.Size(130, 18);
                lblToolInfo.Font = new System.Drawing.Font("Arial", 8F);
                lblToolInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                lblToolInfo.ForeColor = System.Drawing.Color.DarkBlue;
                panel.Controls.Add(lblToolInfo);

                var lblSensor = new System.Windows.Forms.Label();
                lblSensor.Text = "Sensor:";
                lblSensor.Location = new System.Drawing.Point(5, 100);
                lblSensor.Size = new System.Drawing.Size(50, 20);
                panel.Controls.Add(lblSensor);

                var ledSensor = new System.Windows.Forms.Panel();
                ledSensor.Name = "ledSensor";
                ledSensor.Location = new System.Drawing.Point(60, 98);
                ledSensor.Size = new System.Drawing.Size(24, 24);
                ledSensor.BackColor = System.Drawing.Color.Gray;
                panel.Controls.Add(ledSensor);

                var lblActive = new System.Windows.Forms.Label();
                lblActive.Text = "Active:";
                lblActive.Location = new System.Drawing.Point(5, 130);
                lblActive.Size = new System.Drawing.Size(50, 20);
                panel.Controls.Add(lblActive);

                var ledActive = new System.Windows.Forms.Panel();
                ledActive.Name = "ledActive";
                ledActive.Location = new System.Drawing.Point(60, 128);
                ledActive.Size = new System.Drawing.Size(24, 24);
                ledActive.BackColor = System.Drawing.Color.DarkGray;
                panel.Controls.Add(ledActive);

                slotPanels[i] = panel;
                this.panelSlots.Controls.Add(panel);
            }

            // tabSetup
            this.tabSetup.Text = "Setup";
            this.tabSetup.Padding = new System.Windows.Forms.Padding(10);

            this.btnSetupWizard.Text = "Run Setup Wizard";
            this.btnSetupWizard.Location = new System.Drawing.Point(20, 20);
            this.btnSetupWizard.Size = new System.Drawing.Size(150, 40);
            this.btnSetupWizard.Font = new System.Drawing.Font("Arial", 10F);
            this.btnSetupWizard.Click += new System.EventHandler(this.btnSetupWizard_Click);
            this.tabSetup.Controls.Add(this.btnSetupWizard);

            this.btnImportProfile.Text = "Import from HTM_ATC Profile";
            this.btnImportProfile.Location = new System.Drawing.Point(190, 20);
            this.btnImportProfile.Size = new System.Drawing.Size(200, 40);
            this.tabSetup.Controls.Add(this.btnImportProfile);

            // tabDiagnostics
            this.tabDiagnostics.Text = "Diagnostics";
            this.tabDiagnostics.Padding = new System.Windows.Forms.Padding(10);

            // Position display
            var lblPosTitle = new System.Windows.Forms.Label();
            lblPosTitle.Text = "Machine Position:";
            lblPosTitle.Location = new System.Drawing.Point(20, 20);
            lblPosTitle.Size = new System.Drawing.Size(150, 25);
            lblPosTitle.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.tabDiagnostics.Controls.Add(lblPosTitle);

            this.lblMachX.Text = "X: ---";
            this.lblMachX.Location = new System.Drawing.Point(20, 50);
            this.lblMachX.Size = new System.Drawing.Size(200, 25);
            this.lblMachX.Font = new System.Drawing.Font("Consolas", 12F);
            this.tabDiagnostics.Controls.Add(this.lblMachX);

            this.lblMachY.Text = "Y: ---";
            this.lblMachY.Location = new System.Drawing.Point(20, 80);
            this.lblMachY.Size = new System.Drawing.Size(200, 25);
            this.lblMachY.Font = new System.Drawing.Font("Consolas", 12F);
            this.tabDiagnostics.Controls.Add(this.lblMachY);

            this.lblMachZ.Text = "Z: ---";
            this.lblMachZ.Location = new System.Drawing.Point(20, 110);
            this.lblMachZ.Size = new System.Drawing.Size(200, 25);
            this.lblMachZ.Font = new System.Drawing.Font("Consolas", 12F);
            this.tabDiagnostics.Controls.Add(this.lblMachZ);

            // Output status
            var lblOutputTitle = new System.Windows.Forms.Label();
            lblOutputTitle.Text = "Outputs:";
            lblOutputTitle.Location = new System.Drawing.Point(280, 20);
            lblOutputTitle.Size = new System.Drawing.Size(100, 25);
            lblOutputTitle.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.tabDiagnostics.Controls.Add(lblOutputTitle);

            this.lblDrawbar.Text = "Drawbar:";
            this.lblDrawbar.Location = new System.Drawing.Point(280, 50);
            this.lblDrawbar.Size = new System.Drawing.Size(70, 25);
            this.tabDiagnostics.Controls.Add(this.lblDrawbar);

            this.ledDrawbar.Location = new System.Drawing.Point(360, 50);
            this.ledDrawbar.Size = new System.Drawing.Size(24, 24);
            this.ledDrawbar.BackColor = System.Drawing.Color.Gray;
            this.tabDiagnostics.Controls.Add(this.ledDrawbar);

            this.btnTestDrawbar.Text = "Test";
            this.btnTestDrawbar.Location = new System.Drawing.Point(400, 48);
            this.btnTestDrawbar.Size = new System.Drawing.Size(60, 28);
            this.btnTestDrawbar.Click += new System.EventHandler(this.btnTestDrawbar_Click);
            this.tabDiagnostics.Controls.Add(this.btnTestDrawbar);

            this.lblAirBlow.Text = "Air Blow:";
            this.lblAirBlow.Location = new System.Drawing.Point(280, 85);
            this.lblAirBlow.Size = new System.Drawing.Size(70, 25);
            this.tabDiagnostics.Controls.Add(this.lblAirBlow);

            this.ledAirBlow.Location = new System.Drawing.Point(360, 85);
            this.ledAirBlow.Size = new System.Drawing.Size(24, 24);
            this.ledAirBlow.BackColor = System.Drawing.Color.Gray;
            this.tabDiagnostics.Controls.Add(this.ledAirBlow);

            this.btnTestAirBlow.Text = "Test";
            this.btnTestAirBlow.Location = new System.Drawing.Point(400, 83);
            this.btnTestAirBlow.Size = new System.Drawing.Size(60, 28);
            this.btnTestAirBlow.Click += new System.EventHandler(this.btnTestAirBlow_Click);
            this.tabDiagnostics.Controls.Add(this.btnTestAirBlow);

            // Input status
            var lblInputTitle = new System.Windows.Forms.Label();
            lblInputTitle.Text = "Inputs:";
            lblInputTitle.Location = new System.Drawing.Point(520, 20);
            lblInputTitle.Size = new System.Drawing.Size(100, 25);
            lblInputTitle.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.tabDiagnostics.Controls.Add(lblInputTitle);

            var lblPressureLabel = new System.Windows.Forms.Label();
            lblPressureLabel.Text = "Pressure:";
            lblPressureLabel.Location = new System.Drawing.Point(520, 50);
            lblPressureLabel.Size = new System.Drawing.Size(70, 25);
            this.tabDiagnostics.Controls.Add(lblPressureLabel);

            this.ledPressure.Location = new System.Drawing.Point(600, 50);
            this.ledPressure.Size = new System.Drawing.Size(24, 24);
            this.ledPressure.BackColor = System.Drawing.Color.Gray;
            this.tabDiagnostics.Controls.Add(this.ledPressure);

            this.lblPressure.Text = "?";
            this.lblPressure.Location = new System.Drawing.Point(635, 50);
            this.lblPressure.Size = new System.Drawing.Size(50, 25);
            this.tabDiagnostics.Controls.Add(this.lblPressure);

            // tabSimulation
            this.tabSimulation.Text = "Simulation";
            this.tabSimulation.Padding = new System.Windows.Forms.Padding(10);

            this.chkSimulation.Text = "Enable Simulation Mode";
            this.chkSimulation.Location = new System.Drawing.Point(20, 20);
            this.chkSimulation.Size = new System.Drawing.Size(200, 25);
            this.chkSimulation.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
            this.chkSimulation.CheckedChanged += new System.EventHandler(this.chkSimulation_CheckedChanged);
            this.tabSimulation.Controls.Add(this.chkSimulation);

            this.lblSimSlots.Text = "Simulated Slot States (checked = tool present):";
            this.lblSimSlots.Location = new System.Drawing.Point(20, 60);
            this.lblSimSlots.Size = new System.Drawing.Size(350, 25);
            this.tabSimulation.Controls.Add(this.lblSimSlots);

            for (int i = 0; i < 10; i++)
            {
                var chk = new System.Windows.Forms.CheckBox();
                chk.Text = $"Slot {i + 1}";
                chk.Location = new System.Drawing.Point(20 + (i % 5) * 100, 90 + (i / 5) * 30);
                chk.Size = new System.Drawing.Size(90, 25);
                chk.CheckedChanged += new System.EventHandler(this.chkSimSlot_CheckedChanged);
                chkSimSlots[i] = chk;
                this.tabSimulation.Controls.Add(chk);
            }

            this.chkSimPressureOK.Text = "Pressure OK";
            this.chkSimPressureOK.Location = new System.Drawing.Point(20, 160);
            this.chkSimPressureOK.Size = new System.Drawing.Size(120, 25);
            this.chkSimPressureOK.Checked = true;
            this.chkSimPressureOK.CheckedChanged += new System.EventHandler(this.chkSimPressureOK_CheckedChanged);
            this.tabSimulation.Controls.Add(this.chkSimPressureOK);

            this.chkSimOssdOK.Text = "OSSD OK (no faults)";
            this.chkSimOssdOK.Location = new System.Drawing.Point(160, 160);
            this.chkSimOssdOK.Size = new System.Drawing.Size(150, 25);
            this.chkSimOssdOK.Checked = true;
            this.chkSimOssdOK.CheckedChanged += new System.EventHandler(this.chkSimOssdOK_CheckedChanged);
            this.tabSimulation.Controls.Add(this.chkSimOssdOK);

            this.btnRunTestCycle.Text = "Run Test Tool Change Cycle";
            this.btnRunTestCycle.Location = new System.Drawing.Point(20, 210);
            this.btnRunTestCycle.Size = new System.Drawing.Size(200, 40);
            this.btnRunTestCycle.Click += new System.EventHandler(this.btnRunTestCycle_Click);
            this.tabSimulation.Controls.Add(this.btnRunTestCycle);

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HTM ATC - Automatic Tool Changer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);

            this.tabControl.ResumeLayout(false);
            this.tabStatus.ResumeLayout(false);
            this.tabToolRack.ResumeLayout(false);
            this.tabSetup.ResumeLayout(false);
            this.tabDiagnostics.ResumeLayout(false);
            this.tabSimulation.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabStatus;
        private System.Windows.Forms.TabPage tabToolRack;
        private System.Windows.Forms.TabPage tabSetup;
        private System.Windows.Forms.TabPage tabDiagnostics;
        private System.Windows.Forms.TabPage tabSimulation;
        
        private System.Windows.Forms.Label lblCurrentToolLabel;
        private System.Windows.Forms.Label lblCurrentTool;
        private System.Windows.Forms.Panel ledHomed;
        private System.Windows.Forms.Label lblHomed;
        private System.Windows.Forms.Panel ledSafety;
        private System.Windows.Forms.Label lblSafety;
        private System.Windows.Forms.Button btnGoToTCP;
        private System.Windows.Forms.TextBox txtLog;
        
        private System.Windows.Forms.Panel panelSlots;
        private System.Windows.Forms.Panel[] slotPanels;
        
        private System.Windows.Forms.Button btnSetupWizard;
        private System.Windows.Forms.Button btnImportProfile;
        
        private System.Windows.Forms.Label lblMachX;
        private System.Windows.Forms.Label lblMachY;
        private System.Windows.Forms.Label lblMachZ;
        private System.Windows.Forms.Panel ledDrawbar;
        private System.Windows.Forms.Label lblDrawbar;
        private System.Windows.Forms.Panel ledAirBlow;
        private System.Windows.Forms.Label lblAirBlow;
        private System.Windows.Forms.Panel ledPressure;
        private System.Windows.Forms.Label lblPressure;
        private System.Windows.Forms.Button btnTestDrawbar;
        private System.Windows.Forms.Button btnTestAirBlow;
        
        private System.Windows.Forms.CheckBox chkSimulation;
        private System.Windows.Forms.Label lblSimSlots;
        private System.Windows.Forms.CheckBox[] chkSimSlots;
        private System.Windows.Forms.CheckBox chkSimPressureOK;
        private System.Windows.Forms.CheckBox chkSimOssdOK;
        private System.Windows.Forms.Button btnRunTestCycle;
        
        private System.Windows.Forms.Button btnRemoveToolFromSpindle;
        private System.Windows.Forms.Button btnEditTool;
    }
}
