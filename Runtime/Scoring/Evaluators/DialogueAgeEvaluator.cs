using UnityEngine;

namespace ToolkitEngine.Dialogue.Scoring
{
	[EvaluableCategory("Dialogue")]
    public class DialogueAgeEvaluator : BaseEvaluator
    {
		#region Fields

		[SerializeField, MinMax(0f, float.PositiveInfinity, "Min", "Max")]
		private Vector2 m_age = new Vector2(0f, 60f);

		#endregion

		#region Properties

		public float min => m_age.x;
		public float max => m_age.y;

		#endregion

		#region Methods

		protected override float CalculateNormalizedScore(GameObject actor, GameObject target, Vector3 position)
		{
			if (actor.TryGetComponent(out DialogueRunnerControl control))
			{
				return MathUtil.GetPercent(control.age, min, max);
			}
			return 0f;
		}

		#endregion
	}
}