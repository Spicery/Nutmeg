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

/**
 * This is an interface for a char based stream that
 * supports unlimited pushback. 
 */
public interface CharRepeater {
	/**
	 * Returns true if it there are characters available to be consumed
	 * from the input, otherwise false.
	 * @return true if there are more characters, else false
	 */
	boolean hasNextChar();
		
	/**
	 * Returns true if it the next character is the same as wanted,
	 * otherwise false and advances. If the input is exhausted then 
	 * this method returns false.
	 * @return the next character is equal to wanted
	 * @param wanted the character that we are looking for
	 */
	boolean isNextChar( char wanted );
	
	/**
	 * Returns true if it the next sequence of characters matches the string wanted,
	 * otherwise false and advances. If the input is exhausted then this method
	 * returns false.
	 * @param wanted the sequence of characters that we are looking for
	 * @return if true then the wanted string is waiting to be read
	 */
	boolean isNextString( String wanted );
	
	/**
	 * Returns the next character from the input, consuming it from the input. 
	 * If there are no characters waiting to be read an exception is thrown. 
	 * @return the next character.
	 */
    char nextChar();
    
	/**
	 * Returns the next character from the input, consuming it from the input. 
	 * If there are no characters waiting to be read the default value_if_neeeded
	 * is returned.
	 * @param value_if_needed default returned if the input is exhausted.
	 * @return the next character from the input
	 */
    char nextChar( char value_if_needed );

    /**
	 * Pushes a character onto the front of the input. It does not have to
	 * be the same as the previous character that was read from the input.
	 * There is no hard limit to how many characters are pushed.
	 * 
	 * @param value the character to be pushed back
	 */
    void pushChar( char value );
    
	/**
	 * Returns the next character from the input, but does not consume it. 
	 * If there are no characters waiting to be read an exception is thrown.
	 * @return the next character from the input
	 */
    char peekChar();
    
	/**
	 * Returns the next character from the input, but does not consume it. 
	 * If there are no characters waiting to be read the default value_if_neeeded
	 * is returned.
	 * @param value_if_needed default returned if the input is exhausted.
	 * @return the next character from the input
	 */
    char peekChar( char value_if_needed );
 

	/**
	 * Discards the next character from the input. If the input is
	 * exhausted then this method has no effect.
	 */
    void skipChar();
}
