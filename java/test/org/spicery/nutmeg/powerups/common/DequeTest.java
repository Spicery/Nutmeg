package org.spicery.nutmeg.powerups.common;

import static org.junit.jupiter.api.Assertions.*;

import org.junit.jupiter.api.Test;

class DequeTest {

	@Test
	void empty_test_isEmpty() {
		//	Arrange
		Deque<String> d = new Deque<String>();
		
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
		Deque<String> d = new Deque<String>();
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
		Deque<String> d = new Deque<String>();
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
		Deque<String> d = new Deque<String>();
		
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
		Deque<String> d = new Deque<String>();
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

}
