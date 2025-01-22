using System;
using System.Collections.Generic;
using Sims3.Gameplay;
using Sims3.Gameplay.Core;
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
        public static string timeOfDay;
        [Tunable] public static bool bRespectTimeOfDay;
        public static bool bShownDialog;
        [Tunable] public static bool bShowMods;
        public static bool bShouldRemoveEffect;
        private static Random random = new Random();
        private static char previousLetter;
        private static char randomLetter;
        [Tunable] public static int dayHour;
        [Tunable] public static int nightHour;
        [Tunable] public static string startLetter;
        [Tunable] public static string endLetter;
        [Tunable] public static bool bDebugging;
        

    static Main()
        {
            World.sOnStartupAppEventHandler += OnStartupApp;
            World.sOnEnterNotInWorldEventHandler += HandleNotInWorldEvent;
            World.sOnLeaveNotInWorldEventHandler += HandleNotInWorldEvent;
        }

        private static void OnStartupApp(object sender, EventArgs e)
        {
            //In testing only
            if (bDebugging)
            {
                Commands.sGameCommands.Register("cb", "Change Background.", Commands.CommandType.General, (Main.ChangeBackgroundCheat));
                Commands.sGameCommands.Register("fb", "Force change background to image.", Commands.CommandType.General, (Main.ForceBackgroundCheat));
            }
            Commands.sGameCommands.Register("mods", "Shows the mods list.", Commands.CommandType.General, (Main.ModCheckHandler));
        }

        private static int ForceBackgroundCheat(object[] parameters)
        {
            Simulator.AddObject(new OneShotFunctionTask(() =>
            {
                    FBHandler();
            }, StopWatch.TickStyles.Seconds, 0.1f));
            return 1;
        }

        public static void FBHandler()
        {
            string result = StringInputDialog.Show("Force Background", "Enter background image name:", "", false);

            // Ensure the input is exactly 2 characters long
            if (result.Length != 2)
            {
                SimpleMessageDialog.Show("Invalid Input", "Input must be exactly 2 characters long.");
                return;
            }

            char firstChar = result[0];
            char secondChar = result[1];

            // Check if the first character is a letter and not higher than endLetter
            if (!Char.IsLetter(firstChar) || firstChar > endLetter[0])
            {
                SimpleMessageDialog.Show("Invalid Input", $"The first character must be a letter not higher than {endLetter[0]}.");
                return;
            }

            // Check if the second character is a digit and not higher than 1
            if (!Char.IsDigit(secondChar) || secondChar > '1')
            {
                SimpleMessageDialog.Show("Invalid Input", "The second character must be a digit not higher than 1.");
                return;
            }
            
            randomLetter = firstChar; // Store the letter as string
            timeOfDay = secondChar.ToString(); // Store the number as string

            Run();
        }

        private static void HandleNotInWorldEvent(object sender, EventArgs e)
        {
            bShouldRemoveEffect = true;
            GetRandom();
            Simulator.AddObject(new OneShotFunctionTask(Run, StopWatch.TickStyles.Seconds, 0.1f));
        }
        public static void GetRandom()
        {
            DateTime currentTime = DateTime.Now;
            timeOfDay = bRespectTimeOfDay ? GetDayOrNight(currentTime) : GetZeroOrOne().ToString();
            RandomizeLetter();
        }
        
        public static int ChangeBackgroundCheat(object[] parameters)
        {
            if (GameStates.IsInMainMenuState)
            {
                GetRandom();
                Run();
                return 1;
            }
            return 1;
        }
        static string GetDayOrNight(DateTime time)
        {
            return (time.Hour >= dayHour && time.Hour < nightHour) ? "1" : "0";
        }
        private static void Run()
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
                    string imageName = randomLetter + timeOfDay;
                    background.Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(imageName, 0U));
                    
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
        }

        public static void RandomizeLetter()
        {
            // Convert startLetter and endLetter to their integer (Unicode) values
            int start = startLetter[0];
            int end = endLetter[0];

            // Generate a random number between the Unicode values of startLetter and endLetter
            char newLetter;
            do
            {
                newLetter = (char)random.Next(start, end + 1);  // Include the end letter
            } while (newLetter == previousLetter);  // Ensure it's different from the previous letter

            randomLetter = newLetter;
            previousLetter = randomLetter;  // Update the previous letter for the next iteration
        }


        public static int GetZeroOrOne()
        {
            return random.Next(0, 2); // Return either 0 or 1
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
        private static int ModCheckHandler(object[] parameters)
        {
            Simulator.AddObject(new OneShotFunctionTask(ModCheck, StopWatch.TickStyles.Seconds, 0.1f));
            return 1;
        }
    }
}