using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KopliSoft.Investigation
{
    public enum Relation
    {
        Match,
        Contradiction
    }

    public class CharacterDefinition
    {
        // Used as the line prefix in ink: "Guide: Hello."
        public string id;
        // Display name. Should equal the Fungus Character name to reuse its color and portrait.
        public string name;
    }

    public class NoteDefinition
    {
        public string id;
        public string title;
        public string text;
        // testimony | document | observation | evidence
        public string kind;
        public string source;
        // InventoryMaster item id of the same note, used while old Fungus dialogs still exist.
        public int? legacyItemId;
    }

    public class ConclusionDefinition
    {
        public string id;
        public string title;
        public string text;
        public Relation relation = Relation.Match;
        // Notes and/or other conclusions that must be combined, in any order.
        public List<string> requires = new List<string>();
        // A weak conclusion is correct but shallow; a better one usually supersedes it.
        public bool weak;
        public List<string> supersedes = new List<string>();
        public List<string> startsTasks = new List<string>();
        public List<string> completesTasks = new List<string>();
    }

    public class TaskDefinition
    {
        public string id;
        public string title;
        public string text;
    }

    /// <summary>
    /// Static data of one case (chapter): every note, conclusion rule and task. Loaded from case_*.json.
    /// </summary>
    public class CaseDefinition
    {
        public string id;
        public List<CharacterDefinition> characters = new List<CharacterDefinition>();
        public List<string> initialTasks = new List<string>();
        public List<NoteDefinition> notes = new List<NoteDefinition>();
        public List<ConclusionDefinition> conclusions = new List<ConclusionDefinition>();
        public List<TaskDefinition> tasks = new List<TaskDefinition>();
        // Words that must never appear in player-facing text; the validator warns about them.
        public List<string> forbiddenWords = new List<string>();

        private Dictionary<string, NoteDefinition> noteById;
        private Dictionary<string, ConclusionDefinition> conclusionById;
        private Dictionary<string, TaskDefinition> taskById;
        private Dictionary<string, CharacterDefinition> characterById;
        private Dictionary<int, NoteDefinition> noteByLegacyItemId;

        public static CaseDefinition FromJson(string json)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());
            return JsonConvert.DeserializeObject<CaseDefinition>(json, settings);
        }

        public NoteDefinition FindNote(string id)
        {
            BuildIndex();
            return Lookup(noteById, id);
        }

        public ConclusionDefinition FindConclusion(string id)
        {
            BuildIndex();
            return Lookup(conclusionById, id);
        }

        public TaskDefinition FindTask(string id)
        {
            BuildIndex();
            return Lookup(taskById, id);
        }

        public CharacterDefinition FindCharacter(string id)
        {
            BuildIndex();
            return Lookup(characterById, id);
        }

        public NoteDefinition FindNoteByLegacyItemId(int itemId)
        {
            BuildIndex();
            NoteDefinition note;
            noteByLegacyItemId.TryGetValue(itemId, out note);
            return note;
        }

        private static T Lookup<T>(Dictionary<string, T> index, string id) where T : class
        {
            T value;
            if (id != null && index.TryGetValue(id, out value))
            {
                return value;
            }
            return null;
        }

        private void BuildIndex()
        {
            if (noteById != null)
            {
                return;
            }
            // Duplicates are reported by CaseValidator; here the first one wins.
            noteById = ToIndex(notes, n => n.id);
            conclusionById = ToIndex(conclusions, c => c.id);
            taskById = ToIndex(tasks, t => t.id);
            characterById = new Dictionary<string, CharacterDefinition>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var character in characters.Where(c => c.id != null))
            {
                if (!characterById.ContainsKey(character.id))
                {
                    characterById.Add(character.id, character);
                }
            }
            noteByLegacyItemId = new Dictionary<int, NoteDefinition>();
            foreach (var note in notes.Where(n => n.legacyItemId.HasValue))
            {
                if (!noteByLegacyItemId.ContainsKey(note.legacyItemId.Value))
                {
                    noteByLegacyItemId.Add(note.legacyItemId.Value, note);
                }
            }
        }

        private static Dictionary<string, T> ToIndex<T>(IEnumerable<T> items, System.Func<T, string> key)
        {
            var index = new Dictionary<string, T>();
            foreach (var item in items)
            {
                var id = key(item);
                if (id != null && !index.ContainsKey(id))
                {
                    index.Add(id, item);
                }
            }
            return index;
        }
    }
}
