using Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rampastring.Tools;

namespace ClientCore
{
    /// <summary>
    /// Contains various static variables and constants that the client uses for operation.
    /// </summary>
    public static class ProgramConstants
    {
#if DEBUG
        public static readonly string GamePath = Application.StartupPath.Replace('\\', '/') + "/";
#else
        public static readonly string GamePath = Directory.GetParent(Application.StartupPath.TrimEnd(new char[] { '\\' })).FullName.Replace('\\', '/') + "/";
#endif

        public static string ClientUserFilesPath => GamePath + "Client/";

        public static event EventHandler PlayerNameChanged;

        public const string QRES_EXECUTABLE = "qres.dat";

        public const string CNCNET_PROTOCOL_REVISION = "R8";
        public const string LAN_PROTOCOL_REVISION = "RL5";
        public const int LAN_PORT = 1234;
        public const int LAN_INGAME_PORT = 1234;
        public const int LAN_LOBBY_PORT = 1232;
        public const int LAN_GAME_LOBBY_PORT = 1233;
        public const char LAN_DATA_SEPARATOR = (char)01;
        public const char LAN_MESSAGE_SEPARATOR = (char)02;

        public const string SPAWNMAP_INI = "spawnmap.ini";
        public const string SPAWNER_SETTINGS = "spawn.ini";
        public const string SAVED_GAME_SPAWN_INI = "Saved Games/spawnSG.ini";
        public const string MAIN_EXE = "mainexecutable.exe";
        public const string MAIN_EXE_2 = "mainexecutable2.exe";
        public const string MAIN_EXE_3 = "mainexecutable3.exe";
        public const string DISABLE_WIN = "tcdisablewin.exe";
        public const string DISABLE_WIN_PROCESS = "tcdisablewin";

        public const int GAME_ID_MAX_LENGTH = 4;

        public static readonly Encoding LAN_ENCODING = Encoding.UTF8;

        private const string BASE_SHARED_DIR = "GameShaders/BaseShared/";

        public static string GAME_VERSION = "Developer Mode";
        private static string PlayerName = "No name";

        public static string PLAYERNAME
        {
            get { return PlayerName; }
            set
            {
                string oldPlayerName = PlayerName;
                PlayerName = value;
                if (oldPlayerName != PlayerName)
                    PlayerNameChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string BASE_RESOURCE_PATH = "Resources/";
        public static string RESOURCES_DIR = BASE_RESOURCE_PATH;

        public static int LOG_LEVEL = 1;

        public static bool IsInGame { get; set; }

        public static Microsoft.Xna.Framework.Graphics.Texture2D CheckBoxClearHoverTexture { get; set; }

        public static Microsoft.Xna.Framework.Graphics.Texture2D CheckBoxCheckedHoverTexture { get; set; }

        public static string GetResourcePath()
        {
            return GamePath + RESOURCES_DIR;
        }

        public static string GetBaseResourcePath()
        {
            return GamePath + BASE_RESOURCE_PATH;
        }

        public const string GAME_INVITE_CTCP_COMMAND = "INVITE";
        public const string GAME_INVITATION_FAILED_CTCP_COMMAND = "INVITATION_FAILED";

        public static readonly List<string> TEAMS = new List<string> { "A", "B", "C", "D" };
        // Static fields might be initialized before the translation file is loaded. Change to readonly properties here.
        public static List<string> AI_PLAYER_NAMES => new List<string>
        {
            "Easy AI".L10N("UI:Main:EasyAI"),
            "Normal AI".L10N("UI:Main:NormalAI"),
            "Hard AI".L10N("UI:Main:HardAI"),
            "Insane AI".L10N("UI:Main:InsaneAI"),
            "Brutal AI".L10N("UI:Main:BrutalAI"),
            "Abyss AI".L10N("UI:Main:AbyssAI"),
            "Abyss+1 AI".L10N("UI:Main:Abyss1AI"),
            "Abyss+2 AI".L10N("UI:Main:Abyss2AI"),
            "Abyss+3 AI".L10N("UI:Main:Abyss3AI")
        };

        public static string GetBaseSharedPath()
        {
            return GamePath + BASE_SHARED_DIR;
        }

        public static string SortText(string longText, int limit, bool bWordwrap = true)
        {
            if (bWordwrap)
            {
                string[] textArray = longText.Split('@');
                for (int i = 0; i < textArray.Length; i++)
                {
                    if (textArray[i] == "")
                    {
                        textArray[i] = "@";
                        continue;
                    }

                    int iCount = 0;
                    for (int c = 0; c < textArray[i].Length; c++)
                    {
                        if ((int)textArray[i][c] < 128)
                        {
                            iCount++;
                        }
                        else
                        {
                            iCount += 3;
                        }

                        if (iCount > limit)
                        {
                            textArray[i] = textArray[i].Insert(++c, "@");
                            iCount = 0;
                            c++;
                        }
                    }
                }

                longText = String.Empty;
                foreach (string lineText in textArray)
                {
                    longText += lineText.Insert(lineText.Length, "@");
                }
            }

            return longText.Replace("@", Environment.NewLine);
        }

        public static void SetupPreset(bool bIsOnlineGame = false) // Set up shader presets
        {
            // Set up ddraw.ini
            if (!UserINISettings.Instance.DebugReShade)
            {
                IniFile ddrawIni = new IniFile(ProgramConstants.GamePath + "ddraw.ini");
                if (bIsOnlineGame)
                {
                    ddrawIni.SetIntValue("gamemd", "maxfps", 30);
                }
                else
                {
                    int val = ddrawIni.GetIntValue("gamemd", "maxfps", 0);
                    if (val >= 60)
                    {
                        ddrawIni.SetIntValue("gamemd", "maxfps", 60);
                    }
                    else
                    {
                        ddrawIni.SetIntValue("gamemd", "maxfps", 30);
                    }
                }

                ddrawIni.SetIntValue("gamemd", "minfps", -1);
                //ddrawIni.SetIntValue("gamemd", "maxgameticks", -2);
                ddrawIni.SetBooleanValue("gamemd", "devmode", false);
                ddrawIni.SetBooleanValue("gamemd", "resizeable", false);
                ddrawIni.SetBooleanValue("gamemd", "fullscreen", false);
                ddrawIni.SetBooleanValue("gamemd", "nonexclusive", true);
                ddrawIni.SetBooleanValue("gamemd", "singlecpu", !UserINISettings.Instance.MultiCPU);
                ddrawIni.WriteIniFile();
            }

            switch (UserINISettings.Instance.CloudsEffect)
            {
                case 1:
                    ClientConfiguration.TC_TINT_DAY = ClientConfiguration.SHADER_TINT_DAY;
                    ClientConfiguration.TC_TINT_NIGHT = ClientConfiguration.SHADER_TINT_NONE;
                    ClientConfiguration.TC_TINT_NIGHT2 = ClientConfiguration.SHADER_TINT_NONE;
                    ClientConfiguration.TC_TINT_SNOWDAY = ClientConfiguration.SHADER_TINT_SNOWDAY_VANILLA;
                    ClientConfiguration.TC_TINT_SNOWNIGHT = ClientConfiguration.SHADER_TINT_SNOW_LIGHT;
                    break;
                default:
                    ClientConfiguration.TC_TINT_DAY = ClientConfiguration.SHADER_TINT_DAY;
                    ClientConfiguration.TC_TINT_NIGHT = ClientConfiguration.SHADER_TINT_NONE;
                    ClientConfiguration.TC_TINT_NIGHT2 = ClientConfiguration.SHADER_TINT_NONE;
                    ClientConfiguration.TC_TINT_SNOWDAY = ClientConfiguration.SHADER_TINT_SNOWDAY_VANILLA;
                    ClientConfiguration.TC_TINT_SNOWNIGHT = ClientConfiguration.SHADER_TINT_SNOWNIGHT_VANILLA;
                    break;
            }
        }
    }
}
