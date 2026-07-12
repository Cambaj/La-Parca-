using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movimiento")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxFallVelocity;

    [Header("Detecci�n de suelo")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;

    [Header("Salto variable")]
    [SerializeField] private float lowJumpMultiplier;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float peakHoverThreshold = 2f;
    [SerializeField] private float peakHoverGravity = 0.2f;
    [SerializeField] private float normalGravityScale = 1f;
    [Header("Jump Buffer & Coyote Time")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.15f;

    [Header("Grapping Hook")]
    [SerializeField] private bool unlockedGrapple;
    [SerializeField] private float grappleMaxDistance = 10f;
    [SerializeField] private float grappleSpeed = 20f;
    [SerializeField] private float grappleDuration = 3f;
    private float grappleTimer;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LineRenderer grappleline;
    [SerializeField] private GameObject grappleObject;

    //guadana animacion grapple
    [SerializeField] private GameObject grappleLaunchSprite;
    [SerializeField] private GameObject grappleThreadSprite;
    [SerializeField] private GameObject grapplePointSprite;
    private bool isPreparingGrapple = false;

    private Vector2 grapplePoint;
    private bool isGrappling = false;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    private float horizontal;
    private float vertical;
    //Mando Solo
    private float horizontalRightStick;
    private float verticalRightStick;
    private bool grounded;
    private Rigidbody2D rb;

    private bool facingRight = true;

    private Vector2 wallNormal;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = 10f;
    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallSlideTime;
    [SerializeField] private float wallSlideTimeMax;
    private bool isHoldingGrab;
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForceX = 20f;
    [SerializeField] private float wallJumpForceY = 10f;
    private bool isWallJumping;
    private float wallJumpTimer;
    [SerializeField] private float wallJumpDuration = 0.25f;

    [Header("Dash")]
    [SerializeField] private bool unlockedDash;
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    private Vector2 dashVelocity;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTime;
    private bool canGrapple = true;
    private bool hasJumped = false;
    
    //No borrar estas variables, se van a usar despues 
    [Header("GrappleCorector")]
    [SerializeField] private bool TopRight;
    [SerializeField] private bool TopLeft;
    [SerializeField] private bool BottomRight;
    [SerializeField] private bool BottomLeft;


    //Mecanica de granada 
    [Header("Mecanica de Granada")]
    [SerializeField] private  Vector2 granadeLaunchForce = new Vector2(10f, 5f);
    private Granade equippedGranade;
    private bool hasGranade = false;

    //Soul
    [Header("Mecanica de Soul Bone")]
    [SerializeField] private GameObject soulBonePrefab;
    [SerializeField] private Transform soulBoneSpawn;
    [SerializeField] private HungrySoul hungrySoul;

    [SerializeField] private float soulBoneCooldown = 5f;

    private bool hasSoulBone = true;
    private float soulBoneTimer;

    //Portal
    [HideInInspector] public bool externalLaunch;
    [HideInInspector] public float externalLaunchTime;
    [HideInInspector] private float cooldownTP = 0;
    [HideInInspector] public bool canTP = true;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource walkAudio;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip grappleLaunchSound;
    [SerializeField] private AudioClip grappleHookSound;
    [SerializeField] private AudioClip grappleRecoverSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip walkGrassSound;
    [SerializeField] private AudioClip throwBoneSound;
    [SerializeField] private AudioClip dieSound;

    private bool isDead = false;

    private Vector2 controllerAim;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    Animator anim;
    
    //public Transform spawnPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        //spawnPoint = GameObject.Find("SpawnPoint").transform;
        canTP = true;

        if (grapplePointSprite != null)
        {
            grapplePointSprite.transform.SetParent(null);
            grapplePointSprite.SetActive(false);
        }
        if (grappleLaunchSprite != null)
        {
            grappleLaunchSprite.transform.SetParent(null);
            grappleLaunchSprite.SetActive(false);
        }
        if (grappleThreadSprite != null)
        {
            grappleThreadSprite.transform.SetParent(null);
            grappleThreadSprite.SetActive(false);
        }
    }

    void Update()
    {
        if (isDead) return;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

       // horizontalRightStick = Input.GetAxisRaw("LeftStickHorizontal"); //Stick izquierdo en realidad. Para usar stick derecho "RightStickHorizontal"
       // verticalRightStick = Input.GetAxisRaw("LeftStickVertical");// Stick izquierdo en realidad: Para usar stick derecho: "RightStickVertical"

        controllerAim = new Vector2(horizontalRightStick, verticalRightStick);

        if (controllerAim.magnitude < 0.2f)
        {
            controllerAim = Vector2.zero;
        }
        else
        {
            controllerAim.Normalize();
        }


        // Ground Detection
        bool groundCenter = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        bool groundLeft = Physics2D.Raycast(new Vector2(transform.position.x - 0.35f, transform.position.y), Vector2.down, groundCheckDistance, groundLayer);
        bool groundRight = Physics2D.Raycast(new Vector2(transform.position.x + 0.32f, transform.position.y), Vector2.down, groundCheckDistance, groundLayer);

        grounded = groundCenter || groundLeft || groundRight;

        // Coyote time
        if (grounded && !isGrappling && !isDashing && !isDead && canTP)
        {
            coyoteTimeCounter = coyoteTime;
            wallSlideTime = wallSlideTimeMax;
            canDash = true;
            canGrapple = true;
            hasJumped = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump buffer
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // -- Normal Jump --
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !hasJumped && Time.timeScale == 1f)
        {
            DoJump(Vector2.up);
            hasJumped = true;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(jumpSound);
        }

        // -- WALL SLIDE --
        isHoldingGrab = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton4);

        if (isTouchingWall && !grounded && wallSlideTime > 0 && isHoldingGrab && !isWallJumping)
        {
            isWallSliding = true;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                // Direcci�n opuesta a la pared
                float jumpDirectionX = wallNormal.x;

                // Resetear velocidad
                rb.linearVelocity = Vector2.zero;

                // Fuerza del wall jump
                Vector2 wallJumpForce = new Vector2(jumpDirectionX * wallJumpForceX, wallJumpForceY);

                rb.AddForce(wallJumpForce, ForceMode2D.Impulse);

                // Flip visual
                if ((jumpDirectionX > 0 && facingRight) || (jumpDirectionX < 0 && !facingRight))
                {
                    Flip();
                }

                // Salir del wall slide
                isWallSliding = false;
                isTouchingWall = false;

                // Evita salto doble inmediato
                coyoteTimeCounter = 0;
                jumpBufferCounter = 0;

                isWallJumping = true;
                wallJumpTimer = wallJumpDuration;

                if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(jumpSound);

                return;
            }
            // -- Go Up/Down while wall sliding --
            if (vertical > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideSpeed);
            }
            else if (vertical < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed * 0);
            }
            wallSlideTime -= Time.deltaTime;
        }
        else
        {
            isWallSliding = false;
        }

        if (isDashing)
        {
            isWallSliding = false;
        }

        // -- DASH --
        if (unlockedDash == true)
        {
            if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton2)) && canDash && !isGrappling && (horizontal != 0 || vertical != 0) && !isPreparingGrapple && !isDead)
            {
                canDash = false;
                isDashing = true;
                dashTime = dashDuration;

                rb.linearVelocity = Vector2.zero;

                Vector2 dashDirection = new Vector2(horizontal, vertical);

                if (dashDirection == Vector2.zero)
                {
                    dashDirection = facingRight ? Vector2.right : Vector2.left;
                }

                dashDirection.Normalize();

                dashVelocity = dashDirection * dashForce;

                //Animaciones
                int dashType = 0;

                if (!grounded)
                {
                    if (vertical < 0 && horizontal == 0)
                        dashType = 1; // Down
                    else if (vertical > 0 && horizontal == 0)
                        dashType = 2; // Up
                    else if (vertical > 0 && horizontal != 0)
                        dashType = 3; // Diagonal Up
                    else if (vertical < 0 && horizontal != 0)
                        dashType = 4; // Diagonal Down
                    else
                        dashType = 5; // Forward
                }
                else
                {
                    if (vertical > 0 && horizontal == 0)
                        dashType = 6; // Up
                    else if (vertical > 0 && horizontal != 0)
                        dashType = 7; // Diagonal Up
                    else
                        dashType = 8; // Forward
                }

                anim.SetInteger("DashType", dashType);
                anim.SetBool("IsDashing", true);

                rb.linearVelocity = dashVelocity;

                if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(dashSound);
            }
            if (isDashing)
            {
                rb.gravityScale = 0;
                rb.linearVelocity = dashVelocity;
                dashTime -= Time.deltaTime;

                if (dashTime <= 0)
                {
                    isDashing = false;
                    rb.linearVelocity = new Vector2(0, 0);
                    rb.gravityScale = 1;

                    anim.SetBool("IsDashing", false);
                    anim.SetInteger("DashType", 0);
                }
            }
        }

        if (!isDashing && !isGrappling && !isPreparingGrapple && !externalLaunch && !isDead)
        {
            // Caso 1: Cayendo (Ca�da r�pida y pesada)
            if (rb.linearVelocity.y < 0)
            {
                rb.gravityScale = normalGravityScale * fallMultiplier;
            }
            // Caso 2: En el pico del salto (Efecto Suspensi�n / Hover)
            else if (rb.linearVelocity.y > 0 && Mathf.Abs(rb.linearVelocity.y) < peakHoverThreshold)
            {
                rb.gravityScale = peakHoverGravity;
            }
            // Caso 3: Solt� el bot�n de salto antes de tiempo (Corta el salto r�pido)
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.JoystickButton1))
            {
                rb.gravityScale = normalGravityScale * lowJumpMultiplier;
            }
            // Caso 4: Subida normal inicial o en el suelo
            else
            {
                rb.gravityScale = normalGravityScale;
            }
        }
        // FLIP PLAYER
        if (horizontal < 0 && !facingRight && Time.timeScale != 0 && !isDead)
        {
            Flip();
        }
        else if (horizontal > 0 && facingRight && Time.timeScale != 0 && !isDead)
        {
            Flip();
        }

        if (!isDashing && !isGrappling && !isDead)
        {
            // ANIMACIONES y sonido de caminar
            if (horizontal != 0)
            {
                anim.SetBool("IsWalking", true);

                if (!walkAudio.isPlaying && grounded)
                {
                    walkAudio.PlayOneShot(walkGrassSound);
                }
            }
            else
            {
                anim.SetBool("IsWalking", false);
            }

            // Animaciones de salto y caída
            if (rb.linearVelocity.y > 0.1f && !grounded)
            {
                anim.SetBool("IsJump", true);
                anim.SetBool("IsFall", false);
            }
            else if (rb.linearVelocity.y < -0.1f && !grounded)
            {
                anim.SetBool("IsFall", true);
                anim.SetBool("IsJump", false);
            }
            else if (grounded)
            {
                anim.SetBool("IsFall", false);
                anim.SetBool("IsJump", false);
            }
        }

        if (unlockedGrapple == true)
        {
            if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.JoystickButton5)) && canGrapple && !isDashing && !isGrappling && !isDead)
            {
                StartGrapple();
            }

            if (isGrappling)
            {
                grappleTimer -= Time.deltaTime;

                if (grappleTimer <= 0f)
                {
                    StopGrapple();
                    return;
                }

                grappleline.SetPosition(0, new Vector3(transform.position.x, transform.position.y, 0));
                grappleline.SetPosition(1, new Vector3(grapplePoint.x, grapplePoint.y, 0));

                if (grapplePointSprite != null)
                {
                    grapplePointSprite.SetActive(true);
                    grapplePointSprite.transform.position = new Vector3(grapplePoint.x, grapplePoint.y, 0f);
                }

                if (grappleThreadSprite != null && grappleThreadSprite.activeSelf)
                {
                    // Mantiene el hilo en el punto medio exacto entre el jugador y el objetivo
                    grappleThreadSprite.transform.position = ((Vector2)transform.position + grapplePoint) / 2f;
                }

                if (Vector2.Distance(transform.position, grapplePoint) < 2f)
                {
                    grappleThreadSprite.SetActive(false);
                }

                if (Vector2.Distance(transform.position, grapplePoint) < 0.7f)
                {
                    StopGrapple();
                }
            }
        }


        //Input de la granada 

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton7)) && hasGranade)
        {
            ThrowEquippedGranade();
        }

        //Soul bone
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton7)) && !hasGranade)
        {
            ThrowSoulBone();
        }

        if (hasSoulBone)
        {
            soulBoneTimer -= Time.deltaTime;
        }

        if (wallSlideTime <= 0f)
        {
            spriteRenderer.color = Color.red;
        }
        else if (wallSlideTime <= 2f)
        {
            spriteRenderer.color = Color.cyan;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }

        if (unlockedGrapple == true)
        {
            if (grappleObject != null)
            {
                grappleObject.SetActive(canGrapple);
            }
        }


        if (OpcionesManager.PermisoSaltarEscena)
        {
            //  AVANZAR NIVEL: Enter del teclado numérico o Botón X del mando
            if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.JoystickButton11))
            {
                int siguienteIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (siguienteIndex < SceneManager.sceneCountInBuildSettings)
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(siguienteIndex);
                    Debug.Log(" [CHEAT PLAYER] Saltando al siguiente nivel de forma lineal.");
                }
            }

            //  RETROCEDER NIVEL: Tecla Retroceso (Backspace) o Botón Y del mando
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.JoystickButton10))
            {
                int anteriorIndex = SceneManager.GetActiveScene().buildIndex - 1;
                if (anteriorIndex >= 0)
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(anteriorIndex);
                    Debug.Log(" [CHEAT PLAYER] Volviendo al nivel anterior.");
                }
            }
        }

        if (isDead) return;

    }


    private void FixedUpdate()
    {
        if (isDead) return;
        //fall max velocity
        if (rb.linearVelocityY < -maxFallVelocity)
        {
            rb.linearVelocityY = -maxFallVelocity;
        }

        //Portal
        if (externalLaunch)
        {
            externalLaunchTime -= Time.fixedDeltaTime;

            if (externalLaunchTime <= 0)
            {
                externalLaunch = false;
            }

            return;
        }

        if (canTP == false)
        {
            cooldownTP += Time.deltaTime;

            if (cooldownTP >= 1.2f)
            {
                canTP = true;
                cooldownTP = 0f;
            }
        }


        if (isWallJumping)
        {
            wallJumpTimer -= Time.fixedDeltaTime;

            if (wallJumpTimer <= 0)
            {
                isWallJumping = false;
            }
        }

        if (isDashing) return;

        if (isGrappling)
        {
            Vector2 direction = (grapplePoint - (Vector2)transform.position).normalized;

            rb.linearVelocity = direction * grappleSpeed;

            anim.SetBool("IsGrappling", true);

            DestroyBonesWhileGrappling();
        }
        else
        {
            if (isWallSliding)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else if (!isDead)
            {
                if (isWallJumping)
                {
                    // Dejamos que la fuerza del impulso físico trabaje sola en el Rigidbody
                }
                else
                {
                    // 🛹 CONTROL DE INERCIA EN EL AIRE
                    // Si el jugador está volando (por ejemplo, después del Wall Jump),
                    // la velocidad horizontal se suaviza en lugar de cortarse a 0 inmediatamente.
                    if (!grounded)
                    {
                        float targetVelocityX = horizontal * speed * Time.fixedDeltaTime;

                        // El valor '5f' controla qué tan rápido recuperas la respuesta en el aire.
                        // Puedes subirlo a 7f o 8f si prefieres un control más rígido tras los 3 tiles.
                        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, Time.fixedDeltaTime * 5f), rb.linearVelocity.y);
                    }
                    else
                    {
                        // Movimiento responsivo e instantáneo normal cuando pisas el suelo
                        rb.linearVelocity = new Vector2(horizontal * speed * Time.fixedDeltaTime, rb.linearVelocity.y);
                    }
                }
            }
            anim.SetBool("IsGrappling", false);
        }

    }

    //Aqui sigue la accion de la granada
    private void ThrowEquippedGranade()
    {
        if (equippedGranade == null) return;
        Vector2 throwDirection = Vector2.zero;
        //Intenter apuntar con el joystick derecho
        if (controllerAim != Vector2.zero)
        {
            throwDirection = controllerAim;
        }
        else //Si no hay jostcick apunta con el Mouse 
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            throwDirection = (mousePos - (Vector2)transform.position).normalized;
        }
        
        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;
        float rad = snappedAngle * Mathf.Deg2Rad;

        Vector2 finalDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        float totalForce = granadeLaunchForce.x;

        Vector2 FinalImpulse = finalDirection * totalForce;

        FinalImpulse.y += granadeLaunchForce.y; 

        equippedGranade.Throw(FinalImpulse);

        equippedGranade = null;
        hasGranade = false;
    }

    private void ThrowSoulBone()
    {
        if (!hasSoulBone)
            return;

        if (soulBoneTimer > 0)
            return;

        GameObject bone =
            Instantiate(
                soulBonePrefab,
                soulBoneSpawn.position,
                Quaternion.identity);

        if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(throwBoneSound);

        SoulBone soulBone =
            bone.GetComponent<SoulBone>();

        soulBone.Initialize(hungrySoul);

        hasSoulBone = false;
        soulBoneTimer = soulBoneCooldown;
    }

    public void RecoverSoulBone()
    {
        hasSoulBone = true;

        canDash = true;
        canGrapple = true;

        if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(grappleRecoverSound);
    }

    public void Die()
    {
        //  ESCUDO COMPLETO TRUCO INMORTALIDAD (Sincronizado con PlayerPrefs y managers estáticos)
        if (PlayerPrefs.GetInt("SaveInmortal", 0) == 1 || OpcionesManager.CheatInmortalidad || (LevelManager.instance != null && LevelManager.instance.cheatInmortal)) return;

        if (isDead) return;
        isDead = true;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(0, 0);
        if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(dieSound);
        anim.SetTrigger("IsDeath");
    }

    public void ForceToDropGranade()
    {
        equippedGranade = null;
        hasGranade = false;
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void DoJump(Vector2 direction)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
    }

    private void StartGrapple()
    {
        Vector2 direction;

        if (controllerAim != Vector2.zero)
        {
            direction = controllerAim;

            float controllerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float snappedControllerAngle = Mathf.Round(controllerAngle / 45f) * 45f;
            float controllerRad = snappedControllerAngle * Mathf.Deg2Rad;

            direction = new Vector2(Mathf.Cos(controllerRad), Mathf.Sin(controllerRad));
        }
        else
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 rawDirection = (mousePos - (Vector2)transform.position).normalized;

            float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;
            float rad = snappedAngle * Mathf.Deg2Rad;

            direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleMaxDistance, grappleLayer);

        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(grappleHookSound);
        }
        else
        {
            grapplePoint = (Vector2)transform.position + direction * grappleMaxDistance;
            if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(grappleLaunchSound);
        }

        float rotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        StartCoroutine(GrappleAnimationSequence(rotAngle));
    }

    public void StopGrapple()
    {
        isGrappling = false;
        grappleline.enabled = false;

        if (grapplePointSprite != null) grapplePointSprite.SetActive(false);
        if (grappleLaunchSprite != null) grappleLaunchSprite.SetActive(false);
        if (grappleThreadSprite != null) grappleThreadSprite.SetActive(false);

        rb.linearVelocity = new Vector2(0, 0);
        rb.gravityScale = 1;
    }

    private System.Collections.IEnumerator GrappleAnimationSequence(float rotAngle)
    {
        // --- INICIO DE PREPARACIÓN ---
        isPreparingGrapple = true;
        canGrapple = false;

        // Jugador en Gravedad 0 e inmóvil
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        // Fase 1: Aparece el objeto de lanzamiento
        if (grappleLaunchSprite != null)
        {
            grappleLaunchSprite.SetActive(true);

            float offsetDistance = 2f; // Cambia este valor para alejarlo más o menos

            // Calculamos la dirección unitaria basada en el ángulo
            Vector2 launchDirection = new Vector2(Mathf.Cos(rotAngle * Mathf.Deg2Rad), Mathf.Sin(rotAngle * Mathf.Deg2Rad));

            // Posicionamos el objeto sumando el desfase a la posición del jugador
            grappleLaunchSprite.transform.position = (Vector2)transform.position + (launchDirection * offsetDistance);
            // ---------------------------------------

            grappleLaunchSprite.transform.rotation = Quaternion.Euler(0, 0, rotAngle);
        }

        // Esperamos exactamente 3 frames
        yield return null;
        if (isDead) yield break; // Seguridad por si muere durante la animación
        yield return null;
        yield return null;

        // Fase 2: Aparece el hilo entre el jugador y el punto
        if (grappleThreadSprite != null)
        {
            grappleThreadSprite.SetActive(true);

            // Si el pivot (centro) de tu sprite está en la base (donde toca al jugador):
            grappleThreadSprite.transform.position = ((Vector2)transform.position + grapplePoint) / 2f;

            grappleThreadSprite.transform.rotation = Quaternion.Euler(0, 0, rotAngle);
        }
        // --- FASE 3: INICIA EL GRAPPLE REAL ---
        isPreparingGrapple = false;
        isGrappling = true;
        grappleTimer = grappleDuration;
        canGrapple = false;
        grappleline.enabled = false;

        if (grapplePointSprite != null)
        {
            grapplePointSprite.SetActive(true);
            grapplePointSprite.transform.position = new Vector3(grapplePoint.x, grapplePoint.y, 0f);
            grapplePointSprite.transform.rotation = Quaternion.Euler(0, 0, rotAngle);
        }

        // Esperamos 3 frames más
        yield return null;
        if (isDead) yield break;
        yield return null;
        if (grappleLaunchSprite != null)
        {
            grappleLaunchSprite.SetActive(false);
        }
        yield return null;
    }

    //Usada por Portal
    public void StopDash()
    {
        isDashing = false;
        dashTime = 0f;

        rb.gravityScale = 1;
        rb.linearVelocity = Vector2.zero;
    }

    //usada por DashGhost
    public bool CanDash()
    {
        return canDash;
    }
    
    private void DestroyBoneWall(GameObject bone)
    {
        if (bone == null || !bone.CompareTag("Bone"))
            return;

        Collider2D boneCollider = bone.GetComponent<Collider2D>();

        if (boneCollider == null)
            return;

       ContactFilter2D filter = new ContactFilter2D();
       filter.NoFilter();

        Collider2D[] results = new Collider2D[20];

        int count = boneCollider.Overlap(filter, results);

        Destroy(bone);

        for (int i = 0; i < count; i++)
        {
            if (results[i] != null &&
                results[i].CompareTag("Bone") &&
                results[i].gameObject != bone)
            {
                DestroyBoneWall(results[i].gameObject);
            }
        }
    }
    private void DestroyBonesWhileGrappling()
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        Collider2D[] results = new Collider2D[20];
        int count = playerCollider.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            if (results[i] != null && results[i].CompareTag("Bone"))
            {
                DestroyBoneWall(results[i].gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Damage"))
        {
            // ESCUDO COMPLETO TRUCO INMORTALIDAD (Colisiones)
            if (PlayerPrefs.GetInt("SaveInmortal", 0) == 1 || OpcionesManager.CheatInmortalidad || (LevelManager.instance != null && LevelManager.instance.cheatInmortal)) return;

            isDead = true;
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(0, 0);
            anim.SetTrigger("IsDeath");
        }

        if ((isDashing || isGrappling) && collision.gameObject.CompareTag("Bone"))
        {
            Collider2D playerCollision = GetComponent<Collider2D>();
            Collider2D boneCollision = collision.collider;

            Physics2D.IgnoreCollision(playerCollision, boneCollision, true);
            DestroyBoneWall(collision.gameObject);
            rb.linearVelocity = dashVelocity;
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isWallJumping) return; 

        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Bone"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
               
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    isTouchingWall = true;
                    wallNormal = contact.normal;
                    return;
                }
            }
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Granade") && !hasGranade)
        {
            Granade granadeground = collision.GetComponent<Granade>();
            if (granadeground != null && !granadeground.WasThrown())
            {
                equippedGranade = granadeground;
                hasGranade = true;

                equippedGranade.PickUp(transform);

                if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(grappleRecoverSound);
            }
        }

        if (collision.CompareTag("Entity"))
        {
            canDash = true;
            canGrapple = true;
            if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(grappleRecoverSound);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Estancia"))
        {

            // Buscamos la tarjeta de identificación de la meta actual
            MetaNivel infoMeta = collision.GetComponent<MetaNivel>();

            if (infoMeta != null)
            {
                // 2. Le ordenamos a la meta que procese y guarde el progreso en PlayerPrefs
                infoMeta.GuardarProgreso();

                // (Opcional) Si en el futuro usas el LevelManager global, puedes descomentar tu línea:
                // if (LevelManager.instance != null) { ... }
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Bone"))
        {
            isTouchingWall = false;
        }

    }

    //Usada por la animacion de Death
    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);

        Gizmos.DrawLine(new Vector3(transform.position.x - 0.35f, transform.position.y, transform.position.z), new Vector3(transform.position.x - 0.35f, transform.position.y - groundCheckDistance, transform.position.z));
        // Derecho
        Gizmos.DrawLine(new Vector3(transform.position.x + 0.32f, transform.position.y, transform.position.z), new Vector3(transform.position.x + 0.32f, transform.position.y - groundCheckDistance, transform.position.z));

    }
   

}