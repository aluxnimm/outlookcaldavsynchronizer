# <img src="https://gitlab.com/Wacton/Unicolour/-/raw/main/Unicolour/Resources/Unicolour.png" width="32" height="32"> Unicolour
[![pipeline status](https://gitlab.com/Wacton/Unicolour/badges/main/pipeline.svg)](https://gitlab.com/Wacton/Unicolour/-/commits/main)
[![coverage report](https://gitlab.com/Wacton/Unicolour/badges/main/coverage.svg)](https://gitlab.com/Wacton/Unicolour/-/pipelines)
[![tests passed](https://badgen.net/https/waacton.npkn.net/gitlab-test-badge/)](https://gitlab.com/Wacton/Unicolour/-/pipelines)
[![NuGet](https://badgen.net/nuget/v/Wacton.Unicolour?icon)](https://www.nuget.org/packages/Wacton.Unicolour/)

Unicolour is a .NET library written in C# for working with colour:
- Colour space conversion
- Colour mixing / colour interpolation
- Colour comparison
- Colour information
- Colour gamut mapping

Targets [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0) for use in .NET 5.0+, .NET Core 2.0+ and .NET Framework 4.6.1+ applications.

**Contents**
1. 🧭 [Overview](#-overview)
2. ⚡ [Quickstart](#-quickstart)
3. 🔦 [Features](#-features)
4. 🌈 [How to use](#-how-to-use)
5. ✨ [Examples](#-examples)
6. 💡 [Configuration](#-configuration)
7. 🔮 [Datasets](#-datasets)
8. 🦺 [Work in progress](#-work-in-progress)

## 🧭 Overview
A `Unicolour` encapsulates a single colour and its representation across different colour spaces.
It supports:
- RGB
- Linear RGB
- HSB/HSV
- HSL
- HWB
- CIEXYZ
- CIExyY
- CIELAB
- CIELCh<sub>ab</sub>
- CIELUV
- CIELCh<sub>uv</sub>
- HSLuv
- HPLuv
- IC<sub>T</sub>C<sub>P</sub>
- J<sub>z</sub>a<sub>z</sub>b<sub>z</sub>
- J<sub>z</sub>C<sub>z</sub>h<sub>z</sub>
- Oklab
- Oklch
- CIECAM02
- CAM16
- HCT

<details>
<summary>Diagram of colour space relationships</summary>

```mermaid
%%{
  init: {
  "theme": "base",
  "themeVariables": {
    "primaryColor": "#4C566A",
    "primaryTextColor": "#ECEFF4",
    "primaryBorderColor": "#2E3440",
    "lineColor": "#8FBCBB",
    "secondaryColor": "#404046",
    "tertiaryColor": "#404046"
    }
  }
}%%

flowchart TD
  XYY(xyY)
  RGBLIN(Linear RGB)
  RGB(RGB)
  HSB(HSB)
  HSL(HSL)
  HWB(HWB)
  XYZ(XYZ)
  LAB(LAB)
  LCHAB(LCHab)
  LUV(LUV)
  LCHUV(LCHuv)
  HSLUV(HSLuv)
  HPLUV(HPLuv)
  ICTCP(ICtCp)
  JZAZBZ(JzAzBz)
  JZCZHZ(JzCzHz)
  OKLAB(Oklab)
  OKLCH(Oklch)
  CAM02(CAM02)
  CAM02UCS(CAM02-UCS)
  CAM16(CAM16)
  CAM16UCS(CAM16-UCS)
  HCT(HCT)

  XYZ --> XYY
  XYZ --> RGBLIN
  RGBLIN --> RGB
  RGB --> HSB
  HSB --> HSL
  HSB --> HWB
  XYZ --> LAB
  LAB --> LCHAB
  XYZ --> LUV
  LUV --> LCHUV
  LCHUV --> HSLUV
  LCHUV --> HPLUV
  XYZ --> ICTCP
  XYZ --> JZAZBZ
  JZAZBZ --> JZCZHZ
  XYZ --> OKLAB
  OKLAB --> OKLCH
  XYZ --> CAM02
  CAM02 -.-> CAM02UCS
  XYZ --> CAM16
  CAM16 -.-> CAM16UCS
  XYZ --> HCT
```

This diagram summarises how colour space conversions are implemented in Unicolour.
Arrows indicate forward transformations from one space to another.
For each forward transformation there is a corresponding reverse transformation.
XYZ is considered the root colour space.
</details>

This library was initially written for personal projects since existing libraries had complex APIs or missing features.
The goal of this library is to be accurate, intuitive, and easy to use.
Although performance is not a priority, conversions are only calculated once; when first evaluated (either on access or as part of an intermediate conversion step) the result is stored for future use.
It is also [extensively tested](Unicolour.Tests), including verification of roundtrip conversions, validation using known colour values, and 100% line coverage and branch coverage.

## ⚡ Quickstart
| Colour&nbsp;space                       | Create                          | Get            |
|-----------------------------------------|---------------------------------|----------------|
| RGB&nbsp;(Hex)                          | `new(hex)`                      | `.Hex`         |
| RGB&nbsp;(0–255)                        | `new(ColourSpace.Rgb255, ⋯)`    | `.Rgb.Byte255` |
| RGB                                     | `new(ColourSpace.Rgb, ⋯)`       | `.Rgb`         |
| Linear&nbsp;RGB                         | `new(ColourSpace.RgbLinear, ⋯)` | `.RgbLinear`   |
| HSB/HSV                                 | `new(ColourSpace.Hsb, ⋯)`       | `.Hsb`         |
| HSL                                     | `new(ColourSpace.Hsl, ⋯)`       | `.Hsl`         |
| HWB                                     | `new(ColourSpace.Hwb, ⋯)`       | `.Hwb`         |
| CIEXYZ                                  | `new(ColourSpace.Xyz, ⋯)`       | `.Xyz`         |
| CIExyY                                  | `new(ColourSpace.Xyy, ⋯)`       | `.Xyy`         |
| CIELAB                                  | `new(ColourSpace.Lab, ⋯)`       | `.Lab`         |
| CIELCh<sub>ab</sub>                     | `new(ColourSpace.Lchab, ⋯)`     | `.Lchab`       |
| CIELUV                                  | `new(ColourSpace.Luv, ⋯)`       | `.Luv`         |
| CIELCh<sub>uv</sub>                     | `new(ColourSpace.Lchuv, ⋯)`     | `.Lchuv`       |
| HSLuv                                   | `new(ColourSpace.Hsluv, ⋯)`     | `.Hsluv`       |
| HPLuv                                   | `new(ColourSpace.Hpluv, ⋯)`     | `.Hpluv`       |
| IC<sub>T</sub>C<sub>P</sub>             | `new(ColourSpace.Ictcp, ⋯)`     | `.Ictcp`       |
| J<sub>z</sub>a<sub>z</sub>b<sub>z</sub> | `new(ColourSpace.Jzazbz, ⋯)`    | `.Jzazbz`      |
| J<sub>z</sub>C<sub>z</sub>h<sub>z</sub> | `new(ColourSpace.Jzczhz, ⋯)`    | `.Jzczhz`      |
| Oklab                                   | `new(ColourSpace.Oklab, ⋯)`     | `.Oklab`       |
| Oklch                                   | `new(ColourSpace.Oklch, ⋯)`     | `.Oklch`       |
| CIECAM02                                | `new(ColourSpace.Cam02, ⋯)`     | `.Cam02`       |
| CAM16                                   | `new(ColourSpace.Cam16, ⋯)`     | `.Cam16`       |
| HCT                                     | `new(ColourSpace.Hct, ⋯)`       | `.Hct`         |

## 🔦 Features
A `Unicolour` can be instantiated using any of the supported colour spaces.
Conversion to other colour spaces is handled by Unicolour, and the results can be accessed through properties.

Two colours can be mixed / interpolated through any colour space, with or without premultiplied alpha.

Colour difference / colour distance can be calculated using various delta E metrics:
- ΔE<sub>76</sub> (CIE76)
- ΔE<sub>94</sub> (CIE94)
- ΔE<sub>00</sub> (CIEDE2000)
- ΔE<sub>CMC</sub> (CMC l:c)
- ΔE<sub>ITP</sub>
- ΔE<sub>z</sub>
- ΔE<sub>HyAB</sub>
- ΔE<sub>OK</sub>
- ΔE<sub>CAM02</sub>
- ΔE<sub>CAM16</sub>
  
The following colour information is available:
- Hex representation
- Relative luminance
- Temperature (CCT and Duv)

Simulation of colour vision deficiency (CVD) / colour blindness is supported for:
- Protanopia (no red perception)
- Deuteranopia (no green perception)
- Tritanopia (no blue perception)
- Achromatopsia (no colour perception)

If a colour is outwith the display gamut, the closest in-gamut colour can be obtained using gamut mapping.
The algorithm implemented in Unicolour conforms to CSS specifications.

Unicolour uses sRGB as the default RGB model and standard illuminant D65 (2° observer) as the default white point of the XYZ colour space.
These [can be overridden](#-configuration) using the `Configuration` parameter.

## 🌈 How to use
1. Install the package from [NuGet](https://www.nuget.org/packages/Wacton.Unicolour/)
```
dotnet add package Wacton.Unicolour
```

2. Import the package
```c#
using Wacton.Unicolour;
```

3. Create a `Unicolour`
```c#
var unicolour = new Unicolour("#FF1493");
var unicolour = new Unicolour(ColourSpace.Rgb255, 255, 20, 147);
var unicolour = new Unicolour(ColourSpace.Rgb, 1.00, 0.08, 0.58);
var unicolour = new Unicolour(ColourSpace.RgbLinear, 1.00, 0.01, 0.29);
var unicolour = new Unicolour(ColourSpace.Hsb, 327.6, 0.922, 1.000);
var unicolour = new Unicolour(ColourSpace.Hsl, 327.6, 1.000, 0.539);
var unicolour = new Unicolour(ColourSpace.Hwb, 327.6, 0.078, 0.000);
var unicolour = new Unicolour(ColourSpace.Xyz, 0.4676, 0.2387, 0.2974);
var unicolour = new Unicolour(ColourSpace.Xyy, 0.4658, 0.2378, 0.2387);
var unicolour = new Unicolour(ColourSpace.Lab, 55.96, 84.54, -5.7);
var unicolour = new Unicolour(ColourSpace.Lchab, 55.96, 84.73, 356.1);
var unicolour = new Unicolour(ColourSpace.Luv, 55.96, 131.47, -24.35);
var unicolour = new Unicolour(ColourSpace.Lchuv, 55.96, 133.71, 349.5);
var unicolour = new Unicolour(ColourSpace.Hsluv, 349.5, 100.0, 56.0);
var unicolour = new Unicolour(ColourSpace.Hpluv, 349.5, 303.2, 56.0);
var unicolour = new Unicolour(ColourSpace.Ictcp, 0.38, 0.12, 0.19);
var unicolour = new Unicolour(ColourSpace.Jzazbz, 0.106, 0.107, 0.005);
var unicolour = new Unicolour(ColourSpace.Jzczhz, 0.106, 0.107, 2.6);
var unicolour = new Unicolour(ColourSpace.Oklab, 0.65, 0.26, -0.01);
var unicolour = new Unicolour(ColourSpace.Oklch, 0.65, 0.26, 356.9);
var unicolour = new Unicolour(ColourSpace.Cam02, 62.86, 40.81, -1.18);
var unicolour = new Unicolour(ColourSpace.Cam16, 62.47, 42.60, -1.36);
var unicolour = new Unicolour(ColourSpace.Hct, 358.2, 100.38, 55.96);
```

4. Get colour space representations
```c#
var rgb = unicolour.Rgb;
var rgbLinear = unicolour.RgbLinear;
var hsb = unicolour.Hsb;
var hsl = unicolour.Hsl;
var hwb = unicolour.Hwb;
var xyz = unicolour.Xyz;
var xyy = unicolour.Xyy;
var lab = unicolour.Lab;
var lchab = unicolour.Lchab;
var luv = unicolour.Luv;
var lchuv = unicolour.Lchuv;
var hsluv = unicolour.Hsluv;
var hpluv = unicolour.Hpluv;
var ictcp = unicolour.Ictcp;
var jzazbz = unicolour.Jzazbz;
var jzczhz = unicolour.Jzczhz;
var oklab = unicolour.Oklab;
var oklch = unicolour.Oklch;
var cam02 = unicolour.Cam02;
var cam16 = unicolour.Cam16;
var hct = unicolour.Hct;
```

5. Get colour properties
```c#
var hex = unicolour.Hex;
var relativeLuminance = unicolour.RelativeLuminance;
var temperature = unicolour.Temperature;
var inGamut = unicolour.IsInDisplayGamut;
```

6. Mix colours (interpolate between them)
```c#
var mixed = unicolour1.Mix(ColourSpace.Rgb, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.RgbLinear, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Hsb, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Hsl, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Hwb, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Xyz, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Xyy, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Lab, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Lchab, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Luv, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Lchuv, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Hsluv, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Hpluv, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Ictcp, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Jzazbz, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Jzczhz, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Oklab, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Oklch, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Cam02, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Cam16, unicolour2);
var mixed = unicolour1.Mix(ColourSpace.Hct, unicolour2);
```

7. Compare colours
```c#
var contrast = unicolour1.Contrast(unicolour2);
var difference = unicolour1.Difference(DeltaE.Cie76, unicolour2);
var difference = unicolour1.Difference(DeltaE.Cie94, unicolour2);
var difference = unicolour1.Difference(DeltaE.Cie94Textiles, unicolour2);
var difference = unicolour1.Difference(DeltaE.Ciede2000, unicolour2);
var difference = unicolour1.Difference(DeltaE.CmcAcceptability, unicolour2);
var difference = unicolour1.Difference(DeltaE.CmcPerceptibility, unicolour2);
var difference = unicolour1.Difference(DeltaE.Itp, unicolour2);
var difference = unicolour1.Difference(DeltaE.Z, unicolour2);
var difference = unicolour1.Difference(DeltaE.Hyab, unicolour2);
var difference = unicolour1.Difference(DeltaE.Ok, unicolour2);
var difference = unicolour1.Difference(DeltaE.Cam02, unicolour2);
var difference = unicolour1.Difference(DeltaE.Cam16, unicolour2);
```

8. Map colour to display gamut
```c#
var mapped = unicolour.MapToGamut();
```

9. Simulate colour vision deficiency
```c#
var protanopia = unicolour.SimulateProtanopia();
var deuteranopia = unicolour.SimulateDeuteranopia();
var tritanopia = unicolour.SimulateTritanopia();
var achromatopsia = unicolour.SimulateAchromatopsia();
```

## ✨ Examples
This repo contains an [example project](Unicolour.Example/Program.cs) that uses `Unicolour` to:
1. Generate gradients through different colour spaces
2. Render the colour spectrum with different colour vision deficiencies
3. Demonstrate interpolation with and without premultiplied alpha

![Gradients through different colour spaces generated from Unicolour](Unicolour.Example/gradients.png)

![Gradients for different colour vision deficiencies generated from Unicolour](Unicolour.Example/vision-deficiency.png)

![Interpolation from red to transparent to blue, with and without premultiplied alpha](Unicolour.Example/alpha-interpolation.png)

There is also a [console application](Unicolour.Console/Program.cs) that uses `Unicolour` to show colour information for a given hex value:

![Colour information from hex value](Unicolour.Console/colour-info.png)

## 💡 Configuration
A `Configuration` parameter can be used to customise the RGB model (e.g. Display P3, Rec. 2020)
and the white point of the XYZ colour space (e.g. D50 reference white used by ICC profiles).

- RGB configuration requires red, green, and blue chromaticity coordinates, the reference white point, and the companding functions.
  Default configuration for sRGB, Display P3, and Rec. 2020 is provided.

- XYZ configuration only requires the reference white point.
  Default configuration for D65 and D50 (2° observer) is provided.

```c#
// built-in configuration for Rec. 2020 RGB + D65 XYZ
var config = new Configuration(RgbConfiguration.Rec2020, XyzConfiguration.D65);
var unicolour = new Unicolour(ColourSpace.Rgb255, config, 204, 64, 132);
```

```c#
// manual configuration for wide-gamut RGB
var rgbConfig = new RgbConfiguration(
    chromaticityR: new(0.7347, 0.2653),
    chromaticityG: new(0.1152, 0.8264),
    chromaticityB: new(0.1566, 0.0177),
    whitePoint: WhitePoint.From(Illuminant.D50),
    fromLinear: value => Companding.Gamma(value, 2.19921875),
    toLinear: value => Companding.InverseGamma(value, 2.19921875)
);

// manual configuration for Illuminant C (10° observer) XYZ
var xyzConfig = new XyzConfiguration(
    whitePoint: WhitePoint.From(Illuminant.C, Observer.Supplementary10)
);

var config = new Configuration(rgbConfig, xyzConfig);
var unicolour = new Unicolour(ColourSpace.Rgb255, config, 255, 20, 147);
```

Configuration is also available for CAM02 & CAM16 viewing conditions,
IC<sub>T</sub>C<sub>P</sub> scalar,
and J<sub>z</sub>a<sub>z</sub>b<sub>z</sub> scalar.

The default white point used by all colour spaces is D65.
This table lists which `Configuration` property determines the white point of each colour space.

| Colour&nbsp;space                       | White&nbsp;point&nbsp;configuration |
|-----------------------------------------|-------------------------------------|
| RGB                                     | `RgbConfiguration`                  |
| Linear&nbsp;RGB                         | `RgbConfiguration`                  |
| HSB/HSV                                 | `RgbConfiguration`                  |
| HSL                                     | `RgbConfiguration`                  |
| HWB                                     | `RgbConfiguration`                  |
| CIEXYZ                                  | `XyzConfiguration`                  |
| CIExyY                                  | `XyzConfiguration`                  |
| CIELAB                                  | `XyzConfiguration`                  |
| CIELUV                                  | `XyzConfiguration`                  |
| CIELCh<sub>uv</sub>                     | `XyzConfiguration`                  |
| HSLuv                                   | `XyzConfiguration`                  |
| HPLuv                                   | `XyzConfiguration`                  |
| IC<sub>T</sub>C<sub>P</sub>             | None (always D65)                   |
| J<sub>z</sub>a<sub>z</sub>b<sub>z</sub> | None (always D65)                   |
| J<sub>z</sub>C<sub>z</sub>h<sub>z</sub> | None (always D65)                   |
| Oklab                                   | None (always D65)                   |
| Oklch                                   | None (always D65)                   |
| CIECAM02                                | `CamConfiguration`                  |
| CAM16                                   | `CamConfiguration`                  |
| HCT                                     | None (always D65)                   |

A `Unicolour` can be converted to a different configuration, which enables conversions between different RGB and XYZ models.

```c#
// pure sRGB green
var srgbConfig = new Configuration(RgbConfiguration.StandardRgb);
var unicolourSrgb = new Unicolour(ColourSpace.Rgb, srgbConfig, 0, 1, 0);                         
Console.WriteLine(unicolourSrgb.Rgb); // 0.00 1.00 0.00

// ⟶ Display P3
var displayP3Config = new Configuration(RgbConfiguration.DisplayP3);
var unicolourDisplayP3 = unicolourSrgb.ConvertToConfiguration(displayP3Config); 
Console.WriteLine(unicolourDisplayP3.Rgb); // 0.46 0.99 0.30

// ⟶ Rec. 2020
var rec2020Config = new Configuration(RgbConfiguration.Rec2020);
var unicolourRec2020 = unicolourDisplayP3.ConvertToConfiguration(rec2020Config);
Console.WriteLine(unicolourRec2020.Rgb); // 0.57 0.96 0.27
```

## 🔮 Datasets
Some colour datasets have been compiled for convenience and are available as a [NuGet package](https://www.nuget.org/packages/Wacton.Unicolour.Datasets/).

Commonly used sets of colours:
- [CSS specification](https://www.w3.org/TR/css-color-4/#named-colors) named colours
- [Macbeth ColorChecker](https://en.wikipedia.org/wiki/ColorChecker) colour rendition chart

Colour data used in academic literature:
- [Hung-Berns](https://doi.org/10.1002/col.5080200506) constant hue loci data
- [Ebner-Fairchild](https://doi.org/10.1117/12.298269) constant perceived-hue data

Example usage:

1. Install the package from [NuGet](https://www.nuget.org/packages/Wacton.Unicolour.Datasets/)
```
dotnet add package Wacton.Unicolour.Datasets
```

2. Import the package
```c#
using Wacton.Unicolour.Datasets;
```

3. Reference the predefined `Unicolour`
```c#
var unicolour = Css.DeepPink;
```

## 🦺 Work in progress
Version 4 of Unicolour is in development and aims to provide more new features:
- 🌡️ Create a `Unicolour` from temperature (CCT and Duv)
- 🎯 More accurate calculation of temperature (CCT and Duv) 
- 📈 Create a `Unicolour` from a spectral power distribution
- 🚥 More modes of hue interpolation
- 🎨 More default RGB models (e.g. A98, ProPhoto)

---

[Wacton.Unicolour](https://github.com/waacton/Unicolour) is licensed under the [MIT License](https://choosealicense.com/licenses/mit/), copyright © 2022-2023 William Acton.
