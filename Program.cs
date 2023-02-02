/// Console app to write a csv file of the layers in a GIMP .xcf file in the format:
/// imageName,x,y,width,height
/// IMPORTANT
/// The background layer must be labelled with a # prefix ->"#Background" and placed at the end of the layer list
/// All layers on top of the background will be written to the .csv file in reverse order
/// If the first layer is top left (0,0) on the ImageBackground, and subsequent layers are ordered topLeft -> topRight
/// then top -> bottom, the coordinates on the csv file will be in order
/// If the .exe for this app is run from the folder containing the .xcf files, you will be presented a choice of which file you want to process

using System;
using System.Collections.Generic;
using System.IO;
using ImageMagick; // Nuget Magick.net-Q16-AnyCPU

namespace XCFToCSV
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string imagePath;
            string appPath = Path.GetDirectoryName(path: System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            string[] files = { };
            XConsole6.Initialise("Gimp .xcf Layer to CSV tool", 80, 22, "White", "Black"); // Setup the console to 80 x 22 White foreground, Black background
            XConsole6.Header("Gimp xcf Reader", "For extracting layer coordinates and rectangle sizes", "White", "DarkRed"); // Draw the Header White on DarkRed
            XConsole6.Sleep(2); // Pause for 2 seconds
            // Select file extension
            int row = XConsole6.Clear(foreColor: "Yellow", backColor: "Black", width: 80, height: 22); // Clear the console Yellow on Black 80 x 22. Start row Count
            row  += XConsole6.Header("Choose your file extension", "File content is identical in all cases (.csv)", "White", "DarkGreen"); // Draw header and increase row Count
            row += 2; // Add 2 blank lines
            List<string> fileExtensions = new List<string> { "custom...", ".csv", ".txt", ".data", ".xcfdata"  }; // List of strings to display as a Menu
            int choice = XConsole6.Menu("Select the file extension you require",
                                        fileExtensions,
                                        row, 0, "White", "Green"); // Display Menu White on Green. Menu is displayed below the header as row Count is passed
            string fileExtension = ""; // initialise fileExtension as empty string
            if (choice == 0) // User has chosen "custom"
            {
                row = XConsole6.Clear(foreColor: "Yellow", backColor: "Black", width: 80, height: 22);
                row += XConsole6.Header("Type your file extension without the '.' prefix. Minimum 2 characters, max 10", "File content is identical in all cases (.csv)", "Green", "Black");
                row += 2;
                fileExtension = XConsole6.GetString(prompt: "Type the extension you require 3-10 characters ('.' not required)",
                                                    withTitle: false, min: 3, max: 10, row: row, windowWidth: 0); // get a string 3-10 characters
                if (!fileExtension.StartsWith("."))
                    fileExtension = $".{fileExtension}";
            }
            else
                fileExtension = fileExtensions[choice];
            // Path.txt allows running from IDE and locating a specific path for data
            if (File.Exists(Path.Combine(appPath, "Path.txt"))) //Path.txt found so .exe not running in target directory (eg from IDE)
            {
                imagePath = File.ReadAllText(Path.Combine(appPath, "Path.txt"));
                if (Directory.Exists(imagePath))
                    files = Directory.GetFiles(imagePath, "*.xcf");
            }
            else
            {
                imagePath = appPath;
                files = Directory.GetFiles(appPath, "*.xcf");
            }
            int maxFileNameLength = 0;
            foreach (string file in files)
            {
                if(Path.GetFileName(file).Length > maxFileNameLength)
                    maxFileNameLength = Path.GetFileName(file).Length;
            }
            List<string> fileList = new List<string>();
            foreach (string file in files) // Create a neat formatted display of file names and output names
            {
                string inputFile = Path.GetFileName(file).PadRight(maxFileNameLength + 1);
                string outputFile = inputFile.Substring(0,inputFile.IndexOf("."));
                outputFile += fileExtension;
                fileList.Add($"{inputFile} -> {outputFile}");
            }
            fileList.Insert(0, "Quit"); // Add "Quit" to the top of the list
            bool quit = false;
            while (!quit)
            {
                row = XConsole6.Clear(foreColor: "Yellow", backColor: "Black", width: 80, height: 22);
                row += 2;
                choice = XConsole6.Menu("Select the file to read Layer information", fileList, row, 0, "White", "Yellow");
                if (choice == 0) quit = true;
                else
                {
                    string inputFile = files[choice - 1].ToString();
                    string saveFile = Path.Combine(imagePath, Path.GetFileNameWithoutExtension(files[choice - 1]) + fileExtension);

                    XConsole6.Clear();
                    if (File.Exists(saveFile)) File.Delete(saveFile); // if file already exists, delete it

                    using (MagickImageCollection images = new MagickImageCollection(inputFile))
                    {
                        using (StreamWriter f = new StreamWriter(saveFile))
                        {
                            for (int index = images.Count - 1; index > -1; index--)
                            {
                                // lines starting with "#", or (Legacy from previous version) "ImageBackground" or "Group:" NOT processed
                                if (!images[index].Label.StartsWith("#") && images[index].Label != "ImageBackground" && !images[index].Label.StartsWith("Group:"))
                                {
                                    Console.WriteLine($"{images[index].Label},{images[index].Page.X},{images[index].Page.Y},{images[index].Width},{images[index].Height}");
                                    if (index > 1) // terminate line with newline character
                                        f.WriteLine($"{images[index].Label},{images[index].Page.X},{images[index].Page.Y},{images[index].Width},{images[index].Height}");
                                    else // do not add newline character
                                        f.Write($"{images[index].Label},{images[index].Page.X},{images[index].Page.Y},{images[index].Width},{images[index].Height}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
