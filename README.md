# pb_CSG

A C# port of [CSG.js](http://evanw.github.io/csg.js/) by Evan W for use in the Unity game engine.

## Quick Start

pb_CSG provides an interface in the `CSG` class for creating new meshes from boolean operations.  Each function (`Union`, `Subtract`, `Intersect`) accepts 2 gameObjects: the left and right side.  A new mesh is returned.

Example use:

	// Include the library
	using Parabox.CSG;

	...

	// Initialize two new meshes in the scene
	GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
	GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
	sphere.transform.localScale = Vector3.one * 1.3;

	// Perform boolean operation
	Mesh m = CSG.Subtract(cube, sphere);

	// Create a gameObject to render the result
	composite = new GameObject();
	composite.AddComponent<MeshFilter>().sharedMesh = m;
	composite.AddComponent<MeshRenderer>().sharedMaterial = myMaterial;

Result:

![](bin/images/subtract.PNG?raw=true)

