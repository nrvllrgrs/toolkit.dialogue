using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class DialogueEventArgs : System.EventArgs
	{
		#region Properties

		public DialogueRunnerControl control { get; private set; }
		public DialogueType type => control?.dialogueType;
		public DialogueRunner runner => control?.dialogueRunner;

		#endregion

		#region Constructors

		public DialogueEventArgs(DialogueRunnerControl control)
		{
			this.control = control;
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
		private bool m_playOnStart;

		[SerializeField]
		private string m_startNode = "Start";

		[SerializeField]
		private UnityEvent<DialogueEventArgs> m_onDialogueStart;

		[SerializeField]
		private UnityEvent<DialogueEventArgs> m_onDialogueComplete;

		private DialogueRunner m_dialogueRunner;
		private float m_startTime = Mathf.NegativeInfinity;

		#endregion

		#region Events

		public UnityEvent<DialogueEventArgs> onDialogueStart => m_onDialogueStart;
		public UnityEvent<DialogueEventArgs> onDialogueComplete => m_onDialogueComplete;

		#endregion

		#region Properties

		public DialogueRunner dialogueRunner => m_dialogueRunner;
		public DialogueType dialogueType => m_dialogueType;
		
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
		}

		private void OnDisable()
		{
			m_dialogueRunner.onDialogueStart.RemoveListener(DialogueRunner_DialogueStart);
			m_dialogueRunner.onDialogueComplete.RemoveListener(DialogueRunner_DialogueComplete);
		}

		private void Start()
		{
			if (m_playOnStart)
			{
				Play();
			}
		}

		[ContextMenu("Play")]
		public void Play()
		{
			Play(m_startNode);
		}

		public void Play(string startNode)
		{
			// Node doesn't exist, skip
			if (!m_dialogueRunner.NodeExists(startNode))
				return;

			DialogueManager.Instance.Play(this, startNode);
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

			DialogueManager.Instance.Enqueue(this, startNode);
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

			DialogueManager.Instance.Dequeue(this, startNode);
		}

		[ContextMenu("Clear Queue")]
		public void ClearQueue()
		{
			DialogueManager.Instance.ClearQueue(this);
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
			m_onDialogueStart?.Invoke(new DialogueEventArgs(this));
		}

		private void DialogueRunner_DialogueComplete()
		{
			m_onDialogueComplete?.Invoke(new DialogueEventArgs(this));
		}

		#endregion
	}
}