﻿using System.Collections.Generic;
using Rampastring.XNAUI;
using Microsoft.Xna.Framework.Graphics;
using ClientCore.Properties;
using System.Linq;
using System;
using Rampastring.Tools;

namespace ClientCore.CnCNet5
{
    /// <summary>
    /// A class for storing the collection of supported CnCNet games.
    /// </summary>
    public class GameCollection
    {
        public List<CnCNetGame> GameList { get; private set; }

        public void Initialize(GraphicsDevice gd)
        {
            GameList = new List<CnCNetGame>();

            // Default supported games.
            CnCNetGame[] defaultGames = new CnCNetGame[]
            {
                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-dta",
                    ClientExecutableName = "DTA.exe",
                    GameBroadcastChannel = "#cncnet-dta-games",
                    InternalName = "dta",
                    RegistryInstallPath = "HKCU\\Software\\TheDawnOfTheTiberiumAge",
                    UIName = "Dawn of the Tiberium Age",
                    Texture = AssetLoader.TextureFromImage(Resources.dtaicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-ti",
                    ClientExecutableName = "TI_Launcher.exe",
                    GameBroadcastChannel = "#cncnet-ti-games",
                    InternalName = "ti",
                    RegistryInstallPath = "HKCU\\Software\\TwistedInsurrection",
                    UIName = "Twisted Insurrection",
                    Texture = AssetLoader.TextureFromImage(Resources.tiicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-ts",
                    ClientExecutableName = "TiberianSun.exe",
                    GameBroadcastChannel = "#cncnet-ts-games",
                    InternalName = "ts",
                    RegistryInstallPath = "HKLM\\Software\\Westwood\\Tiberian Sun",
                    UIName = "Tiberian Sun",
                    Texture = AssetLoader.TextureFromImage(Resources.tsicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-mo",
                    ClientExecutableName = "MentalOmegaClient.exe",
                    GameBroadcastChannel = "#cncnet-mo-games",
                    InternalName = "mo",
                    RegistryInstallPath = "HKCU\\Software\\MentalOmega",
                    UIName = "Mental Omega",
                    Texture = AssetLoader.TextureFromImage(Resources.moicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-yr",
                    ClientExecutableName = "CnCNetClientYR.exe",
                    GameBroadcastChannel = "#cncnet-yr-games",
                    InternalName = "yr",
                    RegistryInstallPath = "HKLM\\Software\\Westwood\\Yuri's Revenge",
                    UIName = "Yuri's Revenge",
                    Texture = AssetLoader.TextureFromImage(Resources.yricon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#redres-lobby",
                    ClientExecutableName = "RRLauncher.exe",
                    GameBroadcastChannel = "#redres-games",
                    InternalName = "rr",
                    RegistryInstallPath = "HKML\\Software\\RedResurrection",
                    UIName = "YR Red-Resurrection",
                    Texture = AssetLoader.TextureFromImage(Resources.rricon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncreloaded",
                    ClientExecutableName = "CnCReloadedClient.exe",
                    GameBroadcastChannel = "#cncreloaded-games",
                    InternalName = "cncr",
                    RegistryInstallPath = "HKCU\\Software\\CnCReloaded",
                    UIName = "C&C: Reloaded",
                    Texture = AssetLoader.TextureFromImage(Resources.cncricon)
                },
                
                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-rn",
                    ClientExecutableName = "RNLauncher.exe",
                    GameBroadcastChannel = "#cncnet-rn-games",
                    InternalName = "rn",
                    RegistryInstallPath = "HKCU\\Software\\RevengeNow",
                    UIName = "Revenge Now",
                    Texture = AssetLoader.TextureFromImage(Resources.rnicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-fa",
                    ClientExecutableName = "Launch Fantasy ADVENTURE.exe",
                    GameBroadcastChannel = "#cncnet-fa-games",
                    InternalName = "fa",
                    RegistryInstallPath = "HKCU\\Software\\FantasyADVENTURE",
                    UIName = "Fantasy ADVENTURE",
                    Texture = AssetLoader.TextureFromImage(Resources.faicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-es",
                    ClientExecutableName = "Extreme Starry.exe",
                    GameBroadcastChannel = "#cncnet-es-games",
                    InternalName = "es",
                    RegistryInstallPath = "HKCU\\Software\\ExtremeStarry",
                    UIName = "Extreme Starry",
                    Texture = AssetLoader.TextureFromImage(Resources.esicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#riseoftheeast",
                    ClientExecutableName = "REClient.exe",
                    GameBroadcastChannel = "#rote-games",
                    InternalName = "re",
                    RegistryInstallPath = "HKCU\\Software\\RiseoftheEast",
                    UIName = "Rise of the East",
                    Texture = AssetLoader.TextureFromImage(Resources.reicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-fr",
                    ClientExecutableName = "FRLauncher.exe",
                    GameBroadcastChannel = "#cncnet-fr-games",
                    InternalName = "fr",
                    RegistryInstallPath = "HKML\\Software\\FinaleReturn",
                    UIName = "Finale Return",
                    Texture = AssetLoader.TextureFromImage(Resources.fricon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-fcs",
                    ClientExecutableName = "FCSLauncher.exe",
                    GameBroadcastChannel = "#cncnet-fcs-games",
                    InternalName = "fcs",
                    RegistryInstallPath = "HKCU\\Software\\FantasyCombatSimulation",
                    UIName = "Fantasy Combat Simulation",
                    Texture = AssetLoader.TextureFromImage(Resources.fcsicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-mlp",
                    ClientExecutableName = "开始游戏.exe",
                    GameBroadcastChannel = "#cncnet-mlp-games",
                    InternalName = "mlp",
                    RegistryInstallPath = "HKCU\\Software\\MLP AI",
                    UIName = "MLP AI",
                    Texture = AssetLoader.TextureFromImage(Resources.mlpicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-ra1.5",
                    ClientExecutableName = "Red Alert 1.5.exe",
                    GameBroadcastChannel = "#cncnet-ra1.5-games",
                    InternalName = "ra15",
                    RegistryInstallPath = "HKCU\\Software\\Red Alert 1.5",
                    UIName = "Red Alert 1.5",
                    Texture = AssetLoader.TextureFromImage(Resources.ra15icon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-tc",
                    ClientExecutableName = "TiberiumCrisis.exe",
                    GameBroadcastChannel = "#cncnet-tc-games",
                    InternalName = "tc",
                    RegistryInstallPath = "HKCU\\Software\\TiberiumCrisis",
                    UIName = "Tiberium Crisis",
                    Texture = AssetLoader.TextureFromImage(Resources.tcicon)
                }
            };

            // CnCNet chat + unsupported games.
            CnCNetGame[] otherGames = new CnCNetGame[]
            {
                new CnCNetGame()
                {
                    ChatChannel = "#cncnet",
                    InternalName = "cncnet",
                    UIName = "General CnCNet Chat",
                    AlwaysEnabled = true,
                    Texture = AssetLoader.TextureFromImage(Resources.cncneticon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-td",
                    InternalName = "td",
                    UIName = "Tiberian Dawn",
                    Supported = false,
                    Texture = AssetLoader.TextureFromImage(Resources.tdicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-ra",
                    InternalName = "ra",
                    UIName = "Red Alert",
                    Supported = false,
                    Texture = AssetLoader.TextureFromImage(Resources.raicon)
                },

                new CnCNetGame()
                {
                    ChatChannel = "#cncnet-d2",
                    InternalName = "d2",
                    UIName = "Dune 2000",
                    Supported = false,
                    Texture = AssetLoader.TextureFromImage(Resources.unknownicon)
                }
            };

            GameList.AddRange(defaultGames);
            GameList.AddRange(GetCustomGames(defaultGames.Concat(otherGames).ToList()));
            GameList.AddRange(otherGames);

            if (GetGameIndexFromInternalName(ClientConfiguration.Instance.LocalGame) == -1)
            {
                throw new ClientConfigurationException("Could not find a game in the game collection matching LocalGame value of " +
                    ClientConfiguration.Instance.LocalGame + ".");
            }
        }

        private List<CnCNetGame> GetCustomGames(List<CnCNetGame> existingGames)
        {
            IniFile iniFile = new IniFile(ProgramConstants.GetBaseResourcePath() + "GameCollectionConfig.ini");

            List<CnCNetGame> customGames = new List<CnCNetGame>();

            var section = iniFile.GetSection("CustomGames");

            if (section == null)
                return customGames;

            HashSet<string> customGameIDs = new HashSet<string>();
            foreach (var kvp in section.Keys)
            {
                if (!iniFile.SectionExists(kvp.Value))
                    continue;

                string ID = iniFile.GetStringValue(kvp.Value, "InternalName", string.Empty).ToLower();

                if (string.IsNullOrEmpty(ID))
                    throw new GameCollectionConfigurationException("InternalName for game " + kvp.Value + " is not defined or set to an empty value.");

                if (ID.Length > ProgramConstants.GAME_ID_MAX_LENGTH)
                {
                    throw new GameCollectionConfigurationException("InternalGame for game " + kvp.Value + " is set to a value that exceeds length limit of " +
                        ProgramConstants.GAME_ID_MAX_LENGTH + " characters.");
                }

                if (existingGames.Find(g => g.InternalName == ID) != null || customGameIDs.Contains(ID))
                    throw new GameCollectionConfigurationException("Game with InternalName " + ID.ToUpper() + " already exists in the game collection.");

                string iconFilename = iniFile.GetStringValue(kvp.Value, "IconFilename", ID + "icon.png");
                customGames.Add(new CnCNetGame
                {
                    InternalName = ID,
                    UIName = iniFile.GetStringValue(kvp.Value, "UIName", ID.ToUpper()),
                    ChatChannel = GetIRCChannelNameFromIniFile(iniFile, kvp.Value, "ChatChannel"),
                    GameBroadcastChannel = GetIRCChannelNameFromIniFile(iniFile, kvp.Value, "GameBroadcastChannel"),
                    ClientExecutableName = iniFile.GetStringValue(kvp.Value, "ClientExecutableName", string.Empty),
                    RegistryInstallPath = iniFile.GetStringValue(kvp.Value, "RegistryInstallPath", "HKCU\\Software\\"
                    + ID.ToUpper()),
                    Texture = AssetLoader.AssetExists(iconFilename) ? AssetLoader.LoadTexture(iconFilename) :
                    AssetLoader.TextureFromImage(Resources.unknownicon)
                });
                customGameIDs.Add(ID);
            }

            return customGames;
        }

        private string GetIRCChannelNameFromIniFile(IniFile iniFile, string section, string key)
        {
            string channel = iniFile.GetStringValue(section, key, string.Empty);

            if (string.IsNullOrEmpty(channel))
                throw new GameCollectionConfigurationException(key + " for game " + section + " is not defined or set to an empty value.");

            if (channel.Contains(' ') || channel.Contains(',') || channel.Contains((char)7))
                throw new GameCollectionConfigurationException(key + " for game " + section + " contains characters not allowed on IRC channel names.");

            if (!channel.StartsWith("#"))
                return "#" + channel;

            return channel;
        }

        /// <summary>
        /// Gets the index of a CnCNet supported game based on its internal name.
        /// </summary>
        /// <param name="gameName">The internal name (suffix) of the game.</param>
        /// <returns>The index of the specified CnCNet game. -1 if the game is unknown or not supported.</returns>
        public int GetGameIndexFromInternalName(string gameName)
        {
            for (int gId = 0; gId < GameList.Count; gId++)
            {
                CnCNetGame game = GameList[gId];

                if (gameName.ToLowerInvariant() == game.InternalName)
                    return gId;
            }

            return -1;
        }

        /// <summary>
        /// Seeks the supported game list for a specific game's internal name and if found,
        /// returns the game's full name. Otherwise returns the internal name specified in the param.
        /// </summary>
        /// <param name="gameName">The internal name of the game to seek for.</param>
        /// <returns>The full name of a supported game based on its internal name.
        /// Returns the given parameter if the name isn't found in the supported game list.</returns>
        public string GetGameNameFromInternalName(string gameName)
        {
            CnCNetGame game = GameList.Find(g => g.InternalName == gameName.ToLowerInvariant());

            if (game == null)
                return gameName;

            return game.UIName;
        }

        /// <summary>
        /// Returns the full UI name of a game based on its index in the game list.
        /// </summary>
        /// <param name="gameIndex">The index of the CnCNet supported game.</param>
        /// <returns>The UI name of the game.</returns>
        public string GetFullGameNameFromIndex(int gameIndex)
        {
            return GameList[gameIndex].UIName;
        }

        /// <summary>
        /// Returns the internal name of a game based on its index in the game list.
        /// </summary>
        /// <param name="gameIndex">The index of the CnCNet supported game.</param>
        /// <returns>The internal name (suffix) of the game.</returns>
        public string GetGameIdentifierFromIndex(int gameIndex)
        {
            return GameList[gameIndex].InternalName;
        }

        public string GetGameBroadcastingChannelNameFromIdentifier(string gameIdentifier)
        {
            CnCNetGame game = GameList.Find(g => g.InternalName == gameIdentifier.ToLowerInvariant());
            if (game == null)
                return null;
            return game.GameBroadcastChannel;
        }

        public string GetGameChatChannelNameFromIdentifier(string gameIdentifier)
        {
            CnCNetGame game = GameList.Find(g => g.InternalName == gameIdentifier.ToLowerInvariant());
            if (game == null)
                return null;
            return game.ChatChannel;
        }
    }

    /// <summary>
    /// An exception that is thrown when configuration for a game to add to game collection
    /// contains invalid or unexpected settings / data or required settings / data are missing.
    /// </summary>
    class GameCollectionConfigurationException : Exception
    {
        public GameCollectionConfigurationException(string message) : base(message)
        {
        }
    }
}
