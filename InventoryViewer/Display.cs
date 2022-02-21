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
        public class Display
        {
            private class DisplaySettings
            {
                private readonly MyIni ini = new MyIni();
                public bool Cargo { get; private set; } = true;
                public bool Oxygen { get; private set; } = true;
                public bool Hydrogen { get; private set; } = true;
                public string HalfCode { get; private set; } = "\u9703";
                public string FullCode { get; private set; } = "\u9724";
                public int Segments { get; private set; } = 10;

                public DisplaySettings(IMyTextPanel panel)
                {
                    if (this.ini.TryParse(panel.CustomData))
                    {
                        if (this.ini.ContainsSection("inventory"))
                        {
                            this.Cargo = this.ini.Get("inventory", "cargo").ToBoolean(true);
                            this.Oxygen = this.ini.Get("inventory", "oxygen").ToBoolean(true);
                            this.Hydrogen = this.ini.Get("inventory", "hydrogen").ToBoolean(true);
                            this.HalfCode = this.ini.Get("inventory", "half-symbol").ToString(this.HalfCode);
                            this.FullCode = this.ini.Get("inventory", "full-symbol").ToString(this.FullCode);
                            this.Segments = this.ini.Get("inventory", "segments").ToInt32(this.Segments);
                        }
                    }
                }
            }

            private class DisplayPanel
            {
                public IMyTextPanel TextPanel { get; }
                public DisplaySettings Settings { get; }
                private RectangleF Viewport;
                private List<MySprite> Sprites = new List<MySprite>();
                private Action<string> Echo;
                public DisplayPanel(IMyTextPanel panel, Action<string> echo)
                {
                    this.Echo = echo;
                    this.TextPanel = panel;
                    this.Settings = new DisplaySettings(panel);
                    this.Viewport = new RectangleF((this.TextPanel.TextureSize - this.TextPanel.SurfaceSize) / 2f, this.TextPanel.SurfaceSize);
                }

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

                private string GetBlockString(float percentage)
                {
                    int full = (int)Math.Floor(percentage / this.Settings.Segments);
                    int partial = (int)Math.Floor((decimal)(percentage % this.Settings.Segments != 0 ? 1 : 0));
                    int length = 0;
                    string value = string.Empty;
                    for(int i = 0; i < full; i++)
                    {
                        value += this.Settings.FullCode;
                        length++;
                    }

                    if (partial >= (this.Settings.Segments / 2))
                    {
                        value += this.Settings.HalfCode;
                        length++;
                    }

                    while(length < this.Settings.Segments)
                    {
                        value += "_";
                        length++;
                    }

                    return value;
                }
            }

            private readonly Action<string, List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>> SearchBlocksByName;
            private readonly Func<string, IMyTerminalBlock> GetBlocksByName;
            private readonly Action<string> Echo;
            private readonly List<DisplayPanel> TextPanels = new List<DisplayPanel>();
            private readonly List<IMyTerminalBlock> TerminalBlocks = new List<IMyTerminalBlock>();
            public bool SuppressErrors { get; set; }

            public Display(
                Action<string> echo,
                Action<string, List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>> search,
                Func<string, IMyTerminalBlock> getByName)
            {
                this.Echo = echo;
                this.SearchBlocksByName = search;
                this.GetBlocksByName = getByName;
            }

            public void Clear()
            {
                this.TextPanels.Clear();
                this.TerminalBlocks.Clear();
            }

            public void Initialize(string searchTag, string[] blockNames, string[] ignore)
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
                    else
                    {
                        this.Add(panel);
                    }
                }

                if (!string.IsNullOrWhiteSpace(searchTag))
                {
                    this.SearchBlocksByName(searchTag, this.TerminalBlocks, block => block is IMyTextPanel);
                    foreach(IMyTerminalBlock block in this.TerminalBlocks)
                    {
                        this.Add(block);
                    }
                }
            }


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
