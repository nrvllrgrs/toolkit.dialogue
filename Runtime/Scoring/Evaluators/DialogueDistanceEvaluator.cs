using UnityEngine;

namespace ToolkitEngine.Dialogue.Scoring
{
	[EvaluableCategory("Dialogue")]
	public class DialogueDistanceEvaluator : BaseEvaluator
	{
		#region Fields

		[SerializeField, MinMax(0f, float.PositiveInfinity, "Min", "Max")]
		private Vector2 m_range = new Vector2(0, 100);

		#endregion

		#region Properties

		public float minDistance => m_range.x;
		public float maxDistance => m_range.y;

		#endregion

		#region Methods

		protected override float CalculateNormalizedScore(GameObject actor, GameObject target, Vector3 position)
		{
			if (actor.TryGetComponent(out DialogueRunnerControl control))
			{
				AudioListener listener = Object.FindObjectOfType<AudioListener>();
				if (listener != null)
				{
					return MathUtil.GetPercent(
						(control.transform.position - listener.transform.position).sqrMagnitude,
						minDistance * minDistance,
						maxDistance * maxDistance);
				}
			}
			return 0f;
		}

		#endregion
	}
}