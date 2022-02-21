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
        public class ShipInventory
        {
            /// <summary>
            /// Predicate to check if the blocks should be returned by the GridTerminalSystem.
            /// </summary>
            /// <param name="ignoreNames">Search string for ignored blocks.</param>
            /// <returns>True if the block should be returned by the GridTerminalSystem.</returns>
            public static Func<IMyEntity, bool> FilterBlocks(string[] ignoreNames)
            {

                return (IMyEntity block) =>
                {
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

                    if (block is IMyProductionBlock)
                    {
                        return true;
                    }
                    else if (block is IMyCargoContainer)
                    {
                        return true;
                    }
                    else if (block is IMyCockpit)
                    {
                        return true;
                    }
                    else if (block is IMyCollector)
                    {
                        return true;
                    }
                    else if (block is IMyGasTank)
                    {
                        return true;
                    }
                    else if (block is IMyLargeConveyorTurretBase)
                    {
                        return true;
                    }
                    else if (block is IMyGasGenerator)
                    {
                        return true;
                    }
                    else if (block is IMyReactor)
                    {
                        return true;
                    }
                    else if (block is IMyShipConnector)
                    {
                        return true;
                    }
                    else if (block is IMyShipToolBase)
                    {
                        return true;
                    }
                    else if (block is IMySmallGatlingGun)
                    {
                        return true;
                    }
                    else if (block is IMySmallMissileLauncher)
                    {
                        return true;
                    }
                    else if (block is IMyStoreBlock)
                    {
                        return true;
                    }

                    return false;
                };
            }

            private readonly Action<string> Echo;

            public List<IMyGasTank> OxygenInventory { get; } = new List<IMyGasTank>();
            public List<IMyGasTank> HydrogenInventory { get; } = new List<IMyGasTank>();
            public List<IMyEntity> CargoInventory { get; } = new List<IMyEntity>();
            

            public ShipInventory(Action<string> echo)
            {
                this.Echo = echo;
            }

            public InventoryLevelResult Oxygen
            {
                get
                {
                    return this.GetInventoryLevel(this.OxygenInventory);
                }
            }

            public InventoryLevelResult Hydrogen
            {
                get
                {
                    return this.GetInventoryLevel(this.HydrogenInventory);
                }
            }

            public InventoryLevelResult Cargo
            {
                get
                {
                    return this.GetInventoryLevel(this.CargoInventory);
                }
            }

            public void Clear()
            {
                this.HydrogenInventory.Clear();
                this.OxygenInventory.Clear();
                this.CargoInventory.Clear();
            }

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
