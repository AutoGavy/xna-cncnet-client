using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Rampastring.Tools;
using ClientCore;
using ClientCore.Statistics;

namespace DTAClient.Domain
{
    public static class LogbuchParser
    {
        private static readonly string gamepath = ProgramConstants.GamePath;
        private static readonly string fileName = ClientConfiguration.Instance.StatisticsLogFileName;

        private const string DEBUG_FILENAME = "debug/debug.log";
        private const string PROFILE_NAME = "Client/profile_data";

        public static string scoreSong = String.Empty;
        public static bool SongEnded = true;

        public static event Action ScoreSongStateChanged;

        private static void ActiveScoreSong()
        {
            SongEnded = false;
            ScoreSongStateChanged?.Invoke();
        }

        private static string RandomName(string name)
        {
            Random random = new Random();
            if (Convert.ToBoolean(random.Next(0, 2)))
                return name + "final";
            else
                return name;
        }

        private static string GetSkirmishResult(MatchStatistics ms)
        {
            string longResult = String.Empty;
            bool bCanceled = false;

            if (ms.GetPlayerCount() <= 1 && !ms.MapIsCoop)
            {
                Logger.Log("Skipping playing score song: Game only had one player.");
                return longResult;
            }

            if (ms.LengthInSeconds < 60)
            {
                Logger.Log("Playing losing-score song: The game was cancelled.");
                bCanceled = true;
            }

            PlayerStatistics localPlayer = ms.Players.Find(p => p.IsLocalPlayer);

            if (localPlayer == null || localPlayer.WasSpectator)
            {
                Logger.Log("Skipping playing score song: Local player is null or a spectator.");
                return longResult;
            }

            switch (localPlayer.Side)
            {
                case 0:
                case 1:
                case 2:
                case 4:
                    longResult = "gdi";
                    break;
                case 5:
                case 6:
                case 8:
                case 9:
                    longResult = "nod";
                    break;
                case 10:
                case 11:
                case 3:
                case 7:
                    longResult = "nod";
                    break;
                default:
                    Logger.Log("Skipping playing score song: Unknown side.");
                    return longResult;
            }

            longResult = RandomName(longResult);

            if (bCanceled)
            {
                if (longResult.Contains("final"))
                    longResult = longResult.Substring(0, 3) + "lose";
                else
                    longResult += "lose";
            }
            else if (localPlayer.Won)
                longResult += "win";
            else
            {
                if (longResult.Contains("final"))
                    longResult = longResult.Substring(0, 3) + "lose";
                else
                    longResult += "lose";
            }

            return longResult;
        }

        public static void ParseForCampaign(string loadedSide = null, string loadedMisson = null, int difficultyIndex = -1)
        {
            if (!File.Exists(gamepath + DEBUG_FILENAME))
            {
                Logger.Log("LogbuchParser: Failed to read logbuch: the log file does not exist.");
                return;
            }

            Logger.Log("Attempting to read campaign logbuch from " + DEBUG_FILENAME);

            try
            {
                IniFile profileIni = new IniFile(ProgramConstants.GamePath + PROFILE_NAME);
                StreamReader reader = new StreamReader(File.OpenRead(gamepath + DEBUG_FILENAME));
                string side = String.Empty;
                string result = "lose";
                string line;
                string finalSide = String.Empty;
                string curMission = String.Empty;

                bool bShouldUnlock = true;
                bool bEndFound = false;
                bool bFirstRun = true;
                while ((line = reader.ReadLine()) != null)
                {
                    // check mission side
                    if (bFirstRun && !String.IsNullOrEmpty(loadedSide))
                    {
                        switch (loadedSide)
                        {
                            case "GDI":
                                side = RandomName("gdi");
                                break;
                            case "Nod":
                                side = RandomName("nod");
                                break;
                            case "Scrin":
                                side = RandomName("nod");
                                break;
                        }
                        bFirstRun = false;
                    }
                    else if (line.Contains("Starting scnenario: TRA") || line.Contains("Starting scnenario: GDI") || line.Contains("Starting scnenario: END"))
                    {
                        Logger.Log("TRA / GDI / End Mission detected");
                        if (line.Contains("GDI08"))
                        {
                            finalSide = "gdi";
                            side = "gdifinal";
                        }
                        else if (line.Contains("END08"))
                        {
                            finalSide = "end";
                            side = "gdifinal";
                        }
                        else if (line.Contains("END01") || line.Contains("END06"))
                            side = RandomName("nod");
                        else
                            side = RandomName("gdi");
                    }
                    else if (line.Contains("Starting scnenario: NOD") || line.Contains("Starting scnenario: NOF") || line.Contains("Starting scnenario: SCR") || line.Contains("Starting scnenario: SCT"))
                    {
                        Logger.Log("Nod / Scrin Mission detected");
                        if (line.Contains("NOD08"))
                        {
                            finalSide = "nod";
                            side = "nodfinal";
                        }
                        else if (line.Contains("SCR04"))
                        {
                            finalSide = "scrin";
                            side = "nodfinal";
                        }
                        else
                            side = RandomName("nod");
                    }

                    // TC2 mission?
                    if (line.Contains("Starting scnenario: PRL"))
                    {
                        Logger.Log("PROL Mission detected");
                        side = line.Substring(20).ToLower();
                    }
                    else if (line.Contains("Starting scnenario: GDO"))
                    {
                        Logger.Log("GDO Mission detected");
                        side = line.Substring(20).ToLower();
                    }
                    else if (line.Contains("Starting scnenario: TRA"))
                    {
                        Logger.Log("TRA Mission detected");
                        side = line.Substring(20).ToLower();
                    }

                    // check SW
                    if (line.Contains("[LAUNCH] MSWIN_")) // check if player won this mission
                    {
                        if (line.Contains("PRL1"))
                        {
                            Logger.Log("PRL1 Completed");

                            side = "prl1";
                            curMission = "PRL1";
                            profileIni.SetBooleanValue("General", "PRL2", true);

                            Logger.Log("PRL1 Completed, skip 1");
                            profileIni.SetBooleanValue("General", "GDO1", true);
                        }
                        else if (line.Contains("PRL2"))
                        {
                            Logger.Log("PRL2 Completed");

                            side = "prl2";
                            curMission = "PRL2";
                            profileIni.SetBooleanValue("General", "GDO1", true);
                        }
                        else if (line.Contains("GDO2"))
                        {
                            Logger.Log("GDO2 Completed, skip 1");

                            profileIni.SetBooleanValue("General", "GDO4", true);
                        }

                        //for (int i = 1; i <= 12; i++)
                        for (int i = 1; i <= 5; i++) // max enabled to gdo5
                        {
                            string missionIndex = i.ToString();
                            if (line.Contains("GDO" + missionIndex))
                            {
                                Logger.Log("GDO" + missionIndex + " Completed");

                                side = "gdo" + missionIndex;
                                curMission = "GDO" + missionIndex;

                                if (i != 5)
                                    profileIni.SetBooleanValue("General", "GDO" + (i + 1).ToString(), true);
                            }
                        }

                        // unlocking database
                        IniFile campaignPanelIni = new IniFile(ProgramConstants.GetBaseResourcePath() + "CampaignPanel.ini");

                        string strUnlockData = campaignPanelIni.GetStringValue(curMission, "DataUnlock", String.Empty);
                        if (!string.IsNullOrEmpty(strUnlockData))
                        {
                            string[] unlockData = strUnlockData.Split(',');
                            foreach (string data in unlockData)
                            {
                                profileIni.SetBooleanValue(data, "Enable", true);
                                profileIni.SetBooleanValue(data, "New", true);
                            }
                        }
                    }
                    else if (line.Contains("[LAUNCH] DATA_")) // parse player achieved data
                    {
                        string dataName = line.Substring(15);
                        Logger.Log("Data unlocked: " + dataName);
                        if (!profileIni.SectionExists(dataName))
                        {
                            profileIni.AddSection(dataName);
                        }
                        profileIni.SetBooleanValue(dataName, "Enable", true);
                    }
                    else if (line.Contains("[LAUNCH] MEDAL_")) // parse player achieved medal
                    {
                        string medalName = line.Substring(15);
                        Logger.Log("Medal unlocked: " + medalName);
                        if (!profileIni.SectionExists(medalName))
                        {
                            profileIni.AddSection(medalName);
                        }
                        profileIni.SetBooleanValue(medalName, "Enable", true);
                    }

                    if (bShouldUnlock && line.Contains("LoadGame : "))
                    {
                        bShouldUnlock = false;
                        Logger.Log("Disallowed unlocking eggsides because player loaded an another game.");
                    }

                    if (bEndFound)
                    {
                        if (line.Contains("_Movie() as Bink"))
                        {
                            Logger.Log("Play_Movie() found, set as winning");
                            result = "win";
                        }
                        else if (line.Contains("NTRLMD.MIXLOADED NEUTRAL.MIXRelease_Mouse") || line.Contains("NTRLMD.MIXLOADED NEUTRAL.MIXTooltips"))
                        {
                            Logger.Log("Second end sentence found, set as losing");
                            result = "lose";
                        }
                    }
                    else if (line.Contains("NTRLMD.MIXLOADED NEUTRAL.MIXRelease_Mouse") || line.Contains("NTRLMD.MIXLOADED NEUTRAL.MIXTooltips"))
                        bEndFound = true;
                }

                reader.Close();

                if (!String.IsNullOrEmpty(side))
                {
                    if (result == "win" && !String.IsNullOrEmpty(loadedMisson))
                    {
                        switch (loadedMisson)
                        {
                            case "gdi08":
                                finalSide = "gdi";
                                side = "gdifinal";
                                break;
                            case "nod08":
                                finalSide = "nod";
                                side = "nodfinal";
                                break;
                            case "scr04":
                                finalSide = "scrin";
                                side = "nodfinal";
                                break;
                            case "end02":
                                finalSide = "end";
                                side = "gdifinal";
                                break;
                        }
                    }

                    // training mission?
                    if (side.Contains("tra"))
                    {
                        if (result == "lose") // player lost
                        {
                            scoreSong = "gdilose";
                        }
                        else // player won
                        {
                            scoreSong = "gdiwin";
                            UserINISettings.Instance.ReloadSettings();
                            UserINISettings.Instance.TutorialCompleted.Value = true;
                            UserINISettings.Instance.SaveSettings();
                        }
                    }
                    // check if it's TC2 mission
                    else if (side.Contains("prl") || side.Contains("gdo") || side.Contains("nof") || side.Contains("sct"))
                    {
                        if (result == "lose") // player lost
                        {
                            scoreSong = "gdilose";
                        }
                        else // player won
                        {
                            switch (side)
                            {
                                case "gdo1":
                                    scoreSong = "gdolose1";
                                    break;
                                case "gdo5":
                                //case "gdo12":
                                    scoreSong = "gdifinalwin";
                                    UserINISettings.Instance.ReloadSettings();
                                    UserINISettings.Instance.TC2Completed.Value = true;
                                    UserINISettings.Instance.SaveSettings();
                                    break;
                                default:
                                    scoreSong = "gdiwin";
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (result == "lose" && side.Contains("final"))
                            scoreSong = side.Substring(0, 3) + result;
                        else
                            scoreSong = side + result;
                    }

                    ActiveScoreSong();
                    Logger.Log("Setting SongEnded to false value. Socre Song: " + scoreSong);

                    // no sl
                    if (String.IsNullOrEmpty(loadedMisson) && bShouldUnlock && result == "win")
                    {
                        Logger.Log("Player achieved NO SL in this mission.");
                        // unlocking no sl medal
                        if (!String.IsNullOrEmpty(curMission))
                        {
                            string diffcultyName = "none";
                            if (difficultyIndex < 0 || difficultyIndex > 3)
                                difficultyIndex = 0;
                            switch (difficultyIndex)
                            {
                                case 0:
                                    diffcultyName = "easy";
                                    break;
                                case 1:
                                    diffcultyName = "normal";
                                    break;
                                case 2:
                                    diffcultyName = "hard";
                                    break;
                                case 3:
                                    diffcultyName = "abyss";
                                    break;
                            }
                            profileIni.SetStringValue(curMission, "DifficultyMedal", diffcultyName);
                        }

                        // unlocking ra2 factions
                        if (!String.IsNullOrEmpty(finalSide))
                        {
                            UserINISettings.Instance.ReloadSettings();
                            switch (finalSide)
                            {
                                case "gdi":
                                    UserINISettings.Instance.EggSide1.Value = true;
                                    break;
                                case "nod":
                                    UserINISettings.Instance.EggSide2.Value = true;
                                    break;
                                case "scrin":
                                    UserINISettings.Instance.EggSide3.Value = true;
                                    break;
                                case "end":
                                    UserINISettings.Instance.EggSide4.Value = true;
                                    break;
                            }
                            UserINISettings.Instance.SaveSettings();
                        }
                    }
                }
                else
                    Logger.Log("Skipping playing score song: side is empty.");

                profileIni.WriteIniFile();
            }
            catch (Exception ex)
            {
                Logger.Log("LogbuchsParser: Error parsing log file! Message: " + ex.Message);
            }

            return;
        }

        public static void ParseForSkirmish(MatchStatistics ms)
        {
            if (!File.Exists(gamepath + fileName))
            {
                Logger.Log("LogbuchParser: Failed to read logbuch: the log file does not exist.");
                return;
            }

            string longResult = GetSkirmishResult(ms);

            if (!String.IsNullOrEmpty(longResult))
            {
                scoreSong = longResult;
                ActiveScoreSong();
            }
        }

        public static void ParseForLoadedSkirmish(string loadedSide = null)
        {
            if (!File.Exists(gamepath + fileName))
            {
                Logger.Log("LogbuchParser: Failed to read logbuch: the log file does not exist.");
                return;
            }

            if (String.IsNullOrEmpty(loadedSide))
            {
                Logger.Log("LogbuchParser: loadedSide is empty, wtf.");
                return;
            }

            string side = String.Empty;
            switch (loadedSide)
            {
                case "GDI":
                    side = RandomName("gdi");
                    break;
                case "Nod":
                    side = RandomName("nod");
                    break;
                case "Scrin":
                    side = RandomName("nod");
                    break;
            }

            Logger.Log("Attempting to read skirmish loaded logbuch from " + fileName);

            try
            {
                StreamReader reader = new StreamReader(File.OpenRead(gamepath + fileName));
                string result = String.Empty;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(": Loser"))
                    {
                        string playerName = line.Substring(0, line.Length - 7);
                        if (playerName == ProgramConstants.PLAYERNAME)
                        {
                            Logger.Log("Found loser " + playerName + ", local player named " + ProgramConstants.PLAYERNAME);
                            result = "lose";
                        }
                    }
                    else if (line.Contains(": Winner"))
                    {
                        string playerName = line.Substring(0, line.Length - 8);
                        if (playerName == ProgramConstants.PLAYERNAME)
                        {
                            Logger.Log("Found winner " + playerName + ", local player named " + ProgramConstants.PLAYERNAME);
                            result = "win";
                        }
                    }
                }

                reader.Close();

                if (String.IsNullOrEmpty(result))
                {
                    Logger.Log("Skipping playing score music: Player was a spectator.");
                    return;
                }

                if (result == "lose" && side.Contains("final"))
                    scoreSong = side.Substring(0, 3) + result;
                else
                    scoreSong = side + result;

                ActiveScoreSong();
            }
            catch (Exception ex)
            {
                Logger.Log("LogbuchParser: Error parsing score song from loaded skirmish! Message: " + ex.Message);
            }
        }

        public static void ClearTrash()
        {
            Logger.Log("Clearing other debug logs...");
            if (Directory.Exists(ProgramConstants.GamePath + "debug"))
            {
                List<string> files = Directory.GetFiles(ProgramConstants.GamePath + "debug", "debug.*.log", SearchOption.TopDirectoryOnly).ToList();
                files.Sort();
                foreach (string logFile in files)
                {
                    try
                    {
                        File.Delete(logFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error deleting log files, message: " + ex.Message);
                    }
                }

                try
                {
                    File.Delete(gamepath + fileName);
                }
                catch (Exception ex)
                {
                    Logger.Log("Error deleting log files, message: " + ex.Message);
                }
            }

            if (Directory.Exists(ProgramConstants.GamePath + "GameShaders"))
            {
                StreamWriter sw = new StreamWriter(ProgramConstants.GamePath + "GameShaders/TCMainShader.ini");
                sw.WriteLine();
                sw.Close();
            }
        }
    }
}
