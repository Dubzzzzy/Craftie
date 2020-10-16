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
        }

        public ToggleNode Enable { get; set; } = new ToggleNode(true);

        [Menu("Start Crafting Button")] public HotkeyNode StartCraftingButton { get; set; } = Keys.NumPad5;
    }
}