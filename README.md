# EasySave

EasySave is a console application built with C#, using the .NET Framework and DLLs. It allows users to create up to 5 backup jobs (full or differential), track progress in real time, and log all file transfers in JSON format.

# context

The project was created to provide a lightweight and flexible backup solution. It includes:

Full & differential backup support

Real-time status tracking 

Daily logs (C:\ProgramData\EasySave\Logs\YYYY-MM-DD.json)

Multilingual support (FR/EN)

Modular design for future GUI (v2.0)

Note: you can find Log and state files in:
C:\ProgramData\EasySave\ For windows
/Library/Application Support/EasySave/ For macos
/usr/share/EasySave/ For linux

# Prerequisites

Requirements:

Windows OS

.NET Framework 4.7.2+

Visual Studio

# Installation

If you have git, you can clone the repo with this command (or you can directly download it as Zip):

git clone git clone https://github.com/faresbnm/EasySave.git

# Features

EasySave v1.0

- Console-based interface
- Support for up to 5 backup jobs
- Two backup types
- Execution options
- Real-time state tracking
- Daily JSON log generation
- Validation of inputs
- Multilingual-ready
- Modular design with DLL
