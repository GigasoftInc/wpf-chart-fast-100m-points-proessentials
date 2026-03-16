# GigaPrime2D WPF — 100 Million Point WPF Chart

A ProEssentials v10 WPF .NET 8 demonstration of GPU compute shader rendering — 
100 million data points completely re-passed and re-rendered per timer tick.
Live FPS displayed in the title bar.

![GigaPrime2D WPF 100 Million Points](https://gigasoft.com/files/GigaPrime2D-100MPoints-Wpf.png)

---

## What This Demonstrates

GigaPrime2D WPF demonstrates ProEssentials v10 GPU compute shader rendering of
100 million data points per update in a WPF .NET 8 application.

- **5 subsets × 20,000,000 points = 100M data points per update**
- **GPU compute shaders** process all data in parallel on the GPU
- **GPU compute shader render time: ~15ms**
- **End-to-end frame rate: ~17 FPS** on a typical development workstation
  with dedicated GPU (includes 100M point data transfer overhead)
- **Zero memory copy** — chart receives a pointer to `fYDataToChart` directly
  via `UseDataAtLocation`. Changing the array contents is all that is needed
  to update the chart.
- **Live FPS counter** displayed in the window title bar

---

## How It Works

### Data Architecture

```csharp
// 120M point pool — prepared once at startup
float[] fYDataPool = new float[120010000];

// 100M point buffer — pointer passed directly to chart
// Chart forwards this to GPU compute shaders each render
float[] fYDataToChart = new float[100000000];
```

On each timer tick, `Array.Copy` moves 100M points from a random offset
in `fYDataPool` into `fYDataToChart` producing variation. The chart renders
the new data immediately via GPU compute shaders.

### v10 GPU Compute Shader Settings

```csharp
// v10 new feature — builds the scene on the GPU vs CPU
Pesgo1.PeData.ComputeShader  = true;  // GPU-side chart construction
Pesgo1.PeData.Filter2D3D     = true;  // set with ComputeShader + Line plotting
Pesgo1.PeData.StagingBufferY = true;  // always set for ComputeShader
Pesgo1.PeData.StagingBufferX = true;  // always set for ComputeShader
```

### Five Independent Axes

Each of the 5 signal channels gets its own Y axis lane via `MultiAxesSubsets`.
The UI lets you combine, hide, highlight, and resize axes interactively.

---

## WPF vs WinForms Performance

WinForms has a slight performance edge as Direct3D is directly coupled to the
window device context, avoiding the texture compositing step that WPF requires.
Both versions use identical GPU compute shaders and achieve comparable frame rates.

| Version | Render Time | End-to-End FPS |
|---------|-------------|----------------|
| WinForms | ~15ms | ~20 FPS |
| WPF | ~15ms | ~17 FPS |

For maximum real-time throughput see the WinForms version:
➡️ [winforms-chart-100million-points-proessentials](https://github.com/GigasoftInc/winforms-chart-100million-points-proessentials)

---

## Interactive Controls

- **Start/Stop Timer** — enables 100M point continuous re-rendering
- **Mouse wheel** — zooms X axis range
- **Zoom X Axes slider** — programmatic zoom control
- **Combine Axes** — overlaps all 5 signals into one shared graph area
- **Hide Overlapped Axes** — collapses to single combined Y axis label
- **Highlight Signal 1-5** — expands selected channel to 80% of height
- **Show Legend** — toggles legend display
- **Right-click** — full built-in context menu including zoom reset

---

## Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- Internet connection for NuGet restore
- Dedicated GPU recommended

> **Designer Support:** Visual Studio designer requires the full
> ProEssentials installation. The project builds and runs correctly
> via NuGet without a full installation.

---

## How to Run

```
1. Clone this repository
2. Open GigaPrime2D_WPF.sln in Visual Studio 2022
3. Build → Rebuild Solution (restores NuGet package automatically)
4. Press F5
5. Check Start/Stop Timer to begin 100M point rendering
6. Watch live FPS in the title bar
```

---

## NuGet Package

This project references
[`ProEssentials.Chart.Net80.x64.Wpf`](https://www.nuget.org/packages/ProEssentials.Chart.Net80.x64.Wpf)
from nuget.org. Package restore happens automatically on build.

---

## Related

- [WinForms version — winforms-chart-100million-points-proessentials](https://github.com/GigasoftInc/winforms-chart-100million-points-proessentials)
- [Plot 100 Million Points — 5-Library Comparison](https://gigasoft.com/blog/plot-100-million-points-wpf-comparison)
- [Performance — GPU Architecture Comparison](https://gigasoft.com/why-proessentials/performance)
- [No-hassle evaluation download](https://gigasoft.com/net-chart-component-wpf-winforms-download)
- [gigasoft.com](https://gigasoft.com)

---

## License

Example code is MIT licensed. ProEssentials requires a commercial
license for continued use.
