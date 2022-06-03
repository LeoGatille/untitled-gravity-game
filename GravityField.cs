using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GravityField : MonoBehaviour
{
    public float PullRadius;
    public float GravitationalPull;
    public float MinRadius;
    public float DistanceMultiplier;
    public LayerMask LayerToPull;
    // Start is called before the first frame update
    void Start()
    {
        // CustomGravityField();
    }
    private void CustomGravityField()
    {
        Character[] characters = FindObjectsOfType<Character>();
        foreach (var character in characters)
        {
            Collider2D collider = character.GetComponent<Collider2D>();
            Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                continue;
            }

            Vector2 direction = transform.position - collider.transform.position;
            if (direction.magnitude < MinRadius)
            {
                continue;
            }

            float distance = direction.sqrMagnitude * DistanceMultiplier + 1;
            rb.AddForce(direction.normalized * (GravitationalPull / distance) * rb.mass * Time.fixedDeltaTime);
        }
    }
    private void FixedUpdate()
    {
        CustomGravityField();
        // Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, PullRadius, LayerToPull);

        // foreach (var collider in colliders)
        // {
        //     Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
        //     if (rb == null)
        //     {
        //         continue;
        //     }

        //     Vector2 direction = transform.position - collider.transform.position;
        //     if (direction.magnitude < MinRadius)
        //     {
        //         continue;
        //     }

        //     float distance = direction.sqrMagnitude * DistanceMultiplier + 1;
        //     rb.AddForce(direction.normalized * (GravitationalPull / distance) * rb.mass * Time.fixedDeltaTime);
        // }
    }
}
