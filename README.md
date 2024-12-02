## RC version! Still Under progress...
- Use on your own risk!
- Works with UCCNC v1.2117 (not v1.2113 because some tooltable commands are missing).
- It needs the Default2019 screenset and macros.

---

##### Changelog
> 2024.12.02
> - Added Macro Description in README.md file.
> - Some comments translated to English (still have a lot of comments in Romanian, sorry).

---

##### HTM_ATC macros
- **Macroloops** (add them into macroloops):
  - **M20100** - Float formatting macroloop - getting the Position DROs decimals (updown field 195) from Settings->Gen.Setup page.
  - **M20101** - Read LEDs status for input pins (slot sensors, OSSD inputs).

- **Macros** (add them into macroloops):
  - **M20131** - Not used anymore, kept for reference only - old macro for tool edit.
  - **M20200** - ATC Setup page - ***Along X*** button - changes the rack orientation parallel with the X axis.
  - **M20201** - ATC Setup page - ***Along Y*** button - changes the rack orientation parallel with the Y axis.
  - **M20202** - ATC Setup page - ***Slot Sensors ON/OFF*** button - enables/disables slot sensors.
  - **M20203** - ATC Setup page - ***Air Blow ON/OFF*** button - enables/disables air blow during TC operation (dedust).
  - **M20330** - ATC Setup page - ***Measure TCP-TP position*** button - needs the Reference tool with known length as reference.
  - **M20331** - Macro for measuring tools in TCP. **M20330** has to be run first (once per setup). It is called by M6 and M20405.
  - **M20400** - ATC Page - macro for handling the ***Edit Tool*** fields. Is called by ***PLUS*** and ***MINUS*** buttons for tool selection.
  - **M20401** - ATC Page ***Edit Tool*** fields - ***PLUS*** button for tool selection. Will call M20400 for next tool number selection (+1).
  - **M20402** - ATC Page ***Edit Tool*** fields - ***MINUS*** button for tool selection. Will call M20400 for previous tool number selection (-1).
  - **M20403** - ATC Page ***Edit Tool*** fields - ***RH/NRH*** button. Changes the tool type. RH = Repeatable Holder, NRH = Non-Repeatable Holder (needs measurement when loading).
  - **M20404** - ATC Page ***Edit Tool*** fields - ***APPLY*** button. Saves the tool parameters in tool table.
  - **M20405** - ATC Page ***Edit Tool*** fields - ***Measure Tool*** button. Brings a confirmation dialog for measuring the selected tool.
  - **M20406** - ATC Page - ***Remove Tool*** button. Removes the current tool from spindle. The tool has to be removed manually from spindle.
  - **M20500** - ATC Page - macro for assigning a specific tool to active slots. It is called by ***M20501-M20510*** macros.
  - **M20501 to M20510** - ATC Page - caller macros for tool assignment. Transparent buttons above T# fields. See ***M20500***.
  - **M20600** - ATC Setup Page - macro called for slots setup. It is called by ***M20601-M20610*** macros.
  - **M20601 to M20610** - ATC Setup Page - caller macros for slots setup. Buttons ***S01-S10***. See ***M20600***.
---

