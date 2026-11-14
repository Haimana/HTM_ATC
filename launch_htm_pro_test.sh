#!/usr/bin/env bash
set -euo pipefail
exec flatpak run --command=bottles-cli com.usebottles.bottles run -b UCCNC_1.2118 -p "UCCNC - HTM_PRO Test"
