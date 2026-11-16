using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private Vector3 _start;
    [SerializeField] private Vector3 _end;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _delay = 2f;

    private Rigidbody _rigidbody;
    private Vector3 _worldStart;
    private Vector3 _worldEnd;

    private IEnumerator Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            Debug.LogError($"Отсутствует Rigidbody на объекте {name}");
            yield break;
        }

        _rigidbody.isKinematic = true;
        _worldStart = transform.TransformPoint(_start);
        _worldEnd = transform.TransformPoint(_end);

        while (true)
        {
            yield return MoveTo(_worldStart, _worldEnd);
            yield return new WaitForSeconds(_delay);
            yield return MoveTo(_worldEnd, _worldStart);
            yield return new WaitForSeconds(_delay);
        }
    }

    private IEnumerator MoveTo(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        if (distance == 0f)
        {
            yield break;
        }

        float travelTime = distance / _speed;
        float timer = 0f;

        while (timer < travelTime)
        {
            yield return new WaitForFixedUpdate();

            timer += Time.fixedDeltaTime;
            float pathTraveled = Mathf.Clamp01(timer / travelTime);
            _rigidbody.MovePosition(Vector3.Lerp(from, to, pathTraveled));
        }

        _rigidbody.MovePosition(to);
    }

    private void OnDrawGizmos()
    {
        Vector3 start = Application.isPlaying ? _worldStart : transform.TransformPoint(_start);
        Vector3 end = Application.isPlaying ? _worldEnd : transform.TransformPoint(_end);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(start, 0.2f);
        Gizmos.DrawSphere(end, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, end);

        Gizmos.color = Color.white;
    }
}
