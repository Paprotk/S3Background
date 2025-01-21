using System;
using Sims3.Gameplay;
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
        
        public static char randomLetter;
        
        static Main()
        {
            World.sOnStartupAppEventHandler += OnStartupApp;
            World.sOnEnterNotInWorldEventHandler += OnEnterNotInWorldEventHandler;
        }

        private static void OnEnterNotInWorldEventHandler(object sender, EventArgs e)
        {
            Simulator.AddObject(new OneShotFunctionTask(Main.Run, StopWatch.TickStyles.Milliseconds, 1f));
        }

        private static void OnStartupApp(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            timeOfDay = GetDayOrNight(currentTime);
            Randomize();
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

                // If successful, mainMenu now holds the MainMenu object
                Block_9:;
                ImageDrawable background = mainMenu.Drawable as ImageDrawable;
                background.Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(randomLetter + timeOfDay, 0U));
                mainMenu.Invalidate();
            }
        }

        public static void Randomize()
        {
            Random random = new Random();
            int randomIndex = random.Next(0, 26);
            randomLetter = (char)('a' + randomIndex);
        }
    }
}