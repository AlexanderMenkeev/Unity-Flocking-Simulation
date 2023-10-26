using System.Collections.Generic;
using UnityEngine;

public class BirdScript : MonoBehaviour
{
    [Range(0f, 1f)] public float coherence;
    [Range(0f, 1f)] public float separation;
    [Range(0f, 1f)] public float alignment;
    [Range(0f, 15f)] public float maxSpeed;
    
    public GameObject allBirds;
    private Rigidbody2D _rigidbody;
    [SerializeField] private List<Rigidbody2D> visibleBirdsRbs;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        visibleBirdsRbs = new List<Rigidbody2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        allBirds = GameObject.Find("AllBirds");
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D bird = other.GetComponent<Rigidbody2D>();
        visibleBirdsRbs.Add(bird);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D bird = other.GetComponent<Rigidbody2D>();
        visibleBirdsRbs.Remove(bird);
    }
    
    private void SteerTowardsCenter()
    {
        float centeringFactor = 100f;
        Vector2 center = new Vector2(0,0);
        int numNeighbors = 0;
        foreach (Rigidbody2D rb in visibleBirdsRbs)
        {
            center += rb.position;
            numNeighbors++;
        }

        if (numNeighbors != 0)
        {
            center /= (float)numNeighbors;
            _rigidbody.velocity += centeringFactor * coherence * (center - _rigidbody.position);
        }
        
    }

    private void LimitSpeed()
    {
        _rigidbody.velocity += 0.5f*((float)visibleBirdsRbs.Count / (float)allBirds.transform.childCount) * _rigidbody.velocity.normalized;
        
        float speed = _rigidbody.velocity.magnitude;
        if (speed > maxSpeed)
            _rigidbody.velocity = _rigidbody.velocity.normalized * maxSpeed;
        
    }

    private void StayWithinBounds()
    {
        Vector2 p11 = _camera.ViewportToWorldPoint(new Vector3(1, 1, _camera.nearClipPlane));
        Vector2 p00 = _camera.ViewportToWorldPoint(new Vector3(0, 0, _camera.nearClipPlane));

        float turnFactor = 0.6f;
        float margin = 3.5f;

        if (_rigidbody.position.x > p11.x - margin)
            _rigidbody.velocity += Vector2.left * turnFactor;

        if (_rigidbody.position.x < p00.x + margin)
            _rigidbody.velocity += Vector2.right * turnFactor;

        if (_rigidbody.position.y > p11.y - margin)
            _rigidbody.velocity += Vector2.down * turnFactor;

        if (_rigidbody.position.y < p00.y + margin)
            _rigidbody.velocity += Vector2.up * turnFactor;
    }

    private void AvoidOthers()
    {
        Vector2 moveV = new Vector2(0,0);
        float avoidFactor = 100f;
        float minPossibleDistance = 0.5f;
        foreach (Rigidbody2D rbBoid in visibleBirdsRbs)
        {
            float distance = Vector2.Distance(rbBoid.position, _rigidbody.position);
            if (distance < minPossibleDistance)
                moveV += _rigidbody.position - rbBoid.position;
        }

        _rigidbody.velocity += moveV * (avoidFactor * separation);
    }

    private void AlignVelocities()
    {
        float matchingFactor = 100f;
        Vector2 avgVel = new Vector2(0, 0);
        int numNeighbors = 0;

        foreach (Rigidbody2D rb in visibleBirdsRbs)
        {
            avgVel += rb.velocity;
            numNeighbors++;
        }

        if (numNeighbors != 0)
        {
            avgVel /= (float)numNeighbors;
            _rigidbody.velocity += (avgVel - _rigidbody.velocity) * (matchingFactor * alignment);
        }
        
    }
    
    private void FixedUpdate()
    {
        transform.up = _rigidbody.velocity;
        SteerTowardsCenter();
        AvoidOthers();
        AlignVelocities();
        StayWithinBounds();
        LimitSpeed();
    }

    
}
