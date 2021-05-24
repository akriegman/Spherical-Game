# Spherical-Game
A game in spherical space S^3

Check it out on YouTube: https://youtu.be/MtrgYVnKpGE
Download or play in browser: https://akriegman.itch.io/metrica

Controls:
```
Movement: W A S D space shift
Turning: mouse Q E
Break / place blocks: left / right click
Select material: 1 2 3 4 5
Show grid: F1
Change level: 0 - =
```

A synopsis of various files:
```
python
 ∟ meshGenerate.py
    - A python script to generate various polytopes. Writes into a custom text based file format `.tope`.
    Each line of a `.tope` file contains a comment, vertex, edge, face, or cell, denoted by
    '#:', 'v:', etc. Each component has one or more lists of attributes, with those lists
    separated by colons. A vertex has a list of coordinates. An edge has a list of vertices
    and a list of faces. A face has a list of vertices and a list of cells. A cell has a list of vertices.

SphericalGame
 - This folder contains the Unity project.
 ∟ Assets
    ∟ GlmSharp
       - A linear algebra library.
       
    ∟ Scripts
       ∟ Axes.cs
          - A script for a game object that shows the points of interest 1, i, j, k, -1, -i, -j, -k.
       ∟ BallColliderSpherical.cs
          - A collider in spherical space for ball shaped objects.
       ∟ CameraSpherical.cs
          - A script that must be attached to any spherical space camera.
       ∟ Globals.cs
          - A static class holding global variables.
       ∟ MeshColliderSpherical.cs
          - A collider in spherical space for mesh shaped objects.
       ∟ MeshProjector.cs
          - Give an object a normal euclidean 3D model and attach this script to it. When the game
          is loaded in the mesh will be projected into spherical space.
       ∟ MovementController.cs
          - Handles user input and moves the object it is attached to.
       ∟ PhysicsSpherical.cs
          - Contains the static class which controls the physics. Also contains structs for rays and raycast
          hits, and the abstract collider class.
       ∟ Polytope.cs
          - The script for the terrain. In `Start()` it reads a `.tope` file. In `Update()` it recalculates
          a graphical mesh and a collider mesh if the contents of any cells have been changed. Contains
          several small classes for holding polytope data efficiently.
       ∟ RigidBodySpherical.cs
          - A script for objects affected by gravity, velocity, or collisions. To be completed.
       ∟ Rot4.cs
          - The `Rot4` class represents a four dimensional rotation using two isoclinic rotations represented by
          quaternions. Also contains many static linalg methods. The `R4` struct is used for casting
          between various classes that represent points in R^4.
       ∟ Supervisor.cs
          - Right now this just calls a method in `PhysicsSpherical` every fixed update because
          `PhysicsSpherical` is static. Kinda hacky. Have one of these in every scene if you want
          physics.
       ∟ TestBehaviour.cs
          - A script for testing.
       ∟ TestEditor.cs
          - Editor class for `TestBehaviour`. TODO: move this into an Editor folder.
       ∟ TransformSpherical.cs
          - The equivalent of `Transform`. Everything that has a position in spherical space needs one of these.
          
    ∟ Shaders
       ∟ PosNorSpherical.shader
          - The shader for objects in spherical space. Uses two passes, one for objects closer to the camera
          than the antipodal point, and the other for after your line of sight passes through the antipodal
          point. Position is passed as a unit 4 vector, and the normal is passed as another unit 4 vector
          representing a point in space 90 degrees away from the position.
       ∟ SecondPassOnly.shader
          - Same as `PosNorSpherical.shader` but it only does the second pass. Used for the player model 
          so you can see the back of your head after your line of sight goes all the way around.
          
    ∟ StreamingAssets
       - Special folder that allows unusual assets to be included in the build.
       ∟ Polytopes
          - Where we keep the `.tope` files. There is a right hand rule guarantee on each edge and each face,
          which is why "oriented" is in the names. Details of this rule in `meshGenerate.py` towards the bottom.
          ∟ oriented600cell.tope
             - The 600 cell.
          ∟ oriented4800cell.tope
             - A subdivision of the 600 cell.
```
