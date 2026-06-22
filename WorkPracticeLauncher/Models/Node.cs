namespace WorkPracticeLauncher.Models
{
	// Узел односвязного списка
	public class Node<T>
	{
		public T Data { get; set; }
		public Node<T> Next { get; set; }
		// Конструктор узла
		public Node(T data) => Data = data;
	}
}
