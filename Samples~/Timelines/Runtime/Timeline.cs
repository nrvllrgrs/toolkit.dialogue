using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;

namespace ToolkitEngine.Dialogue
{
	[RequireComponent(typeof(PlayableDirector))]
    public class Timeline : MonoBehaviour
    {
		#region Fields

		[SerializeField]
		private string m_key;

		private PlayableDirector m_playableDirector;

		#endregion

		#region Properties

		public string key => m_key;

		public PlayableDirector playableDirector => m_playableDirector;

		#endregion

		#region Methods

		protected void Awake()
		{
			Assert.IsTrue(!string.IsNullOrWhiteSpace(m_key));
			m_playableDirector = GetComponent<PlayableDirector>();
		}

		protected void OnEnable()
		{
			TimelineManager.CastInstance.Register(this);
		}

		protected void OnDisable()
		{
			TimelineManager.CastInstance.Unregister(this);
		}

		#endregion
	}
}