using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using MasterHell.Config;

[assembly: MelonInfo(typeof(Whitelist.Main), "Whitelist", "1.0.0", "MasterHell", null)]
[assembly: MelonGame("ZeoWorks", "Slendytubbies 3")]
[assembly: MelonColor(1, 255, 255, 255)]

namespace Whitelist
{
    public class Main : MelonMod
    {
        public static string WhiteList;
        private Config _config;
        private ConfigManager _configManager = new();
        private string _whitelistConfigPath = "UserData\\Whitelist\\Config.json";
        private string _whitelistPath = "UserData\\Whitelist\\Whitelist.txt";

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
            _config = _configManager.Load(_whitelistConfigPath);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu" || sceneName == "Updater")
                SetupConfigAsync().Wait();
        }

        private async Task SetupConfigAsync()
        {
            if (!File.Exists(_whitelistPath))
                File.Create(_whitelistPath).Dispose();
            else
                WhiteList = File.ReadAllText(_whitelistPath);

            if (_config.OnlineWhitelist)
                WhiteList = await DownloadFile(_config.LinkToOnlineWhitelist);
        }

        private static async Task<string> DownloadFile(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return null;

            using (HttpClient httpClient = new())
            {
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(uri);

                if (httpResponseMessage.IsSuccessStatusCode)
                    return await httpResponseMessage.Content.ReadAsStringAsync();
            }

            return null;
        }

        public static bool CheckPlayer(string playerName)
        {
            if (string.IsNullOrEmpty(WhiteList))
                return false;

            string[] blackList = WhiteList.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

            foreach (string player in blackList)
            {
                string trimmedPlayer = player.Trim();

                if (trimmedPlayer.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static void Kick(int photonPlayerID)
        {
            if (!IsPlayerExist(photonPlayerID))
                return;

            RaiseEventOptions raiseEventOptions = new() { TargetActors = new int[] { photonPlayerID } };
            PhotonNetwork.networkingPeer.OpRaiseEvent(203, null, true, raiseEventOptions);

            Il2CppSystem.Int32 @int = default;
            @int.m_value = photonPlayerID;
            Il2CppSystem.Object eventContent = @int.BoxIl2CppObject();
            PhotonNetwork.RaiseEvent(1, eventContent, true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
        }

        public static void Crash(PhotonPlayer photonPlayer)
        {
            if (!IsPlayerExist(photonPlayer))
                return;

            Il2CppSystem.Single single = default;
            single.m_value = 1E+09f;

            Il2CppSystem.Object[] parameters = ["syncShotGun", single.BoxIl2CppObject()];
            PhotonView photonView = GetPlayer(photonPlayer);
            photonView?.RPC("BCPFIMDIMJE", photonPlayer, parameters);
        }

        public static bool IsPlayerExist(PhotonPlayer photonPlayer)
        {
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (player.NickName.Contains(photonPlayer.NickName))
                    return true;
            }

            return false;
        }

        public static bool IsPlayerExist(int photonPlayerID)
        {
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (player.ID == photonPlayerID)
                    return true;
            }

            return false;
        }

        public static PhotonView GetPlayer(PhotonPlayer photonPlayer)
        {
            GameObject gameObject = GameObject.Find(photonPlayer.name.Split('|')[0]);
            return gameObject?.GetComponent<PhotonView>();
        }
    }
    
    [HarmonyPatch(typeof(WhoKilledWho), "OnPhotonPlayerConnected")]
    public static class OnPlayerJoined
    {
        [HarmonyPrefix]
        private static void Prefix(ref PhotonPlayer otherPlayer, WhoKilledWho __instance)
        {
            string nickName = otherPlayer.name.Split(['|'])[0];

            if (!Main.CheckPlayer(nickName) && PhotonNetwork.isMasterClient)
            {
                Main.Kick(otherPlayer.ID);
                PhotonNetwork.CloseConnection(otherPlayer);
                PhotonNetwork.DestroyPlayerObjects(otherPlayer);
                Main.Crash(otherPlayer);
                MelonLogger.Warning($"Attention: Not Whitelisted Player: {nickName}");
            }
        }
    }
}