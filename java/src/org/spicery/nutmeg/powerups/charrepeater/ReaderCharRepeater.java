/**
 * Copyright Stephen Leach, 2014
 * This file is part of the MinXML for Java library.
 * 
 * MinXML for Java is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Foobar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with MinXML for Java.  If not, see <http://www.gnu.org/licenses/>.
 *  
 */
package org.spicery.nutmeg.powerups.charrepeater;

import java.io.EOFException;
import java.io.IOException;
import java.io.Reader;

/**
 * This class is used to convert a java.lang.Reader into
 * character repeater that supports arbitrary pushback.
 */
public class ReaderCharRepeater implements CharRepeaterInterface {

	private final Reader reader;
	private final CharBuffer buffer = new CharBuffer();
	
	/**
	 * Constructs the character repeater from a java.lang.Reader
	 * @param reader the input stream
	 */
	public ReaderCharRepeater( final Reader reader ) {
		this.reader = reader;
	}

	/**
	 * This is the key method used to take characters
	 * off the input stream. If the method returns true
	 * then it is guaranteed that there is at least one
	 * character in the buffer.
	 */
	@Override
	public boolean hasNextChar() {
		if ( ! this.buffer.isEmpty() ) return true;
		try {
			final int ich = this.reader.read();
			if ( ich >= 0 ) {
				this.buffer.pushChar( (char)ich );
			}
			return ich >= 0;
		} catch ( IOException e ) {
			throw new RuntimeException( e );
		}
	}
	
	/**
	 * Checks whether or not the next character is the same as
	 * wanted. If there's no next character it returns false.
	 * It does not consume any characters.
	 */
	@Override
	public boolean isNextChar( final char wanted ) {
		return( 
			( ! this.buffer.isEmpty() || this.hasNextChar() ) &&
			this.buffer.peekChar() == wanted
		);
	}
	
	/**
	 * Checks whether or not the sequence of characters in the string wanted
	 * is waiting to be read off the input. This is the same as calling
	 * isNextString( wanted, 0 ).
	 * @param wanted The sequence of characters that we are looking for. 
	 * @return True if the input matches wanted.
	 */
	@Override
	public boolean isNextString( final String wanted ) {
		return this.isNextStringHelper( wanted, 0 );	
	}
	
	private boolean isNextStringHelper( final String wanted, final int offset ) {
		if ( offset >= wanted.length() ) {
			return true;
		} else {
			if ( this.isNextChar( wanted.charAt( offset ) ) ) {
				final char ch = this.nextChar();
				final boolean result = this.isNextStringHelper( wanted, offset + 1 );
				this.pushChar( ch );
				return result;
			} else {
				return false;
			}			
		}
	}

	/**
	 * Returns the next character from the input, consuming it from the input. 
	 * If there are no characters waiting to be read an exception is thrown. 
	 */
	@Override
	public char nextChar() {
		if ( this.hasNextChar() ) {
			return this.buffer.popChar();
		} else {
			throw new RuntimeException( new EOFException() );
		}
	}

	/**
	 * Returns the next character from the input, consuming it from the input. 
	 * If there are no characters waiting to be read the default value_if_neeeded
	 * is returned.
	 * @param value_if_needed default returned if the input is exhausted.
	 * @return the next character from the input
	 */
	@Override
	public char nextChar( char value_if_needed ) {
		if ( this.hasNextChar() ) {
			return this.buffer.popChar();
		} else {
			return value_if_needed;
		}
	}

	/**
	 * Pushes a character onto the front of the input. It does not have to
	 * be the same as the previous character that was read from the input.
	 * There is no hard limit to how many characters are pushed.
	 */
	@Override
	public void pushChar( char value ) {
		this.buffer.pushChar( value );
	}

	/**
	 * Returns the next character from the input, but does not consume it. 
	 * If there are no characters waiting to be read an exception is thrown.
	 * @return the next character from the input
	 */
	@Override
	public char peekChar() {
		if ( ! this.buffer.isEmpty() || this.hasNextChar() ) {
			return this.buffer.peekChar();
		} else {
			throw new RuntimeException( new EOFException() );
		}
	}

	/**
	 * Returns the next character from the input, but does not consume it. 
	 * If there are no characters waiting to be read the default value_if_neeeded
	 * is returned.
	 * @param value_if_needed default returned if the input is exhausted.
	 * @return the next character from the input
	 */
	@Override
	public char peekChar( char value_if_needed ) {
		if ( ! this.buffer.isEmpty() || this.hasNextChar() ) {
			return this.buffer.peekChar();
		} else {
			return value_if_needed;
		}
	}

	/**
	 * Discards the next character from the input. If the input is
	 * exhausted then this method has no effect.
	 */
	@Override
	public void skipChar() {
		if ( ! this.buffer.isEmpty() || this.hasNextChar() ) {
			this.buffer.popChar();
		}		
	}
	
}
