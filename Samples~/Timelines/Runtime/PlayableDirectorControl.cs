using ToolkitEngine;
using UnityEngine;
using UnityEngine.Playables;

namespace Mobula
{
	[RequireComponent(typeof(PlayableDirector))]
	[RequireComponent(typeof(TimedCurve))]
	public class PlayableDirectorControl : MonoBehaviour
	{
		#region Fields

		[SerializeField, Min(0f)]
		private float m_speed = 1f;

		private PlayableDirector m_playableDirector;
		private TimedCurve m_timedCurve;

		#endregion

		#region Properties

		public float speed
		{
			get => m_speed;
			set
			{
				// No change, skip
				if (m_speed == value)
					return;

				m_speed = value;
				m_timedCurve.duration = GetTimedCurveDuration();
			}
		}

		public TimedCurve timedCurve => m_timedCurve;

		#endregion

		#region Methods

		private void Awake()
		{
			m_playableDirector = GetComponent<PlayableDirector>();
			m_timedCurve = GetComponent<TimedCurve>();
			m_timedCurve.duration = (float)m_playableDirector.duration;
		}

		private void OnEnable()
		{
			m_timedCurve.OnValueChanged.AddListener(TimedCurve_ValueChanged);
		}

		private void OnDisable()
		{
			m_timedCurve.OnValueChanged.RemoveListener(TimedCurve_ValueChanged);
		}

		private void TimedCurve_ValueChanged(float value)
		{
			m_playableDirector.time = value * m_playableDirector.duration;
			m_playableDirector.Evaluate();
		}

		private float GetTimedCurveDuration() => (float)m_playableDirector.duration / m_speed;

		#endregion

		#region Editor-Only
#if UNITY_EDITOR

		private void OnValidate()
		{
			if (!Application.isPlaying)
				return;

			if (m_timedCurve == null || m_playableDirector == null)
				return;

			m_timedCurve.duration = GetTimedCurveDuration();
		}

#endif
		#endregion
	}
}
