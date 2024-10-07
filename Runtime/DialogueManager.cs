using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ToolkitEngine.Dialogue
{
	public static class DialogueManager
    {
		#region Fields

		[SerializeField]
		private static DialogueManagerConfig s_config;

		private static Dictionary<DialogueType, DialogueCategory> s_priorityToCategoryMap;
		private static Dictionary<DialogueCategory, RuntimeDialogueCategory> s_runtimeMap;

		private static Dictionary<DialogueCategory, DialogueRunnerSettings> s_settingsByCategory;
		private static Dictionary<DialogueType, DialogueRunnerSettings> s_settingsByType;

		#endregion

		#region Events

		public static event EventHandler<DialogueEventArgs> DialogueStarted;
		public static event EventHandler<DialogueEventArgs> DialogueCompleted;
		public static event EventHandler<DialogueEventArgs> NodeStarted;
		public static event EventHandler<DialogueEventArgs> NodeCompleted;
		public static event EventHandler<DialogueEventArgs> Command;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value that indicates if the dialogue is actively
		/// running.
		/// </summary>
		public static bool isAnyDialogueRunning => s_runtimeMap.Any(x => x.Value.isDialogueRunning);

		#endregion

		#region Methods

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void OnRuntimeInitialized()
		{
			s_config = Resources.Load<DialogueManagerConfig>("DialogueManagerConfig");
			s_priorityToCategoryMap = new();
			s_runtimeMap = new();
			s_settingsByCategory = new();
			s_settingsByType = new();

			foreach (var category in s_config.categories)
			{
				foreach (var priority in category.priorities)
				{
					s_priorityToCategoryMap.Add(priority, category);
				}

				var runtimeCategory = new RuntimeDialogueCategory(category);
				runtimeCategory.DialogueStarted += RuntimeCategory_DialogueStart;
				runtimeCategory.DialogueCompleted += RuntimeCategory_DialogueComplete;
				runtimeCategory.NodeStarted += RuntimeCategory_NodeStarted;
				runtimeCategory.NodeCompleted += RuntimeCategory_NodeCompleted;
				runtimeCategory.Command += RuntimeCategory_Command;

				s_runtimeMap.Add(category, runtimeCategory);
			}
		}

		public static bool IsDialogueCategoryRunning(DialogueCategory category)
		{
			return s_runtimeMap.TryGetValue(category, out var runtimeCategory)
				? runtimeCategory.isDialogueRunning
				: false;
		}

		public static void Play(DialogueRunnerControl control, string startNode)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return;

			runtimeCategory.Play(control, startNode);
		}

		public static void Enqueue(DialogueRunnerControl control, string startNode)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return;

			runtimeCategory.Enqueue(control, startNode);
		}

		public static void Dequeue(DialogueRunnerControl control, string startNode)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return;

			runtimeCategory.Dequeue(control);
		}

		public static void ClearQueue(DialogueRunnerControl control)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return;

			runtimeCategory.ClearQueue();
		}

		public static bool TryGetDialogueCategory(DialogueType type, out DialogueCategory category)
		{
			if (TryGetRuntimeDialogueCategory(type, out var runtimeCategory))
			{
				category = runtimeCategory.dialogueCategory;
				return true;
			}

			category = null;
			return false;
		}

		private static bool TryGetRuntimeDialogueCategory(DialogueRunnerControl control, out RuntimeDialogueCategory runtimeCategory)
		{
			return TryGetRuntimeDialogueCategory(control?.dialogueType, out runtimeCategory);
		}

		private static bool TryGetRuntimeDialogueCategory(DialogueType type, out RuntimeDialogueCategory runtimeCategory)
		{
			runtimeCategory = null;

			if (type == null)
			{
				Debug.LogError("DialogueType is undefined! Cannot play dialogue.");
				return false;
			}

			if (!s_priorityToCategoryMap.TryGetValue(type, out var category)
				|| !s_runtimeMap.TryGetValue(category, out runtimeCategory))
			{
				Debug.LogErrorFormat("DialogueType {0} does not exist in config! Cannot play dialogue.", type.name);
				return false;
			}

			return true;
		}

		public static int GetPriority(DialogueType dialogueType)
		{
			return TryGetRuntimeDialogueCategory(dialogueType, out var runtimeCategory)
				? runtimeCategory.dialogueCategory.GetPriority(dialogueType)
				: -1;
		}

		public static float GetQueueAge(DialogueRunnerControl control)
		{
			return TryGetRuntimeDialogueCategory(control, out var runtimeCategory)
				? runtimeCategory.GetQueueAge(control)
				: float.PositiveInfinity;
		}

		#endregion

		#region Settings Methods

		public static void Register(DialogueRunnerSettings settings)
		{
			switch (settings.registration)
			{
				case DialogueRunnerSettings.RegistrationMode.Category:
					if (!s_settingsByCategory.ContainsKey(settings.dialogueCategory))
					{
						s_settingsByCategory.Add(settings.dialogueCategory, settings);
					}
					else
					{
						s_settingsByCategory[settings.dialogueCategory] = settings;
					}
					break;

				case DialogueRunnerSettings.RegistrationMode.Type:
					if (!s_settingsByType.ContainsKey(settings.dialogueType))
					{
						s_settingsByType.Add(settings.dialogueType, settings);
					}
					else
					{
						s_settingsByType[settings.dialogueType] = settings;
					}
					break;
			}
		}

		public static void Unregister(DialogueRunnerSettings settings)
		{
			switch (settings.registration)
			{
				case DialogueRunnerSettings.RegistrationMode.Category:
					s_settingsByCategory.Remove(settings.dialogueCategory);
					break;

				case DialogueRunnerSettings.RegistrationMode.Type:
					s_settingsByType.Remove(settings.dialogueType);
					break;
			}
		}

		public static bool TryGetDialogueRunnerSettings(DialogueCategory category, out DialogueRunnerSettings settings)
		{
			return s_settingsByCategory.TryGetValue(category, out settings);
		}

		public static bool TryGetDialogueRunnerSettings(DialogueType type, out DialogueRunnerSettings settings)
		{
			if (s_settingsByType.TryGetValue(type, out settings))
				return true;

			return TryGetDialogueCategory(type, out var category)
				&& TryGetDialogueRunnerSettings(category, out settings);
		}

		public static bool ReplicateSettings(DialogueRunnerControl control)
		{
			if (TryGetDialogueRunnerSettings(control.dialogueType, out var settings))
			{
				if (settings != null)
				{
					control.dialogueRunner.dialogueViews = settings.dialogueViews;
				}

				if (settings.lineProvider != null)
				{
					control.dialogueRunner.lineProvider = settings.lineProvider;
					control.dialogueRunner.lineProvider.YarnProject = control.dialogueRunner.yarnProject;
				}

				if (settings.variableStorage != null)
				{
					control.dialogueRunner.VariableStorage = settings.variableStorage;
					control.dialogueRunner.SetInitialVariables();
				}

				return true;
			}
			return false;
		}

		#endregion

		#region Callbacks

		private static void RuntimeCategory_DialogueStart(object sender, DialogueEventArgs e)
		{
			DialogueStarted?.Invoke(null, e);
		}

		private static void RuntimeCategory_DialogueComplete(object sender, DialogueEventArgs e)
		{
			DialogueCompleted?.Invoke(null, e);
		}

		private static void RuntimeCategory_NodeStarted(object sender, DialogueEventArgs e)
		{
			NodeStarted?.Invoke(null, e);
		}

		private static void RuntimeCategory_NodeCompleted(object sender, DialogueEventArgs e)
		{
			NodeCompleted?.Invoke(null, e);
		}

		private static void RuntimeCategory_Command(object sender, DialogueEventArgs e)
		{
			Command?.Invoke(null, e);
		}

		#endregion

		#region Structures

		[Serializable]
		public class RuntimeDialogueCategory
		{
			#region Fields

			private List<DialogueRunnerControl> m_activeRunnerControls = new();
			private Dictionary<DialogueRunnerControl, Tuple<string, float>> m_queue = new();
			private bool m_interrupted = false;

			#endregion

			#region Events

			internal event EventHandler<DialogueEventArgs> DialogueStarted;
			internal event EventHandler<DialogueEventArgs> DialogueCompleted;
			internal event EventHandler<DialogueEventArgs> NodeStarted;
			internal event EventHandler<DialogueEventArgs> NodeCompleted;
			internal event EventHandler<DialogueEventArgs> Command;

			#endregion

			#region Properties

			public DialogueCategory dialogueCategory { get; private set; }
			public bool isDialogueRunning => m_activeRunnerControls.Count > 0;
			public int dialogueRunningCount => m_activeRunnerControls.Count;
			public DialogueRunnerControl[] activeRunnerControls => m_activeRunnerControls.ToArray();

			#endregion

			#region Constructors

			public RuntimeDialogueCategory(DialogueCategory category)
			{
				dialogueCategory = category;
			}

			#endregion

			#region Methods

			internal void Play(DialogueRunnerControl control, string startNode)
			{
				// Under allowed simultaneous runners
				if (dialogueCategory.infiniteSimultaneous || m_activeRunnerControls.Count < dialogueCategory.maxSimultaneous)
				{
					PlayInternal(control, startNode);
				}
				else
				{
					// Determine if interruption should occur
					var interruptable = GetInterruptable(dialogueCategory, control);
					if (interruptable != null)
					{
						m_interrupted = true;
						interruptable.Stop();
						Play(control, startNode);
					}
					// Determine if enqueuing should occur
					else if (dialogueCategory.queueable && control.dialogueType.enqueueIfBlocked)
					{
						Enqueue(control, startNode);
					}
				}
			}

			private DialogueRunnerControl GetInterruptable(DialogueCategory category, DialogueRunnerControl control)
			{
				// DialogueType cannot interrupt, skip
				if (control.dialogueType.interruptPriority == 0)
				{
					return null;
				}

				int priority = category.GetPriority(control.dialogueType);

				HashSet<DialogueRunnerControl> interruptableControls = new();
				foreach (var activeControl in m_activeRunnerControls)
				{
					int activePriority = category.GetPriority(activeControl.dialogueType);
					if ((activePriority < priority && (control.dialogueType.interruptPriority & DialogueType.InterruptRule.LessThan) != 0)
						|| (activePriority == priority && (control.dialogueType.interruptPriority & DialogueType.InterruptRule.Equal) != 0)
						|| (activePriority > priority && (control.dialogueType.interruptPriority & DialogueType.InterruptRule.GreaterThan) != 0))
					{
						interruptableControls.Add(activeControl);
					}
				}

				return category.GetInterruptable(interruptableControls);
			}

			private void PlayInternal(DialogueRunnerControl control, string startNode)
			{
				control.onDialogueStarted.AddListener(DialogueRunnerControl_DialogueStart);
				control.onDialogueCompleted.AddListener(DialogueRunnerControl_DialogueComplete);
				control.onNodeStarted.AddListener(DialogueRunnerControl_NodeStarted);
				control.onNodeCompleted.AddListener(DialogueRunnerControl_NodeCompleted);
				control.onCommand.AddListener(DialogueRunnerControl_Command);

				m_activeRunnerControls.Add(control);

				control.PlayInternal(startNode);

				if (dialogueCategory.queueable && control.dialogueType.autoClearQueue)
				{
					ClearQueue();
				}
			}

			internal void Enqueue(DialogueRunnerControl control, string startNode)
			{
				// Under allowed simultaneous runners
				if (dialogueCategory.infiniteSimultaneous || m_activeRunnerControls.Count < dialogueCategory.maxSimultaneous)
				{
					PlayInternal(control, startNode);
				}
				else if (dialogueCategory.queueable)
				{
					EnqueueInternal(control, startNode);
				}
			}

			private void EnqueueInternal(DialogueRunnerControl control, string startNode)
			{
				m_queue.Add(control, new Tuple<string, float>(startNode, Time.time));
			}

			internal void Dequeue(DialogueRunnerControl control)
			{
				if (m_queue.ContainsKey(control))
				{
					m_queue.Remove(control);
				}
			}

			internal void ClearQueue()
			{
				m_queue.Clear();
			}

			internal float GetQueueAge(DialogueRunnerControl control)
			{
				return m_queue.TryGetValue(control, out var tuple)
					? tuple.Item2
					: float.PositiveInfinity;
			}

			#endregion

			#region Callbacks

			private bool IsActiveDialogueRunnerControl(DialogueEventArgs args)
			{
				return args.control != null && m_activeRunnerControls.Contains(args.control);
			}

			private void DialogueRunnerControl_DialogueStart(DialogueEventArgs args)
			{
				if (!IsActiveDialogueRunnerControl(args))
					return;

				DialogueStarted?.Invoke(this, args);
			}

			private void DialogueRunnerControl_DialogueComplete(DialogueEventArgs args)
			{
				if (!IsActiveDialogueRunnerControl(args))
					return;

				args.control.onDialogueStarted.RemoveListener(DialogueRunnerControl_DialogueStart);
				args.control.onDialogueCompleted.RemoveListener(DialogueRunnerControl_DialogueComplete);
				args.control.onNodeStarted.AddListener(DialogueRunnerControl_NodeStarted);
				args.control.onNodeCompleted.AddListener(DialogueRunnerControl_NodeCompleted);
				args.control.onCommand.AddListener(DialogueRunnerControl_Command);

				m_activeRunnerControls.Remove(args.control);

				DialogueCompleted?.Invoke(this, args);

				// Check if dialogue is queued and start next
				// But if interrupted, skip
				if (!m_interrupted && m_queue.Count > 0)
				{
					// Find "forgotten" keys
					var forgottenKeys = m_queue.Keys.Where(x => GetQueueAge(x) > dialogueCategory.timeToForget).ToArray();
					foreach (var key in forgottenKeys)
					{
						m_queue.Remove(key);
					}

					// Find next runner to play
					var next = dialogueCategory.Next(m_queue.Keys);
					if (next != null && m_queue.TryGetValue(next, out var tuple))
					{
						m_queue.Remove(next);
						PlayInternal(next, tuple.Item1);
					}
				}

				m_interrupted = false;
			}

			private void DialogueRunnerControl_NodeStarted(DialogueEventArgs args)
			{
				if (!IsActiveDialogueRunnerControl(args))
					return;

				NodeStarted?.Invoke(this, args);
			}

			private void DialogueRunnerControl_NodeCompleted(DialogueEventArgs args)
			{
				if (!IsActiveDialogueRunnerControl(args))
					return;

				NodeCompleted?.Invoke(this, args);
			}

			private void DialogueRunnerControl_Command(DialogueEventArgs args)
			{
				if (!IsActiveDialogueRunnerControl(args))
					return;

				Command?.Invoke(this, args);
			}

			#endregion
		}

		#endregion
	}
}