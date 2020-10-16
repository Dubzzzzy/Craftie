using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.RenderQ;
using ExileCore.Shared;
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

        private const string REGAL_ORB = "Regal Orb";

        private const string COROUTINE_NAME = "Craftie Coroutine";

        private Coroutine _coroutineWorker;

        private WaitRandom _waitRandom = new WaitRandom(70, 120);

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
            var craftingKeyStateChanged = Input.IsKeyDown(Settings.StartCraftingButton) != CraftingKeyPrevState;
            if (craftingKeyStateChanged)
            {
                if (!Input.IsKeyDown(Settings.StartCraftingButton))
                {
                    Toggled = !Toggled;
                }
            }
            CraftingKeyPrevState = Input.IsKeyDown(Settings.StartCraftingButton);

            if (ShouldCraft())
            {
                if (CraftIsCompleted())
                {
                    var itemToCraft = GetItemToCraft();
                    Graphics.DrawFrame(itemToCraft.GetClientRect(), Color.Green, 5);
                }
                else
                {
                    TryStartCraftItemCoroutine();
                }
            }
        }

        private void TryStartCraftItemCoroutine()
        {
            if (_coroutineWorker != null && _coroutineWorker.IsDone)
            {
                _coroutineWorker = null;
            }

            if (_coroutineWorker == null)
            {
                _coroutineWorker = new Coroutine(CraftItem(), this, COROUTINE_NAME);
                Core.ParallelRunner.Run(_coroutineWorker);
            }
        }

        private IEnumerator CraftItem()
        {
            while (!CraftIsCompleted())
            {
                if (!HasAllCurrencyNeeded())
                {
                    Toggled = false;
                }

                if (!Toggled)
                {
                    yield break;
                }

                var itemToCraft = GetItemToCraft();
                var modsComponent = itemToCraft.Item.GetComponent<Mods>();
                switch (modsComponent.ItemRarity)
                {
                    case ItemRarity.Normal:
                        yield return UseOrbOfTransmutation();
                        break;
                    case ItemRarity.Magic:
                        if (modsComponent.HumanStats.Count < 2)
                        {
                            yield return UseOrbOfAugmentation();
                        }
                        else if (HasDuplicateCurrencyChance(modsComponent) || HasIncQuantity(modsComponent))
                        {
                            yield return UseRegalOrb();
                        }
                        else
                        {
                            yield return UseOrbOfAlteration();
                        }
                        break;
                    case ItemRarity.Rare:
                        yield return UseOrbOfScouring();
                        break;
                }
                yield return _waitRandom;
            }
        }

        private bool CraftIsCompleted()
        {
            var itemToCraft = GetItemToCraft();
            while (itemToCraft == null)
            {
                LogMsg("Item To Craft is null");
                itemToCraft = GetItemToCraft();
                Thread.Sleep(100);
            }
            var modsComponent = itemToCraft.Item.GetComponent<Mods>();
            return HasDuplicateCurrencyChance(modsComponent) && HasIncQuantity(modsComponent);
        }

        private IEnumerator UseOrbOfAlteration()
        {
            LogMsg("UseOrbOfAlteration");
            var orbOfAlterationItem = GetOrbOfAlterationItem();
            return UseOrb(orbOfAlterationItem);
        }

        private IEnumerator UseRegalOrb()
        {
            LogMsg("UseRegalOrb");
            var regalOrbItem = GetRegalOrbItem();
            return UseOrb(regalOrbItem);
        }

        private IEnumerator UseOrbOfAugmentation()
        {
            LogMsg("UseOrbOfAugmentation");
            var orbOfAugmentationItem = GetOrbOfAugmentationItem();
            return UseOrb(orbOfAugmentationItem);
        }

        private IEnumerator UseOrbOfScouring()
        {
            LogMsg("UseOrbOfScouring");
            var orbOfScouringItem = GetOrbOfScouringItem();
            return UseOrb(orbOfScouringItem);
        }

        private IEnumerator UseOrbOfTransmutation()
        {
            LogMsg("UseOrbOfTransmutation");
            var orbOfTransmutationItem = GetOrbOfTransmutationItem();
            return UseOrb(orbOfTransmutationItem);
        }

        private IEnumerator UseOrb(NormalInventoryItem orbItem)
        {
            var itemToCraft = GetItemToCraft();
            yield return Input.SetCursorPositionSmooth(orbItem.GetClientRect().Center);
            yield return _waitRandom;
            Input.Click(MouseButtons.Right);
            yield return _waitRandom;
            yield return Input.SetCursorPositionSmooth(itemToCraft.GetClientRect().Center);
            yield return _waitRandom;
            Input.Click(MouseButtons.Left);
        }

        private bool ShouldCraft()
        {
            if (!Toggled)
                return false;
            if (!GameController.Game.IngameState.IngameUi.StashElement.IsVisible)
                return false;
            if (GameController.Game.IngameState.IngameUi.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)
                return false;
            if (!HasItemToCraft())
                return false;
            if (!HasAllCurrencyNeeded())
                return false;
            return true;
        }

        private bool HasAllCurrencyNeeded() => 
            HasOrbOfAlteration() && HasOrbOfAugmentation() && HasOrbOfScouring() && HasOrbOfTransmutation() && HasRegalOrb();

        private NormalInventoryItem GetItemToCraft() =>
            GetInventoryItemByName(ITEM_TO_CRAFT_NAME);

        private bool HasItemToCraft() =>
            GetItemToCraft() != null;

        private bool HasIncQuantity(Mods mods)
        {
            var regex = new Regex(@"(\d+)% increased Quantity of Items dropped in Heists");
            var quantMod = mods.HumanStats.FirstOrDefault(x => regex.IsMatch(x));
            if (quantMod == null)
                return false;
            var quantCurrentPercent = int.Parse(regex.Match(quantMod).Groups[1].Value);
            return quantCurrentPercent >= Settings.IncreasedQuantity;
        }

        private bool HasDuplicateCurrencyChance(Mods mods)
        {
            var regex = new Regex(@"Heist Chests have a (\d+)% chance to Duplicate contained Currency");
            var duplicateCurrencyMod = mods.HumanStats.FirstOrDefault(x => regex.IsMatch(x));
            if (duplicateCurrencyMod == null)
                return false;
            var duplicateCurrencyCurrentPercent = int.Parse(regex.Match(duplicateCurrencyMod).Groups[1].Value);
            return duplicateCurrencyCurrentPercent >= Settings.DuplicateCurrencyChance;
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

        private NormalInventoryItem GetRegalOrbItem() =>
            GetInventoryItemByName(REGAL_ORB);

        private bool HasRegalOrb() =>
            GetRegalOrbItem() != null;

        private NormalInventoryItem GetInventoryItemByName(string name) =>
            GameController.Game.IngameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems.FirstOrDefault(x => x?.Item?.GetComponent<Base>()?.Name == name);
    }
}
