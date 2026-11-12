using System;
using System.Drawing;
using System.Windows.Forms;
using Plugins.Core;

namespace Plugins.Forms
{
    public static class ToolChangeDialogs
    {
        public static DialogResult ShowManualRemoveDialog(int toolNumber)
        {
            PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ShowManualRemoveDialog: T{toolNumber}");
            
            using (Form dialog = new Form())
            {
                dialog.Text = "Remove Tool!";
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(350, 150);
                dialog.TopMost = true;
                dialog.ShowInTaskbar = true;

                Label label = new Label
                {
                    Text = $"Manually remove Tool {toolNumber} from spindle!\n\nPress OK when done or Cancel to abort!",
                    Top = 20,
                    Left = 20,
                    Width = 310,
                    Height = 60,
                    Font = new Font("Arial", 10, FontStyle.Regular)
                };

                Button okButton = new Button
                {
                    Text = "OK",
                    Left = 80,
                    Top = 100,
                    Width = 80,
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    Left = 190,
                    Top = 100,
                    Width = 80,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                DialogResult result = dialog.ShowDialog();
                PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ManualRemoveDialog result: {result}");
                return result;
            }
        }

        public static DialogResult ShowManualLoadDialog(int toolNumber)
        {
            PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ShowManualLoadDialog: T{toolNumber}");
            
            using (Form dialog = new Form())
            {
                dialog.Text = "Load Tool!";
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(350, 150);
                dialog.TopMost = true;
                dialog.ShowInTaskbar = true;

                Label label = new Label
                {
                    Text = $"Manually load Tool {toolNumber} in spindle!\n\nPress OK when done or Cancel to abort!",
                    Top = 20,
                    Left = 20,
                    Width = 310,
                    Height = 60,
                    Font = new Font("Arial", 10, FontStyle.Regular)
                };

                Button okButton = new Button
                {
                    Text = "OK",
                    Left = 80,
                    Top = 100,
                    Width = 80,
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    Left = 190,
                    Top = 100,
                    Width = 80,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                DialogResult result = dialog.ShowDialog();
                PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ManualLoadDialog result: {result}");
                return result;
            }
        }

        public static DialogResult ShowConfirmRemoveFromSpindleDialog(int toolNumber)
        {
            PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ShowConfirmRemoveFromSpindleDialog: T{toolNumber}");
            
            using (Form dialog = new Form())
            {
                dialog.Text = "Remove Tool from Spindle";
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(380, 150);
                dialog.TopMost = true;
                dialog.ShowInTaskbar = true;

                Label label = new Label
                {
                    Text = $"Are you sure you want to remove current tool T{toolNumber} from spindle?\n\nPress OK to continue or Cancel to abort",
                    Top = 20,
                    Left = 20,
                    Width = 340,
                    Height = 60,
                    Font = new Font("Arial", 10, FontStyle.Regular)
                };

                Button okButton = new Button
                {
                    Text = "OK",
                    Left = 100,
                    Top = 100,
                    Width = 80,
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    Left = 200,
                    Top = 100,
                    Width = 80,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                DialogResult result = dialog.ShowDialog();
                PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ConfirmRemoveFromSpindleDialog result: {result}");
                return result;
            }
        }

        public static int? ShowSlotAssignmentDialog(int slotNumber, int currentTool)
        {
            PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ShowSlotAssignmentDialog: Slot {slotNumber}, current T{currentTool}");
            
            using (Form dialog = new Form())
            {
                string slotStr = slotNumber < 10 ? $"#0{slotNumber}" : $"#{slotNumber}";
                dialog.Text = $"Change Tool in Slot {slotStr}";
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(220, 160);
                dialog.TopMost = true;
                dialog.ShowInTaskbar = true;

                Label label = new Label
                {
                    Text = $"New tool for slot {slotNumber}\n(0 = empty)",
                    Top = 15,
                    Left = 15,
                    Width = 190,
                    Height = 35,
                    Font = new Font("Arial", 10, FontStyle.Regular)
                };

                TextBox inputBox = new TextBox
                {
                    Left = 60,
                    Top = 60,
                    Width = 100,
                    Font = new Font("Arial", 24, FontStyle.Regular),
                    TextAlign = HorizontalAlignment.Center,
                    Text = currentTool > 0 ? currentTool.ToString() : ""
                };

                Button okButton = new Button
                {
                    Text = "OK",
                    Left = 30,
                    Top = 115,
                    Width = 70,
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    Left = 120,
                    Top = 115,
                    Width = 70,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(inputBox);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                inputBox.SelectAll();

                DialogResult result = dialog.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    int newTool;
                    if (int.TryParse(inputBox.Text, out newTool) && newTool >= 0 && newTool <= 96)
                    {
                        PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] SlotAssignmentDialog: User entered T{newTool}");
                        return newTool;
                    }
                    PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] SlotAssignmentDialog: Invalid input '{inputBox.Text}'");
                    return null;
                }
                
                PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] SlotAssignmentDialog: Cancelled");
                return null;
            }
        }

        public static bool ShowToolEditDialog(int toolNumber, ref double diameter, ref double length, ref string toolType)
        {
            PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ShowToolEditDialog: T{toolNumber}");
            
            using (Form dialog = new Form())
            {
                dialog.Text = $"Edit Tool T{toolNumber}";
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(280, 200);
                dialog.TopMost = true;
                dialog.ShowInTaskbar = true;

                Label lblDiameter = new Label { Text = "Diameter:", Top = 20, Left = 20, Width = 70 };
                TextBox txtDiameter = new TextBox
                {
                    Left = 100,
                    Top = 17,
                    Width = 100,
                    Text = diameter.ToString(System.Globalization.CultureInfo.InvariantCulture)
                };

                Label lblLength = new Label { Text = "Length:", Top = 55, Left = 20, Width = 70 };
                TextBox txtLength = new TextBox
                {
                    Left = 100,
                    Top = 52,
                    Width = 100,
                    Text = length.ToString(System.Globalization.CultureInfo.InvariantCulture)
                };

                Label lblType = new Label { Text = "Type:", Top = 90, Left = 20, Width = 70 };
                ComboBox cmbType = new ComboBox
                {
                    Left = 100,
                    Top = 87,
                    Width = 100,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbType.Items.AddRange(new object[] { "RH", "NRH" });
                cmbType.SelectedItem = toolType == "RH" ? "RH" : "NRH";

                Button okButton = new Button
                {
                    Text = "OK",
                    Left = 50,
                    Top = 140,
                    Width = 80,
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    Left = 150,
                    Top = 140,
                    Width = 80,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(lblDiameter);
                dialog.Controls.Add(txtDiameter);
                dialog.Controls.Add(lblLength);
                dialog.Controls.Add(txtLength);
                dialog.Controls.Add(lblType);
                dialog.Controls.Add(cmbType);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                DialogResult result = dialog.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    double newDiameter, newLength;
                    if (double.TryParse(txtDiameter.Text, System.Globalization.NumberStyles.Float, 
                            System.Globalization.CultureInfo.InvariantCulture, out newDiameter) &&
                        double.TryParse(txtLength.Text, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out newLength))
                    {
                        diameter = newDiameter;
                        length = newLength;
                        toolType = cmbType.SelectedItem?.ToString() ?? "NRH";
                        PluginLogger.Log($"[T{System.Threading.Thread.CurrentThread.ManagedThreadId}] ToolEditDialog: D={diameter}, L={length}, Type={toolType}");
                        return true;
                    }
                }
                
                return false;
            }
        }
    }
}
