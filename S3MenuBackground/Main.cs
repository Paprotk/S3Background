﻿using System;
using System.Collections.Generic;
using Sims3.Gameplay;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.GameEntry;
using OneShotFunctionTask = Sims3.Gameplay.OneShotFunctionTask;

namespace S3MenuBackground
{
    public static class Main
    {
        [Tunable]
#pragma warning disable CS0169 // Field is never used
        private static bool kInstantiator;
#pragma warning restore CS0169 // Field is never used
        [Tunable] public static bool bRespectTimeOfDay;
        public static bool bShownDialog;
        [Tunable] public static bool bShowMods;
        public static bool bShouldRemoveEffect;
        private static readonly Random random = new Random();
        [Tunable] public static int dayHour;
        [Tunable] public static int nightHour;
        [Tunable] public static bool bDebugging;
        [Tunable] public static bool bShowInvalidDialog;
        public static readonly List<string> dayFileList = new List<string>();
        public static readonly List<string> nightFileList = new List<string>();
        public static readonly List<string> removedDay = new List<string>();
        public static readonly List<string> removedNight = new List<string>();

    static Main()
        {
            World.sOnStartupAppEventHandler += OnStartupApp;
            World.sOnEnterNotInWorldEventHandler += HandleNotInWorldEvent;
            World.sOnLeaveNotInWorldEventHandler += HandleNotInWorldEvent;
        }
    
        internal static void ParseCustomMainMenuImageData()
        {
            XmlDbData xmlDbData = XmlDbData.ReadData("CustomMainMenuImages");
            if (xmlDbData == null)
            {
                return;
            }

            xmlDbData.Tables.TryGetValue("CustomImage", out var xmlDbTable);
            if (xmlDbTable != null)
            {
                foreach (XmlDbRow xmlDbRow in xmlDbTable.Rows)
                {
                    string dayImagName = xmlDbRow.GetString("dayImagName");
                    string nightImagName = xmlDbRow.GetString("nightImagName");
                    dayFileList.Add(dayImagName);
                    nightFileList.Add(nightImagName);
                }
            }
            ValidateImages();
        }
        public static void ValidateImages()
        {
            // Validate day images
            for (int i = dayFileList.Count - 1; i >= 0; i--)
            {
                string imageName = dayFileList[i];
                if (!IsValidImage(imageName))
                {
                    removedDay.Add(imageName); // Add to removed list
                    dayFileList.RemoveAt(i);
                }
            }

            // Validate night images
            for (int i = nightFileList.Count - 1; i >= 0; i--)
            {
                string imageName = nightFileList[i];
                if (!IsValidImage(imageName))
                {
                    removedNight.Add(imageName); // Add to removed list
                    nightFileList.RemoveAt(i);
                }
            }
        }

        private static bool IsValidImage(string imageName)
        {
            try
            {
                var image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(imageName, 0U));
                return image != null;
            }
            catch
            {
                return false;
            }
        }
        
        private static void OnStartupApp(object sender, EventArgs e)
        {
            ParseCustomMainMenuImageData();
            if (bDebugging)
            {
                Commands.sGameCommands.Register("cb", "Change background image to random.", Commands.CommandType.General, (Cheats.ChangeBackground));
                Commands.sGameCommands.Register("pl", "Print all loaded images.", Commands.CommandType.General, (Cheats.PrintList));
                Commands.sGameCommands.Register("fb", "Force background to specified one.", Commands.CommandType.General, (Cheats.ForceBackgroundHandler));
            }
            Commands.sGameCommands.Register("mods", "Shows the mods list.", Commands.CommandType.General, (Cheats.ModCheckHandler));
        }

        private static void HandleNotInWorldEvent(object sender, EventArgs e)
        {
            bShouldRemoveEffect = true;
            Simulator.AddObject(new OneShotFunctionTask(Run, StopWatch.TickStyles.Seconds, 0.1f));
            if (bShowInvalidDialog && (removedDay.Count > 0 || removedNight.Count > 0))
            {
                string content = "";

                if (removedDay.Count > 0)
                {
                    content += "Removed Day images:\n" + string.Join(", ", removedDay.ToArray()) + "\n\n";
                }

                if (removedNight.Count > 0)
                {
                    content += "Removed Night images:\n" + string.Join(", ", removedNight.ToArray()) + "\n\n";
                }

                SimpleMessageDialog.Show("Removed Images", content);
                removedDay.Clear();
                removedNight.Clear();
            }
        }
        
        private static string lastSelection = string.Empty;
        public static string GetRandom()
        {
            DateTime currentTime = DateTime.Now;
            var timeOfDay = (currentTime.Hour >= dayHour && currentTime.Hour < nightHour) ? "day" : "night";
    
            // Determine whether we should respect the time of day
            if (bRespectTimeOfDay)
            {
                // Randomize from the appropriate list based on time of day
                if (timeOfDay == "day")
                {
                    // Pick a random image from the dayFileList
                    string randomChoice;
                    do
                    {
                        randomChoice = dayFileList[random.Next(dayFileList.Count)];
                    } while (randomChoice == lastSelection); // Ensure it's not the same as the last one
                    lastSelection = randomChoice; // Update the last selection
                    return randomChoice;
                }
                else if (timeOfDay == "night")
                {
                    // Pick a random image from the nightFileList
                    string randomChoice;
                    do
                    {
                        randomChoice = nightFileList[random.Next(nightFileList.Count)];
                    } while (randomChoice == lastSelection); // Ensure it's not the same as the last one
                    lastSelection = randomChoice; // Update the last selection
                    return randomChoice;
                }
            }
            else
            {
                List<string> combinedList = new List<string>(dayFileList);
                combinedList.AddRange(nightFileList);
                string randomChoice;
                do
                {
                    randomChoice = combinedList[random.Next(combinedList.Count)];
                } while (randomChoice == lastSelection); // Ensure it's not the same as the last one
                lastSelection = randomChoice; // Update the last selection
                return randomChoice;
            }
            return string.Empty; // Return empty if something goes wrong
        }
        public static void Run()
        {
            if (GameStates.IsInMainMenuState)
            {
                WindowBase windowBase = UIManager.GetWindowFromPoint(UIManager.GetCursorPosition());
                MainMenu mainMenu = null;

                // Traverse up the window hierarchy to find the MainMenu
                while (windowBase != null)
                {
                    mainMenu = windowBase as MainMenu;
                    if (mainMenu != null)
                    {
                        break; // Exit loop if MainMenu is found
                    }
                    windowBase = UIManager.GetParentWindow(windowBase); // Move to the parent window
                }
                
                if (mainMenu != null)
                {
                    ImageDrawable background = mainMenu.Drawable as ImageDrawable;
                    string imageName = GetRandom();
                    background.Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(imageName, 0U));
                    
                    
                    if (bShouldRemoveEffect) //<3 u Eca
                    {
                        Window effect1 = mainMenu.GetChildByIndex(4) as Window;
                        Window effect2 = mainMenu.GetChildByIndex(2) as Window;
                        mainMenu.DestroyChild(effect1);
                        mainMenu.DestroyChild(effect2);
                    }
                    bShouldRemoveEffect = false;
                    mainMenu.Invalidate();
                    
                    if (!bShownDialog && bShowMods)
                    {
                        Simulator.AddObject(new OneShotFunctionTask(Main.ModCheck, StopWatch.TickStyles.Seconds, 0.1f));
                    }
                }
            }
        }
        public static void ModCheck()
        {
            bShownDialog = true;
            List<string> list = new List<string>();
            uint modFilesCount = GameUtils.GetModFilesCount();
            for (uint num = 0U; num < modFilesCount; num += 1U)
            {
                string modFilesName = GameUtils.GetModFilesName(num);
                if (!string.IsNullOrEmpty(modFilesName))
                {
                    list.Add(modFilesName);
                }
            }
            if (list.Count > 0)
            {
                ModInfoDialogCustom.Show(list.ToArray());
            }
        }
    }
}