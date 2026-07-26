// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class SimplePlayerController : MonoBehaviour
    {
        public float MoveSpeed = 2f;

        private void Update()
        {
            Vector2 move = Vector2.zero;
            if (Input.GetKey(KeyCode.W)) {
                move.y = 1f;
            }
            else
            if (Input.GetKey(KeyCode.S)) {
                move.y = -1f;
            }

            if (Input.GetKey(KeyCode.A)) {
                move.x = -1f;
            }
            else
            if (Input.GetKey(KeyCode.D)) {
                move.x = 1f;
            }

            move.Normalize();
            float speed = MoveSpeed * Time.deltaTime;
            Vector3 position = new Vector3(move.x * speed, 0f, move.y * speed);
            transform.Translate(position, Space.Self);
        }
    }
}
