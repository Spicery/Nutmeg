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
package org.spicery.nutmeg.powerups.io;

import java.io.CharArrayWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.io.Writer;

/**
 * Implements a reusable print-writer that targets a
 * string. It is reusable in the sense that you can 
 * extract the internal string at any time as many times
 * as you like and you can reset it to empty as well. 
 *
 */
public class StringPrintWriter extends PrintWriter {
	
	public StringPrintWriter() {
		super( new CharArrayWriter(), false );
	}

	/**
	 * Resets the internal string buffer to empty.
	 */
	public void reset() {
		getCharArrayWriter().reset();
	}

	/**
	 * The number of characters printed to the string
	 * buffer so far.
	 * @return how big would be the string be
	 */
	public int size() {
		return getCharArrayWriter().size();
	}

	/**
	 * Return the string buffer as a character array.
	 * @return the characters printed so far
	 */
	public char[] toCharArray() {
		return getCharArrayWriter().toCharArray();
	}

	/**
	 * Returns the characters printed so far as a string
	 * @return the characters printed so far
	 */
	public String toString() {
		return getCharArrayWriter().toString();
	}

	/**
	 * Sends the characters printed so far to another Writer.
	 * @param out the destination writer
	 * @throws IOException If an I/O error occurs.
	 */
	public void writeTo( Writer out ) throws IOException {
		getCharArrayWriter().writeTo( out );
	}

	/**
	 * Returns the internal string buffer that is used to track
	 * the characters printed so far. This is shared with the 
	 * StringPrintWriter and not a copy. 
	 * @return the internal string buffer
	 */
	private CharArrayWriter getCharArrayWriter() {
		return (CharArrayWriter)this.out;
	}
	
}
