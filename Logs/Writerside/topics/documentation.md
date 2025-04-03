# Programmers documentation
__NPRG031__ - MFF UK Credit program 2025 LS \
\
Author : __Vojtěch Vlachovský__ \
\
License : __MIT__ \
\
Used technologies : 
+ __OpenTK__ - wrapper for OpenGL
+ __ImGui__ - GUI library 
+ __StbImageSharp__ - Skybox image translation
+ __.NET 9.0__ - Lang API 

## File structure

```text
OpenGL/
│
├── GUI/
│   ├── ImGuiController.cs
│   └── ImGuiElementContainer.cs
│
├── ImageDependencies/
│   └── Skybox/
│       └── Skybox cube faces
│
├── Logs/
│   ├── documentation.md
│   └── known bugs.txt
│
├── Objects/
│   ├── Camera.cs
│   ├── Grid.cs
│   ├── Indicator.cs
│   ├── Object.cs
│   ├── ObjectPicker.cs
│   └── Skybox.cs
│
├── Shaders/
│   ├── *.vert
│   └── *.frag
│
├── Constants.cs
├── InputHandler.cs
├── Program.cs
├── Renderer.cs
├── Shader.cs
└── WindowManager.cs
```

## __Kozmot__  a solar system simulator  
