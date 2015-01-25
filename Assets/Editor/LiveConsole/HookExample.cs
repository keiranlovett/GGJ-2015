using CielaSpike.Unity.LiveConsole;
using UnityEngine;

[assembly: LiveConsoleHook(typeof(Example.ExampleHook))]
namespace Example
{
    class ExampleHook : LiveConsoleHook
    {
        protected override bool IsEnabled()
        {
            // >>>>> Change to true to enable this hook! <<<<<
            return false;
        }

        protected override HookResult OnColoringLogEntry(EntryInfo entry, out Color color)
        {
            color = default(Color);

            // make even row green
            if (entry.RowNumber % 2 == 0)
            {
                color = Color.green;

                // notify color is hooked.
                return HookResult.Hooked;
            }

            // use default behaviour;
            return HookResult.Default;
        }

        protected override HookResult OnTaggingLogEntry(EntryInfo entry, out string tag, out TagColor tagColor)
        {
            tag = null;
            tagColor = TagColor.Cyan;

            // tag even odd row orange and change tag text.
            if (entry.RowNumber % 2 == 1)
            {
                tag = "Odd!";
                tagColor = TagColor.Orange;

                return HookResult.Hooked;
            }

            return HookResult.Default;
        }

        protected override HookResult OnDrawLogEntry(EntryInfo entry)
        {
            // make selected entry a button
            if (entry.IsSelected)
            {
                if (GUI.Button(entry.DrawPosition, "Hooked!"))
                {
                    Debug.Log("Hooked Row " + entry.RowNumber);
                }

                return HookResult.Hooked;
            }

            return HookResult.Default;
        }

        protected override HookResult OnOpenExternalEditor(EntryInfo entry)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // pass editor's executable path and command line args.
                OpenExternalApplication("notepad.exe", entry.FileName);
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                OpenExternalApplication("sublimetext",
                    string.Format("\"{0}:{1}\"", entry.FileName, entry.LineNumber));
            }
            else
            {
                return HookResult.Default;
            }

            // return hooked to turn it on.
            return HookResult.Hooked;
        }
    }
}
