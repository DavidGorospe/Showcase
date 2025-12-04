# Earth Nebula

## Name

David Gorospe

## UtorID

gorosped

## Student Number

1007995225

## Assignment Number Augmented

A6 Shaderpipeline

## Instructions

First, clone this repository.

If the executable in the `build folder` does not work for rendering, these instructions below are to compile. Otherwise can skip to running the command in the last line of instructions.

Assuming the current directory is `computer-graphics-shader-pipeline`, enter the builder director with command: `cd build`.

To compile, open the file `shaderpipeline.sln` in Visual Studio. Right-click on the `shaderpipeline` option in the solution directory and set the build location of `shaderpipeline` in build directory, not in a `Debug` folder. After setting this property, build the solution.

Now in the terminal of your IDE, assuming the directory is `build`, run the command: `./shaderpipeline ../data/test-08.json` to render the piece.

## Description

- Added additional ray tracing
- Added nebula effect to both Earth and Moon
- Added stars to both Earth and Moon
- Added twinkling effect on stars
- Added "city lights" to Earth
- Added day and night cycle on Earth
- Added aurora borealis effect at the poles of Earth
- Added enhanced clouds

To find all changes, access the file `planet.fs` in the src folder of the root directory. This was the only file changed.
