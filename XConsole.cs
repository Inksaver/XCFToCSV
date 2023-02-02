using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace XCFToCSV
{
    /// <summary>
    /// Console colours:
    ///	Black, Blue, Cyan, Gray, Green, Magenta, Red, Yellow, White
    /// DarkBlue, DarkCyan, DarkGray, DarkGreen, DarkMagenta, DarkRed, DarkYellow,
    /// 
    /// Useage:
    /// In Program.cs 
    /// Set window tilte, width, height, background and foreground colours: size in chars NOT in pixels
    ///     XConsole6.Initialise("Window Title Here", 120, 22, "White", "Blue");
    /// Add a title block with 2 lines reserved for title and sub-title, surrounded by utf8 border:
    ///     XConsole6.Header("Header text here","Sub-Header text here","White","DarkRed");
    ///     ╔════════════════════════╗
    ///     ║    Header text here    ║
    ///     ║  Sub-Header text here  ║
    ///     ╚════════════════════════╝
    ///     
    ///  Write a line of text defining text, foreColor, backColor and alignment Left/Centre/Right (also accepts Center)
    ///     XConsole6.WriteLine("Line of Text here", "Black", "Red", "Centre");
    ///     
    /// Add a blank line using previously set fore/back colours
    ///     XConsole6.WriteLine("");
    /// 
    /// int row = XConsole6.Clear();
    /// string name = XConsole6.GetString(prompt: "What is your name?", withTitle: true, min: 1, max: 10, row: row);
    /// row += 2; // keep track of current line. User pressing Enter on Input increases line count
    /// XConsole6.Print($"Hello {name}");
    /// int age = XConsole6.GetInteger("How old are you", 5, 110, row);
    /// row += 2;
    /// XConsole6.Print($"You are {age} years old");
    /// double height = XConsole6.GetRealNumber("How tall are you?", 0.5, 2.0, row);
    /// XConsole6.Print($"You are {height} metres tall");
    /// row += 2;
    /// bool likesPython = XConsole6.GetBoolean("Do you like Python? (y/n)", row);
    /// if (likesPython)
    ///     XConsole6.Print($"You DO like Python");
    /// else
    ///     XConsole6.Print($"You DO NOT like Python");
    /// string title = "Choose your favourite language";
    /// row += 2;
    /// List<string> options = new List<string> { "C#", "Python", "Lua", "Java" };
    /// int choice = XConsole6.Menu(title, options, row);
    /// XConsole6.Print($"Your favourite language is {options[choice]}");
    /// </summary>
    internal static class XConsole6
    {
        public static int Delay = 2000;
        static Dictionary<string, ConsoleColor> dictColors = new Dictionary<string, ConsoleColor>();
        static ConsoleColor defaultBackGround = ConsoleColor.Black;
        static ConsoleColor defaultForeGround = ConsoleColor.White;
        static int Width = 80;
        static int Height = 25;

        public static int Clear(string foreColor = "", string backColor = "", int width = 80, int height = 25)
        {
            /// Clears console and sets fore / back colours
            if (foreColor == string.Empty)
                Console.ForegroundColor = defaultForeGround;
            else
                SetConsoleColor("fore", foreColor);
            if (backColor == string.Empty)
                Console.BackgroundColor = defaultBackGround;
            else
                SetConsoleColor("back", backColor);
            CheckSizeLimits(width, height);
            Console.SetWindowSize(Width, Height);
            Console.BufferWidth = Width;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            return 0;
        }
        private static void ClearInputField(int row)
        {
            /// Clears a row of text ready for re-entry
            if (row >= 0)
            {
                Console.SetCursorPosition(0, row); // left, top 0 based
                Console.Write("".PadRight(Console.WindowWidth - 1));
                Console.SetCursorPosition(0, row);
            }
        }
        private static void ErrorMessage(int row, string errorType, string userInput, double minValue = 0, double maxValue = 0)
        {
            /// Displays appropriate error message
            if (row < 0) row = 0;
            string message = "Just pressing the Enter key or spacebar doesn't work"; // default for "noinput"
            if (errorType == "string")
                message = $"Try entering text between {minValue} and {maxValue} characters";
            else if (errorType == "bool")
                message = "Only anything starting with y or n is accepted";
            else if (errorType == "nan")
                message = $"Try entering a number - {userInput} does not cut it";
            else if (errorType == "notint")
                message = $"Try entering a whole number - {userInput} does not cut it";
            else if (errorType == "range")
                message = $"Try a number from {minValue} to {maxValue}";
            else if (errorType == "intconversion")
                message = $"Try entering a whole number: {userInput} cannot be converted...";
            else if (errorType == "realconversion")
                message = $"Try entering a decimal number: {userInput} cannot be converted...";

            message = $">>> {message} <<<";
            ClearInputField(row + 1);       // clear current line
            Console.Write(message);         // write message
            Sleep(Delay);                   // pause 2 seconds
            ClearInputField(row + 1);       // clear current line
        }
        public static bool GetBoolean(string prompt, int row = -1, int windowWidth = 0)
        {
            /// gets a boolean (yes/no) type entry from the user
            string userInput = ProcessInput(prompt, min: 1, max: 3, dataType: "bool", row, windowWidth);
            return Convert.ToBoolean(userInput);
        }
        public static double GetRealNumber(string prompt, double min, double max, int row = -1, int windowWidth = 0)
        {
            /// gets a float / double from the user
            string userInput = ProcessInput(prompt, min, max, dataType: "real", row, windowWidth);
            return Convert.ToDouble(userInput);
        }
        public static int GetInteger(string prompt, double min, double max, int row = -1, int windowWidth = 0, string textColour = "White")
        {
            /// Public Method: gets an integer from the user ///
            string userInput = ProcessInput(prompt, min, max, dataType: "int", row, windowWidth, textColour = "White");
            return Convert.ToInt32(userInput);
        }
        public static string GetString(string prompt, bool withTitle, int min, int max, int row = -1, int windowWidth = 0)
        {
            /// Public Method: gets a string from the user ///
            string userInput = ProcessInput(prompt, min, max, dataType: "string", row, windowWidth);
            if (withTitle)
            {
                TextInfo textInfo = new CultureInfo("en-UK", false).TextInfo;
                userInput = textInfo.ToTitleCase(userInput);
            }
            return userInput;
        }
        private static void CheckSizeLimits(int width, int height)
        {
            if (width > Console.LargestWindowWidth)
                Width = Console.LargestWindowWidth;
            if (height > Console.LargestWindowHeight)
                Height = Console.LargestWindowHeight;
        }
        public static void Initialise(string title, int width, int height, string foreColor, string backColor)
        {
            /// default Console setup
            Console.Title = title;
            foreach (ConsoleColor color in Enum.GetValues(typeof(ConsoleColor)))
            {
                dictColors.Add(color.ToString(), color);
            }
            defaultForeGround = SetConsoleColor("fore", foreColor);
            defaultBackGround = SetConsoleColor("back", backColor);
            CheckSizeLimits(width, height);
            Console.SetWindowSize(width, height);
            Console.BufferWidth = width;
            Console.Clear();
        }
        public static void Initialise(string title, string swidth, string sheight, string foreColor, string backColor)
        {
            /// Default Console setup
            int width = 80;
            int height = 25;
            int.TryParse(swidth, out width);
            int.TryParse(sheight, out height);
            Initialise(title, width, height, foreColor, backColor);
        }
        public static int Menu(string title, List<string> textLines, int row = -1, int windowWidth = 0, string textColour = "White", string frameColour = "White")
        {
            /// displays a menu using the text in 'title', and a list of menu items (string)
            /// This menu will re-draw until user enters correct data
            int rows = -1;
            if (row >= 0) rows = row + textLines.Count + 4;
            int maxLength = GetMaxLength(title, textLines, windowWidth);
            //string top = "╔".PadRight(maxLength, '═') + "╗";
            WriteLine("╔".PadRight(maxLength, '═') + "╗", frameColour, "Black", "Left");
            Write("║", frameColour);
            title = title.PadRight(maxLength / 2 + title.Length / 2).PadLeft(maxLength - 1);
            Write(title, textColour);
            Write("║\n", frameColour);                                           // print title
            //string divider = "╠".PadRight(maxLength, '═') + "╣\n";
            Write("╠".PadRight(maxLength, '═') + "╣\n", frameColour);
            //WriteLine($"╠{new string('═', maxLength)}╣", frameColour, "Black", "Left");
            for (int i = 0; i < textLines.Count; i++)
            {
                Write("║", frameColour);
                if (i < 9) Write($"     {i + 1}) {textLines[i].PadRight(maxLength - 9)}", textColour);  // print menu options 5 spaces
                else Write($"    {i + 1}) {textLines[i].PadRight(maxLength - 9)}", textColour);         // print menu options 4 spaces
                Write("║\n", frameColour);
            }
            WriteLine("╚".PadRight(maxLength, '═') + "╝", frameColour, "Black", "Left");
            //WriteLine($"╚{new string('═', maxLength)}╝", frameColour);
            int userInput = GetInteger(prompt: $"Type the number of your choice (1 to {textLines.Count})", min: 1, max: textLines.Count, rows);
            return userInput - 1;
        }
        public static int Print(string text = "")
        {
            /// Implementation of Python print()
            int rows = 1; // assume 1 line of text
            if (text.Contains("\n"))
                rows += text.Count(x => x == '\n');
            Console.WriteLine(text);
            return rows;
        }
        public static void Sleep(int delay)
        {
            if (delay < 100) delay *= 1000;
            Thread.Sleep(delay);
        }
        private static int GetMaxLength(string text, List<string> options, int windowWidth = 0)
        {
            /// Get max length of a list of strings
            if (windowWidth == 0) windowWidth = Console.WindowWidth;
            if (windowWidth > 0)
                return windowWidth - 2;

            int maxLength = text.Length;
            foreach (string line in options)
            {
                maxLength = Math.Max(maxLength, line.Length + 9);
            }
            maxLength += maxLength / 4;
            if (maxLength % 2 == 1)
                maxLength += 1;
            return maxLength;
        }
        private static string ProcessInput(string prompt, double min, double max, string dataType, int row, int windowWidth = 0, string textColour = "White")
        {
            /// Takes user input and ensures the output string can be converted to the required dataType
            if (windowWidth == 0) windowWidth = Console.WindowWidth;
            bool valid = false;
            string userInput = "";
            while (!valid)
            {
                ClearInputField(row + 2);
                ClearInputField(row + 1);
                ClearInputField(row);
                WriteLine($"{new string('─', windowWidth - 1)}\n", textColour);
                WriteLine($"{new string('─', windowWidth - 1)}", textColour);
                Console.SetCursorPosition(0, row + 1);
                userInput = Input(prompt);
                if (dataType == "string")
                {
                    if (userInput.Length == 0 && min > 0) ErrorMessage(row, "noinput", userInput);
                    else if (userInput.Length < min) ErrorMessage(row, "string", userInput, min, max);
                    else if (userInput.Length > max) ErrorMessage(row, "string", userInput, min, max);
                    else valid = true;
                }
                else //integer, float, bool
                {
                    if (userInput.Length == 0)                                      // just Enter pressed
                        ErrorMessage(row, "noinput", userInput);
                    else
                    {
                        if (dataType == "bool")
                        {
                            if (userInput.Substring(0, 1).ToLower() == "y")
                            {
                                userInput = "true";
                                valid = true;
                            }
                            else if (userInput.Substring(0, 1).ToLower() == "n")
                            {
                                userInput = "false";
                                valid = true;
                            }
                            else ErrorMessage(row, "bool", userInput);
                        }
                        if (dataType == "int" || dataType == "real")                // integer / float / double
                        {
                            if (double.TryParse(userInput, out double conversion))  // is a number!
                            {
                                if (conversion >= min && conversion <= max)         // within range!
                                {
                                    if (dataType == "int")                          // check if int
                                    {
                                        if (int.TryParse(userInput, out int intconversion))
                                        {
                                            valid = true;                           // userInput can be converted to int
                                        }
                                        else ErrorMessage(row, "notint", userInput);// real number supplied
                                    }
                                    else valid = true;                              // userInput can be converted to float/double/decimal
                                }
                                else ErrorMessage(row, "range", userInput, min, max);// out of range
                            }
                            else ErrorMessage(row, "nan", userInput);               // not a number
                        }
                    }
                }
            }
            return userInput; //string can be converted to bool, int or float
        }
        public static void Write(string text, string foreColor = "", string backColor = "")
        {
            /// Shortened version of Console.Write with colour options
            SetConsoleColor("fore", foreColor);
            SetConsoleColor("back", backColor);
            Console.Write(text);
            Console.ForegroundColor = defaultForeGround;
            Console.BackgroundColor = defaultBackGround;
        }
        public static void WriteLine(string text, string foreColor = "", string backColor = "", string align = "")
        {
            /// Shortened version of Console.WriteLine with colour and alignment options
            SetConsoleColor("fore", foreColor);
            SetConsoleColor("back", backColor);
            if (align == "Centre" || align == "Center")
            {
                text = text.PadRight(Console.WindowWidth / 2 - 1);
                text = text.PadLeft(Console.WindowWidth / 2  - 1);
            }
            else if (align == "Left" || align == "")
                text = text.PadRight(Console.WindowWidth - 2);
            else if (align == "Menu")
                text = text.PadRight(Console.WindowWidth - 2);
            else //Right align
                text = text.PadLeft(Console.WindowWidth - 2);

            Console.WriteLine(text);
        }
        public static string Input(string prompt, string ending = "_")
        {
            /// Get keyboard input from user (requires Enter )
            Console.Write($"{prompt}{ending}");
            return Console.ReadLine();
        }
        private static ConsoleColor SetConsoleColor(string position, string color)
        {
            ConsoleColor retValue = ConsoleColor.White;
            if (color != string.Empty)
            {
                if (!dictColors.ContainsKey(color))
                {
                    if (position == "fore")
                        color = "White";
                    else
                        color = "Black";
                }
                if (position == "fore")
                    Console.ForegroundColor = dictColors[color];
                else
                    Console.BackgroundColor = dictColors[color];

                retValue = dictColors[color];
            }
            return retValue;
        }
        public static int Header(string title, string subtitle = "", string foreColor = "", string backColor = "")
        {
            /// Draws a boxed header with up to 2 lines of text
            SetConsoleColor("fore", foreColor);
            SetConsoleColor("back", backColor);

            int windowWidth = Console.WindowWidth - 2;
            // use String.Format to control spacing. eg 78/2 + 18/2 (title.length = 18) gives {0,48} for title, and {1,31} for "║" for 18 character title
            string titleContent = String.Format("║{0," + ((windowWidth / 2) + (title.Length / 2)) + "}{1," + (windowWidth - (windowWidth / 2) - (title.Length / 2) + 1) + "}", title, "║");
            string subtitleContent = String.Format("║{0," + ((windowWidth / 2) + (subtitle.Length / 2)) + "}{1," + (windowWidth - (windowWidth / 2) - (subtitle.Length / 2) + 1) + "}", subtitle, "║");
            string topHeader   = "╔".PadRight(Console.WindowWidth - 1, '═') + "╗";
            string lowerHeader = "╚".PadRight(Console.WindowWidth - 1, '═') + "╝";
            Console.Write(topHeader);
            Console.Write(titleContent);
            Console.Write(subtitleContent);
            Console.Write(lowerHeader);
            return 4; // default 4 lines written
        }
    }
}
