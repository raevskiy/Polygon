using System.IO;
using System.Linq;
using KopliSoft.Investigation;
using UnityEditor;
using UnityEngine;

namespace KopliSoft.EditorTools
{
    /// <summary>
    /// Validates the case data and ink files in Assets/Story: from the menu, and automatically whenever they change.
    /// </summary>
    public class StoryValidation : AssetPostprocessor
    {
        private const string StoryFolder = "Assets/Story";

        [MenuItem("Polygon/Validate Story")]
        public static void ValidateAll()
        {
            int errors = 0;
            int warnings = 0;
            foreach (var chapter in Directory.GetDirectories(StoryFolder))
            {
                foreach (var caseFile in Directory.GetFiles(chapter, "case_*.json"))
                {
                    var issues = Validate(chapter, caseFile);
                    errors += issues.Count(i => i.Severity == IssueSeverity.Error);
                    warnings += issues.Count(i => i.Severity == IssueSeverity.Warning);
                }
            }
            Debug.Log("Story validation finished: " + errors + " error(s), " + warnings + " warning(s)");
        }

        private static System.Collections.Generic.List<ValidationIssue> Validate(string chapter, string caseFile)
        {
            System.Collections.Generic.List<ValidationIssue> issues;
            try
            {
                var definition = CaseDefinition.FromJson(File.ReadAllText(caseFile));
                var inkFiles = Directory.GetFiles(chapter, "*.ink", SearchOption.AllDirectories)
                    .Select(p => new InkSourceFile(p.Replace('\\', '/'), File.ReadAllText(p)));
                issues = CaseValidator.Validate(definition, inkFiles);
            }
            catch (System.Exception e)
            {
                Debug.LogError(caseFile + ": cannot be read: " + e.Message);
                return new System.Collections.Generic.List<ValidationIssue>
                {
                    new ValidationIssue(IssueSeverity.Error, caseFile, e.Message)
                };
            }

            foreach (var issue in issues)
            {
                var message = "[Story] " + issue;
                if (issue.Severity == IssueSeverity.Error)
                {
                    Debug.LogError(message);
                }
                else
                {
                    Debug.LogWarning(message);
                }
            }
            return issues;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var changed = importedAssets.Concat(deletedAssets).Concat(movedAssets)
                .Any(p => p.StartsWith(StoryFolder + "/") && (p.EndsWith(".ink") || p.EndsWith(".json")) && !IsCompiledInk(p));
            if (changed)
            {
                ValidateAll();
            }
        }

        // ink-unity-integration writes chapter1.json next to chapter1.ink; that output is not ours to validate.
        private static bool IsCompiledInk(string path)
        {
            return path.EndsWith(".json") && File.Exists(Path.ChangeExtension(path, ".ink"));
        }
    }
}
