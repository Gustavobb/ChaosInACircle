using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChaosInACircle : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private RawImage _image;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;

    [Header("Setup")]
    [SerializeField] private int _numCircles = 10;
    [SerializeField] private float _audioMaxCooldown = 0.01f;
    [SerializeField] private float _gravity = 0.1f;
    [SerializeField] private float _mirrorY = 1f;
    [SerializeField] private float _circleSeparation = .0001f;
    [SerializeField] private float _initialXPos = .5f;
    [SerializeField] private float _circleRadius = 0.01f;
    [SerializeField] private float _circumferenceOuterRadius = 0.505f;
    [SerializeField] private float _circumferenceInnerRadius = 0.5f;
    [SerializeField] private float _smoothness = 0.0f;
    [SerializeField] private float _colorDecay = 0.01f;

    private Vector4[] _centers = new Vector4[100];
    private Vector4[] _velocities = new Vector4[100];
    private Color[] _colors = new Color[100];
    private float[] _radius = new float[100];

    private float _audioCooldown = 0;
    [SerializeField] private RenderTexture _previousFrameTex;
    [SerializeField] private TextMeshProUGUI _text;
    private bool _hasTrail = true;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Reset();
    }

    private void Update()
    {
        _text.text = $"{_numCircles} Circles";

        if (Input.GetKeyDown(KeyCode.Space))
            Reset();
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            _numCircles = Random.Range(1, 30);
            _circleRadius = Random.Range(0.005f, 0.03f);
            _gravity = Random.Range(0.1f, 0.7f);
            _initialXPos = Random.Range(0.2f, 0.7f);
            _circleSeparation = Random.Range(0.0001f, 0.00015f);
            _mirrorY = Random.Range(0f, 1f) > 0.5f ? 1 : 0;
            Reset();
        }

        if (Input.GetKeyDown(KeyCode.M))
            _audioSource.mute = !_audioSource.mute;
        
        if (Input.GetKeyDown(KeyCode.T))
            _hasTrail = !_hasTrail;
    }

    private void FixedUpdate()
    {
        float aspect = (float)Screen.width / (float)Screen.height;
        _material.SetFloat("_Aspect", aspect);
        _material.SetInt("_NumCircles", _numCircles);
        _material.SetVector("_CircumferenceRadius", new Vector2(_circumferenceInnerRadius, _circumferenceOuterRadius));
        _material.SetFloat("_Smoothness", _smoothness);
        _material.SetFloat("_ColorDecay", _hasTrail ? _colorDecay : 1);
        _material.SetFloat("_MirrorY", _mirrorY);

        UpdateCircles();
    }

    private void CreateRenderTexture()
    {
        if (_previousFrameTex != null)
            _previousFrameTex.Release();

        _previousFrameTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        _previousFrameTex.Create();
        _material.SetTexture("_PrevFrame", _previousFrameTex);
    }

    private void CreateCircle(int k)
    {
        float xOffset = _initialXPos + (k - _numCircles / 2) * _circleSeparation;
        _centers[k] = new Vector4(xOffset, .5f, 0, 0);
        _velocities[k] = Vector4.zero;
        _colors[k] = Random.ColorHSV(0f, 1f, .5f, 1f, 1f, 1f);
        _radius[k] = _circleRadius;
    }

    [EButton]
    private void Reset()
    {
        CreateRenderTexture();
        StartCoroutine(ResetCoroutine());
        
        _centers = new Vector4[100];
        _velocities = new Vector4[100];
        _colors = new Color[100];
        _radius = new float[100];

        for (int k = 0; k < _numCircles; k++)
            CreateCircle(k);
    }

    private IEnumerator ResetCoroutine()
    {
        _material.SetFloat("_Reset", 1);
        yield return new WaitForSeconds(.1f);
        _material.SetFloat("_Reset", 0);
    }

    private void UpdateCircle(int k)
    {
        Vector4 velocity = _velocities[k];
        Vector4 center = _centers[k];
        float radii = _radius[k];

        velocity.y -= _gravity * Time.fixedDeltaTime;

        Vector4 circumferenceCenter = new Vector4(0.5f, 0.5f, 0, 0);
        Vector4 toCenter = center - circumferenceCenter;
        float pos = toCenter.x * toCenter.x + toCenter.y * toCenter.y;
        float radius = _circumferenceInnerRadius - radii * 1.5f;

        if (pos >= radius * radius)
        {
            velocity = Vector2.Reflect(velocity, toCenter.normalized);
            
            if (_audioCooldown <= 0)
            {
                _audioSource.pitch = Random.Range(0.8f, 1.2f);
                _audioSource.PlayOneShot(_audioClip);
            }

            _audioCooldown = _audioMaxCooldown;
        }

        center += velocity * Time.fixedDeltaTime;
        _centers[k] = center;
        _velocities[k] = velocity;
    }

    private void UpdateCircles()
    {
          for (int k = 0; k < _numCircles; k++)
            UpdateCircle(k);

        _audioCooldown -= Time.fixedDeltaTime;
        _material.SetVectorArray("_Centers", _centers);
        _material.SetColorArray("_Colors", _colors);
        _material.SetFloatArray("_Radius", _radius);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, _material);
        Graphics.Blit(destination, _previousFrameTex);
    }
}