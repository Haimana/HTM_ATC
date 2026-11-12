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
            this.panelPressureSensor = new System.Windows.Forms.Panel();
            this.panelSlotSensors = new System.Windows.Forms.Panel();
            this.panelEnableSlots = new System.Windows.Forms.Panel();
            this.panelFeedsAndTiming = new System.Windows.Forms.Panel();
            this.panelTCP = new System.Windows.Forms.Panel();
            this.panelOptions = new System.Windows.Forms.Panel();
            this.panelSummary = new System.Windows.Forms.Panel();
            
            this.SuspendLayout();

            // Header
            this.lblStepIndicator.Location = new System.Drawing.Point(20, 15);
            this.lblStepIndicator.Size = new System.Drawing.Size(500, 25);
            this.lblStepIndicator.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.Controls.Add(this.lblStepIndicator);

            this.progressBar.Location = new System.Drawing.Point(20, 45);
            this.progressBar.Size = new System.Drawing.Size(560, 10);
            this.Controls.Add(this.progressBar);

            // Navigation buttons
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

            // All panels share same location and size
            var panelLocation = new System.Drawing.Point(20, 65);
            var panelSize = new System.Drawing.Size(560, 340);

            // Panel Welcome (Step 0)
            SetupPanelWelcome(panelLocation, panelSize);
            
            // Panel Rack Orientation (Step 1)
            SetupPanelRackOrientation(panelLocation, panelSize);
            
            // Panel Slot 1 Position (Step 2)
            SetupPanelSlot1Position(panelLocation, panelSize);
            
            // Panel Slot Pitch (Step 3)
            SetupPanelSlotPitch(panelLocation, panelSize);
            
            // Panel Z Heights (Step 4)
            SetupPanelZHeights(panelLocation, panelSize);
            
            // Panel Drawbar (Step 5)
            SetupPanelDrawbar(panelLocation, panelSize);
            
            // Panel Air Blow (Step 6)
            SetupPanelAirBlow(panelLocation, panelSize);
            
            // Panel Pressure Sensor (Step 7)
            SetupPanelPressureSensor(panelLocation, panelSize);
            
            // Panel Slot Sensors (Step 8)
            SetupPanelSlotSensors(panelLocation, panelSize);
            
            // Panel Enable Slots (Step 9)
            SetupPanelEnableSlots(panelLocation, panelSize);
            
            // Panel Feeds and Timing (Step 10)
            SetupPanelFeedsAndTiming(panelLocation, panelSize);
            
            // Panel TCP (Step 11)
            SetupPanelTCP(panelLocation, panelSize);
            
            // Panel Options (Step 12)
            SetupPanelOptions(panelLocation, panelSize);
            
            // Panel Summary (Step 13)
            SetupPanelSummary(panelLocation, panelSize);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 470);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "HTM ATC Setup Wizard";

            this.ResumeLayout(false);
        }

        private void SetupPanelWelcome(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelWelcome.Location = loc;
            panelWelcome.Size = size;
            panelWelcome.Visible = false;

            var lblWelcome = new System.Windows.Forms.Label();
            lblWelcome.Text = "Welcome to the HTM ATC Setup Wizard";
            lblWelcome.Location = new System.Drawing.Point(10, 10);
            lblWelcome.Size = new System.Drawing.Size(540, 30);
            lblWelcome.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold);
            panelWelcome.Controls.Add(lblWelcome);

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "This wizard will guide you through configuring your automatic tool changer.\n\n" +
                "You will configure:\n" +
                "• Tool rack orientation and slot positions\n" +
                "• Z heights for safe travel and tool exchange\n" +
                "• I/O pins for drawbar, air blow, and sensors\n" +
                "• Enabled slots and timing parameters\n" +
                "• Tool change options\n\n" +
                "You can import settings from an existing HTM_ATC profile or start fresh.";
            lblDesc.Location = new System.Drawing.Point(10, 50);
            lblDesc.Size = new System.Drawing.Size(540, 200);
            panelWelcome.Controls.Add(lblDesc);

            btnImportProfile = new System.Windows.Forms.Button();
            btnImportProfile.Text = "Import from HTM_ATC Profile...";
            btnImportProfile.Location = new System.Drawing.Point(10, 270);
            btnImportProfile.Size = new System.Drawing.Size(200, 35);
            btnImportProfile.Click += new System.EventHandler(btnImportProfile_Click);
            panelWelcome.Controls.Add(btnImportProfile);

            this.Controls.Add(panelWelcome);
        }

        private void SetupPanelRackOrientation(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelRackOrientation.Location = loc;
            panelRackOrientation.Size = size;
            panelRackOrientation.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Select the axis along which your tool rack is aligned.\n\n" +
                "Most linear racks have tools arranged in a row along X or Y.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 60);
            panelRackOrientation.Controls.Add(lblDesc);

            rbAxisX = new System.Windows.Forms.RadioButton();
            rbAxisX.Text = "X-axis (tools aligned along X)";
            rbAxisX.Location = new System.Drawing.Point(30, 90);
            rbAxisX.Size = new System.Drawing.Size(250, 30);
            rbAxisX.Font = new System.Drawing.Font("Arial", 11F);
            rbAxisX.Checked = true;
            panelRackOrientation.Controls.Add(rbAxisX);

            rbAxisY = new System.Windows.Forms.RadioButton();
            rbAxisY.Text = "Y-axis (tools aligned along Y)";
            rbAxisY.Location = new System.Drawing.Point(30, 130);
            rbAxisY.Size = new System.Drawing.Size(250, 30);
            rbAxisY.Font = new System.Drawing.Font("Arial", 11F);
            panelRackOrientation.Controls.Add(rbAxisY);

            this.Controls.Add(panelRackOrientation);
        }

        private void SetupPanelSlot1Position(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSlot1Position.Location = loc;
            panelSlot1Position.Size = size;
            panelSlot1Position.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Position the spindle directly above the center of Slot 1.\n" +
                "This is where the tool holder cone aligns with the fork.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 40);
            panelSlot1Position.Controls.Add(lblDesc);

            var lblX = new System.Windows.Forms.Label();
            lblX.Text = "Rack Axis Position:";
            lblX.Location = new System.Drawing.Point(10, 70);
            lblX.Size = new System.Drawing.Size(150, 25);
            panelSlot1Position.Controls.Add(lblX);

            txtSlot1X = new System.Windows.Forms.TextBox();
            txtSlot1X.Location = new System.Drawing.Point(170, 68);
            txtSlot1X.Size = new System.Drawing.Size(100, 25);
            panelSlot1Position.Controls.Add(txtSlot1X);

            var lblY = new System.Windows.Forms.Label();
            lblY.Text = "Secondary Axis Position:";
            lblY.Location = new System.Drawing.Point(10, 105);
            lblY.Size = new System.Drawing.Size(150, 25);
            panelSlot1Position.Controls.Add(lblY);

            txtSlot1Y = new System.Windows.Forms.TextBox();
            txtSlot1Y.Location = new System.Drawing.Point(170, 103);
            txtSlot1Y.Size = new System.Drawing.Size(100, 25);
            panelSlot1Position.Controls.Add(txtSlot1Y);

            btnReadPosition = new System.Windows.Forms.Button();
            btnReadPosition.Text = "Read Current Position";
            btnReadPosition.Location = new System.Drawing.Point(10, 150);
            btnReadPosition.Size = new System.Drawing.Size(160, 35);
            btnReadPosition.Click += new System.EventHandler(btnReadPosition_Click);
            panelSlot1Position.Controls.Add(btnReadPosition);

            this.Controls.Add(panelSlot1Position);
        }

        private void SetupPanelSlotPitch(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSlotPitch.Location = loc;
            panelSlotPitch.Size = size;
            panelSlotPitch.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Enter the center-to-center distance between adjacent slots (in mm).\n\n" +
                "Typical values: 60-100mm depending on tool holder size.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 60);
            panelSlotPitch.Controls.Add(lblDesc);

            var lblPitch = new System.Windows.Forms.Label();
            lblPitch.Text = "Slot Pitch (mm):";
            lblPitch.Location = new System.Drawing.Point(10, 90);
            lblPitch.Size = new System.Drawing.Size(120, 25);
            panelSlotPitch.Controls.Add(lblPitch);

            txtPitch = new System.Windows.Forms.TextBox();
            txtPitch.Location = new System.Drawing.Point(140, 88);
            txtPitch.Size = new System.Drawing.Size(100, 25);
            txtPitch.Text = "80.000";
            panelSlotPitch.Controls.Add(txtPitch);

            this.Controls.Add(panelSlotPitch);
        }

        private void SetupPanelZHeights(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelZHeights.Location = loc;
            panelZHeights.Size = size;
            panelZHeights.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Configure the Z heights for tool change operations.\nAll values are in MACHINE coordinates (G53).";
            lblDesc.Location = new System.Drawing.Point(10, 5);
            lblDesc.Size = new System.Drawing.Size(540, 35);
            panelZHeights.Controls.Add(lblDesc);

            int y = 45;
            int spacing = 55;

            AddZHeightRow(panelZHeights, "Z Safe (clearance):", "Highest Z, clears all obstacles", ref txtZSafe, ref btnReadZSafe, y);
            y += spacing;
            AddZHeightRow(panelZHeights, "Z Empty (empty spindle):", "Z for travel with empty spindle", ref txtZEmpty, ref btnReadZEmpty, y);
            y += spacing;
            AddZHeightRow(panelZHeights, "Y Approach (fork clear):", "Secondary axis fork clearance", ref txtZApproach, ref btnReadZApproach, y);
            y += spacing;
            AddZHeightRow(panelZHeights, "Z Clamp (tool engage):", "Z where tool seats in fork", ref txtZClamp, ref btnReadZClamp, y);
            y += spacing;
            AddZHeightRow(panelZHeights, "Z Check (sensor verify):", "Z for sensor verification", ref txtZCheck, ref btnReadZCheck, y);

            chkZCheckSameAsClamp = new System.Windows.Forms.CheckBox();
            chkZCheckSameAsClamp.Text = "Same as Z Clamp";
            chkZCheckSameAsClamp.Location = new System.Drawing.Point(380, y + 3);
            chkZCheckSameAsClamp.Size = new System.Drawing.Size(150, 20);
            panelZHeights.Controls.Add(chkZCheckSameAsClamp);

            this.Controls.Add(panelZHeights);
        }

        private void AddZHeightRow(System.Windows.Forms.Panel panel, string label, string hint, 
            ref System.Windows.Forms.TextBox txt, ref System.Windows.Forms.Button btn, int y)
        {
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = label;
            lbl.Location = new System.Drawing.Point(10, y);
            lbl.Size = new System.Drawing.Size(180, 20);
            panel.Controls.Add(lbl);

            var lblHint = new System.Windows.Forms.Label();
            lblHint.Text = hint;
            lblHint.Location = new System.Drawing.Point(10, y + 20);
            lblHint.Size = new System.Drawing.Size(180, 20);
            lblHint.ForeColor = System.Drawing.Color.Gray;
            lblHint.Font = new System.Drawing.Font("Arial", 8F);
            panel.Controls.Add(lblHint);

            txt = new System.Windows.Forms.TextBox();
            txt.Location = new System.Drawing.Point(200, y + 5);
            txt.Size = new System.Drawing.Size(100, 25);
            panel.Controls.Add(txt);

            btn = new System.Windows.Forms.Button();
            btn.Text = "Read";
            btn.Location = new System.Drawing.Point(310, y + 3);
            btn.Size = new System.Drawing.Size(60, 28);
            btn.Click += new System.EventHandler(btnReadZ_Click);
            panel.Controls.Add(btn);
        }

        private void SetupPanelDrawbar(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelDrawbar.Location = loc;
            panelDrawbar.Size = size;
            panelDrawbar.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Configure the output that controls the drawbar pneumatic valve.\n" +
                "When activated, the drawbar opens to release the tool.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 40);
            panelDrawbar.Controls.Add(lblDesc);

            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(10, 70);
            lblPort.Size = new System.Drawing.Size(50, 25);
            panelDrawbar.Controls.Add(lblPort);

            nudDrawbarPort = new System.Windows.Forms.NumericUpDown();
            nudDrawbarPort.Location = new System.Drawing.Point(70, 68);
            nudDrawbarPort.Size = new System.Drawing.Size(60, 25);
            nudDrawbarPort.Minimum = 1;
            nudDrawbarPort.Maximum = 5;
            nudDrawbarPort.Value = 1;
            panelDrawbar.Controls.Add(nudDrawbarPort);

            var lblPin = new System.Windows.Forms.Label();
            lblPin.Text = "Pin:";
            lblPin.Location = new System.Drawing.Point(150, 70);
            lblPin.Size = new System.Drawing.Size(40, 25);
            panelDrawbar.Controls.Add(lblPin);

            nudDrawbarPin = new System.Windows.Forms.NumericUpDown();
            nudDrawbarPin.Location = new System.Drawing.Point(200, 68);
            nudDrawbarPin.Size = new System.Drawing.Size(60, 25);
            nudDrawbarPin.Minimum = 1;
            nudDrawbarPin.Maximum = 17;
            nudDrawbarPin.Value = 14;
            panelDrawbar.Controls.Add(nudDrawbarPin);

            btnTestDrawbarWizard = new System.Windows.Forms.Button();
            btnTestDrawbarWizard.Text = "Test Drawbar";
            btnTestDrawbarWizard.Location = new System.Drawing.Point(10, 120);
            btnTestDrawbarWizard.Size = new System.Drawing.Size(120, 35);
            btnTestDrawbarWizard.Click += new System.EventHandler(btnTestDrawbar_Click);
            panelDrawbar.Controls.Add(btnTestDrawbarWizard);

            var lblWarning = new System.Windows.Forms.Label();
            lblWarning.Text = "⚠️ Test will toggle the output. Ensure spindle is clear!";
            lblWarning.Location = new System.Drawing.Point(140, 125);
            lblWarning.Size = new System.Drawing.Size(350, 25);
            lblWarning.ForeColor = System.Drawing.Color.OrangeRed;
            panelDrawbar.Controls.Add(lblWarning);

            this.Controls.Add(panelDrawbar);
        }

        private void SetupPanelAirBlow(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelAirBlow.Location = loc;
            panelAirBlow.Size = size;
            panelAirBlow.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Configure the air blow output for cleaning tool holders during changes.\n" +
                "This is optional.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 40);
            panelAirBlow.Controls.Add(lblDesc);

            chkAirBlowEnabled = new System.Windows.Forms.CheckBox();
            chkAirBlowEnabled.Text = "Enable Air Blow";
            chkAirBlowEnabled.Location = new System.Drawing.Point(10, 60);
            chkAirBlowEnabled.Size = new System.Drawing.Size(150, 25);
            chkAirBlowEnabled.Checked = true;
            panelAirBlow.Controls.Add(chkAirBlowEnabled);

            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(10, 100);
            lblPort.Size = new System.Drawing.Size(50, 25);
            panelAirBlow.Controls.Add(lblPort);

            nudAirBlowPort = new System.Windows.Forms.NumericUpDown();
            nudAirBlowPort.Location = new System.Drawing.Point(70, 98);
            nudAirBlowPort.Size = new System.Drawing.Size(60, 25);
            nudAirBlowPort.Minimum = 1;
            nudAirBlowPort.Maximum = 5;
            nudAirBlowPort.Value = 1;
            panelAirBlow.Controls.Add(nudAirBlowPort);

            var lblPin = new System.Windows.Forms.Label();
            lblPin.Text = "Pin:";
            lblPin.Location = new System.Drawing.Point(150, 100);
            lblPin.Size = new System.Drawing.Size(40, 25);
            panelAirBlow.Controls.Add(lblPin);

            nudAirBlowPin = new System.Windows.Forms.NumericUpDown();
            nudAirBlowPin.Location = new System.Drawing.Point(200, 98);
            nudAirBlowPin.Size = new System.Drawing.Size(60, 25);
            nudAirBlowPin.Minimum = 1;
            nudAirBlowPin.Maximum = 17;
            nudAirBlowPin.Value = 15;
            panelAirBlow.Controls.Add(nudAirBlowPin);

            this.Controls.Add(panelAirBlow);
        }

        private void SetupPanelPressureSensor(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelPressureSensor.Location = loc;
            panelPressureSensor.Size = size;
            panelPressureSensor.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Configure an input to detect low air pressure.\n" +
                "Tool changes will be blocked if pressure is low.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 40);
            panelPressureSensor.Controls.Add(lblDesc);

            chkPressureEnabled = new System.Windows.Forms.CheckBox();
            chkPressureEnabled.Text = "Enable Pressure Sensor";
            chkPressureEnabled.Location = new System.Drawing.Point(10, 60);
            chkPressureEnabled.Size = new System.Drawing.Size(200, 25);
            chkPressureEnabled.Checked = true;
            panelPressureSensor.Controls.Add(chkPressureEnabled);

            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Port:";
            lblPort.Location = new System.Drawing.Point(10, 100);
            lblPort.Size = new System.Drawing.Size(50, 25);
            panelPressureSensor.Controls.Add(lblPort);

            nudPressurePort = new System.Windows.Forms.NumericUpDown();
            nudPressurePort.Location = new System.Drawing.Point(70, 98);
            nudPressurePort.Size = new System.Drawing.Size(60, 25);
            nudPressurePort.Minimum = 1;
            nudPressurePort.Maximum = 5;
            nudPressurePort.Value = 1;
            panelPressureSensor.Controls.Add(nudPressurePort);

            var lblPin = new System.Windows.Forms.Label();
            lblPin.Text = "Pin:";
            lblPin.Location = new System.Drawing.Point(150, 100);
            lblPin.Size = new System.Drawing.Size(40, 25);
            panelPressureSensor.Controls.Add(lblPin);

            nudPressurePin = new System.Windows.Forms.NumericUpDown();
            nudPressurePin.Location = new System.Drawing.Point(200, 98);
            nudPressurePin.Size = new System.Drawing.Size(60, 25);
            nudPressurePin.Minimum = 1;
            nudPressurePin.Maximum = 17;
            nudPressurePin.Value = 10;
            panelPressureSensor.Controls.Add(nudPressurePin);

            this.Controls.Add(panelPressureSensor);
        }

        private void SetupPanelSlotSensors(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSlotSensors.Location = loc;
            panelSlotSensors.Size = size;
            panelSlotSensors.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Configure inputs that detect whether each slot has a tool present.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 25);
            panelSlotSensors.Controls.Add(lblDesc);

            chkSlotSensorsEnabled = new System.Windows.Forms.CheckBox();
            chkSlotSensorsEnabled.Text = "Enable Slot Sensors";
            chkSlotSensorsEnabled.Location = new System.Drawing.Point(10, 40);
            chkSlotSensorsEnabled.Size = new System.Drawing.Size(200, 25);
            chkSlotSensorsEnabled.Checked = true;
            panelSlotSensors.Controls.Add(chkSlotSensorsEnabled);

            var lblPort = new System.Windows.Forms.Label();
            lblPort.Text = "Sensor Port:";
            lblPort.Location = new System.Drawing.Point(10, 75);
            lblPort.Size = new System.Drawing.Size(90, 25);
            panelSlotSensors.Controls.Add(lblPort);

            nudSlotSensorPort = new System.Windows.Forms.NumericUpDown();
            nudSlotSensorPort.Location = new System.Drawing.Point(110, 73);
            nudSlotSensorPort.Size = new System.Drawing.Size(60, 25);
            nudSlotSensorPort.Minimum = 1;
            nudSlotSensorPort.Maximum = 5;
            nudSlotSensorPort.Value = 5;
            panelSlotSensors.Controls.Add(nudSlotSensorPort);

            var lblPins = new System.Windows.Forms.Label();
            lblPins.Text = "Pin mapping (Slot 1-10):";
            lblPins.Location = new System.Drawing.Point(10, 110);
            lblPins.Size = new System.Drawing.Size(200, 25);
            panelSlotSensors.Controls.Add(lblPins);

            nudSlotPins = new System.Windows.Forms.NumericUpDown[10];
            for (int i = 0; i < 10; i++)
            {
                var lblSlot = new System.Windows.Forms.Label();
                lblSlot.Text = $"S{i + 1}:";
                lblSlot.Location = new System.Drawing.Point(10 + (i % 5) * 110, 140 + (i / 5) * 35);
                lblSlot.Size = new System.Drawing.Size(30, 25);
                panelSlotSensors.Controls.Add(lblSlot);

                nudSlotPins[i] = new System.Windows.Forms.NumericUpDown();
                nudSlotPins[i].Location = new System.Drawing.Point(45 + (i % 5) * 110, 138 + (i / 5) * 35);
                nudSlotPins[i].Size = new System.Drawing.Size(55, 25);
                nudSlotPins[i].Minimum = 1;
                nudSlotPins[i].Maximum = 17;
                nudSlotPins[i].Value = i + 2;
                panelSlotSensors.Controls.Add(nudSlotPins[i]);
            }

            this.Controls.Add(panelSlotSensors);
        }

        private void SetupPanelEnableSlots(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelEnableSlots.Location = loc;
            panelEnableSlots.Size = size;
            panelEnableSlots.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Select which slots are physically present and usable in your rack.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 25);
            panelEnableSlots.Controls.Add(lblDesc);

            chkSlotEnabled = new System.Windows.Forms.CheckBox[10];
            for (int i = 0; i < 10; i++)
            {
                chkSlotEnabled[i] = new System.Windows.Forms.CheckBox();
                chkSlotEnabled[i].Text = $"Slot {i + 1}";
                chkSlotEnabled[i].Location = new System.Drawing.Point(30 + (i % 5) * 105, 50 + (i / 5) * 40);
                chkSlotEnabled[i].Size = new System.Drawing.Size(95, 30);
                chkSlotEnabled[i].Checked = true;
                chkSlotEnabled[i].Font = new System.Drawing.Font("Arial", 11F);
                panelEnableSlots.Controls.Add(chkSlotEnabled[i]);
            }

            this.Controls.Add(panelEnableSlots);
        }

        private void SetupPanelFeedsAndTiming(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelFeedsAndTiming.Location = loc;
            panelFeedsAndTiming.Size = size;
            panelFeedsAndTiming.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Configure motion speeds and timing delays.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 25);
            panelFeedsAndTiming.Controls.Add(lblDesc);

            var lblFeed = new System.Windows.Forms.Label();
            lblFeed.Text = "Tool Change Feed Rate (mm/min):";
            lblFeed.Location = new System.Drawing.Point(10, 50);
            lblFeed.Size = new System.Drawing.Size(230, 25);
            panelFeedsAndTiming.Controls.Add(lblFeed);

            txtFeedRate = new System.Windows.Forms.TextBox();
            txtFeedRate.Location = new System.Drawing.Point(250, 48);
            txtFeedRate.Size = new System.Drawing.Size(100, 25);
            txtFeedRate.Text = "2000.000";
            panelFeedsAndTiming.Controls.Add(txtFeedRate);

            var lblDelay = new System.Windows.Forms.Label();
            lblDelay.Text = "Operation Delay (ms):";
            lblDelay.Location = new System.Drawing.Point(10, 90);
            lblDelay.Size = new System.Drawing.Size(230, 25);
            panelFeedsAndTiming.Controls.Add(lblDelay);

            txtOperationDelay = new System.Windows.Forms.TextBox();
            txtOperationDelay.Location = new System.Drawing.Point(250, 88);
            txtOperationDelay.Size = new System.Drawing.Size(100, 25);
            txtOperationDelay.Text = "300";
            panelFeedsAndTiming.Controls.Add(txtOperationDelay);

            var lblWait = new System.Windows.Forms.Label();
            lblWait.Text = "Sensor Wait Time (ms):";
            lblWait.Location = new System.Drawing.Point(10, 130);
            lblWait.Size = new System.Drawing.Size(230, 25);
            panelFeedsAndTiming.Controls.Add(lblWait);

            txtSensorWaitTime = new System.Windows.Forms.TextBox();
            txtSensorWaitTime.Location = new System.Drawing.Point(250, 128);
            txtSensorWaitTime.Size = new System.Drawing.Size(100, 25);
            txtSensorWaitTime.Text = "5000";
            panelFeedsAndTiming.Controls.Add(txtSensorWaitTime);

            this.Controls.Add(panelFeedsAndTiming);
        }

        private void SetupPanelTCP(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelTCP.Location = loc;
            panelTCP.Size = size;
            panelTCP.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Position where the machine moves for manual tool loading/unloading\n" +
                "when a tool is not in the rack.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 40);
            panelTCP.Controls.Add(lblDesc);

            var lblX = new System.Windows.Forms.Label();
            lblX.Text = "X:";
            lblX.Location = new System.Drawing.Point(10, 70);
            lblX.Size = new System.Drawing.Size(30, 25);
            panelTCP.Controls.Add(lblX);

            txtTcpX = new System.Windows.Forms.TextBox();
            txtTcpX.Location = new System.Drawing.Point(50, 68);
            txtTcpX.Size = new System.Drawing.Size(100, 25);
            panelTCP.Controls.Add(txtTcpX);

            var lblY = new System.Windows.Forms.Label();
            lblY.Text = "Y:";
            lblY.Location = new System.Drawing.Point(170, 70);
            lblY.Size = new System.Drawing.Size(30, 25);
            panelTCP.Controls.Add(lblY);

            txtTcpY = new System.Windows.Forms.TextBox();
            txtTcpY.Location = new System.Drawing.Point(210, 68);
            txtTcpY.Size = new System.Drawing.Size(100, 25);
            panelTCP.Controls.Add(txtTcpY);

            var lblZ = new System.Windows.Forms.Label();
            lblZ.Text = "Z:";
            lblZ.Location = new System.Drawing.Point(330, 70);
            lblZ.Size = new System.Drawing.Size(30, 25);
            panelTCP.Controls.Add(lblZ);

            txtTcpZ = new System.Windows.Forms.TextBox();
            txtTcpZ.Location = new System.Drawing.Point(370, 68);
            txtTcpZ.Size = new System.Drawing.Size(100, 25);
            panelTCP.Controls.Add(txtTcpZ);

            btnReadTCP = new System.Windows.Forms.Button();
            btnReadTCP.Text = "Read Current Position";
            btnReadTCP.Location = new System.Drawing.Point(10, 110);
            btnReadTCP.Size = new System.Drawing.Size(160, 35);
            btnReadTCP.Click += new System.EventHandler(btnReadTCP_Click);
            panelTCP.Controls.Add(btnReadTCP);

            this.Controls.Add(panelTCP);
        }

        private void SetupPanelOptions(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelOptions.Location = loc;
            panelOptions.Size = size;
            panelOptions.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Configure optional behaviors during tool changes.";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 25);
            panelOptions.Controls.Add(lblDesc);

            chkApplyG43 = new System.Windows.Forms.CheckBox();
            chkApplyG43.Text = "Apply G43 after tool change (automatic tool length compensation)";
            chkApplyG43.Location = new System.Drawing.Point(10, 50);
            chkApplyG43.Size = new System.Drawing.Size(450, 25);
            chkApplyG43.Checked = true;
            panelOptions.Controls.Add(chkApplyG43);

            chkReturnToXY = new System.Windows.Forms.CheckBox();
            chkReturnToXY.Text = "Return to original XY position after tool change";
            chkReturnToXY.Location = new System.Drawing.Point(10, 85);
            chkReturnToXY.Size = new System.Drawing.Size(450, 25);
            chkReturnToXY.Checked = true;
            panelOptions.Controls.Add(chkReturnToXY);

            chkMeasureNonRH = new System.Windows.Forms.CheckBox();
            chkMeasureNonRH.Text = "Automatically measure tools not marked as 'Reference Height'";
            chkMeasureNonRH.Location = new System.Drawing.Point(10, 120);
            chkMeasureNonRH.Size = new System.Drawing.Size(450, 25);
            panelOptions.Controls.Add(chkMeasureNonRH);

            chkShowATCPage = new System.Windows.Forms.CheckBox();
            chkShowATCPage.Text = "Show ATC display page during tool change";
            chkShowATCPage.Location = new System.Drawing.Point(10, 155);
            chkShowATCPage.Size = new System.Drawing.Size(450, 25);
            panelOptions.Controls.Add(chkShowATCPage);

            chkAirBlowDuringTC = new System.Windows.Forms.CheckBox();
            chkAirBlowDuringTC.Text = "Use air blow during tool change (if enabled)";
            chkAirBlowDuringTC.Location = new System.Drawing.Point(10, 190);
            chkAirBlowDuringTC.Size = new System.Drawing.Size(450, 25);
            chkAirBlowDuringTC.Checked = true;
            panelOptions.Controls.Add(chkAirBlowDuringTC);

            this.Controls.Add(panelOptions);
        }

        private void SetupPanelSummary(System.Drawing.Point loc, System.Drawing.Size size)
        {
            panelSummary.Location = loc;
            panelSummary.Size = size;
            panelSummary.Visible = false;

            var lblDesc = new System.Windows.Forms.Label();
            lblDesc.Text = "Review your settings before saving:";
            lblDesc.Location = new System.Drawing.Point(10, 10);
            lblDesc.Size = new System.Drawing.Size(540, 25);
            panelSummary.Controls.Add(lblDesc);

            txtSummary = new System.Windows.Forms.TextBox();
            txtSummary.Multiline = true;
            txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtSummary.ReadOnly = true;
            txtSummary.Location = new System.Drawing.Point(10, 40);
            txtSummary.Size = new System.Drawing.Size(540, 290);
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
        private System.Windows.Forms.Panel panelPressureSensor;
        private System.Windows.Forms.Panel panelSlotSensors;
        private System.Windows.Forms.Panel panelEnableSlots;
        private System.Windows.Forms.Panel panelFeedsAndTiming;
        private System.Windows.Forms.Panel panelTCP;
        private System.Windows.Forms.Panel panelOptions;
        private System.Windows.Forms.Panel panelSummary;
        
        private System.Windows.Forms.Button btnImportProfile;
        private System.Windows.Forms.RadioButton rbAxisX;
        private System.Windows.Forms.RadioButton rbAxisY;
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
        private System.Windows.Forms.Button btnTestDrawbarWizard;
        private System.Windows.Forms.CheckBox chkAirBlowEnabled;
        private System.Windows.Forms.NumericUpDown nudAirBlowPort;
        private System.Windows.Forms.NumericUpDown nudAirBlowPin;
        private System.Windows.Forms.CheckBox chkPressureEnabled;
        private System.Windows.Forms.NumericUpDown nudPressurePort;
        private System.Windows.Forms.NumericUpDown nudPressurePin;
        private System.Windows.Forms.CheckBox chkSlotSensorsEnabled;
        private System.Windows.Forms.NumericUpDown nudSlotSensorPort;
        private System.Windows.Forms.NumericUpDown[] nudSlotPins;
        private System.Windows.Forms.CheckBox[] chkSlotEnabled;
        private System.Windows.Forms.TextBox txtFeedRate;
        private System.Windows.Forms.TextBox txtOperationDelay;
        private System.Windows.Forms.TextBox txtSensorWaitTime;
        private System.Windows.Forms.TextBox txtTcpX;
        private System.Windows.Forms.TextBox txtTcpY;
        private System.Windows.Forms.TextBox txtTcpZ;
        private System.Windows.Forms.Button btnReadTCP;
        private System.Windows.Forms.CheckBox chkApplyG43;
        private System.Windows.Forms.CheckBox chkReturnToXY;
        private System.Windows.Forms.CheckBox chkMeasureNonRH;
        private System.Windows.Forms.CheckBox chkShowATCPage;
        private System.Windows.Forms.CheckBox chkAirBlowDuringTC;
        private System.Windows.Forms.TextBox txtSummary;
    }
}
