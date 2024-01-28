using Godot;
using StarCoreTacView;
using System;
using System.Collections.Generic;
using System.Linq; // Include this for LINQ extension methods like Min

public partial class SceneBase : Node3D
{

    const string path = "C:\\Users\\User\\AppData\\Roaming\\SpaceEngineers\\Saves\\76561198071098415\\SCTacView\\Storage\\SCCoordinateOutput_ScCoordWriter\\";
    List<GridMovementData> movementDatas = new List<GridMovementData>();
    MeshInstance3D meshInstance;
    MeshInstance3D templateMeshInstance;
    List<MeshInstance3D> meshInstances = new List<MeshInstance3D>();
    float tick;

    public override void _Ready()
    {
        templateMeshInstance = GetNode<MeshInstance3D>("MeshInstance3D"); // Replace with the actual path

        foreach (var file in DirAccess.GetFilesAt(path))
        {
            string fullPath = System.IO.Path.Combine(path, file);
            var gridData = new GridMovementData(fullPath);
            movementDatas.Add(gridData);

            // Create a new MeshInstance3D for each gridData
            var newMeshInstance = (MeshInstance3D)templateMeshInstance.Duplicate(); // Duplicate the template
            AddChild(newMeshInstance); // Add it as a child of the current node

            // Set the scale based on grid data
            newMeshInstance.Scale = ((Vector3)gridData.GridBox) * gridData.GridSize;

            meshInstances.Add(newMeshInstance); // Add the new mesh instance to the list
            templateMeshInstance.QueueFree(); // Remove the template from the scene
        }

        // Set the tick to the earliest across all files
        if (movementDatas.Any())
        {
            tick = movementDatas.Min(data => data.GetFirstTickValue());
        }
        else
        {
            GD.Print("No movement data files found or loaded.");
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.R))
        {
            tick = movementDatas.Min(data => data.GetFirstTickValue()); // Reset to earliest tick across all files
            foreach (var data in movementDatas)
                data.Reset();
        }

        tick += (float)delta;

        DisplayServer.WindowSetTitle(Math.Round(tick / 60) + "s | " + Engine.GetFramesPerSecond() + "fps");

        // Iterate through all mesh instances and update their positions and rotations
        for (int i = 0; i < movementDatas.Count; i++)
        {
            var gridData = movementDatas[i];
            var meshInstance = meshInstances[i];

            if (meshInstance != null) // Check if the mesh instance exists
            {
                meshInstance.GlobalPosition = gridData.GetPosition(tick);
                meshInstance.Quaternion = gridData.GetRotation(tick);
            }
            else
            {
                GD.Print("MeshInstance is null or not properly initialized.");
            }
        }

        tick++;
    }
}
