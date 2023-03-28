using UnityEngine;

public class CloudSimulation : MonoBehaviour
{
    [SerializeField] private Transform _container;

    public Vector3 MinBounds
    {
        get
        {
            if (_container == null)
            {
                return new Vector3(-2048, 500, -2048);
            }

            return _container.position - _container.localScale / 2f;
        }
    }
    public Vector3 MaxBounds
    {
        get
        {
            if (_container == null)
            {
                return new Vector3(2048, 100, 2048);
            }

            return _container.position + _container.localScale / 2f;
        }
    }
}
