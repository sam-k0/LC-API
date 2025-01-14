﻿using BepInEx;
using BepInEx.Bootstrap;
using LC_API.GameInterfaceAPI;
using LC_API.ServerAPI;
using LC_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LC_API
{
    internal static class CheatDatabase
    {
        const string DAT_CD_BROADCAST = "LC_API_CD_Broadcast";
        const string SIG_REQ_GUID = "LC_API_ReqGUID";
        const string SIG_SEND_MODS = "LC_APISendMods";


        public static string customMessage = "";

        private static Dictionary<string, PluginInfo> PluginsLoaded = new Dictionary<string, PluginInfo>();

        private static List<string> KnownCheats = new List<string> {
            "mikes.lethalcompany.mikestweaks",
            "mom.llama.enhancer",
            "Posiedon.GameMaster",
            "LethalCompanyScalingMaster",
            "verity.amberalert"
        };

        public static void RunLocalCheatDetector(bool hideModlist,bool hideCheats)
        {
            if (hideModlist) // Incognito: Block request completely and appear as unmodded
            {
                Plugin.Log.LogWarning("[Incognito Mode for Sussy Imposters] Blocked a modlist request.");
                return;
            };

            PluginsLoaded = Chainloader.PluginInfos;

            // If we are using half incognito: Remove only the suspicious mods
            if(hideCheats)
            {
                // Remove entries with values in KnownCheats list
                PluginsLoaded = PluginsLoaded.Where(kv => !KnownCheats.Contains(kv.Value.Metadata.GUID))
                                             .ToDictionary(kv => kv.Key, kv => kv.Value);

                Plugin.Log.LogWarning("[Half Incognito] Removed only suspicious mods");

                Plugin.Log.LogWarning("[Half Incognito] Filter result:");
                foreach(PluginInfo pluginInfo in PluginsLoaded.Values)
                {
                    Plugin.Log.LogWarning(pluginInfo.Metadata.GUID);
                }
            }



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
            GameTips.ShowTip("Mod List:", "Asking all other players for installed mods..");
            GameTips.ShowTip("Mod List:", "Check the logs for more detailed results.\n<size=13>(Note that if someone doesnt show up on the list, they may not have LC_API installed)</size>");
            Networking.Broadcast(DAT_CD_BROADCAST, SIG_REQ_GUID);
        }

        internal static void CDNetGetString(string data, string signature)
        {
            if (data == DAT_CD_BROADCAST && signature == SIG_REQ_GUID)
            {
                Plugin.Log.LogWarning("DAT_CD_BROADCAST && SIG_REQ_GUID");
                Plugin.Log.LogWarning($"Custom message is: {customMessage}");
                string mods = "";
                foreach (PluginInfo info in PluginsLoaded.Values)
                {
                    mods += "\n" + info.Metadata.GUID;
                }

                string sendModMessage = GameNetworkManager.Instance.localPlayerController.playerUsername + " responded with these mods:" + mods;
                if (customMessage != "")
                {
                    sendModMessage = customMessage; // Overwrite 
                }

                Networking.Broadcast(sendModMessage, SIG_SEND_MODS);
            }

            if (signature == SIG_SEND_MODS)
            {
                GameTips.ShowTip("Mod List (SIG_SEND_MODS):", data);
                Plugin.Log.LogError("SIG_SEND_MODS");
                Plugin.Log.LogWarning(data);
            }
        }
    }
}
