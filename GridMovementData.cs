using Godot;
using System;
using System.Linq;


namespace StarCoreTacView
{
    public class GridMovementData
    {
        public bool IsDone = false;

        public string StartTime = "";
        public string Faction = "";
        public string GridName = "";
        public string GridOwner = "";
        public bool IsGridAlive = true;
        public float GridHealth = 1;

        public float GridSize = 2.5f;
        public Vector3I GridBox = Vector3I.Zero;
        public bool IsStatic = false; // Add IsStatic property

        private string[][] allCells = new string[0][];
        int currentRow = 0;

        int currentTick = 0;
        Vector3 gridPosition = Vector3.Zero;
        Quaternion gridOrientation = Quaternion.Identity;

        int nextTick = 0;
        Vector3 nextGridPosition = Vector3.Zero;
        Quaternion nextGridOrientation = Quaternion.Identity;

        public GridMovementData(string filePath)
        {
            FileAccess Access = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
            GD.Print("LoadFile " + Access?.GetPath()[Access.GetPath().IndexOf("\\")..]);

            string[] allRows = Access.GetAsText(true).Split('\n');
            allCells = new string[allRows.Length][];
            for (int i = 0; i < allRows.Length; i++)
                allCells[i] = allRows[i].Split(",");

            ParseHeaderRow();
            ParseSubHeaderRow();
            ParseDataRow();
            gridPosition = nextGridPosition;
            gridOrientation = nextGridOrientation;

            GD.Print($"Init grid {GridName} of dims {((Vector3)GridBox) * GridSize}, owner {GridOwner}");
        }

        public void Reset()
        {
            IsDone = false;
            currentRow = 2;
            currentTick = 0;
            nextTick = 0;
            ParseDataRow();
            gridPosition = nextGridPosition;
            gridOrientation = nextGridOrientation;
        }

        public int GetFirstTickValue()
        {
            if (allCells.Length > 2)
            {
                // Assuming the first numerical value in each row is the tick value
                // and it's stored at index 0.
                var tickValues = allCells.Skip(2) // Skip header rows
                    .Select(row => int.TryParse(row[0], out var tick) ? tick : (int?)null)
                    .Where(tick => tick.HasValue)
                    .Select(tick => tick.Value);

                if (tickValues.Any())
                {
                    return tickValues.Min();
                }
            }
            return int.MaxValue; // Return a large value if no valid tick found
        }

        public Vector3 GetPosition(float tick)
        {
            if (IsDone)
                return nextGridPosition;

            float delta = (float)(tick - currentTick) / (nextTick - currentTick);

            if (delta < 0)
                return nextGridPosition;

            return gridPosition.Lerp(nextGridPosition, delta);
        }

        public Quaternion GetRotation(float tick)
        {
            if (IsDone)
                return nextGridOrientation;

            float delta = (float)(tick - currentTick) / (nextTick - currentTick);

            if (delta > 1)
                ParseDataRow();
            else if (delta < 0)
                return nextGridOrientation;

            // Normalize the quaternions before performing Slerp
            Quaternion normalizedGridOrientation = gridOrientation.Normalized();
            Quaternion normalizedNextGridOrientation = nextGridOrientation.Normalized();

            return normalizedGridOrientation.Slerp(normalizedNextGridOrientation, delta);
        }

        private void ParseHeaderRow()
        {
            string[] row = allCells[currentRow];

            StartTime = row[0];
            Faction = row[1];
            GridName = row[2];
            GridOwner = row[3];
            IsStatic = bool.Parse(row[4]); // Parse IsStatic value

            currentRow++;
        }

        private void ParseSubHeaderRow()
        {
            string[] row = allCells[currentRow];

            GridSize = float.Parse(row[0]);
            GridBox.X = int.Parse(row[1]);
            GridBox.Y = int.Parse(row[2]);
            GridBox.Z = int.Parse(row[3]);

            currentRow++;
        }

        private void ParseDataRow()
        {
            GD.Print((allCells.Length <= currentRow) + " | " + (allCells.Length - currentRow));
            if (IsDone || allCells.Length - 1 <= currentRow)
            {
                IsDone = true;
                return;
            }

            string[] row = allCells[currentRow];

            gridPosition = nextGridPosition;
            gridOrientation = nextGridOrientation;

            currentTick = nextTick;
            nextTick = int.Parse(row[0]);

            IsGridAlive = bool.Parse(row[1]);
            GridHealth = float.Parse(row[2]);

            nextGridPosition.X = float.Parse(row[3]);
            nextGridPosition.Y = float.Parse(row[4]);
            nextGridPosition.Z = float.Parse(row[5]);

            nextGridOrientation.X = float.Parse(row[6]);
            nextGridOrientation.Y = float.Parse(row[7]);
            nextGridOrientation.Z = float.Parse(row[8]);
            nextGridOrientation.W = float.Parse(row[9]);

            currentRow++;
        }
    }
}
