using System;
using System.Collections.Generic;
using Sims3.Gameplay;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.GameEntry;
using Function = Sims3.Gameplay.Function;
using OneShotFunctionTask = Sims3.Gameplay.OneShotFunctionTask;

namespace S3MenuBackground
{
    public static class Main
    {
        [Tunable]
#pragma warning disable CS0169 // Field is never used
        private static bool kInstantiator;
#pragma warning restore CS0169 // Field is never used
        public static string timeOfDay;
        public static char randomLetter;
        [Tunable]
        // ReSharper disable once UnassignedField.Global
        public static bool bRespectTimeOfDay;
        public static bool bShownDialog;
        [Tunable]
        // ReSharper disable once UnassignedField.Global
        public static bool bShowMods;

        public static bool bShouldRemoveEffect;
        static Main()
        {
            World.sOnStartupAppEventHandler += OnStartupApp;
            World.sOnEnterNotInWorldEventHandler += OnEnterNotInWorldEventHandler;
            World.sOnLeaveNotInWorldEventHandler += OnLeaveNotInWorld;
        }

        private static void OnStartupApp(object sender, EventArgs e)
        {
            //Only in debug
            Commands.sGameCommands.Register("cb", "Change Background.", Commands.CommandType.General, (Main.Cheat));
        }

        public static void GetRandom()
        {
            DateTime currentTime = DateTime.Now;
            if (bRespectTimeOfDay)
            {
                timeOfDay = GetDayOrNight(currentTime);
            }
            if (!bRespectTimeOfDay)
            {
                timeOfDay = GetZeroOrOne().ToString();
            }
            RandomizeLetter();
        }
        private static void OnEnterNotInWorldEventHandler(object sender, EventArgs e)
        {
            bShouldRemoveEffect = true;
            GetRandom();
            Simulator.AddObject(new OneShotFunctionTask(Main.Run, StopWatch.TickStyles.Seconds, 0.1f));
        }
        private static void OnLeaveNotInWorld(object sender, EventArgs e)
        {
            bShouldRemoveEffect = true;
            GetRandom();
            Simulator.AddObject(new OneShotFunctionTask(Main.Run, StopWatch.TickStyles.Seconds, 0.1f));
        }
        
        public static int Cheat(object[] parameters)
        {
            GetRandom();
            Run();
            return 1;
        }
        static string GetDayOrNight(DateTime time)
        {
            // Define day as between 6:00 AM and 6:00 PM
            if (time.Hour >= 6 && time.Hour < 21)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }
        private static void Run()
        {
            if (GameStates.IsInMainMenuState)
            {
                WindowBase windowBase = UIManager.GetWindowFromPoint(UIManager.GetCursorPosition());
                MainMenu mainMenu;

                for (;;)
                {
                    mainMenu = (windowBase as MainMenu); // Attempt to cast WindowBase to MainMenu
                    if (mainMenu != null) // If the cast is successful, exit the loop
                    {
                        break;
                    }

                    windowBase = UIManager.GetParentWindow(windowBase); // Move to the parent window
                    if (!(windowBase != null)) // If no parent window exists, exit the loop
                    {
                        goto Block_9; // Fallback label
                    }
                }
                
                Block_9:;
                ImageDrawable background = mainMenu.Drawable as ImageDrawable;
                string imagename = randomLetter + timeOfDay;
                background.Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(imagename, 0U));
                if (bShouldRemoveEffect)
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

        private static char previousLetter = '\0'; // Initialize with a value that won't match any valid letter.
        public static void RandomizeLetter()
        {
            Random random = new Random();
            char newLetter;
            do
            {
                int randomIndex = random.Next(0, 14);
                newLetter = (char)('a' + randomIndex);
            } while (newLetter == previousLetter);

            randomLetter = newLetter;
            previousLetter = randomLetter; // Update the previous letter
        }

        static int GetZeroOrOne()
        {
            Random random = new Random();
            return random.Next(0, 2);
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