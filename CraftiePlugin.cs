using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.RenderQ;
using ExileCore.Shared.Enums;
using ImGuiNET;
using SharpDX;

namespace Craftie
{
    public class CraftiePlugin : BaseSettingsPlugin<CraftiePluginSettings>
    {
        private const string ITEM_TO_CRAFT_NAME = "Foliate Brooch";

        private const string ORB_OF_ALTERATION = "Orb of Alteration";

        private const string ORB_OF_TRANSMUTATION = "Orb of Transmutation";

        private const string ORB_OF_AUGMENTATION = "Orb of Augmentation";

        private const string ORB_OF_SCOURING = "Orb of Scouring";

        private bool Toggled { get; set; } = false;

        private bool CraftingKeyPrevState { get; set; } = false;

        public override void OnLoad()
        {
        }

        public override bool Initialise()
        {
            Input.RegisterKey(Settings.StartCraftingButton.Value);
            return true;
        }

        public override void Render()
        {
            var craftingKeyStateChanged = Input.IsKeyDown(Settings.StartCraftingButton.Value) != CraftingKeyPrevState;
            if (craftingKeyStateChanged)
            {
                if (!Input.IsKeyDown(Settings.StartCraftingButton.Value))
                {
                    Toggled = !Toggled;
                }
            }
            CraftingKeyPrevState = Input.IsKeyDown(Settings.StartCraftingButton.Value);

            if (ShouldCraft())
            {
                var itemToCraft = GetItemToCraft();
                var mods = itemToCraft.Item.GetComponent<Mods>();
                if (HasIncQuantityAtLeast(mods, 10))
                {
                    Graphics.DrawFrame(itemToCraft.GetClientRect(), Color.Red, 1);
                }
            }
        }

        private bool ShouldCraft()
        {
            if (!Toggled)
                return false;
            if (!GameController.Game.IngameState.IngameUi.StashElement.IsVisible)
                return false;
            if (GameController.Game.IngameState.IngameUi.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)
                return false;
            if (!HasItemToCraft() || !HasOrbOfAlteration() || !HasOrbOfAugmentation() || !HasOrbOfScouring() || !HasOrbOfTransmutation())
                return false;
            return true;
        }

        private NormalInventoryItem GetItemToCraft() =>
            GetInventoryItemByName(ITEM_TO_CRAFT_NAME);

        private bool HasItemToCraft() =>
            GetItemToCraft() != null;

        private bool HasIncQuantityAtLeast(Mods mods, int percent)
        {
            var regex = new Regex(@"(\d+)% increased Quantity of Items dropped in Heists");
            var quantMod = mods.HumanStats.FirstOrDefault(x => regex.IsMatch(x));
            if (quantMod == null)
                return false;
            var quantCurrentPercent = int.Parse(regex.Match(quantMod).Groups[1].Value);
            return quantCurrentPercent >= percent;
        }

        private NormalInventoryItem GetOrbOfAlterationItem() =>
            GetInventoryItemByName(ORB_OF_ALTERATION);

        private bool HasOrbOfAlteration() =>
            GetOrbOfAlterationItem() != null;

        private NormalInventoryItem GetOrbOfTransmutationItem() =>
            GetInventoryItemByName(ORB_OF_TRANSMUTATION);

        private bool HasOrbOfTransmutation() =>
            GetOrbOfTransmutationItem() != null;

        private NormalInventoryItem GetOrbOfAugmentationItem() =>
            GetInventoryItemByName(ORB_OF_AUGMENTATION);

        private bool HasOrbOfAugmentation() =>
            GetOrbOfAugmentationItem() != null;

        private NormalInventoryItem GetOrbOfScouringItem() =>
            GetInventoryItemByName(ORB_OF_SCOURING);
        private bool HasOrbOfScouring() =>
            GetOrbOfScouringItem() != null;

        private NormalInventoryItem GetInventoryItemByName(string name) =>
            GameController.Game.IngameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems.FirstOrDefault(x => x.Item.GetComponent<Base>().Name == name);
    }
}
