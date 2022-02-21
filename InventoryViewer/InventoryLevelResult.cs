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

    /// <summary>
    /// Program partial class. Contains the InventoryLevelResult utility class.
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// Inventory Level Result class is a data class for the inventory result.
        /// </summary>
        public class InventoryLevelResult
        {
            /// <summary>
            /// Gets the percentage level of the inventory.
            /// </summary>
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

            /// <summary>
            /// Gets or sets the current mass.
            /// </summary>
            public MyFixedPoint CurrentMass { get; set; }

            /// <summary>
            /// Gets or sets the current volume.
            /// </summary>
            public MyFixedPoint CurrentVolume { get; set; }

            /// <summary>
            /// Gets or sets the max volume.
            /// </summary>
            public MyFixedPoint MaxVolume { get; set; }

            /// <summary>
            /// Gets or sets the item count.
            /// </summary>
            public int ItemCount { get; set; }
        }
    }
}
