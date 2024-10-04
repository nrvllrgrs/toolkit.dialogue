using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ToolkitEngine.Dialogue
{
	public class DialogueManager : Singleton<DialogueManager>
    {
		#region Fields

		[SerializeField]
		private DialogueManagerConfig m_config;

		private Dictionary<DialogueType, DialogueCategory> m_priorityToCategoryMap = new();
		private Dictionary<DialogueCategory, RuntimeDialogueCategory> m_runtimeMap = new();

		private Dictionary<DialogueCategory, DialogueRunnerSettings> m_settingsByCategory = new();
		private Dictionary<DialogueType, DialogueRunnerSettings> m_settingsByType = new();

		#endregion

		#region Events

		public event EventHandler<DialogueEventArgs> DialogueStart;
		public event EventHandler<DialogueEventArgs> DialogueComplete;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value that indicates if the dialogue is actively
		/// running.
		/// </summary>
		public bool isAnyDialogueRunning => m_runtimeMap.Any(x => x.Value.isDialogueRunning);

		#endregion

		#region Methods

		protected override void Initialize()
		{
			base.Initialize();

			foreach (var category in m_config.categories)
			{
				foreach (var priority in category.priorities)
				{
					m_priorityToCategoryMap.Add(priority, category);
				}

				var runtimeCategory = new RuntimeDialogueCategory(category);
				runtimeCategory.DialogueStart += RuntimeCategory_DialogueStart;
				runtimeCategory.DialogueComplete += RuntimeCategory_DialogueComplete;

				m_runtimeMap.Add(category, runtimeCategory);
			}
		}

		public bool IsDialogueCategoryRunning(DialogueCategory category)
		{
			return m_runtimeMap.TryGetValue(category, out var runtimeCategory)
				? runtimeCategory.isDialogueRunning
				: false;
		}

		public void Play(DialogueRunnerControl control, string startNode)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return;

			runtimeCategory.Play(control, startNode);
		}

		public void Enqueue(DialogueRunnerControl control, string startNode)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return;

			runtimeCategory.Enqueue(control, startNode);
		}

		public void Dequeue(DialogueRunnerControl control, string startNode)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return;

			runtimeCategory.Dequeue(control);
		}

		public void ClearQueue(DialogueRunnerControl control)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return;

			runtimeCategory.ClearQueue();
		}

		public bool TryGetDialogueCategory(DialogueType type, out DialogueCategory category)
		{
			if (TryGetRuntimeDialogueCategory(type, out var runtimeCategory))
			{
				category = runtimeCategory.dialogueCategory;
				return true;
			}

			category = null;
			return false;
		}

		private bool TryGetRuntimeDialogueCategory(DialogueRunnerControl control, out RuntimeDialogueCategory runtimeCategory)
		{
			return TryGetRuntimeDialogueCategory(control?.dialogueType, out runtimeCategory);
		}

		private bool TryGetRuntimeDialogueCategory(DialogueType type, out RuntimeDialogueCategory runtimeCategory)
		{
			runtimeCategory = null;

			if (type == null)
			{
				Debug.LogError("DialogueType is undefined! Cannot play dialogue.");
				return false;
			}

			if (!m_priorityToCategoryMap.TryGetValue(type, out var category)
				|| !m_runtimeMap.TryGetValue(category, out runtimeCategory))
			{
				Debug.LogErrorFormat("DialogueType {0} does not exist in config! Cannot play dialogue.", type.name);
				return false;
			}

			return true;
		}

		public int GetPriority(DialogueType dialogueType)
		{
			return TryGetRuntimeDialogueCategory(dialogueType, out var runtimeCategory)
				? runtimeCategory.dialogueCategory.GetPriority(dialogueType)
				: -1;
		}

		public float GetQueueAge(DialogueRunnerControl control)
		{
			return TryGetRuntimeDialogueCategory(control, out var runtimeCategory)
				? runtimeCategory.GetQueueAge(control)
				: float.PositiveInfinity;
		}

		#endregion

		#region Settings Methods

		public void Register(DialogueRunnerSettings settings)
		{
			switch (settings.registration)
			{
				case DialogueRunnerSettings.RegistrationMode.Category:
					if (!m_settingsByCategory.ContainsKey(settings.dialogueCategory))
					{
						m_settingsByCategory.Add(settings.dialogueCategory, settings);
					}
					else
					{
						m_settingsByCategory[settings.dialogueCategory] = settings;
					}
					break;

				case DialogueRunnerSettings.RegistrationMode.Type:
					if (!m_settingsByType.ContainsKey(settings.dialogueType))
					{
						m_settingsByType.Add(settings.dialogueType, settings);
					}
					else
					{
						m_settingsByType[settings.dialogueType] = settings;
					}
					break;
			}
		}

		public void Unregister(DialogueRunnerSettings settings)
		{
			switch (settings.registration)
			{
				case DialogueRunnerSettings.RegistrationMode.Category:
					m_settingsByCategory.Remove(settings.dialogueCategory);
					break;

				case DialogueRunnerSettings.RegistrationMode.Type:
					m_settingsByType.Remove(settings.dialogueType);
					break;
			}
		}

		public bool TryGetDialogueRunnerSettings(DialogueCategory category, out DialogueRunnerSettings settings)
		{
			return m_settingsByCategory.TryGetValue(category, out settings);
		}

		public bool TryGetDialogueRunnerSettings(DialogueType type, out DialogueRunnerSettings settings)
		{
			if (m_settingsByType.TryGetValue(type, out settings))
				return true;

			return TryGetDialogueCategory(type, out var category)
				&& TryGetDialogueRunnerSettings(category, out settings);
		}

		public bool ReplicateSettings(DialogueRunnerControl control)
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

		private void RuntimeCategory_DialogueStart(object sender, DialogueEventArgs e)
		{
			DialogueStart?.Invoke(this, e);
		}

		private void RuntimeCategory_DialogueComplete(object sender, DialogueEventArgs e)
		{
			DialogueComplete?.Invoke(this, e);
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

			internal event EventHandler<DialogueEventArgs> DialogueStart;
			internal event EventHandler<DialogueEventArgs> DialogueComplete;

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
				control.onDialogueStart.AddListener(DialogueRunnerControl_DialogueStart);
				control.onDialogueComplete.AddListener(DialogueRunnerControl_DialogueComplete);
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

			private void DialogueRunnerControl_DialogueStart(DialogueEventArgs args)
			{
				if (args.control == null || !m_activeRunnerControls.Contains(args.control))
					return;

				DialogueStart?.Invoke(this, args);
			}

			private void DialogueRunnerControl_DialogueComplete(DialogueEventArgs args)
			{
				if (args.control == null || !m_activeRunnerControls.Contains(args.control))
					return;

				args.control.onDialogueStart.RemoveListener(DialogueRunnerControl_DialogueStart);
				args.control.onDialogueComplete.RemoveListener(DialogueRunnerControl_DialogueComplete);
				m_activeRunnerControls.Remove(args.control);

				DialogueComplete?.Invoke(this, args);

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

			#endregion
		}

		#endregion
	}
}