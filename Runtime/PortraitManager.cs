using System.Collections.Generic;
using UnityEngine.UI;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class PortraitManager : Subsystem<PortraitManager>
    {
		#region Fields

		private Dictionary<DialogueSpeakerType, HashSet<Image>> m_map = new();

		private const string DEFAULT_KEY = "Default";
		private const string PORTRAIT_META_KEY = "portrait:";

		#endregion

		#region Methods

		public void Register(Portrait portrait)
		{
			foreach (var speakerType in portrait.speakerTypes)
			{
				if (!m_map.TryGetValue(speakerType, out var set))
				{
					set = new HashSet<Image>();
					m_map.Add(speakerType, set);
				}

				set.Add(portrait.image);
			}
		}

		public void Unregister(Portrait portrait)
		{
			foreach (var speakerType in portrait.speakerTypes)
			{
				if (!m_map.TryGetValue(speakerType, out var set))
					continue;

				set.Remove(portrait.image);

				if (set.Count == 0)
				{
					m_map.Remove(speakerType);
				}
			}
		}

		public void HideAllPortraits()
		{
			foreach (var set in m_map.Values)
			{
				foreach (var image in set)
				{
					image.enabled = false;
				}
			}
		}

		public void SetPortrait(string speakerName, string portraitKey)
		{
			if (DialogueManager.CastInstance.TryGetDialogueSpeakerTypeByCharacterName(speakerName, out var speakerType)
				&& m_map.TryGetValue(speakerType, out var set))
			{
				SetPortrait(speakerType, portraitKey, set);
			}
		}

		public void SetPortrait(DialogueSpeakerType speakerType, LocalizedLine line, IPortraitPresenter presenter = null)
		{
			if (speakerType != null && m_map.TryGetValue(speakerType, out var image))
			{
				SetPortrait(speakerType, line, image, presenter);
			}
		}

		private void SetPortrait(DialogueSpeakerType speakerType, LocalizedLine line, HashSet<Image> set, IPortraitPresenter presenter)
		{
			foreach (var image in set)
			{
				SetPortrait(speakerType, line, image, presenter);
			}
		}

		private void SetPortrait(DialogueSpeakerType speakerType, LocalizedLine line, Image image, IPortraitPresenter presenter)
		{
			if (line != null)
			{
				if ((presenter?.TryGetCustomPortraitKey(speakerType, out string portraitKey) ?? false)
					&& SetPortrait(speakerType, portraitKey, image))
				{
					return;
				}

				for (int i = 0; i < line.Metadata.Length; ++i)
				{
					if (line.Metadata[i].StartsWith(PORTRAIT_META_KEY))
					{
						if (SetPortrait(speakerType, line.Metadata[i].Substring(PORTRAIT_META_KEY.Length), image))
							return;
					}
				}

				if (SetPortrait(speakerType, DEFAULT_KEY, image))
					return;
			}

			SetPortrait(speakerType, portraitKey: null, image);
		}

		private bool SetPortrait(DialogueSpeakerType speakerType, string portraitKey, HashSet<Image> set)
		{
			HideAllPortraits();

			bool allEnabled = true;
			foreach (var image in set)
			{
				allEnabled &= SetPortrait(speakerType, portraitKey, image, false);
			}

			return allEnabled;
		}

		private bool SetPortrait(DialogueSpeakerType speakerType, string portraitKey, Image image, bool hideAllPortraits = true)
		{
			if (hideAllPortraits)
			{
				HideAllPortraits();
			}

			if (!string.IsNullOrWhiteSpace(portraitKey) && (speakerType?.portraitSet?.TryGetPortrait(portraitKey, out var sprite) ?? false))
			{
				image.sprite = sprite;
				image.enabled = true;
			}
			else
			{
				image.enabled = false;
			}
			return image.enabled;
		}

		#endregion

		#region Yarn Methods

		[YarnCommand("defaultPortrait")]
		public static void CmdSetDefaultPortrait(string speakerName)
		{
			CmdSetPortrait(speakerName, DEFAULT_KEY);
		}

		[YarnCommand("portrait")]
		public static void CmdSetPortrait(string speakerName, string portraitKey)
		{
			CastInstance.SetPortrait(speakerName, portraitKey);
		}

		[YarnCommand("hideAllPortraits")]
		public static void CmdHidePortraits()
		{
			CastInstance.HideAllPortraits();
		}

		#endregion
	}
}