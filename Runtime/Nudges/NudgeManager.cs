using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class NudgeManager : InstantiableSubsystem<NudgeManager, NudgeManagerConfig>
	{
		#region Fields

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

				if (runner == null)
				{
					Debug.LogError("DialogRunner in NudgeManagerConfig is undefined!");
					return;
				}

				// Stop if another nudge is actively running
				// Needs to occur before changing runner project
				if (runner.IsDialogueRunning)
				{
					runner.Stop();
				}

				m_activeData = value;

				if (value != null)
				{
					runner.SetProject(value.project);
					m_remainingTime = value.nudgeType.delayTime;

					if (!string.IsNullOrWhiteSpace(m_activeData.nudgeType.indexVarName))
					{
						runner.VariableStorage.SetValue(m_activeData.nudgeType.indexVarName, 0);
					}
				}
				else
				{
					runner.SetProject(null);
					m_remainingTime = float.PositiveInfinity;
				}
			}
		}

		protected DialogueRunner runner
		{
			get
			{
				if (m_runner == null)
				{
					m_runner = GetInstance()?.GetComponent<DialogueRunner>();
				}
				return m_runner;
			}
		}

		protected DialogueRunnerControl control
		{
			get
			{
				if (m_control == null)
				{
					m_control = GetInstance()?.GetComponent<DialogueRunnerControl>();
					if (m_control != null)
					{
						m_control.Set(runner, Config.dialogueType);
					}
				}
				return m_control;
			}
		}

		#endregion

		#region Methods

		protected override void Initialize()
		{
			base.Initialize();

			// Pause when any dialogue starts; unpause when dialogue complete
			DialogueManager.CastInstance.DialogueStarted += DialogueManager_DialogueStart;
			DialogueManager.CastInstance.DialogueCompleted += DialogueManager_DialogueComplete;
			LifecycleSubsystem.Register(this, LifecycleSubsystem.Phase.Update);
		}

		protected override void Terminate()
		{
			base.Terminate();

			LifecycleSubsystem.Unregister(this, LifecycleSubsystem.Phase.Update);
			DialogueManager.CastInstance.DialogueStarted -= DialogueManager_DialogueStart;
			DialogueManager.CastInstance.DialogueCompleted -= DialogueManager_DialogueComplete;
		}

		public override void Update()
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
				priority = Config.GetPriority(nudgeType)
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
			if (control != null)
			{
				control.Play(m_activeData.startNode);
			}
			else
			{
				runner.StartDialogue(m_activeData.startNode);
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