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
    /// Program partial class. Contains the display utility classes.
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// Display class for handling writing to a display.
        /// </summary>
        public class Display
        {
            /// <summary>
            /// Display settings class for getting the setting of a display block.
            /// </summary>
            private class DisplaySettings
            {
                /// <summary>
                /// Display block's INI from the CustomData field.
                /// </summary>
                private readonly MyIni ini = new MyIni();

                /// <summary>
                /// Gets a value indicating whether to display the cargo levels.
                /// </summary>
                public bool Cargo { get; private set; } = true;

                /// <summary>
                /// Gets a value indicating whether to display the oxygen levels.
                /// </summary>
                public bool Oxygen { get; private set; } = true;

                /// <summary>
                /// Gets a value indicating whether to display the hydrogen levels.
                /// </summary>
                public bool Hydrogen { get; private set; } = true;

                /// <summary>
                /// Initializes the display settings class.
                /// </summary>
                /// <param name="panel">Display panel.</param>
                public DisplaySettings(IMyTextPanel panel)
                {
                    if (this.ini.TryParse(panel.CustomData))
                    {
                        if (this.ini.ContainsSection("inventory"))
                        {
                            this.Cargo = this.ini.Get("inventory", "cargo").ToBoolean(true);
                            this.Oxygen = this.ini.Get("inventory", "oxygen").ToBoolean(true);
                            this.Hydrogen = this.ini.Get("inventory", "hydrogen").ToBoolean(true);
                        }
                    }
                }
            }

            /// <summary>
            /// Display panel class handles the writing to the panel.
            /// </summary>
            private class DisplayPanel
            {
                /// <summary>
                /// The writable viewport.
                /// </summary>
                private RectangleF Viewport;

                /// <summary>
                /// Initialized list of sprites.
                /// </summary>
                private List<MySprite> Sprites = new List<MySprite>();

                /// <summary>
                /// Echo delegate.
                /// </summary>
                private Action<string> Echo;

                /// <summary>
                /// Initializes the display panel class.
                /// </summary>
                /// <param name="panel">Panel to write to.</param>
                /// <param name="echo">Echo delegate.</param>
                public DisplayPanel(IMyTextPanel panel, Action<string> echo)
                {
                    this.Echo = echo;
                    this.TextPanel = panel;
                    this.Settings = new DisplaySettings(panel);
                    this.Viewport = new RectangleF((this.TextPanel.TextureSize - this.TextPanel.SurfaceSize) / 2f, this.TextPanel.SurfaceSize);
                }

                /// <summary>
                /// Gets the text panel.
                /// </summary>
                public IMyTextPanel TextPanel { get; }

                /// <summary>
                /// Gets the settings.
                /// </summary>
                public DisplaySettings Settings { get; }

                /// <summary>
                /// Updates the display with the new values.
                /// </summary>
                /// <param name="cargo">Cargo percentage full.</param>
                /// <param name="hydrogen">Hydrogen percentage full.</param>
                /// <param name="oxygen">Oxygen percentage full.</param>
                public void Update(float cargo, float hydrogen, float oxygen)
                {
                    this.TextPanel.ContentType = ContentType.SCRIPT;
                    this.TextPanel.Script = "";
                    using (MySpriteDrawFrame frame = this.TextPanel.DrawFrame())
                    {
                        this.SetSprites(cargo, hydrogen, oxygen);
                        foreach(MySprite sprite in this.Sprites)
                        {
                            frame.Add(sprite);
                        }
                    }
                }

                /// <summary>
                /// Sets the list of sprites to add to the display.
                /// </summary>
                /// <param name="title">Title of the line.</param>
                /// <param name="value">Percentage full.</param>
                /// <param name="starting">Starting position</param>
                private void SetSprites(string title, float value, Vector2 starting)
                {
                    Vector2 position = new Vector2(starting.X, starting.Y);
                    float drawableWidth = this.Viewport.Width - 40f;
                    float barMaxWithd = drawableWidth - 20f;
                    this.Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = $"{title}:",
                        Position = position,
                        RotationOrScale = 0.8f,
                        Color = Color.White,
                        Alignment = TextAlignment.LEFT,
                        FontId = "White"
                    });

                    position += new Vector2(0, 20);

                    this.Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = "[",
                        Position = position,
                        RotationOrScale = 0.8f,
                        Color = Color.White,
                        Alignment = TextAlignment.LEFT,
                        FontId = "White"
                    });
                    position += new Vector2(10, 0);

                    float remainingSpace = barMaxWithd * (value / 100);
                    this.Echo("Creating Boxes: " + remainingSpace.ToString());
                    while(remainingSpace > 20)
                    {
                        this.Sprites.Add(new MySprite()
                        {
                            Type = SpriteType.TEXTURE,
                            Data = "SquareTapered",
                            Position = new Vector2(0, 13) + position,
                            Size = new Vector2(20, 12),
                            Alignment = TextAlignment.LEFT,
                            FontId = "White"
                        });

                        remainingSpace -= 20;
                        position += new Vector2(20, 0);
                    }

                    if (Math.Floor(remainingSpace) > 0.0001)
                    {
                        this.Sprites.Add(new MySprite()
                        {
                            Type = SpriteType.TEXTURE,
                            Data = "SquareTapered",
                            Position = new Vector2(0, 13) + position,
                            Size = new Vector2((float)Math.Floor(remainingSpace), 12),
                            Alignment = TextAlignment.LEFT,
                            FontId = "White"
                        });
                    }

                    position += new Vector2(remainingSpace, 0);
                    position += new Vector2(barMaxWithd - (barMaxWithd * (value / 100)), 0);

                    this.Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = "]",
                        Position = position,
                        RotationOrScale = 0.8f,
                        Color = Color.White,
                        Alignment = TextAlignment.LEFT,
                        FontId = "White"
                    });
                }

                /// <summary>
                /// Coordinates setting the sprites for each row.
                /// </summary>
                /// <param name="cargo">Cargo percentage.</param>
                /// <param name="hydrogen">Hydrogen percentage.</param>
                /// <param name="oxygen">Oxygen percentage.</param>
                private void SetSprites(float cargo, float hydrogen, float oxygen)
                {
                    this.Sprites.Clear();

                    Vector2 position = new Vector2(20, 20) + this.Viewport.Position;

                    if (this.Settings.Cargo)
                    {
                        this.SetSprites("Cargo", cargo, position);
                        position += new Vector2(0, 40);
                    }

                    if (this.Settings.Hydrogen)
                    {
                        this.SetSprites("Hydrogen", hydrogen, position);
                        position += new Vector2(0, 40);
                    }

                    if (this.Settings.Oxygen)
                    {
                        this.SetSprites("Oxygen", oxygen, position);
                    }
                }
            }

            /// <summary>
            /// Search for blocks delegate.
            /// </summary>
            private readonly Action<string, List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>> SearchBlocksByName;

            /// <summary>
            /// Get Blocks by name delegate.
            /// </summary>
            private readonly Func<string, IMyTerminalBlock> GetBlocksByName;

            /// <summary>
            /// Echo delegate.
            /// </summary>
            private readonly Action<string> Echo;

            /// <summary>
            /// Text Panel list.
            /// </summary>
            private readonly List<DisplayPanel> TextPanels = new List<DisplayPanel>();

            /// <summary>
            /// Terminal block list.
            /// </summary>
            private readonly List<IMyTerminalBlock> TerminalBlocks = new List<IMyTerminalBlock>();

            /// <summary>
            /// Initializes the display class.
            /// </summary>
            /// <param name="echo">Echo delegate.</param>
            /// <param name="search">Search delegate.</param>
            /// <param name="getByName">Get block delegate.</param>
            public Display(
                Action<string> echo,
                Action<string, List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>> search,
                Func<string, IMyTerminalBlock> getByName)
            {
                this.Echo = echo;
                this.SearchBlocksByName = search;
                this.GetBlocksByName = getByName;
            }

            /// <summary>
            /// Gets or sets a value indicating whether to suppress exceptions.
            /// </summary>
            public bool SuppressErrors { get; set; }

            /// <summary>
            /// Clears the display class for initialization.
            /// </summary>
            public void Clear()
            {
                this.TextPanels.Clear();
                this.TerminalBlocks.Clear();
            }

            /// <summary>
            /// Initializes the display class by getting the blocks and creating the wrapper classes.
            /// </summary>
            /// <param name="searchTag">String to search for.</param>
            /// <param name="blockNames">Exact block names to get.</param>
            /// <param name="ignore">Blocks to ignore.</param>
            /// <param name="me">Programmable block to check for grid ownership.</param>
            public void Initialize(string searchTag, string[] blockNames, string[] ignore, IMyProgrammableBlock me)
            {
                this.Clear();
                if (string.IsNullOrWhiteSpace(searchTag) && blockNames.Length == 0)
                {
                    this.Echo($"Missing Displays. Add either a 'search' term or comma separated 'panels' in the CustomData configuration.");
                    return;
                }

                foreach(string blockName in blockNames)
                {
                    if (string.IsNullOrWhiteSpace(blockName))
                    {
                        continue;
                    }

                    IMyTerminalBlock panel = this.GetBlocksByName(blockName);
                    if (panel == null)
                    {
                        if (!this.SuppressErrors)
                        {
                            throw new Exception($"Unable to add Text Panel: {blockName} returned no results.");
                        }
                    }
                    else if(panel.IsSameConstructAs(me))
                    {
                        this.Add(panel);
                    }
                }

                if (!string.IsNullOrWhiteSpace(searchTag))
                {
                    this.SearchBlocksByName(searchTag, this.TerminalBlocks, block => block is IMyTextPanel && block.IsSameConstructAs(me));
                    foreach(IMyTerminalBlock block in this.TerminalBlocks)
                    {
                        this.Add(block);
                    }
                }
            }

            /// <summary>
            /// Distributes the update call to all display panels.
            /// </summary>
            /// <param name="cargo">Cargo result.</param>
            /// <param name="hydrogen">Hydrogent result.</param>
            /// <param name="oxygen">Oxygen result.</param>
            public void Update(InventoryLevelResult cargo, InventoryLevelResult hydrogen, InventoryLevelResult oxygen)
            {
                float cargoPercentage = cargo == null ? 0 : cargo.Percentage;
                float hydrogenPercentage = hydrogen == null ? 0 : hydrogen.Percentage;
                float oxygentPercentage = oxygen == null ? 0 : oxygen.Percentage;
                
                foreach(DisplayPanel panel in this.TextPanels)
                {
                    panel.Update(cargoPercentage, hydrogenPercentage, oxygentPercentage);
                }
            }

            /// <summary>
            /// Adds the terminal block to the display panel list.
            /// </summary>
            /// <param name="block">Terminal block.</param>
            private void Add(IMyTerminalBlock block)
            {
                if (block is IMyTextPanel)
                {
                    this.TextPanels.Add(new DisplayPanel((IMyTextPanel)block, this.Echo));
                }
                else
                {
                    if (!this.SuppressErrors)
                    {
                        throw new Exception($"Unable to add Text Panel: {block.DisplayNameText} is not an IMyTextPanel.");
                    }
                }
            }
        }
    }
}
