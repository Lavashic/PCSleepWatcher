# PC Sleep Watcher 💤

![App Icon](./PCSleepWatcher/Assets/icon_on_vector.svg)

[![Build and Release](https://github.com/Lavashic/PCSleepWatcher/actions/workflows/release.yml/badge.svg)](https://github.com/Lavashic/PCSleepWatcher/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)


> A simple tool that monitors your PC's sleep state and automatically sends it back to sleep if no authorized activity is detected. Runs in the system tray for easy control.

## Features ✨
- Monitors sleep state at regular intervals
- System tray integration for quick access
- Lightweight and unobtrusive
- Includes on and off powershell scripts to pause and resume execution (good to setup for auto execution if using streaming apps like Moonlight/Sunshine)

## Installation 📥
### Option 1: Self-contained (No .NET required)
1. Download the latest release from the [Releases page](https://github.com/Lavashic/PCSleepWatcher/releases)
2. Extract the archive
3. Run `PCSleepWatcher.exe`

### Option 2: Framework-dependent (Requires .NET 9)
1. Download and install .NET 9 Runtime
2. Download the latest release from the [Releases page](https://github.com/Lavashic/PCSleepWatcher/releases)
3. Extract the archive
4. Run `PCSleepWatcher.exe`