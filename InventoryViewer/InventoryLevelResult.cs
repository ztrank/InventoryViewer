namespace IngameScript
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Ingame.Utilities;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRageMath;

    partial class Program
    {
        public class InventoryLevelResult
        {
            public float Percentage
            {
                get
                {
                    if (this.CurrentVolume.Equals(MyFixedPoint.Zero) || this.MaxVolume.Equals(MyFixedPoint.Zero))
                    {
                        return 0;
                    }

                    return this.CurrentVolume.RawValue * (1f / this.MaxVolume.RawValue) * 100;
                }
            }
            public MyFixedPoint CurrentMass { get; set; }
            public MyFixedPoint CurrentVolume { get; set; }
            public MyFixedPoint MaxVolume { get; set; }
            public int ItemCount { get; set; }
        }
    }
}
