using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class FiniteStack<T> : LinkedList<T> {

	private const int _size = 10;

	public T Peek(){
		return this.Last.Value;
	}

	public T Pop(){
		LinkedListNode<T> node = this.Last;

		if (node != null) {
			this.RemoveLast ();
			return node.Value;
		}
		else{
			return default(T);
		}
	}

	public void Push(T value){
		LinkedListNode<T> node = new LinkedListNode<T> (value);

		this.AddLast (node);

		if(this.Count > _size){
			this.RemoveFirst ();
		}
	}
}
