using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using System.Linq;
using Terraria;
using TShockAPI;

namespace BossSpawnLimiter
{
    public delegate bool GetDataHandlerDelegate(GetDataHandlerArgs args);

    public class GetDataHandlerArgs : EventArgs
    {
        public TSPlayer Player { get; private set; }
        public MemoryStream Data { get; private set; }

        public Player TPlayer
        {
            get { return Player.TPlayer; }
        }

        public GetDataHandlerArgs(TSPlayer player, MemoryStream data)
        {
            Player = player;
            Data = data;
        }
    }

    public static class GetDataHandlers
    {
        private static Dictionary<PacketTypes, GetDataHandlerDelegate> GetDataHandlerDelegates;
        public static List<Users> Users = new List<Users>();

        public static void InitGetDataHandler()
        {
            GetDataHandlerDelegates = new Dictionary<PacketTypes, GetDataHandlerDelegate>
            {
                {PacketTypes.SpawnBossorInvasion, HandleSpawnBossorInvasion}
            };

        }

        public static bool HandlerGetData(PacketTypes type, TSPlayer player, MemoryStream data, List<Users> userdata)
        {
            GetDataHandlerDelegate handler;
            Users = userdata;

            if (GetDataHandlerDelegates.TryGetValue(type, out handler))
            {
                try
                {
                    return handler(new GetDataHandlerArgs(player, data));
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                }
            }
            return false;
        }

        private static bool HandleSpawnBossorInvasion(GetDataHandlerArgs args)
        {
            var plr = args.Data.ReadInt16();
            var npctype = args.Data.ReadInt16();
            NPC npc = new NPC();
            npc.SetDefaults(npctype);
            int limit = 5;

            if (plr != args.Player.Index)
                return true;

            //Refer to https://tshock.readme.io/docs/multiplayer-packet-structure#spawn-boss-invasion-61
            int npcs = 0;
            switch (npctype)
            {
                case -10:
                    npcs = 1;
                    break;
                case -8:
                    npcs = 1;
                    break;
                case -7:
                    npcs = 1;
                    break;
                case -6:
                    npcs = 1;
                    break;
                case -5:
                    npcs = 1;
                    break;
                case -4:
                    npcs = 1;
                    break;
                case -3:
                    npcs = 1;
                    break;
                case -2:
                    npcs = 1;
                    break;
                case -1:
                    npcs = 1;
                    break;
                default:
                    npcs = 0;
                    break;
            }

            Users PlayerData = Users.FirstOrDefault(i => i.UserID == args.Player.Account.ID);
            if (PlayerData == null)
            {
                Users.Add(new Users { UserID = args.Player.Account.ID, TotalSpawned = 0 });
            }

            if (npcs == 1)
            {
                if (!args.Player.HasPermission("bsl.adminbypass"))
                {
                    //A bit crude double checker, but if it works then it works
                    PlayerData = Users.First(i => i.UserID == args.Player.Account.ID);

                    //Hardcoded Limit :3, you could however make a config file for it.
                    if (PlayerData.TotalSpawned >= limit)
                    {
                        args.Player.SendErrorMessage($"You have reached the maximum amount of Boss/Invansion Spawn for today.");
                        return true;
                    }

                    Users.Where(w => w.UserID == args.Player.Account.ID).ForEach(w => w.TotalSpawned = w.TotalSpawned + 1);
                    args.Player.SendInfoMessage($"Your daily Boss/Invasion Limit is {PlayerData.TotalSpawned}/{limit}");
                }
            }

            return false;
        }
    }
}
