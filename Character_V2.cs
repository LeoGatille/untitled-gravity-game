using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Character_V2 : MonoBehaviour
{

    private Animator animator;
    public Rigidbody2D rb;
    private bool isFacingRight = true;
    public float speed;
    public int jumpPower;
    public float JumpTimeLimit;

    public string characterName = "Coucou toi";
    private Collider2D gravityAncor;
    private bool isFalling;
    private bool isJumping = false;
    private float jumpTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log(animator);
    }

    // Update is called once per frame
    void Update()
    {
        Translate(Input.GetAxis("Horizontal"));
        // ApplyGravity();
        RecordJump();
    }


    // private void FixedUpdate()
    // {
    //     Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, PullRadius, LayerToPull);
    //     foreach (var collider in colliders)
    //     {
    //         Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
    //         if (rb == null) continue;
    //         Vector2 direction = transform.position - collider.transform.position;
    //         if (direction.magnitude < MinRadius) continue;
    //         float distance = direction.sqrMagnitude * DistanceMultiplier + 1;
    //         rb.AddForce(direction.normalized * (GravitationalPull / distance) * rb.mass * Time.fixedDeltaTime);
    //     }
    // }

    private void Translate(float movement)
    {
        if (movement != 0)
        {
            float correctedSpeed = isJumping ? speed / 2 : speed;
            // Debug.Log("movement => " + movement);
            transform.Translate(Vector2.right * movement * Time.deltaTime * correctedSpeed);
            animator.SetBool("isWalking", true);

            if (isFacingRight != (movement > 0)) Flip();
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void RecordJump()
    {
        if (!isJumping && Input.GetButtonDown("Jump"))
        {
            animator.SetBool("isGrounding", false);
            isJumping = true;
            animator.SetBool("isJumping", isJumping);
            jumpTime = 0;
        }

        if (isJumping)
        {
            Jump();
        }

        if (Input.GetButtonUp("Jump") | jumpTime > JumpTimeLimit)
        {
            isJumping = false;
            isFalling = true;
            animator.SetBool("isJumping", isJumping);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        jumpTime += Time.deltaTime;
    }

    private void Flip()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
        isFacingRight = !isFacingRight;
    }

    private void ApplyGravity()
    {
        if (gravityAncor)
        {
            Vector2 direction = gravityAncor.transform.position + transform.position;
            transform.position = direction;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // if (isFalling && other.transform.CompareTag("ground"))
        // {
        //     print("GROUNDING");
        //     isFalling = false;
        //     animator.SetBool("isGrounding", true);
        //     var stopGrounding = new System.Timers.Timer();
        //     stopGrounding.Interval = 500;
        //     stopGrounding.Elapsed += StopGrounding;
        //     stopGrounding.AutoReset = false;
        //     stopGrounding.Enabled = true;
        // }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("gravityField"))
        {
            gravityAncor = other;
        }
    }

#nullable enable
    private void StopGrounding(object? source, ElapsedEventArgs e)
    {
        Debug.Log("STOP GROUNDING");
        animator.SetBool("isGrounding", false);
    }
}
