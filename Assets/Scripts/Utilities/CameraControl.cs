using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
	[Header("ÊÂ¼þ¼àÌý")]
	public VoidEvent afterSceneLoadedEvent;

    private CinemachineConfiner2D Confiner2D;
	public CinemachineImpulseSource impulseSource;

	public VoidEvent cameraShakeEvent;

	private void Awake()
	{
		Confiner2D = GetComponent<CinemachineConfiner2D>();
	}

	private void OnEnable()
	{
		cameraShakeEvent.OnEventRaised += OnCameraShakeEvent;
		afterSceneLoadedEvent.OnEventRaised += onAfterSceneLoadedEvent;
	}

	private void OnDisable()
	{
		cameraShakeEvent.OnEventRaised -= OnCameraShakeEvent;
		afterSceneLoadedEvent.OnEventRaised -= onAfterSceneLoadedEvent;
	}

	private void onAfterSceneLoadedEvent()
	{
		GetNewCameraBounds();
	}

	private void OnCameraShakeEvent()
	{
		impulseSource.GenerateImpulse();
	}

	//private void Start()
	//{
	//	GetNewCameraBounds();
	//}

	private void GetNewCameraBounds()
	{
		var obj = GameObject.FindGameObjectWithTag("Bounds");
		if (obj == null) 
			return;

		Confiner2D.m_BoundingShape2D = obj.GetComponent<Collider2D>();
		Confiner2D.InvalidateCache();
	}
}
