using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_controler : MonoBehaviour
{
    public float Speed, JumpHeight;
    float VelX, VelY;
    Rigidbody2D Rb;
    public Transform Platcheck;
    public bool IsOnPlat;
    public LayerMask WhatIsPlat;

    private float GravedadOriginal;
    public float SaltoStartTiempo = 0.2f;
    private float SaltoTime;
    public bool IsSalto;

    public bool IsWallDesliz;
    public float DeslizSpeed;
    public Transform Wallcheck;
    public LayerMask WallLayer;

    public bool IsWallSalto;
    private float WallSaltoDir;
    public float WallSaltoTime = 0.2f;
    private float WallSaltoCounter;
    public float WallSaltoDuration = 0.4f;
    public Vector2 WallSaltoPower = new Vector2(25f, 45f);

    public float GravityScale = 10;
    public float CaidaGravityScale = 16;

    //variables para el dash
    private bool CanDash = true;
    public bool IsDash;
    public float FuerzaDash = 22f;
    public float TiempoDash;
    public float CooldownDash;
    private Vector2 DashDir;
    public TrailRenderer Tr;

    // Start is called before the first frame update
    void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        Tr = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //prueba de salto fluido
        if (Rb.velocity.y > 0 && !IsDash)
        {

            Rb.gravityScale = GravityScale;
        }
        else if (!IsDash)
        {
            Rb.gravityScale = CaidaGravityScale;
        }

        Jump();

        IsOnPlat = Physics2D.OverlapCircle(Platcheck.position, 0.2f, WhatIsPlat);

        WallSlide();
        WallSalto();

        if (!IsWallSalto)
        {
            FlipCharacter();
        }

        if (Input.GetButtonDown("Dash") && CanDash && VelX != 0f)
        {
            Dash();
        }
        else if (Input.GetKeyDown(KeyCode.RightShift) && CanDash && VelX != 0f)
        {
            Dash();
        }

        if (IsDash)
        {
            Rb.velocity = DashDir.normalized * FuerzaDash;
            DashDir.Normalize();
            //Debug.Log("Dash: " + DashDir.magnitude);
            return;
        }

        if (IsOnPlat)
        {
            CanDash = true;
        }

    }

    private void FixedUpdate()
    {
        if (!IsWallSalto)
        {
            Movement();
        }

    }

    public void Jump()
    {
        if (Input.GetButtonDown("Jump") && IsOnPlat)
        {
            CanDash = false;
            IsSalto = true;
            SaltoTime = SaltoStartTiempo;
            Rb.velocity = Vector2.up * JumpHeight;
            CanDash = true;
            Debug.Log("Brinque");
        }
        if (Input.GetButton("Jump") && IsSalto)
        {
            CanDash = false;
            if (SaltoTime > 0)
            {
                Rb.velocity = Vector2.up * JumpHeight;
                SaltoTime -= Time.deltaTime;
            }
            else
            {
                IsSalto = false;
            }
            CanDash = true;
        }

        if (Input.GetButtonUp("Jump"))
        {
            IsSalto = false;
        }
    }

    private bool JumpToWall()
    {
        return Physics2D.OverlapCircle(Wallcheck.position, 0.2f, WallLayer);
    }

    private void WallSlide()
    {
        if (JumpToWall() && !IsOnPlat && VelX != 0f)
        {
            IsWallDesliz = true;
            Rb.velocity = new Vector2(Rb.velocity.x, Mathf.Clamp(Rb.velocity.y, -DeslizSpeed, float.MaxValue));
        }
        else
        {
            IsWallDesliz = false;
        }
    }

    private void WallSalto()
    {
        if (IsWallDesliz)
        {
            IsWallSalto = false;
            WallSaltoDir = -transform.localScale.x;
            WallSaltoCounter = WallSaltoTime;

            CancelInvoke(nameof(StopWallSalto));
        }
        else
        {
            WallSaltoCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && WallSaltoCounter > 0f)
        {
            IsWallSalto = true;
            Rb.velocity = new Vector2(WallSaltoDir * WallSaltoPower.x, WallSaltoPower.y);
            WallSaltoCounter = 0f;

            if (transform.localScale.x != WallSaltoDir)
            {
                FlipCharacter();
            }

            Invoke(nameof(StopWallSalto), WallSaltoDuration);
        }
    }

    private void StopWallSalto()
    {
        IsWallSalto = false;
    }

    public void Movement()
    {
        VelX = Input.GetAxisRaw("Horizontal");
        VelY = Rb.velocity.y;

        Rb.velocity = new Vector2(VelX * Speed, VelY);
    }

    public void FlipCharacter()
    {
        if (Rb.velocity.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (Rb.velocity.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(TiempoDash);
        Rb.gravityScale = GravedadOriginal;
        Tr.emitting = false;
        IsDash = false;
        yield return new WaitForSeconds(CooldownDash);
    }

    public void Dash()
    {
        IsDash = true;
        CanDash = false;
        Tr.emitting = true;
        GravedadOriginal = Rb.gravityScale;
        Rb.gravityScale = 0;
        DashDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (DashDir == Vector2.zero)
        {
            DashDir = new Vector2(transform.localScale.x, 0);
        }

        StartCoroutine(StopDash());
    }



}
