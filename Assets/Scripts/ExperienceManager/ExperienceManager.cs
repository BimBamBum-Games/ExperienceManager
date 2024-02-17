using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExperienceManager : MonoBehaviour
{
    #region Required Global Propertys and Fields
    [SerializeField] [Min(1)] int _currentLevel;
    [SerializeField] float _totalExperience = 0;
    [SerializeField][Min(1f)] float _expMultiplier = 1f;
    [SerializeField] float _nextLevelPercent;

    [SerializeField] ExpInfoContainer _expInfoCnt = ExpInfoContainer.GetWithNewList();
    public event EventHandler<ExperienceArgs> OnExperienceChange;
    private ExperienceArgs _experienceArgs = ExperienceArgs.GetDefault();
    #endregion

    #region EventArgs Mvp Event Driven
    public class ExperienceArgs : EventArgs {
        public int level;
        public float experience;

        private ExperienceArgs(int lvl, float exp) {
            level = lvl;
            experience = exp;
        }
        public static ExperienceArgs GetWithParameters(int lvl, float exp) {
            return new ExperienceArgs(lvl, exp);
        }

        public static ExperienceArgs GetDefault() {
            return new ExperienceArgs(0, 0);
        }
    }
    #endregion

    #region Experience System Method Group
    [ContextMenu("Get Experience")]
    private void GetLevelTable() {
        float cumulativeExp = 0;
        _expInfoCnt.experienceTable.Clear();
        for (int i = 0; i <= _currentLevel; i++) {
            float exp = ExpGenerator.GetExponential(i) * _expMultiplier;
            cumulativeExp += exp;
            ExpInfo ef = ExpInfo.Get(i, exp, cumulativeExp);
            _expInfoCnt.experienceTable.Add(ef);
        }
    }

    public void AddExperience(float experienceOut) {
        _totalExperience += experienceOut;
        GetLevelFromExperience();
        _experienceArgs.level = _currentLevel;
        _experienceArgs.experience = _totalExperience;
        OnExperienceChange?.Invoke(this, _experienceArgs);
    }

    [ContextMenu("Get Level From Experience")]
    public void GetLevelFromExperience() {
        _currentLevel = 0;
        float closestBaseExperience = 0;
        int levelStep = 0;

        while (true) {       
            closestBaseExperience = ExpGenerator.GetExponential(levelStep);
            //Ornegin 0 ile 1 arasinda ve sifir da dahil aslýnda level 1e giderken seklinde dusunulur.        
            if (closestBaseExperience > _totalExperience) {
                levelStep--;
                break;
            }
            levelStep++;
        }

        _currentLevel = levelStep;
        Debug.LogWarning("determinorExp > " + closestBaseExperience + " Current Level > " + levelStep + " Total Exp > " + _totalExperience);

        _nextLevelPercent = 1 - (closestBaseExperience - _totalExperience) / (closestBaseExperience - ExpGenerator.GetExponential(_currentLevel));
        GetLevelTable();
    }
    #endregion
}

#region New Wrapper and Container Class To Show on Inspector
[Serializable]
public class ExpInfo {
    public int level;
    public float experience;
    public float totalExperience;
    private ExpInfo(int lvl, float exp, float texp) {
        level = lvl;
        experience = exp;
        totalExperience = texp;
    }
    public static ExpInfo Get(int lvl, float exp, float texp) {
        return new ExpInfo(lvl, exp, texp);
    }
}

[Serializable]
public class ExpInfoContainer {
    public List<ExpInfo> experienceTable;
    private ExpInfoContainer(List<ExpInfo> experienceTable) {
        this.experienceTable = experienceTable;
    }

    public static ExpInfoContainer GetWithReference(List<ExpInfo> experienceTable) {
        return new ExpInfoContainer(experienceTable);
    }

    public static ExpInfoContainer GetWithNewList() {     
        return new ExpInfoContainer(new List<ExpInfo>());
    }
}
#endregion

#region PropertyDrawer Rearrangement of Inspector Property

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ExpInfoContainer))]
public class ExpMonitorDrawer : PropertyDrawer {

    int _level;
    float _experience;
    float _totalExperience;
    int _arraySize;
    bool _isFoldedOut;

    GUIStyle _titleStl, _levelStl, _experStl;
    bool _isInitialized = false;

    public void SetInitialStyles() {
        //PropertyDrawer basesini direk cagir. Buna ek olarak bu tureyen classta initialize islemleri gerceklestir.
        _titleStl = new GUIStyle(EditorStyles.numberField);
        _titleStl.normal.textColor = Color.cyan;

        _levelStl = new GUIStyle(EditorStyles.numberField);
        _levelStl.normal.textColor = Color.green;
        //EditorStyles birer asset ve aslinda Create menuden GUISkin ile olusturulmaktadir. Built-in olan burada islenmektedir.
        _experStl = new GUIStyle(EditorStyles.numberField);
        _experStl.normal.textColor = Color.yellow;
        GUI.color = Color.white;
    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {

        if (_isInitialized == false) {
            SetInitialStyles();
            _isInitialized = true;
        }

        EditorGUI.BeginProperty(pos, label, prop);

        Rect initRct = new(pos.x, pos.y, 0, 0);
        Rect foldOutRct = 
            new Rect(initRct.x,
            initRct.max.y, 
            EditorGUIUtility.labelWidth, 
            EditorGUIUtility.singleLineHeight);

        _isFoldedOut = EditorGUI.Foldout(foldOutRct, _isFoldedOut, label);
        //Debug.Log(_isFoldedOut);

        if (_isFoldedOut) {

            SerializedProperty _expInfoListSrp = prop.FindPropertyRelative("experienceTable");
            //Debug.Log(_expInfoListSrp.arraySize);

            Rect titleRct0 =
                new Rect(pos.x,
                foldOutRct.y + EditorGUIUtility.singleLineHeight,
                pos.width * 0.3f - 3f,
                EditorGUIUtility.singleLineHeight);

            Rect titleRct1 =
                new Rect(pos.width * 0.3f,
                foldOutRct.y + EditorGUIUtility.singleLineHeight,
                pos.width * 0.3f - 3f,
                EditorGUIUtility.singleLineHeight);

            Rect titleRct2 =
                new Rect(pos.width * 0.6f,
                foldOutRct.y + EditorGUIUtility.singleLineHeight,
                pos.width - pos.width * 0.6f,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(titleRct0, "Level", _titleStl);
            EditorGUI.LabelField(titleRct1, "Experience", _titleStl);
            EditorGUI.LabelField(titleRct2, "Cumulative", _titleStl);

            _arraySize = _expInfoListSrp.arraySize;

            for (int i = 0; i < _arraySize; i++) {
                SerializedProperty expInfoSrp = _expInfoListSrp.GetArrayElementAtIndex(i);
                _level = expInfoSrp.FindPropertyRelative("level").intValue;
                _experience = expInfoSrp.FindPropertyRelative("experience").floatValue;
                _totalExperience = expInfoSrp.FindPropertyRelative("totalExperience").floatValue;

                DrawLevelRectengle(titleRct0, i);
                DrawExperienceRectengle(titleRct1, i);
                DrawTotalExperienceRectengle(titleRct2, i);

            }
        }

        EditorGUI.EndProperty();
    }

    public Rect DrawLevelRectengle(Rect pos, int index) {
        Rect rect0 =
            new Rect(pos.x,
            pos.y + 2 + EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight * index + 2 * index,
            pos.width,
            EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(rect0, _level.ToString(), _levelStl);
        return rect0;
    }

    public Rect DrawExperienceRectengle(Rect pos, int index) {
        Rect rect1 =
            new Rect(pos.x,
            pos.y + 2 + EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight * index + 2 * index,
            pos.width,
            EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(rect1, _experience.ToString(), _experStl);
        return rect1;
    }

    public Rect DrawTotalExperienceRectengle(Rect pos, int index) {
        Rect rect2 =
            new Rect(pos.x,
            pos.y + 2 + EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight * index + 2 * index,
            pos.width,
            EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(rect2, _totalExperience.ToString(), _experStl);
        return rect2;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (_isFoldedOut) {
            return 2 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight * _arraySize + 2 * _arraySize;
        }
        else {
            return 2 * EditorGUIUtility.singleLineHeight;
        }      
    }
}
#endif

#endregion