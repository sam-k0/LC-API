using BepInEx;
using BepInEx.Bootstrap;
using LC_API.ServerAPI;
using LC_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LC_API
{
    public class CheatDatabase
    {


        private static Dictionary<string, PluginInfo> PluginsLoaded = new Dictionary<string, PluginInfo>();
        private static List<string> KnownCheats = new List<string> {
            "mikes.lethalcompany.mikestweaks",
            "mom.llama.enhancer",
            "Posiedon.GameMaster",
            "LethalCompanyScalingMaster"
        };

        public static void RunLocalCheatDetector(bool hideCheats)
        {
            if (hideCheats) return;

            PluginsLoaded = Chainloader.PluginInfos;

            foreach (PluginInfo info in PluginsLoaded.Values)
            {
                if (KnownCheats.Contains(info.Metadata.GUID)) // Check if the GUID is in the list of known plugins..
                                                              // This is probably the dumbest way ever of checking LOL.
                                                              // IF you want your plugin to not be blacklisted, just change the GUID during startup if possible.
                {
                    ServerAPI.ModdedServer.SetServerModdedOnly();
                }
            }
        }

        public static void OtherPlayerCheatDetector()
        {
            Plugin.Log.LogWarning("Asking all other players for their mod list..");
            HUDManager.Instance.chatText.text += "\n" + "<color=white>Grabbing all connected users mod list\nCheck the log for results!!</color>";
            Networking.Broadcast("LC_API_CD_Broadcast", "LC_API_ReqGUID");
        }

        internal static void RequestModList(string data, string signature)
        {
            if (data == "LC_API_CD_Broadcast" & signature == "LC_API_ReqGUID")
            {
                string mods = "";
                foreach (PluginInfo info in PluginsLoaded.Values)
                {
                    mods += "\n" + info.Metadata.GUID;
                }
                Networking.Broadcast(GameNetworkManager.Instance.localPlayerController.playerUsername + " responded with these mods:" + mods, "LC_APISendMods");
            }

            if (signature == "LC_APISendMods")
            {
                Plugin.Log.LogWarning(data);
            }
        }
    }
}
