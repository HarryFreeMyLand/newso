using FSO.Client;
using FSO.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FSO.UI.Model.DiscordRpc;

namespace FSO.UI.Model
{
    public static class DiscordRpcEngine
    {
        public static bool Active;
        public static bool Disable;
        public static string Secret;
        public static EventHandlers Events;

        public static void Init()
        {
            try
            {
                var handlers = new EventHandlers();
                handlers.readyCallback += Ready;
                handlers.errorCallback += Error;
                handlers.joinCallback += Join;
                handlers.spectateCallback += Spectate;
                handlers.disconnectedCallback += Disconnected;
                handlers.requestCallback += Request;
                Events = handlers;

                Initialize("541327324092563458", ref handlers, true, null);
            }
            catch (Exception)
            {
                Active = false;
            }
        }

        public static void Update()
        {
            if (Disable)
                return;
            try
            {
                RunCallbacks();
            }
            catch (Exception)
            {
                Active = false;
                Disable = true;
            }
        }
        // Method for other game screens
        public static void SendFSOPresence(string state, string details = null)
        {

            if (!Active)
                return; // RPC not active
            var presence = new RichPresence
            {
                largeImageKey = "sunrise_crater",
                largeImageText = "Sunrise Crater",

                state = state,
                details = details ?? ""
            };

            UpdatePresence(ref presence);
        }
        // Standard DiscordRpc presence method
        public static void SendFSOPresence(string activeSim, string lotName, int lotID, int players, int maxSize, int catID, bool isPrivate = false)
        {
            if (!Active)
                return;
            var presence = new RichPresence();

            if (!isPrivate)
            {
                if (lotName?.StartsWith("{job:") == true)
                {
                    var jobStr = "";
                    var split = lotName.Split(':');
                    if (split.Length > 2)
                    {
                        switch (split[1])
                        {
                            case "0": // Robot Factory
                                jobStr = GameFacade.Strings.GetString("f114", "2");
                                break;
                            case "1": // Restaurant
                                jobStr = GameFacade.Strings.GetString("f114", "3");
                                break;
                            case "2": // Nightclub
                                jobStr = GameFacade.Strings.GetString("f114", "4");
                                break;
                            default: // Other
                                jobStr = GameFacade.Strings.GetString("f114", "1");
                                break;
                        }
                        jobStr += " | Level " + split[2].Trim('}');
                    }
                    else
                        jobStr = GameFacade.Strings.GetString("f114", "1");
                    if (activeSim != null)
                        presence.details = "Playing as " + activeSim;
                    presence.state = jobStr;
                }
                else
                {
                    if (activeSim == null)
                    {
                        presence.state = lotName ?? "Idle in City";
                        presence.details = "";
                    }
                    else
                    {
                        presence.details = "Playing as " + activeSim;
                        presence.state = lotName ?? "Idle in City";
                    }
                }

            }
            else
            {
                presence.state = "Online";
                presence.details = "Privacy Enabled";
            }


            presence.largeImageKey = "sunrise_crater";
            presence.largeImageText = "Sunrise Crater";

            if (lotName != null && !isPrivate)
            {
                presence.joinSecret = lotID + "#" + lotName;
                //presence.matchSecret = lotID + "#" + lotName+".";
                presence.spectateSecret = lotID + "#" + lotName + "..";
                presence.partyMax = maxSize;
                presence.partySize = players;
                presence.partyId = lotID.ToString();

                presence.largeImageKey = "cat_" + catID;
                presence.largeImageText = CapFirstWord(((LotCategory)catID).ToString());
            }

            UpdatePresence(ref presence);
        }

        private static string CapFirstWord(string cat)
        {
            return char.ToUpperInvariant(cat[0]) + cat.Substring(1);
        }

        public static void Ready()
        {
            Active = true;
        }

        public static void Error(int errorCode, string message)
        {

        }

        public static void Join(string secret)
        {
            Secret = secret;
        }

        public static void Spectate(string secret)
        {
            Secret = secret;
        }

        public static void Disconnected(int errorCode, string message)
        {

        }

        public static void Request(JoinRequest request)
        {

        }
    }
}