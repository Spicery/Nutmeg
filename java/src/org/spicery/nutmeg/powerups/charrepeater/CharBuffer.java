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
 * This class implements a stack of char of arbitrary depth. The
 * aim is for it to be compact and lightweight. 
 */
class CharBuffer {
	
	private int size = 0;						//	Always non-negative. Next free slot.
	private char[] data = new char[ 2 ];		//	Must be at least 1.
	
	private void expandData() {
		final int L = java.lang.Math.max( 8, this.data.length );	//	Ensure faster expansion at small sizes.
		final int new_size = L + (L >> 1);							//	Roughly 1.5 time.	
		char[] new_data = new char[ new_size ];
		System.arraycopy( this.data, 0, new_data, 0, this.data.length );
		this.data = new_data;
	}
	
	public void pushChar( final char ch ) {
		try {
			data[ this.size ] = ch;
			this.size += 1;
		} catch ( ArrayIndexOutOfBoundsException _e ) {
			//	data is too small - expand and try again.
			this.expandData();
			data[ this.size ] = ch;
			this.size += 1;
		}
	}
	
	public char popChar() {
		if ( this.size > 0 ) {
			return this.data[ --this.size ];
		} else {
			throw new IllegalStateException( "Trying to pop from empty character buffer " );
		}
	}
	
	public char peekChar() {
		if ( this.size > 0 ) {
			return this.data[ this.size - 1 ];
		} else {
			throw new IllegalStateException( "Trying to peek at empty character buffer " );
		}
	}
	
	public boolean isEmpty() {
		return this.size == 0;
	}
	
}
