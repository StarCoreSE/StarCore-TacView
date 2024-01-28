using Godot;
using StarCoreTacView;
using System;
using System.Collections.Generic;

public partial class SceneBase : Node3D
{
	const float StartTicks = 2000;

	const string path = "C:\\Users\\jnick\\AppData\\Roaming\\SpaceEngineers\\Saves\\76561198274566684\\Mod Profiler Test\\Storage\\SCCoordinateOutput_ScCoordWriter\\";
	List<GridMovementData> movementDatas = new List<GridMovementData>();
	MeshInstance3D meshInstance;
	float tick = StartTicks;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (var file in DirAccess.GetFilesAt(path))
			movementDatas.Add(new GridMovementData(path + file));
		meshInstance = GetChild<MeshInstance3D>(0);
		meshInstance.Scale = ((Vector3)movementDatas[0].GridBox) * movementDatas[0].GridSize;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Input.IsKeyPressed(Key.R))
        {
            tick = StartTicks;
            foreach (var data in movementDatas)
                data.Reset();
        }

        tick += (float) delta;

        DisplayServer.WindowSetTitle(Math.Round(tick/60) + "s | " + Engine.GetFramesPerSecond() + "fps");

        meshInstance.GlobalPosition = movementDatas[0].GetPosition(tick);
		meshInstance.Quaternion = movementDatas[0].GetRotation(tick);

		tick++;
	}
}
