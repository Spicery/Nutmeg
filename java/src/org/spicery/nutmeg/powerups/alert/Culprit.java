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
package org.spicery.nutmeg.powerups.alert;

import java.io.PrintWriter;

import org.spicery.nutmeg.powerups.common.IfNull;
import org.spicery.nutmeg.powerups.io.StringPrintWriter;

/**
 * This class has to be final so that other classes can specialise the culprit method.
 *
 */
public final class Culprit {
	
	private final String key;
	private final Object value;

	/**
	 * Programmers are not encouraged to build culprits, so the constructor is not public.
	 * @param key A heading that will be included in the exception report.
	 * @param value The value that will be reported on.
	 */
	Culprit( final String key, final Object value ) {
		this.key = IfNull.ifNull( key, "" );
		this.value = value;
	}
	
	private static String convertToString( final Object x ) {
		return "" + x;
	}

	private String keepShort( final Object x, final int mx_len, final int filemxlen ) {
		final int mxlen = mx_len > 5 ? mx_len : 5;
		final String s = convertToString( x );
		if ( s.length() > mxlen ) {
			if ( s.charAt( 0 ) == '/' && s.length() < filemxlen ) {
				return s;
			} else {
				return s.substring( 0, mxlen - 4 ) + " ...";
			}
		} else {
			return s;
		}
	}

	static final int maxlen = 256;
	static final int filemaxlen = 256;

	private String keepShort( Object x ) {
		return keepShort( x, maxlen, filemaxlen );
	}

	static final int min_pad_width = 8;
	
	void output( final PrintWriter pw ) {
		final String d = this.key.toUpperCase();
		pw.print( d );
		for ( int i = d.length(); i < min_pad_width; i++ ) {
			pw.print( " " );
		}
		pw.print( " : " );
		pw.println( this.keepShort( this.value ) );
	}
	
	String asString() {
		final StringPrintWriter pw = new StringPrintWriter();
		this.output( pw );
		return pw.toString();		
	}

	public String getKey() {
		return this.key;
	}

	public boolean hasKey( final String interned ) {
		return this.key.equals( interned ); 
	}

	public Object getValue() {
		return this.value;
	}

	public String getValueString() {
		return this.value != null ? this.value.toString() : "";
	}

}

