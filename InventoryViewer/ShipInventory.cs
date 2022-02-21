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
    /// Program partial class. Defines the ship inventory untility class.
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// Ship inventory class. Handles all functionality for getting and checking the inventory.
        /// </summary>
        public class ShipInventory
        {
            /// <summary>
            /// Predicate to check if the blocks should be returned by the GridTerminalSystem.
            /// </summary>
            /// <param name="ignoreNames">Search string for ignored blocks.</param>
            /// <param name="me">Programmable block to check grid ownership.</param>
            /// <returns>True if the block should be returned by the GridTerminalSystem.</returns>
            public static Func<IMyEntity, bool> FilterBlocks(string[] ignoreNames, IMyProgrammableBlock me)
            {

                return (IMyEntity block) =>
                {
                    if (!((IMyTerminalBlock)block).IsSameConstructAs(me))
                    {
                        return false;
                    }

                    if (ignoreNames.Any() && block.DisplayName != null)
                    {
                        foreach (string ignore in ignoreNames)
                        {
                            if (block.DisplayName.ToLower().Contains(ignore.ToLower()))
                            {
                                return false;
                            }
                        }
                    }

                    if (block is IMyProductionBlock ||
                        block is IMyCargoContainer ||
                        block is IMyCockpit ||
                        block is IMyCollector ||
                        block is IMyGasTank ||
                        block is IMyLargeConveyorTurretBase ||
                        block is IMyGasGenerator ||
                        block is IMyReactor ||
                        block is IMyShipConnector ||
                        block is IMyShipToolBase ||
                        block is IMySmallGatlingGun ||
                        block is IMySmallMissileLauncher ||
                        block is IMyStoreBlock)
                    {
                        return true;
                    }

                    return false;
                };
            }

            /// <summary>
            /// Delegate function for Echo.
            /// </summary>
            private readonly Action<string> Echo;

            /// <summary>
            /// Initialized Oxygen tank list.
            /// </summary>
            private List<IMyGasTank> OxygenInventory = new List<IMyGasTank>();

            /// <summary>
            /// Initialized Hydrogen tank list.
            /// </summary>
            private List<IMyGasTank> HydrogenInventory = new List<IMyGasTank>();

            /// <summary>
            /// Initialized cargo list.
            /// </summary>
            private List<IMyEntity> CargoInventory = new List<IMyEntity>();
            
            /// <summary>
            /// Initializes the ship inventory class.
            /// </summary>
            /// <param name="echo">Echo delegate.</param>
            public ShipInventory(Action<string> echo)
            {
                this.Echo = echo;
            }

            /// <summary>
            /// Gets the Oxygen Level.
            /// </summary>
            public InventoryLevelResult Oxygen
            {
                get
                {
                    return this.GetInventoryLevel(this.OxygenInventory);
                }
            }

            /// <summary>
            /// Gets the Hydrogen level.
            /// </summary>
            public InventoryLevelResult Hydrogen
            {
                get
                {
                    return this.GetInventoryLevel(this.HydrogenInventory);
                }
            }

            /// <summary>
            /// Gets the cargo level.
            /// </summary>
            public InventoryLevelResult Cargo
            {
                get
                {
                    return this.GetInventoryLevel(this.CargoInventory);
                }
            }

            /// <summary>
            /// Clears the inventory blocks.
            /// </summary>
            public void Clear()
            {
                this.HydrogenInventory.Clear();
                this.OxygenInventory.Clear();
                this.CargoInventory.Clear();
            }

            /// <summary>
            /// Adds the list of terminal blocks, sorting into their correct list.
            /// </summary>
            /// <param name="blocks">Terminal blocks.</param>
            public void AddRange(List<IMyTerminalBlock> blocks)
            {
                foreach (IMyTerminalBlock block in blocks)
                {
                    if (block is IMyGasTank)
                    {
                        this.Echo("Gas Tank/Generator Definition Subtype: " + block.BlockDefinition.SubtypeId);
                        if (block.BlockDefinition.SubtypeId.Contains("Oxygen"))
                        {
                            this.OxygenInventory.Add((IMyGasTank)block);
                        }
                        else
                        {
                            this.HydrogenInventory.Add((IMyGasTank)block);
                        }
                    }
                    else
                    {
                        this.CargoInventory.Add(block);
                    }
                }
            }

            /// <summary>
            /// Calculates the inventory level of the given gas tanks.
            /// </summary>
            /// <param name="gasTanks">List of gas tanks.</param>
            /// <returns>Inventory Level Result</returns>
            private InventoryLevelResult GetInventoryLevel(List<IMyGasTank> gasTanks)
            {
                InventoryLevelResult result = new InventoryLevelResult()
                {
                    CurrentVolume = MyFixedPoint.Zero,
                    MaxVolume = MyFixedPoint.Zero,
                    CurrentMass = MyFixedPoint.Zero,
                    ItemCount = 0
                };

                foreach(IMyGasTank gasTank in gasTanks)
                {
                    result.MaxVolume += (MyFixedPoint)gasTank.Capacity;
                    result.CurrentVolume += (MyFixedPoint)(gasTank.Capacity * gasTank.FilledRatio);
                }

                return result;
            }

            /// <summary>
            /// Calculates the Inventory level of the cargo entities.
            /// </summary>
            /// <param name="entities">List of storage entities.</param>
            /// <returns>Inventory Level Result.</returns>
            private InventoryLevelResult GetInventoryLevel(List<IMyEntity> entities)
            {
                InventoryLevelResult result = new InventoryLevelResult()
                {
                    CurrentVolume = MyFixedPoint.Zero,
                    MaxVolume = MyFixedPoint.Zero,
                    CurrentMass = MyFixedPoint.Zero,
                    ItemCount = 0
                };

                foreach(IMyEntity entity in entities)
                {
                    IMyInventory inventory = entity.GetInventory();
                    if (inventory != null)
                    {
                        result.MaxVolume += inventory.MaxVolume;
                        result.CurrentVolume += inventory.CurrentVolume;
                        result.CurrentMass += inventory.CurrentMass;
                        result.ItemCount += inventory.ItemCount;
                    }
                }

                return result;
            }
        }
    }
}
