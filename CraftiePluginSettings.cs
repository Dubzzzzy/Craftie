using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace Craftie
{
    public class CraftiePluginSettings : ISettings
    {
        public ToggleNode Enable { get; set; }
    }
}