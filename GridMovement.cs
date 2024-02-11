using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarCoreTacView
{
    public class GridMovement
    {
        public GridMovementData GridData;
        public MeshInstance3D MeshInstance;


        public void Update1(float tick)
        {
            if (MeshInstance != null) // Check if the mesh instance exists
            {
                MeshInstance.GlobalPosition = GridData.GetPosition(tick);
                MeshInstance.Quaternion = GridData.GetRotation(tick);
            }
            else
            {
                GD.Print("MeshInstance is null or not properly initialized.");
            }
        }


        public GridMovement() { }

        public GridMovement(string fullPath)
        {
            var gridData = new GridMovementData(fullPath); // Make sure GridMovementData is correctly implemented
            GridData = gridData;

            // Create a new MeshInstance3D for each gridData and clone its children
            MeshInstance3D newMeshInstance;

            if (gridData.IsStatic)
            {
                newMeshInstance = (MeshInstance3D)SceneBase.I.templateStaticMeshInstance.Duplicate();
            }
            else
            {
                newMeshInstance = (MeshInstance3D)SceneBase.I.templateMeshInstance.Duplicate();
            }

            SceneBase.I.AddChild(newMeshInstance);

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

            // Apply color to the particle material
            ApplyColorToParticles(newMeshInstance, gridData.Faction);

            MeshInstance = newMeshInstance;
        }

        private void ApplyColorToParticles(Node node, string faction)
        {
            // Recursively search for GPUParticles3D nodes and apply color to their materials
            if (node is GpuParticles3D particles)
            {
                var particleMaterial = (ParticleProcessMaterial)((ParticleProcessMaterial)particles.ProcessMaterial).Duplicate();

                // Adjust colors according to your conditions
                if (faction.Contains("RED"))
                {
                    particleMaterial.Color = new Color(1f, 0f, 0f); // Red color
                }
                else if (faction.Contains("BLU"))
                {
                    particleMaterial.Color = new Color(0f, 0f, 1f); // Blue color
                }

                particles.ProcessMaterial = particleMaterial;
            }

            foreach (Node child in node.GetChildren())
            {
                ApplyColorToParticles(child, faction);
            }
        }
    }
}
