using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using DuckGame;

// The title of your mod, as displayed in menus
[assembly: AssemblyTitle("KzMod")]

// The author of the mod
[assembly: AssemblyCompany("Stiv")]

// The description of the mod
[assembly: AssemblyDescription("Mods for the pleasure of the Kz folk")]

// The mod's version
[assembly: AssemblyVersion("1.0.0.0")]

namespace KzDuckMods
{	
	public class KzMod : Mod
	{

        internal static string AssemblyName { get; private set; }


        // The mod's priority; this property controls the load order of the mod.
        public override Priority priority
		{
			get { return base.priority; }
		}

		// This function is run before all mods are finished loading.
		protected override void OnPreInitialize()
		{
			base.OnPreInitialize();
		}

		// This function is run after all mods are loaded.
		protected override void OnPostInitialize()
		{
			base.OnPostInitialize();
            CopyLevelsAndCreatePlaylist();
        }



        public static void CopyLevelsAndCreatePlaylist()
        {
            string sourcePath = GetPath<KzMod>("CaraceLevels");
            sourcePath = sourcePath.Replace("/", "\\");

            string basePath = sourcePath;
            for (int i = 0; i < 4; i++)
            {
                basePath = Directory.GetParent(basePath)?.FullName;
            }

            string destinationPathFolder = Path.Combine(basePath, "Levels");

            string destPath = Path.Combine(destinationPathFolder, "CaraceLevels");

            Directory.CreateDirectory(destPath); 

            string[] files = Directory.GetFiles(sourcePath, "*.lev", SearchOption.AllDirectories);
            bool changesDetected = false;
            var relativePaths = new List<string>();

            foreach (string sourceFilePath in files)
            {
                string relativePath = GetRelativePath(sourcePath, sourceFilePath);
                string destFilePath = Path.Combine(destPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                bool shouldCopy = !File.Exists(destFilePath) ||
                                  File.GetLastWriteTimeUtc(sourceFilePath) > File.GetLastWriteTimeUtc(destFilePath);

                if (shouldCopy)
                {
                    File.Copy(sourceFilePath, destFilePath, true);
                    changesDetected = true;
                }

                string customPath = sourceFilePath.Replace("\\", "@02032@");
                relativePaths.Add(customPath);
            }

            string xmlFilePath = Path.Combine(destinationPathFolder, "CaraceLevels.play");

            if (changesDetected || !File.Exists(xmlFilePath))
            {
                var playlistXml = new XElement("playlist",
                    relativePaths.ConvertAll(path => new XElement("element", path))
                );

                playlistXml.Save(xmlFilePath);
            }
        }

        private static string GetRelativePath(string basePath, string fullPath)
        {
            Uri baseUri = new Uri(AppendDirectorySeparatorChar(basePath));
            Uri fullUri = new Uri(fullPath);
            return Uri.UnescapeDataString(
                baseUri.MakeRelativeUri(fullUri)
                       .ToString()
                       .Replace('/', Path.DirectorySeparatorChar)
            );
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return path + Path.DirectorySeparatorChar;
            return path;
        }

    }
}
