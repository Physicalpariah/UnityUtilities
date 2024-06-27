using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class GKPinchRecognizer : GKAbstractGestureRecognizer {
	public event Action<GKPinchRecognizer> gestureRecognizedEvent;
	public event Action<GKPinchRecognizer> gestureCompleteEvent;

	public float deltaScale = 0;
	private float _intialDistance;
	private float _previousDistance;


	private float distanceBetweenTrackedTouches() {
		return Vector2.Distance(_trackingTouches[0].position, _trackingTouches[1].position);
	}


	internal override void fireRecognizedEvent() {
		if (gestureRecognizedEvent != null) {
			Debug.Log("GKLog: recogniser all geezy, firing");
			gestureRecognizedEvent(this);
		}
		else {
			Debug.Log("GKLog: recogniser not subscribed, bailing");
		}
	}


	internal override void touchesBegan(List<GKTouch> touches) {
		if (state == GKGestureRecognizerState.Possible) {
			Debug.Log("GKLog: touches began on pinch");
			// we need to have two touches to work with so we dont set state to Begin until then
			// latch the touches
			for (int i = 0; i < touches.Count; i++) {
				// only add touches in the Began phase
				if (touches[i].phase == TouchPhase.Began) {
					_trackingTouches.Add(touches[i]);

					if (_trackingTouches.Count == 2)
						break;
				}
			} // end for

			if (_trackingTouches.Count == 2) {
				Debug.Log("GKLog: two touches detected, we should be good.");
				deltaScale = 0;

				Debug.Log("1: " + _trackingTouches[0].position);
				Debug.Log("2: " + _trackingTouches[1].position);
				_intialDistance = distanceBetweenTrackedTouches();
				_previousDistance = _intialDistance;
				state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
			}
		}
		else {
			Debug.Log("GKLog: state is not possible: " + state);
		}
	}


	internal override void touchesMoved(List<GKTouch> touches) {
		if (state == GKGestureRecognizerState.RecognizedAndStillRecognizing) {
			var currentDistance = distanceBetweenTrackedTouches();
			deltaScale = (currentDistance - _previousDistance) / _intialDistance;
			_previousDistance = currentDistance;
			state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
		}
	}


	internal override void touchesEnded(List<GKTouch> touches) {
		// remove any completed touches
		for (int i = 0; i < touches.Count; i++) {
			if (touches[i].phase == TouchPhase.Ended)
				_trackingTouches.Remove(touches[i]);
		}

		// if we had previously been recognizing fire our complete event
		if (state == GKGestureRecognizerState.RecognizedAndStillRecognizing) {
			if (gestureCompleteEvent != null)
				gestureCompleteEvent(this);
		}

		// if we still have a touch left continue to wait for another. no touches means its time to reset
		if (_trackingTouches.Count == 1) {
			state = GKGestureRecognizerState.Possible;
			deltaScale = 1;
		}
		else {
			state = GKGestureRecognizerState.Failed;
		}
	}


	public override string ToString() {
		return string.Format("[{0}] state: {1}, deltaScale: {2}", this.GetType(), state, deltaScale);
	}

}
