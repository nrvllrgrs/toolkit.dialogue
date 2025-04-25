using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;

namespace ToolkitEngine.Dialogue
{
	[RequireComponent(typeof(PlayableDirector))]
    public class Timeline : MonoBehaviour
    {
		#region Fields

		private PlayableDirector m_playableDirector;

		#endregion

		#region Properties

		public string key
		{
			get
			{
#if UNITY_EDITOR
				return (Application.isPlaying ? m_playableDirector : GetComponent<PlayableDirector>())?.playableAsset.name;
#else
				return m_playableDirector.playableAsset.name;
#endif
			}
		}


		public PlayableDirector playableDirector => m_playableDirector;

#endregion

		#region Methods

		protected void Awake()
		{
			m_playableDirector = GetComponent<PlayableDirector>();
			Assert.IsNotNull(m_playableDirector.playableAsset);
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