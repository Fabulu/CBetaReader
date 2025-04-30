@echo off
SETLOCAL

echo Setting up embedded Python + Argos Translate...

:: Create Python folder if needed
if not exist Python mkdir Python

:: Download Minimized Python (optional – skipped here)
:: If already using a bundled Python interpreter, just install packages

cd Python

:: Install pip if not available
if not exist get-pip.py (
    echo Downloading get-pip.py...
    curl -O https://bootstrap.pypa.io/get-pip.py
)

:: Install pip
.\python.exe get-pip.py

:: Install Argos Translate and dependencies
.\python.exe -m pip install argostranslate

:: Install Chinese Traditional to English model
echo Installing translation model...
.\python.exe -m argostranslate.package update
.\python.exe -m argostranslate.package install translate-zt_en

echo Done setting up Python environment.
pause
