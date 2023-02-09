using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using ClientCore;
using Rampastring.Tools;
using ClientCore.INIProcessing;
using System.Threading;
using System.Globalization;
using System.Linq;

namespace ClientGUI
{
    /// <summary>
    /// A static class used for controlling the launching and exiting of the game executable.
    /// </summary>
    public static class GameProcessLogic
    {
        public static event Action GameProcessStarted;

        public static event Action GameProcessStarting;

        public static event Action GameProcessExited;

        public static bool UseQres { get; set; }
        public static bool SingleCoreAffinity { get; set; }

        /// <summary>
        /// Starts the main game process.
        /// </summary>
        public static void StartGameProcess(bool bCanControlSpeed = true, bool bIsCampaign = false)
        {
            // reshade stuff
            if (!UserINISettings.Instance.DebugReShade)
            {
                IniFile ReShadeIni = new IniFile(ProgramConstants.GamePath + "ReShade.ini");
                ReShadeIni.SetStringValue("GENERAL", "PresetPath", "./GameShaders/TCMainShader.ini");
                ReShadeIni.SetStringValue("GENERAL", "CurrentPresetPath", "./GameShaders/TCMainShader.ini");
                ReShadeIni.SetStringValue("GENERAL", "EffectSearchPaths", "./GameShaders/ShadersTC");
                ReShadeIni.SetStringValue("GENERAL", "TextureSearchPaths", "./GameShaders/ShadersTC");
                ReShadeIni.SetStringValue("GENERAL", "SkipLoadingDisabledEffects", "1");
                ReShadeIni.SetStringValue("GENERAL", "PerformanceMode", "1");
                ReShadeIni.SetStringValue("GENERAL", "NoReloadOnInit", "0");
                ReShadeIni.SetStringValue("GENERAL", "TutorialProgress", "4");
                ReShadeIni.SetStringValue("OVERLAY", "TutorialProgress", "4");
                ReShadeIni.SetStringValue("INPUT", "KeyEffects", "0,0,0,0");
                ReShadeIni.SetStringValue("INPUT", "KeyMenu", "0,0,0,0");
                ReShadeIni.SetStringValue("INPUT", "KeyOverlay", "0,0,0,0");
                ReShadeIni.WriteIniFile(ProgramConstants.GamePath + "ReShade.ini");
            }

            // choose main game executable file (1 forcespeed, 3 forcespeed + disable music selection)
            string strMainExecutableName;
            if (bCanControlSpeed)
            {
                strMainExecutableName = ProgramConstants.MAIN_EXE_2;
            }
            else if (bIsCampaign)
            {
                strMainExecutableName = ProgramConstants.MAIN_EXE_3;
            }
            else
            {
                strMainExecutableName = ProgramConstants.MAIN_EXE;
            }
            if (File.Exists(ProgramConstants.GetBaseResourcePath() + strMainExecutableName))
            {
                File.Copy(ProgramConstants.GetBaseResourcePath() + strMainExecutableName,
                    ProgramConstants.GamePath + "gamemd.exe", true);
            }

            // disable win
            if (UserINISettings.Instance.bDisableWin)
            {
                try
                {
                    Logger.Log("About to launch disable win executable.");
                    Process.Start(ProgramConstants.GetBaseResourcePath() + ProgramConstants.DISABLE_WIN);
                }
                catch (Exception ex)
                {
                    Logger.Log("Error launching tc disable win: " + ex.Message);
                }
            }

            Logger.Log("About to launch main game executable.");

            // In the relatively unlikely event that INI preprocessing is still going on, just wait until it's done.
            // TODO ideally this should be handled in the UI so the client doesn't appear just frozen for the user.
            int waitTimes = 0;
            while (PreprocessorBackgroundTask.Instance.IsRunning)
            {
                Thread.Sleep(1000);
                waitTimes++;
                if (waitTimes > 10)
                {
                    MessageBox.Show("INI preprocessing not complete. Please try " +
                        "launching the game again. If the problem persists, " +
                        "contact the game or mod authors for support.");
                    return;
                }
            }

            OSVersion osVersion = ClientConfiguration.Instance.GetOperatingSystemVersion();

            string gameExecutableName;
            string additionalExecutableName = string.Empty;

            if (osVersion == OSVersion.UNIX)
                gameExecutableName = ClientConfiguration.Instance.UnixGameExecutableName;
            else
            {
                string launcherExecutableName = ClientConfiguration.Instance.GameLauncherExecutableName;
                if (string.IsNullOrEmpty(launcherExecutableName))
                    gameExecutableName = ClientConfiguration.Instance.GetGameExecutableName();
                else
                {
                    gameExecutableName = launcherExecutableName;
                    additionalExecutableName = "\"" + ClientConfiguration.Instance.GetGameExecutableName() + "\" ";
                }
            }

            const int MAX_THREADS = 24;

            int cpuCount = Math.Min(Environment.ProcessorCount, MAX_THREADS);
            int affinity = (1 << cpuCount) - 1;

            string extraCommandLine = ClientConfiguration.Instance.ExtraExeCommandLineParameters;

            // Remove -AFFINITY params
            var options = extraCommandLine.Split(' ').ToList();
            do
            {
                int? toBeDeleted = null;
                for (int i = 0; i < options.Count; ++i)
                {
                    if (options[i].ToUpper(CultureInfo.InvariantCulture).StartsWith("-AFFINITY:", StringComparison.Ordinal))
                    {
                        toBeDeleted = i;
                        break;
                    }
                }
                if (toBeDeleted != null)
                {
                    options.RemoveAt(toBeDeleted.Value);
                }
                else
                {
                    break;
                }
            } while (true);

            extraCommandLine += " -LegalUse -AFFINITY:" + affinity.ToString(CultureInfo.InvariantCulture) + " -NOLOGO ";

            File.Delete(ProgramConstants.GamePath + "DTA.LOG");
            File.Delete(ProgramConstants.GamePath + "TI.LOG");
            File.Delete(ProgramConstants.GamePath + "TS.LOG");

            GameProcessStarting?.Invoke();

            if (UserINISettings.Instance.WindowedMode && UseQres)
            {
                Logger.Log("Windowed mode is enabled - using QRes.");
                Process QResProcess = new Process();
                QResProcess.StartInfo.FileName = ProgramConstants.QRES_EXECUTABLE;
                QResProcess.StartInfo.UseShellExecute = false;
                if (!string.IsNullOrEmpty(extraCommandLine))
                    QResProcess.StartInfo.Arguments = "c=16 /R " + "\"" + ProgramConstants.GamePath + gameExecutableName + "\" " + additionalExecutableName + "-SPAWN " + extraCommandLine;
                else
                    QResProcess.StartInfo.Arguments = "c=16 /R " + "\"" + ProgramConstants.GamePath + gameExecutableName + "\" " + additionalExecutableName + "-SPAWN";
                QResProcess.EnableRaisingEvents = true;
                QResProcess.Exited += new EventHandler(Process_Exited);
                Logger.Log("Launch executable: " + QResProcess.StartInfo.FileName);
                Logger.Log("Launch arguments: " + QResProcess.StartInfo.Arguments);
                try
                {
                    QResProcess.Start();
                }
                catch (Exception ex)
                {
                    Logger.Log("Error launching QRes: " + ex.Message);
                    MessageBox.Show("Error launching " + ProgramConstants.QRES_EXECUTABLE + ". Please check that your anti-virus isn't blocking the CnCNet Client. " +
                        "You can also try running the client as an administrator." + Environment.NewLine + Environment.NewLine + "You are unable to participate in this match." +
                        Environment.NewLine + Environment.NewLine + "Returned error: " + ex.Message,
                        "Error launching game", MessageBoxButtons.OK);
                    Process_Exited(QResProcess, EventArgs.Empty);
                    return;
                }

                if (Environment.ProcessorCount > 1 && SingleCoreAffinity)
                    QResProcess.ProcessorAffinity = (IntPtr)2;
            }
            else
            {
                Process DtaProcess = new Process();
                DtaProcess.StartInfo.FileName = gameExecutableName;
                DtaProcess.StartInfo.UseShellExecute = false;
                if (!string.IsNullOrEmpty(extraCommandLine))
                    DtaProcess.StartInfo.Arguments = " " + additionalExecutableName + "-SPAWN " + extraCommandLine;
                else
                    DtaProcess.StartInfo.Arguments = additionalExecutableName + "-SPAWN";
                DtaProcess.EnableRaisingEvents = true;
                DtaProcess.Exited += new EventHandler(Process_Exited);
                Logger.Log("Launch executable: " + DtaProcess.StartInfo.FileName);
                //Logger.Log("Launch arguments: " + DtaProcess.StartInfo.Arguments);
                try
                {
                    DtaProcess.Start();
                    Logger.Log("GameProcessLogic: Process started.");
                }
                catch (Exception ex)
                {
                    Logger.Log("Error launching " + gameExecutableName + ": " + ex.Message);
                    MessageBox.Show("Error launching " + gameExecutableName + ". Please check that your anti-virus isn't blocking the CnCNet Client. " +
                        "You can also try running the client as an administrator." + Environment.NewLine + Environment.NewLine + "You are unable to participate in this match." +
                        Environment.NewLine + Environment.NewLine + "Returned error: " + ex.Message,
                        "Error launching game", MessageBoxButtons.OK);
                    Process_Exited(DtaProcess, EventArgs.Empty);
                    return;
                }

                if (Environment.ProcessorCount > 1 && SingleCoreAffinity)
                    DtaProcess.ProcessorAffinity = (IntPtr)2;
            }

            GameProcessStarted?.Invoke();

            Logger.Log("Waiting for qres.dat or " + gameExecutableName + " to exit.");
        }

        static void Process_Exited(object sender, EventArgs e)
        {
            Logger.Log("GameProcessLogic: Process exited.");
            Process proc = (Process)sender;
            proc.Exited -= Process_Exited;
            proc.Dispose();
            GameProcessExited?.Invoke();
        }
    }
}
