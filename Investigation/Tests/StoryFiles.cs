using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ink;
using Path = System.IO.Path;
using Story = Ink.Runtime.Story;

namespace KopliSoft.Investigation.Tests
{
    /// <summary>
    /// Finds and compiles the real story files. No UnityEngine dependency, so the tests also run under plain dotnet.
    /// </summary>
    public static class StoryFiles
    {
        public static string StoryRoot()
        {
            // POLYGON_PROJECT_ROOT lets the tests run outside Unity (e.g. dotnet test from another folder).
            var starts = new[] { Environment.GetEnvironmentVariable("POLYGON_PROJECT_ROOT"), Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
            foreach (var start in starts.Where(s => !string.IsNullOrEmpty(s)))
            {
                for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
                {
                    var candidate = Path.Combine(dir.FullName, "Assets", "Story");
                    if (Directory.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }
            throw new DirectoryNotFoundException("Assets/Story not found above " + Directory.GetCurrentDirectory());
        }

        public static IEnumerable<string> ChapterFolders()
        {
            return Directory.GetDirectories(StoryRoot()).Where(d => Directory.GetFiles(d, "case_*.json").Length > 0);
        }

        public static CaseDefinition LoadCase(string chapterFolder)
        {
            return CaseDefinition.FromJson(File.ReadAllText(Directory.GetFiles(chapterFolder, "case_*.json").Single()));
        }

        public static List<InkSourceFile> InkFiles(string chapterFolder)
        {
            return Directory.GetFiles(chapterFolder, "*.ink", SearchOption.AllDirectories)
                .Select(p => new InkSourceFile(Path.GetFileName(p), File.ReadAllText(p)))
                .ToList();
        }

        /// <summary>
        /// Compiles the chapter's master ink file (the one named like the folder). Throws with all compiler errors.
        /// </summary>
        public static Story CompileChapter(string chapterFolder)
        {
            var master = Path.Combine(chapterFolder, Path.GetFileName(chapterFolder).ToLowerInvariant() + ".ink");
            return Compile(File.ReadAllText(master), new FolderFileHandler(chapterFolder), master);
        }

        public static Story Compile(string source, IFileHandler fileHandler = null, string fileName = "test.ink")
        {
            var errors = new List<string>();
            var compiler = new Compiler(source, new Compiler.Options
            {
                sourceFilename = fileName,
                countAllVisits = true,
                fileHandler = fileHandler,
                errorHandler = (message, type) =>
                {
                    if (type == ErrorType.Error)
                    {
                        errors.Add(message);
                    }
                }
            });
            var story = compiler.Compile();
            if (story == null || errors.Count > 0)
            {
                throw new InvalidOperationException("Ink compilation failed:\n" + string.Join("\n", errors));
            }
            // Round-trip through JSON, exactly like the game loads the compiled TextAsset.
            return new Story(story.ToJson());
        }

        private class FolderFileHandler : IFileHandler
        {
            private readonly string folder;

            public FolderFileHandler(string folder)
            {
                this.folder = folder;
            }

            public string ResolveInkFilename(string includeName)
            {
                return Path.Combine(folder, includeName);
            }

            public string LoadInkFileContents(string fullFilename)
            {
                return File.ReadAllText(fullFilename);
            }
        }
    }
}
