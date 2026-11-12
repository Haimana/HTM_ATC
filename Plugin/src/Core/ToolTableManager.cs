using System;
using System.Globalization;

namespace Plugins.Core
{
    public class ToolInfo
    {
        public int ToolNumber { get; set; }
        public int Slot { get; set; }
        public double Diameter { get; set; }
        public double Length { get; set; }
        public string Type { get; set; } = "NRH";
        public bool IsReferenceHeight => Type == "RH";
    }

    public class ToolTableManager
    {
        private readonly Plugininterface.Entry _uc;

        public ToolTableManager(Plugininterface.Entry uc)
        {
            _uc = uc;
        }

        public ToolInfo ReadToolInfo(int toolNumber)
        {
            if (toolNumber < 1 || toolNumber > 96)
                return null;

            try
            {
                var info = new ToolInfo { ToolNumber = toolNumber };

                string slotStr = _uc.Readtooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Slot);
                int slot;
                if (int.TryParse(slotStr, out slot))
                    info.Slot = slot;

                string diamStr = _uc.Readtooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Diameter);
                double diam;
                if (double.TryParse(diamStr, NumberStyles.Float, CultureInfo.InvariantCulture, out diam))
                    info.Diameter = diam;

                string lenStr = _uc.Readtooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Length);
                double len;
                if (double.TryParse(lenStr, NumberStyles.Float, CultureInfo.InvariantCulture, out len))
                    info.Length = len;

                string typeStr = _uc.Readtooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Type);
                info.Type = string.IsNullOrEmpty(typeStr) ? "NRH" : typeStr;

                return info;
            }
            catch (Exception ex)
            {
                PluginLogger.Log($"Error reading tool info for T{toolNumber}: {ex.Message}");
                return null;
            }
        }

        public int FindToolSlot(int toolNumber)
        {
            if (toolNumber < 1 || toolNumber > 96)
                return 0;

            try
            {
                string slotStr = _uc.Readtooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Slot);
                int slot;
                if (int.TryParse(slotStr, out slot) && slot >= 1 && slot <= 10)
                    return slot;
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public int FindToolInSlot(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > 10)
                return 0;

            try
            {
                for (int tool = 1; tool <= 96; tool++)
                {
                    string slotStr = _uc.Readtooltablecell(tool, Plugininterface.Datatypes.Tooltablecolumnname.Slot);
                    int slot;
                    if (int.TryParse(slotStr, out slot) && slot == slotNumber)
                        return tool;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public bool WriteToolSlot(int toolNumber, int slot)
        {
            if (toolNumber < 0 || toolNumber > 96)
                return false;

            try
            {
                if (toolNumber == 0)
                    return true;

                _uc.Writetooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Slot, slot.ToString());
                PluginLogger.Log($"Wrote tooltable: T{toolNumber} Slot={slot}");
                return true;
            }
            catch (Exception ex)
            {
                PluginLogger.Log($"Error writing tool slot for T{toolNumber}: {ex.Message}");
                return false;
            }
        }

        public bool WriteToolInfo(int toolNumber, double diameter, double length, string type)
        {
            if (toolNumber < 1 || toolNumber > 96)
                return false;

            try
            {
                _uc.Writetooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Diameter,
                    diameter.ToString(CultureInfo.InvariantCulture));
                _uc.Writetooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Length,
                    length.ToString(CultureInfo.InvariantCulture));
                _uc.Writetooltablecell(toolNumber, Plugininterface.Datatypes.Tooltablecolumnname.Type, type ?? "NRH");
                
                PluginLogger.Log($"Wrote tooltable: T{toolNumber} D={diameter}, L={length}, Type={type}");
                return true;
            }
            catch (Exception ex)
            {
                PluginLogger.Log($"Error writing tool info for T{toolNumber}: {ex.Message}");
                return false;
            }
        }

        public bool SaveToolTable()
        {
            try
            {
                _uc.Callbutton(780);
                PluginLogger.Log("Tooltable saved (Callbutton 780)");
                return true;
            }
            catch (Exception ex)
            {
                PluginLogger.Log($"Error saving tooltable: {ex.Message}");
                return false;
            }
        }

        public void ReconcileWithConfig(ATCConfig config)
        {
            PluginLogger.Log("Reconciling tooltable with plugin config...");
            
            bool anyMismatch = false;

            for (int slot = 1; slot <= 10; slot++)
            {
                int toolInTable = FindToolInSlot(slot);
                int toolInConfig = config.Slots[slot - 1].ToolNumber;

                if (toolInTable != toolInConfig)
                {
                    anyMismatch = true;
                    PluginLogger.Log($"MISMATCH Slot {slot}: Tooltable has T{toolInTable}, Config has T{toolInConfig}");
                    
                    config.Slots[slot - 1].ToolNumber = toolInTable;
                }
            }

            if (anyMismatch)
            {
                PluginLogger.Log("Reconciliation complete: Config updated to match tooltable (tooltable is source of truth)");
            }
            else
            {
                PluginLogger.Log("Reconciliation complete: No mismatches found");
            }
        }
    }
}
