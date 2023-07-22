package org.spicery.nutmeg.powerups.repeaters;

import org.spicery.nutmeg.powerups.common.Maybe;
import org.spicery.nutmeg.powerups.common.DequeList;

import java.sql.Array;
import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Deque;
import java.util.List;

public interface PushableRepeater<T> extends Repeater<T> {

    void pushBack(T t);

    Maybe<T> peek();
    Maybe<T> peek(int n);

    T peekOrElse(T other);
    T peekOrElse(int n, T other);


    public static class Implementation<T> implements PushableRepeater<T> {
    	DequeList<T> buffer = new DequeList<T>();
        Repeater<T> repeater;

        public Implementation(Repeater<T> repeater) {
            this.repeater = repeater;
        }

        @Override
        public void pushBack( T t ) {
            buffer.addLast( t );
        }

        @Override
        public Maybe<T> peek() {
            if ( buffer.isEmpty() ) {
                if (repeater.hasNext()) {
                    buffer.addLast( repeater.next() );
                    return Maybe.of(buffer.removeLast());
                } else {
                    return Maybe.empty();
                }
           } else {
                return Maybe.of(buffer.first());
            }
        }

        @Override
        public Maybe<T> peek(int n) {
        	while (buffer.size() < n) {
        		if (repeater.hasNext()) {
        			buffer.addLast( repeater.next() );
        		} else {
        			return Maybe.empty();
        		}
        	}
            return Maybe.of(buffer.get(n));
        }

        @Override
        public T peekOrElse(T other) {
            return null;
        }

        @Override
        public T peekOrElse(int n, T other) {
            return null;
        }

        @Override
        public boolean hasNext() {
            return false;
        }

        @Override
        public T next() {
            return null;
        }

        @Override
        public PushableRepeater<T> newPushable() {
            return null;
        }
    }
}
