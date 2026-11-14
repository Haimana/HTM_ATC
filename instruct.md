# Project Instructions — `HTM_PRO` (UCCNC Stage 1 + Stage 2 Plugin)

1. **Context obligatoriu la pornirea oricărei sesiuni:**
   - Citește mai întâi [`HANDOVER.md`](HANDOVER.md) și [`README.md`](README.md) din rădăcina proiectului. Acolo sunt documentate complet:
     - Arhitectura **Stage 1** (Complete Machine Setup Wizard, auto-detectare unități read-only, stocare separată per interfață hardware, grupare pini Home `Individual / CommonMasterAndCommonSlave / SingleCommonAllAxes`, pairing Master/Slave pe orice axă `X/Y/Z`).
     - Specificația completă agreată cu utilizatorul pentru **Stage 2** (ATC Setup & Runtime: magazie liniară până la 20 sloturi pe 2 rânduri dinamice, rotativă până la 32 sloturi, modurile de senzori `0..3`, editare sculă & schimbare sculă din slot, activare/dezactivare slot din header).
     - Structura planificată pentru licențierea per placă (`DeviceSerial` + `BaseHardwareFamily`, Demo Mode nerestricționat vs Live Hardware Trial/License).

2. **Reguli de lucru în acest repository:**
   - Răspunde în chat **în limba română** (dacă utilizatorul nu cere altfel).
   - Scrie toate comentariile descriptive și XML docstrings din codul C# **în limba engleză**.
   - Nu face presupuneri când lipsesc informații de hardware sau cablare; întreabă utilizatorul înainte.
   - Păstrează intacte proiectul de referință `/home/seb/HTM_ATC` și arhiva `/home/seb/HTM_ATC_Handover.zip` (nu le modifica).
   - Nu comite directoarele `bin/`, `obj/` sau fișierele `.log` (verifică `.gitignore`); menține [`UCCNC_Install_Ready/Plugins/HTM_PRO.dll`](UCCNC_Install_Ready/Plugins/HTM_PRO.dll) sincronizat după build-uri majore Release (`dotnet build -c Release Plugin/HTM_PRO.csproj`).
