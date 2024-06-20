using System.IO;
using UnityEditor;
using UnityEngine;

internal class Note
{
	AssetImporter assetImporter;

	public string guid;
	public string info;

	public Note(string path)
	{
		assetImporter = AssetImporter.GetAtPath(path);
		guid = AssetDatabase.AssetPathToGUID(path);
		info = assetImporter.userData;
	}

	public void Save()
	{
		assetImporter.userData = info;
		assetImporter.SaveAndReimport();
	}
}

public class FolderNoteInspector : ICustomDefaultAssetInspector
{
	Note note;
	readonly GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Height(100) };

	public bool CanDraw(string path)
	{
		return Path_UnityEditorGUIExtention.InspectorShowFolderNote && Directory.Exists(path);
	}
	public void Draw(string path)
	{
		if (AssetDatabase.IsValidFolder(path))
		{
			if (note == null) note = new Note(path);

			GUI.enabled = true;
			EditorGUILayout.LabelField("注释");
			note.info = EditorGUILayout.TextArea(note.info, options);

			if (GUILayout.Button("保存"))
			{
				note.Save();
			}
		}
	}
}
