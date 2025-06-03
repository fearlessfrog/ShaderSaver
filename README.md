# Shader Screensaver

A Windows screensaver that renders beautiful GLSL shaders with automatic cycling between built-in effects.

## Features

- **12 Built-in Shader Effects** - Includes Singularity, Sunset, Starship, Origami, Shield, Ghosts, Waveform, Water Ripples, Simplex, Terraform, DNA, and Rainbow Road
- **Automatic Shader Cycling** - Cycles between effects every 10 seconds (configurable)
- **Full screen and multi-monitor support** - Works across all connected monitors
- **Real-time rendering** - Smooth 60 FPS animation with proper shader uniforms
- **Easy configuration** - Simple settings dialog for cycle timing
- **Two deployment options** - Lightweight or standalone versions

## Requirements

- Windows 10/11
- OpenGL 3.3 compatible graphics card
- For lightweight version: .NET 6.0 Runtime
- For building from source: Visual Studio 2022 or .NET 6.0 SDK

## Installation

### Option 1: Lightweight Version (~150 KB)
**Requirements**: .NET 6.0 Runtime must be installed
1. Download `ShaderSaver.scr` from the releases page
2. Right-click on `ShaderSaver.scr` and select "Install"
3. Or run directly: `ShaderSaver.scr /s` for fullscreen mode

### Option 2: Standalone Version (~150 MB)
**Requirements**: None - completely self-contained
1. Download `ShaderSaver_Standalone.scr` from the releases page
2. Right-click on `ShaderSaver_Standalone.scr` and select "Install"
3. Or run directly: `ShaderSaver_Standalone.scr /s` for fullscreen mode

### Option 3: Build from Source
1. Clone this repository
2. Open in Visual Studio 2022 or use the command line:
   ```bash
   dotnet build --configuration Release
   ```
3. The lightweight screensaver file will be generated in `bin\Release\net6.0-windows\ShaderSaver.scr`
4. For standalone version:
   ```bash
   dotnet publish --configuration Release --self-contained true --runtime win-x64 -p:PublishSingleFile=true
   ```

## Usage

1. After installation, go to Windows Settings > Personalization > Lock screen > Screen saver settings
2. Select "ShaderSaver" from the dropdown
3. Click "Settings" to configure the cycle timing
4. Click "Preview" to test the screensaver

## Built-in Shader Effects

The screensaver includes 12 stunning shader effects that cycle automatically:

1. **Singularity** - Black hole with gravitational lensing effect
2. **Sunset** - Volumetric clouds and atmospheric effects
3. **Starship** - Particle debris inspired by SpaceX tests
4. **Origami** - Soft paper-like geometric shapes
5. **Shield** - Hexagonal energy shield patterns
6. **Ghosts** - 3D volumetric spirits with turbulence
7. **Waveform** - Audio-inspired wave patterns and raymarching
8. **Water Ripples** - Realistic water simulation with drops and reflections
9. **Simplex** - Geometric grid patterns and mathematical shapes
10. **Terraform** - Procedural terrain generation with dynamic landscapes
11. **DNA** - Double helix molecular visualization with realistic lighting
12. **Rainbow Road** - Infinite rainbow highway with perspective effects

## Technical Details

### Shader Uniforms
The screensaver provides these Shadertoy-compatible uniforms to all shaders:
- `vec3 iResolution` - Viewport resolution
- `float iTime` - Current time in seconds
- `float iTimeDelta` - Render time per frame
- `int iFrame` - Current frame number
- `float iFrameRate` - Frames per second
- `vec4 iMouse` - Mouse coordinates (not used in screensaver mode)
- `vec4 iDate` - Current date and time
- `float iSampleRate` - Audio sample rate (44100)

### Command Line Options
- `/s` - Run screensaver in fullscreen mode
- `/p [hwnd]` - Preview mode (for Windows screensaver settings)
- `/c` - Configuration mode (opens settings dialog)

### Architecture
- **Program.cs** - Entry point and command-line argument parsing
- **ShaderForm.cs** - Main rendering window with multi-monitor support
- **ShaderRenderer.cs** - OpenGL shader compilation and rendering
- **ShaderLoader.cs** - Embedded shader loading and Shadertoy format conversion
- **SettingsForm.cs** - Configuration dialog for cycle timing

## Troubleshooting

### Screensaver doesn't appear
- Ensure you have OpenGL 3.3 compatible graphics drivers
- Update your graphics drivers to the latest version
- The screensaver will automatically fall back to a simple gradient if OpenGL fails

### Performance issues
- Some complex shaders may run slowly on older graphics cards
- Ensure your graphics drivers are up to date
- The screensaver includes graceful fallbacks for older hardware

### General issues
- Check Windows Event Viewer for any error messages if the screensaver fails to start
- For the lightweight version, ensure .NET 6.0 Runtime is installed
- Try the standalone version if you encounter dependency issues

## Development

### Adding New Shaders
To add new built-in shaders:
1. Create a new `.glsl` file in the `Shaders/` directory
2. Add it to the `EmbeddedResource` section in `ShaderSaver.csproj`
3. Update the shader loading logic in `ShaderLoader.cs`

### Shader Format
All shaders should use the Shadertoy `mainImage` function format:
```glsl
void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    vec3 color = vec3(uv, 0.5 + 0.5 * sin(iTime));
    fragColor = vec4(color, 1.0);
}
```

## Contributing

Feel free to submit pull requests or open issues for:
- Bug fixes
- Performance improvements
- New shader effects
- Additional Shadertoy features
- UI improvements

## License

This project is open source and available under the MIT License.

## Acknowledgments

- Inspired by [Shadertoy.com](https://www.shadertoy.com/)
- Uses [OpenTK](https://opentk.net/) for OpenGL bindings
- Special thanks to @XorDev for contributing beautiful shader effects
- Based on Shadertoy's uniform system and rendering approach