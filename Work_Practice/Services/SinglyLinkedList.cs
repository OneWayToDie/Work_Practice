using System;
using System.Collections.Generic;
using Work_Practice.Models;

namespace Work_Practice.Services
{
	public class SinglyLinkedList<T>
	{
		public Node<T> Head { get; private set; }

		public void Add(T data)
		{
			var newNode = new Node<T>(data);
			if (Head == null)
			{
				Head = newNode;
				return;
			}
			var current = Head;
			while (current.Next != null)
				current = current.Next;
			current.Next = newNode;
		}

		public bool MoveThirdToFront()
		{
			// Если меньше 3 элементов, операция невозможна
			if (Head?.Next?.Next == null)
				return false;

			var first = Head;
			var second = Head.Next;
			var third = Head.Next.Next;

			// Переподвязка: первый указывает на четвёртый (если есть)
			first.Next = third.Next;
			// Третий становится головой, его следующий – первый
			third.Next = first;
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

		public void Clear()
		{
			Head = null;
		}
	}
}