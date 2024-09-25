﻿using Microsoft.Xna.Framework;
using Rampastring.XNAUI.XNAControls;
using Rampastring.Tools;
using System;
using ClientCore;
using Rampastring.XNAUI;
using ClientGUI;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Localization;

namespace DTAClient.DXGUI
{
    /// <summary>
    /// Displays a dialog in the client when a game is in progress.
    /// Also enables power-saving (lowers FPS) while a game is in progress,
    /// and performs various operations on game start and exit.
    /// </summary>
    public class GameInProgressWindow : XNAPanel
    {
        private const double POWER_SAVING_FPS = 5.0;

        public GameInProgressWindow(WindowManager windowManager) : base(windowManager)
        {
        }

        private bool initialized = false;
        private bool nativeCursorUsed = false;

        private XNAMessageBox ReShadeMSGBox;

#if ARES
        private List<string> debugSnapshotDirectories;
        private DateTime debugLogLastWriteTime;
#else
        private bool deletingLogFilesFailed = false;
#endif

        public override void Initialize()
        {
            if (initialized)
                throw new InvalidOperationException("GameInProgressWindow cannot be initialized twice!");

            initialized = true;

            //BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 1, 1);
            BackgroundTexture = AssetLoader.LoadTexture("generalbglight.png");
            PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            DrawBorders = false;
            ClientRectangle = new Rectangle(0, 0, WindowManager.RenderResolutionX, WindowManager.RenderResolutionY);

            XNAWindow window = new XNAWindow(WindowManager);

            window.Name = "GameInProgressWindow";
            window.BackgroundTexture = AssetLoader.LoadTexture("gameinprogresswindowbg.png");
            window.ClientRectangle = new Rectangle(0, 0, 200, 100);

            XNALabel explanation = new XNALabel(WindowManager);
            explanation.Text = "A game is in progress.".L10N("UI:Main:GameInProgress");

            AddChild(window);

            window.AddChild(explanation);

            base.Initialize();

            GameProcessLogic.GameProcessStarted += SharedUILogic_GameProcessStarted;
            GameProcessLogic.GameProcessExited += SharedUILogic_GameProcessExited;

            explanation.CenterOnParent();

            window.CenterOnParent();

            Game.TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / UserINISettings.Instance.ClientFPS);

            Visible = false;
            Enabled = false;

#if ARES
            try
            {
                if (File.Exists(ProgramConstants.GamePath + "debug/debug.log"))
                    debugLogLastWriteTime = File.GetLastWriteTimeUtc(ProgramConstants.GamePath + "debug/debug.log");
            }
            catch { }
#endif
        }

        private void SharedUILogic_GameProcessStarted()
        {

#if ARES
            debugSnapshotDirectories = GetAllDebugSnapshotDirectories();
#else
            try
            {
                File.Delete(ProgramConstants.GamePath + "EXCEPT.TXT");

                for (int i = 0; i < 8; i++)
                    File.Delete(ProgramConstants.GamePath + "SYNC" + i + ".TXT");

                deletingLogFilesFailed = false;
            }
            catch (Exception ex)
            {
                Logger.Log("Exception when deleting error log files! Message: " + ex.Message);
                deletingLogFilesFailed = true;
            }
#endif

            Visible = true;
            Enabled = true;
            WindowManager.Cursor.Visible = false;
            nativeCursorUsed = Game.IsMouseVisible;
            Game.IsMouseVisible = false;
            ProgramConstants.IsInGame = true;
            Game.TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / POWER_SAVING_FPS);
            if (UserINISettings.Instance.MinimizeWindowsOnGameStart)
                WindowManager.MinimizeWindow();

        }

        private void SharedUILogic_GameProcessExited()
        {
            AddCallback(new Action(HandleGameProcessExited), null);

            if (UserINISettings.Instance.FakeIngameScreenWidth > 1920 || UserINISettings.Instance.FakeIngameScreenHeight > 1440)
            {
                try
                {
                    Logger.Log("Killing upscale process...");
                    foreach (System.Diagnostics.Process process in
                        System.Diagnostics.Process.GetProcessesByName(ProgramConstants.UPSCALE_PROCESS))
                        process.Kill();
                }
                catch (Exception ex)
                {
                    Logger.Log("Error killing upscale process: " + ex.Message);
                }
            }

            if (UserINISettings.Instance.bDisableWin)
            {
                try
                {
                    Logger.Log("Killing disable win process...");
                    foreach (System.Diagnostics.Process process in
                        System.Diagnostics.Process.GetProcessesByName(ProgramConstants.DISABLE_WIN_PROCESS))
                        process.Kill();
                }
                catch (Exception ex)
                {
                    Logger.Log("Error killing disable win exe: " + ex.Message);
                }
            }

            if (UserINISettings.Instance.HighDetail >= 0 && !UserINISettings.Instance.CanReShade)
            {
                string reshadeIni = ProgramConstants.GamePath + "ReShade.ini";
                if (File.Exists(reshadeIni))
                {
                    IniFile reshadeIniInstance = new IniFile(reshadeIni);
                    if (reshadeIniInstance.SectionExists("DX8_BUFFER_DETECTION") ||
                        reshadeIniInstance.SectionExists("DX9_BUFFER_DETECTION") ||
                        reshadeIniInstance.SectionExists("DX10_BUFFER_DETECTION") ||
                        reshadeIniInstance.SectionExists("DX11_BUFFER_DETECTION") ||
                        reshadeIniInstance.SectionExists("DX12_BUFFER_DETECTION") ||
                        reshadeIniInstance.SectionExists("DX13_BUFFER_DETECTION") ||
                        reshadeIniInstance.SectionExists("OPENGL") ||
                        reshadeIniInstance.SectionExists("DEPTH"))
                    {
                        ReShadeMSGBox = XNAMessageBox.ShowYesNoDialog(WindowManager,
                            "Enhanced Quality Successful".L10N("UI:Main:ReShadeSucceed"),
                            string.Format("If got lags, make sure you changed" + Environment.NewLine +
                            "preferred graphics processor to GPU." + Environment.NewLine +
                            "Do you want to change it now?").L10N("UI:Main:ReShadeSucceed_Desc"));
                        UserINISettings.Instance.CanReShade.Value = true;
                        UserINISettings.Instance.SaveSettings();
                        ReShadeMSGBox.YesClickedAction = ReShadeMSGBox_YesClicked_Success;
                    }
                    else
                    {
                        ReShadeMSGBox = XNAMessageBox.ShowYesNoDialog(WindowManager,
                            "Failed to enable Enhanced Quality".L10N("UI:Main:ReShadeFailed"),
                            string.Format("Please try to switch your renderer to" + Environment.NewLine +
                            "OpenGL or DirectX" + Environment.NewLine +
                            "Make sure you got the latest Windows version" + Environment.NewLine +
                            "and installed DirectX End-User Runtimes" + Environment.NewLine +
                            "Do you want to download it now?").L10N("UI:Main:ReShadeFailed_Desc"));
                        ReShadeMSGBox.YesClickedAction = ReShadeMSGBox_YesClicked_Fail;
                    }
                }
                else
                {
                    XNAMessageBox.Show(WindowManager, "Failed to enable Enhanced Quality".L10N("UI:Main:ReShadeFailed"),
                        string.Format("Please try to close your anti-virus softwares," + Environment.NewLine +
                        "and make sure GScript.ext, d3d9.ext, dx3d9_29.ext, Crisis.ext are your game folder.").L10N("UI:Main:ReShadeFailed_Desc2"));
                }
            }
        }

        private void ReShadeMSGBox_YesClicked_Success(XNAMessageBox messageBox)
        {
            try
            {
               // if (ClientConfiguration.Instance.ClientLanguage == 0)
               //    System.Diagnostics.Process.Start(ProgramConstants.GetBaseSharedPath() + "ENHANCED_QUALITY_HELP_ENG.doc");
               // else
                    System.Diagnostics.Process.Start(ProgramConstants.GetBaseSharedPath() + "ENHANCED_QUALITY_HELP_CHS.doc");
            }
            catch (Exception)
            {
                XNAMessageBox.Show(WindowManager, "Cannot open readme doc", "Please manually open this document:"
                    + Environment.NewLine + ProgramConstants.GetBaseSharedPath() + "ENHANCED_QUALITY_HELP_CHS.doc");
                try
                {
                   // if (ClientConfiguration.Instance.ClientLanguage == 0)
                   //     System.Diagnostics.Process.Start("http://docs.google.com/document/d/1z3VoC13PfeiWI_s57uk9OdaG1-zxlFI06njBztS-NoA/edit?usp=sharing");
                   //else
                         System.Diagnostics.Process.Start("http://shimo.im/docs/cHPR9d6RYpcpV9r3");
                }
                catch (Exception)
                {
                    try
                    {
                        //if (ClientConfiguration.Instance.ClientLanguage == 0)
                        //    System.Diagnostics.Process.Start("iexplore.exe", "http://docs.google.com/document/d/1z3VoC13PfeiWI_s57uk9OdaG1-zxlFI06njBztS-NoA/edit?usp=sharing");
                        //else
                            System.Diagnostics.Process.Start("iexplore.exe", "http://shimo.im/docs/cHPR9d6RYpcpV9r3");

                    }
                    catch (Exception)
                    {
                        Logger.Log("Error opening a website.");
                    }
                }
            }
        }

        private void ReShadeMSGBox_YesClicked_Fail(XNAMessageBox messageBox)
        {
            try
            {
                //if (ClientConfiguration.Instance.ClientLanguage == 0)
                //    System.Diagnostics.Process.Start("http://www.microsoft.com/en-us/download/details.aspx?id=8109");
                //else
                    System.Diagnostics.Process.Start("https://www.microsoft.com/zh-cn/download/details.aspx?id=8109");
            }
            catch (Exception)
            {
                try
                {
                    //if (ClientConfiguration.Instance.ClientLanguage == 0)
                    //    System.Diagnostics.Process.Start("iexplore.exe", "http://www.microsoft.com/en-us/download/details.aspx?id=7087");
                    //else
                        System.Diagnostics.Process.Start("iexplore.exe", "http://www.microsoft.com/zh-cn/download/details.aspx?id=7087");
                }
                catch (Exception ex)
                {
                    //if (ClientConfiguration.Instance.ClientLanguage == 0)
                    //    XNAMessageBox.Show(WindowManager, "Cannot open website", "Need to manually put this link into your browser:"
                    //        + Environment.NewLine + "http://www.microsoft.com/en-us/download/details.aspx?id=7087");
                    //else
                        XNAMessageBox.Show(WindowManager, "无法打开网址", "需要手动复制到浏览器地址。"
                            + Environment.NewLine + "http://www.microsoft.com/zh-cn/download/details.aspx?id=7087");

                    Logger.Log("Error opening microsoft website, message: " + ex.Message);
                }
            }
        }

        private void HandleGameProcessExited()
        {
            Visible = false;
            Enabled = false;
            if (nativeCursorUsed)
                Game.IsMouseVisible = true;
            else
                WindowManager.Cursor.Visible = true;
            ProgramConstants.IsInGame = false;
            Game.TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / UserINISettings.Instance.ClientFPS);
            if (UserINISettings.Instance.MinimizeWindowsOnGameStart)
                WindowManager.MaximizeWindow();

            UserINISettings.Instance.ReloadSettings();

            if (UserINISettings.Instance.BorderlessWindowedClient)
            {
                // Hack: Re-set graphics mode
                // Windows resizes our window if we're in fullscreen mode and
                // the in-game resolution is lower than the user's desktop resolution.
                // After the game exits, Windows doesn't properly re-size our window
                // back to cover the entire screen, which causes graphics to get
                // stretched and also messes up input handling since the window manager
                // still thinks it's using the original resolution.
                // Re-setting the graphics mode fixes it.
                GameClass.SetGraphicsMode(WindowManager);
            }

            DateTime dtn = DateTime.Now;

#if ARES
            Task.Factory.StartNew(ProcessScreenshots);

            // TODO: Ares debug log handling should be addressed in Ares DLL itself.
            // For now the following are handled here:
            // 1. Make a copy of syringe.log in debug snapshot directory on both crash and desync.
            // 2. Move SYNCX.txt from game directory to debug snapshot directory on desync.
            // 3. Make a debug snapshot directory & copy debug.log to it on desync even if full crash dump wasn't created.
            // 4. Handle the empty snapshot directories created on a crash if debug logging was disabled.

            string snapshotDirectory = GetNewestDebugSnapshotDirectory();
            bool snapshotCreated = snapshotDirectory != null;

            snapshotDirectory = snapshotDirectory ?? ProgramConstants.GamePath + "debug/snapshot-" +
                dtn.ToString("yyyyMMdd-HHmmss");

            bool debugLogModified = false;
            string debugLogPath = ProgramConstants.GamePath + "debug/debug.log";
            DateTime lastWriteTime = new DateTime();

            if (File.Exists(debugLogPath))
                lastWriteTime = File.GetLastWriteTimeUtc(debugLogPath);

            if (!lastWriteTime.Equals(debugLogLastWriteTime))
            {
                debugLogModified = true;
                debugLogLastWriteTime = lastWriteTime;
            }

            if (CopySyncErrorLogs(snapshotDirectory, null) || snapshotCreated)
            {
                if (File.Exists(debugLogPath) && !File.Exists(snapshotDirectory + "/debug.log") && debugLogModified)
                    File.Copy(debugLogPath, snapshotDirectory + "/debug.log");

                CopyErrorLog(snapshotDirectory, "syringe.log", null);
            }
#else
            if (deletingLogFilesFailed)
                return;

            CopyErrorLog(ProgramConstants.ClientUserFilesPath + "GameCrashLogs", "EXCEPT.TXT", dtn);
            CopySyncErrorLogs(ProgramConstants.ClientUserFilesPath + "SyncErrorLogs", dtn);
#endif
        }

        /// <summary>
        /// Attempts to copy a general error log from game directory to another directory.
        /// </summary>
        /// <param name="directory">Directory to copy error log to.</param>
        /// <param name="filename">Filename of the error log.</param>
        /// <param name="dateTime">Time to to apply as a timestamp to filename. Set to null to not apply a timestamp.</param>
        /// <returns>True if error log was copied, false otherwise.</returns>
        private bool CopyErrorLog(string directory, string filename, DateTime? dateTime)
        {
            bool copied = false;

            try
            {
                if (File.Exists(ProgramConstants.GamePath + filename))
                {
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    Logger.Log("The game crashed! Copying " + filename + " file.");

                    string timeStamp = dateTime.HasValue ? dateTime.Value.ToString("_yyyy_MM_dd_HH_mm") : "";

                    string filenameCopy = Path.GetFileNameWithoutExtension(filename) +
                        timeStamp + Path.GetExtension(filename);

                    File.Copy(ProgramConstants.GamePath + filename, directory + "/" + filenameCopy);
                    copied = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occured while checking for " + filename + " file. Message: " + ex.Message);
            }
            return copied;
        }

        /// <summary>
        /// Attempts to copy sync error logs from game directory to another directory.
        /// </summary>
        /// <param name="directory">Directory to copy sync error logs to.</param>
        /// <param name="dateTime">Time to to apply as a timestamp to filename. Set to null to not apply a timestamp.</param>
        /// <returns>True if any sync logs were copied, false otherwise.</returns>
        private bool CopySyncErrorLogs(string directory, DateTime? dateTime)
        {
            bool copied = false;

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    string filename = "SYNC" + i + ".TXT";

                    if (File.Exists(ProgramConstants.GamePath + filename))
                    {
                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);

                        Logger.Log("There was a sync error! Copying file " + filename);

                        string timeStamp = dateTime.HasValue ? dateTime.Value.ToString("_yyyy_MM_dd_HH_mm") : "";

                        string filenameCopy = Path.GetFileNameWithoutExtension(filename) +
                            timeStamp + Path.GetExtension(filename);

                        File.Copy(ProgramConstants.GamePath + filename, directory + "/" + filenameCopy);
                        copied = true;
                        File.Delete(ProgramConstants.GamePath + filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("An error occured while checking for SYNCX.TXT files. Message: " + ex.Message);
            }
            return copied;
        }

#if ARES
        /// <summary>
        /// Returns the first debug snapshot directory found in Ares debug log directory that was created after last game launch and isn't empty.
        /// Additionally any empty snapshot directories encountered are deleted.
        /// </summary>
        /// <returns>Full path of the debug snapshot directory. If one isn't found, null is returned.</returns>
        private string GetNewestDebugSnapshotDirectory()
        {
            string snapshotDirectory = null;

            if (debugSnapshotDirectories != null)
            {
                var newDirectories = GetAllDebugSnapshotDirectories().Except(debugSnapshotDirectories);

                foreach (string directory in newDirectories)
                {
                    if (Directory.EnumerateFileSystemEntries(directory).Any())
                        snapshotDirectory = directory;
                    else
                    {
                        try
                        {
                            Directory.Delete(directory);
                        }
                        catch { }
                    }
                }
            }

            return snapshotDirectory;
        }

        /// <summary>
        /// Returns list of all debug snapshot directories in Ares debug logs directory.
        /// </summary>
        /// <returns>List of all debug snapshot directories in Ares debug logs directory. Empty list if none are found or an error was encountered.</returns>
        private List<string> GetAllDebugSnapshotDirectories()
        {
            List<string> directories = new List<string>();

            try
            {
                directories.AddRange(Directory.GetDirectories(ProgramConstants.GamePath + "debug", "snapshot-*"));
            }
            catch { }

            return directories;
        }

        /// <summary>
        /// Converts BMP screenshots to PNG and copies them from game directory to Screenshots sub-directory.
        /// </summary>
        private void ProcessScreenshots()
        {
            string[] filenames = Directory.GetFiles(ProgramConstants.GamePath, "SCRN*.bmp");
            string screenshotsDirectory = ProgramConstants.GamePath + "Screenshots";

            if (!Directory.Exists(screenshotsDirectory))
            {
                try
                {
                    Directory.CreateDirectory(screenshotsDirectory);
                }
                catch (Exception ex)
                {
                    Logger.Log("ProcessScreenshots: An error occured trying to create Screenshots directory. Message: " + ex.Message);
                    return;
                }
            }

            foreach (string filename in filenames)
            {
                try
                {
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filename);
                    bitmap.Save(screenshotsDirectory + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename) +
                        ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bitmap.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Log("ProcessScreenshots: Error occured when trying to save " + Path.GetFileNameWithoutExtension(filename) + ".png. Message: " + ex.Message);
                    continue;
                }

                Logger.Log("ProcessScreenshots: " + Path.GetFileNameWithoutExtension(filename) + ".png has been saved to Screenshots directory.");
                File.Delete(filename);
            }
        }
#endif
    }
}
