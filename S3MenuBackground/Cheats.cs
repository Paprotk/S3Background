using System.Collections.Generic;
using Sims3.Gameplay;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.GameEntry;
using OneShotFunctionTask = Sims3.UI.OneShotFunctionTask;

namespace S3MenuBackground
{
    public class Cheats
    {
        public static int ChangeBackground(object[] parameters)
        {
            if (GameStates.IsInMainMenuState)
            {
                Main.Run();
                return 1;
            }
            return 1;
        }
        public static int PrintList(object[] parameters)
        {
            // Ensure the lists are populated before trying to print
            if (Main.dayFileList.Count == 0 || Main.nightFileList.Count == 0)
            {
                SimpleMessageDialog.Show("No Data", "No images found in the lists.");
                return 0;
            }

            // Join both lists into a single string
            string content = "Day images:\n" + string.Join(", ", Main.dayFileList.ToArray()) + "\n\nNight images:\n" + string.Join(", ", Main.nightFileList.ToArray());

            // Show the content in the SimpleMessageDialog
            Simulator.AddObject(new OneShotFunctionTask(() =>
            {
                SimpleMessageDialog.Show("List Contents", content);
            }, StopWatch.TickStyles.Seconds, 0.1f));

            return 1;
        }
        public static int ForceBackgroundHandler(object[] parameters)
        {
            Simulator.AddObject(new OneShotFunctionTask(() =>
            {
                ForceBackground();
            }, StopWatch.TickStyles.Seconds, 0.1f));
            return 1;
        }

        public static void ForceBackground()
        {
            List<string> combinedList = new List<string>(Main.dayFileList);
            combinedList.AddRange(Main.nightFileList);
            string result = StringInputDialog.Show("Force Background", "Enter background image name:", "", false);

            // Ensure the input is exactly 2 characters long
            if (result.Length != 2)
            {
                SimpleMessageDialog.Show("Invalid Input", "Input must be exactly 2 characters long.");
                return;
            }

            char firstChar = result[0];
            char secondChar = result[1];

            // Combine the two characters into a string
            string combinedInput = firstChar.ToString() + secondChar.ToString();

            // Check if the combined string exists in the combinedList
            if (!combinedList.Contains(combinedInput))
            {
                SimpleMessageDialog.Show("Invalid Input", "Image does not exist in the combined list.");
                return;
            }

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

                    // Assign the combined string as the image name
                    string imageName = combinedInput;
                    background.Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(imageName, 0U));
                    mainMenu.Invalidate();
                }
            }
        }
        public static int ModCheckHandler(object[] parameters)
        {
            Simulator.AddObject(new OneShotFunctionTask(Main.ModCheck, StopWatch.TickStyles.Seconds, 0.1f));
            return 1;
        }
    }
}