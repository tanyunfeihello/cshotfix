﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
using System.IO;

public class InjectEditor : ScriptableObject
{
    [MenuItem("CSHotFix/GenHotFixDelegate", false, 1)]
    public static void HotfixGenDelegate()
    {
        if (EditorApplication.isCompiling || Application.isPlaying)
        {
            return;
        }
        if (!EditorUtility.DisplayDialog("警告", "你是否需要重新生成热更新委托信息？", "需要", "按错了"))
        {
            return;
        }
        string editorpath = Application.dataPath;
        editorpath = Path.GetFullPath(editorpath);
        editorpath = editorpath.Replace("UnityDemo\\CSHotFixDemo\\Assets", "");
        editorpath = Path.Combine(editorpath, "CSHotFixInjector\\bin\\Debug\\CSHotFixInjector.exe");
        Process InjectorEditorProcess = new Process();
        InjectorEditorProcess.StartInfo.FileName = editorpath;
        InjectorEditorProcess.StartInfo.Arguments = "GenDelegate";
        InjectorEditorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        InjectorEditorProcess.StartInfo.RedirectStandardOutput = true;
        InjectorEditorProcess.StartInfo.UseShellExecute = false;
        InjectorEditorProcess.StartInfo.CreateNoWindow = true;
        InjectorEditorProcess.Start();
        InjectorEditorProcess.WaitForExit();
        UnityEngine.Debug.Log(InjectorEditorProcess.StandardOutput.ReadToEnd());
        AssetDatabase.Refresh();
    }
    [MenuItem("CSHotFix/GenHotFixField", false, 2)]
    public static void HotfixGenStaticField()
    {
        if (EditorApplication.isCompiling || Application.isPlaying)
        {
            return;
        }
        if (!EditorUtility.DisplayDialog("警告", "你是否需要重新生成热更新字段信息？", "需要", "按错了"))
        {
            return;
        }
        string editorpath = Application.dataPath;
        editorpath = Path.GetFullPath(editorpath);
        editorpath = editorpath.Replace("UnityDemo\\CSHotFixDemo\\Assets", "");
        editorpath = Path.Combine(editorpath, "CSHotFixInjector\\bin\\Debug\\CSHotFixInjector.exe");
        Process InjectorEditorProcess = new Process();
        InjectorEditorProcess.StartInfo.FileName = editorpath;
        InjectorEditorProcess.StartInfo.Arguments = "GenStaticField";
        InjectorEditorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        InjectorEditorProcess.StartInfo.RedirectStandardOutput = true;
        InjectorEditorProcess.StartInfo.UseShellExecute = false;
        InjectorEditorProcess.StartInfo.CreateNoWindow = true;
        InjectorEditorProcess.Start();
        InjectorEditorProcess.WaitForExit();
        UnityEngine.Debug.Log(InjectorEditorProcess.StandardOutput.ReadToEnd());
        AssetDatabase.Refresh();
    }

    [PostProcessScene]
    [MenuItem("CSHotFix/GenHotFix", false, 3)]
    public static void HotfixInject()
    {
        if (EditorApplication.isCompiling || Application.isPlaying)
        {
            return;
        }
        string editorpath = Application.dataPath;
        editorpath = Path.GetFullPath(editorpath);
        editorpath = editorpath.Replace("UnityDemo\\CSHotFixDemo\\Assets","");
        editorpath = Path.Combine(editorpath, "CSHotFixInjector\\bin\\Debug\\CSHotFixInjector.exe");
        Process InjectorEditorProcess = new Process();
        InjectorEditorProcess.StartInfo.FileName = editorpath;
        InjectorEditorProcess.StartInfo.Arguments = "InjectIL";
        InjectorEditorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        InjectorEditorProcess.StartInfo.RedirectStandardOutput = true;
        InjectorEditorProcess.StartInfo.UseShellExecute = false;
        InjectorEditorProcess.StartInfo.CreateNoWindow = true;
        InjectorEditorProcess.Start();
        InjectorEditorProcess.WaitForExit();
        UnityEngine.Debug.Log(InjectorEditorProcess.StandardOutput.ReadToEnd());
        AssetDatabase.Refresh();
    }
}