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
    /// Main Program Partial class. Defines the program entry point and control logic.
    /// </summary>
    partial class Program : MyGridProgram
    {
        /// <summary>
        /// Program INI. Parsed from the programmable block's CustomData
        /// </summary>
        private readonly MyIni ini = new MyIni();

        /// <summary>
        /// Inventory Class.
        /// </summary>
        private readonly ShipInventory Inventory;

        /// <summary>
        /// Display Class.
        /// </summary>
        private readonly Display CargoDisplay;

        /// <summary>
        /// Initialized list for Terminal blocks.
        /// </summary>
        private readonly List<IMyTerminalBlock> TerminalBlocks = new List<IMyTerminalBlock>();

        /// <summary>
        /// Incremental failure count.
        /// </summary>
        private int FailureCount = 0;

        /// <summary>
        /// Instantiates the Program.
        /// </summary>
        public Program()
        {
            this.Inventory = new ShipInventory(this.Echo);
            this.CargoDisplay = new Display(this.Echo, this.GridTerminalSystem.SearchBlocksOfName, this.GridTerminalSystem.GetBlockWithName);
            this.Initialize();
            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        /// <summary>
        /// Main Entry Point. Retries once on an exception at runtime to reinitialized and account for removed blocks.
        /// </summary>
        /// <param name="argument">Argument string.</param>
        /// <param name="updateSource">Update source.</param>
        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                this.Run(argument, updateSource);
                this.FailureCount = 0;
            }
            catch(Exception ex)
            {
                if (this.FailureCount == 0)
                {
                    this.Initialize();
                    this.FailureCount++;
                }
                else
                {
                    this.Echo("Unhandled exception. Tried reinitializing but still failed.");
                    this.Echo(ex.Message);
                }
            }
        }

        /// <summary>
        /// Coordinates the execution.
        /// </summary>
        /// <param name="argument">Argument string.</param>
        /// <param name="updateSource">Update source.</param>
        private void Run(string argument, UpdateType updateSource)
        {
            this.CargoDisplay.Update(this.Inventory.Cargo, this.Inventory.Hydrogen, this.Inventory.Oxygen);
        }

        /// <summary>
        /// Initializes the program.
        /// </summary>
        /// <remarks>
        /// On failures, the program attempts to reinitialize. This is to catch any blocks that may have been removed.
        /// </remarks>
        private void Initialize()
        {
            if (this.ini.TryParse(this.Me.CustomData))
            {
                string[] ignore = this.ini.Get("general", "ignore").ToString("").Split(',');

                bool suppressErrors = this.ini.Get("general", "supressErrors").ToBoolean(false);
                this.CargoDisplay.SuppressErrors = suppressErrors;
                this.TerminalBlocks.Clear();
                this.Inventory.Clear();
                this.CargoDisplay.Clear();
                this.GridTerminalSystem.GetBlocksOfType<IMyEntity>(this.TerminalBlocks, ShipInventory.FilterBlocks(ignore, this.Me));
                this.Inventory.AddRange(this.TerminalBlocks);


                string cargoDisplayPanelTag = this.ini.Get("display", "search").ToString("");
                string[] cargoDisplayPanelNames = this.ini.Get("display", "panels").ToString("").Split(',');
                this.CargoDisplay.Initialize(cargoDisplayPanelTag, cargoDisplayPanelNames, ignore, this.Me);
                this.Echo("Initialization Complete");
            }
            else
            {
                this.Echo("Unable to Parse CustomData");
            }
        }

    }
}
