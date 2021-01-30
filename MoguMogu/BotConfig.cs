using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MoguMogu
{
    public class BotConfig
    {
        public static BotConfig config;

        static BotConfig()
        {
            LoadConfig();
            SaveConfig(config);
        }

        [JsonProperty("Bot_Token")] public string BotToken { set; get; } = "Your Bot Token";
        [JsonProperty("Bot_Prefix")] public string BotPrefix { set; get; } = "*";
        [JsonProperty("Bot_Activity")] public string BotActivity { set; get; } = "Dit me hoaq!!!";
        [JsonProperty("Irc_Username")] public string IrcUsername { set; get; } = "Your IRC Username";
        [JsonProperty("Irc_Password")] public string IrcPassword { set; get; } = "Your IRC Password";
        [JsonProperty("Irc_Prefix")] public string IrcPrefix { set; get; } = "!";
        [JsonProperty("Osu_Api_Key")] public string OsuApi { get; set; } = "Your api key here!";
        [JsonProperty("verified_role_name")] public string RoleName { get; set; } = "177013";
        [JsonProperty("Use_MariaDB")] public bool UseMariaDB { get; set; }
        [JsonProperty("Owner_Id")] public ulong[] OwnerID { set; get; } = {634246091293851649, 154605183714852864};
        [JsonProperty("Sheets_Id")] public string Sheets_Id { set; get; } = "177013";
        [JsonProperty("Google_Client_Id")] public string Client_Id { set; get; } = "177013";
        [JsonProperty("Google_Client_Secret")] public string Client_Secret { set; get; } = "177013";

        [JsonProperty("DB_Connection_String")]
        public string ConnectionString { get; set; } =
            "Server=127.0.0.1;database=mogu;UID=root;password=;Convert Zero Datetime=True;Allow Zero Datetime=True;";

        public static void LoadConfig()
        {
            if (File.Exists("config.json"))
            {
                config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllLines("config.json")
                    .Where(v => !v.StartsWith("//") && !string.IsNullOrEmpty(v))
                    .Aggregate("", (current, v) => current + v + "\r\n"));
            }
            else
            {
                SaveConfig(new BotConfig());
                Console.WriteLine("Created default config, press any key to exit!");
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }

        public static void SaveConfig(BotConfig config)
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
        }
    }
}