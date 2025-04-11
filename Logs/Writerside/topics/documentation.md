# Kozmot Documentation
__NPRG031__ - MFF UK Credit program 2025 LS \
\
Author : __Vojtěch Vlachovský__ \
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

## Overview

*Kozmot* is an experimental, unpolished, highly optimized 3D graphic simulator of a solar system created by __YOU__ focusing mainly on speed and variability instead of high spec graphics, developed using C#. It is designed to run as a single, local application without any need to install additional dependencies. \
\
This Documentation may serve anyone who is browsing github for some simple OpenTK references or students of the BDSMFF social club and is protected under the *Chicken-dance* license of free non-commercial use.


## How to run
```
CMD
> git clone https://github.com/Kropensaft/Kozmot # Clone repo
> cd Kozmot/ # Switch to project root directory
> dotnet run 
```


## Files and their functionality

__Core files__ :
+ *Renderer.cs* - The heart and brain of the whole program, handles virtually all of the rendering either directly or by calling their respective __Render()__ function at an appropriate time and place, the main purpose is of course rendering the planets which you using the UI configure based on your preferences.

+ *Shader.cs* - A custom function which "dumbs-down" the process of creating a new shader program for the pipeline, you simply pass the paths to the vertex and fragment shaders and it will handle the file parsing, compilation and shader linking.

+ *WindowManager.cs* - Serves mainly as an event subscriber which also registers OnWindowClose and calls the Cleanup functions.

+ *Program.cs* - Entry point of the program, as all entry points should be it is simple, concise and as short as possible.

+ *InputHandler.cs* - Class responsible for all input monitoring and logic execution, the core functionality is redirection of IO when GUI is focused.

__GUI__ : 
+ *ImGuiElementContainer.cs*  - This is the file which houses all of the bespoke UI, it utilizes the ImGuiController for rendering on its own vbo buffer and the main ImGui.NET wrapper for the low-level UI functions. If you are curios about the implementation of the UI or something needs to be changed it can be located only here.

+ *ImGuiController.cs* - Backend for ImGuiElementContainer, handles all of the rendering and render-pipeline work behind the scenes so that the new UI can simply be called by OneFunction. UI is rendered on its separate buffers since UI doesn't need a projection matrix and it can simply be rendered atop the screen viewport.


__Objects__ : 

+ **Camera.cs** - Implementation of an ArcBall camera, which is a type of camera moving on all axis around a central pivot in this case a selected celestial object. It is also crucial for creating and passing on the projection and view matrices used for rasterization calculations by OpenTK.
    + _Core Methods_ :
      + Vec3 Position {get; private set; }
      + Vec3 Front {get; private set;}
      + Vec3 Up {get; private set;}
      + Vec3 Right {get; private set;}
      + UpdateVector() - Called by Renderer after a move action is performed in order to recalculate the angles
      + Mat4 GetProjectionMatrix(float aspectRatio) - called when a reference for the projMatrix is needed
+ **Grid.cs** - A white square grid with highlighted main axis in red and blue, rendered on its own buffer.
    + _Core Methods_ :
      + GenerateGeometry(int size, int step) - Generates vertices/indices for the shaders based on the number of steps and size
      + (private) InitializeBuffers() - Prepares the OpenGL calls and necessary arrays
      + Render(int shaderProgram, Mat4 viewMatrix, Mat4 projMatrix) - Call inside Renderer in Update in order to render, can be put inside a function to disable rendering ex. "Button click"
+ **Indicator.cs** - A semi-transparent planetary like object which is used to show the user where the configured object will be located at what approximate proportions, it is also fully customizable in terms of presentation inside the Camera UI tab.
    + _Core Methods_:
      + GetRadii() - Returns a predetermined radius for a planet based on its selected type.
      + Initialize() - Prepares all of the mesh data for the buffers and initializes the shader program.
      + Render(viewMat, projMat, color, alpha) - Renders the indicator at a base location and updates its position and appearance using the parameters.

+ **Object.cs** - Parent class for all planets, implements scaled down Physical theorems and relations in order to simulate gravity and force of attraction as best as possible (for a student of Computer Science).
    + _Core Methods_:
      + Sphere(name, position, rotation, scale, color, mass, orbit radius, ... , isEmissive)
      
+ **Skybox.cs** - Implementation of a skybox cube which creates its own buffers and adds texture to the inner faces of the cube-
    + _Core Methods_: 
      + Skybox(string[] facePaths, string shaderBasePath) - Constructor
      + Render(viewMatrix, projMatrix) - Renders at the point when called using provided matrices.

