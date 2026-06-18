using System.Collections.Generic;

namespace WorkPracticeLauncher.Models
{
	public class MyLinkedList<T>
	{
		public Node<T> Head { get; private set; }

		public void Add(T data)
		{
			var newNode = new Node<T>(data);
			if (Head == null)
				Head = newNode;
			else
			{
				var current = Head;
				while (current.Next != null) current = current.Next;
				current.Next = newNode;
			}
		}

		public bool MoveThirdToFront()
		{
			if (Head?.Next?.Next == null) return false;
			var first = Head;
			var third = Head.Next.Next;
			first.Next = third.Next;
			third.Next = Head;
			Head = third;
			return true;
		}

		public List<T> ToList()
		{
			var result = new List<T>();
			var current = Head;
			while (current != null)
			{
				result.Add(current.Data);
				current = current.Next;
			}
			return result;
		}
	}
}