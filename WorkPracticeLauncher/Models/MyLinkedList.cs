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
			// Необходимо минимум 3 элемента
			if (Head?.Next?.Next == null)
				return false;

			var first = Head;
			var second = Head.Next;
			var third = Head.Next.Next;
			var fourth = third.Next; // может быть null

			// Переставляем ссылки
			first.Next = second;      // 1 -> 2
			second.Next = fourth;     // 2 -> 4 (или null)
			third.Next = first;       // 3 -> 1
			Head = third;             // голова теперь 3

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

		public void Clear()
		{
			Head = null;
		}
	}
}