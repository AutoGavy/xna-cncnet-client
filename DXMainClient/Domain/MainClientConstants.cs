using ClientCore;
using System;
using System.IO;
using System.Windows.Forms;

namespace DTAClient.Domain
{
    public static class MainClientConstants
    {
        public const string CNCNET_TUNNEL_LIST_URL = "http://cncnet.org/master-list";

        public static string GAME_NAME_LONG = "CnCNet Client";
        public static string GAME_NAME_SHORT = "CnCNet";

        public static string CREDITS_URL = "http://rampastring.cncnet.org/TS/Credits.txt";
        /*public static string QQ_URL = ClientConfiguration.Instance.ClientLanguage == 0 ?
            "http://jq.qq.com/?_wv=1027&k=5mo51z3" : "http://jq.qq.com/?_wv=1027&k=5cRWqHk";*/

        public static string SUPPORT_URL_SHORT = "www.cncnet.org";

        public static int MAP_CELL_SIZE_X = 48;
        public static int MAP_CELL_SIZE_Y = 24;

        public static OSVersion OSId = OSVersion.UNKNOWN;

        public static readonly int[] ResolutionList =
{
            1920,	// 1920 x 1080
	        1600,	// 1600 x 900
	        1366,	// 1366 x 768
	        1280,	// 1280 x 720
	        1024		// 1024 × 576
        };

        public static void Initialize()
        {
            var clientConfiguration = ClientConfiguration.Instance;

            OSId = clientConfiguration.GetOperatingSystemVersion();

            GAME_NAME_SHORT = clientConfiguration.LocalGame;
            GAME_NAME_LONG = clientConfiguration.LongGameName;

            SUPPORT_URL_SHORT = clientConfiguration.ShortSupportURL;

            CREDITS_URL = clientConfiguration.CreditsURL;
            //QQ_URL = clientConfiguration.QQURL;

            MAP_CELL_SIZE_X = clientConfiguration.MapCellSizeX;
            MAP_CELL_SIZE_Y = clientConfiguration.MapCellSizeY;

            if (string.IsNullOrEmpty(GAME_NAME_SHORT))
                throw new ClientConfigurationException("LocalGame is set to an empty value.");

            if (GAME_NAME_SHORT.Length > ProgramConstants.GAME_ID_MAX_LENGTH)
            {
                throw new ClientConfigurationException("LocalGame is set to a value that exceeds length limit of " +
                    ProgramConstants.GAME_ID_MAX_LENGTH + " characters.");
            }
        }
    }
}
