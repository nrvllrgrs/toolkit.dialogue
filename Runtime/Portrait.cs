using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ToolkitEngine.Dialogue
{
	public class Portrait : MonoBehaviour
    {
		#region Fields

		[SerializeField]
		private Image m_image;

		[SerializeField]
		private List<DialogueSpeakerType> m_speakerTypes;

		#endregion

		#region Properties

		public Image image => m_image;
		public IEnumerable<DialogueSpeakerType> speakerTypes => m_speakerTypes;

		#endregion

		#region Methods

		private void Awake()
		{
			if (m_image == null)
			{
				m_image = GetComponent<Image>();
			}
			Assert.IsNotNull(m_image);
		}

		private void OnEnable()
		{
			PortraitManager.CastInstance.Register(this);
		}

		private void OnDisable()
		{
			PortraitManager.CastInstance.Unregister(this);
		}

		#endregion
	}
}