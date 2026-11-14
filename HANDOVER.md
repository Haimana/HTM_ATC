# Handover & Technical Architecture — `HTM_PRO` (`v1.0.0`) pentru UCCNC

Acest document centralizează **toată arhitectura Stage 1 implementată, deciziile tehnice, specificația completă convenită pentru Stage 2 (ATC), structura de licențiere per controller și instrucțiunile de build/instalare**, astfel încât dezvoltarea să poată fi reluată imediat de pe orice alt calculator din branch-ul `feature/htm-pro-stage1`.

---

## 1. Contextul Proiectului și Viziunea Unificată (`HTM_PRO.dll`)

`HTM_PRO` este evoluția plugin-ului nativ `HTM_ATC` într-un **singur plugin C# unificat** (`.NET Framework 4.8`, SDK-style `.csproj`) construit în **două stadii**:

1. **Stage 1 — Complete Machine Setup Wizard *(Implementat & Compilat în `v1.0.0`)*:**
   - La instalarea pe un UCCNC nou (cu unitățile de măsură deja setate la instalare) sau pe un profil existent, oferă un wizard complet în 5 pași care configurează toate resursele mașinii (axe, Master/Slave pe orice combinație `X/Y/Z`, limite soft, pini Step/Dir/Enable/Home/Limits/E-Stop/Spindle/Coolant/Probe) și calculează automat bugetul de pini/axe libere rămase pentru Stage 2.
2. **Stage 2 — ATC Setup & Runtime *(Următoarea etapă de implementat)*:**
   - Se deblochează după finalizarea Stage 1 (`Stage1Completed == true`) și gestionează magazia de scule (liniară până la 20 sloturi / rotativă până la 32 sloturi), strategiile de senzori în funcție de pinii liberi și ciclurile `M6` / măsurare / calibrare.

> **Notă repository:** Branch-ul anterior de dezvoltare pentru vechiul plugin `HTM_ATC` se află pe `feature/plugin-hybrid-native` (și `main`). Asset-urile grafice/audio/macro refolosibile pentru Stage 2 au fost deja copiate în [`UCCNC_Install_Ready/`](UCCNC_Install_Ready/).

---

## 2. Arhitectura Stage 1 și Fișierele Sursă (`Plugin/src/`)

Proiectul nu depinde de căi locale către UCCNC (`Plugin/lib/Plugininterface.dll` este inclus local, iar `Microsoft.NETFramework.ReferenceAssemblies` permite compilarea inclusiv pe Linux sau Windows).

### Structura fișierelor
- [`Plugin/HTM_PRO.csproj`](Plugin/HTM_PRO.csproj) — Proiect SDK-style (`net48`, `LangVersion=latest`, `Version=1.0.0`).
- [`Plugin/src/UCCNCplugin.cs`](Plugin/src/UCCNCplugin.cs) — Punctul de intrare UCCNC (`Init_event`, `Startup_event`, `Loop_event`, `Configure_event`, `Showup_event`, `Shutdown_event`, `Buttonpress_event`). La pornire (`Startup_event` + primul pass `Loop_event`), dacă `Stage1Completed == false` pentru controllerul activ, deschide automat `MachineSetupWizardForm`.
- [`Plugin/src/Config/MachineConfig.cs`](Plugin/src/Config/MachineConfig.cs) — Modelul complet de date Stage 1:
  - `AxisRole`: `PrimaryLinear`, `GantrySlave`, `RotaryWorkAxis`, `ReservedForAtcRotary`, `Unused`.
  - `EnablePinStrategy`: `SharedSinglePin`, `IndividualPerAxis`, `ExternalNoPin`.
  - `LimitHomeWiringStrategy`: `HomeOnlyWithSoftLimits`, `SharedHomeAndLimitPerAxis`, `IndividualHomePlusSeriesLimitPin`, `SeparateHomeLimitMinusLimitPlus`.
  - `HomePinGrouping`: `IndividualPerMotor`, `CommonMasterAndCommonSlave`, `SingleCommonAllAxes`.
  - `SpindleControlMode`: `PwmAndRelay`, `StepDirAndRelay`, `RelayOrModbusOnly`.
  - `AxisConfig`: pini Step/Dir/Ena/Home/Limit-/Limit+, kinematică (`StepsPerUnit`, `MaxVelocity`, `Acceleration`, `MaxTravel`, `SoftLimitNegative`, `SoftLimitPositive`), homing (`HomeDirectionPositive`, `HomeAutoZero`, `HomingSpeedUp`, `HomingSpeedDown`, `HomeBackoff`), opțiuni rotative (`IsRotary`, `RotaryRollover`).
- [`Plugin/src/Core/HardwareCatalog.cs`](Plugin/src/Core/HardwareCatalog.cs):
  - Baza de date cu pinout-urile exacte (intrări/ieșiri pe fiecare port) pentru toate cele **11 profile `ControllerModel`** din UCCNC 1.2118, grupate pe cele **5 familii hardware de bază (`BaseHardwareFamily`)**:
    1. `UC100 (USB)` → `UC100` (5 IN / 12 OUT)
    2. `UC400ETH (Ethernet)` → `UC400ETH` (10 IN / 24 OUT)
    3. `AXBB-E (Ethernet)` → `AXBB` (11 IN / 29 OUT)
    4. `UC300 (USB)` → `UC300_5LPT`, `UC300_M44`, `UC300_5441` (+ aliasuri `ISOBOB`, `STEPPER`, `Hi`, `Low`)
    5. `UC300ETH (Ethernet)` → `UC300ETH_5LPT`, `UC300ETH_UB1`, `UC300ETH_M44`, `UC300ETH_M45`, `UC300ETH_5441` (+ aliasuri `ISOBOB`, `STEPPER`, `Hi`, `Low`, `UCMIB`)
  - `DetectActiveControllerWithSerial(Form mainForm)`: citește prin Reflection din `UCCNC.Form1` câmpurile `userselecteddevtype`, `rundemomode` și `devserial`, returnând `(Model, IsDemoMode, RawDeviceType, DeviceSerial)`.
- [`Plugin/src/Core/ProfileService.cs`](Plugin/src/Core/ProfileService.cs):
  - **Unități de măsură automate (read-only):** `DetectImperialFromUccncProfile()` detectează `mm` vs `inch` direct din profilul `.pro` creat la instalarea UCCNC (`POSDROsdigits`, `Linearerror`, `Cornerserror`, `Velocity`, `SafeZ`).
  - **Stocare separată per interfață hardware:** salvează și încarcă separat per `(Profil, ControllerModel)`:
    - `Profiles/HTM_PRO_Config_<Profile>_<ControllerModel>.json`
    - `Profiles/HTM_PRO_Pinout_Summary_<Profile>_<ControllerModel>.txt`
    - (păstrează și fișierele legacy `HTM_PRO_Config_<Profile>.json` / `HTM_PRO_Pinout_Summary_<Profile>.txt` și le migrează automat la prima încărcare).
  - **Comutare automată între plăci:**
    - Dacă pornești UCCNC cu o interfață care are deja JSON salvat cu `Stage1Completed == true`, iar `.pro`-ul activ are altă placă în `[HTM_PRO] ControllerType`, apelează automat `ApplyAndSaveToUccnc` pentru a sincroniza `.pro`-ul și controalele GUI pentru placa curentă.
    - Dacă pornești UCCNC cu o interfață nouă (neconfigurată încă pentru profilul respectiv), `Stage1Completed` rămâne `false` (deci Wizard-ul se deschide automat), preia parametrii mecanici/cinematici de la un alt controller deja configurat (`CopyMechanicalAndFeatureSettings`), resetează pinii vechi (`ResetAllPinAssignments`) și auto-alocă pini compatibili cu noua placă (`PinAllocator.AutoAssignOutputs` / `AutoAssignInputs`).
  - **Aplicare completă în UCCNC (`ApplyAndSaveToUccnc`):**
    - Scrie direct în controalele UI din ecranul de setup UCCNC (`Setfieldtext`, `Setcheckboxstate`, `SelectComboboxByText` cu fallback pe indecși pentru `Slaveaxis` 1..3 și `Homesequence` 4..9), scrie cheile INI în `.pro` (inclusiv `Doontoolchange=2` pentru activare macro `M6`), și apelează `Callbutton(168)` (Apply settings) + `Callbutton(167)` (Save settings).
    - Auto-detectează `HomeGrouping` din pinii salvați dacă lipsește cheia din `.pro` (`DetectHomeGroupingFromPins`).
- [`Plugin/src/Core/PinAllocator.cs`](Plugin/src/Core/PinAllocator.cs):
  - `AutoAssignOutputs` & `AutoAssignInputs`: alocă pini valizi în ordinea preferată de porturi a controllerului activ, ținând cont de `EnablePinStrategy`, `LimitHomeWiringStrategy` și `HomePinGrouping`.
  - `ApplySharedHomeAndLimitIfNeeded`: propagă pinii Home către `Limit-`/`Limit+` când strategia aleasă cere partajarea acestora.
  - `ValidateAndAudit`: verifică pinii invalizi, coliziunile pe ieșiri, coliziunile cu intrări non-switch (`E-Stop`, `Probe 1/2`), permite partajarea pinilor pe bus-uri de tip switch (`Home`, `Limit-`, `Limit+`), dar blochează situația în care un `Slave` împarte exact același pin de Home cu propriul `Master` în modul `CommonMasterAndCommonSlave`.
  - `PinAuditReport.GetAtcReadinessSummary()`: raportează exact ieșirile libere, intrările libere și canalele de axe libere (`A/B/C`) disponibile pentru Stage 2 ATC.
- [`Plugin/src/UI/MachineSetupWizardForm.cs`](Plugin/src/UI/MachineSetupWizardForm.cs):
  - Wizard cu 5 tab-uri (`1. Environment`, `2. Axes & Gantry`, `3. Motor Outputs`, `4. Home & Limits`, `5. I/O & Summary`), tema Dark CNC și bară live de telemetrie resurse.
  - **Pasul 1:** Profil activ, interfață hardware detectată read-only (cu combo fallback doar pe generic `Demo_mode`), `Operating Mode & Device ID` (`Demo Mode` vs `Live Hardware | Serial`), unități de măsură read-only din instalare, listă cu interfețele deja salvate în profil.
  - **Pasul 2:** Pairing `Master / Slave` simultan pe orice combinație din `X`, `Y`, `Z` folosind canalele `A`, `B`, `C` (+ configurare canal direct din tabel: `Unused`, `RotaryWorkAxis`, `ReservedForAtcRotary`, `Slave -> X/Y/Z`).
  - **Pasul 4:** Selector `Home Sensor Grouping` (`IndividualPerMotor`, `CommonMasterAndCommonSlave`, `SingleCommonAllAxes`) cu propagare automată în timp real pe pinii și polaritățile (`Negate`) grupului.
- [`Plugin/src/UI/ScreenInjector.cs`](Plugin/src/UI/ScreenInjector.cs):
  - Injectează butonul nativ `HTM_PRO WIZARD` (`#31000`, bitmap generat automat în `Flashscreen/BMP/HTM_PRO/`) și eticheta de versiune pe Layer `6` (`CONFIGURATION` sidebar) din `Default2019.ssf`.
- [`Plugin/src/Utils/PluginVersion.cs`](Plugin/src/Utils/PluginVersion.cs) & [`Plugin/src/Utils/PluginLogger.cs`](Plugin/src/Utils/PluginLogger.cs): versiune centralizată din `.csproj` și logging thread-safe în `Profiles/HTM_PRO_<Profile>.log`.

---

## 3. Stadiul Curent & Ce Trebuie Verificat Interactiv

### Ce a fost testat și verificat până acum:
- Compilare curată: `dotnet build -c Release Plugin/HTM_PRO.csproj` → `0 Warning(s), 0 Error(s)`.
- Build-ul actual (`HTM_PRO.dll`) include toate ultimele modificări:
  - Suport Slave pe orice axă (`X`, `Y`, `Z`, verificat inclusiv profil `X/Y/Z` + `B->Y` + `C->Z`).
  - Suport `HomePinGrouping` (`CommonMasterAndCommonSlave` / `SingleCommonAllAxes`) și eliminare conflict fals de validare când Master-ele împart un pin (`P1-4`) și Slave-urile împart alt pin (`P1-7`).
  - Unități de măsură auto-detectate read-only și stocare separată per controller (`HTM_PRO_Config_<Profil>_<Controller>.json`).
- În [`UCCNC_Install_Ready/Profiles/`](UCCNC_Install_Ready/Profiles/) există deja configurația de referință pentru profilul `HTM_PRO_Test` pe `UC300ETH_UB1` (`X`, `Y` + slave `B`, `Z` + slave `C`, `CommonMasterAndCommonSlave` pe `P1-4` și `P1-7`).

### Ce trebuie verificat interactiv în UCCNC la reluare:
1. Testarea live în UCCNC (`CONFIGURATION -> HTM_PRO WIZARD`) a selectorului `Home Sensor Grouping` la Pasul 4 și a salvării fără `Pin Validation Warning` la Pasul 5 (`Apply & Save Stage 1`).
2. Testarea live a comutării în Demo Mode între două interfețe (ex. pornire cu `Demo_UC300ETH_UB1`, apoi pornire cu `Demo_UC400ETH` pentru a verifica lansarea automată a Wizard-ului pe noua placă păstrând configurarea `UB1` intactă, apoi revenire pe `Demo_UC300ETH_UB1` cu reîncărcarea automată a profilului `UB1`).

---

## 4. Specificația Completă Convenită pentru Stage 2 (ATC Setup & Runtime) — Următorii Pași

Toate cerințele de mai jos au fost deja discutate și agreat cu utilizatorul pentru etapa următoare:

### 4.1. Tipuri de Magazie și Capacități Maxime
- **Magazie Liniară:** maxim **20 sloturi**.
- **Magazie Rotativă:** maxim **32 sloturi** (necesită 1 canal de axă liber `A/B/C` sau ieșiri de indexare; în Stage 1 `PinAuditReport` monitorizează canalele de axe libere).

### 4.2. Redesign Ecran ATC (`ScreenTab` / Overlay)
- Grilă pe **2 rânduri a câte 10 sloturi**: rândul de sus sloturile `1–10`, rândul de jos sloturile `11–20`.
- **Vizibilitate dinamică a sloturilor:**
  - Dacă magazia este configurată cu `<= 10` sloturi (ex. `8`), se afișează **un singur rând** cu primele `8` sloturi (`9–10` ascunse complet, nu dezactivate vizual).
  - Dacă magazia este configurată cu `14` sloturi, se afișează rândul de sus complet (`1–10`) și rândul de jos cu `4` sloturi (`11–14`).
- Pentru a face loc grilei pe 2 rânduri:
  - Se **elimină** din pagina principală ATC zona de editare a sculei și zona/butonul `Remove Tool from Spindle`.
  - **Fereastră combinată pe slot (Editare & Schimbare Sculă):** click pe numărul sculei din slot (sau butonul de editare de pe fiecare slot activ) deschide o fereastră unificată care permite atât schimbarea sculei alocate slotului, cât și editarea directă a parametrilor sculei (`Length`, `Diameter`, `Description`, `RH / NRH`), plus butoane pentru **Măsurare (`Measure Tool`)** și **`Apply`**.
  - **Activare / Dezactivare slot din header-ul slotului:** se scoate secțiunea `Slot Management` (checkbox-urile de enable/disable slot) din pagina `ATC Setup`; activarea/dezactivarea fiecărui slot se face prin **click pe header-ul slotului** (zona superioară unde este afișat `Slot #`).

### 4.3. Redesign Ecran `ATC Setup` și Strategii de Senzori (Dependente de Resursele Stage 1)
Spațiul eliberat în `ATC Setup` prin mutarea `Slot Management` va găzdui configurarea **Modului de Monitorizare cu Senzori** și alocarea Porturilor/Pinilor disponibili (calculați din intrările/ieșirile libere raportate de Stage 1 `PinAllocator`):

1. **Mod 0 — `Software Tracking Only` (`0` intrări necesare):**
   - Compatibil cu orice interfață (inclusiv `UC100` cu 5 intrări totale). Nu folosește senzori de prezență; urmărește scula în spindle și în sloturi exclusiv din memorie/Tool Table.
2. **Mod 1 — `Spindle Internal Sensors` (`1–2` intrări + opțional `Air Pressure`):**
   - Standard industrial pentru spindle-uri ATC cu senzori interni (`Drawbar Open / Tool Released`, `Tool Clamped`, opțional `Tool Seated`). Verifică prinderea/eliberarea sculei în spindle la fiecare pas critic.
3. **Mod 2 — `Single Active-Slot Sensor` (`1` intrare):**
   - Ideal pentru **magazia rotativă** (sau sisteme cu senzor în zona de preluare): un singur senzor citește exclusiv slotul adus în poziția de încărcare/descărcare.
4. **Mod 3 — `Per-Slot Presence Sensors` (`1` intrare per slot activ):**
   - Păstrează modul DIY / home-made (un senzor dedicat pe fiecare slot din magazia liniară), însă numărul maxim de sloturi permise în UI se limitează dinamic în funcție de **numărul de intrări libere rămase după Stage 1** (până la maxim `20` sloturi pe plăcile `UC300`/`UC300ETH`).

---

## 5. Arhitectura Planificată pentru Licențiere per Placă (`devserial`)

Funcția `HardwareCatalog.DetectActiveControllerWithSerial` este deja implementată și expune în `ProfileService`:
- `IsDemoMode` (`true` când `Form1.rundemomode == true` sau `userselecteddevtype` începe cu `Demo_`)
- `DeviceSerial` (`"DEMO-MODE"` în demo; altfel `Form1.devserial` — seria unică a plăcii fizice)
- `BaseHardwareFamily` (`UC100 (USB)`, `UC400ETH (Ethernet)`, `AXBB-E (Ethernet)`, `UC300 (USB)`, `UC300ETH (Ethernet)`)

**Regulă stabilită pentru etapa viitoare de licențiere:**
- **În Demo Mode:** rulează **nerestricționat pe orice interfață hardware selectată** pentru testare/preconfigurare.
- **Pe mașina live (`IsDemoMode == false`):** cere licență semnată criptografic asociată cu `DeviceSerial` (și opțional cu `BaseHardwareFamily`, astfel încât schimbarea breakout-ului pe aceeași placă `UC300ETH` să păstreze licența validă), oferind un mod **Trial** limitat fie ca **ore de funcționare**, fie ca **număr de operațiuni / cicluri ATC**, contorizat persistent per `DeviceSerial`.

---

## 6. Compilare și Instalare pe Alt Calculator

### 6.1. Cerințe și Compilare din Sursă (Windows sau Linux)
Necesită **.NET SDK** (`.NET 6/8 SDK` cu target `net48` prin pachetul NuGet `Microsoft.NETFramework.ReferenceAssemblies`):

```bash
git clone -b feature/htm-pro-stage1 https://github.com/Haimana/HTM_ATC.git HTM_PRO
cd HTM_PRO
dotnet build -c Release Plugin/HTM_PRO.csproj
```

Output-ul compilat va fi generat în:
- `Plugin/bin/Release/net48/HTM_PRO.dll`

### 6.2. Instalare Rapidă în UCCNC (`C:\UCCNC\` pe Windows sau prefix Wine/Bottles)
1. Copiază `UCCNC_Install_Ready/Plugins/HTM_PRO.dll` (sau DLL-ul compilat) în `<UCCNC>\Plugins\HTM_PRO.dll`.
2. *(Pentru etapa Stage 2 / asset-uri grafice)* Copiază conținutul `UCCNC_Install_Ready/Flashscreen/` peste `<UCCNC>\Flashscreen\` și macro-urile punte `UCCNC_Install_Ready/Profiles/Macro_Bridge/` (`M6.txt`, `M200.txt`) în `<UCCNC>\Profiles\Macro_<ProfilulTau>\`.
3. *(Opțional, profil de referință testat)* `UCCNC_Install_Ready/Profiles/HTM_PRO_Config_HTM_PRO_Test_UC300ETH_UB1.json`.
4. Pornește UCCNC -> alege unitatea de măsură la instalarea inițială UCCNC (`Metric mm` sau `Imperial inch`), selectează controllerul dorit (`Demo_*` sau placa reală) -> `CONFIGURATION -> GENERAL SETTINGS -> Configure plugins` -> bifează **`Enabled`** și **`Call startup`** pentru **`HTM_PRO.dll`** -> `Save settings` -> repornește UCCNC.
5. Wizard-ul Stage 1 pornește automat dacă profilul nu este încă configurat pentru interfața activă și poate fi redeschis oricând din **`CONFIGURATION -> HTM_PRO WIZARD`**.
