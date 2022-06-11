using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionPredictor
{
    private float gravity;
    private Rigidbody2D rb;
    public Vector2 origin;

    public PositionPredictor(Vector2 origin, Rigidbody2D rb, float gravity)
    {
        this.origin = origin;
        this.rb = rb;
        this.gravity = gravity;
    }

    public void updateRefs(Vector2 origin, Rigidbody2D rb, float gravity)
    {
        this.origin = origin;
        this.rb = rb;
        this.gravity = gravity;
    }

    public Vector2[] GetFuturePositions()
    {

        Vector2[] positions = new Vector2[31];

        float lowestTimeValue = MaxTimeX() / 30;

        for (int i = 0; i < positions.Length; i++)
        {
            float currentTimeValue = lowestTimeValue * i;
            positions[i] = CalculatePositionPoint(currentTimeValue);
        }

        return positions;
    }

#nullable enable
    public Vector2 HitPosition()
    {

        float lowestTimeValue = MaxTimeY() / 15;

        for (int i = 1; i < 16; i++)
        {
            float currentIterationTime = lowestTimeValue * i;
            float nextIterationTime = lowestTimeValue * (i + 1);

            RaycastHit2D hit = Physics2D.Linecast(CalculatePositionPoint(currentIterationTime), CalculatePositionPoint(nextIterationTime));
            if (hit)
            {
                return hit.point;
            }
        }

        return CalculatePositionPoint(MaxTimeY());
    }

    private Vector2 CalculatePositionPoint(float time)
    {
        float x = rb.velocity.x * time;
        float y = (rb.velocity.y * time) - (gravity * Mathf.Pow(time, 2) / 2);

        return new Vector2(x + origin.x, y + origin.y);
    }

    private float MaxTimeY()
    {
        float v = rb.velocity.y;
        float vv = v * v;

        return (v + Mathf.Sqrt(vv + 2 * gravity * (origin.y - -8))) / gravity;
    }
    private float MaxTimeX()
    {
        var x = rb.velocity.x;
        return (HitPosition().x - origin.x) / x;
    }



}

//-------------------------
//- Collision detection tuto script
//-------------------------
/**
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryController : MonoBehaviour
{
    [Header("Line renderer veriables")]
    public LineRenderer line;
    [Range(2, 30)]
    public int resolution;

    [Header("Formula variables")]
    public Vector2 velocity;
    public float yLimit;
    private float g;

    [Header("Linecast variables")]
    [Range(2, 30)]
    public int linecastResolution;
    public LayerMask canHit;

    private void Start()
    {
        g = Mathf.Abs(Physics2D.gravity.y);
    }

    private void Update()
    {
        RenderArc();
    }

    private void RenderArc()
    {
        line.positionCount = resolution + 1;
        line.SetPositions(CalculateLineArray());
    }

    private Vector3[] CalculateLineArray()
    {
        //* Génere un liste de Vecor2
        Vector3[] lineArray = new Vector3[resolution + 1];

        //* Détermine le temps minimum passé sur X
        var lowestTimeValue = MaxTimeX() / resolution;

        for (int i = 0; i < lineArray.Length; i++)
        {
            //* Détermine la position de la ligne en fonction du minimum fois son index
            var t = lowestTimeValue * i;
            //* Récupère le point<Vector2> de la ligne actuelle en lui passant la valeur du temps passé sur X
            lineArray[i] = CalculateLinePoint(t);
        }

        return lineArray;
    }

    private Vector2 HitPosition()
    {
        //* Détermine le temps minimum passé  sur Y
        var lowestTimeValue = MaxTimeY() / linecastResolution;

        for (int i = 0; i < linecastResolution + 1; i++)
        {
            //* Détermine le temp passé sur Y pour cette itération
            var t = lowestTimeValue * i;
            //* Détermine le temp passé sur Y pour la prochaine itération
            var tt = lowestTimeValue * (i + 1);

            //* Check si un impact va se produir entre cette itération et la prochaine
            var hit = Physics2D.Linecast(CalculateLinePoint(t), CalculateLinePoint(tt), canHit);

            if (hit)
            {
                //* Si oui le point d'impact est renvoyé
                return hit.point;
            }
        }

        //* Si non le dernier point que peut atteindre le rb sans collision est retourné
        return CalculateLinePoint(MaxTimeY());
    }

    private Vector3 CalculateLinePoint(float t)
    {
        //* Le déplacement sur x est égale à la vélocité sur X fois le temps passé
        float x = velocity.x * t;

        //* Le déplacement sur y est égale à la vélocité sur Y fois le temps passé
        //* Moins la valeur de la gravité fois le temps passé au carré (FOR REASONS...)
        //* Divisé par 2 (FOR REASONS...) 
        float y = (velocity.y * t) - (g * Mathf.Pow(t, 2) / 2);

        //* Retourne le Vector2 issue du cumule de la position Actuel du RB + les facteurs de déplacement
        return new Vector3(x + origin.x, y + origin.y);
    }

    private float MaxTimeY()
    {
        var v = velocity.y;
        var vv = v * v;

        var t = (v + Mathf.Sqrt(vv + 2 * g * (origin.y - yLimit))) / g;
        return t;
    }

    private float MaxTimeX()
    {
        //* Récup la vélocité X du rb
        var x = velocity.x;
        if (x == 0) //* Ne peut être égale à 0 (FOR REASON...)
        {
            velocity.x = 000.1f;
            x = velocity.x;
        }
        //* Récup la valeur du temps (le maxTimeX ne peut être supérieur à HitPosition qui retourne le point d'impact si la valeur max sur X ne peut être atteinte)
        var t = (HitPosition().x - origin.x) / x;
        return t;
    }
}
*/