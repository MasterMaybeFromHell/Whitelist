namespace MasterHell.Config
{
    public struct Config
    {
        public Config()
        {
            OnlineWhitelist = false;
            LinkToOnlineWhitelist = "";
        }

        public bool OnlineWhitelist { get; set; }
        public string LinkToOnlineWhitelist { get; set; }
    }
}