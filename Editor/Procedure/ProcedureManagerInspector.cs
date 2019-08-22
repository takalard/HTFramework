﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework
{
    [CustomEditor(typeof(ProcedureManager))]
    public sealed class ProcedureManagerInspector : HTFEditor<ProcedureManager>
    {
        private Dictionary<string, string> _procedureTypes = new Dictionary<string, string>();

        protected override void OnDefaultEnable()
        {
            base.OnDefaultEnable();

            _procedureTypes.Clear();
            string[] typePaths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < typePaths.Length; i++)
            {
                if (typePaths[i].EndsWith(".cs"))
                {
                    string className = typePaths[i].Substring(typePaths[i].LastIndexOf("/") + 1).Replace(".cs", "");
                    if (!_procedureTypes.ContainsKey(className))
                    {
                        _procedureTypes.Add(className, typePaths[i]);
                    }
                }
            }
        }

        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Activated Procedure Count:" + Target.ActivatedProcedures.Count, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = Target.DefaultProcedure != "";
            GUILayout.Label("Default: " + Target.DefaultProcedure);
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUI.enabled = Target.ActivatedProcedures.Count > 0;
            if (GUILayout.Button("Set Default", "MiniPopup"))
            {
                GenericMenu gm = new GenericMenu();
                for (int i = 0; i < Target.ActivatedProcedures.Count; i++)
                {
                    int j = i;
                    gm.AddItem(new GUIContent(Target.ActivatedProcedures[j]), Target.DefaultProcedure == Target.ActivatedProcedures[j], () =>
                    {
                        Undo.RecordObject(target, "Set Default Procedure");
                        Target.DefaultProcedure = Target.ActivatedProcedures[j];
                        HasChanged();
                    });
                }
                gm.ShowAsContext();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            for (int i = 0; i < Target.ActivatedProcedures.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label((i + 1) + "." + Target.ActivatedProcedures[i]);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("▲", "MiniButtonleft"))
                {
                    if (i > 0)
                    {
                        Undo.RecordObject(target, "Set Procedure Order");
                        string procedure = Target.ActivatedProcedures[i];
                        Target.ActivatedProcedures.RemoveAt(i);
                        Target.ActivatedProcedures.Insert(i - 1, procedure);
                        HasChanged();
                        continue;
                    }
                }
                if (GUILayout.Button("▼", "MiniButtonmid"))
                {
                    if (i < Target.ActivatedProcedures.Count - 1)
                    {
                        Undo.RecordObject(target, "Set Procedure Order");
                        string procedure = Target.ActivatedProcedures[i];
                        Target.ActivatedProcedures.RemoveAt(i);
                        Target.ActivatedProcedures.Insert(i + 1, procedure);
                        HasChanged();
                        continue;
                    }
                }
                if (GUILayout.Button("Edit", "MiniButtonmid"))
                {
                    if (_procedureTypes.ContainsKey(Target.ActivatedProcedures[i]))
                    {
                        UnityEngine.Object classFile = AssetDatabase.LoadAssetAtPath(_procedureTypes[Target.ActivatedProcedures[i]], typeof(TextAsset));
                        if (classFile)
                            AssetDatabase.OpenAsset(classFile);
                        else
                            GlobalTools.LogError("没有找到 " + Target.ActivatedProcedures[i] + " 脚本文件！");
                    }
                    else
                    {
                        GlobalTools.LogError("没有找到 " + Target.ActivatedProcedures[i] + " 脚本文件！");
                    }
                }
                if (GUILayout.Button("Delete", "minibuttonright"))
                {
                    Undo.RecordObject(target, "Delete Procedure");
                    if (Target.DefaultProcedure == Target.ActivatedProcedures[i])
                    {
                        Target.DefaultProcedure = "";
                    }

                    Target.ActivatedProcedures.RemoveAt(i);

                    if (Target.DefaultProcedure == "" && Target.ActivatedProcedures.Count > 0)
                    {
                        Target.DefaultProcedure = Target.ActivatedProcedures[0];
                    }
                    HasChanged();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Procedure", "MiniPopup"))
            {
                GenericMenu gm = new GenericMenu();
                List<Type> types = GlobalTools.GetTypesInRunTimeAssemblies();
                for (int i = 0; i < types.Count; i++)
                {
                    if (types[i].IsSubclassOf(typeof(Procedure)))
                    {
                        int j = i;
                        if (Target.ActivatedProcedures.Contains(types[j].FullName))
                        {
                            gm.AddDisabledItem(new GUIContent(types[j].FullName));
                        }
                        else
                        {
                            gm.AddItem(new GUIContent(types[j].FullName), false, () =>
                            {
                                Undo.RecordObject(target, "Add Procedure");
                                Target.ActivatedProcedures.Add(types[j].FullName);

                                if (Target.DefaultProcedure == "")
                                {
                                    Target.DefaultProcedure = Target.ActivatedProcedures[0];
                                }
                                HasChanged();
                            });
                        }
                    }
                }
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();
        }
    }
}
