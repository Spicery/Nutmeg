package org.spicery.nutmeg.powerups.common;

import java.util.AbstractCollection;
import java.util.Arrays;
import java.util.Collection;
import java.util.Iterator;

import org.spicery.nutmeg.powerups.alert.Alert;

public class DequeList<T> extends AbstractCollection<T> implements Collection<T>
{
	
    /// <summary>
    /// Underlying store for the deque. Indexes into the store are always modulo the
    /// length of the store, so that it is effectively wraparound. As a result the values
    /// are either stored in a single contiguous block or two blocks that are at the start
    /// and end of the array.
    /// </summary>
    private T[] _items;

    /// <summary>
    /// Index of the first element. If empty it can be any valid index into the store.
    /// </summary>
    private int _head;  

    /// <summary>
    /// One past the last element. If empty then it is the same as _head.
    /// </summary>
    private int _tail;

    /// <summary>
    /// The number of elements held in the store. If the elements form a single contiguous
    /// block this is the same as _tail - _head. If the elements are split into two 
    /// contiguous blocks then it is _items.Length + _tail - _head.
    /// </summary>
    private int _size;

    private static final int InitialCapacity = 4;

    @SuppressWarnings("unchecked")
	public DequeList()
    {
    	_items = (T[])new Object[InitialCapacity];
        _head = 0;
        _tail = 0;
        _size = 0;
    }

    public int size() { return _size; }

    public T get(int subscript) {
        checkSubscript(subscript);
        return _items[ normalise(subscript) ];
    }
    
    public void set( int subscript, T value ) {
        checkSubscript(subscript);
        _items[ normalise(subscript) ] = value;
    }

    public T first()
    {
        if (_size == 0) {
            throw new Alert("Taking first item from an empty deque");
        }
        return _items[ _head ];
    }

    public T last()
    {
        if (_size == 0)
        	throw new Alert("Taking last item from an empty deque");
        return _tail > 0 ? _items[ _tail - 1 ] : _items[ _items.length - 1 ];
    }

    public void addFirst(T item)
    {
        resizeIfNeeded();

        // Update head position.
        _head = decrIndex(_head);

        // Add item.
        _items[ _head ] = item;
        _size += 1;
    }

    public void addLast(T item)
    {
        resizeIfNeeded();

        // Add item.
        _items[ _tail ] = item;

        // Update tail position.
        _tail = incrIndex(_tail);
        _size += 1;
    }

    public void clear()
    {
        Arrays.fill(_items, null);
        _head = 0;
        _tail = 0;
        _size = 0;
    }

    @Override
    public boolean contains(Object item)
    {
    	return this.findIndexOf(item) != -1;
    }

    public T removeFirst()
    {
        //  GUARD
        if (_size == 0)
            throw new Alert("Trying to remove an element from an empty deque");
        
        T result = _items[ _head ];
        _items[ _head ] = null;

        // update head position
        _head = incrIndex(_head);
        _size -= 1;

        return result;
    }

    public T removeLast()
    {
        if (_size == 0)
        {
            throw new Alert("Trying to remove an element from an empty deque");
        }

        // update tail position
        _tail = decrIndex(_tail);

        T result = _items[ _tail ];
        _items[ _tail ] = null;
        _size -= 1;

        return result;
    }

    public Iterator<T> iterator()
    {
    	final DequeList<T> deque = this;

        return new Iterator<T>() {
        	
        	int _index = deque._head;
     
			@Override
			public boolean hasNext() {
				return _index != deque._tail;
			}

			@Override
			public T next() {
				T result = deque._items[_index];
				_index = deque.incrIndex( _index );
				return result;
			}
			
			@Override
			public void remove() {
				int count = _index - deque._head;
				if ( count < 0 ) {
					count += deque._items.length;
				}
				deque.removeAt( count );
			}
        	
        };
    }

    @Override
    public boolean add(T item)
    {
        this.addLast(item);
        return true;
    }

    @Override
    public boolean remove(Object item)
    {
        int index = _head;
        for (int i = 0; i < _size; i++)
        {
            if (_items[ index ].equals(item))
            {
                this.removeAt(index);
                return true;
            }
            index = incrIndex(index);
        }

        return false;
    }

    public int indexOf(T item)
    {
        return this.findIndexOf(item);
    }
    
    private int findIndexOf(Object item)
    {
        int index = _head;
        for (int i = 0; i < _size; i++)
        {
            if (_items[ index ].equals(item))
                return i;

            index = incrIndex(index);
        }
        return -1;
    }

    public void insert(int subscript, T item)
    {
        if (subscript == 0)
        {
            addFirst(item);
        }
        else if (subscript == _size)
        {
            addLast(item);
        }
        else
        {
            checkSubscript(subscript);
            resizeIfNeeded();

            int prev = normalise(_size);
            for (int i = _size - 1; i >= subscript; i--)
            {
                int current = normalise(i);
                _items[ prev ] = _items[ current ];
                prev = current;
            }
            _items[ normalise(subscript) ] = item;
            _tail = incrIndex(_tail);
            _size += 1;
        }
    }

    public void removeAt(int subscript)
    {
        checkSubscript(subscript);

        if (subscript == 0)
        {
            removeFirst();
        }
        else if (subscript == _size) 
        { 
            removeLast(); 
        }
        else
        {
            int prev = normalise(subscript);
            for (int j = subscript + 1; j < _size; j++)
            {
                int current = normalise(j);
                _items[ prev ] = _items[ current ];
                prev = current;
            }
            this.removeLast();
        }
    }

    private void checkSubscript(int subscript)
    {
        if (subscript < 0 || subscript >= _size)
        {
            throw new Alert("Index out of range").culprit( "Index", subscript ).culprit( "Size", _size );
        }
    }

    private int normalise(int subscript)
    {
        int index = _head + subscript;
        if (index < 0)
        {
            while (true)
            {
                index += _items.length;
                if (index >= 0) break;
            }
        }
        else if (index >= _items.length)
        {
            while (true)
            {
                index -= _items.length;
                if (index < _items.length) break;
            }
        }
        return index;
    }

    private int decrIndex(int index)
    {
        index -= 1;
        if (index < 0) {
            index = _items.length - 1;
        }
        return index;
    }

    private int incrIndex(int index)
    {
        index += 1;
        if (index >= _items.length) {
            index = 0;
        } 
        return index;
    }

    private void resizeIfNeeded()
    {
    	resizeIfNeeded(1);
    }

    private void resizeIfNeeded(int n)
    {
        if (_size + n > _items.length)
            resize(n);
    }

    private void resize(int n)
    {
        //  Improves the storage utilisation vs doubling.
        setCapacity(16 + (Math.max(_size + n, _items.length) * 3) / 2);
    }

    private void setCapacity(int capacity)
    {
        @SuppressWarnings("unchecked")
		T[] newItems = (T[])new Object[ capacity ];

        if (_size > 0)
        {
            if (_head < _tail)
            {
            	System.arraycopy( _items, _head, newItems, 0, _size );
            }
            else
            {
            	System.arraycopy( _items, _head, newItems, 0, _items.length - _head );
            	System.arraycopy( _items, 0, newItems, _items.length - _head, _tail );
            }
        }

        _items = newItems;
        _head = 0;
        _tail = _size == capacity ? 0 : _size;
    }

	@Override
	public boolean isEmpty() {
		return _size == 0;
	}

}


