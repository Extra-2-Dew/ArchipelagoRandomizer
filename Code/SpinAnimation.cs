using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public class SpinAnimation : MonoBehaviour
    {
        // 0 = x, 1 = y, 2 = z
        public int axis = 1;

        private float startY;
        private Vector3 turnAmount;

        private const float speed = 5;

        private void Start()
        {
            startY = transform.position.y;

            switch (axis)
            {
                case 0:
                    turnAmount = Vector3.right;
                    break;
                case 1:
                    turnAmount = Vector3.up;
                    break;
                case 2:
                    turnAmount = Vector3.forward;
                    break;
            }
        }

        private void FixedUpdate()
        {
            transform.localEulerAngles += turnAmount * speed;
            float nextY = startY + (Mathf.Cos(Time.timeSinceLevelLoad * 3) * 0.2f);
            transform.position = new(transform.position.x, nextY, transform.position.z);
        }
    }
}
