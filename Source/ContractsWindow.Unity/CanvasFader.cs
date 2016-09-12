﻿#region license
/*The MIT License (MIT)
CanvasFader - Monobehaviour for making smooth fade in and fade out for UI windows

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CanvasFader : MonoBehaviour
	{
		[SerializeField]
		private float YShrinkScale = 1;
		[SerializeField]
		private float XShrinkScale = 1;
		[SerializeField]
		private float SlowRate = 0.9f;
		[SerializeField]
		private float FastRate = 0.3f;

		private CanvasGroup canvas;
		private RectTransform rect;
		private IEnumerator fader;

		protected virtual void Awake()
		{
			canvas = GetComponent<CanvasGroup>();
			rect = GetComponent<RectTransform>();
		}

		public bool Fading
		{
			get { return fader != null; }
		}

		protected void Fade(float to, bool fast, bool scale, bool grow = false, Action call = null)
		{
			if (canvas == null)
				return;

			float xTo = 1;
			float yTo = 1;
			float xFrom = 1;
			float yFrom = 1;

			if (scale)
			{
				xTo = grow ? 1 : XShrinkScale;
				yTo = grow ? 1 : YShrinkScale;

				xFrom = rect.localScale.x;
				yFrom = rect.localScale.y;
			}

			Fade(canvas.alpha, to, fast ? FastRate : SlowRate, call, xTo, yTo, xFrom, yFrom);
		}

		protected void Alpha(float to)
		{
			if (canvas == null)
				return;

			to = Mathf.Clamp01(to);
			canvas.alpha = to;
		}

		private void Scale(float toX, float toY)
		{
			if (rect == null)
				return;

			toX = Mathf.Clamp01(toX);
			toY = Mathf.Clamp01(toY);

			rect.localScale = new Vector3(toX, toY, 1);
		}

		private void Fade(float from, float to, float duration, Action call, float scaleToX, float scaleToY, float scaleFromX, float scaleFromY)
		{
			if (fader != null)
				StopCoroutine(fader);

			fader = FadeRoutine(from, to, duration, call, scaleToX, scaleToY, scaleFromX, scaleFromY);
			StartCoroutine(fader);
		}

		private IEnumerator FadeRoutine(float from, float to, float duration, Action call, float scaleToX, float scaleToY, float scaleFromX, float scaleFromY)
		{
			yield return new WaitForEndOfFrame();

			float f = 0;

			while (f <= 1)
			{
				f += Time.deltaTime / duration;
				Alpha(Mathf.Lerp(from, to, f));
				Scale(Mathf.Lerp(scaleFromX, scaleToX, duration), Mathf.Lerp(scaleFromY, scaleToY, duration));
				yield return null;
			}

			if (call != null)
				call.Invoke();

			fader = null;
		}

	}
}
