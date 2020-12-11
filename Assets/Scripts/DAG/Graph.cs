using System.Collections;
using System.Collections.Generic;
public class Graph<T> : IEnumerable<T>
{
	private NodeList<T> nodeSet;
	public Graph() : this(null) { }
	public Graph(NodeList<T> nodeSet)
	{
		if (nodeSet == null)
			this.nodeSet = new NodeList<T>();
		else
			this.nodeSet = nodeSet;
	}
	public void AddNode(GraphNode<T> node)
	{
		nodeSet.Add(node);
	}
	public void AddNode(T value)
	{
		nodeSet.Add(new GraphNode<T>(value));
	}
	public void AddDirectedEdge(GraphNode<T> from, GraphNode<T> to)
	{
		from.Neighbors.Add(to);
	}
	public bool Contains(T value)
	{
		return nodeSet.FindByValue(value) != null;
	}
	public bool Remove(T value)
	{
		GraphNode<T> nodeToRemove = (GraphNode<T>)nodeSet.FindByValue(value);
		if (nodeToRemove == null)
			return false;

		nodeSet.Remove(nodeToRemove);

		foreach (GraphNode<T> gnode in nodeSet)
		{
			int index = gnode.Neighbors.IndexOf(nodeToRemove);
			if (index != -1)
				gnode.Neighbors.RemoveAt(index);
		}

		return true;
	}
	public IEnumerator<T> GetEnumerator()
	{
		return (IEnumerator<T>)nodeSet.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	public NodeList<T> NodeList
	{
		get { return nodeSet; }
	}
}