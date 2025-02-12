[![openupm](https://img.shields.io/npm/v/com.am1goo.spritesheet3000?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.am1goo.spritesheet3000/)

# Spritesheet 3000 for Unity
Custom pipeline plugin about how to make work together Adobe Photoshop's files with Unity Engine's sprites and sprite atlases.
<p align="left">
  <img src="Readme/header-image.gif" alt="header-image"/>
</p>

#### Unity Plugin
The latest version can be installed via [package manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) using following git URL: \
`https://github.com/am1goo/unity-plugin-spritesheet-3000.git#0.4.26`

## Getting started
#### Adobe Photoshop CC Extension
- **Install extentions via Unity Editor menu**\
in top menu find and click `Spritesheet 3000 -> Install extensions -> Adobe Photoshop CC`
<p align="left">
  <img src="Readme/install-photoshop-extension.png" alt="install-photoshop-extension"/>
</p>

or 

- **Install extentions by yourself**\
in top menu find and click `Spritesheet 3000 -> Open extensions folder -> Adobe Photoshop CC`

- **Copy folder `com.am1goo.photoshop.extension.spritesheet3000`**\
into `C:\Program Files\Common Files\Adobe\CEP\extensions\` (for Windows x64)\
or\
into `C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\` (for Windows x86)

## Format structure
Adobe Photoshop's extensions will create some files with ease-to-use structure:
- metafile starts with source `psd filename` (heart of this plugin, contains all required meta information about future animation clip - filter type, compression, ppu and etc.)
- bunch of `animation frames` as single sprites starts with metafile name (don't worry about that, these files will be skipped and don't used in runtime-mode at all, needed only in editor-mode purposes)
- `atlas texture` as result of importing process

```
folder|-
      |- {clipname}.txt
      |- {clipname}_01.png
      |- {clipname}_02.png
      |- {clipname}_03.png
      |- {clipname}_04.png
      |- ...
      |- {clipname}_xx.png
      |- {clipname}_atlas.png
```

## How to use
- **Open extension via Adoby Photoshop CC**\
`Window -> Extensions -> Spritesheet 3000 Exporter`

- **Export all frames as single files and generate metadata**\
Set all options and press button `Export..`
<p align="left">
  <img src="Readme/export-photoshop-frames-and-metadata.png" alt="export-photoshop-frames-and-metadata" width=500 height=auto/>
</p>

- **Import frames via single metedata file**\
Open `Unity Editor`, select single metafile in `Project` window, click `Right Mouse Button` and select `Pack from file`
<p align="left">
  <img src="Readme/import-unity-metadata-single-file.png" alt="import-unity-metadata-single-file"/>
</p>

or

- **Import bunch of frames via folder**\
Open `Unity Editor`, select whole folder in `Project` window, click `Right Mouse Button` and select `Pack from folder`
<p align="left">
  <img src="Readme/import-unity-metadata-whole-folder.png" alt="import-unity-metadata-whole-folder" />
</p>

- **Use packed frames in your game**\
Put component `SpriteAnimator3000` on any game object\
Run any animations via code:
```csharp
using Spritesheet3000;
using UnityEngine;

public class Example : MonoBehaviour
{
    [SerializeField]
    private SpriteAnimationClip3000 _clip;
    [SerializeField]
    private SpriteAnimator3000 _anim;

    private void Awake()
    {
        _anim.Play(_clip);
    }
}
```

## What's next?
- [x] Add ability to trimming all transparent pixels from frames
- [x] Export all frames to `UnityEngine.U2D.SpriteAtlas` file with `.asset` extension

## Tested in
- **Unity 2019.4.x**
- **Unity 2020.3.x**
- **Unity 2022.3.x**

## Using in
- [Sin Slayers: Reign of The 8th](https://sinslayers.com) - RPG with roguelike elements set in a dark fantasy world, where your choices determine how challenging the fights and enemies will be.

## Contribute
Contribution in any form is very welcome. Bugs, feature requests or feedback can be reported in form of Issues.
