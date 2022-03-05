using ClientCore;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using OpenMcdf;

namespace DTAClient.Domain
{
    /// <summary>
    /// A single-player saved game.
    /// </summary>
    public class SavedGame
    {
        const string SAVED_GAME_PATH = "Saved Games/";
        const string findSample = ".MAP";

        private readonly Dictionary<string, string> SideList = new Dictionary<string, string>
        {
            {"GDI-Offense", "GDI"},
            {"GDI-Defense", "GDI"},
            {"GDI-Support", "GDI"},
            {"ZOCOM",       "GDI"},
            {"Nod-Offense", "Nod"},
            {"Nod-Defense", "Nod"},
            {"Nod-Support", "Nod"},
            {"MarkedOfKane","Nod"},
            {"Scrin",       "Scrin"},
            {"Reaper17",    "Scrin"},
            {"Traveler59",  "Scrin"},
            {"Destroyer41", "Scrin"}
        };

        public SavedGame(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }
        public string GUIName { get; private set; }
        public DateTime LastModified { get; private set; }
        public string SideName { get; private set; }
        public string MissionName { get; private set; }

        /// <summary>
        /// Get the saved game's name from a .sav file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string GetArchiveName(Stream file)
        {
            var cf = new CompoundFile(file);
            var archiveNameBytes = cf.RootStorage.GetStream("Scenario Description").GetData();
            var archiveName = System.Text.Encoding.Unicode.GetString(archiveNameBytes);
            archiveName = archiveName.TrimEnd(new char[] { '\0' });
            return archiveName;
        }

        /// <summary>
        /// Reads and sets the saved game's name and last modified date, and returns true if succesful.
        /// </summary>
        /// <returns>True if parsing the info was succesful, otherwise false.</returns>
        public bool ParseInfo()
        {
            try
            {
                using (Stream file = (File.Open(ProgramConstants.GamePath + SAVED_GAME_PATH + FileName, FileMode.Open, FileAccess.Read)))
                {
                    GUIName = GetArchiveName(file);
                }

                LastModified = File.GetLastWriteTime(ProgramConstants.GamePath + SAVED_GAME_PATH + FileName);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occured while parsing saved game " + FileName + ":" +
                    ex.Message);
                return false;
            }
        }

        public bool ParseSideName()
        {
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(ProgramConstants.GamePath + SAVED_GAME_PATH + FileName, FileMode.Open, FileAccess.Read)))
                {
                    br.BaseStream.Position = 2560; // This is wrong. Use OpenMCDF to parse it.
                    string strName = String.Empty;

                    bool wasLastByteZero = false;
                    while (true)
                    {
                        byte characterByte = br.ReadByte();
                        if (characterByte == 0)
                        {
                            if (wasLastByteZero)
                                break;
                            wasLastByteZero = true;
                        }
                        else
                        {
                            wasLastByteZero = false;
                            strName += Convert.ToChar(characterByte);
                        }
                    }

                    br.Close();

                    if (String.IsNullOrEmpty(strName))
                        return false;

                    if (!SideList.ContainsKey(strName))
                        return false;

                    Logger.Log("SavedGame: Player side is " + SideList[strName]);
                    SideName = SideList[strName];
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occured while parsing saved game " + FileName + ":" +
                    ex.Message);
                return false;
            }
        }

        public bool ParseMissionName()
        {
            try
            {
                StreamReader reader = new StreamReader(File.OpenRead(ProgramConstants.GamePath + SAVED_GAME_PATH + FileName));
                reader.BaseStream.Position = 2256; // This is wrong. Use OpenMCDF to parse it.
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(findSample))
                    {
                        MissionName = line.Substring(line.IndexOf(findSample) - findSample.Length - 4, 5).ToLower();
                        SideName = MissionName.Substring(0, 3);

                        reader.Close();

                        switch (SideName)
                        {
                            case "gdo":
                            case "gdi":
                                SideName = "GDI";
                                break;
                            case "nof":
                            case "nod":
                                SideName = "Nod";
                                break;
                            case "sct":
                            case "scr":
                                SideName = "Scrin";
                                break;
                            default:
                                SideName = "GDI";
                                break;
                        }

                        Logger.Log("SavedGame: Game mission name is " + MissionName);
                        Logger.Log("SavedGame: Campaign music side name is " + SideName);
                        return true;
                    }
                }

                reader.Close();
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occured while parsing saved game " + FileName + ":" +
                    ex.Message);
                return false;
            }
        }

        public string LightClouds()
        {
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(ProgramConstants.GamePath + SAVED_GAME_PATH + FileName, FileMode.Open, FileAccess.Read)))
                {
                    br.BaseStream.Position = 5146; // This is wrong. Use OpenMCDF to parse it.
                    string strShaderName = String.Empty;

                    byte characterByte = 0;
                    while ((characterByte = br.ReadByte()) != 32)
                        strShaderName += Convert.ToChar(characterByte);

                    br.Close();

                    Logger.Log("SavedGame: Game cloud shader is " + strShaderName);
                    if (strShaderName == "N_")
                        return "special";
                    else if (strShaderName == "D_")
                        return "vanilla";
                    else
                        return "none";
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occured while parsing saved game " + FileName + ":" +
                    ex.Message);
                return "none";
            }
        }

        public string strShader()
        {
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(ProgramConstants.GamePath + SAVED_GAME_PATH + FileName, FileMode.Open, FileAccess.Read)))
                {
                    br.BaseStream.Position = 5133; // This is wrong. Use OpenMCDF to parse it.
                    string strShaderName = String.Empty;

                    byte characterByte = 0;
                    while ((characterByte = br.ReadByte()) != 0)
                        strShaderName += Convert.ToChar(characterByte);

                    br.Close();

                    Logger.Log("SavedGame: Game global shader is " + strShaderName);
                    if (!String.IsNullOrEmpty(strShaderName))
                        return strShaderName;
                }
                return String.Empty;
            }
            catch (Exception ex)
            {
                Logger.Log("An error occured while parsing saved game " + FileName + ":" +
                    ex.Message);
                return String.Empty;
            }
        }
    }
}
