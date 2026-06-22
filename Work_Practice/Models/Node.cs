namespace Work_Practice.Models
{
	// Узел односвязного списка
	public class Node<T>
	{
		public T Data { get; set; }
		public Node<T> Next { get; set; }

		// Конструктор узла с данными
		public Node(T data)
		{
			Data = data;
			Next = null;
		}
	}
}
