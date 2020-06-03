using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SoL
{
    public class StringInputPopup : EditorWindow
    {
        private string value;
        private string message;
        private Action<string> onSubmit;

        public static void Display(Action<string> onSubmit, string value, string message)
        {
            StringInputPopup window = ScriptableObject.CreateInstance<StringInputPopup>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.value = value;
            window.message = message;
            window.onSubmit = onSubmit;
            window.ShowPopup();
            
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(message);

            EditorGUILayout.Space(32f);
            value = EditorGUILayout.TextField(value);

            EditorGUILayout.Space(32f);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            if (GUILayout.Button("Confirm"))
            {
                onSubmit(value);
                Close();
            }

            EditorGUILayout.EndHorizontal();

        }
    }
}
