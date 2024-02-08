using Godot;
using StarCoreTacView;
using System;
using System.Collections.Generic;
using System.Linq; // Include this for LINQ extension methods like Min

public partial class SceneBase : Node3D
{
    const string path = "C:\\Users\\User\\AppData\\Roaming\\SpaceEngineers\\Saves\\76561198071098415\\SCTacView\\Storage\\SCCoordinateOutput_ScCoordWriter";
    List<GridMovementData> movementDatas = new List<GridMovementData>();
    MeshInstance3D meshInstance;
    MeshInstance3D templateMeshInstance;
    MeshInstance3D templateStaticMeshInstance; // Declare templateStaticMeshInstance
    List<MeshInstance3D> meshInstances = new List<MeshInstance3D>();
    float tick;
    private float simulationSpeed = 60f; // Default simulation speed

    public override void _Ready()
    {

        // Connect the HSliderSim's value_changed signal
        var hSliderSim = GetNode<HSlider>("HSliderSim"); // Adjust the path to your HSliderSim node if necessary
        var callable = new Callable(this, nameof(OnSliderValueChanged));
        hSliderSim.Connect("value_changed", callable);



        templateMeshInstance = GetNode<MeshInstance3D>("ShipMeshInstance3D"); // Replace with the actual path
        templateStaticMeshInstance = GetNode<MeshInstance3D>("StaticMeshInstance3D"); // Replace with the actual path

        foreach (var file in DirAccess.GetFilesAt(path))
        {
            string fullPath = System.IO.Path.Combine(path, file);
            var gridData = new GridMovementData(fullPath); // Make sure GridMovementData is correctly implemented
            movementDatas.Add(gridData);

            // Create a new MeshInstance3D for each gridData and clone its children
            MeshInstance3D newMeshInstance;

            if (gridData.IsStatic)
            {
                newMeshInstance = (MeshInstance3D)templateStaticMeshInstance.Duplicate();
            }
            else
            {
                newMeshInstance = (MeshInstance3D)templateMeshInstance.Duplicate();
            }

            AddChild(newMeshInstance);

            // Set the scale based on grid data
            newMeshInstance.Scale = ((Vector3)gridData.GridBox) * gridData.GridSize;

            // Create a new StandardMaterial3D and set its albedo color
            var material = new StandardMaterial3D();

            // Adjust colors according to your conditions
            if (gridData.Faction.Contains("RED"))
            {
                material.AlbedoColor = new Color(1f, 0f, 0f); // Use normalized RGB values
            }
            else if (gridData.Faction.Contains("BLU"))
            {
                material.AlbedoColor = new Color(0f, 0f, 1f);
            }

            // Assign the StandardMaterial3D to the new MeshInstance
            newMeshInstance.MaterialOverride = material;

            meshInstances.Add(newMeshInstance); // Assuming meshInstances is a list to keep track of instances
        }

        // Free the templateMeshInstance after creating all instances
        templateMeshInstance.QueueFree();
        templateStaticMeshInstance.QueueFree();

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

    private void OnSliderValueChanged(float value)
    {
        simulationSpeed = value;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.R))
        {
            tick = movementDatas.Min(data => data.GetFirstTickValue()); // Reset to the earliest tick across all files
            foreach (var data in movementDatas)
                data.Reset();
        }

        // Increment the tick by the delta time multiplied by the simulation speed
        tick += (float)delta * simulationSpeed;

        DisplayServer.WindowSetTitle(Math.Round(tick / 60) + "s | " + Engine.GetFramesPerSecond() + "fps");

        // Check if the sizes of movementDatas and meshInstances are the same
        if (movementDatas.Count == meshInstances.Count)
        {
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
        }
        else
        {
            GD.Print("Size mismatch between movementDatas and meshInstances.");
        }
    }
}
