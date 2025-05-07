using UnityEngine;
using ToolkitEngine.Scoring;

namespace ToolkitEngine.Dialogue.Scoring
{
	[EvaluableCategory("Dialogue")]
	public class DialogueSmartCategoryPriorityEvaluator : BaseEvaluator
	{
		#region Properties

		public float max => DialogueManager.CastInstance.Config.categories.Length;

		#endregion

		#region Methods

		protected override float CalculateNormalizedScore(GameObject actor, GameObject target, Vector3 position)
		{
			if (actor.TryGetComponent(out DialogueRunnerControl control)
				&& DialogueManager.CastInstance.TryGetDialogueCategory(control.dialogueType, out var category))
			{
				int priority = DialogueManager.CastInstance.GetCategoryPriority(category);
				if (priority > 0)
				{
					return MathUtil.GetPercent(priority, 0f, max);
				}
			}
			return 0f;
		}

		#endregion
	}
}