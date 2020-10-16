using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore;
using ExileCore.RenderQ;
using ExileCore.Shared.Enums;

namespace Craftie
{
    public class CraftiePlugin : BaseSettingsPlugin<CraftiePluginSettings>
    {
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
            if (Input.IsKeyDown(Settings.StartCraftingButton.Value))
            {
            }
        }

        private bool ShouldCraft()
        {
            if (!Input.IsKeyDown(Settings.StartCraftingButton.Value))
                return false;
            DebugWindow.LogMsg(GameController.Game.IngameState.IngameUi.StashElement.IndexVisibleStash.ToString());
            return true;
        }
    }
}
