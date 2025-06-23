using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Dialogue
{
	[RequireComponent(typeof(DialogueRunnerControl))]
    public class DialogueQueue : MonoBehaviour, IList<YarnNode>, IList
    {
		#region Fields

		[SerializeField]
		private List<YarnNode> m_nodes = new();

		private DialogueRunnerControl m_dialogueRunnerControl;

		#endregion

		#region Events

		[SerializeField, Foldout("Events")]
		private UnityEvent<DialogueQueue> m_onFirstEnqueued;

		[SerializeField, Foldout("Events")]
		private UnityEvent<DialogueQueue> m_onEnqueued;

		[SerializeField, Foldout("Events")]
		private UnityEvent<DialogueQueue> m_onDequeued;

		[SerializeField, Foldout("Events")]
		private UnityEvent<DialogueQueue> m_onLastDequeued;

		#endregion

		#region Properties

		object IList.this[int index]
		{
			get => ((IList)m_nodes)[index];
			set => ((IList)m_nodes)[index] = value;
		}

		public YarnNode this[int index]
		{
			get => ((IList<YarnNode>)m_nodes)[index];
			set => ((IList<YarnNode>)m_nodes)[index] = value;
		}

		public int Count => ((ICollection<YarnNode>)m_nodes).Count;

		public bool IsReadOnly => ((ICollection<YarnNode>)m_nodes).IsReadOnly;

		public bool IsFixedSize => ((IList)m_nodes).IsFixedSize;

		public bool IsSynchronized => ((ICollection)m_nodes).IsSynchronized;

		public object SyncRoot => ((ICollection)m_nodes).SyncRoot;

		public UnityEvent<DialogueQueue> OnFirstEnqueued => m_onFirstEnqueued;
		public UnityEvent<DialogueQueue> OnEnqueued => m_onEnqueued;
		public UnityEvent<DialogueQueue> OnDequeued => m_onDequeued;
		public UnityEvent<DialogueQueue> OnLastDequeued => m_onLastDequeued;

		#endregion

		#region Methods

		private void Awake()
		{
			m_dialogueRunnerControl = GetComponent<DialogueRunnerControl>();
		}

		[ContextMenu("Next")]
		public void Next()
		{
			if (m_nodes.Count > 0)
			{
				m_dialogueRunnerControl.Play(m_nodes[0]);
				RemoveAt(0);
			}
		}

		public int IndexOf(YarnNode item)
		{
			return ((IList<YarnNode>)m_nodes).IndexOf(item);
		}

		public void Insert(int index, YarnNode item)
		{
			bool first = Count == 0;
			((IList<YarnNode>)m_nodes).Insert(index, item);

			if (first)
			{
				m_onFirstEnqueued?.Invoke(this);
			}
			m_onEnqueued?.Invoke(this);
		}

		public void RemoveAt(int index)
		{
			((IList<YarnNode>)m_nodes).RemoveAt(index);

			m_onDequeued?.Invoke(this);
			if (Count == 0)
			{
				m_onLastDequeued?.Invoke(this);
			}
		}

		void ICollection<YarnNode>.Add(YarnNode item)
		{
			((ICollection<YarnNode>)m_nodes).Add(item);
		}

		public void Clear()
		{
			((ICollection<YarnNode>)m_nodes).Clear();

			m_onDequeued?.Invoke(this);
			m_onLastDequeued?.Invoke(this);
		}

		public bool Contains(YarnNode item)
		{
			return ((ICollection<YarnNode>)m_nodes).Contains(item);
		}

		public void CopyTo(YarnNode[] array, int arrayIndex)
		{
			((ICollection<YarnNode>)m_nodes).CopyTo(array, arrayIndex);
		}

		bool ICollection<YarnNode>.Remove(YarnNode item)
		{
			return ((ICollection<YarnNode>)m_nodes).Remove(item);
		}

		public IEnumerator<YarnNode> GetEnumerator()
		{
			return ((IEnumerable<YarnNode>)m_nodes).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((System.Collections.IEnumerable)m_nodes).GetEnumerator();
		}

		public int Add(object value)
		{
			bool first = Count == 0;
			int index = ((IList)m_nodes).Add(value);

			if (first)
			{
				m_onFirstEnqueued?.Invoke(this);
			}
			m_onEnqueued?.Invoke(this);

			return index;
		}

		public bool Contains(object value)
		{
			return ((IList)m_nodes).Contains(value);
		}

		public int IndexOf(object value)
		{
			return ((IList)m_nodes).IndexOf(value);
		}

		public void Insert(int index, object value)
		{
			bool first = Count == 0;
			((IList)m_nodes).Insert(index, value);

			if (first)
			{
				m_onFirstEnqueued?.Invoke(this);
			}
			m_onEnqueued?.Invoke(this);
		}

		public void Remove(object value)
		{
			((IList)m_nodes).Remove(value);

			m_onDequeued?.Invoke(this);
			if (Count == 0)
			{
				m_onLastDequeued?.Invoke(this);
			}
		}

		public void CopyTo(Array array, int index)
		{
			((ICollection)m_nodes).CopyTo(array, index);
		}

		#endregion
	}
}