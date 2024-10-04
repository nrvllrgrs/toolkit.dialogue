using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class NudgeManager : Singleton<NudgeManager>
    {
		#region Fields

		[SerializeField]
		private NudgeManagerConfig m_config;

		/// <summary>
		/// Objects pausing nudge countdown timer
		/// </summary>
		private HashSet<object> m_blockers = new();

		private bool m_paused = false;
		private float m_remainingTime = float.PositiveInfinity;

		private Dictionary<NudgeType, NudgeData> m_map = new();
		private NudgeData m_activeData;

		private DialogueRunner m_runner;
		private DialogueRunnerControl m_control;

		#endregion

		#region Events

		public event EventHandler<bool> PauseChanged;

		#endregion

		#region Properties

		public NudgeManagerConfig config => m_config;

		public bool paused
		{
			get => m_paused || activeData == null;
			private set
			{
				// No change, skip
				if (m_paused == value)
					return;

				m_paused = value;
				PauseChanged?.Invoke(this, value);
			}
		}

		private NudgeData activeData
		{
			get => m_activeData;
			set
			{
				// No change, skip
				if (Equals(m_activeData, value))
					return;

				// Stop if another nudge is actively running
				// Needs to occur before changing runner project
				if (m_runner.IsDialogueRunning)
				{
					m_runner.Stop();
				}

				m_activeData = value;

				if (value != null)
				{
					m_runner.SetProject(value.project);
					m_remainingTime = value.nudgeType.delayTime;

					if (!string.IsNullOrWhiteSpace(m_activeData.nudgeType.indexVarName))
					{
						m_runner.VariableStorage.SetValue(m_activeData.nudgeType.indexVarName, 0);
					}
				}
				else
				{
					m_runner.SetProject(null);
					m_remainingTime = float.PositiveInfinity;
				}
			}
		}

		#endregion

		#region Methods

		protected override void Initialize()
		{
			base.Initialize();

			// Pause when any dialogue starts; unpause when dialogue complete
			DialogueManager.Instance.DialogueStart += DialogueManager_DialogueStart;
			DialogueManager.Instance.DialogueComplete += DialogueManager_DialogueComplete;
		}

		protected override void Terminate()
		{
			base.Terminate();

			if (DialogueManager.Exists)
			{
				DialogueManager.Instance.DialogueStart -= DialogueManager_DialogueStart;
				DialogueManager.Instance.DialogueComplete -= DialogueManager_DialogueComplete;
			}
		}

		public void Register(NudgeDialogueRunner runner)
		{
			if (m_runner != null)
				return;

			m_runner = runner.dialogueRunner;
			if (m_config.dialogueType != null)
			{
				m_control = runner.GetComponent<DialogueRunnerControl>();
				if (m_control != null)
				{
					m_control.Set(m_runner, m_config.dialogueType);
				}
			}
		}

		public void Unregister(NudgeDialogueRunner runner)
		{
			if (m_runner != runner.dialogueRunner)
				return;

			m_runner = null;
			m_control = null;
		}

		private void Update()
		{
			if (paused)
				return;

			m_remainingTime -= Time.deltaTime;
			if (m_remainingTime <= 0f)
			{
				Play();
			}
		}

		#endregion

		#region Nudge Methods

		public void Set(NudgeType nudgeType, YarnProject project, string startNode = "Start", bool playImmediately = false)
		{
			var data = new NudgeData()
			{
				nudgeType = nudgeType,
				project = project,
				startNode = startNode,
				priority = config.GetPriority(nudgeType)
			};

			if (!m_map.ContainsKey(nudgeType))
			{
				m_map.Add(nudgeType, data);
			}
			else
			{
				m_map[nudgeType] = data;
			}

			// Current data has higher priority, skip
			if (m_activeData != null && m_activeData.priority > data.priority)
				return;

			activeData = data;

			// Automatically clear other nudges, if defined by NudgeType
			if (nudgeType.autoClear)
			{
				m_map.Clear();
			}

			if (playImmediately && !paused)
			{
				Play();
			}
		}

		public void Clear(NudgeType nudgeType)
		{
			if (!m_map.ContainsKey(nudgeType))
				return;

			// Remove data from map
			m_map.Remove(nudgeType);

			if (m_map.Count > 0)
			{
				// Set data with highest priority
				activeData = m_map.Values.OrderByDescending(x => x.priority).First();
			}
			else
			{
				// No active data
				activeData = null;
			}
		}

		public void ClearAll()
		{
			m_map.Clear();
			activeData = null;
		}

		/// <summary>
		/// Force active nudge to play, ignoring timer
		/// </summary>
		public void Play()
		{
			if (m_control != null)
			{
				m_control.Play(m_activeData.startNode);
			}
			else
			{
				m_runner.StartDialogue(m_activeData.startNode);
			}
			m_remainingTime = m_activeData.nudgeType.delayTime;
		}

		public void ResetTimer()
		{
			m_remainingTime = m_activeData != null
				? m_activeData.nudgeType.delayTime
				: float.PositiveInfinity;
		}

		#endregion

		#region Control Methods

		public void Pause(object source)
		{
			if (m_blockers.Add(source))
			{
				paused = true;
			}
		}

		public void Unpause(object source)
		{
			if (m_blockers.Remove(source))
			{
				paused = m_blockers.Count > 0;
				if (!paused)
				{
					m_remainingTime = Mathf.Max(m_remainingTime, m_activeData.nudgeType.minDelayTime);
				}
			}
		}

		public void ForceUnpause()
		{
			m_blockers.Clear();
			paused = false;
		}

		#endregion

		#region Callbacks

		private void DialogueManager_DialogueStart(object sender, DialogueEventArgs e)
		{
			Pause(sender);
		}

		private void DialogueManager_DialogueComplete(object sender, DialogueEventArgs e)
		{
			Unpause(sender);
		}

		#endregion

		#region Structures

		private class NudgeData
		{
			public NudgeType nudgeType;
			public YarnProject project;
			public string startNode;
			public int priority;
		}

		#endregion
	}
}