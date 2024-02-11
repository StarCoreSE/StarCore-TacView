using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StarCoreTacView
{
    public class GridMovement
    {
        private static StandardMaterial3D MaterialRed = new StandardMaterial3D()
        {
            AlbedoColor = new Color(1f, 0f, 0f)
        };
        private static StandardMaterial3D MaterialBlue = new StandardMaterial3D()
        {
            AlbedoColor = new Color(0f, 0f, 1f)
        };
        private static StandardMaterial3D MaterialRedFaded = new StandardMaterial3D()
        {
            AlbedoColor = new Color(0.1f, 0f, 0f)
        };
        private static StandardMaterial3D MaterialBlueFaded = new StandardMaterial3D()
        {
            AlbedoColor = new Color(0f, 0f, 0.1f)
        };

        public GridMovementData GridData;
        public MeshInstance3D MeshInstance;
        private GpuParticles3D TrailParticles = null;
        Label3D label;

        bool didMarkDead = false;

        public void Update1(float tick)
        {
            // Check if the mesh instance exists
            if (MeshInstance == null)
            {
                GD.PrintErr("MeshInstance is null or not properly initialized.");
                return;
            }
            
            MeshInstance.GlobalPosition = GridData.GetPosition(tick);
            MeshInstance.Quaternion = GridData.GetRotation(tick);


            if (TrailParticles != null)
            {
                float speedScale = SceneBase.I.simulationSpeed / 60f;

                TrailParticles.SpeedScale = speedScale;

                //if (vel > 0)
                //    TrailParticles.Lifetime = speedScale;
            }

            if (!didMarkDead && (!GridData.IsGridAlive || GridData.IsDone))
            {
                if (GridData.Faction.Contains("RED"))
                {
                    MeshInstance.MaterialOverride = MaterialRedFaded;
                }
                else if (GridData.Faction.Contains("BLU"))
                {
                    MeshInstance.MaterialOverride = MaterialBlueFaded;
                }

                if (label != null)
                {
                    label.Modulate *= new Color(1, 1, 1, 0.25f);
                    label.OutlineModulate *= new Color(1, 1, 1, 0.25f);
                }

                didMarkDead = true;

                if (TrailParticles != null)
                {
                    TrailParticles.Emitting = false;
                }
            }
        }


        public GridMovement() { }

        public GridMovement(string fullPath)
        {
            var gridData = new GridMovementData(fullPath); // Make sure GridMovementData is correctly implemented
            GridData = gridData;

            if (gridData.IsStatic)
            {
                MeshInstance = (MeshInstance3D)SceneBase.I.templateStaticMeshInstance.Duplicate();
            }
            else
            {
                MeshInstance = (MeshInstance3D)SceneBase.I.templateMeshInstance.Duplicate();
            }

            SceneBase.I.AddChild(MeshInstance);

            if (MeshInstance.GetChildCount() > 0)
                label = MeshInstance.GetChild(0) as Label3D;

            // Set the scale based on grid data
            label?.SetDisableScale(true);
            MeshInstance.Scale = ((Vector3)gridData.GridBox) * gridData.GridSize;

            // Create a new StandardMaterial3D and set its albedo color

            // Adjust colors according to your conditions
            if (gridData.Faction.Contains("RED"))
            {
                MeshInstance.MaterialOverride = MaterialRed;
            }
            else if (gridData.Faction.Contains("BLU"))
            {
                MeshInstance.MaterialOverride = MaterialBlue;
            }

            // Apply color to the particle material
            ApplyColorToParticles(MeshInstance, gridData.Faction);

            if (MeshInstance.GetChildCount() > 1)
                TrailParticles = MeshInstance.GetChild(1) as GpuParticles3D;

            if (label != null)
                label.Text = gridData.GridName + "\n" + gridData.GridOwner;
        }

        public void Reset()
        {
            GridData.Reset();

            if (didMarkDead)
            {
                if (GridData.Faction.Contains("RED"))
                {
                    MeshInstance.MaterialOverride = MaterialRed;
                }
                else if (GridData.Faction.Contains("BLU"))
                {
                    MeshInstance.MaterialOverride = MaterialBlue;
                }

                if (label != null)
                {
                    label.Modulate *= new Color(1, 1, 1, 4f);
                    label.OutlineModulate *= new Color(1, 1, 1, 4f);
                }

                didMarkDead = false;

                if (TrailParticles != null)
                {
                    TrailParticles.Emitting = true;
                }
            }
        }

        private static void ApplyColorToParticles(Node node, string faction)
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
