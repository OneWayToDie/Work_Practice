using System.Collections.Generic;

namespace WorkPracticeLauncher.Models
{
	public class MyLinkedList<T>
	{
		public Node<T> Head { get; private set; }

		public void Add(T data)
		{
			Node<T> newNode = new Node<T>(data);
			if (Head == null)
				Head = newNode;
			else
			{
				Node<T> current = Head;
				// Поиск последнего узла
				while (current.Next != null) current = current.Next;
				current.Next = newNode;
			}
		}

		public bool MoveThirdToFront()
		{
			if (Head?.Next?.Next == null)
				return false;

			Node<T> first = Head;
			Node<T> second = Head.Next;
			Node<T> third = Head.Next.Next;
			Node<T> fourth = third.Next;

			first.Next = second;
			second.Next = fourth;
			third.Next = first;
			Head = third;

			return true;
		}

		public List<T> ToList()
		{
			List<T> result = new List<T>();
			Node<T> current = Head;
			while (current != null)
			{
				result.Add(current.Data);
				current = current.Next;
			}
			return result;
		}

		public void Clear()
		{
			Head = null;
		}
	}
}
