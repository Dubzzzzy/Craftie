using System.Security.Cryptography;
using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace Craftie
{
    public class CraftiePluginSettings : ISettings
    {
        public CraftiePluginSettings()
        {
            Enable = new ToggleNode(false);
            StartCraftingButton = new HotkeyNode(Keys.NumPad5);
        }

        public ToggleNode Enable { get; set; }

        [Menu("Start Crafting")]
        public HotkeyNode StartCraftingButton { get; set; }
    }
}