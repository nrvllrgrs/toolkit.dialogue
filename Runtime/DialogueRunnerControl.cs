using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class DialogueEventArgs : System.EventArgs
	{
		#region Properties

		public DialogueRunnerControl control { get; private set; }
		public string nodeName { get; private set; }
		public string command { get; private set; }
		public DialogueType type => control?.dialogueType;
		public DialogueRunner runner => control?.dialogueRunner;

		#endregion

		#region Constructors

		public DialogueEventArgs(DialogueRunnerControl control)
			: this(control, null)
		{ }

		public DialogueEventArgs(DialogueRunnerControl control, string nodeName)
			: this(control, nodeName, null)
		{ }

		public DialogueEventArgs(DialogueRunnerControl control, string nodeName, string command)
		{
			this.control = control;
			this.nodeName = nodeName;
			this.command = command;
		}

		#endregion
	}

	[RequireComponent(typeof(DialogueRunner))]
    public class DialogueRunnerControl : MonoBehaviour
    {
		#region Fields

		[SerializeField]
		protected DialogueType m_dialogueType;

		[SerializeField]
		protected YarnNode m_startNode;

		[SerializeField]
		protected bool m_playOnStart;

		[SerializeField]
		protected bool m_replicateSettings = false;

		[SerializeField]
		protected bool m_appendDialogueViews = false;

		[SerializeField]
		protected bool m_keepVariableStorage = false;

		[SerializeField]
		protected UnityEvent<DialogueEventArgs> m_onDialogueStarted;

		[SerializeField]
		protected UnityEvent<DialogueEventArgs> m_onDialogueCompleted;

		[SerializeField]
		protected UnityEvent<DialogueEventArgs> m_onNodeStarted;

		[SerializeField]
		protected UnityEvent<DialogueEventArgs> m_onNodeCompleted;

		[SerializeField]
		protected UnityEvent<DialogueEventArgs> m_onCommand;

		private DialogueRunner m_dialogueRunner;
		private bool m_isDialogueRunning = false;
		private float m_startTime = Mathf.NegativeInfinity;
		protected bool m_isSkipping = false;

		#endregion

		#region Events

		public UnityEvent<DialogueEventArgs> onDialogueStarted => m_onDialogueStarted;
		public UnityEvent<DialogueEventArgs> onDialogueCompleted => m_onDialogueCompleted;
		public UnityEvent<DialogueEventArgs> onNodeStarted => m_onNodeStarted;
		public UnityEvent<DialogueEventArgs> onNodeCompleted => m_onNodeCompleted;
		public UnityEvent<DialogueEventArgs> onCommand => m_onCommand;

		/// <summary>
		/// Used by DialogueManager to cleanup active runners before other subscribers are notified
		/// </summary>
		internal event System.EventHandler<DialogueEventArgs> DialogueCompleting;

		/// <summary>
		/// Used by DialogueManager to cleanup pooled runners after other subscribers are notified
		/// </summary>
		internal event System.EventHandler<DialogueEventArgs> DialogueLateCompleted;

		#endregion

		#region Properties

		public DialogueRunner dialogueRunner => m_dialogueRunner;
		public DialogueType dialogueType { get => m_dialogueType; set => m_dialogueType = value; }
		public bool isDialogueRunning => m_isDialogueRunning;
		public bool startNodeExists => !string.IsNullOrWhiteSpace(m_startNode?.name) && m_dialogueRunner.NodeExists(m_startNode?.name);

		/// <summary>
		/// Time when DialogueRunner last started
		/// </summary>
		public float age => Time.time - m_startTime;

		#endregion

		#region Methods

		protected virtual void Awake()
		{
			m_dialogueRunner = GetComponent<DialogueRunner>();
		}

		private void OnEnable()
		{
			m_dialogueRunner.onDialogueStart.AddListener(DialogueRunner_DialogueStart);
			m_dialogueRunner.onDialogueComplete.AddListener(DialogueRunner_DialogueComplete);
			m_dialogueRunner.onNodeStart.AddListener(DialogueRunner_NodeStart);
			m_dialogueRunner.onNodeComplete.AddListener(DialogueRunner_NodeComplete);
			m_dialogueRunner.onCommand.AddListener(DialogueRunner_Command);
		}

		private void OnDisable()
		{
			m_dialogueRunner.onDialogueStart.RemoveListener(DialogueRunner_DialogueStart);
			m_dialogueRunner.onDialogueComplete.RemoveListener(DialogueRunner_DialogueComplete);
			m_dialogueRunner.onNodeStart.RemoveListener(DialogueRunner_NodeStart);
			m_dialogueRunner.onNodeComplete.RemoveListener(DialogueRunner_NodeComplete);
			m_dialogueRunner.onCommand.RemoveListener(DialogueRunner_Command);
		}

		public void Set(DialogueRunner runner, DialogueType dialogueType)
		{
			m_dialogueRunner = runner;
			m_dialogueType = dialogueType;
		}

		private void Start()
		{
			if (m_playOnStart)
			{
				Play();
			}
		}

		public void QuickPlay()
		{
			Play();
		}

		public void QuickPlay(string startNode)
		{
			Play(startNode);
		}

		[ContextMenu("Play")]
		public bool Play()
		{
			return Play(m_startNode.name);
		}

		public bool Play(string nodeName)
		{
			// Node doesn't exist, skip
			if (!m_dialogueRunner.NodeExists(nodeName))
				return false;

			return DialogueManager.CastInstance.Play(this, nodeName);
		}

		public bool Play(YarnNode node)
		{
			if (m_dialogueRunner.yarnProject != node.project)
			{
				m_dialogueRunner.SetProject(node.project);
			}
			return Play(node.name);
		}

		internal virtual void PlayInternal(string startNode)
		{
			if (m_replicateSettings)
			{
				DialogueManager.CastInstance.ReplicateSettings(this, m_appendDialogueViews, m_keepVariableStorage);
			}
			m_dialogueRunner.StartDialogue(startNode);
		}

		[ContextMenu("Enqueue")]
		public void Enqueue()
		{
			Enqueue(m_startNode.name);
		}

		public void Enqueue(string startNode)
		{
			// Node doesn't exist, skip
			if (!m_dialogueRunner.NodeExists(startNode))
				return;

			DialogueManager.CastInstance.Enqueue(this, startNode);
		}

		[ContextMenu("Dequeue")]
		public void Dequeue()
		{
			Dequeue(m_startNode.name);
		}

		public void Dequeue(string startNode)
		{
			// Node doesn't exist, skip
			if (!m_dialogueRunner.NodeExists(startNode))
				return;

			DialogueManager.CastInstance.Dequeue(this, startNode);
		}

		[ContextMenu("Clear Queue")]
		public void ClearQueue()
		{
			DialogueManager.CastInstance.ClearQueue(this);
		}

		[ContextMenu("Stop")]
		public void Stop()
		{
			Stop(false);
		}

		public virtual void Stop(bool skipping)
		{
			if (!m_isDialogueRunning)
				return;

			m_isSkipping = skipping;
			m_dialogueRunner.Stop();
		}

		public void SetStartNode(YarnProject project, string nodeName)
		{
			if (Application.isPlaying)
			{
				m_dialogueRunner.SetProject(project);
			}
#if UNITY_EDITOR
			else
			{
				GetComponent<DialogueRunner>().SetProject(project);
			}		
#endif
			m_startNode.name = nodeName;
		}

		#endregion

		#region Callbacks

		private void DialogueRunner_DialogueStart()
		{
			// Skipping is not officially completing...
			if (m_isSkipping)
			{
				// ...we've skipped, so don't invoke events
				m_isSkipping = false;
				return;
			}

			m_startTime = Time.time;
			m_isDialogueRunning = true;
			m_onDialogueStarted?.Invoke(new DialogueEventArgs(this));
		}

		private void DialogueRunner_DialogueComplete()
		{
			// Skipping is not officially completing, skip
			if (m_isSkipping)
				return;

			m_isDialogueRunning = false;

			var e = new DialogueEventArgs(this);
			DialogueCompleting?.Invoke(this, e);
			m_onDialogueCompleted?.Invoke(e);
			DialogueLateCompleted?.Invoke(this, e);
		}

		private void DialogueRunner_NodeStart(string nodeName)
		{
			m_onNodeStarted?.Invoke(new DialogueEventArgs(this, nodeName));
		}

		private void DialogueRunner_NodeComplete(string nodeName)
		{
			m_onNodeCompleted?.Invoke(new DialogueEventArgs(this, nodeName));
		}

		private void DialogueRunner_Command(string command)
		{
			m_onCommand?.Invoke(new DialogueEventArgs(this, null, command));
		}

		#endregion
	}
}