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

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value that indicates if the dialogue is actively
		/// running.
		/// </summary>
		public bool IsAnyDialogueRunning => m_runtimeMap.Any(x => x.Value.isDialogueRunning);

		#endregion

		#region Methods

		protected override void Awake()
		{
			base.Awake();

			foreach (var category in m_config.categories)
			{
				foreach (var priority in category.priorities)
				{
					m_priorityToCategoryMap.Add(priority, category);
				}

				m_runtimeMap.Add(category, new RuntimeDialogueCategory(category));
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

		#region Structures

		[Serializable]
		public class RuntimeDialogueCategory
		{
			#region Fields

			private List<DialogueRunnerControl> m_activeRunnerControls = new();
			private Dictionary<DialogueRunnerControl, Tuple<string, float>> m_queue = new();
			private bool m_interrupted = false;

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
				control.onDialogueComplete.AddListener(DialogueRunnerControl_DialogueComplete);
				control.dialogueRunner.StartDialogue(startNode);
				m_activeRunnerControls.Add(control);

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

			private void DialogueRunnerControl_DialogueComplete(DialogueEventArgs args)
			{
				if (args.control == null || !m_activeRunnerControls.Contains(args.control))
					return;

				args.control.onDialogueComplete.RemoveListener(DialogueRunnerControl_DialogueComplete);
				m_activeRunnerControls.Remove(args.control);

				// Check if dialogue is queued and start next
				// But if interrupted, skip
				if (!m_interrupted && m_queue.Count > 0)
				{
					// Find "forgotten" keys
					var forgottenKeys = m_queue.Keys.Where(x => GetQueueAge(x) < dialogueCategory.timeToForget);
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