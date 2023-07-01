package org.spicery.nutmeg.powerups.common;

import java.util.Objects;
import java.util.function.Function;
import java.util.function.Supplier;

import org.spicery.nutmeg.powerups.alert.Alert;

/**
 * This is a complete reimplementation of java.util.Optional that 
 * supports nullable reference types correctly.
 * @param <T> the optionally nullable reference type
 */
public abstract class Maybe<T> {
	
	public static class HasValue<T1> extends Maybe<T1> {
		
		T1 value;
		
		public HasValue( T1 value ) {
			super();
			this.value = value;
		}

		@Override
	    public <U> Maybe<U> map(Function<? super T1, ? extends U> mapper) {
	        if (isEmpty()) {
	            return new None<U>();
	        } else {
	        	U u = mapper.apply(value);
	            return new HasValue<U>(u);
	        }
	    }

		@Override
		public boolean isEmpty() {
			return false;
		}

		@Override
		public T1 get() {
			return value;
		}
		
		@Override
		public T1 orElse(T1 other) {
			return value;
		}

		@Override
		public < U > Maybe< U > flatMap( Function< ? super T1, Maybe< U > > mapper ) {
	        if (isEmpty())
	            return new None<U>();
	        else {
	            return Objects.requireNonNull(mapper.apply(value));
	        }
	    }
		
		@Override
	    public T1 orElseGet(Supplier<? extends T1> other) {
	        return value;
	    }
		
		@Override
	    public <X extends Throwable> T1 orElseThrow(Supplier<? extends X> exceptionSupplier) throws X {
	        return value;
	    }
		
	    @Override
	    public boolean equals(Object obj) {
	        if (this == obj) return true;

	        if (!(obj instanceof Maybe.HasValue)) {
	            return false;
	        }

			HasValue<?> other = (HasValue<?>) obj;
	        return Objects.equals(value, other.value);
	    }
	    
	    @Override
	    public String toString() {
	        return String.format("Maybe[%s]", value);
	    }
		
	}
	
	
	public static class None<T2> extends Maybe<T2>{
		
		static None<?> NONE = new None<Object>();
		
		@SuppressWarnings("unchecked")
		@Override
	    public <U> Maybe<U> map(Function<? super T2, ? extends U> mapper) {
	        return (Maybe< U >) NONE;
	    }

		@Override
		public boolean isEmpty() {
			return true;
		}

		@Override
		public T2 get() {
			throw new Alert("Cannot get the value of an empty Maybe");
		}
		
		@Override
		public T2 orElse(T2 other) {
			return other;
		}

		@SuppressWarnings("unchecked")
		@Override
		public < U > Maybe< U > flatMap(Function< ? super T2, Maybe< U > > mapper ) {
	        return (Maybe< U >) NONE;
		}
		
		@Override
	    public T2 orElseGet(Supplier<? extends T2> other) {
	        return other.get();
	    }
		
	    @Override
	    public <X extends Throwable> T2 orElseThrow(Supplier<? extends X> exceptionSupplier) throws X {
	    	throw exceptionSupplier.get();
	    }
	    
	    @Override
	    public boolean equals(Object obj) {
	        if (this == obj) return true;

	        return obj instanceof Maybe.None;
	    }
		
	    @Override
	    public String toString() {
	        return "Maybe[]";
	    }
	}
	
	public abstract boolean isEmpty();
	
	public abstract T get();
	
	public boolean hasValue() {
		return !isEmpty();
	}
	
    public abstract <U> Maybe<U> map(Function<? super T, ? extends U> mapper);

    public abstract <U> Maybe<U> flatMap(Function<? super T, Maybe<U>> mapper);
    
    public abstract T orElse(T other);
    
    public abstract T orElseGet(Supplier<? extends T> other);
    
    public abstract <X extends Throwable> T orElseThrow(Supplier<? extends X> exceptionSupplier) throws X;
    
    @Override
    public int hashCode() {
        return Objects.hashCode(this.orElse( null ));
    }

    public static <T> Maybe<T> of( T t ) {
    	return new HasValue<T>(t);
    }
    
    @SuppressWarnings("unchecked")
	public static <T> Maybe<T> empty() {
    	return (Maybe< T >) None.NONE;
    }
    
}
