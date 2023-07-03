package org.spicery.nutmeg.powerups.repeaters;

import org.spicery.nutmeg.powerups.common.Maybe;

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
        Deque<T> buffer = new ArrayDeque<T>();
        Repeater<T> repeater;

        public Implementation(Repeater<T> repeater) {
            this.repeater = repeater;
        }

        @Override
        public void pushBack( T t ) {
            buffer.addFirst( t );
        }

        @Override
        public Maybe<T> peek() {
            if ( buffer.isEmpty() ) {
                if (repeater.hasNext()) {
                    buffer.addLast( repeater.next() );
                    return Maybe.of(buffer.getFirst());
                } else {
                    return Maybe.empty();
                }
           } else {
                return Maybe.of(buffer.getFirst());
            }
        }

        @Override
        public Maybe<T> peek(int n) {
            return null;
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
