# ShaderSaver Deployment Guide

## Overview
ShaderSaver can be deployed in two different ways, depending on your needs:

## Option 1: Lightweight Version (Requires .NET Runtime)
**File**: `ShaderSaver.scr` (~150 KB)

### Features:
- Small file size
- All shader files embedded as resources
- Single file deployment
- Requires .NET 6.0 Runtime on target machine

### Requirements:
- .NET 6.0 Runtime must be installed on the target system
- Download from: https://dotnet.microsoft.com/download/dotnet/6.0

### Installation:
1. Copy `ShaderSaver.scr` to any location
2. Right-click and select "Install" to add to Windows screensaver list
3. Or run directly: `ShaderSaver.scr /s` for fullscreen mode

## Option 2: Standalone Version (No Dependencies)
**File**: `ShaderSaver_Standalone.scr` (~150 MB)

### Features:
- Completely self-contained
- No external dependencies required
- Includes all .NET runtime and native libraries
- All shader files embedded as resources
- Single file deployment

### Requirements:
- None - runs on any Windows 10/11 system

### Installation:
1. Copy `ShaderSaver_Standalone.scr` to any location
2. Right-click and select "Install" to add to Windows screensaver list
3. Or run directly: `ShaderSaver_Standalone.scr /s` for fullscreen mode

## Embedded Shaders
Both versions include 6 built-in shader effects:
1. **Singularity** - Black hole with gravitational lensing
2. **Plasma Energy Field** - Colorful energy waves
3. **Tunnel Vortex** - 3D tunnel effect
4. **Origami** - Rotating geometric art by @XorDev
5. **Shield** - Hexagonal energy shield by @XorDev
6. **Ghosts** - 3D volumetric spirits by @XorDev

Shaders cycle automatically every 10 seconds.

## Command Line Options
- `/s` - Run screensaver in fullscreen mode
- `/p [hwnd]` - Preview mode (for Windows screensaver settings)
- `/c` - Configuration mode (opens settings dialog)

## Features
- GPU-accelerated GLSL rendering
- Multi-monitor support
- Proper screensaver behavior (exits on input)
- Automatic cursor hiding
- Graceful fallback to software rendering if OpenGL fails

## Troubleshooting
- If shaders don't load properly, the screensaver will fall back to a default colorful swirl effect
- For systems without modern OpenGL support, a simple gradient fallback is provided
- Check Windows Event Viewer for any error messages if the screensaver fails to start