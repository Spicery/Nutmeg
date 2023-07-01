package org.spicery.nutmeg.powerups.repeaters;

import org.spicery.nutmeg.powerups.common.Maybe;

/**
 *  A Repeater is a throwback to the old Enumeration class. It is typically *not* backed by an 
 *  underlying collection however. When the repeater is exhausted, an attempt to read past the 
 *  end of the stream will throw an error. However, skipping will not throw an exception at 
 *  stream end, nor will next with a default argument.
 */

public interface Repeater< T > {
	
	public boolean hasNext();
	
	public T next();
	
	default Maybe<T> fetch() {
		return hasNext() ? Maybe.of(next()) : Maybe.empty();
	}
	
	default void skip() { 
		next(null);
	}

	default T next( T value_if_at_end ) {
		return hasNext() ? next() : value_if_at_end;
	}
	
}
