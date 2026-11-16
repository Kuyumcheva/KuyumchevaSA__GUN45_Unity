using System;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(PositionSaver))]
    public class ReplayMover : MonoBehaviour
    {
        private PositionSaver _save;

        private int _index;
        private PositionSaver.Data _prev;
        private float _duration;

        private void Start()
        {
            ////todo comment: зачем нужны эти проверки?
            // Ответ: если компонента PositionSaver нет или список записей пуст — воспроизводить нечего. Без проверки код бы упал с ошибкой при обращении к Records.
            if (!TryGetComponent(out _save) || _save.Records.Count == 0)
            {
                Debug.LogError("Records incorrect value", this);
                //todo comment: Для чего выключается этот компонент?
                // Ответ: чтобы Update больше не тикал. Нет данных — значит, нет и работы и не будет спама ошибок каждый кадр.
                enabled = false;
            }
        }

        private void Update()
        {
            var curr = _save.Records[_index];
            //todo comment: Что проверяет это условие (с какой целью)? 
            // Ответ: пора ли переходить к следующей точке. Когда игровое время дошло до метки текущей записи, она становится «прошлой», а счётчик двигается к следующей.
            if (Time.time > curr.Time)
            {
                _prev = curr;
                _index++;
                //todo comment: Для чего нужна эта проверка?
                // Ответ: защита от выхода за пределы списка. Последняя точка проиграна значит, воспроизведение закончено: выключаемся и пишем «finished».

                if (_index >= _save.Records.Count)
                {
                    enabled = false;
                    Debug.Log($"<b>{name}</b> finished", this);
                }
            }
            //todo comment: Для чего производятся эти вычисления (как в дальнейшем они применяются)?
            // Ответ: это вычисление прогресса отрезка — числа от 0 до 1. Оно показывает, какая доля времени между прошлой и текущей точками уже прошла.
            // Это нужно чтобы в Lerp найти промежуточную позицию объекта.

            var delta = (Time.time - _prev.Time) / (curr.Time - _prev.Time);
            //todo comment: Зачем нужна эта проверка?
            // Ответ: если метки прошлой и текущей точек совпали,в делении получится деление на ноль и delta станет NaN.
            // NaN ломает Lerp, поэтому его заменяют нулём.

            if (float.IsNaN(delta)) delta = 0f;
            //todo comment: Опишите, что происходит в этой строчке так подробно, насколько это возможно
            // Ответ: Vector3.Lerp берёт две точки — прошлую и текущую — и по значению delta
            // (0..1) возвращает промежуточную позицию между ними: 0 — у прошлой точки,
            // 1 — у текущей, 0.5 — ровно посередине. Результат записывается в transform.position,
            // то есть объект плавно движется по записанной траектории.

            transform.position = Vector3.Lerp(_prev.Position, curr.Position, delta);
        }
    }
}