﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using System.Runtime.InteropServices;


namespace TeconMoon_s_WiiVC_Injector
{
    public partial class WiiVC_Injector : Form
    {
        public WiiVC_Injector()
        {
            InitializeComponent();
            //Check for if .Net v3.5 component is installed
            CheckForNet35();
            //Delete Temporary Root Folder if it exists
            if (Directory.Exists(TempRootPath))
            {
                Directory.Delete(TempRootPath, true);
            }
            Directory.CreateDirectory(TempRootPath);
            //Extract Tools to temp folder
            File.WriteAllBytes(TempRootPath + "TOOLDIR.zip", Properties.Resources.TOOLDIR);
            ZipFile.ExtractToDirectory(TempRootPath + "TOOLDIR.zip", TempRootPath);
            File.Delete(TempRootPath + "TOOLDIR.zip");
            //Create Source and Build directories
            Directory.CreateDirectory(TempSourcePath);
            Directory.CreateDirectory(TempBuildPath);
        }

        //Testing
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);
        public string ShortenPath(string pathtomakesafe)
        {
            StringBuilder sb = new StringBuilder(1000);
            int n = GetShortPathName(pathtomakesafe, sb, 1000);
            if (n == 0) // check for errors
            {
                return Marshal.GetLastWin32Error().ToString();
            }
            else
            {
                return sb.ToString();
            }
                
        }


        //Specify public variables for later use (ASK ALAN)
        string SystemType = "wii";
        string TitleIDHex;
        string TitleIDText;
        string InternalGameName;
        bool FlagWBFS;
        bool FlagGameSpecified;
        bool FlagGC2Specified;
        bool FlagIconSpecified;
        bool FlagBannerSpecified;
        bool FlagDrcSpecified;
        bool FlagLogoSpecified;
        bool FlagBootSoundSpecified;
        bool BuildFlagSource;
        bool BuildFlagMeta;
        bool BuildFlagAdvance = true;
        bool BuildFlagKeys;
        bool CommonKeyGood;
        bool TitleKeyGood;
        bool AncastKeyGood;
        bool FlagRepo;
        bool HideProcess = true;
        int TitleIDInt;
        long GameType;
        string CucholixRepoID = "";
        string sSourceData;
        byte[] tmpSource;
        byte[] tmpHash;
        string AncastKeyHash;
        string WiiUCommonKeyHash;
        string TitleKeyHash;
        string DRCUSE = "1";
        string pngtemppath;
        string LoopString = " -noLoop";
        string nfspatchflag = "";
        string passpatch = " -passthrough";
        ProcessStartInfo Launcher;
        string LauncherExeFile;
        string LauncherExeArgs;
        string JNUSToolDownloads = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\JNUSToolDownloads\\";
        string TempRootPath = Path.GetTempPath() + "WiiVCInjector\\";
        string TempSourcePath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\";
        string TempBuildPath = Path.GetTempPath() + "WiiVCInjector\\BUILDDIR\\";
        string TempToolsPath = Path.GetTempPath() + "WiiVCInjector\\TOOLDIR\\";
        string TempIconPath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\iconTex.png";
        string TempBannerPath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootTvTex.png";
        string TempDrcPath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootDrcTex.png";
        string TempLogoPath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootLogoTex.png";
        string TempSoundPath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootSound.wav";
        string OGfilepath;

        //call options
        public void LaunchProgram()
        {
            Launcher = new ProcessStartInfo(LauncherExeFile);
            Launcher.Arguments = LauncherExeArgs;
            if (HideProcess)
            {
                Launcher.WindowStyle = ProcessWindowStyle.Hidden;
            }
            Process.Start(Launcher).WaitForExit();
        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        public static string GetFullPath(string fileName)
        {
            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(';'))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
        public void DownloadFromRepo()
        {
            var client = new WebClient();
            IconPreviewBox.Load("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID + "/iconTex.png");
            if (File.Exists(Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\iconTex.png")) { File.Delete(Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\iconTex.png"); }
            client.DownloadFile(IconPreviewBox.ImageLocation, Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\iconTex.png");
            IconSourceDirectory.Text = "iconTex.png downloaded from Cucholix's Repo";
            IconSourceDirectory.ForeColor = Color.Black;
            FlagIconSpecified = true;
            BannerPreviewBox.Load("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID + "/bootTvTex.png");
            if (File.Exists(Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootTvTex.png")) { File.Delete(Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootTvTex.png"); }
            client.DownloadFile(BannerPreviewBox.ImageLocation, Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootTvTex.png");
            BannerSourceDirectory.Text = "bootTvTex.png downloaded from Cucholix's Repo";
            BannerSourceDirectory.ForeColor = Color.Black;
            FlagBannerSpecified = true;
            FlagRepo = true;
        }
        //Called from RepoDownload_Click to check if files exist before downloading
        private bool RemoteFileExists(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }
        private void CheckForNet35()
        {
            if (Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v3.5") == null)
            {
                MessageBox.Show(".NET Framework 3.5 was not detected on your machine, which is required by programs used during the build process.\n\nYou should be able to enable this in \"Programs and Features\" under \"Turn Windows features on or off\", or download it from Microsoft.\n\nClick OK to close the injector and open \"Programs and Features\"...", ".NET Framework v3.5 not found...");
                HideProcess = false;
                LauncherExeFile = "appwiz.cpl";
                LauncherExeArgs = "";
                LaunchProgram();
                Environment.Exit(0);
            }
        }

        // Check if the input byte array is GB2312 encoded.
        private bool IsGB2312EncodingArray(byte[] b)
        {
            int i = 0;
            while (i < b.Length)
            {
                if (b[i] <= 127)
                {
                    ++i;
                    continue;
                }

                if (b[i] >= 176 && b[i] <= 247)
                {
                    if (i == b.Length - 1)
                    {
                        return false;
                    }
                    ++i;

                    if (b[i] < 160 || b[i] > 254)
                    {
                        return false;
                    }

                    ++i;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        // Get a probably encoding object for input array.
        private Encoding GetArrayEncoding(byte[] b)
        {
            if (IsGB2312EncodingArray(b))
            {
                return Encoding.GetEncoding("GB2312");
            }

            // We assume it is utf8 by default.
            return Encoding.UTF8;
        }

        // Read a string from a binary stream.
        private string ReadStringFromBinaryStream(BinaryReader reader, long position, bool peek = false)
        {
            long oldPosition = 0;

            if (peek)
            {
                oldPosition = reader.BaseStream.Position;
            }

            reader.BaseStream.Position = position;
            ArrayList readBuffer = new ArrayList();
            byte b;
            while ((b = reader.ReadByte()) != 0)
            {
                readBuffer.Add(b);
            }

            if (peek)
            {
                reader.BaseStream.Position = oldPosition;
            }

            byte[] readBytes = readBuffer.OfType<byte>().ToArray();
            return Encoding.Default.GetString(Encoding.Convert(
                GetArrayEncoding(readBytes),
                Encoding.Default,
                readBytes));
        }

        //Cleanup when program is closed
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) { Directory.Delete(TempRootPath, true); return; }

            // Confirm user wants to close
            switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    Directory.Delete(TempRootPath, true);
                    break;
            }
        }

        //Radio Buttons for desired injection type (Check with Alan on having one command to clear variables instead of specifying them all 4 times)
        private void WiiRetail_CheckedChanged(object sender, EventArgs e)
        {
            if (WiiRetail.Checked)
            {
                WiiVMC.Enabled = true;
                RepoDownload.Enabled = true;
                GameSourceButton.Enabled = true;
                GameSourceButton.Text = "Game...";
                OpenGame.FileName = "game";
                OpenGame.Filter = "Wii Dumps (*.iso,*.wbfs)|*.iso;*.wbfs";
                GameSourceDirectory.Text = "Game file has not been specified";
                GameSourceDirectory.ForeColor = Color.Red;
                FlagGameSpecified = false;
                SystemType = "wii";
                GameNameLabel.Text = "";
                TitleIDLabel.Text = "";
                TitleIDInt = 0;
                TitleIDHex = "";
                GameType = 0;
                CucholixRepoID = "";
                PackedTitleLine1.Text = "";
                PackedTitleIDLine.Text = "";
                GC2SourceButton.Enabled = false;
                GC2SourceDirectory.Text = "2nd GameCube Disc Image has not been specified";
                GC2SourceDirectory.ForeColor = Color.Red;
                FlagGC2Specified = false;
                if (NoGamePadEmu.Checked == false & CCEmu.Checked == false & HorWiiMote.Checked == false & VerWiiMote.Checked == false & ForceCC.Checked == false & ForceNoCC.Checked == false)
                {
                    NoGamePadEmu.Checked = true;
                    GamePadEmuLayout.Enabled = true;
                    DRCUSE = "1";
                }
                Force43NINTENDONT.Checked = false;
                Force43NINTENDONT.Enabled = false;
                CustomMainDol.Checked = false;
                CustomMainDol.Enabled = false;
                DisableNintendontAutoboot.Checked = false;
                DisableNintendontAutoboot.Enabled = false;
                DisablePassthrough.Checked = false;
                DisablePassthrough.Enabled = false;
                DisableGamePad.Checked = false;
                DisableGamePad.Enabled = false;
                C2WPatchFlag.Checked = false;
                C2WPatchFlag.Enabled = false;
                if (ForceCC.Checked) { DisableTrimming.Checked = false; DisableTrimming.Enabled = false; } else { DisableTrimming.Enabled = true; }
                Force43NAND.Checked = false;
                Force43NAND.Enabled = false;
            }
        }
        private void WiiHomebrew_CheckedChanged(object sender, EventArgs e)
        {
            if (WiiHomebrew.Checked)
            {
                WiiVMC.Checked = false;
                WiiVMC.Enabled = false;
                RepoDownload.Enabled = false;
                GameSourceButton.Enabled = true;
                GameSourceButton.Text = "Game...";
                OpenGame.FileName = "boot.dol";
                OpenGame.Filter = "DOL Files (*.dol)|*.dol";
                GameSourceDirectory.Text = "Game file has not been specified";
                GameSourceDirectory.ForeColor = Color.Red;
                FlagGameSpecified = false;
                SystemType = "dol";
                GameNameLabel.Text = "";
                TitleIDLabel.Text = "";
                TitleIDInt = 0;
                TitleIDHex = "";
                GameType = 0;
                CucholixRepoID = "";
                PackedTitleLine1.Text = "";
                PackedTitleIDLine.Text = "";
                DRCUSE = "65537";
                GC2SourceButton.Enabled = false;
                GC2SourceDirectory.Text = "2nd GameCube Disc Image has not been specified";
                GC2SourceDirectory.ForeColor = Color.Red;
                FlagGC2Specified = false;
                NoGamePadEmu.Checked = false;
                CCEmu.Checked = false;
                HorWiiMote.Checked = false;
                VerWiiMote.Checked = false;
                ForceCC.Checked = false;
                ForceNoCC.Checked = false;
                GamePadEmuLayout.Enabled = false;
                LRPatch.Checked = false;
                LRPatch.Enabled = false;
                Force43NINTENDONT.Checked = false;
                Force43NINTENDONT.Enabled = false;
                CustomMainDol.Checked = false;
                CustomMainDol.Enabled = false;
                DisableNintendontAutoboot.Checked = false;
                DisableNintendontAutoboot.Enabled = false;
                DisablePassthrough.Enabled = true;
                DisableGamePad.Enabled = true;
                C2WPatchFlag.Enabled = true;
                DisableTrimming.Checked = false;
                DisableTrimming.Enabled = false;
                Force43NAND.Checked = false;
                Force43NAND.Enabled = false;
            }
        }
        private void WiiNAND_CheckedChanged(object sender, EventArgs e)
        {
            if (WiiNAND.Checked)
            {
                WiiNANDLoopback:
                WiiVMC.Checked = false;
                WiiVMC.Enabled = false;
                RepoDownload.Enabled = true;
                GameSourceButton.Enabled = false;
                GameSourceButton.Text = "TitleID...";
                OpenGame.FileName = "NULL";
                GameNameLabel.Text = "";
                TitleIDLabel.Text = "";
                TitleIDInt = 0;
                TitleIDHex = "";
                GameType = 0;
                CucholixRepoID = "";
                PackedTitleLine1.Text = "";
                PackedTitleIDLine.Text = "";
                GC2SourceButton.Enabled = false;
                GC2SourceDirectory.Text = "2nd GameCube Disc Image has not been specified";
                GC2SourceDirectory.ForeColor = Color.Red;
                FlagGC2Specified = false;
                Force43NINTENDONT.Checked = false;
                Force43NINTENDONT.Enabled = false;
                CustomMainDol.Checked = false;
                CustomMainDol.Enabled = false;
                DisableNintendontAutoboot.Checked = false;
                DisableNintendontAutoboot.Enabled = false;
                DisablePassthrough.Checked = false;
                DisablePassthrough.Enabled = false;
                DisableGamePad.Checked = false;
                DisableGamePad.Enabled = false;
                C2WPatchFlag.Checked = false;
                C2WPatchFlag.Enabled = false;
                DisableTrimming.Checked = false;
                DisableTrimming.Enabled = false;
                Force43NAND.Enabled = true;
                if (NoGamePadEmu.Checked == false & CCEmu.Checked == false & HorWiiMote.Checked == false & VerWiiMote.Checked == false & ForceCC.Checked == false & ForceNoCC.Checked == false)
                {
                    NoGamePadEmu.Checked = true;
                    GamePadEmuLayout.Enabled = true;
                    DRCUSE = "1";
                }
                GameSourceDirectory.Text = Microsoft.VisualBasic.Interaction.InputBox("Enter your installed Wii Channel's 4-letter Title ID. If you don't know it, open a WAD for the channel in something like ShowMiiWads to view it.", "Enter your WAD's Title ID", "XXXX", 0, 0);
                if (GameSourceDirectory.Text == "")
                {
                    GameSourceDirectory.ForeColor = Color.Red;
                    GameSourceDirectory.Text = "Title ID specification cancelled, reselect vWii NAND Title Launcher to specify";
                    FlagGameSpecified = false;
                    goto skipWiiNandLoopback;
                }
                if (GameSourceDirectory.Text.Length == 4)
                {
                    GameSourceDirectory.Text = GameSourceDirectory.Text.ToUpper();
                    GameSourceDirectory.ForeColor = Color.Black;
                    FlagGameSpecified = true;
                    SystemType = "wiiware";
                    GameNameLabel.Text = "N/A";
                    TitleIDLabel.Text = "N/A";
                    TitleIDText = GameSourceDirectory.Text;
                    CucholixRepoID = GameSourceDirectory.Text;
                    char[] HexIDBuild = GameSourceDirectory.Text.ToCharArray();
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (char c in HexIDBuild)
                    {
                        stringBuilder.Append(((Int16)c).ToString("X"));
                    }
                    PackedTitleIDLine.Text = "00050002" + stringBuilder.ToString();
                }
                else
                {
                    GameSourceDirectory.ForeColor = Color.Red;
                    GameSourceDirectory.Text = "Invalid Title ID";
                    FlagGameSpecified = false;
                    MessageBox.Show("Only 4 characters can be used, try again. Example: The Star Fox 64 (USA) Channel's Title ID is NADE01, so you would specify NADE as the Title ID");
                    goto WiiNANDLoopback;
                }
                skipWiiNandLoopback:;
            }
        }
        private void GCRetail_CheckedChanged(object sender, EventArgs e)
        {
            if (GCRetail.Checked)
            {
                WiiVMC.Checked = false;
                WiiVMC.Enabled = false;
                RepoDownload.Enabled = true;
                GameSourceButton.Enabled = true;
                GameSourceButton.Text = "Game...";
                OpenGame.FileName = "game";
                OpenGame.Filter = "GameCube Dumps (*.gcm,*.iso)|*.gcm;*.iso";
                GameSourceDirectory.Text = "Game file has not been specified";
                GameSourceDirectory.ForeColor = Color.Red;
                FlagGameSpecified = false;
                SystemType = "gcn";
                GameNameLabel.Text = "";
                TitleIDLabel.Text = "";
                TitleIDInt = 0;
                TitleIDHex = "";
                GameType = 0;
                CucholixRepoID = "";
                PackedTitleLine1.Text = "";
                PackedTitleIDLine.Text = "";
                DRCUSE = "65537";
                GC2SourceButton.Enabled = true;
                NoGamePadEmu.Checked = false;
                CCEmu.Checked = false;
                HorWiiMote.Checked = false;
                VerWiiMote.Checked = false;
                ForceCC.Checked = false;
                ForceNoCC.Checked = false;
                GamePadEmuLayout.Enabled = false;
                LRPatch.Checked = false;
                LRPatch.Enabled = false;
                Force43NINTENDONT.Enabled = true;
                CustomMainDol.Enabled = true;
                DisableNintendontAutoboot.Enabled = true;
                DisablePassthrough.Checked = false;
                DisablePassthrough.Enabled = false;
                DisableGamePad.Enabled = true;
                C2WPatchFlag.Checked = false;
                C2WPatchFlag.Enabled = false;
                DisableTrimming.Checked = false;
                DisableTrimming.Enabled = false;
                Force43NAND.Checked = false;
                Force43NAND.Enabled = false;
            }
        }
        private void SDCardStuff_Click(object sender, EventArgs e)
        {
            new SDCardMenu().Show();
        }

        //Performs actions when switching tabs
        private void MainTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Disables Radio buttons when switching away from the main tab
            if (MainTabs.SelectedTab == SourceFilesTab)
            {
                WiiRetail.Enabled = true;
                WiiHomebrew.Enabled = true;
                WiiNAND.Enabled = true;
                GCRetail.Enabled = true;
            }
            else
            {
                WiiRetail.Enabled = false;
                WiiHomebrew.Enabled = false;
                WiiNAND.Enabled = false;
                GCRetail.Enabled = false;
            }
            //Check for building requirements when switching to the Build tab
            if (MainTabs.SelectedTab == BuildTab)
            {
                //Initialize Registry values if they don't exist and pull values from them if they do
                if (Registry.CurrentUser.CreateSubKey("WiiVCInjector").GetValue("WiiUCommonKey") == null)
                {
                    Registry.CurrentUser.CreateSubKey("WiiVCInjector").SetValue("WiiUCommonKey", "00000000000000000000000000000000");
                }
                WiiUCommonKey.Text = Registry.CurrentUser.OpenSubKey("WiiVCInjector").GetValue("WiiUCommonKey").ToString();
                if (Registry.CurrentUser.CreateSubKey("WiiVCInjector").GetValue("TitleKey") == null)
                {
                    Registry.CurrentUser.CreateSubKey("WiiVCInjector").SetValue("TitleKey", "00000000000000000000000000000000");
                }
                TitleKey.Text = Registry.CurrentUser.OpenSubKey("WiiVCInjector").GetValue("TitleKey").ToString();
                Registry.CurrentUser.OpenSubKey("WiiVCInjector").Close();
                //Generate MD5 hashes for loaded keys and check them
                WiiUCommonKey.Text = WiiUCommonKey.Text.ToUpper();
                sSourceData = WiiUCommonKey.Text;
                tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
                tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                WiiUCommonKeyHash = BitConverter.ToString(tmpHash);
                if (WiiUCommonKeyHash == "35-AC-59-94-97-22-79-33-1D-97-09-4F-A2-FB-97-FC")
                {
                    CommonKeyGood = true;
                    WiiUCommonKey.ReadOnly = true;
                    WiiUCommonKey.BackColor = Color.Lime;
                }
                else
                {
                    CommonKeyGood = false;
                    WiiUCommonKey.ReadOnly = false;
                    WiiUCommonKey.BackColor = Color.White;
                }
                TitleKey.Text = TitleKey.Text.ToUpper();
                sSourceData = TitleKey.Text;
                tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
                tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                TitleKeyHash = BitConverter.ToString(tmpHash);
                if (TitleKeyHash == "F9-4B-D8-8E-BB-7A-A9-38-67-E6-30-61-5F-27-1C-9F")
                {
                    TitleKeyGood = true;
                    TitleKey.ReadOnly = true;
                    TitleKey.BackColor = Color.Lime;
                }
                else
                {
                    TitleKeyGood = false;
                    TitleKey.ReadOnly = false;
                    TitleKey.BackColor = Color.White;
                }
                AncastKey.Text = AncastKey.Text.ToUpper();
                sSourceData = AncastKey.Text;
                tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
                tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                AncastKeyHash = BitConverter.ToString(tmpHash);
                if (AncastKeyHash == "31-8D-1F-9D-98-FB-08-E7-7C-7F-E1-77-AA-49-05-43")
                {
                    AncastKeyGood = true;
                }
                else
                {
                    AncastKeyGood = false;
                }
                //Final check for if all requirements are good
                if (FlagGameSpecified & FlagIconSpecified & FlagBannerSpecified)
                {
                    SourceCheck.ForeColor = Color.Green;
                    BuildFlagSource = true;
                }
                else
                {
                    SourceCheck.ForeColor = Color.Red;
                    BuildFlagSource = false;
                }
                if (PackedTitleLine1.Text != "" & PackedTitleIDLine.TextLength == 16)
                {
                    MetaCheck.ForeColor = Color.Green;
                    BuildFlagMeta = true;
                }
                else
                {
                    MetaCheck.ForeColor = Color.Red;
                    BuildFlagMeta = false;
                }

                if (CustomMainDol.Checked == false)
                {
                    AdvanceCheck.ForeColor = Color.Green;
                    BuildFlagAdvance = true;
                }
                else
                {
                    if (Path.GetExtension(OpenMainDol.FileName) == ".dol")
                    {
                        AdvanceCheck.ForeColor = Color.Green;
                        BuildFlagAdvance = true;
                    }
                    else
                    {
                        AdvanceCheck.ForeColor = Color.Red;
                        BuildFlagAdvance = false;
                    }
                }


                //Skip Ancast Key if box not checked in advanced
                if (C2WPatchFlag.Checked == false)
                {
                    if (CommonKeyGood & TitleKeyGood)
                    {
                        KeysCheck.ForeColor = Color.Green;
                        BuildFlagKeys = true;
                    }
                    else
                    {
                        KeysCheck.ForeColor = Color.Red;
                        BuildFlagKeys = false;
                    }
                }
                else
                {
                    if (CommonKeyGood & TitleKeyGood & AncastKeyGood)
                    {
                        KeysCheck.ForeColor = Color.Green;
                        BuildFlagKeys = true;
                    }
                    else
                    {
                        KeysCheck.ForeColor = Color.Red;
                        BuildFlagKeys = false;
                    }
                }
                //Enable Build Button
                if (BuildFlagSource & BuildFlagMeta & BuildFlagAdvance & BuildFlagKeys)
                {
                    TheBigOneTM.Enabled = true;
                }
                else
                {
                    TheBigOneTM.Enabled = false;
                }


            }
        }

        //Events for the "Required Source Files" Tab
        private void GameSourceButton_Click(object sender, EventArgs e)
        {
            if (OpenGame.ShowDialog() == DialogResult.OK)
            {
                GameSourceDirectory.Text = OpenGame.FileName;
                GameSourceDirectory.ForeColor = Color.Black;
                FlagGameSpecified = true;
                //Get values from game file
                using (var reader = new BinaryReader(File.OpenRead(OpenGame.FileName)))
                {
                    reader.BaseStream.Position = 0x00;
                    TitleIDInt = reader.ReadInt32();
                    //WBFS Check
                    if (TitleIDInt == 1397113431 /*'SFBW'*/) //Performs actions if the header indicates a WBFS file
                    {
                        FlagWBFS = true;
                        reader.BaseStream.Position = 0x200;
                        TitleIDInt = reader.ReadInt32();
                        reader.BaseStream.Position = 0x218;
                        GameType = reader.ReadInt64();
                        InternalGameName = ReadStringFromBinaryStream(reader, 0x220);
                        CucholixRepoID = ReadStringFromBinaryStream(reader, 0x200);
                    }
                    else
                    {
                        if (TitleIDInt == 65536) //Performs actions if the header indicates a DOL file
                        {
                            reader.BaseStream.Position = 0x2A0;
                            TitleIDInt = reader.ReadInt32();
                            InternalGameName = "N/A";
                        }
                        else //Performs actions if the header indicates a normal Wii / GC iso
                        {
                            FlagWBFS = false;
                            reader.BaseStream.Position = 0x18;
                            GameType = reader.ReadInt64();
                            InternalGameName = ReadStringFromBinaryStream(reader, 0x20);
                            CucholixRepoID = ReadStringFromBinaryStream(reader, 0x00);
                        }
                    }
                }
                //Flag if GameType Int doesn't match current SystemType
                if (SystemType == "wii" && GameType != 2745048157)
                {
                    GameSourceDirectory.Text = "Game file has not been specified";
                    GameSourceDirectory.ForeColor = Color.Red;
                    FlagGameSpecified = false;
                    GameNameLabel.Text = "";
                    TitleIDLabel.Text = "";
                    TitleIDInt = 0;
                    TitleIDHex = "";
                    GameType = 0;
                    CucholixRepoID = "";
                    PackedTitleLine1.Text = "";
                    PackedTitleIDLine.Text = "";
                    MessageBox.Show("This is not a Wii image. It will not be loaded.");
                    goto EndOfGameSelection;
                }
                if (SystemType == "gcn" && GameType != 4440324665927270400)
                {
                    GameSourceDirectory.Text = "Game file has not been specified";
                    GameSourceDirectory.ForeColor = Color.Red;
                    FlagGameSpecified = false;
                    GameNameLabel.Text = "";
                    TitleIDLabel.Text = "";
                    TitleIDInt = 0;
                    TitleIDHex = "";
                    GameType = 0;
                    CucholixRepoID = "";
                    PackedTitleLine1.Text = "";
                    PackedTitleIDLine.Text = "";
                    MessageBox.Show("This is not a GameCube image. It will not be loaded.");
                    goto EndOfGameSelection;
                }
                GameNameLabel.Text = InternalGameName;
                PackedTitleLine1.Text = InternalGameName;
                //Convert pulled Title ID Int to Hex for use with Wii U Title ID
                TitleIDHex = TitleIDInt.ToString("X");
                TitleIDHex = TitleIDHex.Substring(6, 2) + TitleIDHex.Substring(4, 2) + TitleIDHex.Substring(2, 2) + TitleIDHex.Substring(0, 2);
                if (SystemType == "dol")
                {
                    TitleIDLabel.Text = TitleIDHex;
                    PackedTitleIDLine.Text = ("00050002" + TitleIDHex);
                    TitleIDText = "BOOT";
                }
                else
                {
                    TitleIDText = string.Join("", System.Text.RegularExpressions.Regex.Split(TitleIDHex, "(?<=\\G..)(?!$)").Select(x => (char)Convert.ToByte(x, 16)));
                    TitleIDLabel.Text = (TitleIDText + " / " + TitleIDHex);
                    PackedTitleIDLine.Text = ("00050002" + TitleIDHex);
                }
            }
            else
            {
                GameSourceDirectory.Text = "Game file has not been specified";
                GameSourceDirectory.ForeColor = Color.Red;
                FlagGameSpecified = false;
                GameNameLabel.Text = "";
                TitleIDLabel.Text = "";
                TitleIDInt = 0;
                TitleIDHex = "";
                GameType = 0;
                CucholixRepoID = "";
                PackedTitleLine1.Text = "";
                PackedTitleIDLine.Text = "";
                goto EndOfGameSelection;
            }
            EndOfGameSelection:;
        }
        private void IconSourceButton_Click(object sender, EventArgs e)
        {
            if (FlagRepo)
            {
                IconPreviewBox.Image = null;
                BannerPreviewBox.Image = null;
                FlagIconSpecified = false;
                FlagBannerSpecified = false;
                FlagRepo = false;
                pngtemppath = "";
            }
            MessageBox.Show("Make sure your icon is 128x128 (1:1) to prevent distortion");
            if (OpenIcon.ShowDialog() == DialogResult.OK)
            {
               if (Path.GetExtension(OpenIcon.FileName) == ".tga")
               {
                    pngtemppath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\iconTex.png";
                    if (File.Exists(pngtemppath)) { File.Delete(pngtemppath); }
                    LauncherExeFile = TempToolsPath + "IMG\\tga2pngcmd.exe";
                    LauncherExeArgs = "-i \"" + OpenIcon.FileName + "\" -o \"" + Path.GetDirectoryName(pngtemppath) + "\"";
                    LaunchProgram();
                    File.Move(Path.GetDirectoryName(pngtemppath) + "\\" + Path.GetFileNameWithoutExtension(OpenIcon.FileName) + ".png", pngtemppath);
               }
               else
               {
                   pngtemppath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\iconTex.png";
                   if (File.Exists(pngtemppath)) { File.Delete(pngtemppath); }
                   Image.FromFile(OpenIcon.FileName).Save(pngtemppath, System.Drawing.Imaging.ImageFormat.Png);
               } 
                FileStream tempstream = new FileStream(pngtemppath, FileMode.Open);
                var tempimage = Image.FromStream(tempstream);
                IconPreviewBox.Image = tempimage;
                tempstream.Close();
                IconSourceDirectory.Text = OpenIcon.FileName;
                IconSourceDirectory.ForeColor = Color.Black;
                FlagIconSpecified = true;
                FlagRepo = false;
            }
            else
            {
                IconPreviewBox.Image = null;
                IconSourceDirectory.Text = "Icon has not been specified";
                IconSourceDirectory.ForeColor = Color.Red;
                FlagIconSpecified = false;
                FlagRepo = false;
                pngtemppath = "";
            }
        }
        private void BannerSourceButton_Click(object sender, EventArgs e)
        {
            if (FlagRepo)
            {
                BannerPreviewBox.Image = null;
                BannerPreviewBox.Image = null;
                FlagBannerSpecified = false;
                FlagBannerSpecified = false;
                FlagRepo = false;
                pngtemppath = "";
            }
            MessageBox.Show("Make sure your Banner is 1280x720 (16:9) to prevent distortion");
            if (OpenBanner.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(OpenBanner.FileName) == ".tga")
                {
                    pngtemppath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootTvTex.png";
                    if (File.Exists(pngtemppath)) { File.Delete(pngtemppath); }
                    LauncherExeFile = TempToolsPath + "IMG\\tga2pngcmd.exe";
                    LauncherExeArgs = "-i \"" + OpenBanner.FileName + "\" -o \"" + Path.GetDirectoryName(pngtemppath) + "\"";
                    LaunchProgram();
                    File.Move(Path.GetDirectoryName(pngtemppath) + "\\" + Path.GetFileNameWithoutExtension(OpenBanner.FileName) + ".png", pngtemppath);
                }
                else
                {
                    pngtemppath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootTvTex.png";
                    if (File.Exists(pngtemppath)) { File.Delete(pngtemppath); }
                    Image.FromFile(OpenBanner.FileName).Save(pngtemppath, System.Drawing.Imaging.ImageFormat.Png);
                }
                FileStream tempstream = new FileStream(pngtemppath, FileMode.Open);
                var tempimage = Image.FromStream(tempstream);
                BannerPreviewBox.Image = tempimage;
                tempstream.Close();
                BannerSourceDirectory.Text = OpenBanner.FileName;
                BannerSourceDirectory.ForeColor = Color.Black;
                FlagBannerSpecified = true;
                FlagRepo = false;
            }
            else
            {
                BannerPreviewBox.Image = null;
                BannerSourceDirectory.Text = "Banner has not been specified";
                BannerSourceDirectory.ForeColor = Color.Red;
                FlagBannerSpecified = false;
                FlagRepo = false;
                pngtemppath = "";
            }
        }
        private void RepoDownload_Click(object sender, EventArgs e)
        {
            if (CucholixRepoID == "")
            {
                MessageBox.Show("Please select your game before using this option");
                FlagRepo = false;
            }
            else
            {
                if (SystemType == "wiiware")
                {
                    if (RemoteFileExists("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID + "/iconTex.png") == true)
                    {
                        DownloadFromRepo();
                    }
                    else if (RemoteFileExists("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID.Substring(0, 3) + "E" + "/iconTex.png") == true)
                    {
                        CucholixRepoID = CucholixRepoID.Substring(0, 3) + "E";
                        DownloadFromRepo();
                    }
                    else if (RemoteFileExists("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID.Substring(0, 3) + "P" + "/iconTex.png") == true)
                    {
                        CucholixRepoID = CucholixRepoID.Substring(0, 3) + "P";
                        DownloadFromRepo();
                    }
                    else if (RemoteFileExists("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID.Substring(0, 3) + "J" + "/iconTex.png") == true)
                    {
                        CucholixRepoID = CucholixRepoID.Substring(0, 3) + "J";
                        DownloadFromRepo();
                    }
                    else
                    {
                        FlagRepo = false;
                        if (MessageBox.Show("Cucholix's Repo does not have assets for your game. You will need to provide your own. Would you like to visit the GBAtemp request thread?", "Game not found on Repo", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://gbatemp.net/threads/483080/");
                        }
                    }
                }
                else
                {
                    if (RemoteFileExists("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID + "/iconTex.png") == true)
                    {
                        DownloadFromRepo();
                    }
                    else if (RemoteFileExists("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID.Substring(0, 3) + "E" + CucholixRepoID.Substring(4, 2) + "/iconTex.png") == true)
                    {
                        CucholixRepoID = CucholixRepoID.Substring(0, 3) + "E" + CucholixRepoID.Substring(4, 2);
                        DownloadFromRepo();
                    }
                    else if (RemoteFileExists("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID.Substring(0, 3) + "P" + CucholixRepoID.Substring(4, 2) + "/iconTex.png") == true)
                    {
                        CucholixRepoID = CucholixRepoID.Substring(0, 3) + "P" + CucholixRepoID.Substring(4, 2);
                        DownloadFromRepo();
                    }
                    else if (RemoteFileExists("https://raw.githubusercontent.com/cucholix/wiivc-bis/master/" + SystemType + "/image/" + CucholixRepoID.Substring(0, 3) + "J" + CucholixRepoID.Substring(4, 2) + "/iconTex.png") == true)
                    {
                        CucholixRepoID = CucholixRepoID.Substring(0, 3) + "J" + CucholixRepoID.Substring(4, 2);
                        DownloadFromRepo();
                    }
                    else
                    {
                        FlagRepo = false;
                        if (MessageBox.Show("Cucholix's Repo does not have assets for your game. You will need to provide your own. Would you like to visit the GBAtemp request thread?", "Game not found on Repo", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://gbatemp.net/threads/483080/");
                        }
                    }
                }
            }
        }
        

        //Events for the "Optional Source Files" Tab
        private void GC2SourceButton_Click(object sender, EventArgs e)
        {
            if (OpenGC2.ShowDialog() == DialogResult.OK)
            {
                using (var reader = new BinaryReader(File.OpenRead(OpenGC2.FileName)))
                {
                    reader.BaseStream.Position = 0x18;
                    long GC2GameType = reader.ReadInt64();
                    if (GC2GameType != 4440324665927270400)
                    {
                        MessageBox.Show("This is not a GameCube image. It will not be loaded.");
                        GC2SourceDirectory.Text = "2nd GameCube Disc Image has not been specified";
                        GC2SourceDirectory.ForeColor = Color.Red;
                        FlagGC2Specified = false;
                    }
                    else
                    {
                        GC2SourceDirectory.Text = OpenGC2.FileName;
                        GC2SourceDirectory.ForeColor = Color.Black;
                        FlagGC2Specified = true;
                    }
                }
            }
            else
            {
                GC2SourceDirectory.Text = "2nd GameCube Disc Image has not been specified";
                GC2SourceDirectory.ForeColor = Color.Red;
                FlagGC2Specified = false;
            }
        }
        private void DrcSourceButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Make sure your GamePad Banner is 854x480 (16:9) to prevent distortion");
            if (OpenDrc.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(OpenDrc.FileName) == ".tga")
                {
                    pngtemppath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootDrcTex.png";
                    if (File.Exists(pngtemppath)) { File.Delete(pngtemppath); }
                    LauncherExeFile = TempToolsPath + "IMG\\tga2pngcmd.exe";
                    LauncherExeArgs = "-i \"" + OpenDrc.FileName + "\" -o \"" + Path.GetDirectoryName(pngtemppath) + "\"";
                    LaunchProgram();
                    File.Move(Path.GetDirectoryName(pngtemppath) + "\\" + Path.GetFileNameWithoutExtension(OpenDrc.FileName) + ".png", pngtemppath);
                }
                else
                {
                    pngtemppath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootDrcTex.png";
                    if (File.Exists(pngtemppath)) { File.Delete(pngtemppath); }
                    Image.FromFile(OpenDrc.FileName).Save(pngtemppath, System.Drawing.Imaging.ImageFormat.Png);
                }
                FileStream tempstream = new FileStream(pngtemppath, FileMode.Open);
                var tempimage = Image.FromStream(tempstream);
                DrcPreviewBox.Image = tempimage;
                tempstream.Close();
                DrcSourceDirectory.Text = OpenDrc.FileName;
                DrcSourceDirectory.ForeColor = Color.Black;
                FlagDrcSpecified = true;
                FlagRepo = false;
            }
            else
            {
                DrcPreviewBox.Image = null;
                DrcSourceDirectory.Text = "GamePad Banner has not been specified";
                DrcSourceDirectory.ForeColor = Color.Red;
                pngtemppath = "";
            }
        }
        private void LogoSourceButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Make sure your Logo is 170x42 to prevent distortion");
            if (OpenLogo.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(OpenLogo.FileName) == ".tga")
                {
                    pngtemppath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootLogoTex.png";
                    if (File.Exists(pngtemppath)) { File.Delete(pngtemppath); }
                    LauncherExeFile = TempToolsPath + "IMG\\tga2pngcmd.exe";
                    LauncherExeArgs = "-i \"" + OpenLogo.FileName + "\" -o \"" + Path.GetDirectoryName(pngtemppath) + "\"";
                    LaunchProgram();
                    File.Move(Path.GetDirectoryName(pngtemppath) + "\\" + Path.GetFileNameWithoutExtension(OpenLogo.FileName) + ".png", pngtemppath);
                }
                else
                {
                    pngtemppath = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\bootLogoTex.png";
                    if (File.Exists(pngtemppath)) { File.Delete(pngtemppath); }
                    Image.FromFile(OpenLogo.FileName).Save(pngtemppath, System.Drawing.Imaging.ImageFormat.Png);
                }
                FileStream tempstream = new FileStream(pngtemppath, FileMode.Open);
                var tempimage = Image.FromStream(tempstream);
                LogoPreviewBox.Image = tempimage;
                tempstream.Close();
                LogoSourceDirectory.Text = OpenLogo.FileName;
                LogoSourceDirectory.ForeColor = Color.Black;
                FlagLogoSpecified = true;
                FlagRepo = false;
            }
            else
            {
                LogoPreviewBox.Image = null;
                LogoSourceDirectory.Text = "GamePad Banner has not been specified";
                LogoSourceDirectory.ForeColor = Color.Red;
                pngtemppath = "";
            }
        }
        private void BootSoundButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Your sound file will be cut off if it's longer than 6 seconds to prevent the Wii U from not loading it. When the Wii U plays the boot sound, it will fade out once it's done loading the game (usually after about 5 seconds). You can not change this.");
            if (OpenBootSound.ShowDialog() == DialogResult.OK)
            {
                using (var reader = new BinaryReader(File.OpenRead(OpenBootSound.FileName)))
                {
                    reader.BaseStream.Position = 0x00;
                    long WAVHeader1 = reader.ReadInt32();
                    reader.BaseStream.Position = 0x08;
                    long WAVHeader2 = reader.ReadInt32();
                    if (WAVHeader1 == 1179011410 & WAVHeader2 == 1163280727)
                    {
                        BootSoundDirectory.Text = OpenBootSound.FileName;
                        BootSoundDirectory.ForeColor = Color.Black;
                        BootSoundPreviewButton.Enabled = true;
                        FlagBootSoundSpecified = true;
                    }
                    else
                    {
                        MessageBox.Show("This is not a valid WAV file. It will not be loaded. \nConsider converting it with something like Audacity.");
                        BootSoundDirectory.Text = "Boot Sound has not been specified";
                        BootSoundDirectory.ForeColor = Color.Red;
                        BootSoundPreviewButton.Enabled = false;
                        FlagBootSoundSpecified = false;
                    }
                }
            }
            else
            {
                if (BootSoundPreviewButton.Text != "Stop Sound")
                {
                    BootSoundDirectory.Text = "Boot Sound has not been specified";
                    BootSoundDirectory.ForeColor = Color.Red;
                    BootSoundPreviewButton.Enabled = false;
                    FlagBootSoundSpecified = false;
                }
            }
        }
        private void ToggleBootSoundLoop_CheckedChanged(object sender, EventArgs e)
        {
            if (ToggleBootSoundLoop.Checked)
            {
                LoopString = "";
            }
            else
            {
                LoopString = " -noLoop";
            }
        }
        private void BootSoundPreviewButton_Click(object sender, EventArgs e)
        {
            var simpleSound = new SoundPlayer(OpenBootSound.FileName);
            if (BootSoundPreviewButton.Text == "Stop Sound")
            {
                simpleSound.Stop();
                BootSoundPreviewButton.Text = "Play Sound";
            }
            else
            {
                if (ToggleBootSoundLoop.Checked)
                {
                    simpleSound.PlayLooping();
                    BootSoundPreviewButton.Text = "Stop Sound";
                }
                else
                {
                    simpleSound.Play();
                }
            }
        }

        //Events for the "GamePad/Meta Options" Tab
        private void EnablePackedLine2_CheckedChanged(object sender, EventArgs e)
        {
            if (EnablePackedLine2.Checked)
            {
                PackedTitleLine2.Text = "";
                PackedTitleLine2.BackColor = Color.White;
                PackedTitleLine2.ReadOnly = false;
            }
            else
            {
                PackedTitleLine2.Text = "(Optional) Line 2";
                PackedTitleLine2.BackColor = Color.Silver;
                PackedTitleLine2.ReadOnly = true;
            }

        }
        //Radio Buttons for GamePad Emulation Mode
        private void NoGamePadEmu_CheckedChanged(object sender, EventArgs e)
        {
            if (NoGamePadEmu.Checked)
            {
                DRCUSE = "1";
                nfspatchflag = "";
                LRPatch.Checked = false;
                LRPatch.Enabled = false;
            }
        }
        private void CCEmu_CheckedChanged(object sender, EventArgs e)
        {
            if (CCEmu.Checked)
            {
                DRCUSE = "65537";
                nfspatchflag = "";
                LRPatch.Enabled = true;
            }
        }
        private void HorWiiMote_CheckedChanged(object sender, EventArgs e)
        {
            if (HorWiiMote.Checked)
            {
                DRCUSE = "65537";
                nfspatchflag = " -horizontal";
                LRPatch.Checked = false;
                LRPatch.Enabled = false;
            }
        }
        private void VerWiiMote_CheckedChanged(object sender, EventArgs e)
        {
            if (VerWiiMote.Checked)
            {
                DRCUSE = "65537";
                nfspatchflag = " -wiimote";
                LRPatch.Checked = false;
                LRPatch.Enabled = false;
            }
        }
        private void ForceCC_CheckedChanged(object sender, EventArgs e)
        {
            if (ForceCC.Checked)
            {
                DRCUSE = "65537";
                nfspatchflag = " -instantcc";
                DisableTrimming.Checked = false;
                DisableTrimming.Enabled = false;
                LRPatch.Enabled = true;
            }
        }
        private void ForceWiiMote_CheckedChanged(object sender, EventArgs e)
        {
            if (ForceNoCC.Checked)
            {
                DRCUSE = "65537";
                nfspatchflag = " -nocc";
                LRPatch.Checked = false;
                LRPatch.Enabled = false;
            }
        }
        private void TutorialLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.google.com");
        }

        //Events for the Advanced Tab
        private void Force43NINTENDONT_CheckedChanged(object sender, EventArgs e)
        {
            if (Force43NINTENDONT.Checked)
            {
                CustomMainDol.Checked = false;
                CustomMainDol.Enabled = false;
                DisableNintendontAutoboot.Checked = false;
                DisableNintendontAutoboot.Enabled = false;
            }
            else
            {
                CustomMainDol.Enabled = true;
                DisableNintendontAutoboot.Enabled = true;
            }
        }
        private void CustomMainDol_CheckedChanged(object sender, EventArgs e)
        {
            if (CustomMainDol.Checked)
            {
                MainDolSourceButton.Enabled = true;
                Force43NINTENDONT.Checked = false;
                Force43NINTENDONT.Enabled = false;
                DisableNintendontAutoboot.Checked = false;
                DisableNintendontAutoboot.Enabled = false;

            }
            else
            {
                MainDolSourceButton.Enabled = false;
                MainDolLabel.Text = "<- Specify custom main.dol file";
                Force43NINTENDONT.Enabled = true;
                DisableNintendontAutoboot.Enabled = true;
                OpenMainDol.FileName = null;
            }
        }
        private void NintendontAutoboot_CheckedChanged(object sender, EventArgs e)
        {
            if (DisableNintendontAutoboot.Checked)
            {
                Force43NINTENDONT.Checked = false;
                Force43NINTENDONT.Enabled = false;
                CustomMainDol.Checked = false;
                CustomMainDol.Enabled = false;
            }
            else
            {
                Force43NINTENDONT.Enabled = true;
                CustomMainDol.Enabled = true;
            }
        }
        private void MainDolSourceButton_Click(object sender, EventArgs e)
        {
            if (OpenMainDol.ShowDialog() == DialogResult.OK)
            {
                MainDolLabel.Text = OpenMainDol.FileName;
            }
            else
            {
                MainDolLabel.Text = "<- Specify custom main.dol file";
            }
        }
        private void DisablePassthrough_CheckedChanged(object sender, EventArgs e)
        {
            if (DisablePassthrough.Checked)
            {
                passpatch = "";
            }
            else
            {
                passpatch = " -passthrough";
            }
        }
        private void DisableGamePad_CheckedChanged(object sender, EventArgs e)
        {
            if (DisableGamePad.Checked)
            {
                if (SystemType == "gcn")
                {
                    DRCUSE = "1";
                }
                else if (SystemType == "dol")
                {
                    DRCUSE = "1";
                }
            }
            else
            {
                if (SystemType == "gcn")
                {
                    DRCUSE = "65537";
                }
                else if (SystemType == "dol")
                {
                    DRCUSE = "65537";
                }
            }
        }
        private void C2WPatchFlag_CheckedChanged(object sender, EventArgs e)
        {
            if (C2WPatchFlag.Checked)
            {
                AncastKey.ReadOnly = false;
                AncastKey.BackColor = Color.White;
                SaveAncastKeyButton.Enabled = true;
                if (Registry.CurrentUser.CreateSubKey("WiiVCInjector").GetValue("AncastKey") == null)
                {
                    Registry.CurrentUser.CreateSubKey("WiiVCInjector").SetValue("AncastKey", "00000000000000000000000000000000");
                }
                AncastKey.Text = Registry.CurrentUser.OpenSubKey("WiiVCInjector").GetValue("AncastKey").ToString();
                Registry.CurrentUser.OpenSubKey("WiiVCInjector").Close();
                //If key is correct, lock text box for edits
                AncastKey.Text = AncastKey.Text.ToUpper();
                sSourceData = AncastKey.Text;
                tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
                tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                AncastKeyHash = BitConverter.ToString(tmpHash);
                if (AncastKeyHash == "31-8D-1F-9D-98-FB-08-E7-7C-7F-E1-77-AA-49-05-43")
                {
                    AncastKey.ReadOnly = true;
                    AncastKey.BackColor = Color.Lime;
                }
                else
                {
                    AncastKey.ReadOnly = false;
                    AncastKey.BackColor = Color.White;
                }
            }
            else
            {
                AncastKey.BackColor = Color.Silver;
                AncastKey.ReadOnly = true;
                SaveAncastKeyButton.Enabled = false;
            }
        }
        private void SaveAncastKeyButton_Click(object sender, EventArgs e)
        {
            //Verify Title Key MD5 Hash
            AncastKey.Text = AncastKey.Text.ToUpper();
            sSourceData = AncastKey.Text;
            tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
            tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            AncastKeyHash = BitConverter.ToString(tmpHash);
            if (AncastKeyHash == "31-8D-1F-9D-98-FB-08-E7-7C-7F-E1-77-AA-49-05-43")
            {
                Registry.CurrentUser.CreateSubKey("WiiVCInjector").SetValue("AncastKey", AncastKey.Text);
                Registry.CurrentUser.CreateSubKey("WiiVCInjector").Close();
                MessageBox.Show("The Wii U Starbuck Ancast Key has been verified.");
                AncastKey.ReadOnly = true;
                AncastKey.BackColor = Color.Lime;
            }
            else
            {
                MessageBox.Show("The Wii U Starbuck Ancast Key you have provided is incorrect" + "\n" + "(MD5 Hash verification failed)");
            }
        }
        private void sign_c2w_patcher_link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/FIX94/sign_c2w_patcher");
        }
        private void DisableTrimming_CheckedChanged(object sender, EventArgs e)
        {
            if (DisableTrimming.Checked)
            {
                WiiVMC.Checked = false;
                WiiVMC.Enabled = false;
            }
            else
            {
                if (SystemType == "wii")
                {
                    WiiVMC.Enabled = true;
                }
                else
                {
                    WiiVMC.Checked = false;
                    WiiVMC.Enabled = false;
                }
            }
        }

        //Events for the "Build Title" Tab
        private void SaveCommonKeyButton_Click(object sender, EventArgs e)
        {
            //Verify Wii U Common Key MD5 Hash
            WiiUCommonKey.Text = WiiUCommonKey.Text.ToUpper();
            sSourceData = WiiUCommonKey.Text;
            tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
            tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            WiiUCommonKeyHash = BitConverter.ToString(tmpHash);
            if (WiiUCommonKeyHash == "35-AC-59-94-97-22-79-33-1D-97-09-4F-A2-FB-97-FC")
            {
                Registry.CurrentUser.CreateSubKey("WiiVCInjector").SetValue("WiiUCommonKey", WiiUCommonKey.Text);
                Registry.CurrentUser.CreateSubKey("WiiVCInjector").Close();
                MessageBox.Show("The Wii U Common Key has been verified.");
                MainTabs.SelectedTab = AdvancedTab;
                MainTabs.SelectedTab = BuildTab;
            }
            else
            {
                MessageBox.Show("The Wii U Common Key you have provided is incorrect" + "\n" + "(MD5 Hash verification failed)");
            }
        }
        private void SaveTitleKeyButton_Click(object sender, EventArgs e)
        {
            //Verify Title Key MD5 Hash
            TitleKey.Text = TitleKey.Text.ToUpper();
            sSourceData = TitleKey.Text;
            tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
            tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            TitleKeyHash = BitConverter.ToString(tmpHash);
            if (TitleKeyHash == "F9-4B-D8-8E-BB-7A-A9-38-67-E6-30-61-5F-27-1C-9F")
            {
                Registry.CurrentUser.CreateSubKey("WiiVCInjector").SetValue("TitleKey", TitleKey.Text);
                Registry.CurrentUser.CreateSubKey("WiiVCInjector").Close();
                MessageBox.Show("The Title Key has been verified.");
                MainTabs.SelectedTab = AdvancedTab;
                MainTabs.SelectedTab = BuildTab;
            }
            else
            {
                MessageBox.Show("The Title Key you have provided is incorrect" + "\n" + "(MD5 Hash verification failed)");
            }
        }
        private void DebugButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ShortenPath(OpenGame.FileName));
        }
        //Events for the actual "Build" Button
        private void TheBigOneTM_Click(object sender, EventArgs e)
        {
            //Initialize Build Process
            //Disable form elements so navigation can't be attempted during build process
            MainTabs.Enabled = false;
            //Check for free space
            if (SystemType == "wii")
            {
                long gamesize = new FileInfo(OpenGame.FileName).Length;
                var drive = new DriveInfo(TempRootPath);
                long freeSpaceInBytes = drive.AvailableFreeSpace;
                if (freeSpaceInBytes < gamesize * 2 + 5000000000)
                {
                    DialogResult dialogResult = MessageBox.Show("Your hard drive may be low on space. The conversion process involves temporary files that can amount to more than double the size of your game. If you continue without clearing some hard drive space, the conversion may fail. Do you want to continue anyways?", "Check your hard drive space", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                    {
                        MainTabs.Enabled = true;
                        BuildStatus.Text = "";
                        BuildProgress.Value = 0;
                        goto BuildProcessFin;
                    }
                }
            }
            if (SystemType == "dol")
            {
                var drive = new DriveInfo(TempRootPath);
                long freeSpaceInBytes = drive.AvailableFreeSpace;
                if (freeSpaceInBytes < 6000000000)
                {
                    DialogResult dialogResult = MessageBox.Show("Your hard drive may be low on space. Even for small programs, the conversion process can use almost 5 GB of temporary storage. If you continue without clearing some hard drive space, the conversion may fail. Do you want to continue anyways?", "Check your hard drive space", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                    {
                        MainTabs.Enabled = true;
                        BuildStatus.Text = "";
                        BuildProgress.Value = 0;
                        goto BuildProcessFin;
                    }
                }
            }
            if (SystemType == "wiiware")
            {
                var drive = new DriveInfo(TempRootPath);
                long freeSpaceInBytes = drive.AvailableFreeSpace;
                if (freeSpaceInBytes < 6000000000)
                {
                    DialogResult dialogResult = MessageBox.Show("Your hard drive may be low on space. Even for small programs, the conversion process can use almost 5 GB of temporary storage. If you continue without clearing some hard drive space, the conversion may fail. Do you want to continue anyways?", "Check your hard drive space", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                    {
                        MainTabs.Enabled = true;
                        BuildStatus.Text = "";
                        BuildProgress.Value = 0;
                        goto BuildProcessFin;
                    }
                }
            }
            if (SystemType == "gcn")
            {
                long gamesize = new FileInfo(OpenGame.FileName).Length;
                var drive = new DriveInfo(TempRootPath);
                long freeSpaceInBytes = drive.AvailableFreeSpace;
                if (freeSpaceInBytes < gamesize * 2 + 6000000000)
                {
                    DialogResult dialogResult = MessageBox.Show("Your hard drive may be low on space. The conversion process involves temporary files that can amount to more than double the size of your game. If you continue without clearing some hard drive space, the conversion may fail. Do you want to continue anyways?", "Check your hard drive space", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                    {
                        MainTabs.Enabled = true;
                        BuildStatus.Text = "";
                        BuildProgress.Value = 0;
                        goto BuildProcessFin;
                    }
                }
            }
            //Specify Path Variables to be called later
            if (OutputFolderSelect.ShowDialog() == DialogResult.Cancel)
            {
                MessageBox.Show("Output folder selection has been cancelled, conversion will not continue.");
                MainTabs.Enabled = true;
                goto BuildProcessFin;
            }
            BuildProgress.Value = 2;
            //////////////////////////

            //Download base files with JNUSTool, store them for future use
            if (Directory.Exists(JNUSToolDownloads) == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004000\\code\\deint.txt") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004000\\code\\font.bin") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004001\\code\\c2w.img") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004001\\code\\boot.bin") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004001\\code\\dmcu.d.hex") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\cos.xml") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\frisbiiU.rpx") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\fw.img") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\fw.tmd") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\htk.bin") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\nn_hai_user.rpl") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\content\\assets\\shaders\\cafe\\banner.gsh") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\content\\assets\\shaders\\cafe\\fade.gsh") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\meta\\bootMovie.h264") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\meta\\bootLogoTex.tga") == false)
            {
                goto JNUSStuff;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\meta\\bootSound.btsnd") == false)
            {
                goto JNUSStuff;
            }
            goto SkipJNUS;
            JNUSStuff:;
            if (CheckForInternetConnection() == false)
            {
                DialogResult dialogResult = MessageBox.Show("Your internet connection could not be verified, do you wish to try and download the necessary base files from Nintendo anyways? (This is a one-time download)", "Internet Connection Verification Failed", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    MainTabs.Enabled = true;
                    BuildStatus.Text = "";
                    BuildProgress.Value = 0;
                    goto BuildProcessFin;
                }
            }
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo...";
            BuildStatus.Refresh();
            string[] JNUSToolConfig = { "http://ccs.cdn.wup.shop.nintendo.net/ccs/download", WiiUCommonKey.Text };
            File.WriteAllLines(TempToolsPath + "JAR\\config", JNUSToolConfig);
            Directory.SetCurrentDirectory(TempToolsPath + "JAR");
            BuildProgress.Value = 10;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (deint.txt)";
            BuildStatus.Refresh();
            LauncherExeFile = "JNUSTool.exe";
            LauncherExeArgs = "0005001010004000 -file /code/deint.txt";
            LaunchProgram();
            BuildProgress.Value = 12;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (font.bin)";
            BuildStatus.Refresh();
            LauncherExeArgs = "0005001010004000 -file /code/font.bin";
            LaunchProgram();
            BuildProgress.Value = 15;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (c2w.img)";
            BuildStatus.Refresh();
            LauncherExeArgs = "0005001010004001 -file /code/c2w.img";
            LaunchProgram();
            BuildProgress.Value = 17;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (boot.bin)";
            BuildStatus.Refresh();
            LauncherExeArgs = "0005001010004001 -file /code/boot.bin";
            LaunchProgram();
            BuildProgress.Value = 20;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (dmcu.d.hex)";
            BuildStatus.Refresh();
            LauncherExeArgs = "0005001010004001 -file /code/dmcu.d.hex";
            LaunchProgram();
            BuildProgress.Value = 23;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (cos.xml)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /code/cos.xml";
            LaunchProgram();
            BuildProgress.Value = 25;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (frisbiiU.rpx)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /code/frisbiiU.rpx";
            LaunchProgram();
            BuildProgress.Value = 27;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (fw.img)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /code/fw.img";
            LaunchProgram();
            BuildProgress.Value = 30;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (fw.tmd)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /code/fw.tmd";
            LaunchProgram();
            BuildProgress.Value = 32;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (htk.bin)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /code/htk.bin";
            LaunchProgram();
            BuildProgress.Value = 35;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (nn_hai_user.rpl)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /code/nn_hai_user.rpl";
            LaunchProgram();
            BuildProgress.Value = 37;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (banner.gsh / fade.gsh)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /content/assets/.*";
            LaunchProgram();
            BuildProgress.Value = 40;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (bootMovie.h264)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /meta/bootMovie.h264";
            LaunchProgram();
            BuildProgress.Value = 42;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (bootLogoTex.tga)";
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /meta/bootLogoTex.tga";
            LaunchProgram();
            BuildProgress.Value = 45;
            BuildStatus.Text = "(One-Time Download) Downloading base files from Nintendo... (bootSound.btsnd)";
            BuildStatus.Refresh();
            LauncherExeArgs = "00050000101b0700 " + TitleKey.Text + " -file /meta/bootSound.btsnd";
            LaunchProgram();
            BuildProgress.Value = 47;
            BuildStatus.Text = "Saving files from Nintendo for future use...";
            BuildStatus.Refresh();
            Directory.CreateDirectory(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]");
            Directory.CreateDirectory(JNUSToolDownloads + "0005001010004000");
            Directory.CreateDirectory(JNUSToolDownloads + "0005001010004001");
            FileSystem.CopyDirectory("Rhythm Heaven Fever [VAKE01]", JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]");
            FileSystem.CopyDirectory("0005001010004000", JNUSToolDownloads + "0005001010004000");
            FileSystem.CopyDirectory("0005001010004001", JNUSToolDownloads + "0005001010004001");
            Directory.Delete("Rhythm Heaven Fever [VAKE01]", true);
            Directory.Delete("0005001010004000", true);
            Directory.Delete("0005001010004001", true);
            File.Delete("config");
            //Check if files exist after they were supposed to be downloaded
            bool JNUSFail = false;
            if (File.Exists(JNUSToolDownloads + "0005001010004000\\code\\deint.txt") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004000\\code\\font.bin") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004001\\code\\c2w.img") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004001\\code\\boot.bin") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "0005001010004001\\code\\dmcu.d.hex") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\cos.xml") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\frisbiiU.rpx") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\fw.img") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\fw.tmd") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\htk.bin") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\code\\nn_hai_user.rpl") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\content\\assets\\shaders\\cafe\\banner.gsh") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\content\\assets\\shaders\\cafe\\fade.gsh") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\meta\\bootMovie.h264") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\meta\\bootLogoTex.tga") == false)
            {
                JNUSFail = true;
            }
            if (File.Exists(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]\\meta\\bootSound.btsnd") == false)
            {
                JNUSFail = true;
            }
            if (JNUSFail)
            {
                MessageBox.Show("Failed to download base files using JNUSTool, conversion will not continue");
                MainTabs.Enabled = true;
                BuildStatus.Text = "";
                BuildProgress.Value = 0;
                goto BuildProcessFin;
            }
            Directory.SetCurrentDirectory(TempRootPath);
            SkipJNUS:;
            ///////////////////////////////////

            //Copy downloaded files to the build directory
            BuildStatus.Text = "Copying base files to temporary build directory...";
            BuildStatus.Refresh();
            FileSystem.CopyDirectory(JNUSToolDownloads + "Rhythm Heaven Fever [VAKE01]", TempBuildPath);
            if (C2WPatchFlag.Checked)
            {
                FileSystem.CopyDirectory(JNUSToolDownloads + "0005001010004000", TempBuildPath);
                FileSystem.CopyDirectory(JNUSToolDownloads + "0005001010004001", TempBuildPath);
                string[] AncastKeyCopy = { AncastKey.Text };
                File.WriteAllLines(TempToolsPath + "C2W\\starbuck_key.txt", AncastKeyCopy);
                File.Copy(TempBuildPath + "code\\c2w.img", TempToolsPath + "C2W\\c2w.img");
                Directory.SetCurrentDirectory(TempToolsPath + "C2W");
                LauncherExeFile = "c2w_patcher.exe";
                LauncherExeArgs = "-nc";
                LaunchProgram();
                File.Delete(TempBuildPath + "code\\c2w.img");
                File.Copy(TempToolsPath + "C2W\\c2p.img", TempBuildPath + "code\\c2w.img", true);
                File.Delete(TempToolsPath + "C2W\\c2p.img");
                File.Delete(TempToolsPath + "C2W\\c2w.img");
                File.Delete(TempToolsPath + "C2W\\starbuck_key.txt");
            }
            BuildProgress.Value = 50;
            //////////////////////////////////////////////

            //Generate app.xml & meta.xml
            BuildStatus.Text = "Generating app.xml and meta.xml";
            BuildStatus.Refresh();
            string[] AppXML = { "<?xml version=\"1.0\" encoding=\"utf-8\"?>", "<app type=\"complex\" access=\"777\">", "  <version type=\"unsignedInt\" length=\"4\">16</version>", "  <os_version type=\"hexBinary\" length=\"8\">000500101000400A</os_version>", "  <title_id type=\"hexBinary\" length=\"8\">" + PackedTitleIDLine.Text + "</title_id>", "  <title_version type=\"hexBinary\" length=\"2\">0000</title_version>", "  <sdk_version type=\"unsignedInt\" length=\"4\">21204</sdk_version>", "  <app_type type=\"hexBinary\" length=\"4\">8000002E</app_type>", "  <group_id type=\"hexBinary\" length=\"4\">" + TitleIDHex + "</group_id>", "  <os_mask type=\"hexBinary\" length=\"32\">0000000000000000000000000000000000000000000000000000000000000000</os_mask>", "  <common_id type=\"hexBinary\" length=\"8\">0000000000000000</common_id>", "</app>"};
            File.WriteAllLines(TempBuildPath + "code\\app.xml", AppXML);
            if (EnablePackedLine2.Checked)
            {
                string[] MetaXML = { "<?xml version=\"1.0\" encoding=\"utf-8\"?>", "<menu type=\"complex\" access=\"777\">", "  <version type=\"unsignedInt\" length=\"4\">33</version>", "  <product_code type=\"string\" length=\"32\">WUP-N-" + TitleIDText + "</product_code>", "  <content_platform type=\"string\" length=\"32\">WUP</content_platform>", "  <company_code type=\"string\" length=\"8\">0001</company_code>", "  <mastering_date type=\"string\" length=\"32\"></mastering_date>", "  <logo_type type=\"unsignedInt\" length=\"4\">0</logo_type>", "  <app_launch_type type=\"hexBinary\" length=\"4\">00000000</app_launch_type>", "  <invisible_flag type=\"hexBinary\" length=\"4\">00000000</invisible_flag>", "  <no_managed_flag type=\"hexBinary\" length=\"4\">00000000</no_managed_flag>", "  <no_event_log type=\"hexBinary\" length=\"4\">00000002</no_event_log>", "  <no_icon_database type=\"hexBinary\" length=\"4\">00000000</no_icon_database>", "  <launching_flag type=\"hexBinary\" length=\"4\">00000004</launching_flag>", "  <install_flag type=\"hexBinary\" length=\"4\">00000000</install_flag>", "  <closing_msg type=\"unsignedInt\" length=\"4\">0</closing_msg>", "  <title_version type=\"unsignedInt\" length=\"4\">0</title_version>", "  <title_id type=\"hexBinary\" length=\"8\">" + PackedTitleIDLine.Text + "</title_id>", "  <group_id type=\"hexBinary\" length=\"4\">" + TitleIDHex + "</group_id>", "  <boss_id type=\"hexBinary\" length=\"8\">0000000000000000</boss_id>", "  <os_version type=\"hexBinary\" length=\"8\">000500101000400A</os_version>", "  <app_size type=\"hexBinary\" length=\"8\">0000000000000000</app_size>", "  <common_save_size type=\"hexBinary\" length=\"8\">0000000000000000</common_save_size>", "  <account_save_size type=\"hexBinary\" length=\"8\">0000000000000000</account_save_size>", "  <common_boss_size type=\"hexBinary\" length=\"8\">0000000000000000</common_boss_size>", "  <account_boss_size type=\"hexBinary\" length=\"8\">0000000000000000</account_boss_size>", "  <save_no_rollback type=\"unsignedInt\" length=\"4\">0</save_no_rollback>", "  <join_game_id type=\"hexBinary\" length=\"4\">00000000</join_game_id>", "  <join_game_mode_mask type=\"hexBinary\" length=\"8\">0000000000000000</join_game_mode_mask>", "  <bg_daemon_enable type=\"unsignedInt\" length=\"4\">0</bg_daemon_enable>", "  <olv_accesskey type=\"unsignedInt\" length=\"4\">3921400692</olv_accesskey>", "  <wood_tin type=\"unsignedInt\" length=\"4\">0</wood_tin>", "  <e_manual type=\"unsignedInt\" length=\"4\">0</e_manual>", "  <e_manual_version type=\"unsignedInt\" length=\"4\">0</e_manual_version>", "  <region type=\"hexBinary\" length=\"4\">00000002</region>", "  <pc_cero type=\"unsignedInt\" length=\"4\">128</pc_cero>", "  <pc_esrb type=\"unsignedInt\" length=\"4\">6</pc_esrb>", "  <pc_bbfc type=\"unsignedInt\" length=\"4\">192</pc_bbfc>", "  <pc_usk type=\"unsignedInt\" length=\"4\">128</pc_usk>", "  <pc_pegi_gen type=\"unsignedInt\" length=\"4\">128</pc_pegi_gen>", "  <pc_pegi_fin type=\"unsignedInt\" length=\"4\">192</pc_pegi_fin>", "  <pc_pegi_prt type=\"unsignedInt\" length=\"4\">128</pc_pegi_prt>", "  <pc_pegi_bbfc type=\"unsignedInt\" length=\"4\">128</pc_pegi_bbfc>", "  <pc_cob type=\"unsignedInt\" length=\"4\">128</pc_cob>", "  <pc_grb type=\"unsignedInt\" length=\"4\">128</pc_grb>", "  <pc_cgsrr type=\"unsignedInt\" length=\"4\">128</pc_cgsrr>", "  <pc_oflc type=\"unsignedInt\" length=\"4\">128</pc_oflc>", "  <pc_reserved0 type=\"unsignedInt\" length=\"4\">192</pc_reserved0>", "  <pc_reserved1 type=\"unsignedInt\" length=\"4\">192</pc_reserved1>", "  <pc_reserved2 type=\"unsignedInt\" length=\"4\">192</pc_reserved2>", "  <pc_reserved3 type=\"unsignedInt\" length=\"4\">192</pc_reserved3>", "  <ext_dev_nunchaku type=\"unsignedInt\" length=\"4\">0</ext_dev_nunchaku>", "  <ext_dev_classic type=\"unsignedInt\" length=\"4\">0</ext_dev_classic>", "  <ext_dev_urcc type=\"unsignedInt\" length=\"4\">0</ext_dev_urcc>", "  <ext_dev_board type=\"unsignedInt\" length=\"4\">0</ext_dev_board>", "  <ext_dev_usb_keyboard type=\"unsignedInt\" length=\"4\">0</ext_dev_usb_keyboard>", "  <ext_dev_etc type=\"unsignedInt\" length=\"4\">0</ext_dev_etc>", "  <ext_dev_etc_name type=\"string\" length=\"512\"></ext_dev_etc_name>", "  <eula_version type=\"unsignedInt\" length=\"4\">0</eula_version>", "  <drc_use type=\"unsignedInt\" length=\"4\">" + DRCUSE + "</drc_use>", "  <network_use type=\"unsignedInt\" length=\"4\">0</network_use>", "  <online_account_use type=\"unsignedInt\" length=\"4\">0</online_account_use>", "  <direct_boot type=\"unsignedInt\" length=\"4\">0</direct_boot>", "  <reserved_flag0 type=\"hexBinary\" length=\"4\">00010001</reserved_flag0>", "  <reserved_flag1 type=\"hexBinary\" length=\"4\">00080023</reserved_flag1>", "  <reserved_flag2 type=\"hexBinary\" length=\"4\">" + TitleIDHex + "</reserved_flag2>", "  <reserved_flag3 type=\"hexBinary\" length=\"4\">00000000</reserved_flag3>", "  <reserved_flag4 type=\"hexBinary\" length=\"4\">00000000</reserved_flag4>", "  <reserved_flag5 type=\"hexBinary\" length=\"4\">00000000</reserved_flag5>", "  <reserved_flag6 type=\"hexBinary\" length=\"4\">00000003</reserved_flag6>", "  <reserved_flag7 type=\"hexBinary\" length=\"4\">00000005</reserved_flag7>", "  <longname_ja type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_ja>", "  <longname_en type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_en>", "  <longname_fr type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_fr>", "  <longname_de type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_de>", "  <longname_it type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_it>", "  <longname_es type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_es>", "  <longname_zhs type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_zhs>", "  <longname_ko type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_ko>", "  <longname_nl type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_nl>", "  <longname_pt type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_pt>", "  <longname_ru type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_ru>", "  <longname_zht type=\"string\" length=\"512\">" + PackedTitleLine1.Text, PackedTitleLine2.Text + "</longname_zht>", "  <shortname_ja type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_ja>", "  <shortname_en type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_en>", "  <shortname_fr type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_fr>", "  <shortname_de type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_de>", "  <shortname_it type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_it>", "  <shortname_es type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_es>", "  <shortname_zhs type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_zhs>", "  <shortname_ko type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_ko>", "  <shortname_nl type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_nl>", "  <shortname_pt type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_pt>", "  <shortname_ru type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_ru>", "  <shortname_zht type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_zht>", "  <publisher_ja type=\"string\" length=\"256\"></publisher_ja>", "  <publisher_en type=\"string\" length=\"256\"></publisher_en>", "  <publisher_fr type=\"string\" length=\"256\"></publisher_fr>", "  <publisher_de type=\"string\" length=\"256\"></publisher_de>", "  <publisher_it type=\"string\" length=\"256\"></publisher_it>", "  <publisher_es type=\"string\" length=\"256\"></publisher_es>", "  <publisher_zhs type=\"string\" length=\"256\"></publisher_zhs>", "  <publisher_ko type=\"string\" length=\"256\"></publisher_ko>", "  <publisher_nl type=\"string\" length=\"256\"></publisher_nl>", "  <publisher_pt type=\"string\" length=\"256\"></publisher_pt>", "  <publisher_ru type=\"string\" length=\"256\"></publisher_ru>", "  <publisher_zht type=\"string\" length=\"256\"></publisher_zht>", "  <add_on_unique_id0 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id0>", "  <add_on_unique_id1 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id1>", "  <add_on_unique_id2 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id2>", "  <add_on_unique_id3 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id3>", "  <add_on_unique_id4 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id4>", "  <add_on_unique_id5 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id5>", "  <add_on_unique_id6 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id6>", "  <add_on_unique_id7 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id7>", "  <add_on_unique_id8 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id8>", "  <add_on_unique_id9 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id9>", "  <add_on_unique_id10 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id10>", "  <add_on_unique_id11 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id11>", "  <add_on_unique_id12 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id12>", "  <add_on_unique_id13 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id13>", "  <add_on_unique_id14 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id14>", "  <add_on_unique_id15 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id15>", "  <add_on_unique_id16 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id16>", "  <add_on_unique_id17 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id17>", "  <add_on_unique_id18 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id18>", "  <add_on_unique_id19 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id19>", "  <add_on_unique_id20 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id20>", "  <add_on_unique_id21 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id21>", "  <add_on_unique_id22 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id22>", "  <add_on_unique_id23 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id23>", "  <add_on_unique_id24 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id24>", "  <add_on_unique_id25 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id25>", "  <add_on_unique_id26 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id26>", "  <add_on_unique_id27 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id27>", "  <add_on_unique_id28 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id28>", "  <add_on_unique_id29 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id29>", "  <add_on_unique_id30 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id30>", "  <add_on_unique_id31 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id31>", "</menu>" };
                File.WriteAllLines(TempBuildPath + "meta\\meta.xml", MetaXML);
            }
            else
            {
                string[] MetaXML = { "<?xml version=\"1.0\" encoding=\"utf-8\"?>", "<menu type=\"complex\" access=\"777\">", "  <version type=\"unsignedInt\" length=\"4\">33</version>", "  <product_code type=\"string\" length=\"32\">WUP-N-" + TitleIDText + "</product_code>", "  <content_platform type=\"string\" length=\"32\">WUP</content_platform>", "  <company_code type=\"string\" length=\"8\">0001</company_code>", "  <mastering_date type=\"string\" length=\"32\"></mastering_date>", "  <logo_type type=\"unsignedInt\" length=\"4\">0</logo_type>", "  <app_launch_type type=\"hexBinary\" length=\"4\">00000000</app_launch_type>", "  <invisible_flag type=\"hexBinary\" length=\"4\">00000000</invisible_flag>", "  <no_managed_flag type=\"hexBinary\" length=\"4\">00000000</no_managed_flag>", "  <no_event_log type=\"hexBinary\" length=\"4\">00000002</no_event_log>", "  <no_icon_database type=\"hexBinary\" length=\"4\">00000000</no_icon_database>", "  <launching_flag type=\"hexBinary\" length=\"4\">00000004</launching_flag>", "  <install_flag type=\"hexBinary\" length=\"4\">00000000</install_flag>", "  <closing_msg type=\"unsignedInt\" length=\"4\">0</closing_msg>", "  <title_version type=\"unsignedInt\" length=\"4\">0</title_version>", "  <title_id type=\"hexBinary\" length=\"8\">" + PackedTitleIDLine.Text + "</title_id>", "  <group_id type=\"hexBinary\" length=\"4\">" + TitleIDHex + "</group_id>", "  <boss_id type=\"hexBinary\" length=\"8\">0000000000000000</boss_id>", "  <os_version type=\"hexBinary\" length=\"8\">000500101000400A</os_version>", "  <app_size type=\"hexBinary\" length=\"8\">0000000000000000</app_size>", "  <common_save_size type=\"hexBinary\" length=\"8\">0000000000000000</common_save_size>", "  <account_save_size type=\"hexBinary\" length=\"8\">0000000000000000</account_save_size>", "  <common_boss_size type=\"hexBinary\" length=\"8\">0000000000000000</common_boss_size>", "  <account_boss_size type=\"hexBinary\" length=\"8\">0000000000000000</account_boss_size>", "  <save_no_rollback type=\"unsignedInt\" length=\"4\">0</save_no_rollback>", "  <join_game_id type=\"hexBinary\" length=\"4\">00000000</join_game_id>", "  <join_game_mode_mask type=\"hexBinary\" length=\"8\">0000000000000000</join_game_mode_mask>", "  <bg_daemon_enable type=\"unsignedInt\" length=\"4\">0</bg_daemon_enable>", "  <olv_accesskey type=\"unsignedInt\" length=\"4\">3921400692</olv_accesskey>", "  <wood_tin type=\"unsignedInt\" length=\"4\">0</wood_tin>", "  <e_manual type=\"unsignedInt\" length=\"4\">0</e_manual>", "  <e_manual_version type=\"unsignedInt\" length=\"4\">0</e_manual_version>", "  <region type=\"hexBinary\" length=\"4\">00000002</region>", "  <pc_cero type=\"unsignedInt\" length=\"4\">128</pc_cero>", "  <pc_esrb type=\"unsignedInt\" length=\"4\">6</pc_esrb>", "  <pc_bbfc type=\"unsignedInt\" length=\"4\">192</pc_bbfc>", "  <pc_usk type=\"unsignedInt\" length=\"4\">128</pc_usk>", "  <pc_pegi_gen type=\"unsignedInt\" length=\"4\">128</pc_pegi_gen>", "  <pc_pegi_fin type=\"unsignedInt\" length=\"4\">192</pc_pegi_fin>", "  <pc_pegi_prt type=\"unsignedInt\" length=\"4\">128</pc_pegi_prt>", "  <pc_pegi_bbfc type=\"unsignedInt\" length=\"4\">128</pc_pegi_bbfc>", "  <pc_cob type=\"unsignedInt\" length=\"4\">128</pc_cob>", "  <pc_grb type=\"unsignedInt\" length=\"4\">128</pc_grb>", "  <pc_cgsrr type=\"unsignedInt\" length=\"4\">128</pc_cgsrr>", "  <pc_oflc type=\"unsignedInt\" length=\"4\">128</pc_oflc>", "  <pc_reserved0 type=\"unsignedInt\" length=\"4\">192</pc_reserved0>", "  <pc_reserved1 type=\"unsignedInt\" length=\"4\">192</pc_reserved1>", "  <pc_reserved2 type=\"unsignedInt\" length=\"4\">192</pc_reserved2>", "  <pc_reserved3 type=\"unsignedInt\" length=\"4\">192</pc_reserved3>", "  <ext_dev_nunchaku type=\"unsignedInt\" length=\"4\">0</ext_dev_nunchaku>", "  <ext_dev_classic type=\"unsignedInt\" length=\"4\">0</ext_dev_classic>", "  <ext_dev_urcc type=\"unsignedInt\" length=\"4\">0</ext_dev_urcc>", "  <ext_dev_board type=\"unsignedInt\" length=\"4\">0</ext_dev_board>", "  <ext_dev_usb_keyboard type=\"unsignedInt\" length=\"4\">0</ext_dev_usb_keyboard>", "  <ext_dev_etc type=\"unsignedInt\" length=\"4\">0</ext_dev_etc>", "  <ext_dev_etc_name type=\"string\" length=\"512\"></ext_dev_etc_name>", "  <eula_version type=\"unsignedInt\" length=\"4\">0</eula_version>", "  <drc_use type=\"unsignedInt\" length=\"4\">" + DRCUSE + "</drc_use>", "  <network_use type=\"unsignedInt\" length=\"4\">0</network_use>", "  <online_account_use type=\"unsignedInt\" length=\"4\">0</online_account_use>", "  <direct_boot type=\"unsignedInt\" length=\"4\">0</direct_boot>", "  <reserved_flag0 type=\"hexBinary\" length=\"4\">00010001</reserved_flag0>", "  <reserved_flag1 type=\"hexBinary\" length=\"4\">00080023</reserved_flag1>", "  <reserved_flag2 type=\"hexBinary\" length=\"4\">" + TitleIDHex + "</reserved_flag2>", "  <reserved_flag3 type=\"hexBinary\" length=\"4\">00000000</reserved_flag3>", "  <reserved_flag4 type=\"hexBinary\" length=\"4\">00000000</reserved_flag4>", "  <reserved_flag5 type=\"hexBinary\" length=\"4\">00000000</reserved_flag5>", "  <reserved_flag6 type=\"hexBinary\" length=\"4\">00000003</reserved_flag6>", "  <reserved_flag7 type=\"hexBinary\" length=\"4\">00000005</reserved_flag7>", "  <longname_ja type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_ja>", "  <longname_en type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_en>", "  <longname_fr type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_fr>", "  <longname_de type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_de>", "  <longname_it type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_it>", "  <longname_es type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_es>", "  <longname_zhs type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_zhs>", "  <longname_ko type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_ko>", "  <longname_nl type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_nl>", "  <longname_pt type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_pt>", "  <longname_ru type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_ru>", "  <longname_zht type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</longname_zht>", "  <shortname_ja type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_ja>", "  <shortname_en type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_en>", "  <shortname_fr type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_fr>", "  <shortname_de type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_de>", "  <shortname_it type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_it>", "  <shortname_es type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_es>", "  <shortname_zhs type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_zhs>", "  <shortname_ko type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_ko>", "  <shortname_nl type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_nl>", "  <shortname_pt type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_pt>", "  <shortname_ru type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_ru>", "  <shortname_zht type=\"string\" length=\"512\">" + PackedTitleLine1.Text + "</shortname_zht>", "  <publisher_ja type=\"string\" length=\"256\"></publisher_ja>", "  <publisher_en type=\"string\" length=\"256\"></publisher_en>", "  <publisher_fr type=\"string\" length=\"256\"></publisher_fr>", "  <publisher_de type=\"string\" length=\"256\"></publisher_de>", "  <publisher_it type=\"string\" length=\"256\"></publisher_it>", "  <publisher_es type=\"string\" length=\"256\"></publisher_es>", "  <publisher_zhs type=\"string\" length=\"256\"></publisher_zhs>", "  <publisher_ko type=\"string\" length=\"256\"></publisher_ko>", "  <publisher_nl type=\"string\" length=\"256\"></publisher_nl>", "  <publisher_pt type=\"string\" length=\"256\"></publisher_pt>", "  <publisher_ru type=\"string\" length=\"256\"></publisher_ru>", "  <publisher_zht type=\"string\" length=\"256\"></publisher_zht>", "  <add_on_unique_id0 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id0>", "  <add_on_unique_id1 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id1>", "  <add_on_unique_id2 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id2>", "  <add_on_unique_id3 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id3>", "  <add_on_unique_id4 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id4>", "  <add_on_unique_id5 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id5>", "  <add_on_unique_id6 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id6>", "  <add_on_unique_id7 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id7>", "  <add_on_unique_id8 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id8>", "  <add_on_unique_id9 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id9>", "  <add_on_unique_id10 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id10>", "  <add_on_unique_id11 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id11>", "  <add_on_unique_id12 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id12>", "  <add_on_unique_id13 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id13>", "  <add_on_unique_id14 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id14>", "  <add_on_unique_id15 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id15>", "  <add_on_unique_id16 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id16>", "  <add_on_unique_id17 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id17>", "  <add_on_unique_id18 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id18>", "  <add_on_unique_id19 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id19>", "  <add_on_unique_id20 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id20>", "  <add_on_unique_id21 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id21>", "  <add_on_unique_id22 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id22>", "  <add_on_unique_id23 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id23>", "  <add_on_unique_id24 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id24>", "  <add_on_unique_id25 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id25>", "  <add_on_unique_id26 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id26>", "  <add_on_unique_id27 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id27>", "  <add_on_unique_id28 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id28>", "  <add_on_unique_id29 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id29>", "  <add_on_unique_id30 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id30>", "  <add_on_unique_id31 type=\"hexBinary\" length=\"4\">00000000</add_on_unique_id31>", "</menu>" };
                File.WriteAllLines(TempBuildPath + "meta\\meta.xml", MetaXML);
            }
            BuildProgress.Value = 52;
            /////////////////////////////

            //Convert PNG files to TGA
            BuildStatus.Text = "Converting all image sources to expected TGA specification...";
            BuildStatus.Refresh();
            LauncherExeFile = TempToolsPath + "IMG\\png2tgacmd.exe";
            LauncherExeArgs = "-i \"" + TempIconPath + "\" -o \"" + TempBuildPath + "meta\" --width=128 --height=128 --tga-bpp=32 --tga-compression=none";
            LaunchProgram();
            LauncherExeFile = TempToolsPath + "IMG\\png2tgacmd.exe";
            LauncherExeArgs = "-i \"" + TempBannerPath + "\" -o \"" + TempBuildPath + "meta\" --width=1280 --height=720 --tga-bpp=24 --tga-compression=none";
            LaunchProgram();
            if (FlagDrcSpecified == false)
            {
                File.Copy(TempBannerPath, TempDrcPath);
            }
            LauncherExeFile = TempToolsPath + "IMG\\png2tgacmd.exe";
            LauncherExeArgs = "-i \"" + TempDrcPath + "\" -o \"" + TempBuildPath + "meta\" --width=854 --height=480 --tga-bpp=24 --tga-compression=none";
            LaunchProgram();
            if (FlagLogoSpecified)
            {
                LauncherExeFile = TempToolsPath + "IMG\\png2tgacmd.exe";
                LauncherExeArgs = "-i \"" + TempLogoPath + "\" -o \"" + TempBuildPath + "meta\" --width=170 --height=42 --tga-bpp=32 --tga-compression=none";
                LaunchProgram();
            }
            if (FlagDrcSpecified == false) { File.Delete(TempDrcPath); }
            BuildProgress.Value = 55;
            //////////////////////////

            //Convert Boot Sound if provided by user
            if (FlagBootSoundSpecified)
            {
                BuildStatus.Text = "Converting user provided sound to btsnd format...";
                BuildStatus.Refresh();
                LauncherExeFile = TempToolsPath + "SOX\\sox.exe";
                LauncherExeArgs = "\"" + OpenBootSound.FileName + "\" -b 16 \"" + TempSoundPath + "\" channels 2 rate 48k trim 0 6";
                LaunchProgram();
                File.Delete(TempBuildPath + "meta\\bootSound.btsnd");
                LauncherExeFile = TempToolsPath + "JAR\\wav2btsnd.exe";
                LauncherExeArgs = "-in \"" + TempSoundPath + "\" -out \"" + TempBuildPath + "meta\\bootSound.btsnd\"" + LoopString;
                LaunchProgram();
                File.Delete(TempSoundPath);
            }
            BuildProgress.Value = 60;
            ////////////////////////////////////////

            //Build ISO based on type and user specification
            BuildStatus.Text = "Processing game for NFS Conversion...";
            BuildStatus.Refresh();
            if (OpenGame.FileName != null) { OGfilepath = OpenGame.FileName; }
            if (SystemType == "wii")
            {
                if (FlagWBFS)
                {
                    LauncherExeFile = TempToolsPath + "EXE\\wbfs_file.exe";
                    LauncherExeArgs = "\"" + OpenGame.FileName + "\" convert \"" + TempSourcePath + "wbfsconvert.iso\"";
                    LaunchProgram();
                    OpenGame.FileName = TempSourcePath + "wbfsconvert.iso";
                }
                if (DisableTrimming.Checked == false)
                {
                    LauncherExeFile = TempToolsPath + "WIT\\wit.exe";
                    LauncherExeArgs = "extract " + ShortenPath(OpenGame.FileName) + " --DEST " + ShortenPath(TempSourcePath + "ISOEXTRACT") + " --psel data -vv1";
                    LaunchProgram();
                    MessageBox.Show("");
                    if (ForceCC.Checked)
                    {
                        LauncherExeFile = TempToolsPath + "EXE\\GetExtTypePatcher.exe";
                        LauncherExeArgs = "\"" + TempSourcePath + "ISOEXTRACT\\sys\\main.dol\" -nc";
                        LaunchProgram();
                    }
                    if (WiiVMC.Checked)
                    {
                        MessageBox.Show("The Wii Video Mode Changer will now be launched. I recommend using the Smart Patcher option. \n\nIf you're scared and don't know what you're doing, close the patcher window and nothing will be patched. \n\nClick OK to continue...");
                        HideProcess = false;
                        LauncherExeFile = TempToolsPath + "EXE\\wii-vmc.exe";
                        LauncherExeArgs = "\"" + TempSourcePath + "ISOEXTRACT\\sys\\main.dol\"";
                        LaunchProgram();
                        HideProcess = true;
                        MessageBox.Show("Conversion will now continue...");
                    }
                    LauncherExeFile = TempToolsPath + "WIT\\wit.exe";
                    LauncherExeArgs = "copy " + ShortenPath(TempSourcePath + "ISOEXTRACT") + " --DEST " + ShortenPath(TempSourcePath + "game.iso") + " -ovv --links --iso";
                    LaunchProgram();
                    Directory.Delete(TempSourcePath + "ISOEXTRACT", true);
                    if (File.Exists(TempSourcePath + "wbfsconvert.iso")) { File.Delete(TempSourcePath + "wbfsconvert.iso"); }
                    OpenGame.FileName = TempSourcePath + "game.iso";
                }
            }
            if (SystemType == "dol")
            {
                FileSystem.CreateDirectory(TempSourcePath + "TEMPISOBASE");
                FileSystem.CopyDirectory(TempToolsPath + "BASE", TempSourcePath + "TEMPISOBASE");
                File.Copy(OpenGame.FileName, TempSourcePath + "TEMPISOBASE\\sys\\main.dol");
                LauncherExeFile = TempToolsPath + "WIT\\wit.exe";
                LauncherExeArgs = "copy " + ShortenPath(TempSourcePath + "TEMPISOBASE") + " --DEST " + ShortenPath(TempSourcePath + "game.iso") + " -ovv --links --iso";
                LaunchProgram();
                Directory.Delete(TempSourcePath + "TEMPISOBASE", true);
                OpenGame.FileName = TempSourcePath + "game.iso";
            }
            if (SystemType == "wiiware")
            {
                FileSystem.CreateDirectory(TempSourcePath + "TEMPISOBASE");
                FileSystem.CopyDirectory(TempToolsPath + "BASE", TempSourcePath + "TEMPISOBASE");
                if (Force43NAND.Checked)
                {
                    File.Copy(TempToolsPath + "DOL\\FIX94_wiivc_chan_booter_force43.dol", TempSourcePath + "TEMPISOBASE\\sys\\main.dol");
                }
                else
                {
                    File.Copy(TempToolsPath + "DOL\\FIX94_wiivc_chan_booter.dol", TempSourcePath + "TEMPISOBASE\\sys\\main.dol");
                }
                string[] TitleTXT = { GameSourceDirectory.Text };
                File.WriteAllLines(TempSourcePath + "TEMPISOBASE\\files\\title.txt", TitleTXT);
                LauncherExeFile = TempToolsPath + "WIT\\wit.exe";
                LauncherExeArgs = "copy " + ShortenPath(TempSourcePath + "TEMPISOBASE") + " --DEST " + ShortenPath(TempSourcePath + "game.iso") + " -ovv --links --iso";
                LaunchProgram();
                Directory.Delete(TempSourcePath + "TEMPISOBASE", true);
                OpenGame.FileName = TempSourcePath + "game.iso";
            }
            if (SystemType == "gcn")
            {
                FileSystem.CreateDirectory(TempSourcePath + "TEMPISOBASE");
                FileSystem.CopyDirectory(TempToolsPath + "BASE", TempSourcePath + "TEMPISOBASE");
                if (Force43NINTENDONT.Checked)
                {
                    File.Copy(TempToolsPath + "DOL\\FIX94_nintendont_force43_autoboot.dol", TempSourcePath + "TEMPISOBASE\\sys\\main.dol");
                }
                else if (CustomMainDol.Checked)
                {
                    File.Copy(OpenMainDol.FileName, TempSourcePath + "TEMPISOBASE\\sys\\main.dol");
                }
                else if (DisableNintendontAutoboot.Checked)
                {
                    File.Copy(TempToolsPath + "DOL\\FIX94_nintendont_forwarder.dol", TempSourcePath + "TEMPISOBASE\\sys\\main.dol");
                }
                else
                {
                    File.Copy(TempToolsPath + "DOL\\FIX94_nintendont_default_autoboot.dol", TempSourcePath + "TEMPISOBASE\\sys\\main.dol");
                }
                File.Copy(OpenGame.FileName, TempSourcePath + "TEMPISOBASE\\files\\game.iso");
                if (FlagGC2Specified) { File.Copy(OpenGC2.FileName, TempSourcePath + "TEMPISOBASE\\files\\disc2.iso"); }
                LauncherExeFile = TempToolsPath + "WIT\\wit.exe";
                LauncherExeArgs = "copy " + ShortenPath(TempSourcePath + "TEMPISOBASE") + " --DEST " + ShortenPath(TempSourcePath + "game.iso") + " -ovv --links --iso";
                MessageBox.Show(LauncherExeArgs);
                LaunchProgram();
                Directory.Delete(TempSourcePath + "TEMPISOBASE", true);
                OpenGame.FileName = TempSourcePath + "game.iso";
            }
            LauncherExeFile = TempToolsPath + "WIT\\wit.exe";
            LauncherExeArgs = "extract " + ShortenPath(OpenGame.FileName) + " --psel data --files +tmd.bin --files +ticket.bin --dest " + ShortenPath(TempSourcePath + "TIKTEMP") + " -vv1";
            LaunchProgram();
            File.Copy(TempSourcePath + "TIKTEMP\\tmd.bin", TempBuildPath + "code\\rvlt.tmd");
            File.Copy(TempSourcePath + "TIKTEMP\\ticket.bin", TempBuildPath + "code\\rvlt.tik");
            Directory.Delete(TempSourcePath + "TIKTEMP", true);
            BuildProgress.Value = 70;
            ////////////////////////////////////////////////

            //Convert ISO to NFS format
            BuildStatus.Text = "Converting processed game to NFS format...";
            Directory.SetCurrentDirectory(TempBuildPath + "content");
            BuildStatus.Refresh();
            string lrpatchflag = "";
            if (LRPatch.Checked)
            {
                lrpatchflag = " -lrpatch";
            }
            if (SystemType == "wii")
            {
                LauncherExeFile = TempToolsPath + "EXE\\nfs2iso2nfs.exe";
                LauncherExeArgs = "-enc" + nfspatchflag + lrpatchflag + " -iso \"" + OpenGame.FileName + "\"";
                LaunchProgram();
            }
            if (SystemType == "dol")
            {
                LauncherExeFile = TempToolsPath + "EXE\\nfs2iso2nfs.exe";
                LauncherExeArgs = "-enc -homebrew" + passpatch + " -iso \"" + OpenGame.FileName + "\"";
                LaunchProgram();
            }
            if (SystemType == "wiiware")
            {
                LauncherExeFile = TempToolsPath + "EXE\\nfs2iso2nfs.exe";
                LauncherExeArgs = "-enc -homebrew" + nfspatchflag + lrpatchflag + " -iso \"" + OpenGame.FileName + "\"";
                LaunchProgram();
            }
            if (SystemType == "gcn")
            {
                LauncherExeFile = TempToolsPath + "EXE\\nfs2iso2nfs.exe";
                LauncherExeArgs = "-enc -homebrew -passthrough -iso \"" + OpenGame.FileName + "\"";
                LaunchProgram();
            }
            if (DisableTrimming.Checked == false) { File.Delete(OpenGame.FileName); } else if (FlagWBFS) { File.Delete(OpenGame.FileName); }
            BuildProgress.Value = 85;
            ///////////////////////////

            //Encrypt contents with NUSPacker
            BuildStatus.Text = "Encrypting contents into installable WUP Package...";
            BuildStatus.Refresh();
            Directory.SetCurrentDirectory(TempRootPath);
            LauncherExeFile = TempToolsPath + "JAR\\NUSPacker.exe";
            LauncherExeArgs = "-in BUILDDIR -out \"" + OutputFolderSelect.SelectedPath + "\\WUP-N-" + TitleIDText + "_" + PackedTitleIDLine.Text + "\" -encryptKeyWith " + WiiUCommonKey.Text;
            LaunchProgram();
            BuildProgress.Value = 100;
            /////////////////////////////////

            //Delete Temp Directories
            Directory.SetCurrentDirectory(Application.StartupPath);
            Directory.Delete(TempBuildPath, true);
            Directory.Delete(TempRootPath + "output", true);
            Directory.Delete(TempRootPath + "tmp", true);
            Directory.CreateDirectory(TempBuildPath);
            /////////////////////////

            //END
            BuildStatus.Text = "Conversion complete...";
            BuildStatus.Refresh();
            MessageBox.Show("Conversion Complete! Your packed game can be found here: " + OutputFolderSelect.SelectedPath + "\\WUP-N-" + TitleIDText + "_" + PackedTitleIDLine.Text + ".\n\nInstall your title using WUP Installer GX2 with signature patches enabled (CBHC, Haxchi, etc). Make sure you have signature patches enabled when launching your title.\n\n Click OK to continue...", PackedTitleLine1.Text + " Conversion Complete");
            if (OGfilepath != null) { OpenGame.FileName = OGfilepath; }
            BuildStatus.Text = "";
            MainTabs.Enabled = true;
            MainTabs.SelectedTab = SourceFilesTab;
            BuildProcessFin:;
            /////
        }

        // Modified from MSDN: https://msdn.microsoft.com/en-us/library/bb986765.aspx
        private Font GetGraphicAdjustedFont(
            Graphics g, 
            string graphicString, 
            Font originalFont, 
            int containerWidth,
            int containerHeight,
            int maxFontSize, 
            int minFontSize, 
            StringFormat stringFormat,
            bool smallestOnFail
            )
        {
            Font testFont = null;
            // We utilize MeasureString which we get via a control instance           
            for (int adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize--)
            {
                testFont = new Font(originalFont.Name, adjustedSize, originalFont.Style);

                // Test the string with the new size
                SizeF adjustedSizeNew = g.MeasureString(
                    graphicString, 
                    testFont, 
                    containerWidth,
                    stringFormat);

                if (containerWidth  > Convert.ToInt32(adjustedSizeNew.Width) &&
                    containerHeight > Convert.ToInt32(adjustedSizeNew.Height))
                {
                    // Good font, return it
                    return testFont;
                }
            }

            // If you get here there was no fontsize that worked
            // return minimumSize or original?
            if (smallestOnFail)
            {
                return testFont;
            }
            else
            {
                return originalFont;
            }
        }

        private Font GetTextRendererAdjustedFont(
            Graphics g,
            string text,
            Font originalFont,
            int containerWidth,
            int containerHeight,
            int maxFontSize,
            int minFontSize,
            TextFormatFlags flags,
            bool smallestOnFail
            )
        {
            Font testFont = null;
            // We utilize MeasureString which we get via a control instance           
            for (int adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize--)
            {
                testFont = new Font(originalFont.Name, adjustedSize, originalFont.Style);

                // Test the string with the new size
                Size adjustedSizeNew = TextRenderer.MeasureText(
                    g,
                    text,
                    testFont,
                    new Size(containerWidth, containerHeight),
                    flags);

                if (containerWidth > adjustedSizeNew.Width &&
                    containerHeight > adjustedSizeNew.Height)
                {
                    // Good font, return it
                    return testFont;
                }
            }

            // If you get here there was no fontsize that worked
            // return minimumSize or original?
            if (smallestOnFail)
            {
                return testFont;
            }
            else
            {
                return originalFont;
            }
        }

        private void ImageDrawString(
            ref Bitmap bitmap,
            string s,
            Rectangle rectangle,
            Font font,
            bool adjustedFontByTextRenderer,
            bool drawStringByTextRenderer
            )
        {
            StringFormat stringFormat = StringFormat.GenericDefault;

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter
                    | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.WordBreak;

                if (!adjustedFontByTextRenderer)
                {
                    font = GetGraphicAdjustedFont(
                        graphics,
                        s,
                        font,
                        rectangle.Width,
                        rectangle.Height,
                        100, 8,
                        stringFormat,
                        true);
                }
                else
                {
                    // Can't get the correct word break output
                    // if we use GetGraphicAdjustedFont.
                    // But it's really more slower than 
                    // GetGraphicAdjustedFont.
                    font = GetTextRendererAdjustedFont(
                        graphics,
                        GameNameLabel.Text,
                        font,
                        rectangle.Width,
                        rectangle.Height,
                        64, 8,
                        flags,
                        true);
                }

                if (!drawStringByTextRenderer)
                {
                    SizeF sizeF = graphics.MeasureString(s, font, rectangle.Width);

                    RectangleF rectF = new RectangleF(
                        rectangle.X + (rectangle.Width - sizeF.Width) / 2,
                        rectangle.Y + (rectangle.Height - sizeF.Height) / 2,
                        sizeF.Width,
                        sizeF.Height);

                    graphics.DrawString(
                        s,
                        font,
                        Brushes.Black,
                        rectF,
                        stringFormat);
                }
                else
                {
                    // Poor draw performance, both for speed and output result.
                    Size size = TextRenderer.MeasureText(
                        graphics,
                        s,
                        font,
                        new Size(rectangle.Width, rectangle.Height),
                        flags);

                    TextRenderer.DrawText(
                        graphics,
                        GameNameLabel.Text,
                        font,
                        new Rectangle(
                            rectangle.X + (rectangle.Width - size.Width) / 2,
                            rectangle.Y + (rectangle.Height - size.Height) / 2,
                            size.Width,
                            size.Height),
                        Color.Black,
                        flags);
                }
            }
        }

        struct WiiVcGenerateImage
        {
            public Bitmap bitmap;
            public Rectangle rectangle;
            public string s;
            public string savePath;
            public string dirControlName;
            public string previewControlName;
            public bool adjustedFontByTextRenderer;
            public bool drawStringByTextRenderer;
        };

        private void GenerateImage_Click(object sender, EventArgs e)
        {
            GameNameLabel.Text = "Another R";

            Font arialFont = new Font("Arial", 10);

            Bitmap bitmapGamePadBar = new Bitmap(854, 480);
            using (Graphics graphics = Graphics.FromImage(bitmapGamePadBar))
            {
                graphics.DrawImage(
                    Properties.Resources.universal_Wii_WiiWare_template_bootTvTex,
                    new Rectangle(0, 0, bitmapGamePadBar.Width, bitmapGamePadBar.Height),
                    new Rectangle(
                        0, 0,
                        Properties.Resources.universal_Wii_WiiWare_template_bootTvTex.Width,
                        Properties.Resources.universal_Wii_WiiWare_template_bootTvTex.Height),
                    GraphicsUnit.Pixel);
            }

            Bitmap bitmapBootLogo = new Bitmap(170, 42);
            using (Graphics graphics = Graphics.FromImage(bitmapBootLogo))
            {
                graphics.FillRectangle(
                    Brushes.White, 
                    new Rectangle(0, 0, bitmapBootLogo.Width, bitmapBootLogo.Height));
            }

            string saveDir = Path.GetTempPath() + "WiiVCInjector\\SOURCETEMP\\";

            WiiVcGenerateImage[] images = new WiiVcGenerateImage[]
            {
                new WiiVcGenerateImage {
                    bitmap = Properties.Resources.universal_Wii_WiiWare_template_iconTex,
                    rectangle = new Rectangle(0, 23, 128, 94),
                    s = GameNameLabel.Text,
                    savePath = saveDir + "iconTex.png",
                    dirControlName = "IconSourceDirectory",
                    previewControlName = "IconPreviewBox",
                    adjustedFontByTextRenderer = true,
                    drawStringByTextRenderer = false,
                },
                new WiiVcGenerateImage {
                    bitmap = Properties.Resources.universal_Wii_WiiWare_template_bootTvTex,
                    rectangle = new Rectangle(224, 210, 820, 320),
                    s = GameNameLabel.Text,
                    savePath = saveDir + "bootTvTex.png",
                    dirControlName = "BannerSourceDirectory",
                    previewControlName = "BannerPreviewBox",
                    adjustedFontByTextRenderer = false,
                    drawStringByTextRenderer = false,
                },
                new WiiVcGenerateImage {
                    bitmap = bitmapGamePadBar,
                    rectangle = new Rectangle(148, 138, 556, 212),
                    s = GameNameLabel.Text,
                    savePath = saveDir + "bootDrcTex.png",
                    dirControlName = "DrcSourceDirectory",
                    previewControlName = "DrcPreviewBox",
                    adjustedFontByTextRenderer = false,
                    drawStringByTextRenderer = false,
                },
                new WiiVcGenerateImage {
                    bitmap = bitmapBootLogo,
                    rectangle = new Rectangle(0, 0, 170, 42),
                    s = "WiiWare",
                    savePath = saveDir + "bootLogoTex.png",
                    dirControlName = "LogoSourceDirectory",
                    previewControlName = "LogoPreviewBox",
                    adjustedFontByTextRenderer = true,
                    drawStringByTextRenderer = false,
                },
            };

            for (int i = 0; i < images.Length; ++i)
            {
                ImageDrawString(
                    ref images[i].bitmap,
                    images[i].s,
                    images[i].rectangle,
                    arialFont,
                    images[i].adjustedFontByTextRenderer,
                    images[i].drawStringByTextRenderer);
                images[i].bitmap.Save(images[i].savePath);

                FileStream tempstream = new FileStream(images[i].savePath, FileMode.Open);
                var tempimage = Image.FromStream(tempstream);
                PictureBox previewBox = this.Controls.Find(images[i].previewControlName, true).FirstOrDefault() as PictureBox;
                previewBox.Image = tempimage;
                tempstream.Close();
                Label sourceDirectory = this.Controls.Find(images[i].dirControlName, true).FirstOrDefault() as Label;
                sourceDirectory.Text = "Auto generated.";
                sourceDirectory.ForeColor = Color.Green;
            }

            FlagIconSpecified = true;
            FlagBannerSpecified = true;
            FlagDrcSpecified = true;
            FlagLogoSpecified = true;
            FlagRepo = false;
        }
    }
}
