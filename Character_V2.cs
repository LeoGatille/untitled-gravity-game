using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Character_V2 : MonoBehaviour
{

    private Animator animator;
    public string characterName = "Coucou toi";
    private Collider2D gravityAncor;
    private bool isJumping = false;
    private float jumpTime = 0;
    private bool isAnchored;
    private Vector2 normal;
    private Vector2 collisionPosition;
    private Vector2 contactPoint;

    public float GravitationalPull;
    public Rigidbody2D rb;
    private bool isFacingRight = true;
    public float speed;
    public int jumpPower;
    public float JumpTimeLimit;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gravityAncor != null)
        {
            rb.gravityScale = 0;
            Debug.DrawLine(transform.position, gravityAncor.ClosestPoint(transform.position));
        }
        else
        {
            rb.gravityScale = 1;
        }
        Translate(Input.GetAxis("Horizontal"));
        RecordJump();
    }

    private void FixedUpdate()
    {
        if (isAnchored) StickToPlateform();
    }

    private void StickToPlateform()
    {
        if (!isAnchored || isJumping || gravityAncor == null) return;

        Vector2 v2Postion = transform.position;
        //? Vector2 direction = (transform.position - gravityAncor.transform.position);
        Vector2 direction = v2Postion - gravityAncor.ClosestPoint(transform.position);
        // Debug.DrawLine(gravityAncor.transform.position, transform.position, color: Color.gray, Mathf.Infinity);
        // float distance = direction.sqrMagnitude;
        // print("force : " + direction.normalized * (GravitationalPull / distance) * Time.fixedDeltaTime);
        // rb.AddForce(direction.normalized * -1000 * Time.fixedDeltaTime);
        rb.velocity = direction.normalized * -1000 * Time.fixedDeltaTime;
        // rb.AddForce((direction.normalized * (GravitationalPull / distance) * Time.fixedDeltaTime * -1));
    }

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
        if (isAnchored && Input.GetButtonDown("Jump"))
        {
            isJumping = true;
            jumpTime = 0;

            //! Legacy animation management
            animator.SetBool("isGrounding", false);
            animator.SetBool("isJumping", isJumping);
        }

        if (isJumping)
        {
            Jump();
        }

        if (Input.GetButtonUp("Jump") | jumpTime > JumpTimeLimit)
        {
            isJumping = false;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("gravityField"))
        {
            isAnchored = true;
            gravityAncor = other.transform.parent.gameObject.GetComponent<Collider2D>();

        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.CompareTag("gravityField"))
        {
            isAnchored = false;
            gravityAncor = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.CompareTag("platform"))
        {
            // rb.velocity = new Vector2(0, 0);
            // normal = other.contacts[0].normal;
            // collisionPosition = other.transform.position;
            // contactPoint = other.contacts[0].point;
            // isAnchored = true;
            // gravityAncor = other.collider;
            // OnDrawGizmos();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, normal);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector2(0, 0), normal);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(collisionPosition, normal);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(contactPoint, normal);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(contactPoint, transform.position);


    }

    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     isAnchored = !other.transform.CompareTag("platform");
    //     gravityAncor = null;
    // }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("gravityField"))
    //     {
    //         gravityAncor = other;
    //     }
    // }

#nullable enable
    private void StopGrounding(object? source, ElapsedEventArgs e)
    {
        Debug.Log("STOP GROUNDING");
        animator.SetBool("isGrounding", false);
    }
}
