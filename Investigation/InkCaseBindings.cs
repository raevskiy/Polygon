using System;
using Ink.Runtime;

namespace KopliSoft.Investigation
{
    /// <summary>
    /// Implements the EXTERNAL functions declared in case_functions.ink.
    /// </summary>
    public static class InkCaseBindings
    {
        public static void Bind(Story story, CaseFile caseFile, Action<string> warn)
        {
            story.allowExternalFunctionFallbacks = true;

            story.BindExternalFunction("has_note", (string id) => (object)caseFile.HasNote(Checked(caseFile.Definition.FindNote(id), id, warn)), true);
            story.BindExternalFunction("has_conclusion", (string id) => (object)caseFile.HasConclusion(Checked(caseFile.Definition.FindConclusion(id), id, warn)), true);
            story.BindExternalFunction("task_active", (string id) => (object)caseFile.IsTaskActive(Checked(caseFile.Definition.FindTask(id), id, warn)), true);
            story.BindExternalFunction("task_done", (string id) => (object)caseFile.IsTaskDone(Checked(caseFile.Definition.FindTask(id), id, warn)), true);

            // Side effects must not run during ink's lookahead, hence lookaheadSafe: false.
            story.BindExternalFunction("start_task", (string id) => { caseFile.StartTask(Checked(caseFile.Definition.FindTask(id), id, warn)); }, false);
            story.BindExternalFunction("complete_task", (string id) => { caseFile.CompleteTask(Checked(caseFile.Definition.FindTask(id), id, warn)); }, false);
            story.BindExternalFunction("add_note", (string id) => { caseFile.AddNote(Checked(caseFile.Definition.FindNote(id), id, warn)); }, false);
        }

        private static string Checked(object definition, string id, Action<string> warn)
        {
            if (definition == null && warn != null)
            {
                warn("Ink refers to unknown case id '" + id + "'");
            }
            return id;
        }
    }
}
