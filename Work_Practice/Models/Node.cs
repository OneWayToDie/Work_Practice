namespace Work_Practice.Models
{
	public class Node<T>
	{
		public T Data { get; set; }
		public Node<T> Next { get; set; }

		public Node(T data)
		{
			Data = data;
			Next = null;
		}
	}
}
