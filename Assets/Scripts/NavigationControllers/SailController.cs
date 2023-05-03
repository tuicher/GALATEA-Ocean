using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OceanSystem
{
    public class SailController : MonoBehaviour
    {
        Transform pole;
        [SerializeField] float speed;

        void Awake()
        {
            pole = GameObject.Find("TopSailPole").GetComponent<Transform>();
        }

        void Update()
        {
            pole.position = pole.position + pole.up * -speed * Time.deltaTime;
        }
    }
}
