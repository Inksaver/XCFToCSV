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
            XConsole6.Initialise("Gimp .xcf Layer to CSV tool", 80, 22, "White", "Black");
            XConsole6.Header("Gimp xcf Reader", "For use with Tiled Map Editor", "White", "DarkRed");
            XConsole6.Sleep(2);
            if (File.Exists(Path.Combine(appPath, "Path.txt"))) //Path.txt found so .exe not running in target directory (eg from IDE)
            {
                imagePath = File.ReadAllText(Path.Combine(appPath, "Path.txt"));
                if (Directory.Exists(imagePath))
                {
                    files = Directory.GetFiles(imagePath, "*.xcf");
                }
            }
            else
            {
                imagePath = appPath;
                files = Directory.GetFiles(appPath, "*.xcf");
            }
            List<string> fileList = new List<string>();
            foreach (string file in files)
            {
                fileList.Add(Path.GetFileName(file));
            }
            fileList.Insert(0, "Quit");
            bool quit = false;
            while (!quit)
            {
                int row = XConsole6.Clear(foreColor: "Yellow", backColor: "Black", width: 80, height: 22);
                row += 2;
                int choice = XConsole6.Menu("Select the file to read Layer information", fileList, row, 0, "White", "Yellow");
                if (choice == 0) quit = true;
                else
                {
                    string inputFile = files[choice - 1].ToString();
                    string saveFile = Path.Combine(imagePath, Path.GetFileNameWithoutExtension(files[choice - 1]) + ".data");

                    XConsole6.Clear();
                    if (File.Exists(saveFile)) File.Delete(saveFile);

                    using (MagickImageCollection images = new MagickImageCollection(inputFile))
                    {
                        using (StreamWriter f = new StreamWriter(saveFile))
                        {
                            for (int index = images.Count - 1; index > -1; index--)
                            {
                                if (images[index].Label != "ImageBackground" && !images[index].Label.StartsWith("Group:"))
                                {
                                    Console.WriteLine($"{images[index].Label},{images[index].Page.X},{images[index].Page.Y},{images[index].Width},{images[index].Height}");
                                    if (index > 1)
                                        f.WriteLine($"{images[index].Label},{images[index].Page.X},{images[index].Page.Y},{images[index].Width},{images[index].Height}");
                                    else
                                        f.Write($"{images[index].Label},{images[index].Page.X},{images[index].Page.Y},{images[index].Width},{images[index].Height}");
                                }
                            }
                        }
                    }
                }
            }
            //Console.Write("\nEnter to quit");
            //Console.ReadLine();
        }
    }
}
