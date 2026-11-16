using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class PositionSaver : MonoBehaviour
    {
        [System.Serializable]
        public struct Data
        {
            public Vector3 Position;
            public float Time;
        }

        [ReadOnly]
        [Tooltip("Для заполнения этого поля нужно воспользоваться контекстным меню в инспекторе и командой «Create File».")]
        [SerializeField] private TextAsset _json;

        [HideInInspector]
        public List<Data> Records { get; private set; }

        private string GetFilePath()
        {
            string fileName = (_json != null) ? _json.name : "Path";
            return Path.Combine(Application.dataPath, fileName + ".json");
        }

        private void Awake()
        {
            //todo comment: Что будет, если в теле этого условия не сделать выход из метода?
            // Ответ: код пойдёт дальше и попытается прочитать _json.text, а он null — будет ошибка NullReferenceException.
            // return прерывает метод, чтобы дальше не выполнялось ничего.
            if (_json == null)
            {
                gameObject.SetActive(false);
                Debug.LogError("Please, create TextAsset and add in field _json", this);
                return;
            }

            var path = GetFilePath();
            if (!File.Exists(path))
            {
                Records = new List<Data>(10);
                return;
            }

            string json = File.ReadAllText(path);
            var container = JsonUtility.FromJson<PositionContainer>(json);
            Records = container?.records ?? new List<Data>(10);

            //todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
            // Ответ: подстраховка от null. Если в файле нет записей или чтение вернуло пустоту, Records останется null
            // и любой код (ReplayMover, Gizmos) сломается при обращении к нему.
            // Проверка защищает от случая «JSON прочитался, но внутри оказалось ничего» — и вместо битого null всегда оставляет рабочий, пусть и пустой, список.
            if (Records == null)
                Records = new List<Data>(10);
        }

        private void OnDrawGizmos()
        {
            //todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
            // Ответ: чтобы не рисовать ничего по пустому или несозданному списку. Records == null
            // (например, до создания файла) или Count == 0 (в файле нет точек) — нарисовать нечего,
            // и без проверки на первом же обращении к data[0] будет ошибка NullReferenceException

            if (Records == null || Records.Count == 0) return;
            var data = Records;
            var prev = data[0].Position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(prev, 0.3f);
            //todo comment: Почему итерация начинается не с нулевого элемента?
            // Ответ: нулевая точка уже нарисована (DrawWireSphere перед циклом) и объявлена как prev.
            // Линию можно вести только от предыдущей точки к следующей, а у data[0] предыдущей нет.

            for (int i = 1; i < data.Count; i++)
            {
                var curr = data[i].Position;
                Gizmos.DrawWireSphere(curr, 0.3f);
                Gizmos.DrawLine(prev, curr);
                prev = curr;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Create File")]
        private void CreateFile()
        {
            //todo comment: Что происходит в этой строке?
            // Ответ: создаётся пустой файл Path.txt в папке Assets проекта.
            var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
            //todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав) 
            // Ответ: эта строка «отпускает» файл, чтобы Unity мог его заметить. Без неё файл как будто остаётся открытым и в проекте он не появится, даже после Refresh.
            stream.Dispose();
            UnityEditor.AssetDatabase.Refresh();
            //В Unity можно искать объекты по их типу, для этого используется префикс "t:"
            //После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
            var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
            foreach (var guid in guids)
            {
                //Этой командой можно получить путь к ассету через его гуид
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                //Этой командой можно загрузить сам ассет
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                //todo comment: Для чего нужны эти проверки?
                // Ответ: asset != null отсекает не загрузившиеся ассеты, а asset.name == "Path" отбирает именно нужный файл, а не все TextAsset подряд.

                if (asset != null && asset.name == "Path")
                {
                    _json = asset;
                    UnityEditor.EditorUtility.SetDirty(this);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    //todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
                    // Ответ: нужный ассет уже найден и назначен в _json. Дальше перебирать бессмысленно.
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            if (_json == null || Records == null || Records.Count == 0)
                return;

            var container = new PositionContainer { records = Records };
            string json = JsonUtility.ToJson(container, true);
            File.WriteAllText(GetFilePath(), json);

            Debug.Log("Позиции сохранены в файл: " + GetFilePath());
        }

#endif
        [System.Serializable]
        private class PositionContainer
        {
            public List<Data> records;
        }
    }
}