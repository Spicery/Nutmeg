package org.spicery.nutmeg.powerups.common;

import static org.junit.jupiter.api.Assertions.*;

import org.junit.jupiter.api.Test;

import java.util.Iterator;

class DequeListTest {

	@Test
	void empty_test_isEmpty() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		
		//	Act
		boolean b = d.isEmpty();
		int n = d.size();
		
		//	Assert
		assertTrue( b );
		assertEquals( 0, n );
	}
	
	@Test
	void nonEmpty_test_isEmpty() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		d.add( "foo" );
		
		//	Act
		boolean b = d.isEmpty();
		int n = d.size();
		
		//	Assert
		assertFalse( b );
		assertEquals( 1, n );
		assertEquals( "foo", d.get( 0 ) );
		assertEquals( "foo", d.first() );
		assertEquals( "foo", d.last() );
	}

	@Test
	void test_clear() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		d.add( "foo" );
		
		//	Act
		d.clear();
		boolean b = d.isEmpty();
		int n = d.size();
		
		//	Assert
		assertTrue( b );
		assertEquals( 0, n );
	}

	@Test
	void empty_test_contains() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		
		//	Act
		boolean b = d.contains("foo");
		int n = d.indexOf( "foo" );
		
		//	Assert
		assertFalse( b );
		assertEquals( -1, n );
	}

	@Test
	void nonEmpty_test_contains() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		d.add( "bar" );
		d.add( "foo" );
		d.add( "gort" );
		
		//	Act
		boolean b = d.contains("foo");
		int n = d.indexOf( "foo" );
		
		//	Assert
		assertTrue( b );
		assertEquals( 1, n );
	}

	@Test
	void test_get() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		d.add( "bar" );
		d.add( "foo" );
		d.add( "gort" );

		//	Act
		String s0 = d.get( 0 );
		String s2 = d.get( 2 );
		String s1 = d.get( 1 );

		//	Assert
		assertEquals( "bar", s0 );
		assertEquals( "foo", s1 );
		assertEquals( "gort", s2 );
	}

	@Test
	void test_set() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		d.add( "bar" );
		d.add( "foo" );
		d.add( "gort" );

		//	Act
		d.set( 1, "foople" );
		String s0 = d.get( 0 );
		String s2 = d.get( 2 );
		String s1 = d.get( 1 );

		//	Assert
		assertEquals( "bar", s0 );
		assertEquals( "foople", s1 );
		assertEquals( "gort", s2 );
	}

	@Test
	void test_addFirst_and_addLast() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		d.add( "bar" );
		d.add( "foo" );
		d.add( "gort" );

		//	Act
		for (int i = 0; i < 100; i++) {
			d.addFirst(i + "_first");
			d.addLast("last_" + i);
		}
		for (int i = 0; i < 100; i++) {
			d.removeFirst();
			d.removeLast();
		}
		String s0 = d.get( 0 );
		String s2 = d.get( 2 );
		String s1 = d.get( 1 );

		//	Assert
		assertEquals( 3, d.size() );
		assertEquals( "bar", s0 );
		assertEquals( "foo", s1 );
		assertEquals( "gort", s2 );
	}

	@Test
	void test_iterator() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		d.add( "bar" );
		d.add( "foo" );
		d.add( "gort" );

		//	Act
		String r = "";
		for (String i: d) {
			r += i;
		}

		//	Assert
		assertEquals( "barfoogort", r);
	}

	@Test
	void remove_test_iterator() {
		//	Arrange
		DequeList<String> d = new DequeList<String>();
		d.add( "bar" );
		d.add( "foo" );
		d.add( "gort" );

		//	Act
		Iterator<String> it = d.iterator();
		String a = it.next();
		String b = it.next();
		it.remove();
		String c = it.next();

		//	Assert
		assertEquals( 2, d.size());
		assertEquals( "bar", a );
		assertEquals( "foo", b);
		assertEquals( "gort", c );
		assertEquals( "bar", d.get(0) );
		assertEquals( "gort", d.get(1) );
	}

}
