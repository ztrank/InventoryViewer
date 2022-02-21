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

    partial class Program : MyGridProgram
    {
        private readonly MyIni ini = new MyIni();
        private readonly ShipInventory Inventory;
        private readonly Display CargoDisplay;
        private readonly List<IMyTerminalBlock> TerminalBlocks = new List<IMyTerminalBlock>();
        private int FailureCount = 0;

        /// <summary>
        /// Instantiates the Program.
        /// </summary>
        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
            this.Inventory = new ShipInventory(this.Echo);
            this.CargoDisplay = new Display(this.Echo, this.GridTerminalSystem.SearchBlocksOfName, this.GridTerminalSystem.GetBlockWithName);
            this.Initialize();
            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

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

        

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
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

        private void Run(string argument, UpdateType updateSource)
        {
            this.CargoDisplay.Update(this.Inventory.Cargo, this.Inventory.Hydrogen, this.Inventory.Oxygen);
        }
    }
}
