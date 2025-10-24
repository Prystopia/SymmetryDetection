# Symmetry Detection

Detect planes of symmetry within 3D point clouds in a managed .NET library

This project was created to complete part of my PhD thesis which looked into the world of Generational AI and AI-Generated art.  The tools are based off the [symseg](https://github.com/aecins/symseg) project operated by [Aleksandrs Ecins](https://github.com/aecins).  

An example of how these tools can be used to detect 3D symmetries is below.

## Important Information

The information contained is what I can gather from running my project and some of it is most likely incorrect, don't accept my word as gospel, and feel free to suggest improvements.  One of the main reasons for implementing this project was to help my understanding of how the process works, there are many different tools and libraries that perform the same function in a better manner.

## Code Example

Quick sample showing how to detect the symmetries for a unit cube, code can be found in the e2e project.

### Original Image

```csharp

//build the unit cube as a point cloud
TestFileType testFile = new TestFileType(new List<(Vector3, Vector3)>()
{
    (new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, 0)),
    (new Vector3(-0.5f, -0.5f , 0.5f), new Vector3(5, 5, 5)),
    (new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(10, 10, 10)),
    (new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(15, 15, 15)),
    (new Vector3(0.5f, -0.5f, -0.5f), new Vector3(20, 20, 20)),
    (new Vector3(0.5f, -0.5f, 0.5f), new Vector3(25, 25, 25)),
    (new Vector3(0.5f, 0.5f, 0.5f), new Vector3(30, 30, 30)),
    (new Vector3(0.5f, 0.5f, -0.5f), new Vector3(35, 35, 35)),
});

List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
{
    new ReflectionalSymmetryDetector(new DistanceErrorScoreService()),
};
ISymmetryExporter exporter = new TextExporter();
SymmetryDetectionHandler drs = new SymmetryDetectionHandler(testFile, handlers);
drs.DetectSymmetries();
AverageGlobalScoreService averageScoreService = new AverageGlobalScoreService();
var score = averageScoreService.CalculateGlobalScore(drs);
Console.WriteLine($"Detected symmetry score: {score}, expected: 0.692");
```

## Contributing

Currently, there are no plans to extend this project, however this may change in the near future :).  I am always open to contributions, suggestions and improvements.