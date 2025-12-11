using ToolkitEngine.Dialogue;

namespace Yarn.Unity
{
	public interface IPortraitPresenter
	{
		bool TryGetCustomPortraitKey(DialogueSpeakerType speakerType, out string portraitKey);
	}

	public class PortraitPresenter : DialoguePresenterBase, IPortraitPresenter
	{
		#region Methods

		public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
		{
			if (DialogueManager.CastInstance.TryGetDialogueSpeakerTypeByCharacterName(line.CharacterName, out var speakerType))
			{
				PortraitManager.CastInstance.SetPortrait(speakerType, line, this);
			}
			return YarnTask.CompletedTask;
		}

		public override YarnTask OnDialogueStartedAsync()
		{
			return YarnTask.CompletedTask;
		}

		public override YarnTask OnDialogueCompleteAsync()
		{
			PortraitManager.CastInstance.HideAllPortraits();
			return YarnTask.CompletedTask;
		}

		public virtual bool TryGetCustomPortraitKey(DialogueSpeakerType speakerType, out string portraitKey)
		{
			portraitKey = default;
			return false;
		}

		#endregion
	}
}