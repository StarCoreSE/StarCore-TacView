using Godot;
using StarCoreTacView;
using System;
using System.Collections.Generic;

public partial class SceneBase : Node3D
{
    const float StartTicks = 2000;

    const string path = "C:\\Users\\User\\AppData\\Roaming\\SpaceEngineers\\Saves\\76561198071098415\\SCTacView\\Storage\\SCCoordinateOutput_ScCoordWriter\\";
    List<GridMovementData> movementDatas = new List<GridMovementData>();
    MeshInstance3D meshInstance;
    float tick = StartTicks;
    float elapsedTime = 0f; // Cumulative elapsed time

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        foreach (var file in DirAccess.GetFilesAt(path))
        {
            string fullPath = System.IO.Path.Combine(path, file);
            movementDatas.Add(new GridMovementData(fullPath));
        }

        // Debug statement to confirm the list is populated
        GD.Print("Number of movement data loaded: " + movementDatas.Count);

        if (movementDatas.Count > 0)
        {
            meshInstance = GetChild<MeshInstance3D>(0);
            meshInstance.Scale = ((Vector3)movementDatas[0].GridBox) * movementDatas[0].GridSize;
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
            tick = StartTicks;
            elapsedTime = 0f; // Reset the elapsed time
            foreach (var data in movementDatas)
                data.Reset();
        }

        elapsedTime += (float)delta;
        if (elapsedTime >= 1.0f) // Check if one second has passed
        {
            tick += 60;  // Increment tick by 60 per second
            elapsedTime -= 1.0f; // Reset the elapsed time accumulator
        }

        DisplayServer.WindowSetTitle(Math.Round(tick / 60) + "s | " + Engine.GetFramesPerSecond() + "fps");

        if (movementDatas.Count > 0)
        {
            meshInstance.GlobalPosition = movementDatas[0].GetPosition(tick);
            meshInstance.Quaternion = movementDatas[0].GetRotation(tick);
        }
        else
        {
            GD.Print("No data available to update position and rotation.");
        }
    }
}
