using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Events/Dialogue")]
	public abstract class BaseYarnEventUnit : BaseEventUnit<string>
	{
		#region Fields

		[UnitHeaderInspectable("Filtered")]
		public bool filtered;

		[DoNotSerialize]
		public ValueInput node { get; set; }

		#endregion

		#region Properties

		protected override bool showEventArgs => !filtered;

		#endregion

		#region Methods

		protected override void Definition()
		{
			base.Definition();

			if (filtered)
			{
				node = ValueInput(nameof(node), string.Empty);
			}
		}

		protected override bool ShouldTrigger(Flow flow, string args)
		{
			if (!filtered)
				return true;

			return string.Equals(flow.GetValue<string>(node), args);
		}

		#endregion
	}
}