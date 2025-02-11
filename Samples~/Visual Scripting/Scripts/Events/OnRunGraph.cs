using Unity.VisualScripting;
using ToolkitEngine.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Events/Dialogue")]
	[UnitTitle("On Run Graph")]
    public class OnRunGraph : EventUnit<EmptyEventArgs>
    {
		#region Fields

		[UnitHeaderInspectable]
		public string key { get; private set; }

		protected override bool register => true;
		private string hookName => VisualScriptingManager.GetEventHookName<OnRunGraph>(key);

		#endregion

		#region Methods

		public override void Instantiate(GraphReference instance)
		{
			base.Instantiate(instance);
			VisualScriptingManager.CastInstance.Register(hookName, instance.machine);
		}

		public override void Uninstantiate(GraphReference instance)
		{
			base.Uninstantiate(instance);
			VisualScriptingManager.CastInstance.Unregister(hookName, instance.machine);
		}

		public override EventHook GetHook(GraphReference reference)
		{
			return new EventHook(hookName, reference.self);
		}

		#endregion
	}
}