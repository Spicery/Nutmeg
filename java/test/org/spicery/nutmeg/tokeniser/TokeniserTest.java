package org.spicery.nutmeg.tokeniser;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;

import java.io.StringReader;

import org.junit.jupiter.api.Test;
import org.spicery.nutmeg.powerups.charrepeater.ReaderCharRepeater;

class TokeniserTest {
	
	class FactoryTestDouble implements TokenFactory<String> {

		@Override
		public String nameToken( String value ) {
			return value;
		}

		@Override
		public String stringToken( String value ) {
			return value;
		}

		@Override
		public String intToken( int value ) {
			return null;
		}

		@Override
		public String endOfFile() {
			return null;
		}
		
	}
	

	@Test
	void testReadToken() {
		//	Arrange
		String input = "Two 'rats' in a sack.";
		Tokeniser<String> tokenizer = new Tokeniser<String>(new FactoryTestDouble(), new ReaderCharRepeater(new StringReader(input)));
		
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
		assertEquals("sack.", t4);
		assertNull(t5);
		assertNull(t6);
		
	}

}
