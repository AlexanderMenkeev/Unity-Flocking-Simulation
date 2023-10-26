using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AllBirdsManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    public GameObject birdPrefab;
    public List<GameObject> birds;
    
    [Range(0f, 1f)] private float _coherence;
    [Range(0f, 1f)] private float _separation;
    [Range(0f, 1f)] private float _alignment;
    [Range(0f, 1f)] private float _visualRange;
    [Range(0f, 15f)] private float _maxSpeed;
    [Range(0f, 1f)] private float _zoomSpeed;

    [Range(0, 150)] private int _numberOfBirds;
    public TMP_Text numOfBirdsText;
    
    [SerializeField] private Slider coherenceSlider;
    [SerializeField] private Slider separationSlider;
    [SerializeField] private Slider alignmentSlider;
    [SerializeField] private Slider visualRangeSlider;
    
    
    private void Awake()
    {
        GameObject cameraGameObject = GameObject.FindWithTag("MainCamera");
        _camera = cameraGameObject.GetComponent<Camera>();
        Application.targetFrameRate = 60;
        
        InitializeSliders();
        InitializeInfo();
        UpdateSliderValues();
        InstantiateBirds(_numberOfBirds);
    }
    private void InitializeSliders()
    {
        coherenceSlider.onValueChanged.AddListener((value) => { _coherence = value; UpdateBirdInfo(); });
        separationSlider.onValueChanged.AddListener((value) => { _separation = value; UpdateBirdInfo(); });
        alignmentSlider.onValueChanged.AddListener((value) => { _alignment = value; UpdateBirdInfo(); });
        visualRangeSlider.onValueChanged.AddListener((value) => { _visualRange = value; UpdateBirdInfo(); });
    }
    private void InitializeInfo()
    {
        birds = new List<GameObject>();
        _coherence = 0.001f;
        _separation = 0.05f;
        _alignment = 0.0001f;
        _visualRange = 1f;
        _maxSpeed = 15;
        _numberOfBirds = 50;
        _zoomSpeed = 0.01f;
    }
    private void UpdateSliderValues()
    {
        coherenceSlider.value = _coherence;
        separationSlider.value = _separation;
        alignmentSlider.value = _alignment;
        visualRangeSlider.value = _visualRange;
    }
    private void InstantiateBirds(int num)
    {
        Vector2 p11 = _camera.ViewportToWorldPoint(new Vector3(1, 1, _camera.nearClipPlane));
        Vector2 p00 = _camera.ViewportToWorldPoint(new Vector3(0, 0, _camera.nearClipPlane));
        for (int i = 0; i < num; i++)
        {
            Vector3 pos = new Vector3(Random.Range(p00.x, p11.x), Random.Range(p00.y, p11.y), 0);
            GameObject bird = Instantiate(birdPrefab, pos, Quaternion.identity, transform);
            birds.Add(bird);
        }
        UpdateBirdInfo();
    }
    private void UpdateBirdInfo()
    {
        foreach (var bird in birds)
        {
            var birdScript = bird.GetComponent<BirdScript>();
            
            birdScript.coherence = _coherence;
            birdScript.separation = _separation;
            birdScript.alignment = _alignment;
            birdScript.maxSpeed = _maxSpeed;
            bird.GetComponent<CircleCollider2D>().radius = _visualRange * 10f;
        }
    }
    public void ResetBirds()
    {
        foreach (GameObject bird in birds.ToList())
        {
            Destroy(bird);
            birds.Remove(bird);
        }
        InitializeInfo();
        UpdateSliderValues();
        InstantiateBirds(_numberOfBirds);
    }
    public void AddBirds()
    {
        InstantiateBirds(5);
    }
    public void RemoveBirds()
    {
        int birdsToRemove = 5;
        foreach (GameObject bird in birds.Take(birdsToRemove).ToList())
        {
            Destroy(bird);
            birds.Remove(bird);
        }
    }

    private void MoveCamera()
    {
        // Pinch to zoom
        if (Input.touchCount == 2)
        {
            // get current touch positions
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            
            // get touch position from the previous frame
            Vector2 touch0Previous = touch0.position - touch0.deltaPosition;
            Vector2 touch1Previous = touch1.position - touch1.deltaPosition;

            float oldTouchDistance = Vector2.Distance (touch0Previous, touch1Previous);
            float currentTouchDistance = Vector2.Distance (touch0.position, touch1.position);

            // get offset value
            float deltaDistance = oldTouchDistance - currentTouchDistance;


            float size = _camera.orthographicSize;
            size += deltaDistance * _zoomSpeed;
            _camera.orthographicSize = Mathf.Clamp(size, 4, 15);
        }
    }

    private bool _touched = false;
    private Touch[] _touchArr;
    private void Update()
    {
        numOfBirdsText.text = $"{birds.Count} birds";
        
        if (Input.touchCount > 0)
        {
            _touchArr = Input.touches;
            _touched = true;
        }
        else
            _touched = false;
        
        if (Input.GetKeyDown(KeyCode.Escape)) 
        { 
            Application.Quit(); 
        }
    }
    
    private void FixedUpdate()
    {
        if (_touched) 
        {
            MoveCamera();
            foreach (var touch in _touchArr)
            {
                Vector2 touchPos = _camera.ScreenToWorldPoint(touch.position);
                Vector2 p11 = _camera.ViewportToWorldPoint(new Vector3(1, 1, _camera.nearClipPlane));
                Vector2 p00 = _camera.ViewportToWorldPoint(new Vector3(0, 0, _camera.nearClipPlane));
                p00 += new Vector2(1.5f, 1.5f);
                p11 -= new Vector2(1.5f, 1.5f);

                if (touchPos.y > p00.y && touchPos.y < p11.y)
                {
                    foreach (var bird in birds)
                    {
                        if (Vector2.Distance(touchPos, bird.transform.position) < 10f)
                        {
                            Vector2 vel = (touchPos - (Vector2)bird.transform.position).normalized;
                            bird.GetComponent<Rigidbody2D>().velocity -= vel;
                        }
                    }
                }
            }
            
            
        }
    }
    
    
}
