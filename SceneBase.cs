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
    float tick;

    public override void _Ready()
    {
        foreach (var file in DirAccess.GetFilesAt(path))
        {
            string fullPath = System.IO.Path.Combine(path, file);
            var gridData = new GridMovementData(fullPath);
            movementDatas.Add(gridData);
        }

        // Find the earliest tick across all files
        if (movementDatas.Count > 0)
        {
            tick = movementDatas.Min(data => data.GetFirstTickValue()); // Ensure System.Linq is used
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
            tick = movementDatas.Min(data => data.GetFirstTickValue()); // Reset to earliest tick across all files
            foreach (var data in movementDatas)
                data.Reset();
        }

        tick += (float)delta;

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

        tick++;
    }
}
