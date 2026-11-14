#!/usr/bin/env bash
set -euo pipefail
exec flatpak run --command=bottles-cli com.usebottles.bottles run -b UCCNC_1.2118 -e 'C:\UCCNC\2118\UCCNC.exe' -- /p HTM_PRO_Test
