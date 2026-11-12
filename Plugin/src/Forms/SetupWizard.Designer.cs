namespace Plugins.Forms
{
    partial class SetupWizard
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
            this.lblStepIndicator = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            
            this.panelWelcome = new System.Windows.Forms.Panel();
            this.panelRackOrientation = new System.Windows.Forms.Panel();
            this.panelSlot1Position = new System.Windows.Forms.Panel();
            this.panelSlotPitch = new System.Windows.Forms.Panel();
            this.panelZHeights = new System.Windows.Forms.Panel();
            this.panelDrawbar = new System.Windows.Forms.Panel();
            this.panelAirBlow = new System.Windows.Forms.Panel();
            this.panelDustHood = new System.Windows.Forms.Panel();
            this.panelPressureSensor = new System.Windows.Forms.Panel();
            this.panelSpindleToolSensor = new System.Windows.Forms.Panel();
            this.panelSlotSensors = new System.Windows.Forms.Panel();
            this.panelSafety = new System.Windows.Forms.Panel();
            this.panelProbe = new System.Windows.Forms.Panel();
            this.panelEnableSlots = new System.Windows.Forms.Panel();
            this.panelFeedsAndTiming = new System.Windows.Forms.Panel();
            this.panelOptions = new System.Windows.Forms.Panel();
            this.panelSummary = new System.Windows.Forms.Panel();
            
            this.SuspendLayout();

            this.lblStepIndicator.Location = new System.Drawing.Point(20, 15);
            this.lblStepIndicator.Size = new System.Drawing.Size(500, 25);
            this.lblStepIndicator.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.Controls.Add(this.lblStepIndicator);

            this.progressBar.Location = new System.Drawing.Point(20, 45);
            this.progressBar.Size = new System.Drawing.Size(560, 10);
            this.Controls.Add(this.progressBar);

            this.btnBack.Text = "< Back";
            this.btnBack.Location = new System.Drawing.Point(300, 420);
            this.btnBack.Size = new System.Drawing.Size(90, 35);
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            this.Controls.Add(this.btnBack);

            this.btnNext.Text = "Next >";
            this.btnNext.Location = new System.Drawing.Point(400, 420);
            this.btnNext.Size = new System.Drawing.Size(90, 35);
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            this.Controls.Add(this.btnNext);

            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new System.Drawing.Point(500, 420);
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.Controls.Add(this.btnCancel);

            var panelLocation = new System.Drawing.Point(20, 65);
            var panelSize = new System.Drawing.Size(560, 340);

            SetupPanelWelcome(panelLocation, panelSize);
            SetupPanelRackOrientation(panelLocation, panelSize);
            SetupPanelSlot1Position(panelLocation, panelSize);
            SetupPanelSlotPitch(panelLocation, panelSize);
            SetupPanelZHeights(panelLocation, panelSize);
            SetupPanelDrawbar(panelLocation, panelSize);
            SetupPanelAirBlow(panelLocation, panelSize);
            SetupPanelDustHood(panelLocation, panelSize);
            SetupPanelPressureSensor(panelLocation, panelSize);
            SetupPanelSpindleToolSensor(panelLocation, panelSize);
            SetupPanelSlotSensors(panelLocation, panelSize);
            SetupPanelSafety(panelLocation, panelSize);
            SetupPanelProbe(panelLocation, panelSize);
            SetupPanelEnableSlots(panelLocation, panelSize);
            SetupPanelFeedsAndTiming(panelLocation, panelSize);
            SetupPanelOptions(panelLocation, panelSize);
            SetupPanelSummary(panelLocation, panelSize);

            this.ClientSize = new System.Drawing.Size(610, 470);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "HTM ATC Setup Wizard";
            
            this.ResumeLayout(false);
        }

        private void SetupPanelWelcome(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelWelcome.Location = loc;
            panelWelcome.Size = size;
            panelWelcome.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Welcome to HTM ATC Setup\n\nThis wizard will guide you through configuring your Automatic Tool Changer.\n\nYou can import settings from an existing HTM_ATC profile or configure from scratch.";
            lbl.Location = new System.Drawing.Point(20, 20);
            lbl.Size = new System.Drawing.Size(500, 100);
            panelWelcome.Controls.Add(lbl);
            
            btnImportProfile = new System.Windows.Forms.Button();
            btnImportProfile.Text = "Import from HTM_ATC Profile...";
            btnImportProfile.Location = new System.Drawing.Point(20, 140);
            btnImportProfile.Size = new System.Drawing.Size(200, 30);
            btnImportProfile.Click += new System.EventHandler(this.btnImportProfile_Click);
            panelWelcome.Controls.Add(btnImportProfile);
            
            this.Controls.Add(panelWelcome);
        }

        private void SetupPanelRackOrientation(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelRackOrientation.Location = loc;
            panelRackOrientation.Size = size;
            panelRackOrientation.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Select the axis along which the tool rack is aligned:";
            lbl.Location = new System.Drawing.Point(20, 20);
            lbl.Size = new System.Drawing.Size(400, 25);
            panelRackOrientation.Controls.Add(lbl);
            
            rbAxisX = new System.Windows.Forms.RadioButton();
            rbAxisX.Text = "X Axis (rack extends along X, slots at different X positions)";
            rbAxisX.Location = new System.Drawing.Point(40, 60);
            rbAxisX.Size = new System.Drawing.Size(400, 25);
            rbAxisX.Checked = true;
            panelRackOrientation.Controls.Add(rbAxisX);
            
            rbAxisY = new System.Windows.Forms.RadioButton();
            rbAxisY.Text = "Y Axis (rack extends along Y, slots at different Y positions)";
            rbAxisY.Location = new System.Drawing.Point(40, 90);
            rbAxisY.Size = new System.Drawing.Size(400, 25);
            panelRackOrientation.Controls.Add(rbAxisY);
            
            this.Controls.Add(panelRackOrientation);
        }

        private void SetupPanelSlot1Position(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSlot1Position.Location = loc;
            panelSlot1Position.Size = size;
            panelSlot1Position.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Enter the machine coordinates (G53) of Slot 1 center.\nJog to slot 1 center and click 'Read Current Position'.";
            lbl.Location = new System.Drawing.Point(20, 20);
            lbl.Size = new System.Drawing.Size(500, 45);
            panelSlot1Position.Controls.Add(lbl);
            
            lblSlot1XLabel = new System.Windows.Forms.Label();
            lblSlot1XLabel.Text = "X (along rack) - Machine coordinate (G53):";
            lblSlot1XLabel.Location = new System.Drawing.Point(20, 80);
            lblSlot1XLabel.Size = new System.Drawing.Size(300, 20);
            panelSlot1Position.Controls.Add(lblSlot1XLabel);
            
            txtSlot1X = new System.Windows.Forms.TextBox();
            txtSlot1X.Location = new System.Drawing.Point(320, 78);
            txtSlot1X.Size = new System.Drawing.Size(100, 22);
            panelSlot1Position.Controls.Add(txtSlot1X);
            
            lblSlot1YLabel = new System.Windows.Forms.Label();
            lblSlot1YLabel.Text = "Y (secondary) - Machine coordinate (G53):";
            lblSlot1YLabel.Location = new System.Drawing.Point(20, 110);
            lblSlot1YLabel.Size = new System.Drawing.Size(300, 20);
            panelSlot1Position.Controls.Add(lblSlot1YLabel);
            
            txtSlot1Y = new System.Windows.Forms.TextBox();
            txtSlot1Y.Location = new System.Drawing.Point(320, 108);
            txtSlot1Y.Size = new System.Drawing.Size(100, 22);
            panelSlot1Position.Controls.Add(txtSlot1Y);
            
            btnReadPosition = new System.Windows.Forms.Button();
            btnReadPosition.Text = "Read Current Position";
            btnReadPosition.Location = new System.Drawing.Point(20, 150);
            btnReadPosition.Size = new System.Drawing.Size(150, 28);
            btnReadPosition.Click += new System.EventHandler(this.btnReadPosition_Click);
            panelSlot1Position.Controls.Add(btnReadPosition);
            
            this.Controls.Add(panelSlot1Position);
        }

        private void SetupPanelSlotPitch(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSlotPitch.Location = loc;
            panelSlotPitch.Size = size;
            panelSlotPitch.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Enter the distance between slot centers (pitch) in mm:";
            lbl.Location = new System.Drawing.Point(20, 20);
            lbl.Size = new System.Drawing.Size(400, 25);
            panelSlotPitch.Controls.Add(lbl);
            
            txtPitch = new System.Windows.Forms.TextBox();
            txtPitch.Location = new System.Drawing.Point(20, 50);
            txtPitch.Size = new System.Drawing.Size(100, 22);
            txtPitch.Text = "80.000";
            panelSlotPitch.Controls.Add(txtPitch);
            
            var lblUnit = new System.Windows.Forms.Label();
            lblUnit.Text = "mm";
            lblUnit.Location = new System.Drawing.Point(125, 53);
            lblUnit.Size = new System.Drawing.Size(30, 20);
            panelSlotPitch.Controls.Add(lblUnit);
            
            this.Controls.Add(panelSlotPitch);
        }

        private void SetupPanelZHeights(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelZHeights.Location = loc;
            panelZHeights.Size = size;
            panelZHeights.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Enter the Z heights (machine coordinates G53). Use 'Read Z' buttons to capture current position.";
            lbl.Location = new System.Drawing.Point(20, 10);
            lbl.Size = new System.Drawing.Size(500, 35);
            panelZHeights.Controls.Add(lbl);
            
            int y = 50;
            string[] labels = { "Z Safe (clearance above rack):", "Z Empty (spindle empty, above forks):", 
                "Z Approach (approaching slot):", "Z Clamp (tool fully in fork):", "Z Check (verify tool seated):" };
            System.Windows.Forms.TextBox[] boxes = new System.Windows.Forms.TextBox[5];
            System.Windows.Forms.Button[] btns = new System.Windows.Forms.Button[5];
            
            for (int i = 0; i < 5; i++)
            {
                var lblZ = new System.Windows.Forms.Label();
                lblZ.Text = labels[i];
                lblZ.Location = new System.Drawing.Point(20, y + 3);
                lblZ.Size = new System.Drawing.Size(220, 20);
                panelZHeights.Controls.Add(lblZ);
                
                boxes[i] = new System.Windows.Forms.TextBox();
                boxes[i].Location = new System.Drawing.Point(250, y);
                boxes[i].Size = new System.Drawing.Size(80, 22);
                panelZHeights.Controls.Add(boxes[i]);
                
                btns[i] = new System.Windows.Forms.Button();
                btns[i].Text = "Read Z";
                btns[i].Location = new System.Drawing.Point(340, y - 2);
                btns[i].Size = new System.Drawing.Size(70, 25);
                btns[i].Click += new System.EventHandler(this.btnReadZ_Click);
                panelZHeights.Controls.Add(btns[i]);
                
                y += 35;
            }
            
            txtZSafe = boxes[0];
            txtZEmpty = boxes[1];
            txtZApproach = boxes[2];
            txtZClamp = boxes[3];
            txtZCheck = boxes[4];
            btnReadZSafe = btns[0];
            btnReadZEmpty = btns[1];
            btnReadZApproach = btns[2];
            btnReadZClamp = btns[3];
            btnReadZCheck = btns[4];
            
            chkZCheckSameAsClamp = new System.Windows.Forms.CheckBox();
            chkZCheckSameAsClamp.Text = "Z Check same as Z Clamp";
            chkZCheckSameAsClamp.Location = new System.Drawing.Point(250, y + 10);
            chkZCheckSameAsClamp.Size = new System.Drawing.Size(200, 25);
            panelZHeights.Controls.Add(chkZCheckSameAsClamp);
            
            this.Controls.Add(panelZHeights);
        }

        private void SetupPanelDrawbar(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelDrawbar.Location = loc;
            panelDrawbar.Size = size;
            panelDrawbar.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Configure the drawbar output (opens/closes the tool clamp):";
            lbl.Location = new System.Drawing.Point(20, 20);
            lbl.Size = new System.Drawing.Size(400, 25);
            panelDrawbar.Controls.Add(lbl);
            
            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(20, 60);
            lblPort.Size = new System.Drawing.Size(50, 20);
            panelDrawbar.Controls.Add(lblPort);
            
            nudDrawbarPort = new System.Windows.Forms.NumericUpDown();
            nudDrawbarPort.Location = new System.Drawing.Point(70, 58);
            nudDrawbarPort.Size = new System.Drawing.Size(60, 22);
            nudDrawbarPort.Minimum = 1;
            nudDrawbarPort.Maximum = 10;
            nudDrawbarPort.Value = 1;
            panelDrawbar.Controls.Add(nudDrawbarPort);
            
            var lblPin = new System.Windows.Forms.Label();
            lblPin.Text = "Pin:";
            lblPin.Location = new System.Drawing.Point(150, 60);
            lblPin.Size = new System.Drawing.Size(40, 20);
            panelDrawbar.Controls.Add(lblPin);
            
            nudDrawbarPin = new System.Windows.Forms.NumericUpDown();
            nudDrawbarPin.Location = new System.Drawing.Point(190, 58);
            nudDrawbarPin.Size = new System.Drawing.Size(60, 22);
            nudDrawbarPin.Minimum = 1;
            nudDrawbarPin.Maximum = 32;
            nudDrawbarPin.Value = 14;
            panelDrawbar.Controls.Add(nudDrawbarPin);
            
            btnTestDrawbar = new System.Windows.Forms.Button();
            btnTestDrawbar.Text = "Test Drawbar";
            btnTestDrawbar.Location = new System.Drawing.Point(20, 100);
            btnTestDrawbar.Size = new System.Drawing.Size(120, 28);
            btnTestDrawbar.Click += new System.EventHandler(this.btnTestDrawbar_Click);
            panelDrawbar.Controls.Add(btnTestDrawbar);
            
            this.Controls.Add(panelDrawbar);
        }

        private void SetupPanelAirBlow(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelAirBlow.Location = loc;
            panelAirBlow.Size = size;
            panelAirBlow.Visible = false;
            
            chkAirBlowEnabled = new System.Windows.Forms.CheckBox();
            chkAirBlowEnabled.Text = "Enable Air Blow Output";
            chkAirBlowEnabled.Location = new System.Drawing.Point(20, 20);
            chkAirBlowEnabled.Size = new System.Drawing.Size(200, 25);
            panelAirBlow.Controls.Add(chkAirBlowEnabled);
            
            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(20, 60);
            lblPort.Size = new System.Drawing.Size(50, 20);
            panelAirBlow.Controls.Add(lblPort);
            
            nudAirBlowPort = new System.Windows.Forms.NumericUpDown();
            nudAirBlowPort.Location = new System.Drawing.Point(70, 58);
            nudAirBlowPort.Size = new System.Drawing.Size(60, 22);
            nudAirBlowPort.Minimum = 1;
            nudAirBlowPort.Maximum = 10;
            panelAirBlow.Controls.Add(nudAirBlowPort);
            
            var lblPin = new System.Windows.Forms.Label();
            lblPin.Text = "Pin:";
            lblPin.Location = new System.Drawing.Point(150, 60);
            lblPin.Size = new System.Drawing.Size(40, 20);
            panelAirBlow.Controls.Add(lblPin);
            
            nudAirBlowPin = new System.Windows.Forms.NumericUpDown();
            nudAirBlowPin.Location = new System.Drawing.Point(190, 58);
            nudAirBlowPin.Size = new System.Drawing.Size(60, 22);
            nudAirBlowPin.Minimum = 1;
            nudAirBlowPin.Maximum = 32;
            panelAirBlow.Controls.Add(nudAirBlowPin);
            
            this.Controls.Add(panelAirBlow);
        }

        private void SetupPanelDustHood(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelDustHood.Location = loc;
            panelDustHood.Size = size;
            panelDustHood.Visible = false;
            
            chkDustHoodEnabled = new System.Windows.Forms.CheckBox();
            chkDustHoodEnabled.Text = "Enable Dust Hood Output";
            chkDustHoodEnabled.Location = new System.Drawing.Point(20, 20);
            chkDustHoodEnabled.Size = new System.Drawing.Size(200, 25);
            panelDustHood.Controls.Add(chkDustHoodEnabled);
            
            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(20, 60);
            lblPort.Size = new System.Drawing.Size(50, 20);
            panelDustHood.Controls.Add(lblPort);
            
            nudDustHoodPort = new System.Windows.Forms.NumericUpDown();
            nudDustHoodPort.Location = new System.Drawing.Point(70, 58);
            nudDustHoodPort.Size = new System.Drawing.Size(60, 22);
            nudDustHoodPort.Minimum = 1;
            nudDustHoodPort.Maximum = 10;
            panelDustHood.Controls.Add(nudDustHoodPort);
            
            var lblPin = new System.Windows.Forms.Label();
            lblPin.Text = "Pin:";
            lblPin.Location = new System.Drawing.Point(150, 60);
            lblPin.Size = new System.Drawing.Size(40, 20);
            panelDustHood.Controls.Add(lblPin);
            
            nudDustHoodPin = new System.Windows.Forms.NumericUpDown();
            nudDustHoodPin.Location = new System.Drawing.Point(190, 58);
            nudDustHoodPin.Size = new System.Drawing.Size(60, 22);
            nudDustHoodPin.Minimum = 1;
            nudDustHoodPin.Maximum = 32;
            panelDustHood.Controls.Add(nudDustHoodPin);
            
            chkDustHoodRaise = new System.Windows.Forms.CheckBox();
            chkDustHoodRaise.Text = "Raise dust hood before tool change";
            chkDustHoodRaise.Location = new System.Drawing.Point(20, 100);
            chkDustHoodRaise.Size = new System.Drawing.Size(280, 25);
            chkDustHoodRaise.Checked = true;
            panelDustHood.Controls.Add(chkDustHoodRaise);
            
            chkDustHoodLower = new System.Windows.Forms.CheckBox();
            chkDustHoodLower.Text = "Lower dust hood after tool change";
            chkDustHoodLower.Location = new System.Drawing.Point(20, 130);
            chkDustHoodLower.Size = new System.Drawing.Size(280, 25);
            chkDustHoodLower.Checked = true;
            panelDustHood.Controls.Add(chkDustHoodLower);
            
            this.Controls.Add(panelDustHood);
        }

        private void SetupPanelPressureSensor(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelPressureSensor.Location = loc;
            panelPressureSensor.Size = size;
            panelPressureSensor.Visible = false;
            
            chkPressureEnabled = new System.Windows.Forms.CheckBox();
            chkPressureEnabled.Text = "Enable Air Pressure Sensor";
            chkPressureEnabled.Location = new System.Drawing.Point(20, 20);
            chkPressureEnabled.Size = new System.Drawing.Size(200, 25);
            panelPressureSensor.Controls.Add(chkPressureEnabled);
            
            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(20, 60);
            lblPort.Size = new System.Drawing.Size(50, 20);
            panelPressureSensor.Controls.Add(lblPort);
            
            nudPressurePort = new System.Windows.Forms.NumericUpDown();
            nudPressurePort.Location = new System.Drawing.Point(70, 58);
            nudPressurePort.Size = new System.Drawing.Size(60, 22);
            nudPressurePort.Minimum = 1;
            nudPressurePort.Maximum = 10;
            panelPressureSensor.Controls.Add(nudPressurePort);
            
            var lblPin = new System.Windows.Forms.Label();
            lblPin.Text = "Pin:";
            lblPin.Location = new System.Drawing.Point(150, 60);
            lblPin.Size = new System.Drawing.Size(40, 20);
            panelPressureSensor.Controls.Add(lblPin);
            
            nudPressurePin = new System.Windows.Forms.NumericUpDown();
            nudPressurePin.Location = new System.Drawing.Point(190, 58);
            nudPressurePin.Size = new System.Drawing.Size(60, 22);
            nudPressurePin.Minimum = 1;
            nudPressurePin.Maximum = 32;
            panelPressureSensor.Controls.Add(nudPressurePin);
            
            this.Controls.Add(panelPressureSensor);
        }

        private void SetupPanelSpindleToolSensor(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSpindleToolSensor.Location = loc;
            panelSpindleToolSensor.Size = size;
            panelSpindleToolSensor.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Optional sensor to detect if a tool is present in the spindle:";
            lbl.Location = new System.Drawing.Point(20, 10);
            lbl.Size = new System.Drawing.Size(400, 25);
            panelSpindleToolSensor.Controls.Add(lbl);
            
            chkSpindleToolSensorEnabled = new System.Windows.Forms.CheckBox();
            chkSpindleToolSensorEnabled.Text = "Enable Spindle Tool Presence Sensor";
            chkSpindleToolSensorEnabled.Location = new System.Drawing.Point(20, 40);
            chkSpindleToolSensorEnabled.Size = new System.Drawing.Size(280, 25);
            panelSpindleToolSensor.Controls.Add(chkSpindleToolSensorEnabled);
            
            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(20, 80);
            lblPort.Size = new System.Drawing.Size(50, 20);
            panelSpindleToolSensor.Controls.Add(lblPort);
            
            nudSpindleToolSensorPort = new System.Windows.Forms.NumericUpDown();
            nudSpindleToolSensorPort.Location = new System.Drawing.Point(70, 78);
            nudSpindleToolSensorPort.Size = new System.Drawing.Size(60, 22);
            nudSpindleToolSensorPort.Minimum = 1;
            nudSpindleToolSensorPort.Maximum = 10;
            panelSpindleToolSensor.Controls.Add(nudSpindleToolSensorPort);
            
            var lblPin = new System.Windows.Forms.Label();
            lblPin.Text = "Pin:";
            lblPin.Location = new System.Drawing.Point(150, 80);
            lblPin.Size = new System.Drawing.Size(40, 20);
            panelSpindleToolSensor.Controls.Add(lblPin);
            
            nudSpindleToolSensorPin = new System.Windows.Forms.NumericUpDown();
            nudSpindleToolSensorPin.Location = new System.Drawing.Point(190, 78);
            nudSpindleToolSensorPin.Size = new System.Drawing.Size(60, 22);
            nudSpindleToolSensorPin.Minimum = 1;
            nudSpindleToolSensorPin.Maximum = 32;
            panelSpindleToolSensor.Controls.Add(nudSpindleToolSensorPin);
            
            chkSpindleToolSensorActiveLow = new System.Windows.Forms.CheckBox();
            chkSpindleToolSensorActiveLow.Text = "Active Low (sensor ON = tool present)";
            chkSpindleToolSensorActiveLow.Location = new System.Drawing.Point(20, 115);
            chkSpindleToolSensorActiveLow.Size = new System.Drawing.Size(300, 25);
            chkSpindleToolSensorActiveLow.Checked = true;
            panelSpindleToolSensor.Controls.Add(chkSpindleToolSensorActiveLow);
            
            this.Controls.Add(panelSpindleToolSensor);
        }

        private void SetupPanelSlotSensors(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSlotSensors.Location = loc;
            panelSlotSensors.Size = size;
            panelSlotSensors.Visible = false;
            
            chkSlotSensorsEnabled = new System.Windows.Forms.CheckBox();
            chkSlotSensorsEnabled.Text = "Enable Slot Presence Sensors";
            chkSlotSensorsEnabled.Location = new System.Drawing.Point(20, 10);
            chkSlotSensorsEnabled.Size = new System.Drawing.Size(200, 25);
            panelSlotSensors.Controls.Add(chkSlotSensorsEnabled);
            
            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(20, 40);
            lblPort.Size = new System.Drawing.Size(50, 20);
            panelSlotSensors.Controls.Add(lblPort);
            
            nudSlotSensorPort = new System.Windows.Forms.NumericUpDown();
            nudSlotSensorPort.Location = new System.Drawing.Point(70, 38);
            nudSlotSensorPort.Size = new System.Drawing.Size(60, 22);
            nudSlotSensorPort.Minimum = 1;
            nudSlotSensorPort.Maximum = 10;
            nudSlotSensorPort.Value = 5;
            panelSlotSensors.Controls.Add(nudSlotSensorPort);
            
            nudSlotPins = new System.Windows.Forms.NumericUpDown[10];
            for (int i = 0; i < 10; i++)
            {
                int row = i / 5;
                int col = i % 5;
                
                var lblSlot = new System.Windows.Forms.Label();
                lblSlot.Text = $"Slot {i + 1}:";
                lblSlot.Location = new System.Drawing.Point(20 + col * 105, 75 + row * 50);
                lblSlot.Size = new System.Drawing.Size(50, 20);
                panelSlotSensors.Controls.Add(lblSlot);
                
                nudSlotPins[i] = new System.Windows.Forms.NumericUpDown();
                nudSlotPins[i].Location = new System.Drawing.Point(20 + col * 105, 95 + row * 50);
                nudSlotPins[i].Size = new System.Drawing.Size(50, 22);
                nudSlotPins[i].Minimum = 1;
                nudSlotPins[i].Maximum = 32;
                nudSlotPins[i].Value = 2 + i;
                panelSlotSensors.Controls.Add(nudSlotPins[i]);
            }
            
            this.Controls.Add(panelSlotSensors);
        }

        private void SetupPanelSafety(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSafety.Location = loc;
            panelSafety.Size = size;
            panelSafety.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Configure OSSD safety inputs (default: UC300ETH-UB1 LEDs 3-7):";
            lbl.Location = new System.Drawing.Point(20, 10);
            lbl.Size = new System.Drawing.Size(450, 25);
            panelSafety.Controls.Add(lbl);
            
            chkSafetyEnabled = new System.Windows.Forms.CheckBox();
            chkSafetyEnabled.Text = "Enable Safety Monitoring";
            chkSafetyEnabled.Location = new System.Drawing.Point(20, 35);
            chkSafetyEnabled.Size = new System.Drawing.Size(200, 25);
            chkSafetyEnabled.Checked = true;
            panelSafety.Controls.Add(chkSafetyEnabled);
            
            txtOssdName = new System.Windows.Forms.TextBox[5];
            nudOssdLed = new System.Windows.Forms.NumericUpDown[5];
            chkOssdActiveLow = new System.Windows.Forms.CheckBox[5];
            chkOssdEnabled = new System.Windows.Forms.CheckBox[5];
            
            string[] defaultNames = { "E-Stop", "X-Axis Driver", "Z-Axis Driver", "C-Axis Driver", "Power Supply" };
            int[] defaultLeds = { 3, 4, 5, 6, 7 };
            
            for (int i = 0; i < 5; i++)
            {
                int y = 70 + i * 30;
                
                chkOssdEnabled[i] = new System.Windows.Forms.CheckBox();
                chkOssdEnabled[i].Location = new System.Drawing.Point(20, y);
                chkOssdEnabled[i].Size = new System.Drawing.Size(20, 20);
                chkOssdEnabled[i].Checked = true;
                panelSafety.Controls.Add(chkOssdEnabled[i]);
                
                txtOssdName[i] = new System.Windows.Forms.TextBox();
                txtOssdName[i].Location = new System.Drawing.Point(45, y);
                txtOssdName[i].Size = new System.Drawing.Size(130, 22);
                txtOssdName[i].Text = defaultNames[i];
                panelSafety.Controls.Add(txtOssdName[i]);
                
                var lblLed = new System.Windows.Forms.Label();
                lblLed.Text = "LED:";
                lblLed.Location = new System.Drawing.Point(185, y + 3);
                lblLed.Size = new System.Drawing.Size(35, 20);
                panelSafety.Controls.Add(lblLed);
                
                nudOssdLed[i] = new System.Windows.Forms.NumericUpDown();
                nudOssdLed[i].Location = new System.Drawing.Point(220, y);
                nudOssdLed[i].Size = new System.Drawing.Size(50, 22);
                nudOssdLed[i].Minimum = 0;
                nudOssdLed[i].Maximum = 255;
                nudOssdLed[i].Value = defaultLeds[i];
                panelSafety.Controls.Add(nudOssdLed[i]);
                
                chkOssdActiveLow[i] = new System.Windows.Forms.CheckBox();
                chkOssdActiveLow[i].Text = "Active Low";
                chkOssdActiveLow[i].Location = new System.Drawing.Point(280, y);
                chkOssdActiveLow[i].Size = new System.Drawing.Size(100, 20);
                chkOssdActiveLow[i].Checked = true;
                panelSafety.Controls.Add(chkOssdActiveLow[i]);
            }
            
            this.Controls.Add(panelSafety);
        }

        private void SetupPanelProbe(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelProbe.Location = loc;
            panelProbe.Size = size;
            panelProbe.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Tool length probe settings for measuring tool offsets:";
            lbl.Location = new System.Drawing.Point(20, 10);
            lbl.Size = new System.Drawing.Size(400, 25);
            panelProbe.Controls.Add(lbl);
            
            chkUseUCCNCProbe = new System.Windows.Forms.CheckBox();
            chkUseUCCNCProbe.Text = "Use UCCNC probe settings (fields 2709, 2710, 2706, button 853, field 41)";
            chkUseUCCNCProbe.Location = new System.Drawing.Point(20, 40);
            chkUseUCCNCProbe.Size = new System.Drawing.Size(450, 25);
            chkUseUCCNCProbe.Checked = true;
            chkUseUCCNCProbe.CheckedChanged += new System.EventHandler(this.chkUseUCCNCProbe_CheckedChanged);
            panelProbe.Controls.Add(chkUseUCCNCProbe);
            
            var lblFast = new System.Windows.Forms.Label();
            lblFast.Text = "Fast feed (mm/min):";
            lblFast.Location = new System.Drawing.Point(20, 80);
            lblFast.Size = new System.Drawing.Size(130, 20);
            panelProbe.Controls.Add(lblFast);
            
            txtProbeFastFeed = new System.Windows.Forms.TextBox();
            txtProbeFastFeed.Location = new System.Drawing.Point(160, 78);
            txtProbeFastFeed.Size = new System.Drawing.Size(80, 22);
            panelProbe.Controls.Add(txtProbeFastFeed);
            
            var lblSlow = new System.Windows.Forms.Label();
            lblSlow.Text = "Slow feed (mm/min):";
            lblSlow.Location = new System.Drawing.Point(20, 110);
            lblSlow.Size = new System.Drawing.Size(130, 20);
            panelProbe.Controls.Add(lblSlow);
            
            txtProbeSlowFeed = new System.Windows.Forms.TextBox();
            txtProbeSlowFeed.Location = new System.Drawing.Point(160, 108);
            txtProbeSlowFeed.Size = new System.Drawing.Size(80, 22);
            panelProbe.Controls.Add(txtProbeSlowFeed);
            
            var lblRetract = new System.Windows.Forms.Label();
            lblRetract.Text = "Retract (mm):";
            lblRetract.Location = new System.Drawing.Point(20, 140);
            lblRetract.Size = new System.Drawing.Size(130, 20);
            panelProbe.Controls.Add(lblRetract);
            
            txtProbeRetract = new System.Windows.Forms.TextBox();
            txtProbeRetract.Location = new System.Drawing.Point(160, 138);
            txtProbeRetract.Size = new System.Drawing.Size(80, 22);
            panelProbe.Controls.Add(txtProbeRetract);
            
            chkProbeDualCycle = new System.Windows.Forms.CheckBox();
            chkProbeDualCycle.Text = "Dual cycle (fast then slow)";
            chkProbeDualCycle.Location = new System.Drawing.Point(20, 170);
            chkProbeDualCycle.Size = new System.Drawing.Size(200, 25);
            chkProbeDualCycle.Checked = true;
            panelProbe.Controls.Add(chkProbeDualCycle);
            
            var lblMinZ = new System.Windows.Forms.Label();
            lblMinZ.Text = "Min Z limit:";
            lblMinZ.Location = new System.Drawing.Point(20, 200);
            lblMinZ.Size = new System.Drawing.Size(130, 20);
            panelProbe.Controls.Add(lblMinZ);
            
            txtProbeMinZ = new System.Windows.Forms.TextBox();
            txtProbeMinZ.Location = new System.Drawing.Point(160, 198);
            txtProbeMinZ.Size = new System.Drawing.Size(80, 22);
            panelProbe.Controls.Add(txtProbeMinZ);
            
            this.Controls.Add(panelProbe);
        }

        private void SetupPanelEnableSlots(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelEnableSlots.Location = loc;
            panelEnableSlots.Size = size;
            panelEnableSlots.Visible = false;
            
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "Select which slots are physically present and usable:";
            lbl.Location = new System.Drawing.Point(20, 20);
            lbl.Size = new System.Drawing.Size(400, 25);
            panelEnableSlots.Controls.Add(lbl);
            
            chkSlotEnabled = new System.Windows.Forms.CheckBox[10];
            for (int i = 0; i < 10; i++)
            {
                int row = i / 5;
                int col = i % 5;
                
                chkSlotEnabled[i] = new System.Windows.Forms.CheckBox();
                chkSlotEnabled[i].Text = $"Slot {i + 1}";
                chkSlotEnabled[i].Location = new System.Drawing.Point(20 + col * 100, 60 + row * 40);
                chkSlotEnabled[i].Size = new System.Drawing.Size(80, 25);
                chkSlotEnabled[i].Checked = true;
                panelEnableSlots.Controls.Add(chkSlotEnabled[i]);
            }
            
            this.Controls.Add(panelEnableSlots);
        }

        private void SetupPanelFeedsAndTiming(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelFeedsAndTiming.Location = loc;
            panelFeedsAndTiming.Size = size;
            panelFeedsAndTiming.Visible = false;
            
            var lblFeed = new System.Windows.Forms.Label();
            lblFeed.Text = "Feed rate for tool change moves (mm/min):";
            lblFeed.Location = new System.Drawing.Point(20, 20);
            lblFeed.Size = new System.Drawing.Size(280, 20);
            panelFeedsAndTiming.Controls.Add(lblFeed);
            
            txtFeedRate = new System.Windows.Forms.TextBox();
            txtFeedRate.Location = new System.Drawing.Point(300, 18);
            txtFeedRate.Size = new System.Drawing.Size(80, 22);
            panelFeedsAndTiming.Controls.Add(txtFeedRate);
            
            var lblDelay = new System.Windows.Forms.Label();
            lblDelay.Text = "Operation delay between steps (ms):";
            lblDelay.Location = new System.Drawing.Point(20, 55);
            lblDelay.Size = new System.Drawing.Size(280, 20);
            panelFeedsAndTiming.Controls.Add(lblDelay);
            
            txtOperationDelay = new System.Windows.Forms.TextBox();
            txtOperationDelay.Location = new System.Drawing.Point(300, 53);
            txtOperationDelay.Size = new System.Drawing.Size(80, 22);
            panelFeedsAndTiming.Controls.Add(txtOperationDelay);
            
            var lblWait = new System.Windows.Forms.Label();
            lblWait.Text = "Sensor wait timeout (ms):";
            lblWait.Location = new System.Drawing.Point(20, 90);
            lblWait.Size = new System.Drawing.Size(280, 20);
            panelFeedsAndTiming.Controls.Add(lblWait);
            
            txtSensorWaitTime = new System.Windows.Forms.TextBox();
            txtSensorWaitTime.Location = new System.Drawing.Point(300, 88);
            txtSensorWaitTime.Size = new System.Drawing.Size(80, 22);
            panelFeedsAndTiming.Controls.Add(txtSensorWaitTime);
            
            this.Controls.Add(panelFeedsAndTiming);
        }

        private void SetupPanelOptions(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelOptions.Location = loc;
            panelOptions.Size = size;
            panelOptions.Visible = false;
            
            chkApplyG43 = new System.Windows.Forms.CheckBox();
            chkApplyG43.Text = "Apply G43 (tool length compensation) after tool change";
            chkApplyG43.Location = new System.Drawing.Point(20, 20);
            chkApplyG43.Size = new System.Drawing.Size(400, 25);
            panelOptions.Controls.Add(chkApplyG43);
            
            chkReturnToXY = new System.Windows.Forms.CheckBox();
            chkReturnToXY.Text = "Return to original XY position after tool change";
            chkReturnToXY.Location = new System.Drawing.Point(20, 50);
            chkReturnToXY.Size = new System.Drawing.Size(400, 25);
            panelOptions.Controls.Add(chkReturnToXY);
            
            chkMeasureNonRH = new System.Windows.Forms.CheckBox();
            chkMeasureNonRH.Text = "Measure non-registered holder tools";
            chkMeasureNonRH.Location = new System.Drawing.Point(20, 80);
            chkMeasureNonRH.Size = new System.Drawing.Size(400, 25);
            panelOptions.Controls.Add(chkMeasureNonRH);
            
            chkShowATCPage = new System.Windows.Forms.CheckBox();
            chkShowATCPage.Text = "Show ATC page after tool change";
            chkShowATCPage.Location = new System.Drawing.Point(20, 110);
            chkShowATCPage.Size = new System.Drawing.Size(400, 25);
            panelOptions.Controls.Add(chkShowATCPage);
            
            chkAirBlowDuringTC = new System.Windows.Forms.CheckBox();
            chkAirBlowDuringTC.Text = "Use air blow during tool change";
            chkAirBlowDuringTC.Location = new System.Drawing.Point(20, 140);
            chkAirBlowDuringTC.Size = new System.Drawing.Size(400, 25);
            panelOptions.Controls.Add(chkAirBlowDuringTC);
            
            this.Controls.Add(panelOptions);
        }

        private void SetupPanelSummary(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSummary.Location = loc;
            panelSummary.Size = size;
            panelSummary.Visible = false;
            
            txtSummary = new System.Windows.Forms.TextBox();
            txtSummary.Location = new System.Drawing.Point(10, 10);
            txtSummary.Size = new System.Drawing.Size(540, 320);
            txtSummary.Multiline = true;
            txtSummary.ReadOnly = true;
            txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtSummary.Font = new System.Drawing.Font("Consolas", 9F);
            panelSummary.Controls.Add(txtSummary);
            
            this.Controls.Add(panelSummary);
        }

        #endregion

        private System.Windows.Forms.Label lblStepIndicator;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnCancel;
        
        private System.Windows.Forms.Panel panelWelcome;
        private System.Windows.Forms.Panel panelRackOrientation;
        private System.Windows.Forms.Panel panelSlot1Position;
        private System.Windows.Forms.Panel panelSlotPitch;
        private System.Windows.Forms.Panel panelZHeights;
        private System.Windows.Forms.Panel panelDrawbar;
        private System.Windows.Forms.Panel panelAirBlow;
        private System.Windows.Forms.Panel panelDustHood;
        private System.Windows.Forms.Panel panelPressureSensor;
        private System.Windows.Forms.Panel panelSpindleToolSensor;
        private System.Windows.Forms.Panel panelSlotSensors;
        private System.Windows.Forms.Panel panelSafety;
        private System.Windows.Forms.Panel panelProbe;
        private System.Windows.Forms.Panel panelEnableSlots;
        private System.Windows.Forms.Panel panelFeedsAndTiming;
        private System.Windows.Forms.Panel panelOptions;
        private System.Windows.Forms.Panel panelSummary;
        
        private System.Windows.Forms.Button btnImportProfile;
        private System.Windows.Forms.RadioButton rbAxisX;
        private System.Windows.Forms.RadioButton rbAxisY;
        private System.Windows.Forms.Label lblSlot1XLabel;
        private System.Windows.Forms.Label lblSlot1YLabel;
        private System.Windows.Forms.TextBox txtSlot1X;
        private System.Windows.Forms.TextBox txtSlot1Y;
        private System.Windows.Forms.Button btnReadPosition;
        private System.Windows.Forms.TextBox txtPitch;
        private System.Windows.Forms.TextBox txtZSafe;
        private System.Windows.Forms.TextBox txtZEmpty;
        private System.Windows.Forms.TextBox txtZApproach;
        private System.Windows.Forms.TextBox txtZClamp;
        private System.Windows.Forms.TextBox txtZCheck;
        private System.Windows.Forms.Button btnReadZSafe;
        private System.Windows.Forms.Button btnReadZEmpty;
        private System.Windows.Forms.Button btnReadZApproach;
        private System.Windows.Forms.Button btnReadZClamp;
        private System.Windows.Forms.Button btnReadZCheck;
        private System.Windows.Forms.CheckBox chkZCheckSameAsClamp;
        private System.Windows.Forms.NumericUpDown nudDrawbarPort;
        private System.Windows.Forms.NumericUpDown nudDrawbarPin;
        private System.Windows.Forms.Button btnTestDrawbar;
        private System.Windows.Forms.CheckBox chkAirBlowEnabled;
        private System.Windows.Forms.NumericUpDown nudAirBlowPort;
        private System.Windows.Forms.NumericUpDown nudAirBlowPin;
        private System.Windows.Forms.CheckBox chkDustHoodEnabled;
        private System.Windows.Forms.NumericUpDown nudDustHoodPort;
        private System.Windows.Forms.NumericUpDown nudDustHoodPin;
        private System.Windows.Forms.CheckBox chkDustHoodRaise;
        private System.Windows.Forms.CheckBox chkDustHoodLower;
        private System.Windows.Forms.CheckBox chkPressureEnabled;
        private System.Windows.Forms.NumericUpDown nudPressurePort;
        private System.Windows.Forms.NumericUpDown nudPressurePin;
        private System.Windows.Forms.CheckBox chkSpindleToolSensorEnabled;
        private System.Windows.Forms.NumericUpDown nudSpindleToolSensorPort;
        private System.Windows.Forms.NumericUpDown nudSpindleToolSensorPin;
        private System.Windows.Forms.CheckBox chkSpindleToolSensorActiveLow;
        private System.Windows.Forms.CheckBox chkSlotSensorsEnabled;
        private System.Windows.Forms.NumericUpDown nudSlotSensorPort;
        private System.Windows.Forms.NumericUpDown[] nudSlotPins;
        private System.Windows.Forms.CheckBox chkSafetyEnabled;
        private System.Windows.Forms.TextBox[] txtOssdName;
        private System.Windows.Forms.NumericUpDown[] nudOssdLed;
        private System.Windows.Forms.CheckBox[] chkOssdActiveLow;
        private System.Windows.Forms.CheckBox[] chkOssdEnabled;
        private System.Windows.Forms.CheckBox chkUseUCCNCProbe;
        private System.Windows.Forms.TextBox txtProbeFastFeed;
        private System.Windows.Forms.TextBox txtProbeSlowFeed;
        private System.Windows.Forms.TextBox txtProbeRetract;
        private System.Windows.Forms.CheckBox chkProbeDualCycle;
        private System.Windows.Forms.TextBox txtProbeMinZ;
        private System.Windows.Forms.CheckBox[] chkSlotEnabled;
        private System.Windows.Forms.TextBox txtFeedRate;
        private System.Windows.Forms.TextBox txtOperationDelay;
        private System.Windows.Forms.TextBox txtSensorWaitTime;
        private System.Windows.Forms.CheckBox chkApplyG43;
        private System.Windows.Forms.CheckBox chkReturnToXY;
        private System.Windows.Forms.CheckBox chkMeasureNonRH;
        private System.Windows.Forms.CheckBox chkShowATCPage;
        private System.Windows.Forms.CheckBox chkAirBlowDuringTC;
        private System.Windows.Forms.TextBox txtSummary;
    }
}
