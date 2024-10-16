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
		private DialogueType m_dialogueType;

		[SerializeField]
		private string m_startNode = "Start";

		[SerializeField]
		private bool m_playOnStart;

		[SerializeField]
		private bool m_replicateSettings = true;

		[SerializeField]
		private UnityEvent<DialogueEventArgs> m_onDialogueStarted;

		[SerializeField]
		private UnityEvent<DialogueEventArgs> m_onDialogueCompleted;

		[SerializeField]
		private UnityEvent<DialogueEventArgs> m_onNodeStarted;

		[SerializeField]
		private UnityEvent<DialogueEventArgs> m_onNodeCompleted;

		[SerializeField]
		private UnityEvent<DialogueEventArgs> m_onCommand;

		private DialogueRunner m_dialogueRunner;
		private float m_startTime = Mathf.NegativeInfinity;

		#endregion

		#region Events

		public UnityEvent<DialogueEventArgs> onDialogueStarted => m_onDialogueStarted;
		public UnityEvent<DialogueEventArgs> onDialogueCompleted => m_onDialogueCompleted;
		public UnityEvent<DialogueEventArgs> onNodeStarted => m_onNodeStarted;
		public UnityEvent<DialogueEventArgs> onNodeCompleted => m_onNodeCompleted;
		public UnityEvent<DialogueEventArgs> onCommand => m_onCommand;

		#endregion

		#region Properties

		public DialogueRunner dialogueRunner => m_dialogueRunner;
		public DialogueType dialogueType => m_dialogueType;

		public bool isDialogueRunning => m_dialogueRunner.IsDialogueRunning;

		/// <summary>
		/// Time when DialogueRunner last started
		/// </summary>
		public float age => Time.time - m_startTime;

		#endregion

		#region Methods

		private void Awake()
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

		[ContextMenu("Play")]
		public bool Play()
		{
			return Play(m_startNode);
		}

		public bool Play(string startNode)
		{
			// Node doesn't exist, skip
			if (!m_dialogueRunner.NodeExists(startNode))
				return false;

			return DialogueManager.CastInstance.Play(this, startNode);
		}

		internal void PlayInternal(string startNode)
		{
			if (m_replicateSettings)
			{
				DialogueManager.CastInstance.ReplicateSettings(this);
			}
			m_dialogueRunner.StartDialogue(startNode);
		}

		[ContextMenu("Enqueue")]
		public void Enqueue()
		{
			Enqueue(m_startNode);
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
			Dequeue(m_startNode);
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
			m_dialogueRunner.Stop();
		}

		#endregion

		#region Callbacks

		private void DialogueRunner_DialogueStart()
		{
			m_startTime = Time.time;
			m_onDialogueStarted?.Invoke(new DialogueEventArgs(this));
		}

		private void DialogueRunner_DialogueComplete()
		{
			m_onDialogueCompleted?.Invoke(new DialogueEventArgs(this));
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