using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Yarn.Unity;
using Yarn.Unity.UnityLocalization;

namespace ToolkitEngine.Dialogue
{
	public class DialogueManager : ConfigurableSubsystem<DialogueManager, DialogueManagerConfig>
    {
		#region Fields

		private Dictionary<DialogueType, DialogueCategory> m_priorityToCategoryMap;
		private Dictionary<DialogueCategory, RuntimeDialogueCategory> m_runtimeMap;

		private Dictionary<DialogueCategory, DialogueRunnerSettings> m_settingsByCategory;
		private Dictionary<DialogueType, DialogueRunnerSettings> m_settingsByType;

		private Dictionary<Tuple<DialogueType, YarnProject, string>, DialogueRunnerControl> m_spawnMap = new();
		private Dictionary<DialogueSpeakerType, HashSet<DialogueSpeaker>> m_speakerMap = new();
		private Dictionary<string, DialogueSpeakerType> m_characterNameToSpeakerTypeMap = new();

#if UNITY_EDITOR
		private static GameObject s_container;
#endif
		#endregion

		#region Events

		public event EventHandler<DialogueEventArgs> DialogueStarted;
		public event EventHandler<DialogueEventArgs> DialogueCompleted;
		public event EventHandler<DialogueEventArgs> NodeStarted;
		public event EventHandler<DialogueEventArgs> NodeCompleted;
		public event EventHandler<DialogueEventArgs> Command;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value that indicates if the dialogue is actively
		/// running.
		/// </summary>
		public bool isAnyDialogueRunning => m_runtimeMap.Any(x => x.Value.isDialogueRunning);

#if UNITY_EDITOR
		private static Transform container
		{
			get
			{
				if (s_container == null)
				{
					s_container = new GameObject("Dialogues");
					UnityEngine.Object.DontDestroyOnLoad(s_container);
				}
				return s_container.transform;
			}
		}
#endif
		#endregion

		#region Methods

		protected override void Initialize()
		{
			m_priorityToCategoryMap = new();
			m_runtimeMap = new();
			m_settingsByCategory = new();
			m_settingsByType = new();
			m_spawnMap = new();
			m_speakerMap = new();
			m_characterNameToSpeakerTypeMap = new();

			int categoryPriority = Config.categories.Length;
			foreach (var category in Config.categories)
			{
				foreach (var priority in category.priorities)
				{
					m_priorityToCategoryMap.Add(priority, category);
				}

				var runtimeCategory = new RuntimeDialogueCategory(category, categoryPriority--);
				runtimeCategory.DialogueStarted += RuntimeCategory_DialogueStart;
				runtimeCategory.DialogueCompleted += RuntimeCategory_DialogueComplete;
				runtimeCategory.NodeStarted += RuntimeCategory_NodeStarted;
				runtimeCategory.NodeCompleted += RuntimeCategory_NodeCompleted;
				runtimeCategory.Command += RuntimeCategory_Command;

				m_runtimeMap.Add(category, runtimeCategory);
			}

			foreach (var speaker in Config.speakers)
			{
				if (!m_characterNameToSpeakerTypeMap.ContainsKey(speaker.name))
				{
					m_characterNameToSpeakerTypeMap.Add(speaker.name, speaker);
				}
			}

			// Any instantiated DialogueRunners should automatically be cleared by PoolItemManager
		}

		protected override void Terminate()
		{
			foreach (var runtimeCategory in m_runtimeMap.Values)
			{
				runtimeCategory.Dispose();
			}
			m_runtimeMap = null;
		}

		#endregion

		#region Control Methods

		public bool IsDialogueCategoryRunning(DialogueCategory category)
		{
			return m_runtimeMap.TryGetValue(category, out var runtimeCategory)
				? runtimeCategory.isDialogueRunning
				: false;
		}

		public bool Play(DialogueType dialogueType, YarnProject project, string startNode, Action<GameObject> onSpawned = null)
		{
			if (!TryGetRuntimeDialogueCategory(dialogueType, out var runtimeCategory))
				return false;

			Config.dialogueSpawner.Instantiate(DialogueSpawned, runtimeCategory, dialogueType, project, startNode, true, onSpawned);
			return false;
		}

		public bool Play(DialogueRunnerControl control, string startNode)
		{
			if (!TryGetRuntimeDialogueCategory(control, out var runtimeCategory))
				return false;

			return runtimeCategory.Play(control, startNode);
		}

		public bool Enqueue(DialogueType dialogueType, YarnProject project, string startNode, Action<GameObject> onSpawned = null)
		{
			if (!TryGetRuntimeDialogueCategory(dialogueType, out var runtimeCategory))
				return false;

			Config.dialogueSpawner.Instantiate(DialogueSpawned, runtimeCategory, dialogueType, project, startNode, false, onSpawned);
			return false;
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

		public void ClearQueue(DialogueType dialogueType)
		{
			if (!TryGetRuntimeDialogueCategory(dialogueType, out var runtimeCategory))
				return;

			runtimeCategory.ClearQueue();
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

		public int GetCategoryPriority(DialogueCategory category)
		{
			return m_runtimeMap.TryGetValue(category, out var runtimeCategory)
				? runtimeCategory.priority
				: -1;
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

		public bool ReplicateSettings(DialogueRunnerControl control, bool appendDialogueViews)
		{
			if (TryGetDialogueRunnerSettings(control.dialogueType, out var settings))
			{
				if (settings != null)
				{
					if (!appendDialogueViews)
					{
						control.dialogueRunner.dialogueViews = settings.dialogueViews;
					}
					else
					{
						// Going to append, skipping DialogueViews of the same type
						var existingTypes = control.dialogueRunner.dialogueViews.Select(x => x.GetType()).ToHashSet();

						// Appended list
						var list = new List<DialogueViewBase>(control.dialogueRunner.dialogueViews);
						foreach (var dialogueView in settings.dialogueViews)
						{
							if (existingTypes.Contains(dialogueView.GetType()))
								continue;

							list.Add(dialogueView);
						}

						control.dialogueRunner.dialogueViews = list.ToArray();
					}
				}

				if (settings.variableStorage != null)
				{
					control.dialogueRunner.VariableStorage = settings.variableStorage;
				}

				return true;
			}
			return false;
		}

		#endregion

		#region Spawner Methods

		internal static void UpdateLineProvider(DialogueRunnerControl control)
		{
#if USE_UNITY_LOCALIZATION
			if (control.dialogueRunner.yarnProject.localizationType == LocalizationType.Unity
				&& control.dialogueRunner.lineProvider is UnityLocalisedLineProvider localizedLineProvider
				&& (CastInstance.Config.tableMap?.TryGetTables(control.dialogueRunner.yarnProject, out var tables) ?? false))
			{
				ReflectionUtil.TrySetFieldValue(localizedLineProvider, "stringsTable", tables.stringTable);
				ReflectionUtil.TrySetFieldValue(localizedLineProvider, "assetTable", tables.audioTable);
			}
#endif
		}

		private void DialogueSpawned(GameObject obj, params object[] args)
		{
			UnityEngine.Object.DontDestroyOnLoad(obj);

#if UNITY_EDITOR
			obj.transform.SetParent(container);
#endif

			var control = obj.GetComponent<DialogueRunnerControl>();
			if (control == null)
				return;

			control.dialogueType = args[1] as DialogueType;
			control.dialogueRunner.SetProject(args[2] as YarnProject);
			string startNode = args[3] as string;

			// Map parameters to spawned object so it can be referenced
			var key = new Tuple<DialogueType, YarnProject, string>(control.dialogueType, control.dialogueRunner.yarnProject, startNode);
			m_spawnMap.Add(key, control);

			UpdateLineProvider(control);

			// Notify custom behaviour that DialogueRunnerControl has been spawned
			(args[5] as Action<GameObject>)?.Invoke(obj);

			// Control may have been been able to play (e.g. blocked by simultaneous limit, "forgotten" in queue)
			// Unsubscribe before subscribing to release pool item
			control.DialogueLateCompleted -= Instance_DialogueLateCompleted;
			control.DialogueLateCompleted += Instance_DialogueLateCompleted;

			// Play versus enqueue
			var runtimeCategory = args[0] as RuntimeDialogueCategory;
			if ((bool)args[4])
			{
				runtimeCategory.Play(control, startNode);
			}
			else
			{
				runtimeCategory.Enqueue(control, startNode);
			}
		}

		private void Instance_DialogueLateCompleted(object sender, DialogueEventArgs e)
		{
			if (e?.control == null)
				return;

			e.control.DialogueLateCompleted -= Instance_DialogueLateCompleted;
			PoolItem.Destroy(e.control.gameObject);
		}

		public DialogueRunnerControl GetDialogueRunnerControl(DialogueType dialogueType, YarnProject project, string startNode)
		{
			var key = new Tuple<DialogueType, YarnProject, string>(dialogueType, project, startNode);
			return m_spawnMap.TryGetValue(key, out var control)
				? control
				: null;
		}

		#endregion

		#region Speaker Methods

		public void Register(DialogueSpeaker speaker)
		{
			Assert.IsNotNull(speaker);
			Assert.IsNotNull(speaker.speakerType);

			if (!m_speakerMap.TryGetValue(speaker.speakerType, out var set))
			{
				set = new();
				m_speakerMap.Add(speaker.speakerType, set);
			}

			set.Add(speaker);
		}

		public void Unregister(DialogueSpeaker speaker)
		{
			Assert.IsNotNull(speaker);
			Assert.IsNotNull(speaker.speakerType);

			if (!m_speakerMap.TryGetValue(speaker.speakerType, out var set))
				return;

			set.Remove(speaker);
		}

		public bool TryGetDialogueSpeakers(DialogueSpeakerType speakerType, out HashSet<DialogueSpeaker> speakers)
		{
			speakers = null;
			return speakerType != null && m_speakerMap.TryGetValue(speakerType, out speakers);
		}

		public bool TryGetDialogueSpeakerTypeByCharacterName(string characterName, out DialogueSpeakerType speakerType)
		{
			speakerType = null;
			return characterName != null && m_characterNameToSpeakerTypeMap.TryGetValue(characterName, out speakerType);
		}

		public bool TryGetDialogueSpeakersByCharacterName(string characterName, out HashSet<DialogueSpeaker> speakers)
		{
			speakers = null;
			return TryGetDialogueSpeakerTypeByCharacterName(characterName, out var speakerType)
				&& TryGetDialogueSpeakers(speakerType, out speakers);
		}

		#endregion

		#region Callbacks

		private void RuntimeCategory_DialogueStart(object sender, DialogueEventArgs e)
		{
			DialogueStarted?.Invoke(null, e);
		}

		private void RuntimeCategory_DialogueComplete(object sender, DialogueEventArgs e)
		{
			DialogueCompleted?.Invoke(null, e);
		}

		private void RuntimeCategory_NodeStarted(object sender, DialogueEventArgs e)
		{
			NodeStarted?.Invoke(null, e);
		}

		private void RuntimeCategory_NodeCompleted(object sender, DialogueEventArgs e)
		{
			NodeCompleted?.Invoke(null, e);
		}

		private void RuntimeCategory_Command(object sender, DialogueEventArgs e)
		{
			Command?.Invoke(null, e);
		}

		#endregion

		#region Structures

		[Serializable]
		public class RuntimeDialogueCategory : IDisposable
		{
			#region Fields

			private List<DialogueRunnerControl> m_activeRunnerControls = new();
			private Dictionary<DialogueRunnerControl, Tuple<string, float>> m_queue = new();
			private bool m_interrupted = false;

			private bool m_disposed;

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
			public int priority { get; private set; }
			public bool isDialogueRunning => m_activeRunnerControls.Count > 0;
			public int dialogueRunningCount => m_activeRunnerControls.Count;
			public DialogueRunnerControl[] activeRunnerControls => m_activeRunnerControls.ToArray();

			#endregion

			#region Constructors

			public RuntimeDialogueCategory(DialogueCategory category, int priority)
			{
				dialogueCategory = category;
				this.priority = priority;
			}

			#endregion

			#region Methods

			internal bool Play(DialogueRunnerControl control, string startNode)
			{
				// Under allowed simultaneous runners
				if (dialogueCategory.infiniteSimultaneous || m_activeRunnerControls.Count < dialogueCategory.maxSimultaneous)
				{
					PlayInternal(control, startNode);
					return true;
				}
				else
				{
					// Determine if interruption should occur
					var interruptable = GetInterruptable(dialogueCategory, control);
					if (interruptable != null)
					{
						m_interrupted = true;
						if (interruptable.dialogueRunner.IsDialogueRunning)
						{
							interruptable.Stop();
						}
						Remove(interruptable);

						return Play(control, startNode);
					}
					// Determine if enqueuing should occur
					else if (dialogueCategory.queueable && control.dialogueType.enqueueIfBlocked)
					{
						Enqueue(control, startNode);
						return true;
					}
				}
				return false;
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
				control.DialogueCompleting += DialogueRunnerControl_DialogueCompleting;
				control.onDialogueStarted.AddListener(DialogueRunnerControl_DialogueStarted);
				control.onDialogueCompleted.AddListener(DialogueRunnerControl_DialogueCompleted);
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

			private void Remove(DialogueRunnerControl control)
			{
				if (!m_activeRunnerControls.Contains(control))
					return;

				m_activeRunnerControls.Remove(control);
			}

			#endregion

			#region Callbacks

			private bool IsActiveDialogueRunnerControl(DialogueEventArgs e)
			{
				return e.control != null && m_activeRunnerControls.Contains(e.control);
			}

			private void DialogueRunnerControl_DialogueStarted(DialogueEventArgs e)
			{
				if (!IsActiveDialogueRunnerControl(e))
					return;

				DialogueStarted?.Invoke(this, e);
			}

			private void DialogueRunnerControl_DialogueCompleting(object sender, DialogueEventArgs e)
			{
				if (!IsActiveDialogueRunnerControl(e))
					return;

				e.control.DialogueCompleting -= DialogueRunnerControl_DialogueCompleting;
				e.control.onDialogueStarted.RemoveListener(DialogueRunnerControl_DialogueStarted);
				e.control.onDialogueCompleted.RemoveListener(DialogueRunnerControl_DialogueCompleted);
				e.control.onNodeStarted.AddListener(DialogueRunnerControl_NodeStarted);
				e.control.onNodeCompleted.AddListener(DialogueRunnerControl_NodeCompleted);
				e.control.onCommand.AddListener(DialogueRunnerControl_Command);

				Remove(e.control);
				DialogueRunnerControl_DialogueCompleted(e);
			}

			private void DialogueRunnerControl_DialogueCompleted(DialogueEventArgs e)
			{
				DialogueCompleted?.Invoke(this, e);

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

			private void DialogueRunnerControl_NodeStarted(DialogueEventArgs e)
			{
				if (!IsActiveDialogueRunnerControl(e))
					return;

				NodeStarted?.Invoke(this, e);
			}

			private void DialogueRunnerControl_NodeCompleted(DialogueEventArgs e)
			{
				if (!IsActiveDialogueRunnerControl(e))
					return;

				NodeCompleted?.Invoke(this, e);
			}

			private void DialogueRunnerControl_Command(DialogueEventArgs e)
			{
				if (!IsActiveDialogueRunnerControl(e))
					return;

				Command?.Invoke(this, e);
			}

			#endregion

			#region IDisposable Methods

			~RuntimeDialogueCategory()
			{
				Dispose(false);
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (m_disposed)
					return;

				m_activeRunnerControls = null;
				m_queue = null;
				m_disposed = true;
			}

			#endregion
		}

		#endregion
	}
}