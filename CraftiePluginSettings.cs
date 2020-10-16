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

        [Menu("Start Crafting Button")] 
        public HotkeyNode StartCraftingButton { get; set; } = Keys.NumPad5;

        [Menu("Duplicate Currency Chance Modifier")]
        public RangeNode<int> DuplicateCurrencyChance { get; set; } = new RangeNode<int>(10, 6, 15);
        
        [Menu("Increased Quantity Modifier")]
        public RangeNode<int> IncreasedQuantity { get; set; } = new RangeNode<int>(10, 7, 15);
    }
}