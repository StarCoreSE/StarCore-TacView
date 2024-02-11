using Godot;
using StarCoreTacView;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SceneBase : Node3D
{
    public static SceneBase I;

    //const string path = "C:\\Users\\User\\AppData\\Roaming\\SpaceEngineers\\Saves\\76561198071098415\\SCTacView\\Storage\\SCCoordinateOutput_ScCoordWriter";
    const string path = "C:\\Users\\jnick\\Downloads\\SCCoordinateOutput_ScCoordWriter";
    MeshInstance3D meshInstance;
    public MeshInstance3D templateMeshInstance;
    public MeshInstance3D templateStaticMeshInstance; // Declare templateStaticMeshInstance
    float tick;
    float minTick = 0;
    public float simulationSpeed = 60f; // Default simulation speed
    private float bufferSimulationSpeed = 60f; // Default simulation speed


    List<GridMovement> GridMovements = new List<GridMovement>();

    public override void _Ready()
    {
        I = this;

        // Connect the HSliderSim's value_changed signal
        var hSliderSim = GetNode<HSlider>("HSliderSim"); // Adjust the path to your HSliderSim node if necessary
        var callable = new Callable(this, nameof(OnSliderValueChanged));
        hSliderSim.Connect("value_changed", callable);

        templateMeshInstance = GetNode<MeshInstance3D>("ShipMeshInstance3D"); // Replace with the actual path
        templateStaticMeshInstance = GetNode<MeshInstance3D>("StaticMeshInstance3D"); // Replace with the actual path

        foreach (var file in DirAccess.GetFilesAt(path))
            GridMovements.Add(new GridMovement(System.IO.Path.Combine(path, file)));

        // Free the templateMeshInstance after creating all instances
        templateMeshInstance.QueueFree();
        templateStaticMeshInstance.QueueFree();

        // Set the tick to the earliest across all files
        if (GridMovements.Any())
            minTick = GridMovements.Min(data => data.GridData.GetFirstTickValue());
        else
            GD.Print("No movement data files found or loaded.");

        tick = minTick;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        I = null;
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
            tick = minTick; // Reset to the earliest tick across all files
            foreach (var data in GridMovements)
                data.GridData.Reset();
        }

        if (Input.IsActionJustPressed("Pause"))
        {
            if (simulationSpeed == 0)
            {
                simulationSpeed = bufferSimulationSpeed;
            }
            else
            {
                bufferSimulationSpeed = simulationSpeed;
                simulationSpeed = 0;
            }
        }

        // Increment the tick by the delta time multiplied by the simulation speed
        tick += (float)delta * simulationSpeed;

        DisplayServer.WindowSetTitle($"{Math.Round(tick / 60)}s | {Engine.GetFramesPerSecond()}fps | {Math.Round(simulationSpeed/60f, 2)} sim");

        foreach (var gridMovement in GridMovements)
        {
            gridMovement.Update1(tick);
        }
    }
}
