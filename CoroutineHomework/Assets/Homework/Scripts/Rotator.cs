using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    private Vector3 _rotate = Vector3.zero;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            Debug.LogError($"[Rotator] На объекте {gameObject.name} отсутствует компонент Rigidbody!");
        }
        else if (!_rigidbody.isKinematic)
        {
            Debug.LogWarning($"[Rotator] Rigidbody на объекте {gameObject.name} не является кинематическим. Вращение через MoveRotation требует isKinematic = true.");
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();

        while (true)
        {
            Quaternion deltaRotation = Quaternion.Euler(
                _rotate.x * Time.fixedDeltaTime,
                _rotate.y * Time.fixedDeltaTime,
                _rotate.z * Time.fixedDeltaTime
            );

            _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);

            yield return new WaitForFixedUpdate();
        }
    }
}
