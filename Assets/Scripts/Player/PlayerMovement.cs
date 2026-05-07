using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Si no somos los dueños de este jugador, apagamos su cámara
        if (!IsOwner)
        {
            GetComponentInChildren<Camera>().enabled = false;
        }
    }

    void Update()
    {
        // LA REGLA DE ORO: Solo el dueño mueve a este personaje
        if (!IsOwner) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 newPosition = rb.position + move * speed * Time.deltaTime;
        rb.MovePosition(newPosition);

        if (Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}