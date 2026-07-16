using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private float speed = 15f;
    public float MinSpeed = 15f;
    public float MaxSpeed = 25f;
    public float JumpForce = 10f;
    public float CaerForce = 5f;

    //CAMARA
    public float Sensibility = 2f;
    public float LimitX = 45;
    public Transform cam;

    private float rotationX;
    private float rotationY;

    //SUELO
    public bool IsGrounded;
    public int MaxJumps = 2;
    public int MinJumpParticle = 1;
    private int jumpCount;

    public Transform GroundCheck;          // position to check ground (assign in inspector)
    public float GroundDistance = 0.3f;    // radius / distance for ground check
    public LayerMask GroundLayer;          // which layers count as ground
    public float MaxSlopeAngle = 45f;      // max angle to be considered ground

    // Coyote time (buffer para saltar al salir de un borde)
    public float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;

    //ParticleSystems (asignar en inspector)
    public ParticleSystem JumpParticles;
    public ParticleSystem LandParticles;

    // para detectar aterrizaje y reproducir partículas
    private bool wasGrounded = false; 

    private Vector3 groundNormal = Vector3.up;
    
    private bool IsWin;

    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        jumpCount = 0;
        wasGrounded = IsGrounded;
    }

    // Update is called once per frame
    void Update()
    {
        //MOVIMIENTO
        if (!IsWin && !UIManager.inst.Pause)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = MaxSpeed;
            }
            else
            {
                speed = MinSpeed;
            }

            // Actualizar estado de suelo (spherecast para bordes e inclinaciones)
            UpdateGroundedStatus();

            //CAMARA
            rotationX += -Input.GetAxis("Mouse Y") * Sensibility;
            rotationX = Mathf.Clamp(rotationX, -LimitX, LimitX);
            cam.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * Sensibility, 0);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIManager.inst.ShowPauseScreen();
            }

            //salto
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            transform.Translate(new Vector3(x, 0, y) * Time.deltaTime * speed);

            //caida
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                FastFall();
            }


            // Coyote timer
            if (IsGrounded) coyoteTimer = coyoteTime;
            else coyoteTimer -= Time.deltaTime;

            // Reset de saltos solo al aterrizar
            if (IsGrounded)
            {
                jumpCount = 0;
            }

            // Detectar aterrizaje (transición a grounded)
            if (!wasGrounded && IsGrounded)
            {
                // reproducir partículas de aterrizaje si existen
                if (LandParticles != null)
                {
                    LandParticles.Play();
                }
                // reset de saltos al aterrizar
                jumpCount = 0;
            }
            wasGrounded = IsGrounded;

            
        }

    }


    //SALTO
    public void Jump()
    {
        // Permitir salto si hay saltos restantes o estamos en coyote time
        if (jumpCount < MaxJumps || coyoteTimer > 0f)
        {
            // Normalizar la velocidad vertical antes de aplicar el impulso para saltos consistentes
            Vector3 v = rb.velocity;
            v.y = 0f;
            rb.velocity = v;

            // Si estamos en suelo (o coyote) usamos la normal del suelo para dar un salto mas natural en pendientes
            Vector3 jumpDir = Vector3.up;
            if (IsGrounded)
            {
                jumpDir = (Vector3.up + groundNormal).normalized;
            }

            rb.AddForce(jumpDir * JumpForce, ForceMode.Impulse);

            jumpCount++;
            coyoteTimer = 0f; // consumir coyote

            // reproducir partículas del segundo salto
            if (jumpCount <= MinJumpParticle && JumpParticles != null)
            {
                JumpParticles.Play();
            }
        }
    }


    //CAIDA RAPIDA
    public void FastFall()
    {
        if (!IsGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, -CaerForce, rb.velocity.z);
        }
    }


    //Colisiones
    public void OnCollisionEnter(Collision collision)
    {
        // Mantenemos la comprobacion de colision con tags que tenias
        if (collision.gameObject.tag == "Ground")
        {
            // Usaremos OnCollisionStay para normal promedio, pero aqui aseguramos grounded inmediato
            IsGrounded = true;
            coyoteTimer = coyoteTime;
            jumpCount = 0;
        }

        if (collision.gameObject.tag == "End" && !IsWin)
        {
            UIManager.inst.ShowWinScreen();
            IsWin = true;
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            // no marcar inmediatamente como no grounded; dejamos que el Update y coyote time manejen transiciones
            IsGrounded = false;
        }
    }

    // Helper: comprobacion de suelo robusta (spherecast) y calculo de normal para pendientes
    private void UpdateGroundedStatus()
    {
        if (GroundCheck == null)
        {
            // fallback: mantener la variable anterior
            return;
        }

        IsGrounded = false;
        groundNormal = Vector3.up;

        RaycastHit hit;
        float radius = Mathf.Max(0.01f, GroundDistance * 0.5f);
        float maxDistance = GroundDistance + 0.05f;

        // SphereCast hacia abajo desde GroundCheck
        if (Physics.SphereCast(GroundCheck.position, radius, Vector3.down, out hit, maxDistance, GroundLayer, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle <= MaxSlopeAngle)
            {
                IsGrounded = true;
                groundNormal = hit.normal;
            }
        }
        else
        {
            // adicional: comprobar pequeños contactos con OverlapSphere (util en bordes)
            Collider[] cols = Physics.OverlapSphere(GroundCheck.position, radius + 0.02f, GroundLayer, QueryTriggerInteraction.Ignore);
            if (cols.Length > 0)
            {
                // intentar raycast hacia cada collider para obtener normal aceptable
                foreach (var col in cols)
                {
                    Vector3 dir = (GroundCheck.position - col.ClosestPoint(GroundCheck.position)).normalized;
                    if (Physics.Raycast(GroundCheck.position, Vector3.down, out hit, maxDistance + 0.1f, GroundLayer, QueryTriggerInteraction.Ignore))
                    {
                        float angle = Vector3.Angle(hit.normal, Vector3.up);
                        if (angle <= MaxSlopeAngle)
                        {
                            IsGrounded = true;
                            groundNormal = hit.normal;
                            break;
                        }
                    }
                }
            }
        }
    }
}