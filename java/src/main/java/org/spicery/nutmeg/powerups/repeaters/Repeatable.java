package org.spicery.nutmeg.powerups.repeaters;

import java.util.Iterator;

public interface Repeatable< T > {
	Repeater< T > repeater();
	
	default Iterator< T > iterator() {
		final Repeater<T> r = this.repeater(); 
		return new Iterator<T>() {

			@Override
			public boolean hasNext() {
				return r.hasNext();
			}

			@Override
			public T next() {
				return r.next();
			}
			
		};
	}
}
