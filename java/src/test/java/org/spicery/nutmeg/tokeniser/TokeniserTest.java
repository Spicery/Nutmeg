package org.spicery.nutmeg.tokeniser;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;

import java.io.StringReader;
import java.util.List;

import org.junit.jupiter.api.Test;
import org.spicery.nutmeg.powerups.charrepeater.ReaderCharRepeater;

class TokeniserTest {
	
	class OriginalStringTestDouble implements TokenFactoryInterface<String> {

		@Override
		public String nameToken( String original, String value ) {
			return original;
		}

		@Override
		public String stringToken( String original, String value, char quote ) {
			return original;
		}

		@Override
		public String intToken( String original, int value ) {
			return original;
		}

		@Override
		public String endOfFile() {
			return null;
		}

		@Override
		public String charToken( String original, char value ) {
			return original;
		}

		@Override
		public String symbolToken( String original, String value ) {
			return original;
		}
		
	}	
	
	class FactoryTestDouble implements TokenFactoryInterface<String> {

		@Override
		public String nameToken( String original, String value ) {
			return value;
		}

		@Override
		public String stringToken( String original, String value, char quote ) {
			return value;
		}

		@Override
		public String intToken( String original, int value ) {
			return null;
		}

		@Override
		public String endOfFile() {
			return null;
		}

		@Override
		public String charToken( String original, char value ) {
			return Character.toString( value );
		}

		@Override
		public String symbolToken( String original, String value ) {
			return value;
		}
		
	}
	

	@Test
	void testReadToken() {
		//	Arrange
		String input = "Two 'rats' in a \"sack\".";
		Tokenizer<String> tokenizer = new Tokenizer<String>(new FactoryTestDouble(), new ReaderCharRepeater(new StringReader(input)));
		
		//	Act
		String t0 = tokenizer.readToken();
		String t1 = tokenizer.readToken();
		String t2 = tokenizer.readToken();
		String t3 = tokenizer.readToken();
		String t4 = tokenizer.readToken();
		String t5 = tokenizer.readToken();
		String t6 = tokenizer.readToken();
		
		//	Assert
		assertEquals("Two", t0);
		assertEquals("rats", t1);
		assertEquals("in", t2);
		assertEquals("a", t3);
		assertEquals("sack", t4);
		assertEquals(".", t5);
		assertNull(t6);
		
	}

	@Test
	void test_OriginalString_ReadToken() {
		//	Arrange
		String input = "Two 'rats' in a \"sack\".";
		Tokenizer<String> tokenizer = new Tokenizer<String>(new OriginalStringTestDouble(), new ReaderCharRepeater(new StringReader(input)));
		
		//	Act
		String t0 = tokenizer.readToken();
		String t1 = tokenizer.readToken();
		String t2 = tokenizer.readToken();
		String t3 = tokenizer.readToken();
		String t4 = tokenizer.readToken();
		String t5 = tokenizer.readToken();
		String t6 = tokenizer.readToken();
		
		//	Assert
		assertEquals("Two", t0);
		assertEquals("'rats'", t1);
		assertEquals("in", t2);
		assertEquals("a", t3);
		assertEquals("\"sack\"", t4);
		assertEquals(".", t5);
		assertNull(t6);
		
	}

	@Test
	void test_Comments() {
		//	Arrange
		String input = "def f(x):\n"
				+ "  ### This is a comment\n"
				+ "  ### And so is this!\n"
				+ "  99\n"
				+ "enddef";
		Tokenizer<String> tokenizer = new Tokenizer<String>(new OriginalStringTestDouble(), new ReaderCharRepeater(new StringReader(input)));

		//	Act
		List<String> list = tokenizer.toList();
		
		//	Assert
		assertEquals("def", list.get( 0 ));
		assertEquals("f", list.get( 1 ));
		assertEquals("(", list.get( 2 ));
		assertEquals("x", list.get( 3 ));
		assertEquals(")", list.get( 4 ));
		assertEquals(":", list.get( 5 ));
		assertEquals("99", list.get( 6 ));
		assertEquals("enddef", list.get( 7 ));
		assertEquals(8, list.size());
	}
}
