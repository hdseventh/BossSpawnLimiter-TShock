using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.IO;
using System.Timers;

namespace BossSpawnLimiter
{
    [ApiVersion(2, 1)]
    public class BossSpawnLimiter : TerrariaPlugin
    {
        public override string Author => "hdseventh";
        public override string Description => "BossSpawnLimiter for TShock";
        public override string Name => "BossSpawnLimiter";
        public override Version Version { get { return new Version(1, 0, 0, 0); } }
        public BossSpawnLimiter(Main game) : base(game) { }
        public List<Users> Users = new List<Users>();

        public Timer timerchecker = new Timer();
        private DateTime nextRun = DateTime.MinValue;

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.NetGetData.Register(this, onGetData);
            GetDataHandlers.InitGetDataHandler();
        }

        public void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("bsl.adminbypass",menuBSL, "bslreset"));

            timerchecker.Interval = 10000;
            timerchecker.AutoReset = true;
            timerchecker.Elapsed += new ElapsedEventHandler(timeroff);
            timerchecker.Start();
        }

        public void timeroff(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now < nextRun)
            {
                return;
            }

            nextRun = GetNextRun(DateTime.Now);

            //clear the Users List
            GetDataHandlers.Users.Clear();
            TShockAPI.TSPlayer.All.SendInfoMessage("Boss/Invasions Spawn Limit has been reseted.");
        }
        private static DateTime GetNextRun(DateTime lastRun)
        {
            var next = lastRun.AddDays(1);
            //Will reset at 9 am
            return new DateTime(next.Year, next.Month, next.Day, 9, 0, 0);
        }

        private void menuBSL(CommandArgs args)
        {
            GetDataHandlers.Users.Clear();
            args.Player.SendSuccessMessage("BossSpawnLimiter has been reseted.");
        }
        private void onGetData(GetDataEventArgs e)
        {
            PacketTypes type = e.MsgID;
            var player = TShock.Players[e.Msg.whoAmI];
            if (player == null || !player.ConnectionAlive)
            {
                e.Handled = true;
                return;
            }

            using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
            {
                try
                {
                    if (GetDataHandlers.HandlerGetData(type, player, data, Users))
                        e.Handled = true;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                }
            }
        }

    }
}