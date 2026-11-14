# `HTM_PRO` — Unified Machine Setup Wizard & ATC Plugin for UCCNC

`HTM_PRO` (`v1.0.0`) este un plugin C# nativ (`.NET Framework 4.8`) pentru **UCCNC**, dezvoltat în două stadii:

- **Stage 1 (Implementat):** Wizard interactiv în 5 pași pentru configurarea completă a mașinii CNC la prima pornire UCCNC (axe `X/Y/Z/A/B/C`, pairing Master/Slave pe orice axă, alocare inteligentă pini Step/Dir/Enable/Home/Limits/Spindle/Coolant/Probe, grupare pini Home `Individual / Common Master & Common Slave / Single Common`, unități read-only din instalarea UCCNC, stocare separată per controller hardware și analiză de resurse libere).
- **Stage 2 (Planificat):** Configurare & Runtime ATC (magazie liniară până la 20 sloturi pe 2 rânduri dinamice / rotativă până la 32 sloturi, moduri de senzori adaptate pinilor liberi din Stage 1, editare scule direct din slot).

### Documentație completă pentru continuare și arhitectură
Consultă **[HANDOVER.md](HANDOVER.md)** pentru:
- Descrierea detaliată a tuturor modulelor din [`Plugin/src/`](Plugin/src/)
- Specificația completă agreată pentru Stage 2 (UI & senzori)
- Arhitectura planificată pentru licențierea per placă (`devserial`)
- Instrucțiunile complete de compilare și instalare în UCCNC (`UCCNC_Install_Ready/`)

### Compilare rapidă
```bash
dotnet build -c Release Plugin/HTM_PRO.csproj
```
